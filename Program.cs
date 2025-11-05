using AdrenalineSpy;

namespace AdrenalineSpy
{
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
                LoggingTask.RegistrarInfo("Iniciando Workflow...");

                // TODO: Implementar Workflow.cs
                var googleTask = new NavigationGoogle();
                await googleTask.ExemploPesquisarGoogle();

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
}