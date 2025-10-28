using System.Net;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text;
using TestePIM;

Console.WriteLine("Iniciando servidor HTTP simples em http://localhost:5000/");

var sessions = new ConcurrentDictionary<string, SessionState>();
var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:5000/");
listener.Start();

_ = Task.Run(() =>
{
    while (listener.IsListening)
    {
        try
        {
            var ctx = listener.GetContext();
            _ = Task.Run(() => HandleRequest(ctx));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Listener error: " + ex.Message);
        }
    }
});

Console.WriteLine("Pressione Enter para encerrar...");
Console.ReadLine();
listener.Stop();

async void HandleRequest(HttpListenerContext ctx)
{
    var req = ctx.Request;
    var resp = ctx.Response;

    try
    {
        var path = req.Url?.AbsolutePath ?? "/";

        if (path.StartsWith("/api/login") && req.HttpMethod == "POST")
        {
            using var sr = new StreamReader(req.InputStream, req.ContentEncoding);
            var body = await sr.ReadToEndAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var login = JsonSerializer.Deserialize<LoginRequest>(body, options);

            if (login == null || string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
            {
                await WriteJson(resp, 400, new { error = "Credenciais inválidas" });
                return;
            }

            var sessionId = Guid.NewGuid().ToString();
            var chamado = new Chamado();
            var state = new SessionState { Conversa = chamado.novaConversa, LastContext = "root" };
            sessions[sessionId] = state;

            await WriteJson(resp, 200, new { sessionId, welcome = chamado.MensagemBoasVindas });
            return;
        }
        else if (path.StartsWith("/api/message") && req.HttpMethod == "POST")
        {
            using var sr = new StreamReader(req.InputStream, req.ContentEncoding);
            var body = await sr.ReadToEndAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var messageReq = JsonSerializer.Deserialize<MessageRequest>(body, options);

            if (messageReq == null || string.IsNullOrWhiteSpace(messageReq.SessionId) || string.IsNullOrWhiteSpace(messageReq.Content))
            {
                await WriteJson(resp, 400, new { error = "sessionId e content são obrigatórios" });
                return;
            }

            if (!sessions.TryGetValue(messageReq.SessionId, out var state))
            {
                await WriteJson(resp, 400, new { error = "Sessão não encontrada. Faça login novamente." });
                return;
            }

            var content = messageReq.Content.Trim();

            var userMsg = EnvioMsg.EnviarUsuario(messageReq.Content);
            state.Conversa.AdicionarMensagem(userMsg);

            Mensagem resposta;

            var lower = content.ToLower();

            if (lower == "1" || lower == "ajuda")
            {
                resposta = Chamado.GerarMensagemAjuda();
                state.LastContext = "ajuda";
                state.Conversa.AdicionarMensagem(resposta);
            }
            else if (lower == "2" || lower == "faq")
            {
                resposta = Chamado.GerarMensagemFAQ();
                state.LastContext = "faq";
                state.Conversa.AdicionarMensagem(resposta);
            }
            else if (state.LastContext == "faq" && int.TryParse(lower, out _))
            {
                resposta = state.Conversa.GerarRespostaFaq(lower);
                state.LastContext = "root";
                state.Conversa.AdicionarMensagem(resposta);
            }
            else if (state.LastContext == "ajuda")
            {
                resposta = state.Conversa.GerarRespostaAjuda(messageReq.Content);
                state.LastContext = "root";
                state.Conversa.AddicionarMensagemIfNotPresent(resposta);
            }
            else
            {
                resposta = state.Conversa.GerarResposta(lower);
                state.LastContext = "root";
                state.Conversa.AddicionarMensagemIfNotPresent(resposta);
            }

            await WriteJson(resp, 200, resposta);
            return;
        }
        else
        {
            // Serve arquivos estáticos de wwwroot
            var filePath = GetFilePathForRequest(path);
            Console.WriteLine($"[Request] Path: {path} -> FilePath: {filePath}");
            if (File.Exists(filePath))
            {
                var bytes = await File.ReadAllBytesAsync(filePath);
                resp.ContentType = GetContentType(Path.GetExtension(filePath));
                resp.ContentLength64 = bytes.Length;
                await resp.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                resp.StatusCode = 200;
                resp.OutputStream.Close();
                return;
            }

            // Fallback para Single Page App: servir index.html se existir
            var www = FindWwwRoot();
            var indexFile = Path.Combine(www, "index.html");
            if (File.Exists(indexFile))
            {
                var bytes = await File.ReadAllBytesAsync(indexFile);
                resp.ContentType = "text/html; charset=utf-8";
                resp.ContentLength64 = bytes.Length;
                await resp.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                resp.StatusCode = 200;
                resp.OutputStream.Close();
                return;
            }

            // Not found
            resp.StatusCode = 404;
            await WriteString(resp, "Not found");
            return;
        }
    }
    catch (Exception ex)
    {
        resp.StatusCode = 500;
        await WriteJson(resp, 500, new { error = ex.Message });
    }
}

string GetFilePathForRequest(string path)
{
    var www = FindWwwRoot();
    Console.WriteLine($"[Static] wwwroot resolved to: {www}");
    if (path == "/" || string.IsNullOrEmpty(path) || path == "/index.html")
        return Path.Combine(www, "index.html");

    // remove leading '/'
    if (path.StartsWith("/")) path = path.Substring(1);

    var combined = Path.Combine(www, path.Replace('/', Path.DirectorySeparatorChar));
    return Path.GetFullPath(combined);
}

string FindWwwRoot()
{
    // Tenta encontrar a pasta wwwroot subindo a partir do diretório atual ou AppContext.BaseDirectory
    var candidates = new List<string>
    {
        AppContext.BaseDirectory,
        Directory.GetCurrentDirectory()
    };

    foreach (var start in candidates)
    {
        var dir = new DirectoryInfo(start);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "wwwroot");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "index.html")))
                return candidate;

            dir = dir.Parent;
        }
    }

    // Fallback: wwwroot in AppContext.BaseDirectory
    var fallback = Path.Combine(AppContext.BaseDirectory, "wwwroot");
    return fallback;
}

