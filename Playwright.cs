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

        // Log para confirmar UserAgent personalizado
        if (!string.IsNullOrEmpty(_config.Navegacao.UserAgent))
        {
            LoggingTask.RegistrarDebug($"UserAgent personalizado aplicado: {_config.Navegacao.UserAgent[..50]}...");
        }

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
    /// Maximizar janela do navegador usando Windows API (Win + Up Arrow)
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

            // Pressionar Win
            keybd_event(VK_LWIN, 0, 0, 0);
            await Task.Delay(50);

            // Pressionar Up Arrow
            keybd_event(VK_UP, 0, 0, 0);
            await Task.Delay(50);

            // Soltar Up Arrow
            keybd_event(VK_UP, 0, KEYEVENTF_KEYUP, 0);
            await Task.Delay(50);

            // Soltar Win
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
    /// Finalizar e liberar recursos do Playwright
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