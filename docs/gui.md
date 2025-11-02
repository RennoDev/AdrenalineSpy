# GUI - Interface Gráfica do Usuário

## O que é GUI

**GUI (Graphical User Interface)** são as tecnologias .NET para criar interfaces visuais desktop que permitam ao usuário interagir com o sistema através de janelas, botões e controles gráficos.

**Onde é usado no AdrenalineSpy:**
- Interface principal para controlar o scraping manualmente
- Painel de monitoramento em tempo real da coleta
- Configuração visual das opções de automação
- Visualização das notícias coletadas em grids
- Controle de agendamento e execução automática
- Dashboard com estatísticas e gráficos de progresso
- Interface para gerar e visualizar relatórios

**Tecnologias disponíveis**: WPF, WinForms, Avalonia, Terminal.Gui, Electron.NET

## Como Instalar

### Opção 1: WPF (Recomendado - Windows apenas)

```powershell
# WPF já vem integrado no .NET 9
# Apenas habilitar no .csproj
```

### Opção 2: Avalonia (Multiplataforma)

```powershell
dotnet add package Avalonia
dotnet add package Avalonia.Desktop
dotnet add package Avalonia.Themes.Fluent
```

### Opção 3: Terminal.Gui (Console avançado)

```powershell
dotnet add package Terminal.Gui
```

### Verificar .csproj (WPF)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <OutputType>WinExe</OutputType>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="MaterialDesignThemes" Version="4.9.0" />
  </ItemGroup>
</Project>
```

## Implementar no AutomationSettings.json

Adicione configurações de interface na raiz do JSON:

```json
{
  "Navegacao": {
    "UrlBase": "https://www.adrenaline.com.br",
    "DelayEntrePaginas": 2000
  },
  "InterfaceUsuario": {
    "TecnologiaGUI": "WPF",
    "TemaVisual": "Dark",
    "IdiomaInterface": "pt-BR",
    "AtualizacaoTempo": 2000,
    "ConfiguracaoJanela": {
      "TituloAplicacao": "AdrenalineSpy - Monitor de Notícias",
      "LarguraInicial": 1200,
      "AlturaInicial": 800,
      "PosicaoInicial": "Center",
      "Redimensionavel": true,
      "MinimizarParaSistray": true
    },
    "FuncionalidadesInterface": {
      "HabilitarModoEscuro": true,
      "ExibirLogsTempo": true,
      "NotificacoesDesktop": true,
      "AutoSalvarConfiguracoes": true,
      "ExibirProgressoDetalhado": true,
      "HabilitarControleManual": true
    },
    "MonitoramentoTempo": {
      "AtualizarEstatisticas": 5000,
      "AtualizarGrid": 3000,
      "AtualizarGraficos": 10000,
      "TimeoutOperacoes": 30000
    }
  },
  "Database": {
    "ConnectionString": "Server=localhost;Database=AdrenalineSpy;..."
  },
  "Logging": {
    "Nivel": "Information",
    "CaminhoArquivo": "logs/adrenaline-spy.log"
  }
}
```

**Configurações específicas da GUI:**
- **`TecnologiaGUI`**: WPF, Avalonia, Terminal.Gui ou WinForms
- **`ConfiguracaoJanela`**: Aparência e comportamento da janela principal
- **`FuncionalidadesInterface`**: Recursos disponíveis na interface
- **`MonitoramentoTempo`**: Frequência de atualização dos componentes

## Implementar no Config.cs

Adicione classes de configuração para interface:

```csharp
public class ConfiguracaoJanelaConfig
{
    public string TituloAplicacao { get; set; } = "AdrenalineSpy - Monitor de Notícias";
    public int LarguraInicial { get; set; } = 1200;
    public int AlturaInicial { get; set; } = 800;
    public string PosicaoInicial { get; set; } = "Center";
    public bool Redimensionavel { get; set; } = true;
    public bool MinimizarParaSistray { get; set; } = true;
}

public class FuncionalidadesInterfaceConfig
{
    public bool HabilitarModoEscuro { get; set; } = true;
    public bool ExibirLogsTempo { get; set; } = true;
    public bool NotificacoesDesktop { get; set; } = true;
    public bool AutoSalvarConfiguracoes { get; set; } = true;
    public bool ExibirProgressoDetalhado { get; set; } = true;
    public bool HabilitarControleManual { get; set; } = true;
}

