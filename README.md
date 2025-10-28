# Sistema de Atendimento Klebao

## Descrição

Sistema de atendimento ao cliente desenvolvido em C# com servidor HTTP integrado, utilizando IA para respostas automáticas e suporte a atendimento humano prioritário.

## Tecnologias Utilizadas

- .NET 8.0
- C# 12.0
- HttpListener para servidor HTTP
- System.Text.Json para serialização
- Frontend em HTML, CSS e JavaScript

## Funcionalidades

### Para Usuários

- **Login**: Autenticação por email e senha
- **Chat Automatizado**: Respostas por IA para dúvidas gerais
- **Sistema de Prioridades**: Classificação de chamados (Alta, Média, Baixa)
- **FAQ**: Perguntas frequentes com respostas predefinidas
- **Atendimento Humano**: Encaminhamento para atendentes em casos prioritários

### Para Administradores

- **Fila de Atendimento**: Visualização de chamados pendentes
- **Chat com Usuários**: Comunicação direta com clientes
- **Resolução de Chamados**: Marcação de chamados como resolvidos
- **Priorização Automática**: Ordenação por prioridade (Alta > Média)

## Estrutura do Projeto

```
TestePIM/
??? Program.cs              # Servidor HTTP e endpoints API
??? Chamado.cs              # Gerenciamento de chamados
??? Conversa.cs             # Gerenciamento de conversas
??? EnvioMsg.cs             # Envio de mensagens (IA/Bot)
??? Mensagem.cs             # Modelo de mensagem
??? Models.cs               # Modelos de dados
??? Prioridade.cs           # Classificação de prioridades
??? wwwroot/
    ??? index.html          # Interface do usuário
    ??? app.js              # Lógica do frontend
    ??? app.css             # Estilos
```

## Endpoints da API

### Autenticação

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
  "content": "mensagem do usuário"
}
```

**GET /api/messages?sessionId={id}**

### Administração

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

1. Clone o repositório
2. Navegue até o diretório do projeto
3. Execute o comando:
```bash
dotnet run
```
4. Acesse no navegador: `http://localhost:5000`

## Credenciais de Administrador

- Email: `admin@klebao.com`
- Senha: `admin@123`

## Fluxo de Atendimento

1. Usuário faz login
2. Sistema apresenta menu com opções (Ajuda, FAQ, Sair)
3. Para dúvidas gerais: IA responde automaticamente
4. Para problemas específicos:
   - Sistema solicita detalhes
   - Usuário informa prioridade
   - Se prioridade baixa: IA responde
   - Se prioridade média/alta: encaminha para atendente
5. Atendente visualiza fila ordenada por prioridade
6. Atendente responde e resolve chamado
7. Sistema reinicia estado do usuário

## Observações

- Sessões são armazenadas em memória (ConcurrentDictionary)
- Servidor escuta em `http://localhost:5000/`
- Frontend é SPA (Single Page Application)
- Sistema suporta múltiplas sessões simultâneas
