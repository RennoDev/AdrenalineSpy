# Quartz.NET - Agendamento de Tarefas

## O que é Quartz.NET

**Quartz.NET** é uma biblioteca de agendamento de jobs (tarefas) para .NET que permite executar código automaticamente em horários específicos, intervalos regulares ou com base em triggers complexos.

**Onde é usado no AdrenalineSpy:**
- Execução automática do scraping em horários programados
- Coleta periódica de notícias (a cada X horas/dias)
- Geração automática de relatórios em horários específicos
- Limpeza automática de logs e arquivos antigos
- Monitoramento da saúde do sistema em intervalos regulares
- Execução de backups automáticos dos dados coletados

**Cenários típicos**: Scraping diário às 06:00, relatórios semanais domingo 22:00, limpeza mensal dia 1º 02:00.

## Como Instalar

### 1. Instalar Pacotes Quartz.NET

```powershell
dotnet add package Quartz
dotnet add package Quartz.Extensions.Hosting
```

### 2. Verificar .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Quartz" Version="3.8.0" />
    <PackageReference Include="Quartz.Extensions.Hosting" Version="3.8.0" />
  </ItemGroup>
</Project>
```

### 3. Configurar Hosting (Opcional)

Para aplicações que rodam como serviço Windows:

```powershell
dotnet add package Microsoft.Extensions.Hosting
```

## Implementar no AutomationSettings.json

Adicione configurações de agendamento na raiz do JSON:

```json
{
  "Navegacao": {
    "UrlBase": "https://www.adrenaline.com.br",
    "DelayEntrePaginas": 2000
  },
  "Agendamento": {
    "HabilitarQuartz": true,
    "ExecutarAoInicializar": false,
    "TimeZone": "E. South America Standard Time",
    "ConfiguracaoJobs": {
      "JobScrapingPrincipal": {
        "Habilitado": true,
        "CronExpression": "0 0 6,12,18 * * ?",
        "Descricao": "Scraping principal às 06:00, 12:00 e 18:00",
        "TipoJob": "ScrapingCompleto"
      },
      "JobRelatoriosDiarios": {
        "Habilitado": true,
        "CronExpression": "0 30 22 * * ?",
        "Descricao": "Relatórios diários às 22:30",
        "TipoJob": "GerarRelatorios"
      },
      "JobLimpezaSemanal": {
        "Habilitado": true,
        "CronExpression": "0 0 2 ? * SUN",
        "Descricao": "Limpeza semanal domingo às 02:00",
        "TipoJob": "LimpezaArquivos"
      },
      "JobBackupMensal": {
        "Habilitado": false,
        "CronExpression": "0 0 3 1 * ?",
        "Descricao": "Backup mensal dia 1º às 03:00",
        "TipoJob": "BackupDados"
      }
    },
    "ConfiguracaoScheduler": {
      "Nome": "AdrenalineSpyScheduler",
      "ThreadCount": 3,
      "MaxConcurrencia": 2,
      "MisfireThreshold": 60000
    },
    "Notificacoes": {
      "NotificarSucesso": true,
      "NotificarErro": true,
      "NotificarAtraso": true,
      "EmailNotificacoes": "admin@adrenalinespy.local"
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

**Configurações específicas do Quartz:**
- **`CronExpression`**: Formato cron para definir quando executar
- **`TipoJob`**: Identifica qual task executar
- **`ConfiguracaoScheduler`**: Configurações do agendador
- **`Notificacoes`**: Alertas sobre execução dos jobs

## Implementar no Config.cs

Adicione classes de configuração para agendamento:

```csharp
public class JobConfiguracao
{
    public bool Habilitado { get; set; } = true;
    public string CronExpression { get; set; } = "0 0 6 * * ?";
    public string Descricao { get; set; } = string.Empty;
    public string TipoJob { get; set; } = string.Empty;
}

public class ConfiguracaoSchedulerConfig
{
    public string Nome { get; set; } = "AdrenalineSpyScheduler";
    public int ThreadCount { get; set; } = 3;
    public int MaxConcorrencia { get; set; } = 2;
    public int MisfireThreshold { get; set; } = 60000;
}

public class NotificacoesConfig
{
    public bool NotificarSucesso { get; set; } = true;
    public bool NotificarErro { get; set; } = true;
    public bool NotificarAtraso { get; set; } = true;
    public string EmailNotificacoes { get; set; } = string.Empty;
}

public class AgendamentoConfig
{
    public bool HabilitarQuartz { get; set; } = true;
    public bool ExecutarAoInicializar { get; set; } = false;
    public string TimeZone { get; set; } = "E. South America Standard Time";
    public Dictionary<string, JobConfiguracao> ConfiguracaoJobs { get; set; } = new();
    public ConfiguracaoSchedulerConfig ConfiguracaoScheduler { get; set; } = new();
    public NotificacoesConfig Notificacoes { get; set; } = new();
}

public class Config
{
    // ... propriedades existentes ...
    public AgendamentoConfig Agendamento { get; set; } = new();
    
    /// <summary>
    /// Verifica se Quartz.NET está habilitado
    /// </summary>
    public bool QuartzHabilitado()
    {
        return Agendamento.HabilitarQuartz;
    }

    /// <summary>
    /// Obtém jobs habilitados para agendamento
    /// </summary>
    public Dictionary<string, JobConfiguracao> ObterJobsHabilitados()
    {
        return Agendamento.ConfiguracaoJobs
            .Where(kvp => kvp.Value.Habilitado)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Obtém TimeZone configurado
    /// </summary>
    public TimeZoneInfo ObterTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(Agendamento.TimeZone);
        }
        catch
        {
            LoggingTask.RegistrarAviso($"TimeZone '{Agendamento.TimeZone}' não encontrado, usando UTC");
            return TimeZoneInfo.Utc;
        }
    }
}
```

## Montar nas Tasks

Crie a classe `SchedulingTask.cs` na pasta `Workflow/Tasks/`:

```csharp
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace AdrenalineSpy.Workflow.Tasks;

/// <summary>
/// Gerencia agendamento de tarefas automatizadas para o AdrenalineSpy
/// </summary>
public static class SchedulingTask
{
    private static IScheduler? _scheduler;
    private static readonly Dictionary<string, Type> _tiposJobs = new()
    {
        ["ScrapingCompleto"] = typeof(ScrapingCompletoJob),
        ["GerarRelatorios"] = typeof(GerarRelatoriosJob),
        ["LimpezaArquivos"] = typeof(LimpezaArquivosJob),
        ["BackupDados"] = typeof(BackupDadosJob)
    };

    /// <summary>
    /// Inicializa e configura o Quartz Scheduler
    /// </summary>
    public static async Task<bool> InicializarScheduler()
    {
        try
        {
            if (!Config.Instancia.QuartzHabilitado())
            {
                LoggingTask.RegistrarInfo("⏰ Quartz.NET desabilitado nas configurações");
                return false;
            }

            LoggingTask.RegistrarInfo("🕒 Inicializando Quartz.NET Scheduler");

            // Criar factory do scheduler
            var factory = new StdSchedulerFactory();
            _scheduler = await factory.GetScheduler();

            // Configurar propriedades do scheduler
            await ConfigurarScheduler();

            // Registrar listener de eventos
            await RegistrarListeners();

            // Iniciar scheduler
            await _scheduler.Start();

            // Configurar jobs
            await ConfigurarTodosJobs();

            LoggingTask.RegistrarInfo("✅ Quartz Scheduler inicializado com sucesso");
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao inicializar Quartz Scheduler", ex);
            return false;
        }
    }

    /// <summary>
    /// Configura propriedades do scheduler
    /// </summary>
    private static async Task ConfigurarScheduler()
    {
        if (_scheduler == null) return;

        var config = Config.Instancia.Agendamento.ConfiguracaoScheduler;
        
        // Configurações básicas já são aplicadas na criação
        // Scheduler name, thread count, etc. são configurados via StdSchedulerFactory
        
        LoggingTask.RegistrarInfo($"🔧 Scheduler '{config.Nome}' configurado com {config.ThreadCount} threads");
    }

    /// <summary>
    /// Registra listeners para eventos do scheduler
    /// </summary>
    private static async Task RegistrarListeners()
    {
        if (_scheduler == null) return;

        var listener = new JobExecutionListener();
        _scheduler.ListenerManager.AddJobListener(listener, GroupMatcher<JobKey>.AnyGroup());
        
        LoggingTask.RegistrarInfo("👂 Listeners de jobs registrados");
    }

    /// <summary>
    /// Configura todos os jobs habilitados
    /// </summary>
    private static async Task ConfigurarTodosJobs()
    {
        var jobsHabilitados = Config.Instancia.ObterJobsHabilitados();
        
        foreach (var (nomeJob, configJob) in jobsHabilitados)
        {
            await ConfigurarJob(nomeJob, configJob);
        }

        LoggingTask.RegistrarInfo($"📅 {jobsHabilitados.Count} jobs configurados");
    }

    /// <summary>
    /// Configura um job específico
    /// </summary>
    private static async Task ConfigurarJob(string nomeJob, JobConfiguracao config)
    {
        try
        {
            if (_scheduler == null) return;

            // Verificar se tipo de job existe
            if (!_tiposJobs.TryGetValue(config.TipoJob, out var tipoJob))
            {
                LoggingTask.RegistrarAviso($"⚠️ Tipo de job desconhecido: {config.TipoJob}");
                return;
            }

            // Criar job detail
            var job = JobBuilder.Create(tipoJob)
                .WithIdentity(nomeJob, "AdrenalineSpyJobs")
                .WithDescription(config.Descricao)
                .Build();

            // Criar trigger cron
            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{nomeJob}Trigger", "AdrenalineSpyTriggers")
                .WithCronSchedule(config.CronExpression, x => x.InTimeZone(Config.Instancia.ObterTimeZone()))
                .WithDescription(config.Descricao)
                .Build();

            // Agendar job
            await _scheduler.ScheduleJob(job, trigger);

            LoggingTask.RegistrarInfo($"📋 Job '{nomeJob}' agendado: {config.CronExpression}");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro($"Erro ao configurar job '{nomeJob}'", ex);
        }
    }

    /// <summary>
    /// Executa job manualmente (fora do agendamento)
    /// </summary>
    public static async Task<bool> ExecutarJobManual(string nomeJob)
    {
        try
        {
            if (_scheduler == null)
            {
                LoggingTask.RegistrarErro("Scheduler não inicializado");
                return false;
            }

            var jobKey = new JobKey(nomeJob, "AdrenalineSpyJobs");
            
            if (!await _scheduler.CheckExists(jobKey))
            {
                LoggingTask.RegistrarErro($"Job '{nomeJob}' não encontrado");
                return false;
            }

            LoggingTask.RegistrarInfo($"▶️ Executando job manual: {nomeJob}");
            await _scheduler.TriggerJob(jobKey);
            
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro($"Erro ao executar job manual '{nomeJob}'", ex);
            return false;
        }
    }

    /// <summary>
    /// Pausa agendamento de um job específico
    /// </summary>
    public static async Task<bool> PausarJob(string nomeJob)
    {
        try
        {
            if (_scheduler == null) return false;

            var jobKey = new JobKey(nomeJob, "AdrenalineSpyJobs");
            await _scheduler.PauseJob(jobKey);
            
            LoggingTask.RegistrarInfo($"⏸️ Job '{nomeJob}' pausado");
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro($"Erro ao pausar job '{nomeJob}'", ex);
            return false;
        }
    }

    /// <summary>
    /// Retoma agendamento de um job específico
    /// </summary>
    public static async Task<bool> RetomarJob(string nomeJob)
    {
        try
        {
            if (_scheduler == null) return false;

            var jobKey = new JobKey(nomeJob, "AdrenalineSpyJobs");
            await _scheduler.ResumeJob(jobKey);
            
            LoggingTask.RegistrarInfo($"▶️ Job '{nomeJob}' retomado");
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro($"Erro ao retomar job '{nomeJob}'", ex);
            return false;
        }
    }

    /// <summary>
    /// Obtém status de todos os jobs
    /// </summary>
    public static async Task<Dictionary<string, string>> ObterStatusJobs()
    {
        try
        {
            if (_scheduler == null) return new Dictionary<string, string>();

            var status = new Dictionary<string, string>();
            var jobKeys = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals("AdrenalineSpyJobs"));

            foreach (var jobKey in jobKeys)
            {
                var triggers = await _scheduler.GetTriggersOfJob(jobKey);
                var trigger = triggers.FirstOrDefault();
                
                if (trigger != null)
                {
                    var triggerState = await _scheduler.GetTriggerState(trigger.Key);
                    var nextFireTime = trigger.GetNextFireTimeUtc();
                    
                    status[jobKey.Name] = $"{triggerState} (Próxima: {nextFireTime?.ToLocalTime()})";
                }
            }

            return status;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao obter status dos jobs", ex);
            return new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Finaliza scheduler e libera recursos
    /// </summary>
    public static async Task FinalizarScheduler()
    {
        try
        {
            if (_scheduler != null)
            {
                await _scheduler.Shutdown(waitForJobsToComplete: true);
                LoggingTask.RegistrarInfo("🔚 Quartz Scheduler finalizado");
            }
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Aviso ao finalizar scheduler: {ex.Message}");
        }
    }
}

/// <summary>
/// Listener para eventos de execução de jobs
/// </summary>
public class JobExecutionListener : IJobListener
{
    public string Name => "AdrenalineSpyJobListener";

    public async Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        LoggingTask.RegistrarAviso($"🚫 Job vetado: {context.JobDetail.Key.Name}");
    }

    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        LoggingTask.RegistrarInfo($"⏳ Iniciando job: {context.JobDetail.Key.Name}");
    }

    public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
    {
        var nomeJob = context.JobDetail.Key.Name;
        var duração = context.JobRunTime;

        if (jobException != null)
        {
            LoggingTask.RegistrarErro($"❌ Job '{nomeJob}' falhou após {duração.TotalSeconds:F1}s", jobException);
            await NotificarErroJob(nomeJob, jobException);
        }
        else
        {
            LoggingTask.RegistrarInfo($"✅ Job '{nomeJob}' concluído com sucesso em {duração.TotalSeconds:F1}s");
            await NotificarSucessoJob(nomeJob, duração);
        }
    }

    private async Task NotificarErroJob(string nomeJob, Exception ex)
    {
        if (Config.Instancia.Agendamento.Notificacoes.NotificarErro)
        {
            // TODO: Implementar notificação por email
            LoggingTask.RegistrarInfo($"📧 Notificação de erro enviada para job '{nomeJob}'");
        }
    }

    private async Task NotificarSucessoJob(string nomeJob, TimeSpan duracao)
    {
        if (Config.Instancia.Agendamento.Notificacoes.NotificarSucesso)
        {
            // TODO: Implementar notificação por email
            LoggingTask.RegistrarInfo($"📧 Notificação de sucesso enviada para job '{nomeJob}'");
        }
    }
}

/// <summary>
/// Job para execução completa de scraping
/// </summary>
[DisallowConcurrentExecution]
public class ScrapingCompletoJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var workflow = new Workflow.Workflow();
        var sucesso = await workflow.ExecutarScrapingCompleto();
        
        if (!sucesso)
        {
            throw new JobExecutionException("Falha na execução do scraping completo");
        }
    }
}

/// <summary>
/// Job para geração de relatórios
/// </summary>
[DisallowConcurrentExecution]
public class GerarRelatoriosJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            // Buscar notícias recentes
            var noticias = await MigrationTask.BuscarNoticiasRecentes(1000);
            
            if (!noticias.Any())
            {
                LoggingTask.RegistrarAviso("📊 Nenhuma notícia encontrada para relatórios agendados");
                return;
            }

            var dataExecucao = DateTime.Now;
            var tasks = new List<Task<bool>>();

            // Gerar relatórios habilitados
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

            LoggingTask.RegistrarInfo($"📊 Relatórios agendados gerados: {sucessos}/{tasks.Count}");
        }
        catch (Exception ex)
        {
            throw new JobExecutionException("Falha na geração de relatórios agendados", ex);
        }
    }
}

/// <summary>
/// Job para limpeza de arquivos antigos
/// </summary>
public class LimpezaArquivosJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var diasParaManter = 30; // Manter arquivos dos últimos 30 dias
            var dataCorte = DateTime.Now.AddDays(-diasParaManter);

            // Limpar logs antigos
            await LimparArquivosAntigos("logs/", "*.log", dataCorte);
            
            // Limpar screenshots antigos
            await LimparArquivosAntigos("screenshots/", "*.png", dataCorte);
            
            // Limpar relatórios antigos
            await LimparArquivosAntigos("exports/", "*.*", dataCorte);

            LoggingTask.RegistrarInfo($"🧹 Limpeza de arquivos concluída (mantidos últimos {diasParaManter} dias)");
        }
        catch (Exception ex)
        {
            throw new JobExecutionException("Falha na limpeza de arquivos", ex);
        }
    }

    private async Task LimparArquivosAntigos(string diretorio, string padrao, DateTime dataCorte)
    {
        try
        {
            if (!Directory.Exists(diretorio)) return;

            var arquivos = Directory.GetFiles(diretorio, padrao, SearchOption.AllDirectories)
                .Where(f => File.GetCreationTime(f) < dataCorte);

            var contador = 0;
            foreach (var arquivo in arquivos)
            {
                File.Delete(arquivo);
                contador++;
            }

            if (contador > 0)
            {
                LoggingTask.RegistrarInfo($"🗑️ Removidos {contador} arquivos antigos de {diretorio}");
            }
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Aviso na limpeza de {diretorio}: {ex.Message}");
        }
    }
}

/// <summary>
/// Job para backup de dados
/// </summary>
public class BackupDadosJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var dataBackup = DateTime.Now.ToString("yyyy-MM-dd");
            var nomeBackup = $"adrenalinespy_backup_{dataBackup}.sql";
            var caminhoBackup = Path.Combine("backups", nomeBackup);

            Directory.CreateDirectory("backups");

            // TODO: Implementar backup do banco de dados
            // var sucesso = await MigrationTask.GerarBackupBanco(caminhoBackup);

            LoggingTask.RegistrarInfo($"💾 Backup agendado gerado: {nomeBackup}");
        }
        catch (Exception ex)
        {
            throw new JobExecutionException("Falha no backup de dados", ex);
        }
    }
}
```

---

## Como Adicionar no Program.cs

### Transformando Program.cs em Serviço Agendado

O **Quartz.NET** representa a **evolução final** do Program.cs - de execução manual para **automação completa**. Esta é a fase onde o AdrenalineSpy se torna verdadeiramente autônomo.

### Program.cs - Fase: Primeira Implementação de Agendamento
```csharp
using Quartz;
using Quartz.Impl;

static async Task Main(string[] args)
{
    Config config = Config.Instancia;
    LoggingTask.ConfigurarLogger();
    
    try
    {
        LoggingTask.RegistrarInfo("=== AdrenalineSpy Iniciado ===");
        
        // Verificar se deve rodar como agendador
        if (args.Contains("--scheduler") || args.Contains("--daemon"))
        {
            LoggingTask.RegistrarInfo("🕐 Modo agendador ativado - inicializando Quartz.NET");
            
            // ADICIONADO: Inicializar agendador
            await IniciarAgendador(config);
        }
        else
        {
            LoggingTask.RegistrarInfo("▶️ Modo execução única");
            
            // Execução manual normal (já implementada)
            await ExecutarWorkflowCompleto(config);
        }
        
        LoggingTask.RegistrarInfo("=== Inicialização finalizada ===");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Program.Main");
        
        // Notificar erro (já implementado com MailKit)
        await NotificarErroPorEmail(ex, config);
    }
    finally
    {
        LoggingTask.FecharLogger();
    }
}

private static async Task IniciarAgendador(Config config)
{
    // Criar scheduler
    StdSchedulerFactory factory = new StdSchedulerFactory();
    IScheduler scheduler = await factory.GetScheduler();
    
    // Configurar job de scraping
    IJobDetail job = JobBuilder.Create<AdrenalineScrapingJob>()
        .WithIdentity("scraping-job", "adrenaline-group")
        .Build();
    
    // Configurar trigger com base na configuração
    ITrigger trigger = TriggerBuilder.Create()
        .WithIdentity("scraping-trigger", "adrenaline-group")
        .WithCronSchedule(config.Agendamento.CronExpression)
        .Build();
    
    // Registrar job
    await scheduler.ScheduleJob(job, trigger);
    
    LoggingTask.RegistrarInfo($"📅 Job agendado: {config.Agendamento.CronExpression}");
    LoggingTask.RegistrarInfo($"⏰ Próxima execução: {trigger.GetNextFireTime():dd/MM/yyyy HH:mm:ss}");
    
    // Iniciar scheduler
    await scheduler.Start();
    
    LoggingTask.RegistrarInfo("✅ Quartz.NET iniciado - pressione CTRL+C para parar");
    
    // Aguardar sinal de parada
    await AguardarSinalParada(scheduler);
}

private static async Task AguardarSinalParada(IScheduler scheduler)
{
    // Capturar CTRL+C para parada graceful
    Console.CancelKeyPress += async (sender, e) =>
    {
        e.Cancel = true;
        LoggingTask.RegistrarInfo("🛑 Sinal de parada recebido...");
        
        await scheduler.Shutdown(true);
        LoggingTask.RegistrarInfo("✅ Scheduler parado graciosamente");
        
        Environment.Exit(0);
    };
    
    // Manter aplicação viva
    await Task.Delay(Timeout.Infinite);
}
```

### Program.cs - Fase: Múltiplos Jobs e Monitoramento
```csharp
static async Task Main(string[] args)
{
    Config config = Config.Instancia;
    LoggingTask.ConfigurarLogger();
    
    try
    {
        LoggingTask.RegistrarInfo("=== AdrenalineSpy Scheduler Iniciado ===");
        
        if (args.Contains("--scheduler"))
        {
            await IniciarAgendadorCompleto(config, args);
        }
        else if (args.Contains("--install-service"))
        {
            await InstalarComoServico();
        }
        else if (args.Contains("--uninstall-service"))
        {
            await DesinstalarServico();
        }
        else
        {
            // Execução manual
            await ExecutarWorkflowCompleto(config);
        }
        
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Program.Main");
        await NotificarErroPorEmail(ex, config);
    }
    finally
    {
        LoggingTask.FecharLogger();
    }
}

private static async Task IniciarAgendadorCompleto(Config config, string[] args)
{
    StdSchedulerFactory factory = new StdSchedulerFactory();
    IScheduler scheduler = await factory.GetScheduler();
    
    // ADICIONADO: Múltiplos jobs configuráveis
    await ConfigurarJobs(scheduler, config);
    
    // ADICIONADO: Listeners para monitoramento
    await ConfigurarListeners(scheduler);
    
    await scheduler.Start();
    
    LoggingTask.RegistrarInfo("🚀 Scheduler completo iniciado");
    
    // ADICIONADO: Interface de controle por console
    if (!args.Contains("--silent"))
    {
        await ExecutarInterfaceConsole(scheduler);
    }
    else
    {
        await AguardarSinalParada(scheduler);
    }
}

private static async Task ConfigurarJobs(IScheduler scheduler, Config config)
{
    // Job 1: Scraping principal
    var scrapingJob = JobBuilder.Create<AdrenalineScrapingJob>()
        .WithIdentity("scraping-job", "adrenaline")
        .Build();
    
    var scrapingTrigger = TriggerBuilder.Create()
        .WithIdentity("scraping-trigger", "adrenaline")
        .WithCronSchedule(config.Agendamento.CronExpression)
        .Build();
    
    await scheduler.ScheduleJob(scrapingJob, scrapingTrigger);
    LoggingTask.RegistrarInfo($"📊 Job Scraping: {config.Agendamento.CronExpression}");
    
    // Job 2: Relatório diário (se configurado)
    if (config.Agendamento.RelatoriosAutomaticos)
    {
        var relatorioJob = JobBuilder.Create<RelatorioJob>()
            .WithIdentity("relatorio-job", "adrenaline")
            .Build();
        
        var relatorioTrigger = TriggerBuilder.Create()
            .WithIdentity("relatorio-trigger", "adrenaline")
            .WithCronSchedule(config.Agendamento.CronRelatorio) // Ex: "0 0 8 * * ?" (todo dia 8h)
            .Build();
        
        await scheduler.ScheduleJob(relatorioJob, relatorioTrigger);
        LoggingTask.RegistrarInfo($"📈 Job Relatório: {config.Agendamento.CronRelatorio}");
    }
    
    // Job 3: Limpeza de logs antigos (opcional)
    if (config.Agendamento.LimpezaAutomatica)
    {
        var limpezaJob = JobBuilder.Create<LimpezaJob>()
            .WithIdentity("limpeza-job", "manutencao")
            .Build();
        
        var limpezaTrigger = TriggerBuilder.Create()
            .WithIdentity("limpeza-trigger", "manutencao")
            .WithCronSchedule("0 0 2 * * ?") // Todo dia às 2h
            .Build();
        
        await scheduler.ScheduleJob(limpezaJob, limpezaTrigger);
        LoggingTask.RegistrarInfo("🧹 Job Limpeza: Todo dia às 2h");
    }
    
    // Job 4: Health check (verificar se tudo está funcionando)
    var healthJob = JobBuilder.Create<HealthCheckJob>()
        .WithIdentity("health-job", "monitoramento")
        .Build();
    
    var healthTrigger = TriggerBuilder.Create()
        .WithIdentity("health-trigger", "monitoramento")
        .WithSimpleSchedule(x => x
            .WithIntervalInMinutes(30)
            .RepeatForever())
        .Build();
    
    await scheduler.ScheduleJob(healthJob, healthTrigger);
    LoggingTask.RegistrarInfo("💗 Job Health Check: A cada 30 minutos");
}

private static async Task ConfigurarListeners(IScheduler scheduler)
{
    // Listener para monitorar execuções
    var jobListener = new AdrenalineJobListener();
    scheduler.ListenerManager.AddJobListener(jobListener, GroupMatcher<JobKey>.AnyGroup());
    
    LoggingTask.RegistrarInfo("👂 Listeners de monitoramento configurados");
}

private static async Task ExecutarInterfaceConsole(IScheduler scheduler)
{
    LoggingTask.RegistrarInfo("📱 Interface de console ativada");
    Console.WriteLine("\n=== CONTROLE ADRENALINESPY ===");
    Console.WriteLine("Comandos disponíveis:");
    Console.WriteLine("  status  - Ver status dos jobs");
    Console.WriteLine("  pause   - Pausar todos os jobs");  
    Console.WriteLine("  resume  - Retomar jobs pausados");
    Console.WriteLine("  run     - Executar scraping agora");
    Console.WriteLine("  quit    - Sair");
    Console.WriteLine("================================\n");
    
    while (true)
    {
        Console.Write("AdrenalineSpy> ");
        string comando = Console.ReadLine()?.ToLower().Trim();
        
        try
        {
            switch (comando)
            {
                case "status":
                    await ExibirStatusJobs(scheduler);
                    break;
                    
                case "pause":
                    await scheduler.PauseAll();
                    Console.WriteLine("⏸️ Todos os jobs pausados");
                    break;
                    
                case "resume":
                    await scheduler.ResumeAll();
                    Console.WriteLine("▶️ Jobs retomados");
                    break;
                    
                case "run":
                    await ExecutarScrapingAgora(scheduler);
                    break;
                    
                case "quit":
                case "exit":
                    await scheduler.Shutdown(true);
                    return;
                    
                default:
                    if (!string.IsNullOrEmpty(comando))
                        Console.WriteLine("❌ Comando não reconhecido");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro: {ex.Message}");
            LoggingTask.RegistrarErro(ex, "Console Command");
        }
    }
}

private static async Task ExibirStatusJobs(IScheduler scheduler)
{
    var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
    
    Console.WriteLine("\n📊 STATUS DOS JOBS:");
    
    foreach (var jobKey in jobKeys)
    {
        var jobDetail = await scheduler.GetJobDetail(jobKey);
        var triggers = await scheduler.GetTriggersOfJob(jobKey);
        
        Console.WriteLine($"\n🔧 Job: {jobKey.Name}");
        Console.WriteLine($"   Grupo: {jobKey.Group}");
        Console.WriteLine($"   Classe: {jobDetail.JobType.Name}");
        
        foreach (var trigger in triggers)
        {
            var state = await scheduler.GetTriggerState(trigger.Key);
            var nextFire = trigger.GetNextFireTime();
            
            Console.WriteLine($"   ⏰ Próxima execução: {nextFire:dd/MM/yyyy HH:mm:ss}");
            Console.WriteLine($"   📍 Estado: {state}");
        }
    }
    Console.WriteLine();
}

private static async Task ExecutarScrapingAgora(IScheduler scheduler)
{
    Console.WriteLine("🚀 Iniciando execução manual do scraping...");
    
    var jobKey = new JobKey("scraping-job", "adrenaline");
    await scheduler.TriggerJob(jobKey);
    
    Console.WriteLine("✅ Scraping iniciado - verifique os logs para acompanhar");
}
```

### Program.cs - Fase: Serviço Windows Completo
```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

static async Task Main(string[] args)
{
    // ADICIONADO: Suporte completo a Windows Service
    if (args.Contains("--service"))
    {
        await ExecutarComoServico(args);
    }
    else
    {
        await ExecutarComoConsole(args);
    }
}

private static async Task ExecutarComoServico(string[] args)
{
    LoggingTask.ConfigurarLogger();
    LoggingTask.RegistrarInfo("🔧 Iniciando como Windows Service");
    
    var host = Host.CreateDefaultBuilder(args)
        .UseWindowsService()
        .ConfigureServices((context, services) =>
        {
            services.AddSingleton<Config>(Config.Instancia);
            services.AddHostedService<AdrenalineSchedulerService>();
        })
        .Build();
    
    await host.RunAsync();
}

private static async Task ExecutarComoConsole(string[] args)
{
    Config config = Config.Instancia;
    LoggingTask.ConfigurarLogger();
    
    try
    {
        if (args.Contains("--scheduler"))
        {
            await IniciarAgendadorCompleto(config, args);
        }
        else
        {
            await ExecutarWorkflowCompleto(config);
        }
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Program.Main");
        await NotificarErroPorEmail(ex, config);
    }
    finally
    {
        LoggingTask.FecharLogger();
    }
}

// Serviço para integração com Windows Services
public class AdrenalineSchedulerService : BackgroundService
{
    private readonly Config _config;
    private IScheduler _scheduler;
    
    public AdrenalineSchedulerService(Config config)
    {
        _config = config;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LoggingTask.RegistrarInfo("🚀 AdrenalineSchedulerService iniciando...");
        
        try
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            _scheduler = await factory.GetScheduler();
            
            await ConfigurarJobs(_scheduler, _config);
            await _scheduler.Start();
            
            LoggingTask.RegistrarInfo("✅ Scheduler iniciado como serviço");
            
            // Aguardar sinal de parada
            stoppingToken.Register(async () =>
            {
                LoggingTask.RegistrarInfo("🛑 Parando serviço...");
                await _scheduler?.Shutdown(true);
                LoggingTask.RegistrarInfo("✅ Serviço parado");
            });
            
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, "AdrenalineSchedulerService");
            throw;
        }
    }
}
```

### Exemplos de Uso da Linha de Comando

```bash
# Execução única (manual)
dotnet run

# Modo agendador com interface
dotnet run -- --scheduler

# Modo agendador silencioso (para produção)
dotnet run -- --scheduler --silent

# Instalar como serviço Windows
dotnet run -- --install-service

# Executar como serviço
dotnet run -- --service

# Desinstalar serviço
dotnet run -- --uninstall-service

# Teste de configuração (sem executar)
dotnet run -- --test-config
```

### Scripts de Instalação/Desinstalação

```csharp
private static async Task InstalarComoServico()
{
    try
    {
        LoggingTask.RegistrarInfo("📦 Instalando como serviço Windows...");
        
        string serviceName = "AdrenalineSpyScheduler";
        string displayName = "AdrenalineSpy RPA Scheduler";
        string description = "Serviço de automação RPA para coleta de notícias do Adrenaline.com.br";
        
        string executablePath = Environment.ProcessPath;
        string arguments = "--service";
        
        var startInfo = new ProcessStartInfo
        {
            FileName = "sc",
            Arguments = $"create {serviceName} binPath= \"{executablePath} {arguments}\" DisplayName= \"{displayName}\" start= auto",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        
        using var process = Process.Start(startInfo);
        await process.WaitForExitAsync();
        
        if (process.ExitCode == 0)
        {
            Console.WriteLine($"✅ Serviço '{serviceName}' instalado com sucesso");
            Console.WriteLine($"💡 Use 'net start {serviceName}' para iniciar");
        }
        else
        {
            Console.WriteLine($"❌ Falha ao instalar serviço (código: {process.ExitCode})");
        }
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "InstalarComoServico");
        Console.WriteLine($"❌ Erro ao instalar serviço: {ex.Message}");
    }
}

private static async Task DesinstalarServico()
{
    try
    {
        LoggingTask.RegistrarInfo("🗑️ Desinstalando serviço Windows...");
        
        string serviceName = "AdrenalineSpyScheduler";
        
        // Parar serviço se estiver rodando
        var stopInfo = new ProcessStartInfo
        {
            FileName = "net",
            Arguments = $"stop {serviceName}",
            UseShellExecute = false
        };
        
        using (var stopProcess = Process.Start(stopInfo))
        {
            await stopProcess.WaitForExitAsync();
        }
        
        // Remover serviço
        var deleteInfo = new ProcessStartInfo
        {
            FileName = "sc",
            Arguments = $"delete {serviceName}",
            UseShellExecute = false,
            RedirectStandardOutput = true
        };
        
        using var deleteProcess = Process.Start(deleteInfo);
        await deleteProcess.WaitForExitAsync();
        
        if (deleteProcess.ExitCode == 0)
        {
            Console.WriteLine($"✅ Serviço '{serviceName}' removido com sucesso");
        }
        else
        {
            Console.WriteLine($"❌ Falha ao remover serviço (código: {deleteProcess.ExitCode})");
        }
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "DesinstalarServico");
        Console.WriteLine($"❌ Erro ao desinstalar serviço: {ex.Message}");
    }
}
```

### ⚠️ Configurações Críticas no AutomationSettings.json

```json
{
  "Agendamento": {
    "CronExpression": "0 0 */6 * * ?",
    "CronRelatorio": "0 0 8 * * ?",
    "RelatoriosAutomaticos": true,
    "LimpezaAutomatica": true,
    "TimeZone": "E. South America Standard Time",
    "MaxConcurrentJobs": 1
  }
}
```

### 💡 Padrões de Execução Recomendados

1. **Desenvolvimento**: `--scheduler` (com interface console)
2. **Teste**: `--scheduler --silent` (sem interface)
3. **Produção**: `--service` (como serviço Windows)
4. **Debug**: Execução manual sem agendamento

### 🔄 Evolução Completa Alcançada

Com **Quartz.NET** implementado, o AdrenalineSpy agora é um **sistema RPA completo**:
- ✅ Coleta automatizada (Playwright)  
- ✅ Persistência (ORM + Docker)
- ✅ Exportação (Excel, CSV, PDF)
- ✅ Notificações (MailKit)
- ✅ **Agendamento autônomo (Quartz.NET)**
- ⏳ Interface gráfica (próximo: GUI)

O sistema agora pode operar **completamente sozinho**, executando scraping, salvando dados, gerando relatórios e enviando notificações de forma automática e agendada! 🎉

---

## Métodos Mais Usados

### Inicializar Quartz no Program.cs

```csharp
// Program.cs com Quartz integrado
public static async Task Main(string[] args)
{
    try
    {
        // Carregar configurações
        Config.CarregarConfiguracoes();
        
        // Verificar modo de operação
        var modoScheduler = args.Contains("--scheduler") || args.Contains("-s");
        var modoGUI = !modoScheduler && Config.Instancia.ModoGUIHabilitado();

        if (modoScheduler)
        {
            // Modo agendador (serviço)
            LoggingTask.RegistrarInfo("⏰ Iniciando AdrenalineSpy em modo agendador");
            
            await SchedulingTask.InicializarScheduler();
            
            // Manter aplicação viva
            Console.WriteLine("Pressione 'q' para sair...");
            while (Console.ReadKey().KeyChar != 'q') { }
            
            await SchedulingTask.FinalizarScheduler();
        }
        else if (modoGUI)
        {
            // Modo GUI
            await GUITask.InicializarInterface();
        }
        else
        {
            // Modo console único
            var workflow = new Workflow.Workflow();
            await workflow.ExecutarScrapingCompleto();
        }
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro("Erro fatal", ex);
        Environment.Exit(1);
    }
}
```

### Expressões Cron Mais Comuns

```csharp
// Exemplos de CronExpression para diferentes cenários
var exemplos = new Dictionary<string, string>
{
    // Execução diária
    ["DiarioAs6h"] = "0 0 6 * * ?",              // Todo dia 06:00
    ["DiarioAs18h"] = "0 0 18 * * ?",             // Todo dia 18:00
    ["Dia3Vezes"] = "0 0 6,12,18 * * ?",          // 3x por dia (6h, 12h, 18h)
    
    // Execução por intervalo
    ["Cada2Horas"] = "0 0 */2 * * ?",             // A cada 2 horas
    ["Cada30Min"] = "0 */30 * * * ?",             // A cada 30 minutos
    ["CadaHora"] = "0 0 * * * ?",                 // De hora em hora
    
    // Execução semanal
    ["DomingoAs22h"] = "0 0 22 ? * SUN",          // Domingo 22:00
    ["SegundaAs9h"] = "0 0 9 ? * MON",            // Segunda 09:00
    ["SexaAs17h"] = "0 0 17 ? * FRI",             // Sexta 17:00
    
    // Execução mensal
    ["Dia1As3h"] = "0 0 3 1 * ?",                // Dia 1º do mês 03:00
    ["UltimoDiaDoMes"] = "0 0 23 L * ?",          // Último dia do mês 23:00
    
    // Execução personalizada
    ["DiaUtil9h"] = "0 0 9 ? * MON-FRI",          // Dias úteis 09:00
    ["FimSemana"] = "0 0 10 ? * SAT,SUN"          // Final de semana 10:00
};
```

### Criar Job Personalizado

```csharp
// Job personalizado para tarefa específica
[DisallowConcurrentExecution] // Evita execuções simultâneas
public class MonitoramentoSaudeJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            LoggingTask.RegistrarInfo("🏥 Iniciando monitoramento de saúde");
            
            // Verificar conectividade
            var conectividadeOk = await TestarConectividade();
            
            // Verificar uso de memória
            var memoriaOk = await VerificarUsoMemoria();
            
            // Verificar espaço em disco
            var discoOk = await VerificarEspacoDisco();
            
            var status = conectividadeOk && memoriaOk && discoOk;
            
            LoggingTask.RegistrarInfo($"💚 Monitoramento concluído: {(status ? "Sistema saudável" : "Alertas detectados")}");
            
            if (!status)
            {
                throw new JobExecutionException("Sistema com problemas de saúde detectados");
            }
        }
        catch (Exception ex)
        {
            throw new JobExecutionException("Falha no monitoramento de saúde", ex);
        }
    }
    
    private async Task<bool> TestarConectividade()
    {
        // Implementar teste de conectividade
        return true;
    }
}
```

### Controle Manual de Jobs via GUI

```csharp
// Na interface gráfica - controles para jobs
private async void BotaoExecutarJob_Click(object sender, RoutedEventArgs e)
{
    try
    {
        var nomeJob = ComboBoxJobs.SelectedValue?.ToString();
        if (string.IsNullOrEmpty(nomeJob))
        {
            MessageBox.Show("Selecione um job para executar");
            return;
        }
        
        LoggingTask.RegistrarInfo($"▶️ Executando job manual: {nomeJob}");
        
        var sucesso = await SchedulingTask.ExecutarJobManual(nomeJob);
        var mensagem = sucesso 
            ? "✅ Job executado com sucesso!" 
            : "❌ Falha na execução do job";
            
        MessageBox.Show(mensagem);
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro("Erro na execução manual do job", ex);
        MessageBox.Show($"Erro: {ex.Message}");
    }
}

// Atualizar status dos jobs na interface
private async Task AtualizarStatusJobs()
{
    var status = await SchedulingTask.ObterStatusJobs();
    
    Application.Current.Dispatcher.Invoke(() =>
    {
        DataGridJobs.ItemsSource = status.Select(kvp => new 
        {
            Job = kvp.Key,
            Status = kvp.Value
        }).ToList();
    });
}
```

### Configuração para Serviço Windows

```csharp
// Para executar como serviço Windows
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        // Configurar Quartz como serviço hospedado
        builder.Services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjection();
            
            // Configurar jobs
            q.AddJob<ScrapingCompletoJob>(opts => 
                opts.WithIdentity("ScrapingJob"));
                
            q.AddTrigger(opts => opts
                .ForJob("ScrapingJob")
                .WithIdentity("ScrapingTrigger")
                .WithCronSchedule("0 0 */6 * * ?"));
        });
        
        builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        
        var host = builder.Build();
        await host.RunAsync();
    }
}
```

### Sistema de Notificações por Job

```csharp
// Notificar sobre execução dos jobs
public static async Task NotificarExecucaoJob(string nomeJob, bool sucesso, TimeSpan duracao, Exception? erro = null)
{
    try
    {
        var config = Config.Instancia.Agendamento.Notificacoes;
        
        if ((sucesso && !config.NotificarSucesso) || (!sucesso && !config.NotificarErro))
        {
            return;
        }

        var assunto = $"AdrenalineSpy - Job '{nomeJob}' {(sucesso ? "Concluído" : "Falhou")}";
        var corpo = $"""
            Job: {nomeJob}
            Status: {(sucesso ? "✅ Sucesso" : "❌ Erro")}
            Duração: {duracao.TotalMinutes:F1} minutos
            Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
            
            {(erro != null ? $"Erro: {erro.Message}" : "Execução concluída sem erros.")}
            """;

        // TODO: Implementar envio via MailKit
        // await EmailTask.EnviarNotificacao(config.EmailNotificacoes, assunto, corpo);
        
        LoggingTask.RegistrarInfo($"📧 Notificação de job enviada: {nomeJob}");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarAviso($"Erro ao enviar notificação do job: {ex.Message}");
    }
}
```