string GetContentType(string ext)
{
    return ext.ToLower() switch
    {
        ".html" => "text/html; charset=utf-8",
        ".css" => "text/css",
        ".js" => "application/javascript",
        ".json" => "application/json",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        _ => "application/octet-stream",
    };
}

Task WriteJson(HttpListenerResponse resp, int status, object obj)
{
    var options = new JsonSerializerOptions { PropertyNamingPolicy = null };
    var json = JsonSerializer.Serialize(obj, options);
    resp.StatusCode = status;
    resp.ContentType = "application/json; charset=utf-8";
    var bytes = Encoding.UTF8.GetBytes(json);
    resp.ContentLength64 = bytes.Length;
    return resp.OutputStream.WriteAsync(bytes, 0, bytes.Length);
}

Task WriteString(HttpListenerResponse resp, string text)
{
    var bytes = Encoding.UTF8.GetBytes(text);
    resp.ContentType = "text/plain; charset=utf-8";
    resp.ContentLength64 = bytes.Length;
    return resp.OutputStream.WriteAsync(bytes, 0, bytes.Length);
}

// Modelos auxiliares para API e estado de sessão
public record LoginRequest(string Email, string Password);
public record MessageRequest(string SessionId, string Content);

public class SessionState
{
    public Conversa Conversa { get; set; } = new Conversa();
    public string LastContext { get; set; } = "root";
}

// Extensões utilitárias para não duplicar mensagens
static class ConversaExtensions
{
    public static void AddicionarMensagemIfNotPresent(this Conversa conversa, Mensagem msg)
    {
        if (!conversa.Mensagens.Any(m => m.Conteudo == msg.Conteudo))
            conversa.AdicionarMensagem(msg);
    }
}