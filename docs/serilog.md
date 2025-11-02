# 📝 Serilog - Sistema de Logging do AdrenalineSpy

## O que é

**Serilog:** Biblioteca de logging estruturado para .NET que registra eventos da aplicação  
**Por que usar:** RPA sem logs é impossível de debugar quando algo dá errado  

**Onde é usado no AdrenalineSpy:**
- LoggingTask.cs centraliza todos os logs do projeto
- Separação automática: logs de sucesso vs. logs de falha
- Tracking completo das operações de scraping do Adrenaline.com.br
- Debug de problemas em NavigationTask, ExtractionTask e MigrationTask
- Logs estruturados com contexto (URL, timestamp, dados extraídos)

**Posição no fluxo:** Etapa 5 de 17 - implementar ANTES das Tasks porque todas usarão logging

## Como Instalar

### 1. Instalar Pacotes NuGet

```powershell
# Navegar até o projeto
cd C:\Users\lucas\OneDrive\Documentos\CsharpProjects\AdrenalineSpy

# Pacotes essenciais do Serilog
dotnet add package Serilog
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Settings.Configuration
dotnet add package Serilog.Expressions

# Verificar instalação
dotnet list package | findstr Serilog
```

### 2. Verificar .csproj

Confirme que os pacotes foram adicionados ao `AdrenalineSpy.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Serilog" Version="4.0.2" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
    <PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
    <PackageReference Include="Serilog.Settings.Configuration" Version="8.0.4" />
    <PackageReference Include="Serilog.Expressions" Version="5.0.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>

</Project>
```

## Implementar no AutomationSettings.json

A seção `Logging` no JSON já está configurada corretamente:

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

**Explicação das configurações:**

- **`DiretorioLogs`**: Pasta onde serão salvos os arquivos de log
- **`NivelMinimo`**: Nível mínimo de log a ser registrado
  - `Verbose` → `Debug` → `Information` → `Warning` → `Error` → `Fatal`
- **`ArquivoSucesso`**: Template para logs de operações bem-sucedidas
- **`ArquivoFalha`**: Template para logs de erros e exceções
- **`{Date}`**: Placeholder que será substituído pela data atual (YYYY-MM-DD)

**Estrutura de pastas resultante:**
```
logs/
├── sucesso/
│   ├── log-2024-11-01.txt
│   ├── log-2024-11-02.txt
│   └── ...
└── falha/
    ├── log-2024-11-01.txt
    ├── log-2024-11-02.txt
    └── ...
```

## Implementar no Config.cs

A classe `LoggingConfig` já está implementada no `Config.cs`:

```csharp
public class LoggingConfig
{
    public string DiretorioLogs { get; set; } = "logs";
    public string NivelMinimo { get; set; } = "Information";
    public string ArquivoSucesso { get; set; } = "sucesso/log-{Date}.txt";
    public string ArquivoFalha { get; set; } = "falha/log-{Date}.txt";
}
```

A configuração é acessada através do singleton:
```csharp
var config = Config.Instancia;
var loggingConfig = config.Logging;
```

## Montar nas Tasks

### 1. Criar LoggingTask.cs

Crie o arquivo `Workflow/Tasks/LoggingTask.cs`:

