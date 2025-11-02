# Serilog - Logging Estruturado

## 📋 Índice
1. [Introdução](#introdução)
2. [Instalação](#instalação)
3. [Configuração no Projeto](#configuração-no-projeto)
4. [Uso no Código](#uso-no-código)
5. [Níveis de Log](#níveis-de-log)
6. [Enriquecimento Avançado (Opcional)](#enriquecimento-avançado-opcional)

---

## Introdução

**Serilog** é uma biblioteca de logging estruturado para .NET que permite registrar eventos de forma organizada e pesquisável, separando logs de sucesso e falha automaticamente.

### ✅ Vantagens
- Logging estruturado e pesquisável
- Separação automática de sucesso/falha
- Múltiplos destinos (console, arquivo)
- Performance excelente
- Integração com `Config.cs`

---

## Instalação

### Pacotes Necessários

```bash
# Serilog Core
dotnet add package Serilog

# Sinks (destinos dos logs)
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File

# Enriquecedores básicos (para nome da máquina, etc)
dotnet add package Serilog.Enrichers.Environment
```

**⚠️ Importante:** Execute os comandos na raiz do projeto

---

## Configuração no Projeto

### Passo 1: Configurar AutomationSettings.json

Adicione a seção `Logging` no seu arquivo de configuração:

```json
{
  "Logging": {
    "DiretorioLogs": "logs",
    "NivelMinimo": "Information",
    "ArquivoSucesso": "sucesso/log-{Date}.txt",
    "ArquivoFalha": "falha/log-{Date}.txt"
  }
}
```

**Níveis disponíveis:** `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`

---

### Passo 2: Criar LoggingTask.cs

Crie o arquivo `Workflow/Tasks/LoggingTask.cs`:

```csharp
using Serilog;
using Serilog.Events;

namespace AdrenalineSpy;

/// <summary>
/// Helper centralizado para logging usando Serilog
/// </summary>
public static class LoggingTask
{
    private static bool _configurado = false;

    /// <summary>
    /// Configura o Serilog (chamar UMA VEZ no início)
    /// </summary>
    public static void ConfigurarLogger()
    {
        if (_configurado)
            return;

        var config = Config.Instancia;
        var timestamp = DateTime.Now.ToString("dd-MM-yyyy");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(ParseNivel(config.Logging.NivelMinimo))
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Aplicacao", "AdrenalineSpy")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            // Logs de sucesso
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level < LogEventLevel.Warning)
                .WriteTo.File(
                    path: $"{config.Logging.DiretorioLogs}/{config.Logging.ArquivoSucesso.Replace("{Date}", timestamp)}",
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
            // Logs de falha
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Warning)
                .WriteTo.File(
                    path: $"{config.Logging.DiretorioLogs}/{config.Logging.ArquivoFalha.Replace("{Date}", timestamp)}",
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
            .CreateLogger();

        _configurado = true;
        Log.Information("✅ Logger configurado com sucesso!");
    }

    public static void RegistrarInfo(string mensagem)
    {
        Log.Information(mensagem);
    }

    public static void RegistrarAviso(string mensagem, string contexto)
    {
        Log.Warning("[{Contexto}] {Mensagem}", contexto, mensagem);
    }

    public static void RegistrarErro(Exception ex, string contexto)
    {
        Log.Error(ex, "[{Contexto}] Erro: {Mensagem}", contexto, ex.Message);
    }

    public static void RegistrarDebug(string mensagem)
    {
        Log.Debug(mensagem);
    }

    public static void FecharLogger()
    {
        Log.Information("🔚 Encerrando logger...");
        Log.CloseAndFlush();
    }

    private static LogEventLevel ParseNivel(string nivel)
    {
        return nivel.ToLower() switch
        {
            "verbose" => LogEventLevel.Verbose,
            "debug" => LogEventLevel.Debug,
            "information" => LogEventLevel.Information,
            "warning" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
}
```

---

### Passo 3: Estrutura de Diretórios

Crie as pastas para os logs (ou deixe o Serilog criar automaticamente):

```
logs/
├── .gitkeep
├── sucesso/
│   └── .gitkeep
└── falha/
    └── .gitkeep
```

**⚠️ Importante:** Adicione `/logs/` ao `.gitignore`:

```gitignore
# Logs
logs/*.log
logs/*.txt
logs/sucesso/*.log
logs/sucesso/*.txt
logs/falha/*.log
logs/falha/*.txt
```

---

## Uso no Código

### Em Program.cs

```csharp
using AdrenalineSpy;

namespace AdrenalineSpy
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Carregar configurações
            Config config = Config.Instancia;
            
            if (!config.Validar())
            {
                Console.WriteLine("❌ Configurações inválidas!");
                return;
            }

            // 2. Configurar logger
            LoggingTask.ConfigurarLogger();

            try
            {
                // 3. Usar logging
                LoggingTask.RegistrarInfo("=== Aplicação Iniciada ===");
                
                // Seu código aqui...
                
                LoggingTask.RegistrarInfo("=== Aplicação Finalizada ===");
            }
            catch (Exception ex)
            {
                LoggingTask.RegistrarErro(ex, "Program.Main");
            }
            finally
            {
                // 4. SEMPRE fechar
                LoggingTask.FecharLogger();
            }
        }
    }
}
```

### Em Tasks (NavigationTask, ExtractionTask, etc)

```csharp
namespace AdrenalineSpy;

public class NavigationTask
{
    public void Navegar()
    {
        try
        {
            LoggingTask.RegistrarInfo("Iniciando navegação...");
            
            // Código de navegação...
            
            LoggingTask.RegistrarInfo("Navegação concluída!");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, "NavigationTask.Navegar");
            throw;
        }
    }
}
```

---

## Níveis de Log

### Quando Usar Cada Nível

```csharp
// Verbose - Detalhes técnicos (raramente usado)
LoggingTask.RegistrarDebug("Valor da variável X: 42");

// Debug - Informações de desenvolvimento
LoggingTask.RegistrarDebug("Processando item 5 de 10");

// Information - Fluxo normal da aplicação ✅ MAIS USADO
LoggingTask.RegistrarInfo("Usuário logou no sistema");
LoggingTask.RegistrarInfo("Processamento concluído");

// Warning - Algo estranho mas não é erro
LoggingTask.RegistrarAviso("Tentativa 2 de 3 falhou", "ProcessarItem");

// Error - Erros recuperáveis ✅ IMPORTANTE
try
{
    // código
}
catch (Exception ex)
{
    LoggingTask.RegistrarErro(ex, "NomeDaFuncao");
}

// Fatal - Erros críticos que param tudo
LoggingTask.RegistrarFatal(ex, "Program.Main");
```

### Estrutura dos Logs Gerados

**Console:**
```
[14:30:15 INF] ✅ Logger configurado com sucesso!
[14:30:15 INF] === Aplicação Iniciada ===
[14:30:16 WRN] [ProcessarItem] Tentativa falhou
[14:30:17 ERR] [NavigationTask] Erro: Timeout na navegação
```

**Arquivo `logs/sucesso/log-01-11-2025-14-30.txt`:**
```
2025-11-01 14:30:15 [INF] ✅ Logger configurado com sucesso!
2025-11-01 14:30:15 [INF] === Aplicação Iniciada ===
2025-11-01 14:30:20 [INF] === Aplicação Finalizada ===
```

**Arquivo `logs/falha/log-01-11-2025-14-30.txt`:**
```
2025-11-01 14:30:16 [WRN] [ProcessarItem] Tentativa falhou
2025-11-01 14:30:17 [ERR] [NavigationTask] Erro: Timeout na navegação
System.TimeoutException: A operação expirou...
```

---

## Enriquecimento Avançado (Opcional)

Se você quiser adicionar mais informações aos logs, pode enriquecer o `LoggingTask.cs`:

### Adicionar Thread ID

```csharp
// No ConfigurarLogger(), adicione:
.Enrich.WithThreadId()
```

### Adicionar Propriedades Customizadas

```csharp
.Enrich.WithProperty("Versao", "1.0.0")
.Enrich.WithProperty("Ambiente", "Producao")
```

### LogContext (Propriedades Dinâmicas)

```csharp
using Serilog.Context;

public void ProcessarItem(int itemId)
{
    using (LogContext.PushProperty("ItemId", itemId))
    {
        LoggingTask.RegistrarInfo("Processando item");
        // Todos os logs terão ItemId automaticamente
    }
}
```

### Rolling por Tamanho

Limitar tamanho dos arquivos:

```csharp
.WriteTo.File(
    path: "logs/app.log",
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 30,  // Manter últimos 30 dias
    fileSizeLimitBytes: 10 * 1024 * 1024  // 10 MB por arquivo
)
```

### Filtros Personalizados

```csharp
// Logar apenas erros específicos
.WriteTo.Logger(lc => lc
    .Filter.ByIncludingOnly(evt => 
        evt.Exception != null && 
        evt.Exception.GetType() == typeof(TimeoutException))
    .WriteTo.File("logs/timeouts.log"))
```

### Templates de Output Customizados

```csharp
// Mais detalhado
outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{MachineName}] {Message:lj}{NewLine}{Exception}"

// Com propriedades
outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {Message} {Properties:j}{NewLine}{Exception}"
```

---

## Boas Práticas

### ✅ Fazer

```csharp
// Usar logging estruturado
LoggingTask.RegistrarInfo("Processado pedido ID: {0}", pedidoId);

// Logar no catch
try { }
catch (Exception ex) 
{ 
    LoggingTask.RegistrarErro(ex, "Contexto"); 
}

// Sempre fechar no finally
finally 
{ 
    LoggingTask.FecharLogger(); 
}
```

### ❌ Evitar

```csharp
// String interpolation (perde estrutura)
LoggingTask.RegistrarInfo($"Processado pedido {pedidoId}");

// Logar dados sensíveis
LoggingTask.RegistrarInfo($"Senha: {senha}");  // ❌ NUNCA!

// Esquecer de fechar
// Sem Log.CloseAndFlush() = logs podem se perder
```

---

## Recursos Adicionais

- **Site Oficial:** https://serilog.net/
- **GitHub:** https://github.com/serilog/serilog
- **Sinks Disponíveis:** https://github.com/serilog/serilog/wiki/Provided-Sinks

---

**Versão:** 2.0 (Simplificado)  
**Última atualização:** Novembro 2025