public class MonitoramentoTempoConfig
{
    public int AtualizarEstatisticas { get; set; } = 5000;
    public int AtualizarGrid { get; set; } = 3000;
    public int AtualizarGraficos { get; set; } = 10000;
    public int TimeoutOperacoes { get; set; } = 30000;
}

public class InterfaceUsuarioConfig
{
    public string TecnologiaGUI { get; set; } = "WPF";
    public string TemaVisual { get; set; } = "Dark";
    public string IdiomaInterface { get; set; } = "pt-BR";
    public int AtualizacaoTempo { get; set; } = 2000;
    public ConfiguracaoJanelaConfig ConfiguracaoJanela { get; set; } = new();
    public FuncionalidadesInterfaceConfig FuncionalidadesInterface { get; set; } = new();
    public MonitoramentoTempoConfig MonitoramentoTempo { get; set; } = new();
}

public class Config
{
    // ... propriedades existentes ...
    public InterfaceUsuarioConfig InterfaceUsuario { get; set; } = new();
    
    /// <summary>
    /// Verifica se deve executar em modo GUI
    /// </summary>
    public bool ModoGUIHabilitado()
    {
        try
        {
            return !string.IsNullOrEmpty(InterfaceUsuario.TecnologiaGUI) &&
                   InterfaceUsuario.TecnologiaGUI.ToUpper() != "CONSOLE";
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Obtém configurações da janela principal
    /// </summary>
    public (int width, int height, string position) ObterDimensoesJanela()
    {
        var config = InterfaceUsuario.ConfiguracaoJanela;
        return (config.LarguraInicial, config.AlturaInicial, config.PosicaoInicial);
    }
}
```

## Montar nas Tasks

Crie a classe `GUITask.cs` na pasta `Workflow/Tasks/`:

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;

namespace AdrenalineSpy.Workflow.Tasks;

/// <summary>
/// Gerencia interface gráfica do AdrenalineSpy
/// </summary>
public static class GUITask
{
    private static MainWindow? _janelaPrincipal;
    private static readonly DispatcherTimer _timerAtualizacao = new();
    private static bool _scrapingEmAndamento = false;

    /// <summary>
    /// Inicializa e exibe interface gráfica principal
    /// </summary>
    public static async Task<bool> InicializarInterface()
    {
        try
        {
            if (!Config.Instancia.ModoGUIHabilitado())
            {
                LoggingTask.RegistrarInfo("🖥️ Modo GUI desabilitado, executando em console");
                return false;
            }

            LoggingTask.RegistrarInfo("🖼️ Inicializando interface gráfica WPF");

            Application.Current.Dispatcher.Invoke(() =>
            {
                _janelaPrincipal = new MainWindow();
                
                var (width, height, position) = Config.Instancia.ObterDimensoesJanela();
                _janelaPrincipal.Width = width;
                _janelaPrincipal.Height = height;
                
                if (position == "Center")
                {
                    _janelaPrincipal.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }

                _janelaPrincipal.Title = Config.Instancia.InterfaceUsuario.ConfiguracaoJanela.TituloAplicacao;
                
                ConfigurarEventosInterface();
                IniciarMonitoramentoAutomatico();
                
                _janelaPrincipal.Show();
            });

            LoggingTask.RegistrarInfo("✅ Interface gráfica inicializada com sucesso");
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao inicializar interface gráfica", ex);
            return false;
        }
    }

    /// <summary>
    /// Configura eventos da interface
    /// </summary>
    private static void ConfigurarEventosInterface()
    {
        if (_janelaPrincipal == null) return;

        // Evento de iniciar scraping manual
        _janelaPrincipal.BotaoIniciar.Click += async (s, e) =>
        {
            await IniciarScrapingManual();
        };

        // Evento de parar scraping
        _janelaPrincipal.BotaoParar.Click += (s, e) =>
        {
            PararScraping();
        };

        // Evento de gerar relatórios
        _janelaPrincipal.BotaoRelatorios.Click += async (s, e) =>
        {
            await GerarRelatoriosManual();
        };

        // Evento de fechar aplicação
        _janelaPrincipal.Closing += (s, e) =>
        {
            SalvarConfiguracoes();
        };
    }

    /// <summary>
    /// Inicia monitoramento automático da interface
    /// </summary>
    private static void IniciarMonitoramentoAutomatico()
    {
        _timerAtualizacao.Interval = TimeSpan.FromMilliseconds(
            Config.Instancia.InterfaceUsuario.AtualizacaoTempo);
        
        _timerAtualizacao.Tick += async (s, e) =>
        {
            await AtualizarInterface();
        };
        
        _timerAtualizacao.Start();
        LoggingTask.RegistrarInfo("⏰ Monitoramento automático da interface iniciado");
    }

    /// <summary>
    /// Atualiza componentes da interface periodicamente
    /// </summary>
    private static async Task AtualizarInterface()
    {
        try
        {
            if (_janelaPrincipal == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Atualizar status na interface
                _janelaPrincipal.LabelStatus.Content = _scrapingEmAndamento 
                    ? "🔄 Coletando notícias..." 
                    : "⏸️ Aguardando";

                // Atualizar logs recentes
                AtualizarLogsRecentes();

                // Atualizar estatísticas
                AtualizarEstatisticas();
            });
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Aviso na atualização da interface: {ex.Message}");
        }
    }

    /// <summary>
    /// Inicia processo de scraping via interface
    /// </summary>
    public static async Task<bool> IniciarScrapingManual()
    {
        try
        {
            if (_scrapingEmAndamento)
            {
                ExibirMensagem("⚠️ Scraping já está em andamento!");
                return false;
            }

            LoggingTask.RegistrarInfo("▶️ Iniciando scraping via interface manual");
            _scrapingEmAndamento = true;

            // Atualizar interface para modo execução
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_janelaPrincipal != null)
                {
                    _janelaPrincipal.BotaoIniciar.IsEnabled = false;
                    _janelaPrincipal.BotaoParar.IsEnabled = true;
                    _janelaPrincipal.ProgressBar.Visibility = Visibility.Visible;
                }
            });

            // Executar workflow completo
            var workflow = new Workflow.Workflow();
            var sucesso = await workflow.ExecutarScrapingCompleto();

            _scrapingEmAndamento = false;

            // Restaurar interface
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_janelaPrincipal != null)
                {
                    _janelaPrincipal.BotaoIniciar.IsEnabled = true;
                    _janelaPrincipal.BotaoParar.IsEnabled = false;
                    _janelaPrincipal.ProgressBar.Visibility = Visibility.Collapsed;
                }
            });

            var mensagem = sucesso 
                ? "✅ Scraping concluído com sucesso!"
                : "❌ Erro durante o scraping. Verifique os logs.";
            
            ExibirMensagem(mensagem);
            
            LoggingTask.RegistrarInfo($"🏁 Scraping manual finalizado: {(sucesso ? "sucesso" : "erro")}");
            return sucesso;
        }
        catch (Exception ex)
        {
            _scrapingEmAndamento = false;
            LoggingTask.RegistrarErro("Erro no scraping manual via interface", ex);
            ExibirMensagem($"❌ Erro: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Para processo de scraping em andamento
    /// </summary>
    public static void PararScraping()
    {
        try
        {
            if (!_scrapingEmAndamento)
            {
                ExibirMensagem("⚠️ Nenhum scraping em andamento!");
                return;
            }

            LoggingTask.RegistrarInfo("⏹️ Solicitação de parada do scraping via interface");
            _scrapingEmAndamento = false;

            // TODO: Implementar cancelamento do workflow
            ExibirMensagem("⏹️ Solicitação de parada enviada");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao parar scraping", ex);
        }
    }

    /// <summary>
    /// Gera relatórios manualmente via interface
    /// </summary>
    public static async Task<bool> GerarRelatoriosManual()
    {
        try
        {
            LoggingTask.RegistrarInfo("📊 Gerando relatórios via interface manual");

            // Buscar notícias recentes do banco
            var noticias = await ObterNoticiasRecentes();
            
            if (!noticias.Any())
            {
                ExibirMensagem("⚠️ Nenhuma notícia encontrada para gerar relatórios!");
                return false;
            }

            // Gerar relatórios
            var tasks = new List<Task<bool>>();
            var dataExecucao = DateTime.Now;

            if (Config.Instancia.Relatorios.HabilitarExportacaoCSV)
            {
                tasks.Add(CsvExportTask.ExportarNoticias(noticias));
            }

            if (Config.Instancia.Relatorios.HabilitarRelatorioExcel)
            {
                tasks.Add(ExcelReportTask.GerarRelatorioCompleto(noticias, dataExecucao));
            }

            if (Config.Instancia.Relatorios.HabilitarRelatorioPDF)
            {
                tasks.Add(PDFReportTask.GerarRelatorioPDF(noticias, dataExecucao));
            }

            var resultados = await Task.WhenAll(tasks);
            var sucessos = resultados.Count(r => r);

            var mensagem = $"📊 Relatórios gerados: {sucessos}/{tasks.Count} formatos";
            ExibirMensagem(mensagem);
            
            LoggingTask.RegistrarInfo(mensagem);
            return sucessos > 0;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao gerar relatórios via interface", ex);
            ExibirMensagem($"❌ Erro ao gerar relatórios: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Atualiza logs recentes na interface
    /// </summary>
    private static void AtualizarLogsRecentes()
    {
        try
        {
            if (_janelaPrincipal?.ListaLogs == null) return;

            // TODO: Implementar leitura dos logs mais recentes
            // var logsRecentes = LoggingTask.ObterLogsRecentes(10);
            // _janelaPrincipal.ListaLogs.ItemsSource = logsRecentes;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Erro ao atualizar logs na interface: {ex.Message}");
        }
    }

    /// <summary>
    /// Atualiza estatísticas na interface
    /// </summary>
    private static void AtualizarEstatisticas()
    {
        try
        {
            if (_janelaPrincipal == null) return;

            // TODO: Buscar estatísticas do banco
            // var estatisticas = await ObterEstatisticasRecentes();
            // _janelaPrincipal.LabelTotalNoticias.Content = $"Total: {estatisticas.Total}";
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Erro ao atualizar estatísticas: {ex.Message}");
        }
    }

    /// <summary>
    /// Exibe mensagem para o usuário
    /// </summary>
    private static void ExibirMensagem(string mensagem)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBox.Show(mensagem, "AdrenalineSpy", MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }

    /// <summary>
    /// Salva configurações da interface
    /// </summary>
    private static void SalvarConfiguracoes()
    {
        try
        {
            if (Config.Instancia.InterfaceUsuario.FuncionalidadesInterface.AutoSalvarConfiguracoes)
            {
                // TODO: Salvar posição da janela, preferências do usuário, etc.
                LoggingTask.RegistrarInfo("💾 Configurações da interface salvas");
            }
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Aviso ao salvar configurações: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtém notícias recentes do banco para relatórios
    /// </summary>
    private static async Task<List<Noticia>> ObterNoticiasRecentes()
    {
        try
        {
            // TODO: Implementar busca no banco
            // return await MigrationTask.BuscarNoticiasRecentes(100);
            
            // Temporário: retornar lista vazia
            return new List<Noticia>();
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao buscar notícias recentes", ex);
            return new List<Noticia>();
        }
    }

    /// <summary>
    /// Finaliza interface e libera recursos
    /// </summary>
    public static void FinalizarInterface()
    {
        try
        {
            _timerAtualizacao?.Stop();
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                _janelaPrincipal?.Close();
            });
            
            LoggingTask.RegistrarInfo("🔚 Interface gráfica finalizada");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Aviso ao finalizar interface: {ex.Message}");
        }
    }
}

/// <summary>
/// Classe auxiliar para dados da interface
/// </summary>
public class NoticiaViewModel : INotifyPropertyChanged
{
    public string Titulo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public DateTime DataPublicacao { get; set; }
    public string Url { get; set; } = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

## Métodos Mais Usados

### Inicializar Aplicação WPF

```csharp
using System.Windows;

// No Program.cs - modo gráfico
[STAThread]
public static void Main(string[] args)
{
    var app = new Application();
    
    // Verificar se deve usar GUI
    if (Config.Instancia.ModoGUIHabilitado())
    {
        LoggingTask.RegistrarInfo("🖼️ Iniciando em modo gráfico");
        
        // Inicializar interface
        await GUITask.InicializarInterface();
        
        // Executar loop da aplicação WPF
        app.Run();
    }
    else
    {
        LoggingTask.RegistrarInfo("💻 Executando em modo console");
        
        // Executar workflow direto
        var workflow = new Workflow.Workflow();
        await workflow.ExecutarScrapingCompleto();
    }
}
```

### Criar Janela Principal WPF

```csharp
// MainWindow.xaml.cs
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ConfigurarInterface();
    }

