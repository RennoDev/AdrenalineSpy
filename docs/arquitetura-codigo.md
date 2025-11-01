# Arquitetura de Código RPA - Organização de Projetos

## Índice
1. [Introdução](#introdução)
2. [Padrão Main → Workflow → Tasks](#padrão-main--workflow--tasks)
3. [Estrutura de Arquivos](#estrutura-de-arquivos)
4. [Implementação Passo a Passo](#implementação-passo-a-passo)
5. [Exemplos Práticos](#exemplos-práticos)
6. [Boas Práticas](#boas-práticas)
7. [Padrões Alternativos](#padrões-alternativos)

---

## Introdução

Organizar código RPA de forma escalável e manutenível é essencial para projetos que crescem em complexidade. Este guia apresenta o padrão **Main → Workflow → Tasks** para estruturar projetos RPA em .NET.

### Vantagens
- ✅ **Separação de responsabilidades:** Cada camada tem um propósito claro
- ✅ **Facilita testes:** Tasks podem ser testadas individualmente
- ✅ **Reutilização:** Tasks podem ser usadas em múltiplos Workflows
- ✅ **Manutenção:** Mudanças isoladas em cada camada
- ✅ **Legibilidade:** Código organizado e fácil de entender
- ✅ **Escalabilidade:** Adicionar novos workflows e tasks sem bagunça

---

## Padrão Main → Workflow → Tasks

### Camadas

```
Program.cs (Main)
    ↓
    └── Configura aplicação (logging, DI, GUI)
        └── Chama Workflows

Workflow.cs
    ↓
    └── Orquestra sequência de Tasks
        └── Trata erros gerais
            └── Chama Tasks na ordem correta

Tasks.cs
    ↓
    └── Executa ações específicas
        └── Login, scraping, salvamento, etc.
```

### Responsabilidades

| Camada | Responsabilidade | Exemplo |
|--------|------------------|---------|
| **Program.cs** | Entry point, configuração global | Serilog, Quartz, GUI, DI container |
| **Workflow.cs** | Orquestração de processos | `ExecutarScrapingCompleto()` |
| **Tasks.cs** | Ações atômicas específicas | `FazerLogin()`, `ColetarNoticias()` |

---

## Estrutura de Arquivos

### Organização Recomendada

```
AdrenalineSpy/
├── Program.cs                         # Entry point - orquestra ordem de execução
├── Config.cs                          # Herda configurações e distribui pela automação
├── AutomationSettings.json            # 🔐 Credenciais REAIS (git-ignored)
├── AutomationSettings.example.json    # 📋 Template para outros devs
├── Workflow/                          # Pasta única - projeto tem UM fluxo
│   ├── Workflow.cs                    # O fluxo único do projeto
│   └── Tasks/
│       ├── NavigationTask.cs          # Task: Navegação no site
│       ├── ExtractionTask.cs          # Task: Extração de dados
│       ├── MigrationTask.cs           # Task: Migração para banco
│       └── LoggingTask.cs             # Task: Helper de logging centralizado
├── logs/                              # 📁 Logs gerados automaticamente
│   ├── sucesso/                       # Logs de execuções bem-sucedidas
│   │   └── 01-11-2025-14:30.log
│   └── falha/                         # Logs de execuções com erros
│       └── 01-11-2025-15:45.log
└── relatorios/                        # 📁 Relatórios exportados (Excel/CSV)
```

**Estrutura Explicada:**
- **Program.cs**: Apenas orquestra execução
- **Config.cs**: Carrega `AutomationSettings.json` e distribui
- **Workflow/**: Pasta **singular** - projeto tem apenas UM fluxo de trabalho
- **Workflow.cs**: Gerencia sequência das Tasks
- **Tasks/**: Tasks atômicas e específicas do projeto:
  - `NavigationTask` - Navega pelo site Adrenaline
  - `ExtractionTask` - Extrai dados das páginas
  - `MigrationTask` - Salva dados no banco Docker
  - `LoggingTask` - **Helper centralizado de logging** (chamado em try/catch)

**⚠️ Importante:** 
- Adicione `/logs/` ao `.gitignore` para não versionar arquivos de log!
- Adicione `AutomationSettings.json` ao `.gitignore` para não versionar credenciais!
- Versione apenas `AutomationSettings.example.json` como template

---

## Implementação Passo a Passo

### Passo 1: Program.cs (Entry Point)

**Responsabilidade:** Orquestrar ordem de execução - APENAS isso

```csharp
using Serilog;

namespace AdrenalineSpy;

class Program
{
    static async Task Main(string[] args)
    {
        // Nome do arquivo de log com timestamp
        var timestamp = DateTime.Now.ToString("dd-MM-yyyy-HH:mm");
        var caminhoLogSucesso = $"logs/sucesso/{timestamp}.log";
        var caminhoLogFalha = $"logs/falha/{timestamp}.log";

        // Configurar Logging inicial (console)
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("=== Iniciando AdrenalineSpy ===");

            // Carregar configurações
            var config = Config.Carregar();

            // Executar workflow único do projeto
            var workflow = new Workflow(config);
            await workflow.ExecutarAsync();

            // Sucesso: reconfigurar log para pasta de sucesso
            ConfigurarLogSucesso(caminhoLogSucesso);
            Log.Information("=== AdrenalineSpy finalizado com sucesso ===");
        }
        catch (Exception ex)
        {
            // Falha: reconfigurar log para pasta de falha
            ConfigurarLogFalha(caminhoLogFalha);
            
            // LoggingTask.cs seria chamado aqui para centralizar registro de erro
            LoggingTask.RegistrarErro(ex, "Erro fatal na aplicação");
            
            Log.Fatal(ex, "Erro fatal na aplicação");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    static void ConfigurarLogSucesso(string caminho)
    {
        Log.CloseAndFlush();
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(caminho)
            .CreateLogger();
    }

    static void ConfigurarLogFalha(string caminho)
    {
        Log.CloseAndFlush();
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(caminho)
            .CreateLogger();
    }
}
```

**Abordagem Alternativa (Mais Simples):**

```csharp
using Serilog;
using Serilog.Events;

namespace AdrenalineSpy;

class Program
{
    static async Task Main(string[] args)
    {
        var timestamp = DateTime.Now.ToString("dd-MM-yyyy-HH:mm");

        // Configurar log com múltiplos destinos desde o início
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information || 
                                             e.Level == LogEventLevel.Debug || 
                                             e.Level == LogEventLevel.Verbose)
                .WriteTo.File($"logs/sucesso/{timestamp}.log"))
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Warning)
                .WriteTo.File($"logs/falha/{timestamp}.log"))
            .CreateLogger();

        try
        {
            Log.Information("=== Iniciando AdrenalineSpy ===");

            var config = Config.Carregar();
            var workflow = new Workflow(config);
            await workflow.ExecutarAsync();

            Log.Information("=== AdrenalineSpy finalizado com sucesso ===");
        }
        catch (Exception ex)
        {
            // LoggingTask centraliza registro de erros
            LoggingTask.RegistrarErro(ex, "Erro fatal na aplicação");
            Log.Fatal(ex, "Erro fatal na aplicação");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
```

---

### Passo 2: Config.cs (Configurações)

**Responsabilidade:** Herdar configurações de `AutomationSettings.json` e distribuir pela automação

```csharp
using System.Text.Json;

namespace AdrenalineSpy;

/// <summary>
/// Classe de configuração que herda settings de AutomationSettings.json
/// e distribui as configurações para toda a automação
/// </summary>
public class Config
{
    // URLs
    public string UrlBase { get; set; } = "https://www.adrenaline.com.br";
    
    // Credenciais (NUNCA commitar com valores reais!)
    public string Usuario { get; set; } = "";
    public string Senha { get; set; } = "";

    // Banco de Dados
    public string StringConexao { get; set; } = "";

    // Scraping
    public int DelayEntreRequests { get; set; } = 2000; // ms
    public int MaxRetentativas { get; set; } = 3;
    public bool ModoHeadless { get; set; } = false;

    // Agendamento
    public TimeSpan IntervaloExecucao { get; set; } = TimeSpan.FromHours(6);

    // Exportação
    public string DiretorioRelatorios { get; set; } = "relatorios/";

    /// <summary>
    /// Carrega configurações de AutomationSettings.json
    /// </summary>
    public static Config Carregar(string caminho = "AutomationSettings.json")
    {
        // Se não existir, criar do exemplo
        if (!File.Exists(caminho))
        {
            var caminhoExemplo = "AutomationSettings.example.json";
            if (File.Exists(caminhoExemplo))
            {
                Console.WriteLine($"⚠️  Arquivo '{caminho}' não encontrado!");
                Console.WriteLine($"📋 Copiando de '{caminhoExemplo}'...");
                File.Copy(caminhoExemplo, caminho);
                Console.WriteLine($"✅ Configure suas credenciais em '{caminho}' e execute novamente.");
                Environment.Exit(1);
            }
            else
            {
                throw new FileNotFoundException($"Arquivo de configuração não encontrado: {caminho}");
            }
        }

        var json = File.ReadAllText(caminho);
        var config = JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        }) ?? new Config();

        // Validar configurações obrigatórias
        ValidarConfig(config);

        return config;
    }

    /// <summary>
    /// Cria arquivo de exemplo (template)
    /// </summary>
    public static void CriarArquivoExemplo(string caminho = "AutomationSettings.example.json")
    {
        var configExemplo = new Config
        {
            UrlBase = "https://www.adrenaline.com.br",
            Usuario = "seu_usuario_aqui",
            Senha = "sua_senha_aqui",
            StringConexao = "Server=localhost;Port=3306;Database=adrenaline_db;User=root;Password=sua_senha_mysql;",
            DelayEntreRequests = 2000,
            MaxRetentativas = 3,
            ModoHeadless = false,
            DiretorioRelatorios = "relatorios/"
        };

        var json = JsonSerializer.Serialize(configExemplo, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        File.WriteAllText(caminho, json);
        Console.WriteLine($"✅ Arquivo de exemplo criado: {caminho}");
    }

    /// <summary>
    /// Valida se configurações obrigatórias estão presentes
    /// </summary>
    private static void ValidarConfig(Config config)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(config.StringConexao))
            erros.Add("StringConexao não pode estar vazia");

        if (config.StringConexao.Contains("sua_senha"))
            erros.Add("Configure a senha do banco de dados em AutomationSettings.json");

        if (config.Senha == "sua_senha_aqui")
            erros.Add("Configure as credenciais reais em AutomationSettings.json");

        if (erros.Any())
        {
            Console.WriteLine("❌ Erros de configuração:");
            erros.ForEach(e => Console.WriteLine($"   - {e}"));
            Environment.Exit(1);
        }
    }
}
```

### Arquivo: AutomationSettings.example.json (Template)

Crie este arquivo na raiz do projeto e **versione no Git**:

```json
{
  "UrlBase": "https://www.adrenaline.com.br",
  "Usuario": "seu_usuario_aqui",
  "Senha": "sua_senha_aqui",
  "StringConexao": "Server=localhost;Port=3306;Database=adrenaline_db;User=root;Password=sua_senha_mysql;",
  "DelayEntreRequests": 2000,
  "MaxRetentativas": 3,
  "ModoHeadless": false,
  "IntervaloExecucao": "06:00:00",
  "DiretorioRelatorios": "relatorios/"
}
```

### Arquivo: AutomationSettings.json (Credenciais Reais)

Copie de `AutomationSettings.example.json` e **NÃO versione no Git**:

```json
{
  "UrlBase": "https://www.adrenaline.com.br",
  "Usuario": "meu_usuario_real",
  "Senha": "MinhaS3nhaF0rt3!",
  "StringConexao": "Server=localhost;Port=3306;Database=adrenaline_db;User=root;Password=SenhaMySQL123;",
  "DelayEntreRequests": 2000,
  "MaxRetentativas": 3,
  "ModoHeadless": false,
  "DiretorioRelatorios": "relatorios/"
}
```

**Fluxo de Uso:**
1. `Config.Carregar()` lê `AutomationSettings.json`
2. `Config` é passado para Workflow
3. Workflow distribui `Config` para Tasks
4. Tasks usam as configurações herdadas

---

### Passo 3: Workflow.cs (Gerenciamento de Tasks)

**Responsabilidade:** Gerenciar sequência de tasks e tratar erros

```csharp
using Serilog;

namespace AdrenalineSpy;

public class Workflow
{
    private readonly Config _config;
    private readonly NavigationTask _navigationTask;
    private readonly ExtractionTask _extractionTask;
    private readonly MigrationTask _migrationTask;

    public Workflow(Config config)
    {
        _config = config;
        _navigationTask = new NavigationTask(config);
        _extractionTask = new ExtractionTask(config);
        _migrationTask = new MigrationTask(config);
    }

    public async Task ExecutarAsync()
    {
        Log.Information("Iniciando workflow do AdrenalineSpy");

        try
        {
            // Etapa 1: Inicializar navegador e navegar no site
            await _navigationTask.InicializarAsync();
            Log.Information("✓ Navegador inicializado");

            // Etapa 2: Navegar categorias e coletar URLs
            var categorias = new[] { "tecnologia", "games", "hardware" };
            var todasNoticias = new List<Noticia>();

            foreach (var categoria in categorias)
            {
                Log.Information("Navegando categoria: {Categoria}", categoria);
                
                // NavigationTask navega e retorna URLs
                var urls = await _navigationTask.ColetarUrlsPorCategoriaAsync(categoria);
                
                // ExtractionTask extrai dados de cada URL
                var noticias = await _extractionTask.ExtrairDadosAsync(urls, categoria);
                todasNoticias.AddRange(noticias);
                
                await Task.Delay(_config.DelayEntreRequests); // Rate limiting
            }

            Log.Information("✓ {Count} notícias coletadas no total", todasNoticias.Count);

            // Etapa 3: Migrar dados para banco Docker
            await _migrationTask.SalvarNoticiasAsync(todasNoticias);
            Log.Information("✓ Notícias migradas para banco de dados");

            Log.Information("Workflow concluído com sucesso");
        }
        catch (Exception ex)
        {
            // LoggingTask centraliza registro de exceções
            LoggingTask.RegistrarErro(ex, "Erro durante execução do workflow");
            Log.Error(ex, "Erro durante execução do workflow");
            throw;
        }
        finally
        {
            // Cleanup: fechar recursos
            await _navigationTask.FinalizarAsync();
        }
    }
}
```

---

### Passo 4: Tasks Individuais

#### NavigationTask.cs (Navegação no Site)

**Responsabilidade:** Navegar no site Adrenaline.com.br e coletar URLs

```csharp
using Microsoft.Playwright;
using Serilog;

namespace AdrenalineSpy;

public class NavigationTask
{
    private readonly Config _config;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;

    public NavigationTask(Config config)
    {
        _config = config;
    }

    public async Task InicializarAsync()
    {
        Log.Debug("Inicializando navegador");
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = _config.ModoHeadless
        });
        _page = await _browser.NewPageAsync();
    }

    public async Task<List<string>> ColetarUrlsPorCategoriaAsync(string categoria)
    {
        if (_page == null) throw new InvalidOperationException("Navegador não inicializado");

        Log.Debug("Navegando categoria: {Categoria}", categoria);
        var urls = new List<string>();

        try
        {
            var url = $"{_config.UrlBase}/categoria/{categoria}";
            await _page.GotoAsync(url);
            
            // Aguardar carregamento
            await _page.WaitForSelectorAsync(".article-list");
            
            // Coletar links dos artigos
            var artigos = await _page.QuerySelectorAllAsync(".article-item a");
            
            foreach (var artigo in artigos)
            {
                var href = await artigo.GetAttributeAsync("href");
                if (!string.IsNullOrEmpty(href))
                {
                    urls.Add(href);
                }
            }

            Log.Debug("Coletadas {Count} URLs de {Categoria}", urls.Count, categoria);
        }
        catch (Exception ex)
        {
            // Usa LoggingTask para centralizar registro de exceções
            LoggingTask.RegistrarErro(ex, $"Erro ao navegar categoria {categoria}");
            Log.Warning(ex, "Erro ao navegar categoria {Categoria}", categoria);
        }

        return urls;
    }

    public async Task FinalizarAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }
        
        _playwright?.Dispose();
        Log.Debug("Navegador finalizado");
    }
}
```

#### ExtractionTask.cs (Extração de Dados)

**Responsabilidade:** Extrair dados estruturados das páginas de notícias

```csharp
using Microsoft.Playwright;
using Serilog;

namespace AdrenalineSpy;

public class ExtractionTask
{
    private readonly Config _config;
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public ExtractionTask(Config config)
    {
        _config = config;
    }

    public async Task<List<Noticia>> ExtrairDadosAsync(List<string> urls, string categoria)
    {
        var noticias = new List<Noticia>();
        
        // Inicializar navegador para extração
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = _config.ModoHeadless });

        foreach (var url in urls)
        {
            try
            {
                var page = await _browser.NewPageAsync();
                await page.GotoAsync(url);

                // Extrair dados estruturados
                var titulo = await page.QuerySelectorAsync("h1.article-title");
                var conteudo = await page.QuerySelectorAsync(".article-content");
                var data = await page.QuerySelectorAsync(".article-date");

                var noticia = new Noticia
                {
                    Titulo = await titulo?.InnerTextAsync() ?? "",
                    Url = url,
                    Categoria = categoria,
                    Conteudo = await conteudo?.InnerTextAsync() ?? "",
                    DataPublicacao = await data?.InnerTextAsync() ?? "",
                    DataColeta = DateTime.Now
                };

                noticias.Add(noticia);
                await page.CloseAsync();

                Log.Debug("Extraída notícia: {Titulo}", noticia.Titulo);
                await Task.Delay(_config.DelayEntreRequests);
            }
            catch (Exception ex)
            {
                // LoggingTask registra exceções de forma centralizada
                LoggingTask.RegistrarErro(ex, $"Erro ao extrair URL {url}");
                Log.Warning(ex, "Erro ao extrair dados de {Url}", url);
            }
        }

        await _browser.CloseAsync();
        _playwright.Dispose();

        Log.Information("Extração concluída: {Count} notícias", noticias.Count);
        return noticias;
    }
}
```

#### MigrationTask.cs (Migração para Banco)

**Responsabilidade:** Salvar dados no banco de dados Docker

```csharp
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AdrenalineSpy;

public class MigrationTask
{
    private readonly Config _config;

    public MigrationTask(Config config)
    {
        _config = config;
    }

    public async Task SalvarNoticiasAsync(List<Noticia> noticias)
    {
        Log.Debug("Iniciando migração de {Count} notícias", noticias.Count);

        try
        {
            using var db = new AppDbContext(_config.StringConexao);

            // Garantir que banco existe
            await db.Database.EnsureCreatedAsync();

            // Salvar notícias
            await db.Noticias.AddRangeAsync(noticias);
            var registrosSalvos = await db.SaveChangesAsync();

            Log.Information("✓ {Count} notícias migradas para banco", registrosSalvos);
        }
        catch (Exception ex)
        {
            // LoggingTask centraliza erros
            LoggingTask.RegistrarErro(ex, "Erro ao salvar no banco de dados");
            Log.Error(ex, "Erro durante migração para banco");
            throw;
        }
    }
}
```

#### LoggingTask.cs (Helper de Logging Centralizado)

**Responsabilidade:** Centralizar registro de exceções em toda a aplicação

```csharp
using Serilog;

namespace AdrenalineSpy;

/// <summary>
/// Task helper para centralizar registro de exceções.
/// Chamada em blocos try/catch de todas as outras tasks.
/// </summary>
public static class LoggingTask
{
    /// <summary>
    /// Registra erro com contexto adicional de forma centralizada
    /// </summary>
    public static void RegistrarErro(Exception ex, string contexto)
    {
        // Enriquecer log com informações adicionais
        Log.Error(ex, "[LoggingTask] {Contexto} | Tipo: {TipoExcecao} | Mensagem: {Mensagem}", 
            contexto, 
            ex.GetType().Name, 
            ex.Message);

        // Registrar stack trace completo em Debug
        Log.Debug("Stack Trace: {StackTrace}", ex.StackTrace);

        // Se houver inner exception, registrar também
        if (ex.InnerException != null)
        {
            Log.Error(ex.InnerException, "[LoggingTask] Inner Exception: {Mensagem}", 
                ex.InnerException.Message);
        }
    }

    /// <summary>
    /// Registra aviso com contexto
    /// </summary>
    public static void RegistrarAviso(string mensagem, string contexto)
    {
        Log.Warning("[LoggingTask] {Contexto} | {Mensagem}", contexto, mensagem);
    }

    /// <summary>
    /// Registra informação com timestamp
    /// </summary>
    public static void RegistrarInfo(string mensagem)
    {
        Log.Information("[LoggingTask] {Mensagem} às {Timestamp}", 
            mensagem, 
            DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
    }
}
```

---

### Passo 5: Models (Modelos de Dados)

Pode ser uma classe separada ou no mesmo arquivo do projeto:

```csharp
namespace AdrenalineSpy;

public class Noticia
{
    public int Id { get; set; }
    public string Titulo { get; set; } = "";
    public string Url { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string Conteudo { get; set; } = "";
    public string DataPublicacao { get; set; } = "";
    public DateTime DataColeta { get; set; }
}
```

**Nota:** O modelo `Noticia` representa os dados extraídos das páginas do Adrenaline.com.br.

---

## Exemplos Práticos

### Exemplo 1: Workflow com Retry Logic e LoggingTask

```csharp
using Serilog;

namespace AdrenalineSpy;

public class WorkflowComRetry
{
    private readonly NavigationTask _navigationTask;
    private const int MaxTentativas = 3;

    public WorkflowComRetry(Config config)
    {
        _navigationTask = new NavigationTask(config);
    }

    public async Task ExecutarAsync()
    {
        for (int tentativa = 1; tentativa <= MaxTentativas; tentativa++)
        {
            try
            {
                Log.Information("Tentativa {Num}/{Max}", tentativa, MaxTentativas);
                
                await _navigationTask.InicializarAsync();
                var urls = await _navigationTask.ColetarUrlsPorCategoriaAsync("tecnologia");
                
                // Sucesso, sair do loop
                Log.Information("Sucesso na tentativa {Num}", tentativa);
                LoggingTask.RegistrarInfo($"Workflow executado com sucesso na tentativa {tentativa}");
                return;
            }
            catch (Exception ex) when (tentativa < MaxTentativas)
            {
                // LoggingTask registra avisos de tentativas falhadas
                LoggingTask.RegistrarAviso(
                    $"Tentativa {tentativa} falhou, tentando novamente...", 
                    "WorkflowComRetry");
                
                Log.Warning(ex, "Erro na tentativa {Num}, tentando novamente...", tentativa);
                await Task.Delay(TimeSpan.FromSeconds(5 * tentativa)); // Backoff exponencial
            }
            catch (Exception ex)
            {
                // Tentativa final falhou, registrar erro fatal
                LoggingTask.RegistrarErro(ex, $"Falha definitiva após {MaxTentativas} tentativas");
                throw;
            }
            finally
            {
                await _navigationTask.FinalizarAsync();
            }
        }

        var erroFinal = new Exception($"Falha após {MaxTentativas} tentativas");
        LoggingTask.RegistrarErro(erroFinal, "Todas as tentativas do workflow falharam");
        throw erroFinal;
    }
}
```

### Exemplo 2: Workflow com Múltiplas Categorias

```csharp
public class ScrapingMultiCategorias
{
    public async Task ExecutarAsync(List<string> categorias)
    {
        foreach (var categoria in categorias)
        {
            Log.Information("Processando categoria: {Categoria}", categoria);

            var noticias = await _webTasks.ColetarNoticiasPorCategoriaAsync(categoria);
            await _dbTasks.SalvarNoticiasAsync(noticias);

            // Delay entre categorias para evitar rate limiting
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}
```

### Exemplo 2: Scraping Paralelo com LoggingTask

```csharp
using Serilog;

namespace AdrenalineSpy;

public class ScrapingParalelo
{
    private readonly Config _config;
    private readonly MigrationTask _migrationTask;

    public ScrapingParalelo(Config config)
    {
        _config = config;
        _migrationTask = new MigrationTask(config);
    }

    public async Task ExecutarAsync(List<string> urls)
    {
        LoggingTask.RegistrarInfo($"Iniciando scraping paralelo de {urls.Count} URLs");

        var tasks = urls.Select(async url =>
        {
            var extractionTask = new ExtractionTask(_config);
            
            try
            {
                return await extractionTask.ExtrairDadosAsync(new List<string> { url }, "geral");
            }
            catch (Exception ex)
            {
                // LoggingTask registra erros individuais sem parar todo o processo
                LoggingTask.RegistrarErro(ex, $"Erro ao processar URL paralela: {url}");
                return new List<Noticia>(); // Retorna lista vazia para continuar
            }
        });

        var resultados = await Task.WhenAll(tasks);
        var todasNoticias = resultados.SelectMany(x => x).ToList();

        LoggingTask.RegistrarInfo($"Scraping paralelo concluído: {todasNoticias.Count} notícias coletadas");
        await _migrationTask.SalvarNoticiasAsync(todasNoticias);
    }
}
```

---

## Boas Práticas

### ✅ Program.cs
- **Mínimo possível:** Apenas orquestração de execução
- **Logging global:** Configurar Serilog no início
- **Try-catch top-level:** Capturar erros fatais
- **Não coloque lógica de negócio aqui**

### ✅ Config.cs
- **Todas as configurações em um lugar:** URLs, credenciais, timeouts, etc.
- **Valores padrão sensatos:** Para facilitar testes
- **Suporte a múltiplas fontes:** JSON, variáveis de ambiente, argumentos
- **Validação opcional:** Verificar se configurações estão corretas

### ✅ Workflow.cs
- **Um workflow = um processo completo:** Ex: "Scraping completo", "Geração de relatórios"
- **Gerencia tasks, não implementa:** Delega trabalho para Tasks
- **Tratamento de erros:** Try-catch e logging adequado
- **Cleanup no finally:** Sempre liberar recursos
- **Sem lógica complexa:** Se ficar complexo, crie uma Task

### ✅ TaskX.cs (Tasks Individuais)
- **Uma responsabilidade:** Cada Task faz UMA coisa específica
- **Métodos públicos claros:** `InicializarAsync()`, `ExecutarAsync()`, `FinalizarAsync()`
- **Reutilizáveis:** Podem ser usadas em múltiplos workflows
- **Testáveis:** Fácil de escrever testes unitários
- **Estado interno isolado:** Cada Task gerencia seus próprios recursos

### ✅ Nomenclatura
```csharp
// ✅ Arquivos do AdrenalineSpy
Program.cs              // Entry point
Config.cs               // Configurações
Workflow.cs             // Workflow único do projeto (em Workflow/)
NavigationTask.cs       // Task: Navegação (em Workflow/Tasks/)
ExtractionTask.cs       // Task: Extração de dados (em Workflow/Tasks/)
MigrationTask.cs        // Task: Migração para banco (em Workflow/Tasks/)
LoggingTask.cs          // Task: Helper de logging (em Workflow/Tasks/)

// ✅ Métodos
await InicializarAsync()
await ColetarNoticiasAsync()
await SalvarAsync()
await FinalizarAsync()

// ❌ Ruim
await Process()
await DoStuff()
await Execute()
```

### ✅ Responsabilidades Claras
```
Program.cs    → Orquestra ordem de execução (qual workflow iniciar)
Config.cs     → Centraliza configurações (valores usados no projeto)
Workflow.cs   → Gerencia Tasks (ordem, retry, tratamento de erros)
TaskX.cs      → Executa ação específica (web scraping, banco, etc.)
```

---

## Padrões Alternativos

### Padrão MVC (Model-View-Controller)
**Quando usar:** Projetos com interface gráfica complexa
```
Models/         # Dados
Views/          # Interface (WPF/WinForms)
Controllers/    # Lógica de controle
Services/       # Lógica de negócio (equivalente a Tasks)
```

### Padrão Repository
**Quando usar:** Acesso a dados complexo
```
Repositories/
    INoticiaRepository.cs
    NoticiaRepository.cs
```

### Clean Architecture
**Quando usar:** Projetos grandes e complexos
```
Core/           # Entidades e interfaces
Application/    # Casos de uso (Workflows)
Infrastructure/ # Implementações (Tasks, DB, etc)
Presentation/   # UI
```

### Padrão Simples (para projetos pequenos)
```
Program.cs      # Tudo em um arquivo
                # Aceitável para scripts simples
```

---

## 💡 Dicas Finais

1. **Comece simples:** Use Main → Workflow → Tasks para 90% dos casos
2. **Refatore quando necessário:** Se Tasks ficar grande, divida em subclasses
3. **Use interfaces:** Facilita testes e mocks
4. **Documente decisões:** Comente o "porquê", não o "como"
5. **Consistência:** Mantenha o padrão em todo o projeto

---

## 📚 Recursos Relacionados

- [Serilog](serilog.md) - Logging estruturado
- [Quartz](quartz.md) - Agendamento de workflows
- [ORM](orm.md) - Entity Framework e Dapper para Tasks de banco
- [Playwright](playwright.md) - Tasks de web scraping

---

*Última atualização: 01/11/2025*
