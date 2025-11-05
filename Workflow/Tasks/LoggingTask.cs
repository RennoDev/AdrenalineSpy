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