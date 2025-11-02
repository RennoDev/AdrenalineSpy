# 📧 MailKit - Envio de Emails do AdrenalineSpy

## O que é

**MailKit:** Biblioteca .NET para envio e recebimento de emails via SMTP/IMAP  
**Por que usar:** Notificar resultados do scraping e enviar relatórios automaticamente

**Onde é usado no AdrenalineSpy:**
- EmailTask.cs envia relatórios diários de scraping do Adrenaline.com.br  
- Notificações de erro quando scraping falha
- Envio de estatísticas e resumos de dados extraídos
- Relatórios em Excel/CSV como anexo por email
- Alertas quando novas categorias são detectadas

**Posição no fluxo:** Etapa 12 de 17 - implementar APÓS ferramentas básicas (opcional para notificações)

## Como Instalar

### 1. Instalar Pacote MailKit

```powershell
# Navegar até o projeto
cd C:\Users\lucas\OneDrive\Documentos\CsharpProjects\AdrenalineSpy

# Instalar MailKit
dotnet add package MailKit

# Verificar instalação
dotnet list package | findstr MailKit
```

### 2. Verificar .csproj

Confirme que o pacote foi adicionado:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- MailKit para emails -->
    <PackageReference Include="MailKit" Version="4.3.0" />
    
    <!-- Outros pacotes já existentes -->
    <PackageReference Include="Serilog" Version="4.0.2" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
</Project>
```

## Implementar no AutomationSettings.json

Adicione a seção `Email` no JSON com configurações SMTP:

```json
{
  "Email": {
    "HabilitarEnvio": true,
    "SmtpServidor": "smtp.gmail.com",
    "SmtpPorta": 587,
    "UsuarioEmail": "seu.email@gmail.com", 
    "SenhaEmail": "sua_senha_app_aqui",
    "EmailDestinatario": "relatorios@empresa.com",
    "NomeRemetente": "AdrenalineSpy RPA",
    "EnviarRelatorioDiario": true,
    "EnviarNotificacaoErro": true
  }
}
```

**Explicação das configurações:**

- **`HabilitarEnvio`**: Liga/desliga envio de emails (true/false)
- **`SmtpServidor`**: Servidor SMTP (Gmail: smtp.gmail.com, Outlook: smtp-mail.outlook.com)
- **`SmtpPorta`**: Porta SMTP (587 para TLS, 465 para SSL, 25 para local)
- **`UsuarioEmail`**: Email de origem (precisa ter permissão SMTP)
- **`SenhaEmail`**: Senha do email ou App Password (Gmail)
- **`EmailDestinatario`**: Para quem enviar os relatórios
- **`NomeRemetente`**: Nome que aparece como remetente
- **`EnviarRelatorioDiario`**: Enviar relatório diário automaticamente
- **`EnviarNotificacaoErro`**: Notificar por email quando há erros

**⚠️ Gmail:** Use App Password, não a senha normal:
1. Conta Google → Segurança → Verificação em 2 etapas → Senhas de app
2. Gere senha específica para "App de email"

## Implementar no Config.cs

Adicione a classe `EmailConfig` ao `Config.cs`:

```csharp
public class EmailConfig
{
    public bool HabilitarEnvio { get; set; } = false;
    public string SmtpServidor { get; set; } = "smtp.gmail.com";
    public int SmtpPorta { get; set; } = 587;
    public string UsuarioEmail { get; set; } = string.Empty;
    public string SenhaEmail { get; set; } = string.Empty;
    public string EmailDestinatario { get; set; } = string.Empty;
    public string NomeRemetente { get; set; } = "AdrenalineSpy RPA";
    public bool EnviarRelatorioDiario { get; set; } = true;
    public bool EnviarNotificacaoErro { get; set; } = true;
}
```

**No Config.cs principal, adicione a propriedade:**

```csharp
public class Config
{
    // ... outras propriedades existentes ...
    public EmailConfig Email { get; set; } = new();

    // ... métodos existentes ...
    
    /// <summary>
    /// Valida se as configurações de email estão corretas
    /// </summary>
    public bool ValidarEmail()
    {
        if (!Email.HabilitarEnvio) return true; // Se desabilitado, não validar
        
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(Email.SmtpServidor))
            erros.Add("Email.SmtpServidor não pode estar vazio");

