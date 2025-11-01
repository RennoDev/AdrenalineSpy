# MailKit - Envio e Recebimento de Emails

## Índice
1. [Introdução](#introdução)
2. [Instalação](#instalação)
3. [Enviar Email (SMTP)](#enviar-email-smtp)
4. [Receber Email (IMAP/POP3)](#receber-email-imappop3)
5. [Anexos](#anexos)
6. [HTML e Formatação](#html-e-formatação)
7. [Exemplos Práticos](#exemplos-práticos)

---

## Introdução

**MailKit** é uma biblioteca completa para trabalhar com emails em .NET, suportando SMTP, POP3 e IMAP.

### Vantagens
- ✅ Moderno e mantido
- ✅ Suporta SMTP, POP3, IMAP
- ✅ SSL/TLS
- ✅ Anexos e HTML
- ✅ Multiplataforma

---

## Instalação

```bash
dotnet add package MailKit
```

---

## Enviar Email (SMTP)

### Email Simples

```csharp
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

void EnviarEmailSimples()
{
    var message = new MimeMessage();
    
    // Remetente
    message.From.Add(new MailboxAddress("Meu Nome", "seu@email.com"));
    
    // Destinatário
    message.To.Add(new MailboxAddress("Nome Destino", "destino@email.com"));
    
    // Assunto
    message.Subject = "Teste de Email";
    
    // Corpo (texto simples)
    message.Body = new TextPart("plain")
    {
        Text = "Olá! Este é um email de teste."
    };
    
    // Enviar
    using (var client = new SmtpClient())
    {
        // Conectar ao servidor SMTP
        client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        
        // Autenticar
        client.Authenticate("seu@email.com", "sua_senha");
        
        // Enviar
        client.Send(message);
        
        // Desconectar
        client.Disconnect(true);
    }
    
    Console.WriteLine("Email enviado!");
}
```

### Configurações SMTP Comuns

```csharp
// Gmail
client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
client.Authenticate("seuemail@gmail.com", "senha_app");

// Outlook/Hotmail
client.Connect("smtp-mail.outlook.com", 587, SecureSocketOptions.StartTls);
client.Authenticate("seuemail@outlook.com", "senha");

// Yahoo
client.Connect("smtp.mail.yahoo.com", 587, SecureSocketOptions.StartTls);

// Office 365
client.Connect("smtp.office365.com", 587, SecureSocketOptions.StartTls);

// Servidor local (sem SSL)
client.Connect("localhost", 25, SecureSocketOptions.None);
```

### Múltiplos Destinatários

```csharp
var message = new MimeMessage();
message.From.Add(new MailboxAddress("Remetente", "from@email.com"));

// Para (To)
message.To.Add(new MailboxAddress("Pessoa 1", "pessoa1@email.com"));
message.To.Add(new MailboxAddress("Pessoa 2", "pessoa2@email.com"));

// Cópia (Cc)
message.Cc.Add(new MailboxAddress("Copia", "copia@email.com"));

// Cópia Oculta (Bcc)
message.Bcc.Add(new MailboxAddress("Oculto", "oculto@email.com"));

message.Subject = "Email para múltiplos destinatários";
message.Body = new TextPart("plain") { Text = "Mensagem" };

// Enviar...
```

---

## Receber Email (IMAP/POP3)

### Listar Emails (IMAP)

```csharp
using MailKit.Net.Imap;

void ListarEmails()
{
    using (var client = new ImapClient())
    {
        client.Connect("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
        client.Authenticate("seu@email.com", "sua_senha");
        
        // Abrir pasta
        var inbox = client.Inbox;
        inbox.Open(FolderAccess.ReadOnly);
        
        Console.WriteLine($"Total de mensagens: {inbox.Count}");
        Console.WriteLine($"Novas mensagens: {inbox.Recent}");
        
        // Listar últimos 10 emails
        for (int i = inbox.Count - 1; i >= Math.Max(0, inbox.Count - 10); i--)
        {
            var message = inbox.GetMessage(i);
            
            Console.WriteLine($"\n--- Email {i + 1} ---");
            Console.WriteLine($"De: {message.From}");
            Console.WriteLine($"Assunto: {message.Subject}");
            Console.WriteLine($"Data: {message.Date}");
        }
        
        client.Disconnect(true);
    }
}
```

### Buscar Emails Não Lidos

```csharp
using MailKit.Search;

var uids = inbox.Search(SearchQuery.NotSeen);

foreach (var uid in uids)
{
    var message = inbox.GetMessage(uid);
    Console.WriteLine($"Não lido: {message.Subject}");
}
```

### Marcar como Lido

```csharp
inbox.AddFlags(uid, MessageFlags.Seen, true);
```

### Deletar Email

```csharp
inbox.AddFlags(uid, MessageFlags.Deleted, true);
inbox.Expunge(); // Permanentemente remover
```

---

## Anexos

### Enviar com Anexo

```csharp
var message = new MimeMessage();
message.From.Add(new MailboxAddress("Remetente", "from@email.com"));
message.To.Add(new MailboxAddress("Destinatário", "to@email.com"));
message.Subject = "Email com Anexo";

var builder = new BodyBuilder();
builder.TextBody = "Veja o anexo em anexo.";

// Adicionar anexo
builder.Attachments.Add("C:\\caminho\\arquivo.pdf");
builder.Attachments.Add("C:\\caminho\\imagem.png");

// Anexar de bytes
byte[] dados = File.ReadAllBytes("arquivo.xlsx");
builder.Attachments.Add("planilha.xlsx", dados);

message.Body = builder.ToMessageBody();

// Enviar...
```

### Baixar Anexos

```csharp
var message = inbox.GetMessage(0);

foreach (var attachment in message.Attachments)
{
    if (attachment is MimePart mimePart)
    {
        string fileName = mimePart.FileName;
        
        using (var stream = File.Create(fileName))
        {
            mimePart.Content.DecodeTo(stream);
        }
        
        Console.WriteLine($"Anexo salvo: {fileName}");
    }
}
```

---

## HTML e Formatação

### Email HTML

```csharp
var message = new MimeMessage();
message.From.Add(new MailboxAddress("Remetente", "from@email.com"));
message.To.Add(new MailboxAddress("Destinatário", "to@email.com"));
message.Subject = "Email HTML";

var builder = new BodyBuilder();

// HTML
builder.HtmlBody = @"
<html>
<head>
    <style>
        body { font-family: Arial; }
        h1 { color: #333; }
        .destaque { background-color: yellow; }
    </style>
</head>
<body>
    <h1>Bem-vindo!</h1>
    <p>Este é um <span class='destaque'>email HTML</span>.</p>
    <p><a href='https://example.com'>Clique aqui</a></p>
</body>
</html>";

// Alternativa em texto (fallback)
builder.TextBody = "Bem-vindo! Este é um email HTML.";

message.Body = builder.ToMessageBody();

// Enviar...
```

### HTML com Imagem Embutida

```csharp
var builder = new BodyBuilder();

// Adicionar imagem como recurso
var image = builder.LinkedResources.Add("logo.png");
image.ContentId = MimeUtils.GenerateMessageId();

builder.HtmlBody = $@"
<html>
<body>
    <img src='cid:{image.ContentId}' alt='Logo'/>
    <p>Email com imagem embutida</p>
</body>
</html>";

message.Body = builder.ToMessageBody();
```

---

## Exemplos Práticos

### Exemplo 1: Notificação de Erro

```csharp
void EnviarNotificacaoErro(Exception ex, string contexto)
{
    var message = new MimeMessage();
    message.From.Add(new MailboxAddress("Sistema RPA", "sistema@empresa.com"));
    message.To.Add(new MailboxAddress("Suporte", "suporte@empresa.com"));
    message.Subject = $"[ERRO] {contexto}";
    
    var builder = new BodyBuilder();
    builder.HtmlBody = $@"
    <html>
    <body>
        <h2 style='color: red;'>Erro na Automação</h2>
        <p><strong>Contexto:</strong> {contexto}</p>
        <p><strong>Mensagem:</strong> {ex.Message}</p>
        <p><strong>Stack Trace:</strong></p>
        <pre>{ex.StackTrace}</pre>
        <p><strong>Data/Hora:</strong> {DateTime.Now}</p>
    </body>
    </html>";
    
    message.Body = builder.ToMessageBody();
    
    using (var client = new SmtpClient())
    {
        client.Connect("smtp.empresa.com", 587, SecureSocketOptions.StartTls);
        client.Authenticate("sistema@empresa.com", "senha");
        client.Send(message);
        client.Disconnect(true);
    }
}
```

### Exemplo 2: Relatório com Anexo

```csharp
void EnviarRelatorioComAnexo(string relatorioPath)
{
    var message = new MimeMessage();
    message.From.Add(new MailboxAddress("Automação RPA", "rpa@empresa.com"));
    message.To.Add(new MailboxAddress("Gerente", "gerente@empresa.com"));
    message.Subject = $"Relatório Diário - {DateTime.Now:dd/MM/yyyy}";
    
    var builder = new BodyBuilder();
    builder.HtmlBody = $@"
    <html>
    <body>
        <h2>Relatório Diário de Processamento</h2>
        <p>Segue em anexo o relatório do dia {DateTime.Now:dd/MM/yyyy}.</p>
        <p>
            <strong>Resumo:</strong><br/>
            - Total processado: 150 itens<br/>
            - Sucesso: 145 itens<br/>
            - Erros: 5 itens
        </p>
        <p>Atenciosamente,<br/>Sistema RPA</p>
    </body>
    </html>";
    
    builder.Attachments.Add(relatorioPath);
    message.Body = builder.ToMessageBody();
    
    using (var client = new SmtpClient())
    {
        client.Connect("smtp.empresa.com", 587, SecureSocketOptions.StartTls);
        client.Authenticate("rpa@empresa.com", "senha");
        client.Send(message);
        client.Disconnect(true);
    }
    
    Console.WriteLine("Relatório enviado por email!");
}
```

### Exemplo 3: Processar Emails Não Lidos

```csharp
void ProcessarEmailsNaoLidos()
{
    using (var client = new ImapClient())
    {
        client.Connect("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
        client.Authenticate("seu@email.com", "senha_app");
        
        var inbox = client.Inbox;
        inbox.Open(FolderAccess.ReadWrite);
        
        var uids = inbox.Search(SearchQuery.NotSeen);
        
        foreach (var uid in uids)
        {
            var message = inbox.GetMessage(uid);
            
            Console.WriteLine($"Processando: {message.Subject}");
            
            // Processar email
            if (message.Subject.Contains("URGENTE"))
            {
                ProcessarEmailUrgente(message);
            }
            
            // Baixar anexos
            foreach (var attachment in message.Attachments.OfType<MimePart>())
            {
                string fileName = attachment.FileName;
                using (var stream = File.Create($"downloads/{fileName}"))
                {
                    attachment.Content.DecodeTo(stream);
                }
            }
            
            // Marcar como lido
            inbox.AddFlags(uid, MessageFlags.Seen, true);
        }
        
        client.Disconnect(true);
    }
}
```

### Exemplo 4: Classe EmailService

```csharp
public class EmailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _username;
    private readonly string _password;
    
    public EmailService(string smtpServer, int smtpPort, string username, string password)
    {
        _smtpServer = smtpServer;
        _smtpPort = smtpPort;
        _username = username;
        _password = password;
    }
    
    public void EnviarEmail(string para, string assunto, string corpoHtml, List<string> anexos = null)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Sistema", _username));
        message.To.Add(MailboxAddress.Parse(para));
        message.Subject = assunto;
        
        var builder = new BodyBuilder();
        builder.HtmlBody = corpoHtml;
        
        if (anexos != null)
        {
            foreach (var anexo in anexos)
            {
                builder.Attachments.Add(anexo);
            }
        }
        
        message.Body = builder.ToMessageBody();
        
        using (var client = new SmtpClient())
        {
            client.Connect(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
            client.Authenticate(_username, _password);
            client.Send(message);
            client.Disconnect(true);
        }
    }
    
    public async Task EnviarEmailAsync(string para, string assunto, string corpoHtml)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Sistema", _username));
        message.To.Add(MailboxAddress.Parse(para));
        message.Subject = assunto;
        message.Body = new TextPart("html") { Text = corpoHtml };
        
        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_username, _password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}

// Uso
var emailService = new EmailService(
    "smtp.gmail.com", 
    587, 
    "seu@email.com", 
    "senha_app"
);

emailService.EnviarEmail(
    "destino@email.com",
    "Teste",
    "<h1>Olá!</h1><p>Mensagem de teste</p>"
);
```

---

## Boas Práticas

### 1. Use Senhas de Aplicativo (Gmail)

Para Gmail, não use sua senha normal. Crie uma senha de aplicativo:
1. Acesse sua conta Google
2. Segurança → Verificação em duas etapas → Senhas de app
3. Gere uma senha para "App de email"

### 2. Use Async para Múltiplos Emails

```csharp
await client.ConnectAsync(...);
await client.AuthenticateAsync(...);
await client.SendAsync(message);
```

### 3. Trate Exceções

```csharp
try
{
    client.Send(message);
}
catch (SmtpCommandException ex)
{
    Console.WriteLine($"Erro SMTP: {ex.Message}");
}
catch (SmtpProtocolException ex)
{
    Console.WriteLine($"Erro de protocolo: {ex.Message}");
}
```

### 4. Reutilize Conexão para Múltiplos Envios

```csharp
using (var client = new SmtpClient())
{
    client.Connect(...);
    client.Authenticate(...);
    
    foreach (var destinatario in destinatarios)
    {
        var message = CriarMensagem(destinatario);
        client.Send(message);
    }
    
    client.Disconnect(true);
}
```

### 5. Use Configuration

```csharp
// appsettings.json
{
    "Email": {
        "SmtpServer": "smtp.gmail.com",
        "SmtpPort": 587,
        "Username": "seu@email.com",
        "Password": "senha_app"
    }
}

// Código
var smtpServer = configuration["Email:SmtpServer"];
```

---

## Troubleshooting

### Gmail: "Less secure app access"

Gmail bloqueou acesso. Solução:
- Use senha de aplicativo (recomendado)
- Ou habilite "acesso de apps menos seguros" (não recomendado)

### Timeout ao conectar

```csharp
client.Timeout = 30000; // 30 segundos
```

### SSL/TLS Error

Tente diferentes opções:
```csharp
SecureSocketOptions.StartTls
SecureSocketOptions.SslOnConnect
SecureSocketOptions.Auto
```

---

## Recursos Adicionais

- **GitHub**: https://github.com/jstedfast/MailKit
- **Documentação**: http://www.mimekit.net/docs/

---

**Versão:** 1.0  
**Última atualização:** Novembro 2025
