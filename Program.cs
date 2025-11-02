using AdrenalineSpy;

namespace AdrenalineSpy
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Carregar configurações
            Config config = Config.Instancia;

            if (!config.Validar())
            {
                Console.WriteLine("❌ Configurações inválidas!");
                return;
            }

            // 2. Configurar logger
            LoggingTask.ConfigurarLogger();

            try
            {
                // 3. Usar logging
                LoggingTask.RegistrarInfo("=== Aplicação Iniciada ===");

                // Seu código aqui...

                LoggingTask.RegistrarInfo("=== Aplicação Finalizada ===");
            }
            catch (Exception ex)
            {
                LoggingTask.RegistrarErro(ex, "Program.Main");
            }
            finally
            {
                // 4. SEMPRE fechar
                LoggingTask.FecharLogger();
            }
        }
    }
}