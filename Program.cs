//11
using Microsoft.Data.Sqlite;
using System.Net;
using OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using Microsoft.AspNetCore.HttpOverrides;
using System.Diagnostics;

const string PROMPT_BASE = @"
Você é um especialista em aquicultura.

SOBRE A EMPRESA:
A Ecomarine Biotech é referência em biotecnologia aplicada à aquicultura, oferecendo soluções avançadas para maximizar produtividade, reduzir riscos operacionais e elevar a qualidade da água nos sistemas de cultivo. Com foco em inovação e sustentabilidade, a empresa desenvolve tecnologias que atuam diretamente na saúde dos organismos aquáticos, no equilíbrio do ambiente e na eficiência da produção. A Ecomarine Biotech se destaca pelo suporte técnico especializado e por entregar resultados consistentes para produtores que buscam alto desempenho e segurança na operação.

Site oficial: https://www.ecomarinebiotech.com

REGRAS:
- Responda apenas sobre aquicultura OU sobre a empresa Ecomarine Biotech quando solicitado
- Quando a pergunta for sobre a empresa, destaque seus diferenciais, inovação e impacto na produtividade
- Seja direto, técnico e prático.
- Não solicite dados ao usuário automaticamente.
- Sempre tente responder com base no que estiver disponível.

IMAGENS:
- Se houver imagem, analise primeiro.
- Descreva o que vê antes de qualquer conclusão.
- Use a imagem como principal fonte de informação.
- Só diga que precisa de mais dados se a imagem for inútil.

SENSORES:
- Temperatura, pH e oxigênio são opcionais.
- Nunca peça esses dados como requisito.
- Use apenas se já estiverem disponíveis.

COMPORTAMENTO:
- Não force diagnóstico sem evidência.
- Se houver limitação, diga claramente.
- Evite respostas genéricas.
";

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseWebRoot("wwwroot");

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// CONFIG OPENAI
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

if (string.IsNullOrEmpty(apiKey))
{
    throw new Exception("ERRO: Defina a variável de ambiente OPENAI_API_KEY");
}

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

    cmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS InteracoesWhatsApp (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            DataHora TEXT,
            ClienteId TEXT,
            Telefone TEXT,
            Tipo TEXT,
            MensagemTexto TEXT,
            UrlMidia TEXT,
            RespostaIA TEXT
        );
    ";
    cmd.ExecuteNonQuery();
}

app.UseForwardedHeaders();

async Task<string> AnalisarImagemIA(string url)
{
    using var http = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    http.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

    var requestBody = new
    {
        model = "gpt-4.1-mini",
        input = new object[]
        {
            new
            {
                role = "user",
                content = new object[]
                {
                    new
                    {
                        type = "input_text",
                        text = PROMPT_BASE + @"

                        TAREFA:
                        Analise a imagem enviada.

                        - Descreva o que você vê primeiro.
                        - Identifique se há relação com aquicultura.
                        - Se não houver, diga claramente.
                        - Se a imagem estiver ruim, informe limitação.
                        - Só depois faça análise técnica se houver evidência.
                        "
                    },                    new
                    {
                        type = "input_image",
                        image_url = url
                    }
                }
            }
        }
    };

    var response = await http.PostAsJsonAsync(
        "https://api.openai.com/v1/responses",
        requestBody
    );

    var raw = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
        return $"Erro IA: {raw}";

    using var doc = JsonDocument.Parse(raw);

    return ExtrairTexto(doc.RootElement);
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

        // ?? BUSCAR CONHECIMENTO (CONEXÃO PRÓPRIA)
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
            {PROMPT_BASE}

            BASE DE CONHECIMENTO:
            {contextoFormatado}

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
                input = prompt
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

// FUNÇÃO DE SALVAR AS INTERAÇÕES DO WHATSAPP
async Task SalvarInteracao(
    string clienteId,
    string telefone,
    string tipo,
    string mensagem,
    string? urlMidia,
    string resposta)
{
    using var conn = new SqliteConnection("Data Source=aqua.db;Cache=Shared");
    await conn.OpenAsync();

    var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        INSERT INTO InteracoesWhatsApp
        (ClienteId, Telefone, Tipo, MensagemTexto, UrlMidia, RespostaIA, DataHora)
        VALUES ($clienteId, $telefone, $tipo, $mensagem, $urlMidia, $resposta, $data)
    ";

    cmd.Parameters.AddWithValue("$clienteId", clienteId);
    cmd.Parameters.AddWithValue("$telefone", telefone);
    cmd.Parameters.AddWithValue("$tipo", tipo);
    cmd.Parameters.AddWithValue("$mensagem", mensagem ?? "");
    cmd.Parameters.AddWithValue("$urlMidia", (object?)urlMidia ?? DBNull.Value);
    cmd.Parameters.AddWithValue("$resposta", resposta ?? "");
    cmd.Parameters.AddWithValue("$data", DateTime.Now.ToString("s"));

    await cmd.ExecuteNonQueryAsync();
}

// FUNÇÃO DE TRANSCRIÇÃO
async Task<string> TranscreverAudio(byte[] audioBytes)
{
    using var http = new HttpClient();

    using var content = new MultipartFormDataContent();

    var fileContent = new ByteArrayContent(audioBytes);
    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/ogg");

    content.Add(fileContent, "file", "audio.ogg");
    content.Add(new StringContent("gpt-4o-mini-transcribe"), "model");

    http.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            Environment.GetEnvironmentVariable("OPENAI_API_KEY")
        );

    var response = await http.PostAsync(
        "https://api.openai.com/v1/audio/transcriptions",
        content
    );

    var json = await response.Content.ReadAsStringAsync();

    using var doc = JsonDocument.Parse(json);

    var texto = doc.RootElement.GetProperty("text").GetString();

    return texto ?? "";
}

