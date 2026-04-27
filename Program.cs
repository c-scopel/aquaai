using Microsoft.Data.Sqlite;
using System.Net;
using OpenAI;
using OpenAI.Chat;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// CONFIG OPENAI
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

if (string.IsNullOrEmpty(apiKey))
{
    throw new Exception("ERRO: Defina a variável de ambiente OPENAI_API_KEY");
}

var openAI = new OpenAIClient(apiKey);

// Criar tabelas
using (var conn = new SqliteConnection("Data Source=aqua.db;Cache=Shared"))
{
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Leituras (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Tanque TEXT,
            Temperatura REAL,
            Ph REAL,
            Oxigenio REAL,
            Data TEXT
        );
    ";
    cmd.ExecuteNonQuery();

    cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Mensagens (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Tanque TEXT,
            Remetente TEXT,
            Mensagem TEXT,
            Data TEXT
        );
    ";
    cmd.ExecuteNonQuery();

    cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Conhecimento (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ClienteId INTEGER NULL,
            Conteudo TEXT,
            Tags TEXT,
            CriadoPor TEXT,
            Data TEXT
        );
    ";
    cmd.ExecuteNonQuery();
}

// Inserir leitura
app.MapPost("/leitura", async (HttpContext context) =>
{
    var leitura = await context.Request.ReadFromJsonAsync<Leitura>();
    if (leitura == null) return Results.BadRequest("Leitura inválida.");

    using var conn = new SqliteConnection("Data Source=aqua.db;Cache=Shared");
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        INSERT INTO Leituras (Tanque, Temperatura, Ph, Oxigenio, Data)
        VALUES ($tanque, $temp, $ph, $oxi, $data)
    ";
    cmd.Parameters.AddWithValue("$tanque", leitura.Tanque);
    cmd.Parameters.AddWithValue("$temp", leitura.Temperatura);
    cmd.Parameters.AddWithValue("$ph", leitura.Ph);
    cmd.Parameters.AddWithValue("$oxi", leitura.Oxigenio);
    cmd.Parameters.AddWithValue("$data", DateTime.Now.ToString("s"));

    cmd.ExecuteNonQuery();
    return Results.Ok("Leitura salva!");
});

string BuscarConhecimento(SqliteConnection conn, string pergunta, int clienteId)
{
    var palavras = pergunta.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

    if (palavras.Length == 0)
        return "";

    var lista = new List<string>();

    var cmd = conn.CreateCommand();

    // Busca simples por LIKE (MVP)
    var filtros = string.Join(" OR ", palavras.Select((p, i) => $"(Tags LIKE $p{i} OR Conteudo LIKE $p{i})"));

    cmd.CommandText = $@"
        SELECT Conteudo 
        FROM Conhecimento
        WHERE ({filtros})
        AND (ClienteId IS NULL OR ClienteId = $clienteId)
        LIMIT 5
    ";

    for (int i = 0; i < palavras.Length; i++)
    {
        cmd.Parameters.AddWithValue($"$p{i}", $"%{palavras[i]}%");
    }

    cmd.Parameters.AddWithValue("$clienteId", clienteId);

    using var reader = cmd.ExecuteReader();

    while (reader.Read())
    {
        lista.Add(reader.GetString(0));
    }

    return string.Join("\n", lista);
}

string ExtrairTexto(JsonElement root)
{
    if (root.TryGetProperty("output_text", out var text))
        return text.GetString() ?? "";

    if (root.TryGetProperty("output", out var output))
    {
        foreach (var item in output.EnumerateArray())
        {
            if (item.TryGetProperty("content", out var content))
            {
                foreach (var c in content.EnumerateArray())
                {
                    if (c.TryGetProperty("text", out var t))
                        return t.GetString() ?? "";
                }
            }
        }
    }

    return "";
}

// Função de validação da pergunta por palavra-chave
bool PerguntaValida(string pergunta)
{
    if (string.IsNullOrWhiteSpace(pergunta))
        return false;

    var texto = pergunta.ToLower();

    var palavrasChave = new[]
    {
        "tanque", "peixe", "água", "oxigênio", "ph",
        "amônia", "ração", "aeração", "mortalidade",
        "biomassa", "densidade", "qualidade"
    };

    return palavrasChave.Any(p => texto.Contains(p));
}

