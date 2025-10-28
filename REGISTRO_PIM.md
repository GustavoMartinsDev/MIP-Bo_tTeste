# REGISTRO DO PROJETO INTEGRADO MULTIDISCIPLINAR

## 1 INTRODUÇÃO

Este documento registra o desenvolvimento do sistema de atendimento ao cliente Klebao, apresentando a evolução técnica e as decisões de implementação conforme normas ABNT.

## 2 OBJETIVOS

Desenvolver sistema de atendimento automatizado com suporte a intervenção humana, utilizando classificação de prioridades e integração com inteligência artificial.

## 3 DESENVOLVIMENTO

### 3.1 Estrutura do Servidor

O sistema utiliza HttpListener para criar servidor HTTP sem dependências externas de frameworks web.

```csharp
var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:5000/");
listener.Start();
```

A arquitetura assíncrona permite múltiplas requisições simultâneas através de tasks:

```csharp
_ = Task.Run(() =>
{
    while (listener.IsListening)
    {
        var ctx = listener.GetContext();
        _ = Task.Run(() => HandleRequest(ctx));
    }
});
```

### 3.2 Gerenciamento de Sessões

Implementação de dicionário concorrente para armazenamento thread-safe de sessões:

```csharp
var sessions = new ConcurrentDictionary<string, SessionState>();
```

Cada sessão mantém estado da conversa, contexto atual e informações de prioridade:

```csharp
var state = new SessionState 
{ 
    Conversa = chamado.novaConversa, 
    LastContext = "root", 
    Email = login.Email 
};
```

### 3.3 Sistema de Autenticação

Endpoint de login valida credenciais e cria sessão única:

```csharp
if (path.StartsWith("/api/login") && req.HttpMethod == "POST")
{
    var sessionId = Guid.NewGuid().ToString();
    sessions[sessionId] = state;
}
```

Conta administrativa possui privilégios especiais:

```csharp
if (login.Email == "admin@klebao.com" && login.Password == "admin@123")
{
    state.IsAdmin = true;
}
```

### 3.4 Fluxo Conversacional

Sistema implementa máquina de estados para controlar contexto da conversa:

```csharp
if (state.LastContext == "awaiting_priority")
{
    var p = TestePIM.Utils.ParsePriority(lower);
    // Processa prioridade informada
}
```

Detecção de necessidade de atendimento humano:

```csharp
var classification = Prioridade.ClassificarPrioridade(lower);
if (classification == "Não classificada")
{
    var iaResp = EnvioMsg.EnviarBot(messageReq.Content);
    // Responde por IA
}
```

### 3.5 Sistema de Prioridades

Classificação em três níveis (Alta, Média, Baixa):

```csharp
if (p == TestePIM.Priority.Low)
{
    var iaResp = EnvioMsg.EnviarBot(state.TempDetail);
    // IA resolve
}
else
{
    state.IsWaitingHuman = true;
    state.Priority = p;
    // Encaminha para humano
}
```

### 3.6 Interface Administrativa

Fila de atendimento ordenada por prioridade:

```csharp
var list = sessions.Where(kv => 
    kv.Value.IsWaitingHuman && 
    (kv.Value.Priority == Priority.High || 
     kv.Value.Priority == Priority.Medium)
).OrderBy(x => x.priorityLevel);
```

Envio de mensagens do atendente:

```csharp
var msg = new Mensagem 
{ 
    Remetente = "Atendente", 
    Conteudo = sendReq.Content 
};
targetState.Conversa.AdicionarMensagem(msg);
```

### 3.7 Resolução de Chamados

Marcação de resolução e reset do estado do usuário:

```csharp
targetState.Conversa = chamado.novaConversa;
targetState.LastContext = "root";
targetState.IsWaitingHuman = false;
targetState.Priority = Priority.Invalid;
targetState.AssignedAgent = null;
```

### 3.8 Servir Arquivos Estáticos

Sistema localiza pasta wwwroot e serve arquivos HTML, CSS e JavaScript:

```csharp
string FindWwwRoot()
{
    var candidates = new List<string>
    {
        AppContext.BaseDirectory,
        Directory.GetCurrentDirectory()
    };
    // Busca recursiva por wwwroot
}
```

Determinação de Content-Type baseado em extensão:

```csharp
string GetContentType(string ext)
{
    return ext.ToLower() switch
    {
        ".html" => "text/html; charset=utf-8",
        ".css" => "text/css",
        ".js" => "application/javascript",
        _ => "application/octet-stream",
    };
}
```

## 4 RESULTADOS

Sistema implementado apresenta:

- Gerenciamento concorrente de múltiplas sessões
- Classificação automática de prioridades
- Integração IA para atendimento básico
- Interface administrativa para gestão de fila
- Persistência de conversas durante sessão

## 5 CONSIDERAÇÕES FINAIS

O projeto demonstra aplicação de conceitos de programação assíncrona, gerenciamento de estado, APIs RESTful e integração frontend-backend, atendendo aos requisitos propostos.

## REFERÊNCIAS

MICROSOFT. System.Net.HttpListener Class. Disponível em: https://learn.microsoft.com/dotnet/api/system.net.httplistener. Acesso em: 2024.

MICROSOFT. System.Collections.Concurrent Namespace. Disponível em: https://learn.microsoft.com/dotnet/api/system.collections.concurrent. Acesso em: 2024.