```csharp
using Serilog;
using Serilog.Events;

namespace AdrenalineSpy;

public static class LoggingTask
{
    private static bool _configurado = false;
    
    public static void ConfigurarLogger()
    {
        if (_configurado)
            return;

        var config = Config.Instancia;
        var timestamp = DateTime.Now.ToString("dd-MM-yyyy");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(ParseNivel(config.Logging.NivelMinimo))
            .Enrich.FromLogContext()
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

### 2. Integrar no Program.cs

Modifique o `Program.cs` para inicializar o logging:

```csharp
namespace AdrenalineSpy;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Inicializar logging PRIMEIRO
            LoggingTask.ConfigurarLogger();
            LoggingTask.RegistrarInfo("=== AdrenalineSpy RPA Iniciado ===");

            // Carregar configurações
            var config = Config.Instancia;
            if (!config.Validar())
            {
                LoggingTask.RegistrarErro(new Exception("Configurações inválidas"), "Program");
                return;
            }

            LoggingTask.RegistrarInfo($"Configurações carregadas - URL: {config.Navegacao.UrlBase}, Categorias: {config.Categorias.Count}");

            // Aqui virão as outras Tasks (NavigationTask, ExtractionTask, etc.)
            LoggingTask.RegistrarInfo("Iniciando workflow de scraping...");
            
            // TODO: Implementar Workflow.cs
            
            LoggingTask.RegistrarInfo("=== AdrenalineSpy RPA Finalizado com Sucesso ===");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, "Program - Erro Fatal");
            Console.WriteLine($"❌ Erro fatal: {ex.Message}");
        }
        finally
        {
            // Finalizar logging
            LoggingTask.FecharLogger();
        }
    }
}
```

### 3. Padrão de Uso nas Outras Tasks

**NavigationTask.cs (exemplo):**
```csharp
namespace AdrenalineSpy.Workflow.Tasks;

public class NavigationTask
{
    public async Task<List<string>> ObterUrlsCategoria(string categoria)
    {
        try
        {
            LoggingTask.RegistrarInfo($"Iniciando navegação na categoria: {categoria}");
            
            var config = Config.Instancia;
            var urlCategoria = config.Navegacao.UrlBase + config.Categorias[categoria];
            
            LoggingTask.RegistrarInfo($"Abrindo página: {urlCategoria}");
            
            // ... lógica do Playwright ...
            
            var urls = new List<string>(); // URLs extraídas
            
            LoggingTask.RegistrarInfo($"Navegação concluída - {urls.Count} URLs encontradas na categoria {categoria}");
            
            return urls;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, $"NavigationTask - Categoria: {categoria}");
            throw;
        }
    }
}
```

**ExtractionTask.cs (exemplo):**
```csharp
namespace AdrenalineSpy.Workflow.Tasks;

public class ExtractionTask
{
    public async Task<NoticiaModel> ExtrairDados(string url)
    {
        try
        {
            LoggingTask.RegistrarInfo($"Iniciando extração de dados da URL: {url}");
            
            // ... lógica de extração ...
            
            var noticia = new NoticiaModel(); // Dados extraídos
            
            LoggingTask.RegistrarInfo($"Extração concluída - Título: {noticia.Titulo}, Categoria: {noticia.Categoria}");
            
            return noticia;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, $"ExtractionTask - URL: {url}");
            throw;
        }
    }
}
```

## Métodos Mais Usados

### Logging Básico

```csharp
// Inicializar (fazer uma vez no Program.cs)
LoggingTask.ConfigurarLogger();

// Log de informação (vai para logs/sucesso/)
LoggingTask.RegistrarInfo("Operação realizada com sucesso");
LoggingTask.RegistrarInfo("Processando categoria Games");
LoggingTask.RegistrarInfo($"Configurações carregadas - Timeout: {timeout}s");

// Log de aviso (vai para logs/falha/)
LoggingTask.RegistrarAviso("Timeout detectado - tentando novamente", "NavigationTask");
LoggingTask.RegistrarAviso("Rate limit atingido", "Scraping");

// Log de erro (vai para logs/falha/)
LoggingTask.RegistrarErro(exception, "DatabaseConnection");
LoggingTask.RegistrarErro(playwrightException, $"Playwright - URL: {url}");

