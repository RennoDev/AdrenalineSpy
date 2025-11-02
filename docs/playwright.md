# Microsoft.Playwright - Automação Web

## 1. O que é o Playwright

**Microsoft Playwright** é uma biblioteca de automação web desenvolvida pela Microsoft que permite controlar navegadores (Chromium, Firefox, WebKit/Safari) através de código .NET.

### Por que usar no AdrenalineSpy?

✅ **Auto-waiting** - Espera automática por elementos (elimina `Thread.Sleep`)  
✅ **Multi-browser** - Suporte a Chromium, Firefox e WebKit com mesma API  
✅ **Headless/Headed** - Modo invisível para produção ou visível para debug  
✅ **Performance** - API assíncrona ideal para scraping em larga escala  
✅ **Interceptação de rede** - Bloquear recursos desnecessários (imagens, CSS)  
✅ **Screenshots e PDFs** - Captura de tela e geração de PDFs automática

### Onde é usado no projeto

- **NavigationTask** - Navegar no Adrenaline.com.br e coletar URLs de notícias por categoria
- **ExtractionTask** - Acessar páginas individuais e extrair dados estruturados (título, conteúdo, data)

---

## 2. Como Instalar

### Passo 1: Instalar Pacote NuGet

```bash
dotnet add package Microsoft.Playwright
```

### Passo 2: Instalar Motores dos Navegadores (OBRIGATÓRIO!)

Após instalar o pacote, você **DEVE** executar o comando para baixar os navegadores:

**No Windows (PowerShell):**

```bash
# 1. Primeiro compile o projeto
dotnet build

# 2. Instale os navegadores
pwsh bin\Debug\net9.0\playwright.ps1 install
```

**No Linux/macOS:**

```bash
# 1. Primeiro compile o projeto  
dotnet build

# 2. Instale os navegadores
pwsh bin/Debug/net9.0/playwright.ps1 install
```

⚠️ **Erro comum:** Se você ver `Executable doesn't exist at ...`, significa que esqueceu este passo!

### Passo 3 (Opcional): Instalar apenas um navegador

Para economizar espaço em disco:

```bash
# Instalar apenas Chromium (recomendado para scraping)
pwsh bin\Debug\net9.0\playwright.ps1 install chromium

# Opções disponíveis: chromium, firefox, webkit
```

---

## 3. Implementar no AutomationSettings.json

Use a seção `Navegacao` existente, enriquecendo conforme necessário:

```json
{
  "Navegacao": {
    "UrlBase": "https://www.adrenaline.com.br",
    "TimeoutSegundos": 30,
    "HeadlessMode": false,
    "NavegadorPadrao": "chromium",
    "ViewportWidth": 1920,
    "ViewportHeight": 1080,
    "UserAgent": "",
    "BloquearImagens": true,
    "BloquearCSS": false
  }
}
```

**Configurações explicadas:**

- `UrlBase` - Site base para todas as navegações
- `TimeoutSegundos` - Timeout padrão para todas as operações (30s recomendado)
- `HeadlessMode` - `false` = visível (debug), `true` = invisível (produção)
- `NavegadorPadrao` - `"chromium"`, `"firefox"` ou `"webkit"`
- `ViewportWidth/Height` - Resolução da janela do navegador
- `UserAgent` - String personalizada do user-agent (vazio = padrão)
- `BloquearImagens` - Acelera scraping bloqueando imagens
- `BloquearCSS` - Bloquear CSS (pode quebrar layout mas é mais rápido)

---

## 4. Implementar no Config.cs

### NavegacaoConfig (Config.cs)

A classe `NavegacaoConfig` já existe e herda automaticamente do JSON. Atualize conforme necessário:

```csharp
public class NavegacaoConfig
{
    public string UrlBase { get; set; } = string.Empty;
    public int TimeoutSegundos { get; set; } = 30;
    public bool HeadlessMode { get; set; } = false;
    public string NavegadorPadrao { get; set; } = "chromium";
    public int ViewportWidth { get; set; } = 1920;
    public int ViewportHeight { get; set; } = 1080;
    public string UserAgent { get; set; } = string.Empty;
    public bool BloquearImagens { get; set; } = false;
    public bool BloquearCSS { get; set; } = false;
}
```

### Playwright.cs (Classe Dedicada)

**Crie uma classe específica** para centralizar toda configuração do "motor" do Playwright e evitar código repetitivo nas Tasks:

#### Passo 1: Criar o arquivo Playwright.cs

Na **raiz do projeto** (mesmo nível de `Program.cs`), crie um novo arquivo chamado `Playwright.cs`:

