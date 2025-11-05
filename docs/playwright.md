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
    "BloquearCSS": false,
    "JanelaMaximizada": true
  }
}
```

**Configurações explicadas:**

- `UrlBase` - Site base para todas as navegações
- `TimeoutSegundos` - Timeout padrão para todas as operações (30s recomendado)
- `HeadlessMode` - `false` = visível (debug), `true` = invisível (produção)
- `NavegadorPadrao` - `"chromium"`, `"firefox"` ou `"webkit"`
- `ViewportWidth/Height` - Resolução da janela do navegador (ignorado se JanelaMaximizada = true)
- `UserAgent` - String personalizada do user-agent (vazio = padrão)
- `BloquearImagens` - Acelera scraping bloqueando imagens
- `BloquearCSS` - Bloquear CSS (pode quebrar layout mas é mais rápido)
- `JanelaMaximizada` - `true` = janela maximizada (modo debug), `false` = usar ViewportWidth/Height

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
    public bool JanelaMaximizada { get; set; } = true;
}

**💡 Dica sobre JanelaMaximizada:**
- Se `JanelaMaximizada = true`, usa atalho **Win + ↑** para maximizar janela automaticamente
- Funciona apenas em modo **não-headless** (HeadlessMode = false)
- **Recomendado:** true para desenvolvimento visual, false para automação em produção
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
    public static async Task<IBrowser> InicializarNavegador()
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
    public static async Task<IPage> CriarPagina()
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

        // Log da configuração de janela
        if (_config.Navegacao.JanelaMaximizada && !_config.Navegacao.HeadlessMode)
        {
            LoggingTask.RegistrarDebug("Janela configurada para usar tamanho da tela (maximizada)");
        }

        return page;
    }

    /// <summary>
    /// Configurar bloqueios de recursos (imagens, CSS) para acelerar scraping
    /// </summary>
    private static async Task ConfigurarBloqueiosRecursos(IBrowserContext context)
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
    public static async Task NavegarPara(IPage page, string url)
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
    /// Maximizar janela do navegador usando atalho Windows (Win + ↑)
    /// </summary>
    public static async Task MaximizarJanela(IPage page)
    {
        if (_config.Navegacao.HeadlessMode)
        {
            LoggingTask.RegistrarInfo("⚠️ Não é possível maximizar janela em modo headless");
            return;
        }

        if (!_config.Navegacao.JanelaMaximizada)
        {
            LoggingTask.RegistrarDebug("Maximização de janela desabilitada na configuração");
            return;
        }

        try
        {
            // Aguardar um pouco para garantir que a janela foi criada e está ativa
            await Task.Delay(800);

            // Usar P/Invoke para enviar Win + Up Arrow (maximizar janela ativa)
            LoggingTask.RegistrarInfo("🔲 Maximizando janela com Win + ↑");
            
            // Importar funções Win32
            const int VK_LWIN = 0x5B;      // Tecla Windows esquerda
            const int VK_UP = 0x26;        // Seta para cima
            const int KEYEVENTF_KEYUP = 0x02;

            // Simular pressionar Win + Up Arrow
            keybd_event(VK_LWIN, 0, 0, 0);
            await Task.Delay(50);
            keybd_event(VK_UP, 0, 0, 0);
            await Task.Delay(50);
            keybd_event(VK_UP, 0, KEYEVENTF_KEYUP, 0);
            await Task.Delay(50);
            keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, 0);

            LoggingTask.RegistrarInfo("✅ Janela maximizada usando atalho do Windows");
            
            // Aguardar para a janela se ajustar
            await Task.Delay(500);
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, "Erro ao maximizar janela do navegador");
        }
    }

    // Importar função Windows API para simulação de teclas
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    /// <summary>
    /// Finalizar e fechar todos os recursos do Playwright
    /// </summary>
    public static async Task Finalizar()
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

            // Criar página usando nossa classe Playwright e Navegar para o Google
            var page = await Playwright.CriarPagina();
            await Playwright.NavegarPara(page, "https://www.google.com.br");

            // Aguardar campo de pesquisa aparecer (usando name='q' que é mais estável)
            await page.WaitForSelectorAsync("textarea[name='q']", new PageWaitForSelectorOptions
            {
                Timeout = 10000
            });

            // Capturar elemento e digitar com delay aleatório
            var campoPesquisa = page.Locator("textarea[name='q'], input[name='q']");

            // Usar FillAsync (método recomendado pelo Playwright)
            await campoPesquisa.FillAsync("playwright RPA automation");
            LoggingTask.RegistrarInfo("✅ Texto preenchido com FillAsync");

            // Pressionar Enter para enviar a pesquisa
            await campoPesquisa.PressAsync("Enter");
            LoggingTask.RegistrarInfo("✅ Tecla Enter pressionada");

            // Pausa para visualização manual dos resultados
            LoggingTask.RegistrarInfo("📋 Pressione qualquer tecla para continuar e fechar o navegador");
            Console.ReadKey();

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

#### Usando a Estrutura do AdrenalineSpy (RECOMENDADO)

```csharp
// Usar a classe Playwright.cs (estrutura do projeto)
var page = await Playwright.CriarPagina();
await Playwright.NavegarPara(page, "https://www.adrenaline.com.br/tecnologia");