string ObterExtensao(string contentType)
{
    if (string.IsNullOrWhiteSpace(contentType))
        return "";

    var partes = contentType.Split('/');

    if (partes.Length != 2)
        return "";

    var ext = partes[1].ToLower();

    // normalizações importantes
    return ext switch
    {
        "jpeg" => ".jpg",
        "pjpeg" => ".jpg",
        "png" => ".png",
        "webp" => ".webp",
        "gif" => ".gif",
        "heic" => ".heic",

        "ogg" => ".ogg",
        "mpeg" => ".mp3",
        "mp3" => ".mp3",
        "mp4" => ".mp4",
        "aac" => ".aac",

        _ => "." + ext // fallback inteligente
    };
}

async Task<string> DownloadMidia(string url, string extensao)
{
    var http = new HttpClient();

    var bytes = await http.GetByteArrayAsync(url);

    var nomeArquivo = $"video_{Guid.NewGuid()}.{extensao}";
    var caminho = Path.Combine("wwwroot/uploads", nomeArquivo);

    await File.WriteAllBytesAsync(caminho, bytes);

    return caminho;
}

string ExtrairAudio(string caminhoVideo)
{
    var caminhoAudio = caminhoVideo.Replace(".mp4", ".mp3");

    var processo = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-i \"{caminhoVideo}\" -q:a 0 -map a \"{caminhoAudio}\" -y",
            UseShellExecute = false,
            CreateNoWindow = true
        }
    };

    processo.Start();
    processo.WaitForExit();

    return File.Exists(caminhoAudio) ? caminhoAudio : null;
}

async Task<string> AnalisarVideoComIA(List<string> frames)
{
    var contents = new List<object>();

    contents.Add(new
    {
        type = "input_text",
        text = "Analise este vídeo de aquicultura. Verifique qualidade da água, comportamento dos peixes e possíveis problemas."
    });

    foreach (var frame in frames)
    {
        var bytes = await File.ReadAllBytesAsync(frame);
        var base64 = Convert.ToBase64String(bytes);

        contents.Add(new
        {
            type = "input_image",
            image_base64 = base64
        });
    }

    var request = new
    {
        model = "gpt-4.1-mini",
        input = new[]
        {
            new
            {
                role = "user",
                content = contents
            }
        }
    };

    var http = new HttpClient();
    http.DefaultRequestHeaders.Add("Authorization", $"Bearer SUA_API_KEY");

    var response = await http.PostAsJsonAsync("https://api.openai.com/v1/responses", request);
    var json = await response.Content.ReadFromJsonAsync<JsonElement>();

    return json
        .GetProperty("output")[0]
        .GetProperty("content")[0]
        .GetProperty("text")
        .GetString();
}


