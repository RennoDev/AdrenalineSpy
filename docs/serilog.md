# Serilog - Logging Estruturado

## Índice
1. [Introdução](#introdução)
2. [Instalação](#instalação)
3. [Configuração Básica](#configuração-básica)
4. [Níveis de Log](#níveis-de-log)
5. [Sinks (Destinos)](#sinks-destinos)
6. [Enriquecedores](#enriquecedores)
7. [Exemplos Práticos](#exemplos-práticos)

---

## Introdução

**Serilog** é uma biblioteca de logging estruturado para .NET, permitindo registrar eventos de forma rica e pesquisável.

### Vantagens
- ✅ Logging estruturado
- ✅ Múltiplos destinos (console, arquivo, BD, etc)
- ✅ Formatação flexível
- ✅ Performance excelente
- ✅ Fácil configuração

---

## Instalação

```bash
# Core
dotnet add package Serilog

# Sinks
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File

# Opcional: Configuração por appsettings.json
dotnet add package Serilog.Settings.Configuration
```

---

## Configuração Básica

### Setup Simples

```csharp
using Serilog;

class Program
{
    static void Main()
    {
        // Configurar Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/app.log")
            .CreateLogger();
        
        // Usar
        Log.Information("Aplicação iniciada");
        Log.Warning("Este é um aviso");
        Log.Error("Ocorreu um erro");
        
        // Importante: Fechar no final
        Log.CloseAndFlush();
    }
}
```

### Configuração Completa

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() // Nível mínimo
    .Enrich.FromLogContext() // Enriquecimento
    .Enrich.WithMachineName() // Nome da máquina
    .Enrich.WithThreadId() // ID da thread
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();
```

### Logs Organizados por Sucesso/Falha

Para projetos RPA, é útil separar logs de execuções bem-sucedidas de falhas:

```csharp
using Serilog;
using Serilog.Events;

var timestamp = DateTime.Now.ToString("dd-MM-yyyy-HH:mm");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    // Logs de sucesso (Info, Debug, Verbose)
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Level < LogEventLevel.Warning)
        .WriteTo.File($"logs/sucesso/{timestamp}.log"))
    // Logs de falha (Warning, Error, Fatal)
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Warning)
        .WriteTo.File($"logs/falha/{timestamp}.log"))
    .CreateLogger();
```

**Estrutura gerada:**
```
logs/
├── sucesso/
│   ├── 01-11-2025-14:30.log
│   └── 01-11-2025-16:00.log
└── falha/
    └── 01-11-2025-15:45.log
```

**⚠️ Importante:** Adicione `/logs/` ao `.gitignore`

---

## Níveis de Log

### Níveis Disponíveis (do menos ao mais grave)

```csharp
// Verbose - Informações muito detalhadas (debug)
Log.Verbose("Detalhes técnicos: valor = {Valor}", 42);

// Debug - Informações de depuração
Log.Debug("Variável X = {X}, Y = {Y}", x, y);

// Information - Informações gerais do fluxo
Log.Information("Usuário {Usuario} logou no sistema", "João");

// Warning - Avisos que não impedem execução
Log.Warning("Tentativa {Tentativa} de 3 falhou", 1);

// Error - Erros que podem ser recuperados
Log.Error("Erro ao processar item {Id}: {Erro}", itemId, erro);

// Fatal - Erros críticos que param a aplicação
Log.Fatal("Falha crítica no banco de dados");
```

### Com Exceção

```csharp
try
{
    // código
}
catch (Exception ex)
{
    Log.Error(ex, "Erro ao processar pedido {PedidoId}", pedidoId);
    // ou
    Log.Fatal(ex, "Erro fatal irrecuperável");
}
```

### Propriedades Estruturadas

```csharp
// ✅ BOM - Logging estruturado (pesquisável)
Log.Information("Processado pedido {PedidoId} para cliente {Cliente} no valor de {Valor}", 
    123, "João Silva", 500.00m);

// ❌ RUIM - String interpolation (não estruturado)
Log.Information($"Processado pedido {pedidoId} para cliente {cliente}");
```

---

## Sinks (Destinos)

### Console

```csharp
.WriteTo.Console(
    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
```

### Arquivo

```csharp
// Arquivo simples
.WriteTo.File("logs/app.log")

// Com rolling (novo arquivo por dia)
.WriteTo.File(
    path: "logs/app-.log",
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 30, // Manter últimos 30 dias
    fileSizeLimitBytes: 10 * 1024 * 1024 // 10 MB
)

// Arquivo JSON
.WriteTo.File(
    new JsonFormatter(),
    "logs/app.json"
)
```

### Múltiplos Destinos

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log")
    .WriteTo.File("logs/errors.log", restrictedToMinimumLevel: LogEventLevel.Error)
    .CreateLogger();
```

### Sink Condicional

```csharp
.WriteTo.Logger(lc => lc
    .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Error)
    .WriteTo.File("logs/errors.log")
)
```

---

## Enriquecedores

### Enriquecedores Internos

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()      // Contexto do log
    .Enrich.WithMachineName()     // Nome da máquina
    .Enrich.WithThreadId()        // ID da thread
    .Enrich.WithEnvironmentUserName() // Usuário do SO
    .Enrich.WithProperty("Application", "MeuApp") // Propriedade customizada
    .Enrich.WithProperty("Version", "1.0.0")
    .CreateLogger();
```

### LogContext (Propriedades dinâmicas)

```csharp
using Serilog.Context;

using (LogContext.PushProperty("UsuarioId", userId))
using (LogContext.PushProperty("Sessao", sessionId))
{
    Log.Information("Usuário acessou página");
    // Logs dentro deste bloco terão UsuarioId e Sessao
}
```

---

## Exemplos Práticos

### Exemplo 1: Configuração para RPA

```csharp
using Serilog;
using Serilog.Events;

class Program
{
    static void Main()
    {
        // Configurar log
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Aplicacao", "RPA_AdrenalineSpy")
            .Enrich.WithMachineName()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/rpa-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/errors-.log",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Error)
            .CreateLogger();
        
        try
        {
            Log.Information("=== RPA Iniciado ===");
            
            // Sua automação aqui
            ExecutarAutomacao();
            
            Log.Information("=== RPA Finalizado com Sucesso ===");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "RPA falhou");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
    
    static void ExecutarAutomacao()
    {
        Log.Information("Iniciando processamento");
        
        for (int i = 1; i <= 10; i++)
        {
            using (LogContext.PushProperty("ItemId", i))
            {
                try
                {
                    Log.Debug("Processando item {Item}", i);
                    
                    // Processar...
                    Thread.Sleep(100);
                    
                    Log.Information("Item {Item} processado com sucesso", i);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Erro ao processar item {Item}", i);
                }
            }
        }
    }
}
```

### Exemplo 2: Classe LoggerHelper

```csharp
public static class LoggerHelper
{
    public static void ConfigurarLogger(string nomeApp)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.WithProperty("Aplicacao", nomeApp)
            .Enrich.WithMachineName()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File($"logs/{nomeApp}-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        
        Log.Information($"{nomeApp} - Logger configurado");
    }
    
    public static void LogInicio(string processoNome)
    {
        Log.Information("========================================");
        Log.Information("Processo: {Processo} - INICIADO", processoNome);
        Log.Information("Hora: {Hora}", DateTime.Now);
        Log.Information("========================================");
    }
    
    public static void LogFim(string processoNome, int sucesso, int erros)
    {
        Log.Information("========================================");
        Log.Information("Processo: {Processo} - FINALIZADO", processoNome);
        Log.Information("Sucesso: {Sucesso} | Erros: {Erros}", sucesso, erros);
        Log.Information("Hora: {Hora}", DateTime.Now);
        Log.Information("========================================");
    }
    
    public static void LogExcecao(Exception ex, string contexto)
    {
        Log.Error(ex, "EXCEÇÃO em {Contexto}", contexto);
        Log.Error("Mensagem: {Mensagem}", ex.Message);
        Log.Error("StackTrace: {StackTrace}", ex.StackTrace);
    }
}

// Uso
LoggerHelper.ConfigurarLogger("MeuRPA");
LoggerHelper.LogInicio("ProcessarPedidos");

// ... código ...

LoggerHelper.LogFim("ProcessarPedidos", 95, 5);
Log.CloseAndFlush();
```

### Exemplo 3: Integração com Try-Catch

```csharp
public class ProcessadorPedidos
{
    public void ProcessarTodos(List<Pedido> pedidos)
    {
        Log.Information("Iniciando processamento de {Quantidade} pedidos", pedidos.Count);
        
        int sucesso = 0;
        int erros = 0;
        
        foreach (var pedido in pedidos)
        {
            using (LogContext.PushProperty("PedidoId", pedido.Id))
            {
                try
                {
                    Log.Debug("Processando pedido {PedidoId}", pedido.Id);
                    
                    ProcessarPedido(pedido);
                    
                    sucesso++;
                    Log.Information("Pedido {PedidoId} processado com sucesso", pedido.Id);
                }
                catch (Exception ex)
                {
                    erros++;
                    Log.Error(ex, "Erro ao processar pedido {PedidoId}", pedido.Id);
                }
            }
        }
        
        Log.Information("Processamento concluído: {Sucesso} sucessos, {Erros} erros", sucesso, erros);
    }
    
    private void ProcessarPedido(Pedido pedido)
    {
        Log.Debug("Validando pedido");
        ValidarPedido(pedido);
        
        Log.Debug("Salvando no banco");
        SalvarPedido(pedido);
        
        Log.Debug("Enviando email");
        EnviarEmailConfirmacao(pedido);
    }
}
```

### Exemplo 4: Configuração por appsettings.json

```json
// appsettings.json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ],
    "Properties": {
      "Application": "MeuRPA"
    }
  }
}
```

```csharp
// Program.cs
using Microsoft.Extensions.Configuration;
using Serilog;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Aplicação iniciada");
    // ... código ...
}
finally
{
    Log.CloseAndFlush();
}
```

---

## Templates de Output

### Templates Úteis

```csharp
// Simples
"[{Timestamp:HH:mm:ss} {Level}] {Message}{NewLine}{Exception}"

// Detalhado
"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"

// Com cores (Console)
"[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"

// Para arquivo
"{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}"
```

---

## Boas Práticas

### 1. Sempre Feche o Logger

```csharp
try
{
    // código
}
finally
{
    Log.CloseAndFlush();
}
```

### 2. Use Logging Estruturado

```csharp
// ✅ BOM
Log.Information("Pedido {PedidoId} processado em {Tempo}ms", id, tempo);

// ❌ RUIM
Log.Information($"Pedido {id} processado em {tempo}ms");
```

### 3. Níveis Apropriados

- **Verbose/Debug**: Desenvolvimento
- **Information**: Fluxo normal
- **Warning**: Algo estranho mas não é erro
- **Error**: Erros recuperáveis
- **Fatal**: Erros críticos

### 4. LogContext para Contexto

```csharp
using (LogContext.PushProperty("CorrelationId", correlationId))
{
    // Todos os logs terão CorrelationId
}
```

### 5. Não Logue Dados Sensíveis

```csharp
// ❌ RUIM - expõe senha
Log.Information("Login: {Usuario} com senha {Senha}", user, password);

// ✅ BOM
Log.Information("Login bem-sucedido para {Usuario}", user);
```

---

## Recursos Adicionais

- **Site Oficial**: https://serilog.net/
- **GitHub**: https://github.com/serilog/serilog
- **Sinks**: https://github.com/serilog/serilog/wiki/Provided-Sinks

---

**Versão:** 1.0  
**Última atualização:** Novembro 2025