// Finalizar recursos
await page.Context.CloseAsync();
await Playwright.Finalizar();
```

#### Métodos Diretos da Page (para casos específicos)

```csharp
// Navegar diretamente (quando não usar Playwright.NavegarPara)
await page.GotoAsync("https://exemplo.com", new PageGotoOptions
{
    Timeout = _config.Navegacao.TimeoutSegundos * 1000,
    WaitUntil = WaitUntilState.NetworkIdle
});

// Voltar e avançar no histórico
await page.GoBackAsync();
await page.GoForwardAsync();

// Recarregar página
await page.ReloadAsync();
```

#### Padrão Completo nas Tasks do AdrenalineSpy

```csharp
public async Task<List<string>> MinhaTask()
{
    try
    {
        LoggingTask.RegistrarInfo("Iniciando navegação...");
        
        var page = await Playwright.CriarPagina();
        await Playwright.NavegarPara(page, _config.Navegacao.UrlBase + "/categoria");
        
        // Sua lógica aqui...
        
        await page.Context.CloseAsync();
        return resultado;
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "MinhaTask");
        throw;
    }
}
```

---

### 6.2. Como Conseguir e Configurar Seletores

#### Exemplo Prático: Seletores do Adrenaline.com.br

```csharp
public async Task<List<string>> ColetarLinksAdrenaline()
{
    var page = await Playwright.CriarPagina();
    await Playwright.NavegarPara(page, _config.Navegacao.UrlBase);
    
    // Aguardar elementos aparecerem
    await page.WaitForSelectorAsync("article", new PageWaitForSelectorOptions
    {
        Timeout = _config.Navegacao.TimeoutSegundos * 1000
    });
    
    // Coletar links (AJUSTE CONFORME HTML REAL DO ADRENALINE)
    var links = await page.Locator("article a[href*='/noticia']").AllAsync();
    
    var urls = new List<string>();
    foreach (var link in links)
    {
        var href = await link.GetAttributeAsync("href");
        if (!string.IsNullOrEmpty(href))
        {
            urls.Add(href.StartsWith("http") ? href : _config.Navegacao.UrlBase + href);
        }
    }
    
    await page.Context.CloseAsync();
    return urls;
}
```

#### Tipos de Seletores Mais Usados no AdrenalineSpy

```csharp
// 1. CSS Selector para artigos/notícias
var artigos = page.Locator("article.news-item");
var titulos = page.Locator("h1.article-title, h2.news-title");
var links = page.Locator("a[href*='/noticia'], a[href*='/review']");

// 2. Por texto visível (útil para navegação)
var menuTecnologia = page.Locator("text=Tecnologia");
var btnProxima = page.Locator("text=/Próxima|Next/i");

// 3. Por atributo específico
var dataId = page.Locator("[data-article-id]");
var categoria = page.Locator("[data-category='tecnologia']");

// 4. Combinação de seletores (mais específicos)
var linksDentroDeArtigos = page.Locator("article >> a.permalink");
var titulosComTexto = page.Locator("h2:has-text('Review')");
```

#### Como Descobrir Seletores no Adrenaline.com.br

1. **Abra o site:** https://www.adrenaline.com.br
2. **DevTools:** Pressione F12
3. **Ferramenta de seleção:** Ctrl+Shift+C (ou clique no ícone 🔍)
4. **Clique no elemento:** Artigo, título, link que você quer capturar
5. **Copie o seletor:** Botão direito no HTML → Copy → Copy selector

#### Exemplo de Debug de Seletores

```csharp
public async Task TestarSeletores()
{
    var page = await Playwright.CriarPagina();
    await Playwright.NavegarPara(page, "https://www.adrenaline.com.br");
    
    // Contar elementos para verificar se seletor funciona
    int totalArtigos = await page.Locator("article").CountAsync();
    LoggingTask.RegistrarInfo($"Total de artigos encontrados: {totalArtigos}");
    
    // Se não encontrar, testar seletores alternativos
    if (totalArtigos == 0)
    {
        totalArtigos = await page.Locator(".post, .news-item, .article").CountAsync();
        LoggingTask.RegistrarInfo($"Seletores alternativos: {totalArtigos}");
    }
    
    // Screenshot para debug visual
    await page.ScreenshotAsync(new PageScreenshotOptions
    {
        Path = "debug-adrenaline.png",
        FullPage = true
    });
    
    await page.Context.CloseAsync();
}

---

### 6.3. Cliques e Interações

#### Padrão de Cliques no AdrenalineSpy

```csharp
public async Task NavegarPorCategoria(string categoria)
{
    var page = await Playwright.CriarPagina();
    await Playwright.NavegarPara(page, _config.Navegacao.UrlBase);
    
    try
    {
        // Aguardar menu aparecer
        await page.WaitForSelectorAsync("nav.menu", new PageWaitForSelectorOptions
        {
            Timeout = _config.Navegacao.TimeoutSegundos * 1000
        });
        
        // Clique no menu da categoria
        await page.Locator($"nav.menu a:has-text('{categoria}')").ClickAsync();
        
        LoggingTask.RegistrarInfo($"✅ Navegou para categoria: {categoria}");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, $"Erro ao navegar para {categoria}");
        throw;
    }
    finally
    {
        await page.Context.CloseAsync();
    }
}
```

#### Tipos de Cliques com Timeout do Config

