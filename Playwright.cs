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

            // Configurar opções do navegador
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

        // Aplicar bloqueios de recursos se configurado
        await ConfigurarBloqueiosRecursos(context);

        var page = await context.NewPageAsync();

        // Configurar timeout padrão para todas as operações da página
        page.SetDefaultTimeout(_config.Navegacao.TimeoutSegundos * 1000);

        LoggingTask.RegistrarDebug("Nova página criada com configurações aplicadas");
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
    /// Aguardar elemento com timeout da configuração
    /// </summary>
    public static async Task AguardarElemento(IPage page, string seletor, int? timeoutPersonalizado = null)
    {
        var timeout = timeoutPersonalizado ?? _config.Navegacao.TimeoutSegundos * 1000;

        await page.WaitForSelectorAsync(seletor, new PageWaitForSelectorOptions
        {
            Timeout = timeout,
            State = WaitForSelectorState.Visible
        });
    }

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

    /// <summary>
    /// Capturar screenshot para debug com configuração automática
    /// </summary>
    public static async Task CapturarScreenshotDebug(IPage page, string nomeArquivo, bool paginaCompleta = true)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var path = $"debug_{timestamp}_{nomeArquivo}.png";

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = path,
                FullPage = paginaCompleta
            });

            LoggingTask.RegistrarDebug($"Screenshot salvo: {path}");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, "Playwright.CapturarScreenshotDebug");
        }
    }

    /// <summary>
    /// Obter configuração atual para debug
    /// </summary>
    public static object ObterConfiguracaoAtual()
    {
        return new
        {
            NavegadorAtivo = _browser != null,
            PlaywrightAtivo = _playwright != null,
            Configuracao = new
            {
                _config.Navegacao.NavegadorPadrao,
                _config.Navegacao.HeadlessMode,
                _config.Navegacao.TimeoutSegundos,
                _config.Navegacao.ViewportWidth,
                _config.Navegacao.ViewportHeight,
                _config.Navegacao.BloquearImagens,
                _config.Navegacao.BloquearCSS
            }
        };
    }
}