/////////////////
/// ENDPOINTS ///
/////////////////

// CHAT
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
    var numMedia = form["NumMedia"].ToString();
    var telefone = form["From"].ToString();
    var clienteId = telefone; // MVP

    string resposta = "";

    Console.WriteLine("===== NOVA REQUISIÇÃO WHATSAPP =====");
    Console.WriteLine("Telefone: " + telefone);
    Console.WriteLine("Body: " + body);
    Console.WriteLine("NumMedia: " + numMedia);

    if (!string.IsNullOrEmpty(numMedia) && numMedia != "0")
    {
        var mediaUrl = form["MediaUrl0"].ToString();
        var contentType = form["MediaContentType0"].ToString();

        Console.WriteLine("MEDIA DETECTADA");
        Console.WriteLine("MediaUrl0: " + mediaUrl);
        Console.WriteLine("ContentType: " + contentType);

        try
        {
            var accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
            var authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

            using var http = new HttpClient();

            var byteArray = System.Text.Encoding.ASCII.GetBytes($"{accountSid}:{authToken}");
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(byteArray)
                );

            Console.WriteLine("BAIXANDO MÍDIA...");

            var response = await http.GetAsync(mediaUrl);

            Console.WriteLine("STATUS DOWNLOAD: " + response.StatusCode);

            response.EnsureSuccessStatusCode();

            var mediaBytes = await response.Content.ReadAsByteArrayAsync();

            var uploadsPath = Path.Combine(app.Environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);

            var extension = ObterExtensao(contentType);

            if (string.IsNullOrEmpty(extension))
                extension = ".bin";

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            await File.WriteAllBytesAsync(filePath, mediaBytes);

            var baseUrl = Environment.GetEnvironmentVariable("BASE_URL")
              ?? $"{context.Request.Scheme}://{context.Request.Host}";

            var publicUrl = $"{baseUrl}/uploads/{fileName}";


            // ============================
            // IMAGEM
            // ============================
            if (contentType.StartsWith("image"))
            {
                Console.WriteLine("PROCESSANDO IMAGEM");

                resposta = await AnalisarImagemIA(publicUrl);

                await SalvarInteracao(
                    clienteId,
                    telefone,
                    contentType,
                    "imagem recebida",
                    publicUrl,
                    resposta
                );
            }
            // ============================
            // ÁUDIO
            // ============================
            else if (contentType.StartsWith("audio"))
            {
                Console.WriteLine("PROCESSANDO ÁUDIO");

                string texto;

                if (mediaBytes.Length == 0)
                {
                    resposta = "Áudio vazio ou inválido.";

                    await SalvarInteracao(
                        clienteId,
                        telefone,
                        "audio",
                        "",
                        publicUrl,
                        resposta
                    );

                    return Results.Content(
                        "<Response><Message>Áudio inválido.</Message></Response>",
                        "text/xml"
                    );
                }

                texto = await TranscreverAudio(mediaBytes);

                Console.WriteLine("TRANSCRIÇÃO: " + texto);

                resposta = await ProcessChat(new ChatRequest { Mensagem = texto });

                await SalvarInteracao(
                    clienteId,
                    telefone,
                    "audio",
                    texto,
                    publicUrl,
                    resposta
                );
            }
            // ============================
            // VÍDEO
            // ============================
            else if (contentType.StartsWith("video"))
            {
                Console.WriteLine("PROCESSANDO VÍDEO");

                try
                {
                    // ============================
                    // 🎞️ EXTRAIR FRAMES (AQUI!)
                    // ============================

                    var pastaFrames = Path.Combine(uploadsPath, $"frames_{Guid.NewGuid()}");

                    var frames = VideoHelper.ExtrairFrames(
                        filePath,        // caminho do vídeo que você salvou
                        pastaFrames,     // pasta onde vão os frames
                        3                // 1 frame a cada 3 segundos
                    )
                    .Take(5)            // 🔥 limita (importantíssimo)
                    .ToList();

                    Console.WriteLine($"FRAMES GERADOS: {frames.Count}");

                    if (frames.Count == 0)
                    {
                        resposta = "Não consegui extrair imagens do vídeo.";

                        await SalvarInteracao(
                            clienteId,
                            telefone,
                            "video",
                            "falha ao extrair frames",
                            publicUrl,
                            resposta
                        );

                        return Results.Content(
                            "<Response><Message>Erro ao processar vídeo.</Message></Response>",
                            "text/xml"
                        );
                    }

                    // ============================
                    // IA
                    // ============================
                    resposta = await AnalisarVideoComIA(frames);

                    await SalvarInteracao(
                        clienteId,
                        telefone,
                        "video",
                        "video recebido",
                        publicUrl,
                        resposta
                    );

                    // ============================
                    // LIMPEZA
                    // ============================
                    try
                    {
                        Directory.Delete(pastaFrames, true);
                        File.Delete(filePath);
                    }
                    catch { }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERRO PROCESSAR VIDEO: " + ex.ToString());

                    resposta = "Erro ao analisar o vídeo.";

                    await SalvarInteracao(
                        clienteId,
                        telefone,
                        "video",
                        "erro",
                        publicUrl,
                        resposta
                    );
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERRO MÍDIA: " + ex.ToString());

            resposta = "Não consegui processar a mídia. Tente novamente.";

            await SalvarInteracao(
                clienteId,
                telefone,
                "erro",
                body,
                null,
                resposta
            );
        }
    }
    else
    {
        Console.WriteLine("PROCESSANDO TEXTO");

        try
        {
            resposta = await ProcessChat(new ChatRequest { Mensagem = body });

            await SalvarInteracao(
                clienteId,
                telefone,
                "texto",
                body,
                null,
                resposta
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERRO TEXTO: " + ex.ToString());

            resposta = "Erro ao processar sua mensagem.";

            await SalvarInteracao(
                clienteId,
                telefone,
                "erro",
                body,
                null,
                resposta
            );
        }
    }

    if (string.IsNullOrWhiteSpace(resposta))
    {
        resposta = "Não consegui gerar resposta.";
    }

    Console.WriteLine("RESPOSTA FINAL: " + resposta);

    return Results.Content(
        $"<Response><Message>{System.Net.WebUtility.HtmlEncode(resposta)}</Message></Response>",
        "text/xml"
    );
});