```csharp
// Clique simples usando timeout da configuração
await page.Locator("button.load-more").ClickAsync(new LocatorClickOptions
{
    Timeout = _config.Navegacao.TimeoutSegundos * 1000
});

// Clique duplo em artigo para abrir
await page.Locator("article.news-item").DblClickAsync();

// Clique com botão direito (para debug)
await page.Locator("article").ClickAsync(new LocatorClickOptions
{
    Button = MouseButton.Right
});

// Forçar clique quando elemento está parcialmente oculto
await page.Locator("button.hidden-menu").ClickAsync(new LocatorClickOptions
{
    Force = true
});

// Clique em coordenadas específicas (raramente usado)
await page.Locator("canvas").ClickAsync(new LocatorClickOptions
{
    Position = new Position { X = 100, Y = 50 }
});
```

#### Exemplo Real: Paginação no Adrenaline

```csharp
public async Task<List<string>> ColetarTodasPaginas(string categoria)
{
    var todasUrls = new List<string>();
    var page = await Playwright.CriarPagina();
    
    try
    {
        await Playwright.NavegarPara(page, $"{_config.Navegacao.UrlBase}/{categoria}");
        
        bool temProximaPagina = true;
        int paginaAtual = 1;
        
        while (temProximaPagina && paginaAtual <= 5) // Limite de páginas
        {
            LoggingTask.RegistrarInfo($"Processando página {paginaAtual}...");
            
            // Coletar URLs da página atual
            var urlsPagina = await ColetarUrlsPagina(page);
            todasUrls.AddRange(urlsPagina);
            
            // Tentar ir para próxima página
            try
            {
                await page.Locator("a.next, button.load-more").ClickAsync(new LocatorClickOptions
                {
                    Timeout = 5000
                });
                
                // Aguardar novo conteúdo carregar
                await Task.Delay(2000);
                paginaAtual++;
            }
            catch (TimeoutException)
            {
                temProximaPagina = false;
                LoggingTask.RegistrarInfo("Última página alcançada");
            }
        }
    }
    finally
    {
        await page.Context.CloseAsync();
    }
    
    return todasUrls;
}
```

---

### 6.4. Hover e Menus Dropdown

#### Padrão para Navegação por Menus no Adrenaline

```csharp
public async Task NavegarPorMenuDropdown(string categoria)
{
    var page = await Playwright.CriarPagina();
    await Playwright.NavegarPara(page, _config.Navegacao.UrlBase);
    
    try
    {
        LoggingTask.RegistrarDebug($"Navegando por menu: {categoria}");
        
        // Hover no menu principal
        await page.Locator("nav.main-menu").HoverAsync();
        
        // Aguardar submenu aparecer
        await page.WaitForSelectorAsync(".dropdown-menu", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = _config.Navegacao.TimeoutSegundos * 1000
        });
        
        // Clicar na categoria desejada
        await page.Locator($".dropdown-menu a:has-text('{categoria}')").ClickAsync();
        
        LoggingTask.RegistrarInfo($"✅ Menu navegado: {categoria}");
    }
    catch (TimeoutException ex)
    {
        LoggingTask.RegistrarAviso($"Menu dropdown não abriu: {categoria}", "NavigationTask");
        
        // Fallback: tentar navegação direta
        await Playwright.NavegarPara(page, $"{_config.Navegacao.UrlBase}/{categoria.ToLower()}");
    }
    finally
    {
        await page.Context.CloseAsync();
    }
}
```

#### Versão Robusta com Retry

```csharp
public async Task<bool> TentarMenuDropdownComRetry(string categoria, int maxTentativas = 3)
{
    for (int tentativa = 1; tentativa <= maxTentativas; tentativa++)
    {
        try
        {
            var page = await Playwright.CriarPagina();
            await Playwright.NavegarPara(page, _config.Navegacao.UrlBase);
            
            // Hover no elemento pai
            await page.Locator("nav.categories").HoverAsync();
            
            // Aguardar dropdown
            await page.Locator(".category-dropdown").WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 5000
            });
            
            // Clicar na categoria
            await page.Locator($"a[data-category='{categoria}']").ClickAsync();
            
            await page.Context.CloseAsync();
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Tentativa {tentativa} falhou para menu {categoria}: {ex.Message}");
            
            if (tentativa == maxTentativas)
            {
                LoggingTask.RegistrarErro(ex, $"Menu dropdown falhou após {maxTentativas} tentativas");
                return false;
            }
            
            await Task.Delay(1000 * tentativa); // Delay progressivo
        }
    }
    
    return false;
}
```

---

### 6.5. Esperas Explícitas no AdrenalineSpy

#### Aguardar Elementos do Adrenaline.com.br

```csharp
public async Task<List<NoticiaBasica>> AguardarEColetarNoticias()
{
    var page = await Playwright.CriarPagina();
    await Playwright.NavegarPara(page, _config.Navegacao.UrlBase);
    
    try
    {
        // Aguardar artigos carregarem (padrão do projeto)
        await page.WaitForSelectorAsync("article, .news-item", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = _config.Navegacao.TimeoutSegundos * 1000
        });
        
        LoggingTask.RegistrarInfo("✅ Artigos carregados, iniciando coleta...");
        
        // Aguardar spinner de loading desaparecer
        try
        {
            await page.Locator(".loading, .spinner").WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Hidden,
                Timeout = 5000
            });
        }
        catch (TimeoutException)
        {
            // Loading não existe ou já sumiu, continuar
        }
        
        // Coletar dados após tudo carregar
        var noticias = await ColetarDadosNoticias(page);
        return noticias;
    }
    finally
    {
        await page.Context.CloseAsync();
    }
}
```

