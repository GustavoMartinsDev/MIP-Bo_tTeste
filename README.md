# Sistema de Atendimento Klebao

## Descri��o

Sistema de atendimento ao cliente desenvolvido em C# com servidor HTTP integrado, utilizando IA para respostas autom�ticas e suporte a atendimento humano priorit�rio.

## Tecnologias Utilizadas

- .NET 8.0
- C# 12.0
- HttpListener para servidor HTTP
- System.Text.Json para serializa��o
- Frontend em HTML, CSS e JavaScript

## Funcionalidades

### Para Usu�rios

- **Login**: Autentica��o por email e senha
- **Chat Automatizado**: Respostas por IA para d�vidas gerais
- **Sistema de Prioridades**: Classifica��o de chamados (Alta, M�dia, Baixa)
- **FAQ**: Perguntas frequentes com respostas predefinidas
- **Atendimento Humano**: Encaminhamento para atendentes em casos priorit�rios

### Para Administradores

- **Fila de Atendimento**: Visualiza��o de chamados pendentes
- **Chat com Usu�rios**: Comunica��o direta com clientes
- **Resolu��o de Chamados**: Marca��o de chamados como resolvidos
- **Prioriza��o Autom�tica**: Ordena��o por prioridade (Alta > M�dia)

## Estrutura do Projeto

```
TestePIM/
??? Program.cs              # Servidor HTTP e endpoints API
??? Chamado.cs              # Gerenciamento de chamados
??? Conversa.cs             # Gerenciamento de conversas
??? EnvioMsg.cs             # Envio de mensagens (IA/Bot)
??? Mensagem.cs             # Modelo de mensagem
??? Models.cs               # Modelos de dados
??? Prioridade.cs           # Classifica��o de prioridades
??? wwwroot/
    ??? index.html          # Interface do usu�rio
    ??? app.js              # L�gica do frontend
    ??? app.css             # Estilos
```

## Endpoints da API

### Autentica��o

**POST /api/login**
```json
{
  "email": "usuario@email.com",
  "password": "senha123"
}
```

### Mensagens

**POST /api/message**
```json
{
  "sessionId": "guid-da-sessao",
  "content": "mensagem do usu�rio"
}
```

**GET /api/messages?sessionId={id}**

### Administra��o

**GET /api/admin/queue?sessionId={adminSessionId}**

**GET /api/admin/conversation?adminSessionId={id}&targetSessionId={id}**

**POST /api/admin/send**
```json
{
  "adminSessionId": "guid-admin",
  "targetSessionId": "guid-usuario",
  "content": "resposta do atendente"
}
```

**POST /api/admin/resolve**

## Como Executar

1. Clone o reposit�rio
2. Navegue at� o diret�rio do projeto
3. Execute o comando:
```bash
dotnet run
```
4. Acesse no navegador: `http://localhost:5000`

## Credenciais de Administrador

- Email: `admin@klebao.com`
- Senha: `admin@123`

## Fluxo de Atendimento

1. Usu�rio faz login
2. Sistema apresenta menu com op��es (Ajuda, FAQ, Sair)
3. Para d�vidas gerais: IA responde automaticamente
4. Para problemas espec�ficos:
   - Sistema solicita detalhes
   - Usu�rio informa prioridade
   - Se prioridade baixa: IA responde
   - Se prioridade m�dia/alta: encaminha para atendente
5. Atendente visualiza fila ordenada por prioridade
6. Atendente responde e resolve chamado
7. Sistema reinicia estado do usu�rio

## Observa��es

- Sess�es s�o armazenadas em mem�ria (ConcurrentDictionary)
- Servidor escuta em `http://localhost:5000/`
- Frontend � SPA (Single Page Application)
- Sistema suporta m�ltiplas sess�es simult�neas
