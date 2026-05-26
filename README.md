# whatsapp-bot-csharp

Bot de WhatsApp com inteligência artificial integrada, construído do zero em C# com ASP.NET Core. Recebe mensagens, processa com um modelo de linguagem e responde automaticamente — com histórico de conversa, painel de configuração web e arquitetura limpa e extensível.

---

## Stack

<p>
  <img src="https://img.shields.io/badge/C%23-%23239120?style=for-the-badge&logo=csharp&logoColor=white" />
  <img src="https://img.shields.io/badge/.NET_8-%23512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/SQLite-%23003B57?style=for-the-badge&logo=sqlite&logoColor=white" />
  <img src="https://img.shields.io/badge/Docker-%232496ED?style=for-the-badge&logo=docker&logoColor=white" />
  <img src="https://img.shields.io/badge/Groq-black?style=for-the-badge&logo=groq&logoColor=white" />
  <img src="https://img.shields.io/badge/Evolution_API-%2325D366?style=for-the-badge&logo=whatsapp&logoColor=white" />
</p>

---

## Como funciona

```
Usuário envia mensagem no WhatsApp
        ↓
Evolution API detecta e dispara webhook → ASP.NET Core
        ↓
WebhookController extrai número e texto
        ↓
Busca histórico de conversa no SQLite
        ↓
Envia contexto + mensagem pro modelo via Groq API
        ↓
Salva resposta no banco
        ↓
Envia resposta de volta via Evolution API
        ↓
Usuário recebe a resposta
```

---

## Estrutura

```
WhatsAppBot/
├── Controllers/
│   ├── WebhookController.cs     # recebe mensagens do WhatsApp
│   └── ConfigController.cs      # API do painel de configuração
├── Services/
│   ├── GroqService.cs           # integração com Groq (LLaMA 3.3)
│   └── WhatsAppService.cs       # envio de mensagens via Evolution API
├── Models/
│   ├── BotConfig.cs             # número, system prompt, credenciais
│   └── Conversation.cs          # histórico de mensagens por contato
├── Data/
│   └── AppDbContext.cs          # EF Core + SQLite
├── wwwroot/
│   └── index.html               # painel de configuração web
└── Program.cs
```

---

## Painel de configuração

O projeto inclui uma interface web acessível em `http://localhost:5034` para configurar o bot sem tocar no código:

- Definir o system prompt (personalidade do bot)
- Cadastrar o número conectado
- Inserir credenciais da Evolution API
- Limpar histórico de conversa por contato

---

## Rodando localmente

### Pré-requisitos

- .NET 8 SDK
- Docker Desktop
- Conta na [Groq](https://console.groq.com) (gratuita)

### 1. Suba a Evolution API

```bash
docker compose up -d
```

### 2. Configure o appsettings.json

```json
{
  "Groq": {
    "ApiKey": "sua-chave-groq"
  },
  "Evolution": {
    "InstanceName": "minha-instancia"
  }
}
```

### 3. Rode o projeto

```bash
dotnet ef database update
dotnet run
```

### 4. Conecte o WhatsApp

Acesse `http://localhost:8080/manager`, crie uma instância e escaneie o QR Code.

### 5. Configure o webhook

Aponte o webhook da instância para:

```
http://host.docker.internal:5034/webhook
```

Evento: `MESSAGES_UPSERT`

---

## Variáveis de ambiente sensíveis

Nunca suba suas chaves reais para o repositório. Use o `appsettings.json` localmente e mantenha os placeholders no arquivo versionado.

| Variável | Descrição |
|---|---|
| `Groq:ApiKey` | Chave da API do Groq |
| `Evolution:InstanceName` | Nome da instância na Evolution API |
| `BotConfig.EvolutionApiKey` | Chave de autenticação da Evolution |

---

## Autor

Desenvolvido por **Kauã Daudt**  
Estudante de Análise e Desenvolvimento de Sistemas — FIAP  
[github.com/KauaDaudt](https://github.com/KauaDaudt)