#### Esperas Específicas por Estado

```csharp
// Aguardar elemento específico aparecer usando Config
await page.Locator("div.article-content").WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Visible,
    Timeout = _config.Navegacao.TimeoutSegundos * 1000
});

// Aguardar elemento existir (mesmo invisível)
await page.Locator(".hidden-metadata").WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Attached,
    Timeout = 10000
});

// Aguardar elemento desaparecer (loading)
await page.Locator(".loading-overlay").WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Hidden,
    Timeout = 15000
});

// Aguardar remoção completa do DOM
await page.Locator(".temp-notification").WaitForAsync(new LocatorWaitForOptions
{
    State = WaitForSelectorState.Detached
});
```

#### Esperas de Condições de Página no AdrenalineSpy

```csharp
public async Task AguardarPaginaCompleta(string urlEsperada)
{
    var page = await Playwright.CriarPagina();
    
    try
    {
        // Aguardar URL mudar para categoria específica
        await page.WaitForURLAsync($"**/{urlEsperada}");
        LoggingTask.RegistrarDebug($"✅ URL mudou para: {urlEsperada}");
        
        // Aguardar rede ficar ociosa (importante para SPAs)
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions
        {
            Timeout = _config.Navegacao.TimeoutSegundos * 1000
        });
        
        // Aguardar condição específica: mínimo de artigos carregados
        await page.WaitForFunctionAsync(
            "() => document.querySelectorAll('article').length >= 5",
            new PageWaitForFunctionOptions 
            { 
                Timeout = 15000 
            });
        
        LoggingTask.RegistrarInfo("✅ Página totalmente carregada");
    }
    catch (TimeoutException ex)
    {
        LoggingTask.RegistrarAviso($"Timeout ao aguardar página: {ex.Message}");
        throw;
    }
    finally
    {
        await page.Context.CloseAsync();
    }
}
```

#### Timeouts Personalizados Usando Config

```csharp
// Timeout longo para páginas lentas
await page.Locator(".heavy-content").WaitForAsync(new LocatorWaitForOptions
{
    Timeout = _config.Navegacao.TimeoutSegundos * 2000 // 2x o timeout padrão
});

// Timeout curto para verificações rápidas
await page.Locator(".error-flash").WaitForAsync(new LocatorWaitForOptions
{
    Timeout = 3000 // Apenas 3 segundos
});

// Timeout padrão do projeto
await page.Locator("main.content").WaitForAsync(new LocatorWaitForOptions
{
    Timeout = _config.Navegacao.TimeoutSegundos * 1000
});
```

#### Padrão de Retry com Esperas

```csharp
public async Task<bool> AguardarComRetry<T>(Func<Task<T>> acao, int maxTentativas = 3)
{
    for (int tentativa = 1; tentativa <= maxTentativas; tentativa++)
    {
        try
        {
            await acao();
            return true;
        }
        catch (TimeoutException)
        {
            LoggingTask.RegistrarAviso($"Timeout na tentativa {tentativa}/{maxTentativas}");
            
            if (tentativa < maxTentativas)
                await Task.Delay(2000 * tentativa); // Delay progressivo
        }
    }
    
    return false;
}

---

### 6.6. Extração de Dados do Adrenaline.com.br

#### Exemplo Completo: Extrair Dados de uma Notícia

```csharp
public async Task<NoticiaCompleta?> ExtrairNoticiaCompleta(string urlNoticia)
{
    var page = await Playwright.CriarPagina();
    
    try
    {
        await Playwright.NavegarPara(page, urlNoticia);
        
        // Aguardar conteúdo principal carregar
        await page.WaitForSelectorAsync("article, .post-content", new PageWaitForSelectorOptions
        {
            Timeout = _config.Navegacao.TimeoutSegundos * 1000
        });
        
        // Extrair dados estruturados
        var noticia = new NoticiaCompleta
        {
            Url = urlNoticia,
            
            // Título principal (vários seletores como fallback)
            Titulo = await ExtrairTextoComFallback(page, [
                "h1.post-title",
                "h1.article-title", 
                "h1",
                ".entry-title"
            ]),
            
            // Conteúdo do artigo
            ConteudoHtml = await page.Locator(".post-content, .entry-content, article .content").InnerHTMLAsync(),
            ConteudoTexto = await page.Locator(".post-content, .entry-content, article .content").InnerTextAsync(),
            
            // Metadados
            Autor = await ExtrairTextoComFallback(page, [".author", ".by-author", "[rel='author']"]),
            DataPublicacao = await ExtrairTextoComFallback(page, [".publish-date", ".date", "time"]),
            Categoria = await ExtrairTextoComFallback(page, [".category", ".tag", ".post-category"]),
            
            // Imagem destacada
            ImagemDestacada = await page.Locator(".featured-image img, .post-thumbnail img").GetAttributeAsync("src"),
            
            // Tags
            Tags = await ExtrairListaTextos(page, ".tags a, .post-tags a"),
            
            // Contadores
            Visualizacoes = await ExtrairNumero(page, ".view-count"),
            Comentarios = await ExtrairNumero(page, ".comment-count"),
            
            DataExtracao = DateTime.Now
        };
        
        LoggingTask.RegistrarInfo($"✅ Dados extraídos: {noticia.Titulo}");
        return noticia;
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, $"Erro ao extrair notícia: {urlNoticia}");
        return null;
    }
    finally
    {
        await page.Context.CloseAsync();
    }
}
```

#### Métodos Auxiliares para Extração Robusta

```csharp
/// <summary>
/// Extrair texto com múltiplos seletores como fallback
/// </summary>
private async Task<string> ExtrairTextoComFallback(IPage page, string[] seletores)
{
    foreach (var seletor in seletores)
    {
        try
        {
            var texto = await page.Locator(seletor).TextContentAsync();
            if (!string.IsNullOrWhiteSpace(texto))
                return texto.Trim();
        }
        catch
        {
            // Tentar próximo seletor
        }
    }
    
    return string.Empty;
}

