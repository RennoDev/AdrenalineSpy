using Newtonsoft.Json;

namespace AdrenalineSpy;

/// <summary>
/// Classe responsável por carregar e gerenciar as configurações da aplicação
/// a partir do arquivo AutomationSettings.json
/// </summary>
public class Config
{
    private static Config? _instancia;
    private readonly string _caminhoArquivo = "AutomationSettings.json";

    // Propriedades principais
    public NavegacaoConfig Navegacao { get; set; } = new();
    public Dictionary<string, string> Categorias { get; set; } = new();
    public ScrapingConfig Scraping { get; set; } = new();
    public DatabaseConfig Database { get; set; } = new();
    public LoggingConfig Logging { get; set; } = new();
    public AgendamentoConfig Agendamento { get; set; } = new();
    public ExportacaoConfig Exportacao { get; set; } = new();

    // Singleton - Instância única
    public static Config Instancia
    {
        get
        {
            if (_instancia == null)
            {
                _instancia = new Config();
                _instancia.Carregar();
            }
            return _instancia;
        }
    }

    // Construtor privado (Singleton)
    private Config() { }

    /// <summary>
    /// Carrega as configurações do arquivo JSON
    /// </summary>
    public void Carregar()
    {
        try
        {
            if (!File.Exists(_caminhoArquivo))
            {
                throw new FileNotFoundException(
                    $"Arquivo de configuração não encontrado: {_caminhoArquivo}\n" +
                    $"Copie o arquivo 'AutomationSettings.example.json' para 'AutomationSettings.json' e configure."
                );
            }

            string json = File.ReadAllText(_caminhoArquivo);
            var configuracoes = JsonConvert.DeserializeObject<Config>(json);

            if (configuracoes == null)
            {
                throw new InvalidOperationException("Falha ao deserializar configurações.");
            }

            // Copiar propriedades
            Navegacao = configuracoes.Navegacao;
            Categorias = configuracoes.Categorias;
            Scraping = configuracoes.Scraping;
            Database = configuracoes.Database;
            Logging = configuracoes.Logging;
            Agendamento = configuracoes.Agendamento;
            Exportacao = configuracoes.Exportacao;

            Console.WriteLine("✅ Configurações carregadas com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao carregar configurações: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Valida se as configurações obrigatórias estão preenchidas
    /// </summary>
    public bool Validar()
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(Navegacao.UrlBase))
            erros.Add("Navegacao.UrlBase não pode estar vazia");

        if (Navegacao.TimeoutSegundos <= 0)
            erros.Add("Navegacao.TimeoutSegundos deve ser maior que zero");

        if (Categorias.Count == 0)
            erros.Add("Pelo menos uma categoria deve ser configurada");

        if (string.IsNullOrWhiteSpace(Database.NomeBanco))
            erros.Add("Database.NomeBanco não pode estar vazio");

        if (string.IsNullOrWhiteSpace(Database.Usuario))
            erros.Add("Database.Usuario não pode estar vazio");

        if (erros.Any())
        {
            Console.WriteLine("❌ Erros de validação:");
            erros.ForEach(e => Console.WriteLine($"   - {e}"));
            return false;
        }

        Console.WriteLine("✅ Configurações validadas com sucesso!");
        return true;
    }

    /// <summary>
    /// Obtém a connection string do banco de dados
    /// </summary>
    public string ObterConnectionString()
    {
        return Database.Provider.ToLower() switch
        {
            "mysql" => $"Server={Database.Host};Port={Database.Port};Database={Database.NomeBanco};" +
                      $"Uid={Database.Usuario};Pwd={Database.Senha};Connection Timeout={Database.ConnectionTimeout};",

            "postgresql" => $"Host={Database.Host};Port={Database.Port};Database={Database.NomeBanco};" +
                           $"Username={Database.Usuario};Password={Database.Senha};Timeout={Database.ConnectionTimeout};",

            "sqlserver" => $"Server={Database.Host},{Database.Port};Database={Database.NomeBanco};" +
                          $"User Id={Database.Usuario};Password={Database.Senha};Connection Timeout={Database.ConnectionTimeout};",

            _ => throw new NotSupportedException($"Provider '{Database.Provider}' não suportado")
        };
    }
}

// Classes auxiliares para organização e tipagem das configurações vindas do JSON, além de valores padrão caso o JSON não os defina.
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

public class ScrapingConfig
{
    public int IntervaloEntreRequests { get; set; } = 2000;
    public int MaximoTentativas { get; set; } = 3;
    public int DelayAposErro { get; set; } = 5000;
}

public class DatabaseConfig
{
    public string Provider { get; set; } = "MySQL";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 3306;
    public string NomeBanco { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public int ConnectionTimeout { get; set; } = 30;
}

public class LoggingConfig
{
    public string DiretorioLogs { get; set; } = "logs";
    public string NivelMinimo { get; set; } = "Information";
    public string ArquivoSucesso { get; set; } = "sucesso/log-{Date}.txt";
    public string ArquivoFalha { get; set; } = "falha/log-{Date}.txt";
}

public class AgendamentoConfig
{
    public bool HabilitarScheduler { get; set; } = true;
    public int IntervaloMinutos { get; set; } = 60;
    public bool ExecutarAoIniciar { get; set; } = false;
}

public class ExportacaoConfig
{
    public bool HabilitarExportacao { get; set; } = true;
    public string FormatoPadrao { get; set; } = "Excel";
    public string DiretorioSaida { get; set; } = "exports";
}