// Função IA + fallback
async Task<string> ProcessChat(ChatRequest req)
{
    try
    {

        Console.WriteLine("PROCESSCHAT INICIO: " + DateTime.Now);

        var msg = req.Mensagem.Trim();

        // 🔎 BUSCAR CONHECIMENTO (CONEXÃO PRÓPRIA)
        string conhecimento;

        using (var conn = new SqliteConnection("Data Source=aqua.db"))
        {
            conn.Open();
            conhecimento = BuscarConhecimento(conn, msg, 1);
        }


        // Salvar mensagem usuário
        await AppState.Lock.WaitAsync();
        try
        {
            using var conn = new SqliteConnection("Data Source=aqua.db");
            conn.Open();

            var cmd = conn.CreateCommand();

            cmd.CommandText = @"
            INSERT INTO Mensagens (Tanque, Remetente, Mensagem, Data)
            VALUES ('1', 'user', $msg, $data)
        ";
            cmd.Parameters.AddWithValue("$msg", msg);
            cmd.Parameters.AddWithValue("$data", DateTime.Now.ToString("s"));
            cmd.ExecuteNonQuery();
        }
        finally
        {
            AppState.Lock.Release();
        }

        // Buscar última leitura
        double? temp = null, ph = null, oxi = null;

        //BUSCAR LEITURA (CONEXÃO ISOLADA)
        using (var conn = new SqliteConnection("Data Source=aqua.db"))
        {
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT Temperatura, Ph, Oxigenio 
        FROM Leituras 
        WHERE Tanque = '1'
        ORDER BY Id DESC 
        LIMIT 1
    ";

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                temp = reader.IsDBNull(0) ? null : reader.GetDouble(0);
                ph = reader.IsDBNull(1) ? null : reader.GetDouble(1);
                oxi = reader.IsDBNull(2) ? null : reader.GetDouble(2);
            }
        }

        string resposta = "";

        try
        {
            bool temConhecimento = !string.IsNullOrWhiteSpace(conhecimento);

            string contextoFormatado = temConhecimento
                ? string.Join("\n", conhecimento.Split('\n').Select(c => "- " + c))
                : "Nenhum contexto adicional encontrado.";

            var prompt = $@"
            Você é um especialista em aquicultura focado em operação de tanques.

            REGRAS IMPORTANTES:
            - Responda SOMENTE sobre aquicultura.
            - Se a pergunta não for relacionada, diga educadamente que só pode ajudar com aquicultura.
            - Nunca invente dados técnicos ou valores.
            - Se não tiver informação suficiente, peça mais dados antes de concluir.
            - Prefira recomendações seguras e conservadoras.
            - Evite qualquer suposição não baseada nos dados fornecidos.
            - Não responda perguntas fora do contexto técnico do sistema.
            - Se existir BASE DE CONHECIMENTO, priorize sua resposta primeiro utilizando ela.

            BASE DE CONHECIMENTO (PRIORIDADE MÁXIMA):
            {contextoFormatado}

            REGRAS CRÍTICAS:
            - Use PRIORITARIAMENTE a base de conhecimento acima.
            - Se a resposta estiver na base, NÃO use conhecimento externo.
            - NÃO contradiga a base de conhecimento.
            - Só utilize conhecimento geral se a base não contiver a resposta.
            - Se houver dúvida ou conflito, peça mais dados ao usuário.
            - Evite respostas genéricas.
            - Prefira repetir ou adaptar o conteúdo da base de conhecimento.

            DADOS DO TANQUE:
            - Temperatura: {(temp.HasValue ? temp.Value.ToString() : "sem leitura")}
            - pH: {(ph.HasValue ? ph.Value.ToString() : "sem leitura")}
            - Oxigênio dissolvido: {(oxi.HasValue ? oxi.Value.ToString() : "sem leitura")}

            INSTRUÇÕES DE RESPOSTA:
            - Seja claro, direto e útil
            - Máximo 10 linhas
            - Use linguagem prática (como orientação de campo)
            - Nunca forneça recomendações que possam causar risco sem alertar claramente.
            - Se houver risco, destaque o risco primeiro

            PERGUNTA:
            {msg}
            ";

            using var http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(20)
            };

            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                model = "gpt-4.1-mini",
                input = $"Você é um especialista em aquicultura.\n\n{prompt}"
            };

            var response = await http.PostAsJsonAsync(
                "https://api.openai.com/v1/responses",
                requestBody
            );

            // DEBUG REAL
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("OPENAI ERROR:");
                Console.WriteLine(raw);

                throw new Exception($"Erro OpenAI: {response.StatusCode} - {raw}");
            }

            // Parse seguro
            using var doc = System.Text.Json.JsonDocument.Parse(raw);

            var root = doc.RootElement;

            resposta = ExtrairTexto(root);

            if (string.IsNullOrWhiteSpace(resposta))
            {
                Console.WriteLine("DEBUG: resposta vazia da OpenAI");
                resposta = "Não consegui interpretar a resposta da IA. Tente novamente.";
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine("PROCESSCHAT ERROR:");
            Console.WriteLine(ex.ToString());

            // fallback
            var msgLower = msg.ToLower();

            if (msgLower.Contains("oxigenio"))
                resposta += oxi < 5 ? "Oxigênio baixo!\n" : "Oxigênio ok.\n";
            if (msgLower.Contains("ph"))
                resposta += (ph < 6.5 || ph > 8.5) ? "pH fora do ideal!\n" : "pH ok.\n";
            if (msgLower.Contains("temperatura"))
                resposta += temp > 30 ? "Temperatura alta!\n" : "Temperatura ok.\n";

            if (string.IsNullOrWhiteSpace(resposta))
                resposta = $"Resumo: Temp {temp}°C, pH {ph}, Oxigênio {oxi}";

            resposta += $"\n\n(IA indisponível: {ex.Message})";
        }

        // Salvar resposta
        await AppState.Lock.WaitAsync();
        try
        {
            using var conn = new SqliteConnection("Data Source=aqua.db");
            conn.Open();

            var cmdBot = conn.CreateCommand();
            cmdBot.CommandText = @"
        INSERT INTO Mensagens (Tanque, Remetente, Mensagem, Data)
        VALUES ('1', 'bot', $msg, $data)
    ";

            resposta = resposta.Trim();

            cmdBot.Parameters.AddWithValue("$msg", resposta);
            cmdBot.Parameters.AddWithValue("$data", DateTime.Now.ToString("s"));

            cmdBot.ExecuteNonQuery();
        }
        finally
        {
            AppState.Lock.Release();
        }

        Console.WriteLine("PROCESSCHAT FIM: " + DateTime.Now);

        return resposta.Trim();

    }
    finally
    {
    }

}