/// <summary>
/// Extrair lista de textos (ex: tags, categorias)
/// </summary>
private async Task<List<string>> ExtrairListaTextos(IPage page, string seletor)
{
    try
    {
        var elementos = await page.Locator(seletor).AllAsync();
        var textos = new List<string>();
        
        foreach (var elemento in elementos)
        {
            var texto = await elemento.TextContentAsync();
            if (!string.IsNullOrWhiteSpace(texto))
                textos.Add(texto.Trim());
        }
        
        return textos;
    }
    catch
    {
        return new List<string>();
    }
}

/// <summary>
/// Extrair número de string (ex: "123 visualizações" -> 123)
/// </summary>
private async Task<int> ExtrairNumero(IPage page, string seletor)
{
    try
    {
        var texto = await page.Locator(seletor).TextContentAsync();
        if (string.IsNullOrWhiteSpace(texto)) return 0;
        
        // Extrair apenas dígitos
        var numeros = new string(texto.Where(char.IsDigit).ToArray());
        return int.TryParse(numeros, out int resultado) ? resultado : 0;
    }
    catch
    {
        return 0;
    }
}

#### Coleta em Massa de Notícias

```csharp
public async Task<List<NoticiaBasica>> ColetarTodasNoticias(string categoria)
{
    var page = await Playwright.CriarPagina();
    var todasNoticias = new List<NoticiaBasica>();
    
    try
    {
        await Playwright.NavegarPara(page, $"{_config.Navegacao.UrlBase}/{categoria}");
        
        // Aguardar artigos carregarem
        await page.WaitForSelectorAsync("article", new PageWaitForSelectorOptions
        {
            Timeout = _config.Navegacao.TimeoutSegundos * 1000
        });
        
        // Contar total de artigos disponíveis
        int totalArtigos = await page.Locator("article").CountAsync();
        LoggingTask.RegistrarInfo($"Total de artigos encontrados: {totalArtigos}");
        
        if (totalArtigos == 0)
        {
            LoggingTask.RegistrarAviso("Nenhum artigo encontrado na categoria");
            return todasNoticias;
        }
        
        // Extrair dados de todos os artigos
        var articles = await page.Locator("article").AllAsync();
        
        foreach (var article in articles)
        {
            try
            {
                var noticia = new NoticiaBasica
                {
                    Titulo = await ExtrairTextoSeguro(article, "h2, h3, .title"),
                    Url = await ExtrairLinkCompleto(article, "a"),
                    Resumo = await ExtrairTextoSeguro(article, ".excerpt, .summary"),
                    DataPublicacao = await ExtrairTextoSeguro(article, ".date, time"),
                    Categoria = categoria,
                    DataColeta = DateTime.Now
                };
                
                if (!string.IsNullOrWhiteSpace(noticia.Titulo))
                {
                    todasNoticias.Add(noticia);
                    LoggingTask.RegistrarDebug($"Coletado: {noticia.Titulo}");
                }
            }
            catch (Exception ex)
            {
                LoggingTask.RegistrarAviso($"Erro ao extrair artigo: {ex.Message}");
            }
        }
        
        LoggingTask.RegistrarInfo($"✅ Coletadas {todasNoticias.Count} notícias de {categoria}");
    }
    finally
    {
        await page.Context.CloseAsync();
    }
    
    return todasNoticias;
}

/// <summary>
/// Extrair texto com fallback para elemento não encontrado
/// </summary>
private async Task<string> ExtrairTextoSeguro(ILocator elemento, string seletor)
{
    try
    {
        return await elemento.Locator(seletor).TextContentAsync() ?? string.Empty;
    }
    catch
    {
        return string.Empty;
    }
}

/// <summary>
/// Extrair URL completa (resolve URLs relativas)
/// </summary>
private async Task<string> ExtrairLinkCompleto(ILocator elemento, string seletor)
{
    try
    {
        var href = await elemento.Locator(seletor).GetAttributeAsync("href");
        if (string.IsNullOrWhiteSpace(href)) return string.Empty;
        
        // Converter URL relativa em absoluta
        if (href.StartsWith("/"))
            return _config.Navegacao.UrlBase + href;
        
        return href.StartsWith("http") ? href : $"{_config.Navegacao.UrlBase}/{href}";
    }
    catch
    {
        return string.Empty;
    }
}
```