```csharp
using Microsoft.Playwright;

namespace AdrenalineSpy;

/// <summary>
/// Classe responsável por centralizar toda configuração do Playwright
/// Evita código repetitivo nas Tasks e facilita manutenção
/// </summary>
public static class Playwright
{
    private static IPlaywright? _playwright;
    private static IBrowser? _browser;
    private static readonly Config _config = Config.Instancia;

    /// <summary>
    /// Inicializa o Playwright e navegador usando configurações do Config
    /// </summary>
    public static async IBrowser InicializarNavegador()
    {
        if (_browser != null)
            return _browser; // Reutilizar se já existe

        try
        {
            LoggingTask.RegistrarInfo("Inicializando Playwright...");

            // Criar instância do Playwright
            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();

            // Configurar opções do navegador baseado no Config
            var opcoes = new BrowserTypeLaunchOptions
            {
                Headless = _config.Navegacao.HeadlessMode,
                Timeout = _config.Navegacao.TimeoutSegundos * 1000
            };

            // Escolher navegador baseado na configuração
            _browser = _config.Navegacao.NavegadorPadrao.ToLower() switch
            {
                "firefox" => await _playwright.Firefox.LaunchAsync(opcoes),
                "webkit" => await _playwright.Webkit.LaunchAsync(opcoes),
                _ => await _playwright.Chromium.LaunchAsync(opcoes)
            };

            LoggingTask.RegistrarInfo($"✅ Navegador {_config.Navegacao.NavegadorPadrao} iniciado");
            return _browser;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, "Playwright.InicializarNavegador");
            throw;
        }
    }

    /// <summary>
    /// Criar nova página com todas as configurações personalizadas aplicadas
    /// </summary>
    public static async IPage CriarPagina()
    {
        var browser = await InicializarNavegador();

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize 
            { 
                Width = _config.Navegacao.ViewportWidth, 
                Height = _config.Navegacao.ViewportHeight 
            },
            UserAgent = string.IsNullOrEmpty(_config.Navegacao.UserAgent) 
                ? null 
                : _config.Navegacao.UserAgent
        });

        // Aplicar bloqueios de recursos automaticamente
        await ConfigurarBloqueiosRecursos(context);

        var page = await context.NewPageAsync();
        
        // Configurar timeout padrão para todas as operações
        page.SetDefaultTimeout(_config.Navegacao.TimeoutSegundos * 1000);

        return page;
    }

    /// <summary>
    /// Configurar bloqueios de recursos (imagens, CSS) para acelerar scraping
    /// </summary>
    private static async ConfigurarBloqueiosRecursos(IBrowserContext context)
    {
        if (!_config.Navegacao.BloquearImagens && !_config.Navegacao.BloquearCSS)
            return;

        var recursos = new List<string>();

        if (_config.Navegacao.BloquearImagens)
            recursos.AddRange(new[] { "**/*.{png,jpg,jpeg,gif,svg,webp,ico,bmp}" });

        if (_config.Navegacao.BloquearCSS)
            recursos.AddRange(new[] { "**/*.css", "**/*.woff", "**/*.woff2", "**/*.ttf" });

        foreach (var recurso in recursos)
        {
            await context.RouteAsync(recurso, route => route.AbortAsync());
        }

        LoggingTask.RegistrarDebug($"Bloqueio de recursos configurado: {string.Join(", ", recursos)}");
    }

    /// <summary>
    /// Navegar para URL com configurações otimizadas
    /// </summary>
    public static async NavegarPara(IPage page, string url)
    {
        try
        {
            LoggingTask.RegistrarDebug($"Navegando para: {url}");

            await page.GotoAsync(url, new PageGotoOptions
            {
                Timeout = _config.Navegacao.TimeoutSegundos * 1000,
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            LoggingTask.RegistrarInfo($"✅ Página carregada: {url}");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, $"Playwright.NavegarPara({url})");
            throw;
        }
    }

    /// <summary>
    /// Finalizar e fechar todos os recursos do Playwright
    /// </summary>
    public static async Finalizar()
    {
        try
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
                _browser = null;
                LoggingTask.RegistrarInfo("Navegador fechado");
            }

            _playwright?.Dispose();
            _playwright = null;

            LoggingTask.RegistrarInfo("Playwright finalizado");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, "Playwright.Finalizar");
        }
    }
}
```

#### Passo 2: Verificar se compilou corretamente

Após criar o arquivo, teste se está tudo funcionando:

```bash
dotnet build
```

Se der erro de compilação, verifique:
- ✅ O arquivo está na **raiz do projeto** (mesmo nível do `.csproj`)
- ✅ O namespace é `AdrenalineSpy` 
- ✅ A linha `_playwright = await Microsoft.Playwright.Playwright.CreateAsync();` está correta

### Como usar nas Tasks:

```csharp
// Em vez de repetir configuração, use o Playwright
var page = await Playwright.CriarPagina();
await Playwright.NavegarPara(page, "https://site.com");

// No final da aplicação
await Playwright.Finalizar();
```

### Exemplo Prático: Pesquisar no Google

Aqui está um exemplo **completo e funcional** de como criar uma classe para pesquisar no Google usando Playwright:

#### Passo 1: Criar o arquivo NavigationGoogle.cs

Na pasta `Workflow/Tasks/`, crie um novo arquivo `NavigationGoogle.cs`:

```csharp
using Microsoft.Playwright;

namespace AdrenalineSpy;

/// <summary>
/// Exemplo de navegação no Google usando Playwright
/// </summary>
public class NavigationGoogle
{
    private readonly Config _config;

    public NavigationGoogle()
    {
        _config = Config.Instancia;
    }

    /// <summary>
    /// Exemplo prático: Pesquisar "playwright" no Google
    /// </summary>
    public async Task ExemploPesquisarGoogle()
    {
        try
        {
            LoggingTask.RegistrarInfo("Iniciando pesquisa no Google...");

            // Criar página usando nossa classe Playwright
            var page = await Playwright.CriarPagina();

            // Navegar para o Google
            await Playwright.NavegarPara(page, "https://www.google.com");

            // Aguardar campo de pesquisa aparecer
            await page.WaitForSelectorAsync("input[name='q']", new PageWaitForSelectorOptions
            {
                Timeout = 10000
            });

            // Localizar campo de pesquisa (seletor do Google)
            var campoPesquisa = page.Locator("input[name='q']");

            // Escrever "playwright" no campo
            await campoPesquisa.FillAsync("playwright");
            LoggingTask.RegistrarInfo("✅ Texto 'playwright' digitado no campo de pesquisa");

            // Pressionar Enter para enviar a pesquisa
            await campoPesquisa.PressAsync("Enter");
            LoggingTask.RegistrarInfo("✅ Tecla Enter pressionada");

            // Aguardar resultados carregarem
            await page.WaitForSelectorAsync("#search", new PageWaitForSelectorOptions
            {
                Timeout = 10000
            });

            // Capturar screenshot dos resultados
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "google-resultados-playwright.png",
                FullPage = false
            });

            LoggingTask.RegistrarInfo("✅ Pesquisa concluída e screenshot salvo");

            // Fechar página
            await page.Context.CloseAsync();
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, "ExemploPesquisarGoogle");
        }
    }
}
```

#### Passo 2: Como usar no Program.cs

```csharp
// No seu Program.cs ou onde quiser testar
var googleTask = new NavigationGoogle();
await googleTask.ExemploPesquisarGoogle();
```

**Seletores importantes do Google:**
- `input[name='q']` - Campo de pesquisa principal
- `input[value='Pesquisa Google']` - Botão "Pesquisa Google" 
- `input[value='Estou com sorte']` - Botão "Estou com sorte"
- `#search` - Container dos resultados de pesquisa
- `.g` - Cada resultado individual de pesquisa

**Dicas para descobrir seletores:**
1. Abra o Google no navegador
2. Pressione F12 para abrir DevTools
3. Clique na ferramenta de seleção (🔍 ou Ctrl+Shift+C)
4. Clique no elemento desejado
5. Copie o seletor CSS no painel Elements

---

## 5. Montar nas Tasks (NavigationTask.cs)

### Estrutura básica da NavigationTask (SIMPLIFICADA)

Com o `Playwright.cs`, a `NavigationTask` fica muito mais limpa e focada apenas na lógica de negócio:

