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