---

### 6.7. Busca e Formulários

#### Buscar Notícias no Adrenaline.com.br

```csharp
public async Task<List<NoticiaBasica>> BuscarNoticias(string termoBusca)
{
    var page = await Playwright.CriarPagina();
    var resultados = new List<NoticiaBasica>();
    
    try
    {
        await Playwright.NavegarPara(page, _config.Navegacao.UrlBase);
        
        // Localizar campo de busca
        await page.WaitForSelectorAsync("input[type='search'], .search-input", new PageWaitForSelectorOptions
        {
            Timeout = _config.Navegacao.TimeoutSegundos * 1000
        });
        
        // Preencher campo de busca
        await page.Locator("input[type='search'], .search-input").FillAsync(termoBusca);
        
        LoggingTask.RegistrarInfo($"Buscando por: {termoBusca}");
        
        // Pressionar Enter ou clicar no botão
        await page.Locator("input[type='search']").PressAsync("Enter");
        
        // Aguardar resultados carregarem
        await page.WaitForSelectorAsync(".search-results, .results", new PageWaitForSelectorOptions
        {
            Timeout = 15000
        });
        
        // Extrair resultados da busca
        var articles = await page.Locator(".search-result, .result-item").AllAsync();
        
        foreach (var article in articles)
        {
            var noticia = new NoticiaBasica
            {
                Titulo = await ExtrairTextoSeguro(article, "h2, h3, .title"),
                Url = await ExtrairLinkCompleto(article, "a"),
                Resumo = await ExtrairTextoSeguro(article, ".excerpt, .summary"),
                DataColeta = DateTime.Now
            };
            
            resultados.Add(noticia);
        }
        
        LoggingTask.RegistrarInfo($"✅ Encontradas {resultados.Count} notícias para '{termoBusca}'");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, $"Erro ao buscar: {termoBusca}");
    }
    finally
    {
        await page.Context.CloseAsync();
    }
    
    return resultados;
}
```

#### Preenchimento Avançado de Formulários

**🎯 Qual método usar quando?**

- **`FillAsync`** ✅ - Para preenchimento rápido e confiável (95% dos casos)
- **`PressSequentiallyAsync`** ⚠️ - Apenas quando precisar de delay para stealth
- **`TypeAsync`** ❌ - Obsoleto (não usar mais)

```csharp
// ✅ MÉTODO RECOMENDADO: FillAsync (rápido e confiável)
await page.Locator("input#search").FillAsync(""); // limpar
await page.Locator("input#search").FillAsync("Tecnologia");

// ✅ Para campos que precisam de delay (comportamento stealth)
// Use PressSequentiallyAsync quando necessário
await page.Locator("input#search").PressSequentiallyAsync("Playwright RPA", new LocatorPressSequentiallyOptions
{
    Delay = Random.Shared.Next(80, 200) // Entre 80-200ms
});

// Pressionar teclas especiais
await page.Locator("input").PressAsync("Enter");
await page.Locator("input").PressAsync("Tab");
await page.Locator("input").PressAsync("Escape");

// Combinações de teclas úteis
await page.Locator("input").PressAsync("Control+A"); // Selecionar tudo
await page.Locator("input").PressAsync("Control+C"); // Copiar
await page.Locator("input").PressAsync("Control+V"); // Colar

// Combinações de teclas
await page.Locator("input").PressAsync("Control+A"); // Selecionar tudo
await page.Locator("input").PressAsync("Control+C"); // Copiar
```

---

### 6.8. Verificações e Validações no AdrenalineSpy

#### Validações de Página e Conteúdo

```csharp
public async Task<bool> ValidarPaginaAdrenaline(string categoria)
{
    var page = await Playwright.CriarPagina();
    
    try
    {
        await Playwright.NavegarPara(page, $"{_config.Navegacao.UrlBase}/{categoria}");
        
        // Verificar se carregou corretamente
        bool paginaValida = await page.Locator("article, .news-item").CountAsync() > 0;
        
        if (!paginaValida)
        {
            LoggingTask.RegistrarAviso($"Página {categoria} não contém artigos");
            return false;
        }
        
        // Verificar se não é página de erro
        bool temErro = await page.Locator(".error, .not-found, .404").IsVisibleAsync();
        if (temErro)
        {
            string mensagemErro = await page.Locator(".error, .not-found").TextContentAsync();
            LoggingTask.RegistrarErro(new Exception($"Página de erro: {mensagemErro}"), "ValidarPagina");
            return false;
        }
        
        // Verificar título da página
        string titulo = await page.TitleAsync();
        bool tituloValido = !string.IsNullOrWhiteSpace(titulo) && !titulo.Contains("Error");
        
        LoggingTask.RegistrarInfo($"✅ Página válida - Título: {titulo}");
        return paginaValida && !temErro && tituloValido;
    }
    finally
    {
        await page.Context.CloseAsync();
    }
}
```

#### Verificações Condicionais com Logging