```csharp
using Microsoft.Playwright;

namespace AdrenalineSpy;

/// <summary>
/// Task responsável por navegação e coleta de URLs usando Playwright
/// Toda configuração do "motor" delegada para Playwright
/// </summary>
public class NavigationTask
{
    private readonly Config _config;

    public NavigationTask()
    {
        _config = Config.Instancia;
    }

    /// <summary>
    /// Coletar URLs de uma categoria específica
    /// </summary>
    public async List<string> ColetarUrlsCategoria(string categoria, string caminhoCategoria)
    {
        var urls = new List<string>();
        
        try
        {
            LoggingTask.RegistrarInfo($"Coletando URLs da categoria: {categoria}");

            // Playwright cuida de toda a configuração!
            var page = await Playwright.CriarPagina();
            string urlCompleta = _config.Navegacao.UrlBase + caminhoCategoria;
            
            await Playwright.NavegarPara(page, urlCompleta);

            // Aguardar elementos de notícias aparecerem
            await page.WaitForSelectorAsync("article", new PageWaitForSelectorOptions
            {
                Timeout = 10000
            });

            // Coletar todos os links de notícias (AJUSTAR SELETOR CONFORME HTML REAL)
            var links = await page.Locator("article a[href]").AllAsync();

            foreach (var link in links)
            {
                var href = await link.GetAttributeAsync("href");
                
                if (!string.IsNullOrEmpty(href))
                {
                    string urlCompleteLink = href.StartsWith("/") 
                        ? _config.Navegacao.UrlBase + href
                        : href;
                    
                    urls.Add(urlCompleteLink);
                }
            }

            LoggingTask.RegistrarInfo($"✅ {urls.Count} URLs coletadas de {categoria}");
            
            await page.Context.CloseAsync();
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, $"NavigationTask.ColetarUrlsCategoria({categoria})");
        }

        return urls;
    }

    /// <summary>
    /// Coletar URLs de múltiplas categorias
    /// </summary>
    public async Dictionary<string, List<string>> ColetarUrlsMultiplasCategorias()
    {
        var resultado = new Dictionary<string, List<string>>();

        // Categorias do Adrenaline.com.br (AJUSTAR CONFORME SITE REAL)
        var categorias = new Dictionary<string, string>
        {
            { "Tecnologia", "/tecnologia" },
            { "Games", "/games" },
            { "Hardware", "/hardware" },
            { "Smartphones", "/smartphones" }
        };

        foreach (var categoria in categorias)
        {
            try
            {
                var urls = await ColetarUrlsCategoria(categoria.Key, categoria.Value);
                resultado[categoria.Key] = urls;

                LoggingTask.RegistrarInfo($"Categoria {categoria.Key}: {urls.Count} URLs coletadas");

                // Delay entre categorias para ser "educado"
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                LoggingTask.RegistrarErro(ex, $"Erro ao coletar categoria {categoria.Key}");
                resultado[categoria.Key] = new List<string>(); // Lista vazia em caso de erro
            }
        }

        return resultado;
    }

    /// <summary>
    /// Navegar para uma página específica e extrair dados básicos
    /// </summary>
    public async NoticiaBasica? ExtrairDadosBasicos(string url)
    {
        try
        {
            LoggingTask.RegistrarDebug($"Extraindo dados básicos de: {url}");

            var page = await Playwright.CriarPagina();
            await Playwright.NavegarPara(page, url);

            // Aguardar conteúdo principal
            await page.WaitForSelectorAsync("main, article, .content", new PageWaitForSelectorOptions
            {
                Timeout = 10000
            });

            // Extrair dados básicos (AJUSTAR SELETORES CONFORME HTML REAL)
            var noticia = new NoticiaBasica
            {
                Url = url,
                Titulo = await page.Locator("h1").TextContentAsync() ?? "Sem título",
                DataPublicacao = await page.Locator(".publish-date, .date").TextContentAsync() ?? "",
                Categoria = await page.Locator(".category, .tag").FirstAsync().TextContentAsync() ?? "",
                Resumo = await page.Locator(".summary, .excerpt").TextContentAsync() ?? "",
                DataColeta = DateTime.Now
            };

            LoggingTask.RegistrarDebug($"✅ Dados extraídos: {noticia.Titulo}");
            
            await page.Context.CloseAsync();
            return noticia;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, $"NavigationTask.ExtrairDadosBasicos({url})");
            return null;
        }
    }

    /// <summary>
    /// Finalizar recursos do Playwright (chama Playwright)
    /// </summary>
    public async Finalizar()
    {
        await Playwright.Finalizar();
        LoggingTask.RegistrarInfo("NavigationTask finalizada");
    }
}

/// <summary>
/// Classe para dados básicos de uma notícia
/// </summary>
public class NoticiaBasica
{
    public string Url { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string DataPublicacao { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Resumo { get; set; } = string.Empty;
    public DateTime DataColeta { get; set; }
}
```

---

## Como Adicionar no Program.cs

### Evoluindo o Program.cs com Web Scraping

Após implementar **RestSharp+JSON**, **Serilog** e criar a **NavigationTask.cs**, agora você integra o web scraping no Program.cs.

### Program.cs - Fase: Primeira Execução de Scraping
```csharp
static async Task Main(string[] args)
{
    Config config = Config.Instancia;
    
    if (!config.Validar())
    {
        Console.WriteLine("❌ Configurações inválidas!");
        return;
    }
    
    LoggingTask.ConfigurarLogger();
    
    try
    {
        LoggingTask.RegistrarInfo("=== AdrenalineSpy Iniciado ===");
        
        // ADICIONADO: Primeira execução do web scraping
        var navigationTask = new NavigationTask();
        
        LoggingTask.RegistrarInfo("Iniciando coleta de URLs...");
        var urls = await navigationTask.ColetarUrlsCategoriaAsync("tecnologia");
        
        LoggingTask.RegistrarInfo($"Coletadas {urls.Count} URLs da categoria tecnologia");
        
        // Exibir URLs coletadas (temporário para teste)
        foreach (var url in urls.Take(5)) // Apenas 5 primeiras
        {
            Console.WriteLine($"📄 {url}");
        }
        
        LoggingTask.RegistrarInfo("=== Primeira coleta concluída ===");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Program.Main");
    }
    finally
    {
        LoggingTask.FecharLogger();
    }
}
```