// CONHECIMENTO
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

    var env = app.Environment;

    var uploadsPath = Path.Combine(
        env.WebRootPath,
        "uploads",
        clienteId
    );

    // ?? AQUI É O PONTO IMPORTANTE
    // cria a pasta automaticamente no servidor
    if (!Directory.Exists(uploadsPath))
        Directory.CreateDirectory(uploadsPath);

    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
    var filePath = Path.Combine(uploadsPath, fileName);

    using (var stream = File.Create(filePath))
    {
        await file.CopyToAsync(stream);
    }

    Console.WriteLine("SALVANDO EM: " + filePath);

    var baseUrl = Environment.GetEnvironmentVariable("BASE_URL")
              ?? $"{context.Request.Scheme}://{context.Request.Host}";

    var url = $"{baseUrl}/uploads/{clienteId}/{fileName}";

    // CHAMA IA AUTOMATICAMENTE
    var analise = await AnalisarImagemIA(url);

    return Results.Ok(new
    {
        url,
        analise
    });
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

public static class VideoHelper
{
    public static List<string> ExtrairFrames(
        string caminhoVideo,
        string pastaSaida,
        int intervaloSegundos = 3)
    {
        if (!Directory.Exists(pastaSaida))
            Directory.CreateDirectory(pastaSaida);

        string outputPattern = Path.Combine(pastaSaida, "frame_%03d.jpg");

        string argumentos = $"-i \"{caminhoVideo}\" -vf fps=1/{intervaloSegundos} \"{outputPattern}\" -hide_banner -loglevel error";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = argumentos,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();

        return Directory.GetFiles(pastaSaida, "frame_*.jpg")
                        .OrderBy(f => f)
                        .ToList();
    }
}