```csharp
public async Task VerificarElementosOpcionais(IPage page)
{
    // Verificar se botão "Load More" está disponível
    bool temLoadMore = await page.Locator("button.load-more, .pagination").IsVisibleAsync();
    if (temLoadMore)
    {
        LoggingTask.RegistrarDebug("Paginação disponível na página");
    }
    
    // Verificar se há notificações ou alertas
    if (await page.Locator(".alert, .notification").IsVisibleAsync())
    {
        string mensagem = await page.Locator(".alert, .notification").TextContentAsync();
        LoggingTask.RegistrarInfo($"Notificação na página: {mensagem}");
    }
    
    // Verificar se conteúdo está carregando
    bool carregando = await page.Locator(".loading, .spinner").IsVisibleAsync();
    if (carregando)
    {
        LoggingTask.RegistrarDebug("Aguardando conteúdo carregar...");
        
        // Aguardar loading desaparecer
        await page.Locator(".loading, .spinner").WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = 10000
        });
    }
}

---

### 6.9. Screenshots e Debug

#### Sistema de Screenshots Automático

```csharp
public class DebugHelper
{
    private readonly Config _config;
    private static int _contadorScreenshots = 0;
    
    public DebugHelper()
    {
        _config = Config.Instancia;
    }
    
    /// <summary>
    /// Capturar screenshot com nome automático e timestamp
    /// </summary>
    public async Task CapturarScreenshot(IPage page, string contexto)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var contador = Interlocked.Increment(ref _contadorScreenshots);
            var nomeArquivo = $"debug-{contexto}-{timestamp}-{contador:D3}.png";
            
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine("Screenshots", nomeArquivo),
                FullPage = true
            });
            
            LoggingTask.RegistrarDebug($"Screenshot capturada: {nomeArquivo}");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Erro ao capturar screenshot: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Screenshot automático em caso de erro
    /// </summary>
    public async Task ExecutarComScreenshotDeErro(IPage page, Func<Task> acao, string contexto)
    {
        try
        {
            await acao();
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, $"Erro em {contexto}");
            await CapturarScreenshot(page, $"erro-{contexto}");
            throw; // Re-propagar o erro
        }
    }
}
```

#### Uso Prático no NavigationTask

```csharp
public async Task<List<string>> ColetarUrlsComDebug(string categoria)
{
    var page = await Playwright.CriarPagina();
    var debugHelper = new DebugHelper();
    
    try
    {
        await Playwright.NavegarPara(page, $"{_config.Navegacao.UrlBase}/{categoria}");
        
        // Screenshot da página inicial
        await debugHelper.CapturarScreenshot(page, $"pagina-{categoria}");
        
        // Executar coleta com screenshot automático em caso de erro
        var urls = new List<string>();
        
        await debugHelper.ExecutarComScreenshotDeErro(page, async () =>
        {
            await page.WaitForSelectorAsync("article");
            var links = await page.Locator("article a").AllAsync();
            
            foreach (var link in links)
            {
                var href = await link.GetAttributeAsync("href");
                if (!string.IsNullOrEmpty(href))
                    urls.Add(href);
            }
        }, $"coleta-{categoria}");
        
        return urls;
    }
    finally
    {
        await page.Context.CloseAsync();
    }
}
---

### 6.10. Exemplo Completo: Task de Coleta Robusta

```csharp
/// <summary>
/// Task completa de coleta usando todos os padrões do AdrenalineSpy
/// </summary>
public class NavigationTaskCompleta
{
    private readonly Config _config;
    private readonly DebugHelper _debugHelper;
    
    public NavigationTaskCompleta()
    {
        _config = Config.Instancia;
        _debugHelper = new DebugHelper();
    }
    
    /// <summary>
    /// Coleta robusta com retry, logging e screenshots
    /// </summary>
    public async Task<List<NoticiaCompleta>> ColetarNoticiasCompletas(string categoria, int maxPaginas = 3)
    {
        var todasNoticias = new List<NoticiaCompleta>();
        
        try
        {
            LoggingTask.RegistrarInfo($"=== Iniciando coleta completa: {categoria} ===");
            
            // Validar categoria antes de começar
            if (!await ValidarCategoriaExiste(categoria))
            {
                LoggingTask.RegistrarAviso($"Categoria {categoria} não existe");
                return todasNoticias;
            }
            
            // Coletar URLs de múltiplas páginas
            var urls = await ColetarUrlsMultiplasPaginas(categoria, maxPaginas);
            LoggingTask.RegistrarInfo($"URLs coletadas: {urls.Count}");
            
            // Processar cada URL com controle de paralelismo
            var semaforo = new SemaphoreSlim(2); // Máximo 2 páginas simultâneas
            var tasks = urls.Select(async url =>
            {
                await semaforo.WaitAsync();
                try
                {
                    return await ExtrairNoticiaComRetry(url, maxTentativas: 3);
                }
                finally
                {
                    semaforo.Release();
                }
            });
            
            var resultados = await Task.WhenAll(tasks);
            todasNoticias = resultados.Where(n => n != null).ToList();
            
            LoggingTask.RegistrarInfo($"✅ Coleta finalizada: {todasNoticias.Count} notícias extraídas");
            return todasNoticias;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, $"Erro na coleta completa de {categoria}");
            return todasNoticias;
        }
    }
    
    /// <summary>
    /// Validar se categoria existe no site
    /// </summary>
    private async Task<bool> ValidarCategoriaExiste(string categoria)
    {
        var page = await Playwright.CriarPagina();
        try
        {
            await Playwright.NavegarPara(page, $"{_config.Navegacao.UrlBase}/{categoria}");
            
            // Verificar se não é página 404
            bool isError = await page.Locator(".error-404, .not-found").CountAsync() > 0;
            return !isError;
        }
        finally
        {
            await page.Context.CloseAsync();
        }
    }
    
    /// <summary>
    /// Extrair notícia com retry automático
    /// </summary>
    private async Task<NoticiaCompleta?> ExtrairNoticiaComRetry(string url, int maxTentativas = 3)
    {
        for (int tentativa = 1; tentativa <= maxTentativas; tentativa++)
        {
            var page = await Playwright.CriarPagina();
            
            try
            {
                await _debugHelper.ExecutarComScreenshotDeErro(page, async () =>
                {
                    await Playwright.NavegarPara(page, url);
                    await page.WaitForSelectorAsync("article, .post", new PageWaitForSelectorOptions
                    {
                        Timeout = _config.Navegacao.TimeoutSegundos * 1000
                    });
                }, $"extracao-tentativa-{tentativa}");
                
                var noticia = await ExtrairNoticiaCompleta(page, url);
                LoggingTask.RegistrarDebug($"✅ Extraída: {noticia?.Titulo}");
                
                return noticia;
            }
            catch (Exception ex)
            {
                LoggingTask.RegistrarAviso($"Tentativa {tentativa} falhou para {url}: {ex.Message}");
                
                if (tentativa == maxTentativas)
                {
                    LoggingTask.RegistrarErro(ex, $"Falha definitiva: {url}");
                    return null;
                }
                
                await Task.Delay(2000 * tentativa); // Delay progressivo
            }
            finally
            {
                await page.Context.CloseAsync();
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Finalizar todos os recursos do Playwright
    /// </summary>
    public async Task Finalizar()
    {
        await Playwright.Finalizar();
        LoggingTask.RegistrarInfo("NavigationTaskCompleta finalizada");
    }
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

## Boas Práticas do AdrenalineSpy

### ✅ SEMPRE Fazer (Padrões do Projeto)

```csharp
// 1. Usar a classe Playwright.cs centralizada
var page = await Playwright.CriarPagina();
await Playwright.NavegarPara(page, url);