### Program.cs - Fase: Múltiplas Categorias
```csharp
static async Task Main(string[] args)
{
    Config config = Config.Instancia;
    LoggingTask.ConfigurarLogger();
    
    try
    {
        LoggingTask.RegistrarInfo("=== AdrenalineSpy Iniciado ===");
        
        var navigationTask = new NavigationTask();
        var todasUrls = new List<string>();
        
        // ADICIONADO: Processar múltiplas categorias
        foreach (var categoria in config.Categorias.Keys)
        {
            LoggingTask.RegistrarInfo($"Processando categoria: {categoria}");
            
            var urls = await navigationTask.ColetarUrlsCategoriaAsync(categoria);
            todasUrls.AddRange(urls);
            
            LoggingTask.RegistrarInfo($"Categoria {categoria}: {urls.Count} URLs coletadas");
            
            // Aguardar entre categorias (evitar sobrecarga)
            await Task.Delay(2000);
        }
        
        LoggingTask.RegistrarInfo($"Total geral: {todasUrls.Count} URLs coletadas");
        
        LoggingTask.RegistrarInfo("=== Coleta de URLs finalizada ===");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Program.Main");
    }
    finally
    {
        LoggingTask.FecharLogger();
    }
}
```

### Program.cs - Fase: Com Argumentos de Linha de Comando
```csharp
static async Task Main(string[] args)
{
    Config config = Config.Instancia;
    LoggingTask.ConfigurarLogger();
    
    try
    {
        LoggingTask.RegistrarInfo("=== AdrenalineSpy Iniciado ===");
        
        var navigationTask = new NavigationTask();
        
        // ADICIONADO: Processar argumentos
        string categoria = ObterCategoriaArgumentos(args);
        bool modoHeadless = args.Contains("--headless");
        
        if (modoHeadless)
        {
            LoggingTask.RegistrarInfo("Modo headless ativado");
        }
        
        if (!string.IsNullOrEmpty(categoria))
        {
            // Categoria específica
            LoggingTask.RegistrarInfo($"Coletando categoria específica: {categoria}");
            var urls = await navigationTask.ColetarUrlsCategoriaAsync(categoria);
            LoggingTask.RegistrarInfo($"Coletadas {urls.Count} URLs");
        }
        else
        {
            // Todas as categorias
            await ProcessarTodasCategorias(navigationTask, config);
        }
        
        LoggingTask.RegistrarInfo("=== Scraping finalizado ===");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Program.Main");
    }
    finally
    {
        LoggingTask.FecharLogger();
    }
}

private static string ObterCategoriaArgumentos(string[] args)
{
    var argCategoria = args.FirstOrDefault(a => a.StartsWith("--categoria="));
    return argCategoria?.Substring(12); // Remove "--categoria="
}

private static async Task ProcessarTodasCategorias(NavigationTask navigationTask, Config config)
{
    foreach (var categoria in config.Categorias.Keys)
    {
        LoggingTask.RegistrarInfo($"Processando: {categoria}");
        
        var urls = await navigationTask.ColetarUrlsCategoriaAsync(categoria);
        LoggingTask.RegistrarInfo($"{categoria}: {urls.Count} URLs");
        
        await Task.Delay(2000); // Intervalo entre categorias
    }
}
```

### Program.cs - Fase: Com Tratamento de Erros Robusto
```csharp
static async Task Main(string[] args)
{
    Config config = Config.Instancia;
    LoggingTask.ConfigurarLogger();
    
    try
    {
        LoggingTask.RegistrarInfo("=== AdrenalineSpy Iniciado ===");
        
        // ADICIONADO: Inicialização com verificações
        if (!await ValidarConexaoInternet())
        {
            LoggingTask.RegistrarErro(new Exception("Sem conexão com internet"), "Program");
            return;
        }
        
        if (!await ValidarSiteDisponivel(config.Navegacao.UrlBase))
        {
            LoggingTask.RegistrarErro(new Exception("Site Adrenaline indisponível"), "Program");
            return;
        }
        
        var navigationTask = new NavigationTask();
        
        // Executar com retry automático
        await ExecutarComRetry(async () =>
        {
            await ProcessarTodasCategorias(navigationTask, config);
        }, maxTentativas: 3);
        
        LoggingTask.RegistrarInfo("=== Scraping concluído com sucesso ===");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Program.Main - Erro Fatal");
        Console.WriteLine($"❌ Erro fatal: {ex.Message}");
    }
    finally
    {
        LoggingTask.FecharLogger();
    }
}

private static async Task<bool> ValidarConexaoInternet()
{
    try
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(10);
        var response = await client.GetAsync("https://www.google.com");
        return response.IsSuccessStatusCode;
    }
    catch
    {
        return false;
    }
}

private static async Task<bool> ValidarSiteDisponivel(string url)
{
    try
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(15);
        var response = await client.GetAsync(url);
        return response.IsSuccessStatusCode;
    }
    catch
    {
        return false;
    }
}

private static async Task ExecutarComRetry(Func<Task> acao, int maxTentativas = 3)
{
    for (int tentativa = 1; tentativa <= maxTentativas; tentativa++)
    {
        try
        {
            await acao();
            return; // Sucesso
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Tentativa {tentativa} falhou: {ex.Message}");
            
            if (tentativa == maxTentativas)
                throw; // Última tentativa, propagar erro
                
            await Task.Delay(5000 * tentativa); // Delay progressivo
        }
    }
}
```

### Exemplos de Uso da Linha de Comando

