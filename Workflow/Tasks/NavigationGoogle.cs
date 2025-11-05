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

            // Maximizar janela antes de navegar
            await Playwright.MaximizarJanela(page);

            // Navegar para o site de teste
            await Playwright.NavegarPara(page, "https://example.com");

            // Aguardar o título da página aparecer
            await page.WaitForSelectorAsync("h1", new PageWaitForSelectorOptions
            {
                Timeout = 10000
            });

            // Verificar se a página carregou corretamente
            var titulo = await page.TitleAsync();
            LoggingTask.RegistrarInfo($"✅ Página carregada: {titulo}");

            // Verificar o tamanho atual da janela via JavaScript
            var tamanhoJanela = await page.EvaluateAsync<dynamic>("() => ({ width: window.outerWidth, height: window.outerHeight, screen: { width: screen.width, height: screen.height } })");
            LoggingTask.RegistrarInfo($"📏 Tamanho da janela: {tamanhoJanela.width}x{tamanhoJanela.height}");
            LoggingTask.RegistrarInfo($"📺 Tamanho da tela: {tamanhoJanela.screen.width}x{tamanhoJanela.screen.height}");

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