// 2. Usar configurações do Config.cs
var timeout = _config.Navegacao.TimeoutSegundos * 1000;

// 3. Logging completo com LoggingTask
LoggingTask.RegistrarInfo("Iniciando navegação");
LoggingTask.RegistrarErro(ex, "Contexto do erro");

// 4. SEMPRE fechar contextos
await page.Context.CloseAsync();
await Playwright.Finalizar(); // No final da aplicação

// 5. Aguardar elementos antes de interagir
await page.WaitForSelectorAsync("article");
var elementos = await page.Locator("article").AllAsync();

// 6. Tratar exceções específicas
try { }
catch (TimeoutException ex) { /* retry */ }
catch (Exception ex) { LoggingTask.RegistrarErro(ex, "Contexto"); }

// 7. Usar try-finally para limpeza garantida
try
{
    // operações
}
finally
{
    await page.Context.CloseAsync();
}
```

### ❌ NUNCA Fazer

```csharp
// 1. Usar page.GotoAsync diretamente
await page.GotoAsync(url); // ❌ Use Playwright.NavegarPara()

// 2. Thread.Sleep ou delays fixos
Thread.Sleep(5000); // ❌ Use esperas explícitas do Playwright

// 3. Métodos obsoletos de digitação
await page.Locator("input").TypeAsync("texto"); // ❌ Use FillAsync ou PressSequentiallyAsync

// 4. Hardcoded timeouts
await page.WaitForTimeout(30000); // ❌ Use _config.Navegacao.TimeoutSegundos

// 4. Ignorar erros
catch (Exception) { } // ❌ SEMPRE logar com LoggingTask

// 5. Deixar recursos abertos
// ❌ Sempre feche: page.Context.CloseAsync() + Playwright.Finalizar()

// 6. Criar múltiplas instâncias do navegador
var browser1 = await Playwright.CreateAsync(); // ❌
var browser2 = await Playwright.CreateAsync(); // ❌
// Use a instância centralizada da classe Playwright

// 7. Seletores genéricos
page.Locator("div"); // ❌ Seja específico: "article.news-item"
```

### 💡 Dicas Específicas para Adrenaline.com.br

```csharp
// 1. Sempre aguardar artigos carregarem
await page.WaitForSelectorAsync("article", new PageWaitForSelectorOptions
{
    Timeout = _config.Navegacao.TimeoutSegundos * 1000
});

// 2. Usar múltiplos seletores como fallback
string titulo = await ExtrairTextoComFallback(page, [
    "h1.post-title", "h1", ".title"
]);

// 3. Verificar se é página de erro
bool isError = await page.Locator(".error-404, .not-found").CountAsync() > 0;

// 4. Screenshots automáticos para debug
await debugHelper.CapturarScreenshot(page, "contexto");

// 5. Controlar paralelismo
var semaforo = new SemaphoreSlim(2); // Máximo 2 páginas simultâneas

// 6. Janela maximizada para debug visual
// Configure no AutomationSettings.json: "JanelaMaximizada": true
// Funciona apenas em modo não-headless (HeadlessMode: false)
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