```bash
# Executar todas as categorias
dotnet run

# Categoria específica
dotnet run -- --categoria=tecnologia

# Modo headless
dotnet run -- --headless

# Combinado
dotnet run -- --categoria=games --headless
```

### ⚠️ Ordem de Implementação Recomendada

1. **Comece simples** - Uma categoria, URLs impressas no console
2. **Adicione múltiplas categorias** - Loop pelas categorias do Config
3. **Implemente argumentos** - Flexibilidade de execução
4. **Adicione validações** - Verificar internet e site
5. **Implemente retry** - Robustez para falhas temporárias

### 💡 Próxima Evolução

Após dominar a coleta de URLs, o próximo passo será implementar **ExtractionTask** para extrair dados de cada página e **ORM** para salvar no banco de dados.

---

## 6. Métodos Mais Usados

### 6.1. Navegação Básica

```csharp
// Navegar para uma página
await page.GotoAsync("https://exemplo.com");

// Navegar com opções específicas
await page.GotoAsync("https://exemplo.com", new PageGotoOptions
{
    Timeout = 30000,
    WaitUntil = WaitUntilState.NetworkIdle // ou DOMContentLoaded, Load
});

// Voltar e avançar no histórico
await page.GoBackAsync();
await page.GoForwardAsync();

// Recarregar página
await page.ReloadAsync();
```

---

### 6.2. Como Conseguir e Configurar Seletores

#### Tipos de seletores mais usados:

```csharp
// 1. CSS Selector (mais comum)
var elemento = page.Locator("button.submit");
var elemento = page.Locator("#login-btn");
var elemento = page.Locator("div.content > p:first-child");

// 2. Por texto visível
var elemento = page.Locator("text=Entrar");
var elemento = page.Locator("text=/Login|Entrar/i"); // regex case-insensitive

// 3. Por atributo data
var elemento = page.Locator("[data-testid='submit-button']");
var elemento = page.Locator("[data-id='123']");

// 4. XPath (quando CSS não é suficiente)
var elemento = page.Locator("xpath=//button[@type='submit' and contains(text(), 'Enviar')]");

// 5. Combinação de seletores
var elemento = page.Locator("div.form >> button.submit"); // dentro de
var elemento = page.Locator("button:has-text('Salvar')"); // que contém texto
```

#### Como descobrir seletores no navegador:

1. Abrir DevTools (F12)
2. Usar ferramenta de seleção (Ctrl+Shift+C)
3. Clicar no elemento desejado
4. Copiar seletor CSS ou criar XPath

---

### 6.3. Cliques em Seletores

```csharp
// Clique simples (com auto-wait)
await page.Locator("button.submit").ClickAsync();

// Clique com timeout personalizado
await page.Locator("button").ClickAsync(new LocatorClickOptions
{
    Timeout = 5000 // 5 segundos
});

// Clique duplo
await page.Locator("div.item").DblClickAsync();

// Clique com botão direito
await page.Locator("div").ClickAsync(new LocatorClickOptions
{
    Button = MouseButton.Right
});

// Forçar clique (ignorar verificações)
await page.Locator("button").ClickAsync(new LocatorClickOptions
{
    Force = true
});

// Clique em coordenadas específicas do elemento
await page.Locator("canvas").ClickAsync(new LocatorClickOptions
{
    Position = new Position { X = 100, Y = 50 }
});
```

---

### 6.4. Hover + Click (Menu Dropdown)

```csharp
// Hover simples
await page.Locator("button.menu").HoverAsync();

// Hover + aguardar submenu + clicar
await page.Locator("button.menu").HoverAsync();

// Aguardar submenu ficar visível
await page.Locator("ul.submenu").WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Visible,
    Timeout = 5000
});

// Clicar no item do submenu
await page.Locator("ul.submenu li a[href='/categoria']").ClickAsync();

// Versão mais robusta com try-catch
try 
{
    await page.Locator("nav.menu > li.dropdown").HoverAsync();
    
    // Aguardar dropdown aparecer
    await page.WaitForSelectorAsync("nav.menu .dropdown-menu", new PageWaitForSelectorOptions
    {
        State = WaitForSelectorState.Visible,
        Timeout = 3000
    });
    
    await page.Locator(".dropdown-menu a:has-text('Tecnologia')").ClickAsync();
}
catch (TimeoutException)
{
    LoggingTask.RegistrarAviso("Menu dropdown não abriu no tempo esperado", "NavigationTask");
}
```

---

### 6.5. Esperas Explícitas

#### Esperar elemento aparecer na tela:

```csharp
// Esperar elemento ficar visível (mais usado)
await page.Locator("div.resultado").WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Visible,
    Timeout = 30000 // 30 segundos
});

// Esperar elemento existir no DOM (mesmo que invisível)
await page.Locator("div.hidden-content").WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Attached,
    Timeout = 10000
});

// Esperar elemento desaparecer
await page.Locator("div.loading-spinner").WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Hidden,
    Timeout = 15000
});

// Esperar elemento ser removido do DOM
await page.Locator("div.temp-message").WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Detached
});
```