    private void ConfigurarInterface()
    {
        Title = Config.Instancia.InterfaceUsuario.ConfiguracaoJanela.TituloAplicacao;
        
        // Aplicar tema escuro se configurado
        if (Config.Instancia.InterfaceUsuario.FuncionalidadesInterface.HabilitarModoEscuro)
        {
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 48));
        }
    }
}
```

### Atualizar Interface em Tempo Real

```csharp
// Timer para atualizações automáticas
private static void ConfigurarAtualizacaoAutomatica()
{
    var timer = new DispatcherTimer();
    timer.Interval = TimeSpan.FromMilliseconds(Config.Instancia.InterfaceUsuario.AtualizacaoTempo);
    
    timer.Tick += async (s, e) =>
    {
        // Atualizar status na UI thread
        Application.Current.Dispatcher.Invoke(() =>
        {
            labelStatus.Content = _scrapingAtivo ? "🔄 Executando..." : "⏸️ Parado";
        });
        
        // Atualizar dados
        await AtualizarEstatisticas();
    };
    
    timer.Start();
    LoggingTask.RegistrarInfo("⏰ Atualização automática configurada");
}
```

### Controles de Execução Manual

```csharp
// Botão para iniciar scraping
private async void BotaoIniciar_Click(object sender, RoutedEventArgs e)
{
    try
    {
        // Desabilitar botão durante execução
        BotaoIniciar.IsEnabled = false;
        ProgressBar.Visibility = Visibility.Visible;
        
        LoggingTask.RegistrarInfo("▶️ Scraping iniciado via interface");
        
        // Executar workflow
        var sucesso = await GUITask.IniciarScrapingManual();
        
        // Exibir resultado
        var mensagem = sucesso ? "✅ Sucesso!" : "❌ Erro - verifique logs";
        MessageBox.Show(mensagem, "Resultado", MessageBoxButton.OK);
        
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro("Erro no botão iniciar", ex);
        MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    finally
    {
        // Restaurar interface
        BotaoIniciar.IsEnabled = true;
        ProgressBar.Visibility = Visibility.Collapsed;
    }
}
```

### Grid de Notícias com Binding

```csharp
// ViewModel para binding de dados
public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<NoticiaViewModel> Noticias { get; set; } = new();
    
    public async Task CarregarNoticias()
    {
        try
        {
            var noticias = await MigrationTask.BuscarNoticiasRecentes(50);
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                Noticias.Clear();
                foreach (var noticia in noticias)
                {
                    Noticias.Add(new NoticiaViewModel
                    {
                        Titulo = noticia.Titulo,
                        Categoria = noticia.Categoria,
                        DataPublicacao = noticia.DataPublicacao,
                        Url = noticia.Url
                    });
                }
            });
            
            LoggingTask.RegistrarInfo($"📋 {noticias.Count} notícias carregadas na interface");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao carregar notícias na interface", ex);
        }
    }
}
```

### Sistema de Notificações

```csharp
// Notificações do sistema Windows
public static void ExibirNotificacaoSistema(string titulo, string mensagem)
{
    try
    {
        if (!Config.Instancia.InterfaceUsuario.FuncionalidadesInterface.NotificacoesDesktop)
        {
            return;
        }

        // Usar ToastNotification ou System.Windows.Forms.NotifyIcon
        var notificacao = new NotifyIcon
        {
            Icon = SystemIcons.Information,
            BalloonTipTitle = titulo,
            BalloonTipText = mensagem,
            Visible = true
        };

        notificacao.ShowBalloonTip(3000);
        LoggingTask.RegistrarInfo($"🔔 Notificação exibida: {titulo}");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarAviso($"Erro na notificação: {ex.Message}");
    }
}
```

### Integração com Program.cs

```csharp
// Program.cs com suporte a GUI e Console
public class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        try
        {
            // Carregar configurações
            Config.CarregarConfiguracoes();
            
            // Verificar argumentos da linha de comando
            var modoConsole = args.Contains("--console") || args.Contains("-c");
            
            if (!modoConsole && Config.Instancia.ModoGUIHabilitado())
            {
                // Modo gráfico
                LoggingTask.RegistrarInfo("🖼️ Iniciando AdrenalineSpy em modo gráfico");
                
                var app = new Application();
                await GUITask.InicializarInterface();
                app.Run();
            }
            else
            {
                // Modo console
                LoggingTask.RegistrarInfo("💻 Executando AdrenalineSpy em modo console");
                
                var workflow = new Workflow.Workflow();
                var sucesso = await workflow.ExecutarScrapingCompleto();
                
                Environment.Exit(sucesso ? 0 : 1);
            }
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro fatal na aplicação", ex);
            Environment.Exit(1);
        }
    }
}
```