// Log de debug (só aparece se NivelMinimo = "Debug")
LoggingTask.RegistrarDebug("Estado detalhado do navegador");
```

### Padrão Try-Catch com Logging

```csharp
public async Task MinhaOperacao(string parametro)
{
    try
    {
        LoggingTask.RegistrarInfo($"Iniciando operação com parâmetro: {parametro}");
        
        // ... sua lógica aqui ...
        
        LoggingTask.RegistrarInfo($"Operação concluída com sucesso - Parâmetro: {parametro}");
    }
    catch (TimeoutException ex)
    {
        LoggingTask.RegistrarAviso("Timeout na operação - será reexecutada", $"Parametro: {parametro}");
        // ... lógica de retry ...
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, $"MinhaOperacao - Parametro: {parametro}");
        throw; // Re-throw para não mascarar o erro
    }
}
```

### Logging de Performance

```csharp
public async Task OperacaoComTempo()
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    try
    {
        LoggingTask.RegistrarInfo("Iniciando operação longa");
        
        // ... operação demorada ...
        
        stopwatch.Stop();
        LoggingTask.RegistrarInfo($"Operação concluída em {stopwatch.ElapsedMilliseconds}ms");
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        LoggingTask.RegistrarErro(ex, $"OperacaoComTempo - Falha após {stopwatch.ElapsedMilliseconds}ms");
        throw;
    }
}
```

### Logging Estruturado para Scraping

```csharp
// Log de início de categoria
var categoria = "games";
var urlBase = "https://www.adrenaline.com.br/games/";
LoggingTask.RegistrarInfo($"Iniciando scraping - Categoria: {categoria}, URL: {urlBase}");

// Log de progresso
var urlsProcessadas = 15;
var urlsTotal = 50;
var porcentagem = (urlsProcessadas / (double)urlsTotal) * 100;
LoggingTask.RegistrarInfo($"Progresso: {urlsProcessadas}/{urlsTotal} URLs ({porcentagem:F1}%)");

// Log de resultado final
var noticiasExtraidas = 45;
var noticiasNovas = 12;
var tempoTotal = 120;
LoggingTask.RegistrarInfo($"Scraping finalizado - {noticiasExtraidas} extraídas, {noticiasNovas} novas, {tempoTotal}s");
```

### Debugging e Troubleshooting

```csharp
// Para debug detalhado (só aparece se NivelMinimo = "Debug" no JSON)
LoggingTask.RegistrarDebug($"Estado do navegador - Página carregada: {true}, URL: {page.Url}, Elementos: {elementos.Count}");

// Para acompanhar erros específicos do Playwright
try
{
    await page.ClickAsync("button");
}
catch (PlaywrightException ex) when (ex.Message.Contains("timeout"))
{
    var timeout = Config.Instancia.Navegacao.TimeoutSegundos;
    LoggingTask.RegistrarAviso("Timeout no click - elemento pode não estar visível", $"Selector: button, Timeout: {timeout}s");
}

// Para tracking de dados extraídos
LoggingTask.RegistrarInfo($"Dados extraídos - Título: {noticia.Titulo}, Categoria: {noticia.Categoria}, Tamanho: {noticia.Conteudo?.Length ?? 0} chars");
```

### Finalização Correta

```csharp
// No final do Program.cs (dentro do finally)
try
{
    LoggingTask.RegistrarInfo("Finalizando aplicação...");
    
    // ... cleanup de recursos ...
    
    LoggingTask.RegistrarInfo("Aplicação finalizada corretamente");
}
finally
{
    // SEMPRE finalizar para liberar recursos
    LoggingTask.FecharLogger();
}
```

---

## 💡 Resumo para AdrenalineSpy

**Setup único (fazer uma vez):**
1. `dotnet add package` dos 6 pacotes Serilog
2. Seção `Logging` no `AutomationSettings.json` (já existe)
3. Classe `LoggingConfig` no `Config.cs` (já existe)
4. Criar `LoggingTask.cs` (código fornecido acima)
5. Inicializar no `Program.cs` com `LoggingTask.ConfigurarLogger()`

**Uso em todas as Tasks:**
- `LoggingTask.RegistrarInfo()` → operações normais e sucessos
- `LoggingTask.RegistrarAviso()` → avisos (com contexto)
- `LoggingTask.RegistrarErro()` → erros em try/catch (com Exception)
- `LoggingTask.RegistrarDebug()` → debug detalhado

**Resultado:** 
- Logs automáticos separados: `logs/sucesso/` e `logs/falha/`
- Debug fácil quando o scraping falhar
- Histórico completo de todas as operações
- Logs estruturados com JSON para análise posterior

**Próxima etapa:** Implementar Playwright (que usará LoggingTask extensivamente)! 🚀
