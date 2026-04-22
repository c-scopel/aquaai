using Microsoft.Data.Sqlite;
using System.Net;
using OpenAI;
using OpenAI.Chat;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// CONFIG OPENAI
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

Console.WriteLine("API KEY DEBUG: " + Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

if (string.IsNullOrEmpty(apiKey))
{
    throw new Exception("ERRO: Defina a variável de ambiente OPENAI_API_KEY");
}

var openAI = new OpenAIClient(apiKey);

// Criar tabelas
using (var conn = new SqliteConnection("Data Source=aqua.db"))
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

    using var conn = new SqliteConnection("Data Source=aqua.db");
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
    if (root.TryGetProperty("output", out var output))
    {
        foreach (var item in output.EnumerateArray())
        {
            if (item.TryGetProperty("content", out var content))
            {
                foreach (var c in content.EnumerateArray())
                {
                    if (c.TryGetProperty("text", out var text))
                    {
                        return text.GetString() ?? "";
                    }
                }
            }
        }
    }

    return "";
}

// Função IA + fallback
async Task<string> ProcessChat(ChatRequest req)
{
    var msg = req.Mensagem.Trim();

    using var conn = new SqliteConnection("Data Source=aqua.db");
    conn.Open();

    var conhecimento = BuscarConhecimento(conn, msg, 1);

    // Salvar mensagem usuário
    var cmdUser = conn.CreateCommand();
    cmdUser.CommandText = @"
        INSERT INTO Mensagens (Tanque, Remetente, Mensagem, Data)
        VALUES ('1', 'user', $msg, $data)
    ";
    cmdUser.Parameters.AddWithValue("$msg", msg);
    cmdUser.Parameters.AddWithValue("$data", DateTime.Now.ToString("s"));
    cmdUser.ExecuteNonQuery();

    // Buscar última leitura
    var cmdRead = conn.CreateCommand();
    cmdRead.CommandText = @"
        SELECT Temperatura, Ph, Oxigenio 
        FROM Leituras 
        WHERE Tanque = '1'
        ORDER BY Id DESC LIMIT 1
    ";

    using var reader = cmdRead.ExecuteReader();

    double? temp = null, ph = null, oxi = null;
    if (reader.Read())
    {
        temp = reader.IsDBNull(0) ? null : reader.GetDouble(0);
        ph = reader.IsDBNull(1) ? null : reader.GetDouble(1);
        oxi = reader.IsDBNull(2) ? null : reader.GetDouble(2);
    }


    string resposta = "";

    try
    {
        string contextoFormatado = string.IsNullOrWhiteSpace(conhecimento)
            ? "Nenhum contexto adicional encontrado."
            : string.Join("\n", conhecimento.Split('\n').Select(c => "- " + c));

        var prompt = $@"
        Você é um especialista em aquicultura.

        Faça 5 perguntas que você precisaria saber as respostas para sugerir melhorias no meu processo diariamente.

        Contexto técnico:
        {contextoFormatado}

        Dados do tanque:
        - Temperatura: {(temp.HasValue ? temp.Value.ToString() : "sem leitura")}
        - pH: {(ph.HasValue ? ph.Value.ToString() : "sem leitura")}
        - Oxigênio: {(oxi.HasValue ? oxi.Value.ToString() : "sem leitura")}

        Responda de forma clara, objetiva e útil.
        Se algum dado estiver sem leitura, não assuma risco real — peça validação.
        Se houver risco, alerte claramente.

        Após a sua resposta, sugira o que posso fazer para melhorar meu processo, baseado nas respostas das 5 perguntas iniciais.

        Pergunta:
        { msg}
        ";

        using var http = new HttpClient();

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
        Console.WriteLine("RESPOSTA OPENAI:");
        Console.WriteLine(raw);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine("OPENAI ERROR: " + error);
            throw new Exception("Erro na OpenAI: " + response.StatusCode);
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
    var cmdBot = conn.CreateCommand();
    cmdBot.CommandText = @"
        INSERT INTO Mensagens (Tanque, Remetente, Mensagem, Data)
        VALUES ('1', 'bot', $msg, $data)
    ";
    resposta = resposta.Trim();
    cmdBot.Parameters.AddWithValue("$msg", resposta);
    cmdBot.Parameters.AddWithValue("$data", DateTime.Now.ToString("s"));
    cmdBot.ExecuteNonQuery();

    return resposta.Trim();
    //return "IA OK - TESTE TWILIO";
}

// Chat endpoint
app.MapPost("/chat", async (HttpContext context) =>
{
    var req = await context.Request.ReadFromJsonAsync<ChatRequest>();
    if (req == null) return Results.Ok("Mensagem inválida.");
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

    using var conn = new SqliteConnection("Data Source=aqua.db");
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

// Histórico
app.MapGet("/mensagens", () =>
{
    var lista = new List<dynamic>();
    using var conn = new SqliteConnection("Data Source=aqua.db");
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