#### Esperas de condições da página:

```csharp
// Esperar URL mudar
await page.WaitForURLAsync("**/success");
await page.WaitForURLAsync("https://exemplo.com/dashboard");

// Esperar carregamento da rede
await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions
{
    Timeout = 30000
});

// Esperar DOM carregar
await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

// Esperar função JavaScript retornar true
await page.WaitForFunctionAsync("() => document.readyState === 'complete'");

// Esperar condição personalizada
await page.WaitForFunctionAsync("() => document.querySelectorAll('article').length >= 10");
```

#### Esperas com timeout personalizados:

```csharp
// Timeout longo para elementos que demoram
await page.Locator("div.slow-loading").WaitForAsync(new LocatorWaitForOptions
{
    Timeout = 60000 // 1 minuto
});

// Timeout curto para verificações rápidas
await page.Locator("div.error-message").WaitForAsync(new LocatorWaitForOptions
{
    Timeout = 2000 // 2 segundos
});

// Usar timeout da configuração
await page.Locator("div.content").WaitForAsync(new LocatorWaitForOptions
{
    Timeout = _config.Navegacao.TimeoutSegundos * 1000
});
```

---

### 6.6. Extração de Dados dos Seletores

```csharp
// Extrair texto visível
string titulo = await page.Locator("h1.title").TextContentAsync();

// Extrair texto interno (sem HTML)
string conteudo = await page.Locator("div.content").InnerTextAsync();

// Extrair HTML interno
string html = await page.Locator("div.article").InnerHTMLAsync();

// Extrair atributos
string link = await page.Locator("a.read-more").GetAttributeAsync("href");
string imagem = await page.Locator("img.thumbnail").GetAttributeAsync("src");
string dataId = await page.Locator("article").GetAttributeAsync("data-id");

// Extrair múltiplos elementos
var titulos = await page.Locator("h2.article-title").AllTextContentsAsync();
var links = await page.Locator("article a.permalink").AllAsync();

// Iterar sobre múltiplos elementos
foreach (var item in links)
{
    string href = await item.GetAttributeAsync("href");
    string texto = await item.TextContentAsync();
    
    LoggingTask.RegistrarDebug($"Link encontrado: {texto} -> {href}");
}

// Contar elementos
int totalArtigos = await page.Locator("article.news-item").CountAsync();

// Verificar se elemento existe
bool temResultados = await page.Locator("div.results").CountAsync() > 0;

// Extrair dados estruturados
var noticias = new List<Noticia>();
var articles = await page.Locator("article.news").AllAsync();

foreach (var article in articles)
{
    var noticia = new Noticia
    {
        Titulo = await article.Locator("h2.title").TextContentAsync(),
        Url = await article.Locator("a.permalink").GetAttributeAsync("href"),
        DataTexto = await article.Locator(".publish-date").TextContentAsync(),
        Resumo = await article.Locator(".summary").TextContentAsync()
    };
    
    noticias.Add(noticia);
}
```

---

### 6.7. Preenchimento de Formulários

```csharp
// Preencher campos de texto
await page.Locator("input#name").FillAsync("João Silva");
await page.Locator("textarea#message").FillAsync("Mensagem de teste");

// Limpar campo e preencher
await page.Locator("input#email").FillAsync(""); // limpar
await page.Locator("input#email").FillAsync("novo@email.com");

// Digitar com delay (simular digitação humana)
await page.Locator("input#search").TypeAsync("Playwright", new LocatorTypeOptions
{
    Delay = 100 // 100ms entre cada tecla
});

// Pressionar teclas especiais
await page.Locator("input").PressAsync("Enter");
await page.Locator("input").PressAsync("Tab");
await page.Locator("input").PressAsync("Escape");

// Combinações de teclas
await page.Locator("input").PressAsync("Control+A"); // Selecionar tudo
await page.Locator("input").PressAsync("Control+C"); // Copiar
```

---

### 6.8. Verificações e Validações

```csharp
// Verificar se elemento está visível
bool isVisible = await page.Locator("button.submit").IsVisibleAsync();

// Verificar se elemento está habilitado
bool isEnabled = await page.Locator("button").IsEnabledAsync();

// Verificar se checkbox está marcado
bool isChecked = await page.Locator("input[type='checkbox']").IsCheckedAsync();

// Usar verificações em condições
if (await page.Locator("div.error").IsVisibleAsync())
{
    string errorMessage = await page.Locator("div.error").TextContentAsync();
    LoggingTask.RegistrarErro(new Exception(errorMessage), "Erro na página");
}

// Aguardar condição ser verdadeira
await page.Locator("button.submit").WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Visible
});

if (await page.Locator("button.submit").IsEnabledAsync())
{
    await page.Locator("button.submit").ClickAsync();
}
```

---

### 6.9. Screenshots para Debug

