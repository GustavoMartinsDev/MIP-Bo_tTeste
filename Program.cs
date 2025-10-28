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
            var state = new SessionState { Conversa = chamado.novaConversa, LastContext = "root", Email = login.Email };

            // Admin account special
            if (login.Email == "admin@klebao.com" && login.Password == "admin@123")
            {
                state.IsAdmin = true;
            }

            sessions[sessionId] = state;

            await WriteJson(resp, 200, new { sessionId, welcome = chamado.MensagemBoasVindas, isAdmin = state.IsAdmin });
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

            // If admin is sending to a user via /api/message, disallow here (admin uses admin endpoint)
            if (state.IsAdmin)
            {
                await WriteJson(resp, 403, new { error = "Admin deve usar a área administrativa" });
                return;
            }

            // Register user's message
            var userMsg = EnvioMsg.EnviarUsuario(messageReq.Content);
            state.Conversa.AdicionarMensagem(userMsg);

            Mensagem resposta = null;
            bool queued = false; // indicates session was queued for human

            var lower = content.ToLower();

            // If user is in human assistance (waiting or assigned), don't process, just store message
            if (state.IsWaitingHuman || !string.IsNullOrEmpty(state.AssignedAgent))
            {
                // Just acknowledge, don't send to IA
                await WriteJson(resp, 200, new { ok = true, inHumanQueue = true });
                return;
            }

            // PRIORITY/CONTEXT-SPECIFIC HANDLING FIRST to avoid conflicts with menu numbers
            if (state.LastContext == "awaiting_priority")
            {
                // Validate priority
                var p = TestePIM.Utils.ParsePriority(lower);
                if (p == TestePIM.Priority.Invalid)
                {
                    resposta = new Mensagem { Remetente = "Klebao", Conteudo = "Prioridade invalida. Digite: 1 para Alta, 2 para Media, 3 para Baixa." };
                    state.Conversa.AdicionarMensagem(resposta);
                }
                else if (p == TestePIM.Priority.Low)
                {
                    // Low -> IA (use the stored TempDetail)
                    var iaResp = EnvioMsg.EnviarBot(state.TempDetail ?? messageReq.Content);
                    state.Conversa.AdicionarMensagem(iaResp);
                    resposta = iaResp;
                    state.LastContext = "root";
                    // clear temp detail after handling
                    state.TempDetail = null;
                }
                else
                {
                    // Medium or High -> enviar para fila humana, keep TempDetail for agent
                    state.IsWaitingHuman = true;
                    state.Priority = p;
                    state.ChamadoTitulo = state.TempDetail?.Length > 50 ? state.TempDetail.Substring(0, 50) + "..." : state.TempDetail;
                    state.LastContext = "waiting_human";
                    queued = true;
                    resposta = new Mensagem { Remetente = "Klebao", Conteudo = "Obrigado - sua solicitacao foi encaminhada para atendimento humano. Aguarde que um atendente ira falar com voce em breve." };
                    state.Conversa.AdicionarMensagem(resposta);
                }

                // Return result
                if (resposta != null)
                {
                    await WriteJson(resp, 200, new { message = resposta, queued });
                }
                else
                {
                    await WriteJson(resp, 200, new { ok = true, queued });
                }
                return;
            }

            // If we were asking for details specifically
            if (state.LastContext == "awaiting_details")
            {
                // Save detail and ask for priority - do not interpret numbers here as menu options
                state.TempDetail = messageReq.Content;
                resposta = new Mensagem { Remetente = "Klebao", Conteudo = "Por favor, informe a prioridade do atendimento: 1 - Alta, 2 - Media, 3 - Baixa" };
                state.LastContext = "awaiting_priority";
                state.Conversa.AdicionarMensagem(resposta);

                await WriteJson(resp, 200, new { message = resposta, queued = false });
                return;
            }

            // If we are inside FAQ expecting a number, handle that
            if (state.LastContext == "faq" && int.TryParse(lower, out _))
            {
                resposta = state.Conversa.GerarRespostaFaq(lower);
                state.LastContext = "root"; // Volta ao contexto root após responder FAQ
                state.Conversa.AdicionarMensagem(resposta);

                // Also add the welcome message again so user can choose menu options
                var welcomeAgain = new Mensagem 
                { 
                    Remetente = "Klebao", 
                    Conteudo = "Me diz ai do que voce precisa:\n\n1 - Ajuda\n2 - FAQ (Perguntas Frequentes)\n0 - Sair" 
                };
                state.Conversa.AdicionarMensagem(welcomeAgain);

                await WriteJson(resp, 200, new { messages = new[] { resposta, welcomeAgain }, queued = false });
                return;
            }

            // MENU and generic handling (only when not in a special awaiting state)
            if (lower == "1" || lower == "ajuda")
            {
                resposta = Chamado.GerarMensagemAjuda();
                state.LastContext = "awaiting_details";
                state.Conversa.AdicionarMensagem(resposta);

                await WriteJson(resp, 200, new { message = resposta, queued = false });
                return;
            }

            if (lower == "2" || lower == "faq")
            {
                resposta = Chamado.GerarMensagemFAQ();
                state.LastContext = "faq";
                state.Conversa.AdicionarMensagem(resposta);

                await WriteJson(resp, 200, new { message = resposta, queued = false });
                return;
            }

            // Generic input when in root: decide if it's a help request or general query
            var classification = Prioridade.ClassificarPrioridade(lower);

            if (classification == "Não classificada")
            {
                // Not a help/issue we recognize -> answer by IA immediately
                var iaResp = EnvioMsg.EnviarBot(messageReq.Content);
                resposta = iaResp;
                state.Conversa.AdicionarMensagem(resposta);
                state.LastContext = "root";

                await WriteJson(resp, 200, new { message = resposta, queued = false });
                return;
            }
            else
            {
                // Recognized as an issue: ask for priority before queuing
                state.TempDetail = messageReq.Content;
                resposta = new Mensagem { Remetente = "Klebao", Conteudo = "Detectei que voce precisa de ajuda. Por favor, informe a prioridade do atendimento: 1 - Alta, 2 - Media, 3 - Baixa" };
                state.LastContext = "awaiting_priority";
                state.Conversa.AdicionarMensagem(resposta);

                await WriteJson(resp, 200, new { message = resposta, queued = false });
                return;
            }
        }
        else if (path.StartsWith("/api/messages") && req.HttpMethod == "GET")
        {
            var q = req.QueryString;
            var sessionId = q["sessionId"];
            if (string.IsNullOrWhiteSpace(sessionId) || !sessions.TryGetValue(sessionId, out var state))
            {
                await WriteJson(resp, 400, new { error = "sessionId inválido" });
                return;
            }

            // Return messages plus waiting/human info
            await WriteJson(resp, 200, new { messages = state.Conversa.Mensagens, waitingHuman = state.IsWaitingHuman, assignedAgent = state.AssignedAgent });
            return;
        }
        else if (path.StartsWith("/api/admin/queue") && req.HttpMethod == "GET")
        {
            var q = req.QueryString;
            var adminSessionId = q["sessionId"];
            if (string.IsNullOrWhiteSpace(adminSessionId) || !sessions.TryGetValue(adminSessionId, out var adminState) || !adminState.IsAdmin)
            {
                await WriteJson(resp, 403, new { error = "Acesso negado" });
                return;
            }

            // Only show users that are still waiting for human (not assigned or still in queue)
            var list = sessions.Where(kv => 
                kv.Value.IsWaitingHuman && 
                (kv.Value.Priority == Priority.High || kv.Value.Priority == Priority.Medium)
            ).Select(kv => new {
                sessionId = kv.Key,
                email = kv.Value.Email,
                titulo = kv.Value.ChamadoTitulo ?? (kv.Value.TempDetail?.Length > 50 ? kv.Value.TempDetail.Substring(0, 50) + "..." : kv.Value.TempDetail ?? "Sem título"),
                priority = TestePIM.Utils.PriorityToPortuguese(kv.Value.Priority),
                priorityLevel = (int)kv.Value.Priority
            }).OrderBy(x => x.priorityLevel).ToList();

            await WriteJson(resp, 200, list);
            return;
        }
        else if (path.StartsWith("/api/admin/conversation") && req.HttpMethod == "GET")
        {
            var q = req.QueryString;
            var adminSessionId = q["adminSessionId"];
            var target = q["targetSessionId"];
            if (string.IsNullOrWhiteSpace(adminSessionId) || !sessions.TryGetValue(adminSessionId, out var adminState) || !adminState.IsAdmin)
            {
                await WriteJson(resp, 403, new { error = "Acesso negado" });
                return;
            }

            if (string.IsNullOrWhiteSpace(target) || !sessions.TryGetValue(target, out var targetState))
            {
                await WriteJson(resp, 400, new { error = "targetSessionId inválido" });
                return;
            }

            await WriteJson(resp, 200, targetState.Conversa.Mensagens);
            return;
        }
        else if (path.StartsWith("/api/admin/send") && req.HttpMethod == "POST")
        {
            using var sr = new StreamReader(req.InputStream, req.ContentEncoding);
            var body = await sr.ReadToEndAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var sendReq = JsonSerializer.Deserialize<AdminSendRequest>(body, options);

            if (sendReq == null || string.IsNullOrWhiteSpace(sendReq.AdminSessionId) || string.IsNullOrWhiteSpace(sendReq.TargetSessionId) || string.IsNullOrWhiteSpace(sendReq.Content))
            {
                await WriteJson(resp, 400, new { error = "Parâmetros inválidos" });
                return;
            }

            if (!sessions.TryGetValue(sendReq.AdminSessionId, out var adminState) || !adminState.IsAdmin)
            {
                await WriteJson(resp, 403, new { error = "Acesso negado" });
                return;
            }

            if (!sessions.TryGetValue(sendReq.TargetSessionId, out var targetState))
            {
                await WriteJson(resp, 400, new { error = "Target não encontrado" });
                return;
            }

            var msg = new Mensagem { Remetente = "Atendente", Conteudo = sendReq.Content };
            targetState.Conversa.AdicionarMensagem(msg);
            
            // Only mark as not waiting for human if this is the first admin message (assignment)
            // Keep IsWaitingHuman true so it stays in queue until resolved
            if (string.IsNullOrEmpty(targetState.AssignedAgent))
            {
                targetState.AssignedAgent = adminState.Email;
            }

            await WriteJson(resp, 200, new { ok = true });
            return;
        }
        else if (path.StartsWith("/api/admin/resolve") && req.HttpMethod == "POST")
        {
            using var sr = new StreamReader(req.InputStream, req.ContentEncoding);
            var body = await sr.ReadToEndAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var resolveReq = JsonSerializer.Deserialize<AdminSendRequest>(body, options);

            if (resolveReq == null || string.IsNullOrWhiteSpace(resolveReq.AdminSessionId) || string.IsNullOrWhiteSpace(resolveReq.TargetSessionId))
            {
                await WriteJson(resp, 400, new { error = "Parâmetros inválidos" });
                return;
            }

            if (!sessions.TryGetValue(resolveReq.AdminSessionId, out var adminState) || !adminState.IsAdmin)
            {
                await WriteJson(resp, 403, new { error = "Acesso negado" });
                return;
            }

            if (!sessions.TryGetValue(resolveReq.TargetSessionId, out var targetState))
            {
                await WriteJson(resp, 400, new { error = "Target não encontrado" });
                return;
            }

            // Send resolution message to user
            var resolutionMsg = new Mensagem 
            { 
                Remetente = "Atendente", 
                Conteudo = "Seu chamado foi marcado como resolvido. Obrigado por utilizar nossos servicos!" 
            };
            targetState.Conversa.AdicionarMensagem(resolutionMsg);

            // Wait a bit before resetting so user can see the message
            await Task.Delay(1000);

            // Reset user state completely
            var chamado = new Chamado();
            targetState.Conversa = chamado.novaConversa;
            targetState.LastContext = "root";
            targetState.IsWaitingHuman = false;
            targetState.Priority = Priority.Invalid;
            targetState.TempDetail = null;
            targetState.AssignedAgent = null;
            targetState.ChamadoTitulo = null;

            await WriteJson(resp, 200, new { ok = true, resolved = true });
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
    var options = new JsonSerializerOptions 
    { 
        PropertyNamingPolicy = null,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
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