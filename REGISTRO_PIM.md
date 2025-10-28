# REGISTRO DO PROJETO INTEGRADO MULTIDISCIPLINAR

## 1 INTRODU��O

Este documento registra o desenvolvimento do sistema de atendimento ao cliente Klebao, apresentando a evolu��o t�cnica e as decis�es de implementa��o conforme normas ABNT.

## 2 OBJETIVOS

Desenvolver sistema de atendimento automatizado com suporte a interven��o humana, utilizando classifica��o de prioridades e integra��o com intelig�ncia artificial.

## 3 DESENVOLVIMENTO

### 3.1 Estrutura do Servidor

O sistema utiliza HttpListener para criar servidor HTTP sem depend�ncias externas de frameworks web.

```csharp
var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:5000/");
listener.Start();
```

A arquitetura ass�ncrona permite m�ltiplas requisi��es simult�neas atrav�s de tasks:

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

### 3.2 Gerenciamento de Sess�es

Implementa��o de dicion�rio concorrente para armazenamento thread-safe de sess�es:

```csharp
var sessions = new ConcurrentDictionary<string, SessionState>();
```

Cada sess�o mant�m estado da conversa, contexto atual e informa��es de prioridade:

```csharp
var state = new SessionState 
{ 
    Conversa = chamado.novaConversa, 
    LastContext = "root", 
    Email = login.Email 
};
```

### 3.3 Sistema de Autentica��o

Endpoint de login valida credenciais e cria sess�o �nica:

```csharp
if (path.StartsWith("/api/login") && req.HttpMethod == "POST")
{
    var sessionId = Guid.NewGuid().ToString();
    sessions[sessionId] = state;
}
```

Conta administrativa possui privil�gios especiais:

```csharp
if (login.Email == "admin@klebao.com" && login.Password == "admin@123")
{
    state.IsAdmin = true;
}
```

### 3.4 Fluxo Conversacional

Sistema implementa m�quina de estados para controlar contexto da conversa:

```csharp
if (state.LastContext == "awaiting_priority")
{
    var p = TestePIM.Utils.ParsePriority(lower);
    // Processa prioridade informada
}
```

Detec��o de necessidade de atendimento humano:

```csharp
var classification = Prioridade.ClassificarPrioridade(lower);
if (classification == "N�o classificada")
{
    var iaResp = EnvioMsg.EnviarBot(messageReq.Content);
    // Responde por IA
}
```

### 3.5 Sistema de Prioridades

Classifica��o em tr�s n�veis (Alta, M�dia, Baixa):

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

### 3.7 Resolu��o de Chamados

Marca��o de resolu��o e reset do estado do usu�rio:

```csharp
targetState.Conversa = chamado.novaConversa;
targetState.LastContext = "root";
targetState.IsWaitingHuman = false;
targetState.Priority = Priority.Invalid;
targetState.AssignedAgent = null;
```

### 3.8 Servir Arquivos Est�ticos

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

Determina��o de Content-Type baseado em extens�o:

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

- Gerenciamento concorrente de m�ltiplas sess�es
- Classifica��o autom�tica de prioridades
- Integra��o IA para atendimento b�sico
- Interface administrativa para gest�o de fila
- Persist�ncia de conversas durante sess�o

## 5 CONSIDERA��ES FINAIS

O projeto demonstra aplica��o de conceitos de programa��o ass�ncrona, gerenciamento de estado, APIs RESTful e integra��o frontend-backend, atendendo aos requisitos propostos.

## REFER�NCIAS

MICROSOFT. System.Net.HttpListener Class. Dispon�vel em: https://learn.microsoft.com/dotnet/api/system.net.httplistener. Acesso em: 2024.

MICROSOFT. System.Collections.Concurrent Namespace. Dispon�vel em: https://learn.microsoft.com/dotnet/api/system.collections.concurrent. Acesso em: 2024.