```csharp
// Screenshot da página inteira
await page.ScreenshotAsync(new PageScreenshotOptions
{
    Path = "debug-pagina.png",
    FullPage = true
});

// Screenshot de um elemento específico
await page.Locator("div.content").ScreenshotAsync(new LocatorScreenshotOptions
{
    Path = "debug-elemento.png"
});

// Screenshot condicionado (só em caso de erro)
try
{
    await page.Locator("button").ClickAsync();
}
catch (Exception ex)
{
    await page.ScreenshotAsync(new PageScreenshotOptions
    {
        Path = $"erro-{DateTime.Now:yyyyMMdd-HHmmss}.png"
    });
    
    LoggingTask.RegistrarErro(ex, "Erro ao clicar no botão");
    throw;
}
```

---

### 6.10. Exemplo Prático: Scraping com Retry

```csharp
/// <summary>
/// Navegar para uma página com retry automático em caso de falha
/// </summary>
public async string NavegarComRetry(string url, int maxTentativas = 3)
{
    int tentativa = 0;
    
    while (tentativa < maxTentativas)
    {
        try
        {
            tentativa++;
            LoggingTask.RegistrarDebug($"Tentativa {tentativa}/{maxTentativas} para {url}");

            var page = await CriarPagina();

            // Navegar com timeout
            await page.GotoAsync(url, new PageGotoOptions
            {
                Timeout = _config.Navegacao.TimeoutSegundos * 1000,
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            // Aguardar conteúdo principal carregar
            await page.WaitForSelectorAsync("main, #content, .content", new PageWaitForSelectorOptions
            {
                Timeout = 10000
            });

            // Extrair conteúdo HTML
            string html = await page.ContentAsync();
            
            await page.Context.CloseAsync();

            LoggingTask.RegistrarInfo($"✅ Página carregada com sucesso: {url}");
            return html;
        }
        catch (TimeoutException ex)
        {
            LoggingTask.RegistrarAviso($"Timeout na tentativa {tentativa}: {ex.Message}", "NavegarComRetry");
            
            if (tentativa >= maxTentativas)
            {
                LoggingTask.RegistrarErro(ex, $"Todas as tentativas falharam para {url}");
                throw;
            }

            // Aguardar antes da próxima tentativa
            await Task.Delay(_config.Scraping.DelayAposErro);
        }
    }

    throw new Exception($"Falha ao navegar para {url} após {maxTentativas} tentativas");
}
```

---

## Recursos Avançados (Opcional)

### Interceptação de Requests

```csharp
// Bloquear recursos desnecessários
await page.RouteAsync("**/*.{png,jpg,jpeg,gif,svg,css,woff,woff2}", route => route.AbortAsync());

// Interceptar e modificar requests
await page.RouteAsync("**/api/**", async route =>
{
    var headers = route.Request.Headers.ToDictionary(h => h.Key, h => h.Value);
    headers["Authorization"] = "Bearer token123";
    
    await route.ContinueAsync(new RouteContinueOptions
    {
        Headers = headers
    });
});
```

### Executar JavaScript

```csharp
// Scroll até o final da página
await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");

// Obter dados do JavaScript
string pageTitle = await page.EvaluateAsync<string>("document.title");
int articleCount = await page.EvaluateAsync<int>("document.querySelectorAll('article').length");

// Executar função complexa
var dados = await page.EvaluateAsync<dynamic>(@"
    () => {
        const articles = Array.from(document.querySelectorAll('article'));
        return articles.map(article => ({
            title: article.querySelector('h2')?.textContent,
            link: article.querySelector('a')?.href
        }));
    }
");
```

---

## Boas Práticas

### ✅ Fazer

```csharp
// Usar configurações do Config.cs
var timeout = _config.Navegacao.TimeoutSegundos * 1000;

// Aguardar elementos antes de interagir
await page.WaitForSelectorAsync("button");
await page.Locator("button").ClickAsync();

// Logar operações importantes
LoggingTask.RegistrarInfo("Iniciando extração de dados");

// Fechar contextos para liberar memória
await page.Context.CloseAsync();

// Tratar exceções específicas
try { }
catch (TimeoutException ex) { /* retry logic */ }
```

### ❌ Evitar

```csharp
// Thread.Sleep (usar Task.Delay e esperas do Playwright)
Thread.Sleep(5000); // ❌

// Hardcoded timeouts (usar Config)
await page.WaitForTimeout(30000); // ❌

// Ignorar erros sem log
catch (Exception) { } // ❌

// Deixar páginas abertas sem fechar contextos
// Sempre feche com page.Context.CloseAsync()
```

---

## Recursos Adicionais

- **Documentação Oficial:** https://playwright.dev/dotnet/
- **API Reference:** https://playwright.dev/dotnet/docs/api/class-playwright
- **Seletores:** https://playwright.dev/dotnet/docs/selectors
- **Exemplos:** https://github.com/microsoft/playwright-dotnet
- **CodeGen:** `pwsh bin\Debug\net9.0\playwright.ps1 codegen https://site.com` (gera código automaticamente)

---

**Versão:** 4.0 (Tutorial Completo)  
**Última atualização:** Novembro 2025