        if (Email.SmtpPorta <= 0)
            erros.Add("Email.SmtpPorta deve ser maior que zero");

        if (string.IsNullOrWhiteSpace(Email.UsuarioEmail))
            erros.Add("Email.UsuarioEmail não pode estar vazio");

        if (string.IsNullOrWhiteSpace(Email.SenhaEmail))
            erros.Add("Email.SenhaEmail não pode estar vazio");

        if (string.IsNullOrWhiteSpace(Email.EmailDestinatario))
            erros.Add("Email.EmailDestinatario não pode estar vazio");

        if (erros.Any())
        {
            LoggingTask.RegistrarAviso("Erros de validação de email:", string.Join(", ", erros));
            return false;
        }

        LoggingTask.RegistrarInfo("✅ Configurações de email validadas");
        return true;
    }
}
```

## Montar nas Tasks

Crie a classe `EmailTask.cs` na pasta `Workflow/Tasks/`:

```csharp
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AdrenalineSpy.Workflow.Tasks;

/// <summary>
/// Gerencia envio de emails para notificações e relatórios do AdrenalineSpy
/// </summary>
public static class EmailTask
{
    /// <summary>
    /// Envia relatório diário com estatísticas de scraping
    /// </summary>
    public static async Task EnviarRelatorioDiario(int totalNoticias, int noticiasTech, int noticiasGaming)
    {
        try
        {
            if (!Config.Instancia.Email.HabilitarEnvio || !Config.Instancia.Email.EnviarRelatorioDiario)
            {
                LoggingTask.RegistrarInfo("📧 Relatório diário desabilitado nas configurações");
                return;
            }

            var assunto = $"[AdrenalineSpy] Relatório Diário - {DateTime.Now:dd/MM/yyyy}";
            var corpo = $@"
🚀 Relatório de Scraping - Adrenaline.com.br

📊 Estatísticas do Dia:
• Total de notícias coletadas: {totalNoticias}
• Tecnologia: {noticiasTech}
• Gaming: {noticiasGaming}

🕐 Executado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}

---
AdrenalineSpy RPA System
";

            await EnviarEmail(assunto, corpo);
            LoggingTask.RegistrarInfo($"✅ Relatório diário enviado: {totalNoticias} notícias processadas");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Falha ao enviar relatório diário", ex);
        }
    }

    /// <summary>
    /// Envia notificação de erro crítico
    /// </summary>
    public static async Task EnviarNotificacaoErro(string operacao, Exception erro)
    {
        try
        {
            if (!Config.Instancia.Email.HabilitarEnvio || !Config.Instancia.Email.EnviarNotificacaoErro)
                return;

            var assunto = $"[AdrenalineSpy] ERRO: {operacao}";
            var corpo = $@"
⚠️ Erro Detectado no AdrenalineSpy

🎯 Operação: {operacao}
🕐 Horário: {DateTime.Now:dd/MM/yyyy HH:mm:ss}

💥 Detalhes do Erro:
{erro.Message}

📚 Stack Trace:
{erro.StackTrace}

---
Verifique os logs para mais detalhes.
";

            await EnviarEmail(assunto, corpo);
            LoggingTask.RegistrarInfo($"📧 Notificação de erro enviada para: {operacao}");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Falha ao enviar notificação de erro por email", ex);
        }
    }

    /// <summary>
    /// Testa configuração de email enviando mensagem de teste
    /// </summary>
    public static async Task<bool> TestarConfiguracao()
    {
        try
        {
            if (!Config.Instancia.ValidarEmail())
                return false;

            var assunto = "[AdrenalineSpy] Teste de Configuração";
            var corpo = $@"
✅ Teste de Configuração de Email

🎯 Este é um email de teste para validar as configurações do MailKit no AdrenalineSpy.

📧 Configurações utilizadas:
• Servidor SMTP: {Config.Instancia.Email.SmtpServidor}
• Porta: {Config.Instancia.Email.SmtpPorta}
• Remetente: {Config.Instancia.Email.NomeRemetente}

🕐 Enviado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}

Se você recebeu este email, as configurações estão funcionando corretamente! 🚀
";

            await EnviarEmail(assunto, corpo);
            LoggingTask.RegistrarInfo("✅ Teste de email enviado com sucesso");
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Falha no teste de configuração de email", ex);
            return false;
        }
    }

    /// <summary>
    /// Método interno para envio de emails
    /// </summary>
    private static async Task EnviarEmail(string assunto, string corpo)
    {
        var config = Config.Instancia.Email;

        // Criar mensagem
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(config.NomeRemetente, config.UsuarioEmail));
        message.To.Add(MailboxAddress.Parse(config.EmailDestinatario));
        message.Subject = assunto;

        // Corpo do email
        var builder = new BodyBuilder
        {
            TextBody = corpo
        };
        message.Body = builder.ToMessageBody();

        // Enviar via SMTP
        using var client = new SmtpClient();
        
        await client.ConnectAsync(config.SmtpServidor, config.SmtpPorta, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(config.UsuarioEmail, config.SenhaEmail);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
```
---

## Como Adicionar no Program.cs

### Quando Usar Email no AdrenalineSpy

O **MailKit** é usado para **notificações automáticas** do sistema RPA:
- 📊 **Relatórios automáticos** - Enviar estatísticas diárias/semanais por email  
- ⚠️ **Alertas de erro** - Notificar quando o scraping falha
- ✅ **Confirmação de execução** - Confirmar que o workflow foi executado com sucesso
- 📁 **Anexar relatórios** - Enviar arquivos Excel/PDF/CSV gerados

### Program.cs - Fase: Notificações de Erro
```csharp
static async Task Main(string[] args)
{
    Config config = Config.Instancia;
    LoggingTask.ConfigurarLogger();
    
    try
    {
        LoggingTask.RegistrarInfo("=== AdrenalineSpy Iniciado ===");
        
        // Workflow normal (coleta + banco + exportação)
        await ExecutarWorkflowCompleto(config);
        
        LoggingTask.RegistrarInfo("=== Execução finalizada com sucesso ===");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Program.Main - Erro Fatal");
        
        // ADICIONADO: Notificar erro por email
        await NotificarErroPorEmail(ex, config);
        
        Console.WriteLine($"❌ Erro fatal: {ex.Message}");
    }
    finally
    {
        LoggingTask.FecharLogger();
    }
}

private static async Task NotificarErroPorEmail(Exception ex, Config config)
{
    try
    {
        if (config.Email.NotificarErros)
        {
            LoggingTask.RegistrarInfo("Enviando notificação de erro por email...");
            
            var emailTask = new EmailTask();
            await emailTask.EnviarNotificacaoErroAsync(ex);
            
            LoggingTask.RegistrarInfo("✅ Notificação de erro enviada");
        }
    }
    catch (Exception emailEx)
    {
        LoggingTask.RegistrarErro(emailEx, "Falha ao enviar email de erro");
        // Não propagar erro de email - o erro original é mais importante
    }
}
```

### Program.cs - Fase: Relatórios Automáticos
```csharp
static async Task Main(string[] args)
{
    Config config = Config.Instancia;
    LoggingTask.ConfigurarLogger();
    
    try
    {
        LoggingTask.RegistrarInfo("=== AdrenalineSpy Iniciado ===");
        
        // Validar se deve enviar relatório
        bool enviarRelatorio = args.Contains("--relatorio-email") || 
                              config.Email.RelatoriosAutomaticos;
        
        // Workflow principal
        var resultados = await ExecutarWorkflowComMetricas(config);
        
        // ADICIONADO: Enviar relatório automático
        if (enviarRelatorio)
        {
            await EnviarRelatorioAutomatico(resultados, config);
        }
        
        LoggingTask.RegistrarInfo("=== Execução finalizada ===");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Program.Main");
        await NotificarErroPorEmail(ex, config);
    }
    finally
    {
        LoggingTask.FecharLogger();
    }
}

private static async Task<ResultadosExecucao> ExecutarWorkflowComMetricas(Config config)
{
    var inicio = DateTime.Now;
    var resultados = new ResultadosExecucao { InicioExecucao = inicio };
    
    try
    {
        // Executar workflow normal
        var migrationTask = new MigrationTask();
        var navigationTask = new NavigationTask();
        
        foreach (var categoria in config.Categorias.Keys)
        {
            var urls = await navigationTask.ColetarUrlsCategoriaAsync(categoria);
            // ... processar categoria ...
            
            resultados.CategoriasProcessadas.Add(categoria, urls.Count);
        }
        
        resultados.Status = "Sucesso";
    }
    catch (Exception ex)
    {
        resultados.Status = "Erro";
        resultados.ErroDetalhes = ex.Message;
        throw;
    }
    finally
    {
        resultados.FimExecucao = DateTime.Now;
        resultados.DuracaoTotal = resultados.FimExecucao - resultados.InicioExecucao;
    }
    
    return resultados;
}

private static async Task EnviarRelatorioAutomatico(ResultadosExecucao resultados, Config config)
{
    try
    {
        LoggingTask.RegistrarInfo("Gerando e enviando relatório automático...");
        
        var emailTask = new EmailTask();
        
        // Gerar arquivos de relatório se necessário
        List<string> anexos = await GerarAnexosRelatorio(config, resultados);
        
        // Enviar email com relatório
        await emailTask.EnviarRelatorioExecucaoAsync(resultados, anexos);
        
        LoggingTask.RegistrarInfo($"✅ Relatório enviado para {config.Email.DestinatariosRelatorio.Count} destinatários");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Erro ao enviar relatório automático");
    }
}

private static async Task<List<string>> GerarAnexosRelatorio(Config config, ResultadosExecucao resultados)
{
    var anexos = new List<string>();
    
    if (config.Email.AnexarExcel)
    {
        var exportTask = new ExportTask();
        string arquivoExcel = await exportTask.GerarRelatorioExcelAsync();
        anexos.Add(arquivoExcel);
    }
    
    if (config.Email.AnexarLogs && File.Exists("logs/sucesso.log"))
    {
        anexos.Add("logs/sucesso.log");
    }
    
    return anexos;
}
```

### Program.cs - Fase: Notificações Inteligentes
```csharp
static async Task Main(string[] args)
{
    Config config = Config.Instancia;
    LoggingTask.ConfigurarLogger();
    
    try
    {
        LoggingTask.RegistrarInfo("=== AdrenalineSpy Iniciado ===");
        
        // ADICIONADO: Verificar se deve notificar início
        if (config.Email.NotificarInicio && !args.Contains("--silencioso"))
        {
            await NotificarInicioExecucao(config);
        }
        
        var resultados = await ExecutarWorkflowComMetricas(config);
        
        // ADICIONADO: Decisões inteligentes de notificação
        await ProcessarNotificacoes(resultados, config, args);
        
        LoggingTask.RegistrarInfo("=== Execução finalizada ===");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Program.Main");
        await NotificarErroPorEmail(ex, config);
    }
    finally
    {
        LoggingTask.FecharLogger();
    }
}

private static async Task NotificarInicioExecucao(Config config)
{
    try
    {
        LoggingTask.RegistrarInfo("Enviando notificação de início...");
        
        var emailTask = new EmailTask();
        await emailTask.EnviarNotificacaoInicioAsync();
        
        LoggingTask.RegistrarInfo("✅ Notificação de início enviada");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Erro ao enviar notificação de início");
        // Não bloquear execução por falha de email
    }
}

private static async Task ProcessarNotificacoes(ResultadosExecucao resultados, Config config, string[] args)
{
    var emailTask = new EmailTask();
    
    // Notificar sucesso (se configurado)
    if (resultados.Status == "Sucesso" && config.Email.NotificarSucesso)
    {
        LoggingTask.RegistrarInfo("Enviando notificação de sucesso...");
        await emailTask.EnviarNotificacaoSucessoAsync(resultados);
    }
    
    // Relatório detalhado (se solicitado)
    if (args.Contains("--relatorio-detalhado"))
    {
        LoggingTask.RegistrarInfo("Enviando relatório detalhado...");
        var anexos = await GerarAnexosRelatorio(config, resultados);
        await emailTask.EnviarRelatorioDetalhadoAsync(resultados, anexos);
    }
    
    // Verificar se deve notificar administradores sobre métricas
    if (resultados.DuracaoTotal > TimeSpan.FromMinutes(config.Email.LimiteTempoNotificacao))
    {
        LoggingTask.RegistrarAviso($"Execução demorou {resultados.DuracaoTotal:mm\\:ss} - notificando administradores");
        await emailTask.NotificarExecucaoLentaAsync(resultados);
    }
}
```

### Program.cs - Fase: Agendamento com Notificações
```csharp
static async Task Main(string[] args)
{
    Config config = Config.Instancia;
    LoggingTask.ConfigurarLogger();
    
    try
    {
        LoggingTask.RegistrarInfo("=== AdrenalineSpy Iniciado ===");
        
        // Verificar modo de execução
        if (args.Contains("--scheduler") || args.Contains("--agendado"))
        {
            LoggingTask.RegistrarInfo("Modo agendado detectado - configurando notificações especiais");
            
            // ADICIONADO: Notificações específicas para execução agendada
            await ExecutarModoAgendado(config);
        }
        else
        {
            // Execução manual normal
            await ExecutarModoManual(config, args);
        }
        
        LoggingTask.RegistrarInfo("=== Execução finalizada ===");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Program.Main");
        
        // Notificação de erro diferenciada para modo agendado
        bool modoAgendado = Environment.GetCommandLineArgs().Contains("--scheduler");
        await NotificarErro(ex, config, modoAgendado);
    }
    finally
    {
        LoggingTask.FecharLogger();
    }
}

private static async Task ExecutarModoAgendado(Config config)
{
    var emailTask = new EmailTask();
    
    try
    {
        // Execução silenciosa, notificar apenas se houver problemas ou conforme configurado
        var resultados = await ExecutarWorkflowComMetricas(config);
        
        // Notificar apenas se configurado para execução agendada
        if (config.Email.NotificarExecucoesAgendadas)
        {
            await emailTask.EnviarRelatorioAgendadoAsync(resultados);
        }
        
        // Sempre notificar se houve problemas
        if (resultados.CategoriasComErro.Any())
        {
            await emailTask.NotificarProblemasExecucaoAsync(resultados);
        }
    }
    catch (Exception ex)
    {
        await emailTask.EnviarNotificacaoErroAgendadoAsync(ex);
        throw;
    }
}

private static async Task ExecutarModoManual(Config config, string[] args)
{
    // Execução manual - mais verbosa, notificações opcionais
    var resultados = await ExecutarWorkflowComMetricas(config);
    await ProcessarNotificacoes(resultados, config, args);
}

private static async Task NotificarErro(Exception ex, Config config, bool modoAgendado)
{
    var emailTask = new EmailTask();
    
    if (modoAgendado)
    {
        // Erro em execução agendada - sempre notificar
        await emailTask.EnviarNotificacaoErroAgendadoAsync(ex);
    }
    else if (config.Email.NotificarErros)
    {
        // Execução manual - notificar conforme configuração
        await emailTask.EnviarNotificacaoErroAsync(ex);
    }
}
```

### Exemplos de Uso da Linha de Comando

```bash
# Execução normal (sem email)
dotnet run

# Com relatório por email
dotnet run -- --relatorio-email

# Relatório detalhado com anexos
dotnet run -- --relatorio-detalhado

# Modo silencioso (sem notificações de início)
dotnet run -- --silencioso

# Modo agendado (notificações especiais)
dotnet run -- --scheduler

# Forçar notificação mesmo em caso de sucesso
dotnet run -- --notificar-sempre
```

### Classe de Modelo para Resultados
```csharp
public class ResultadosExecucao
{
    public DateTime InicioExecucao { get; set; }
    public DateTime FimExecucao { get; set; }
    public TimeSpan DuracaoTotal { get; set; }
    public string Status { get; set; } = "Sucesso";
    public string ErroDetalhes { get; set; }
    
    public Dictionary<string, int> CategoriasProcessadas { get; set; } = new();
    public List<string> CategoriasComErro { get; set; } = new();
    
    public int TotalUrlsColetadas => CategoriasProcessadas.Values.Sum();
    public int TotalNoticiasExtraidas { get; set; }
    public int TotalNoticiasNovas { get; set; }
}
```

### ⚠️ Configurações Importantes no AutomationSettings.json

Certifique-se de configurar adequadamente:
```json
{
  "Email": {
    "NotificarErros": true,
    "NotificarSucesso": false,
    "NotificarInicio": false,
    "RelatoriosAutomaticos": true,
    "NotificarExecucoesAgendadas": true,
    "LimiteTempoNotificacao": 30,
    "AnexarExcel": true,
    "AnexarLogs": false,
    "DestinatariosRelatorio": ["gestor@empresa.com"],
    "DestinatariosErro": ["dev@empresa.com", "ops@empresa.com"]
  }
}
```

### 💡 Boas Práticas de Integração

1. **Sempre envolver em try/catch** - Falhas de email não devem quebrar o workflow
2. **Configurar limites** - Evitar spam de notificações
3. **Diferenciar tipos de execução** - Manual vs Agendada vs Teste
4. **Anexar logs seletivamente** - Apenas quando necessário
5. **Validar configurações** - Testar SMTP antes de usar em produção

### 🔄 Próxima Evolução

Após implementar notificações por email, o próximo passo é **Quartz.NET** para agendamento automático, criando um ciclo completo de automação com notificações.

---

## Métodos Mais Usados

### Conectar e Autenticar SMTP

```csharp
using var client = new SmtpClient();

// Conectar ao servidor
await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

// Autenticar com credenciais
await client.AuthenticateAsync("seu@email.com", "sua_app_password");

// Usar nas operações...

// Desconectar
await client.DisconnectAsync(true);
```

### Criar Mensagem Básica

```csharp
var message = new MimeMessage();

// Remetente
message.From.Add(new MailboxAddress("AdrenalineSpy", "noreply@adrenalinespy.com"));

// Destinatário
message.To.Add(MailboxAddress.Parse("admin@exemplo.com"));

// Assunto
message.Subject = "[AdrenalineSpy] Relatório de Scraping";

// Corpo
var builder = new BodyBuilder
{
    TextBody = "Relatório de scraping do Adrenaline.com.br executado com sucesso!"
};
message.Body = builder.ToMessageBody();
```

### Enviar Email com Retry

```csharp
public static async Task<bool> EnviarComRetry(MimeMessage message, int maxTentativas = 3)
{
    for (int tentativa = 1; tentativa <= maxTentativas; tentativa++)
    {
        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(Config.Instancia.Email.SmtpServidor, 
                                    Config.Instancia.Email.SmtpPorta, 
                                    SecureSocketOptions.StartTls);
            
            await client.AuthenticateAsync(Config.Instancia.Email.UsuarioEmail, 
                                         Config.Instancia.Email.SenhaEmail);
            
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            
            LoggingTask.RegistrarInfo($"✅ Email enviado na tentativa {tentativa}");
            return true;
        }
        catch (Exception ex) when (tentativa < maxTentativas)
        {
            LoggingTask.RegistrarAviso($"⚠️ Falha na tentativa {tentativa}: {ex.Message}");
            await Task.Delay(TimeSpan.FromSeconds(tentativa * 2)); // Backoff progressivo
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro($"❌ Falha definitiva após {maxTentativas} tentativas", ex);
            return false;
        }
    }
    return false;
}
```

### Validar Configuração

```csharp
public static async Task<bool> TestarConexaoSmtp()
{
    try
    {
        if (!Config.Instancia.ValidarEmail())
        {
            LoggingTask.RegistrarAviso("❌ Configuração de email inválida");
            return false;
        }

        using var client = new SmtpClient();
        
        // Timeout de 10 segundos para teste
        client.Timeout = 10000;
        
        await client.ConnectAsync(Config.Instancia.Email.SmtpServidor, 
                                Config.Instancia.Email.SmtpPorta, 
                                SecureSocketOptions.StartTls);
        
        await client.AuthenticateAsync(Config.Instancia.Email.UsuarioEmail, 
                                     Config.Instancia.Email.SenhaEmail);
        
        await client.DisconnectAsync(true);
        
        LoggingTask.RegistrarInfo("✅ Conexão SMTP validada com sucesso");
        return true;
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro("❌ Falha na validação SMTP", ex);
        return false;
    }
}
```

### Integração com Workflow

```csharp
// No Workflow.cs - após completar scraping
try
{
    var totalNoticias = await MigrationTask.ContarNoticias(DateTime.Today);
    var noticiasTech = await MigrationTask.ContarNoticiasPorCategoria("Tecnologia");
    var noticiasGaming = await MigrationTask.ContarNoticiasPorCategoria("Gaming");
    
    // Enviar relatório automático
    await EmailTask.EnviarRelatorioDiario(totalNoticias, noticiasTech, noticiasGaming);
}
catch (Exception ex)
{
    // Notificar erro por email também
    await EmailTask.EnviarNotificacaoErro("Geração de Relatório", ex);
    LoggingTask.RegistrarErro("Falha ao gerar relatório por email", ex);
}



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