app.MapPost("/chat", async (HttpContext context) =>
{
    var req = await context.Request.ReadFromJsonAsync<ChatRequest>();
    if (req == null) return Results.Ok("Mensagem inválida.");

    var msg = req.Mensagem;

    // valida só contexto
    if (!PerguntaValida(req.Mensagem))
    {
        return Results.Ok(new
        {
            resposta = "Posso ajudar apenas com aquicultura."
        });
    }

    //Só chega aqui se for válido
    return Results.Ok(await ProcessChat(req));
});


// WhatsApp (Twilio)
app.MapPost("/whatsapp", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var body = form["Body"].ToString();

    var resposta = await ProcessChat(new ChatRequest { Mensagem = body });

    return Results.Content(
        $"<Response><Message>{WebUtility.HtmlEncode(resposta)}</Message></Response>",
        "text/xml"
    );
});

app.MapPost("/conhecimento", async (HttpContext context) =>
{
    var item = await context.Request.ReadFromJsonAsync<ConhecimentoRequest>();
    if (item == null) return Results.BadRequest("Dados inválidos.");

    using var conn = new SqliteConnection("Data Source=aqua.db;Cache=Shared");
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        INSERT INTO Conhecimento (ClienteId, Conteudo, Tags, CriadoPor, Data)
        VALUES ($clienteId, $conteudo, $tags, $criadoPor, $data)
    ";

    cmd.Parameters.AddWithValue("$clienteId", (object?)item.ClienteId ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$conteudo", item.Conteudo);
    cmd.Parameters.AddWithValue("$tags", item.Tags);
    cmd.Parameters.AddWithValue("$criadoPor", item.CriadoPor ?? "manual");
    cmd.Parameters.AddWithValue("$data", DateTime.Now.ToString("s"));

    cmd.ExecuteNonQuery();

    return Results.Ok("Conhecimento salvo!");
});

// Uploads
app.MapPost("/upload", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();

    var file = form.Files.FirstOrDefault();
    var clienteId = form["clienteId"].ToString();

    if (string.IsNullOrWhiteSpace(clienteId))
        return Results.BadRequest("clienteId é obrigatório.");

    if (file == null || file.Length == 0)
        return Results.BadRequest("Nenhuma imagem enviada.");

    if (!file.ContentType.StartsWith("image/"))
        return Results.BadRequest("Arquivo deve ser imagem.");

    clienteId = clienteId.Trim().ToLower().Replace(" ", "-");

    var uploadsPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "wwwroot",
        "uploads",
        clienteId
    );

    // 🔥 AQUI É O PONTO IMPORTANTE
    // cria a pasta automaticamente no servidor
    if (!Directory.Exists(uploadsPath))
        Directory.CreateDirectory(uploadsPath);

    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
    var filePath = Path.Combine(uploadsPath, fileName);

    using (var stream = File.Create(filePath))
    {
        await file.CopyToAsync(stream);
    }

    var url = $"{context.Request.Scheme}://{context.Request.Host}/uploads/{clienteId}/{fileName}";

    return Results.Ok(new { url });
});

// Histórico
app.MapGet("/mensagens", () =>
{
    var lista = new List<dynamic>();
    using var conn = new SqliteConnection("Data Source=aqua.db;Cache=Shared");
    conn.Open();

    var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT Id, Tanque, Remetente, Mensagem, Data FROM Mensagens";

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        lista.Add(new
        {
            Id = reader.GetInt32(0),
            Tanque = reader.GetString(1),
            Remetente = reader.GetString(2),
            Mensagem = reader.GetString(3),
            Data = reader.GetString(4)
        });
    }

    return lista;
});

// Health
app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "ok",
        time = DateTime.UtcNow
    });
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();

// Models
record Leitura
{
    public string Tanque { get; set; } = "";
    public double Temperatura { get; set; }
    public double Ph { get; set; }
    public double Oxigenio { get; set; }
}

record ChatRequest
{
    public string Mensagem { get; set; } = "";
}

record ConhecimentoRequest
{
    public int? ClienteId { get; set; }
    public string Conteudo { get; set; } = "";
    public string Tags { get; set; } = "";
    public string? CriadoPor { get; set; }
}

class AppState
{
    public static SemaphoreSlim Lock = new SemaphoreSlim(1, 1);
}

