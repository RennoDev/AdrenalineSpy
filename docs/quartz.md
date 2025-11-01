# Quartz.NET - Agendamento de Tarefas

## Índice
1. [Introdução](#introdução)
2. [Instalação](#instalação)
3. [Conceitos Básicos](#conceitos-básicos)
4. [Agendar Tarefas](#agendar-tarefas)
5. [Cron Expressions](#cron-expressions)
6. [Exemplos Práticos](#exemplos-práticos)

---

## Introdução

**Quartz.NET** é um agendador de tarefas robusto para .NET, permitindo executar jobs em horários específicos ou intervalos regulares.

### Vantagens
- ✅ Cron expressions
- ✅ Jobs persistentes
- ✅ Clustering
- ✅ Missfire handling
- ✅ Listeners e plugins

---

## Instalação

```bash
dotnet add package Quartz
```

---

## Conceitos Básicos

### Componentes Principais

1. **Scheduler**: Orquestrador principal
2. **Job**: Tarefa a ser executada
3. **Trigger**: Define quando o job será executado
4. **JobDetail**: Metadados do job

---

## Agendar Tarefas

### Job Simples

```csharp
using Quartz;
using Quartz.Impl;

// 1. Criar Job
public class MeuJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine($"Job executado às {DateTime.Now}");
        await Task.CompletedTask;
    }
}

// 2. Configurar e Iniciar
class Program
{
    static async Task Main()
    {
        // Criar scheduler
        IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        await scheduler.Start();
        
        // Definir job
        IJobDetail job = JobBuilder.Create<MeuJob>()
            .WithIdentity("meuJob", "grupo1")
            .Build();
        
        // Definir trigger (executar a cada 10 segundos)
        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("meuTrigger", "grupo1")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInSeconds(10)
                .RepeatForever())
            .Build();
        
        // Agendar
        await scheduler.ScheduleJob(job, trigger);
        
        Console.WriteLine("Pressione qualquer tecla para parar...");
        Console.ReadKey();
        
        await scheduler.Shutdown();
    }
}
```

### Job com Parâmetros

```csharp
public class JobComParametros : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // Obter dados do JobDataMap
        JobDataMap dataMap = context.JobDetail.JobDataMap;
        
        string nome = dataMap.GetString("nome");
        int quantidade = dataMap.GetInt("quantidade");
        
        Console.WriteLine($"Processando: {nome}, Qtd: {quantidade}");
        
        await Task.CompletedTask;
    }
}

// Passar parâmetros
IJobDetail job = JobBuilder.Create<JobComParametros>()
    .WithIdentity("jobParams")
    .UsingJobData("nome", "Produto X")
    .UsingJobData("quantidade", 100)
    .Build();
```

---

## Cron Expressions

### Exemplos de Cron

```csharp
// Diariamente às 8:00
.WithCronSchedule("0 0 8 * * ?")

// De segunda a sexta às 9:00
.WithCronSchedule("0 0 9 ? * MON-FRI")

// A cada hora
.WithCronSchedule("0 0 * * * ?")

// A cada 5 minutos
.WithCronSchedule("0 */5 * * * ?")

// Todo dia 1º do mês às 00:00
.WithCronSchedule("0 0 0 1 * ?")

// A cada 30 segundos
.WithCronSchedule("*/30 * * * * ?")
```

### Formato Cron

```
┌─── Segundo (0-59)
│ ┌─── Minuto (0-59)
│ │ ┌─── Hora (0-23)
│ │ │ ┌─── Dia do mês (1-31)
│ │ │ │ ┌─── Mês (1-12 ou JAN-DEC)
│ │ │ │ │ ┌─── Dia da semana (1-7 ou SUN-SAT)
│ │ │ │ │ │ ┌─── Ano (opcional)
│ │ │ │ │ │ │
* * * * * ? *
```

### Usar Cron

```csharp
ITrigger trigger = TriggerBuilder.Create()
    .WithIdentity("cronTrigger")
    .WithCronSchedule("0 0 8 * * ?") // 8:00 todos os dias
    .Build();
```

---

## Exemplos Práticos

### Exemplo 1: RPA Agendado Diariamente

```csharp
public class RPADiarioJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine($"[{DateTime.Now}] Iniciando RPA Diário");
        
        try
        {
            // Sua automação aqui
            await ExecutarRPA();
            
            Console.WriteLine($"[{DateTime.Now}] RPA concluído com sucesso");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now}] Erro no RPA: {ex.Message}");
        }
    }
    
    private async Task ExecutarRPA()
    {
        // Processar pedidos
        // Gerar relatórios
        // Enviar emails
        await Task.Delay(1000); // Simular processamento
    }
}

// Agendar para executar todo dia às 8:00
class Program
{
    static async Task Main()
    {
        IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        await scheduler.Start();
        
        IJobDetail job = JobBuilder.Create<RPADiarioJob>()
            .WithIdentity("rpaDiario")
            .Build();
        
        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("triggerDiario")
            .WithCronSchedule("0 0 8 * * ?") // 8:00 AM
            .Build();
        
        await scheduler.ScheduleJob(job, trigger);
        
        Console.WriteLine("RPA agendado para executar diariamente às 8:00");
        Console.WriteLine("Pressione Enter para sair...");
        Console.ReadLine();
        
        await scheduler.Shutdown();
    }
}
```

### Exemplo 2: Múltiplos Jobs

```csharp
public class ProcessarPedidosJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("Processando pedidos...");
        await Task.Delay(500);
    }
}

public class GerarRelatorioJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("Gerando relatório...");
        await Task.Delay(500);
    }
}

public class EnviarEmailJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("Enviando emails...");
        await Task.Delay(500);
    }
}

class Program
{
    static async Task Main()
    {
        IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        await scheduler.Start();
        
        // Job 1: Processar pedidos a cada hora
        var job1 = JobBuilder.Create<ProcessarPedidosJob>()
            .WithIdentity("processarPedidos")
            .Build();
        
        var trigger1 = TriggerBuilder.Create()
            .WithIdentity("trigger1")
            .WithCronSchedule("0 0 * * * ?") // A cada hora
            .Build();
        
        // Job 2: Gerar relatório diariamente às 18:00
        var job2 = JobBuilder.Create<GerarRelatorioJob>()
            .WithIdentity("gerarRelatorio")
            .Build();
        
        var trigger2 = TriggerBuilder.Create()
            .WithIdentity("trigger2")
            .WithCronSchedule("0 0 18 * * ?") // 18:00
            .Build();
        
        // Job 3: Enviar email de segunda a sexta às 9:00
        var job3 = JobBuilder.Create<EnviarEmailJob>()
            .WithIdentity("enviarEmail")
            .Build();
        
        var trigger3 = TriggerBuilder.Create()
            .WithIdentity("trigger3")
            .WithCronSchedule("0 0 9 ? * MON-FRI") // 9:00 seg-sex
            .Build();
        
        await scheduler.ScheduleJob(job1, trigger1);
        await scheduler.ScheduleJob(job2, trigger2);
        await scheduler.ScheduleJob(job3, trigger3);
        
        Console.WriteLine("Jobs agendados:");
        Console.WriteLine("- Processar pedidos: A cada hora");
        Console.WriteLine("- Gerar relatório: Diariamente às 18:00");
        Console.WriteLine("- Enviar email: Seg-Sex às 9:00");
        
        Console.ReadLine();
        await scheduler.Shutdown();
    }
}
```

### Exemplo 3: Job com Dependência (Serilog)

```csharp
using Serilog;
using Quartz;

public class RPAComLoggingJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        Log.Information("=== Iniciando Execução do Job ===");
        Log.Information("Próxima execução: {ProximaExec}", context.NextFireTimeUtc);
        
        try
        {
            Log.Information("Processando itens...");
            
            for (int i = 1; i <= 5; i++)
            {
                Log.Debug("Processando item {Item}", i);
                await Task.Delay(100);
            }
            
            Log.Information("Job concluído com sucesso");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro na execução do job");
            
            // Requeue job se necessário
            var exception = new JobExecutionException(ex)
            {
                RefireImmediately = true // Tentar novamente imediatamente
            };
            throw exception;
        }
        finally
        {
            Log.Information("=== Finalizando Execução do Job ===");
        }
    }
}
```

### Exemplo 4: Classe Helper

```csharp
public static class QuartzHelper
{
    private static IScheduler _scheduler;
    
    public static async Task IniciarScheduler()
    {
        _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        await _scheduler.Start();
        Console.WriteLine("Scheduler iniciado");
    }
    
    public static async Task AgendarJob<T>(
        string jobId, 
        string cronExpression, 
        string descricao = null) where T : IJob
    {
        IJobDetail job = JobBuilder.Create<T>()
            .WithIdentity(jobId)
            .WithDescription(descricao)
            .Build();
        
        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity($"{jobId}_trigger")
            .WithCronSchedule(cronExpression)
            .Build();
        
        await _scheduler.ScheduleJob(job, trigger);
        Console.WriteLine($"Job '{jobId}' agendado: {cronExpression}");
    }
    
    public static async Task PausarJob(string jobId)
    {
        await _scheduler.PauseJob(new JobKey(jobId));
        Console.WriteLine($"Job '{jobId}' pausado");
    }
    
    public static async Task RetomarJob(string jobId)
    {
        await _scheduler.ResumeJob(new JobKey(jobId));
        Console.WriteLine($"Job '{jobId}' retomado");
    }
    
    public static async Task RemoverJob(string jobId)
    {
        await _scheduler.DeleteJob(new JobKey(jobId));
        Console.WriteLine($"Job '{jobId}' removido");
    }
    
    public static async Task PararScheduler()
    {
        await _scheduler.Shutdown();
        Console.WriteLine("Scheduler parado");
    }
}

// Uso
await QuartzHelper.IniciarScheduler();

await QuartzHelper.AgendarJob<RPADiarioJob>(
    "rpaDiario", 
    "0 0 8 * * ?", 
    "RPA executado diariamente às 8:00");

await QuartzHelper.AgendarJob<ProcessarPedidosJob>(
    "processarPedidos", 
    "0 */30 * * * ?", 
    "Processar pedidos a cada 30 minutos");

Console.ReadLine();
await QuartzHelper.PararScheduler();
```

---

## Configuração Avançada

### Persistência (Banco de Dados)

```csharp
using Quartz.Impl;

// Configurar para usar banco de dados
var properties = new NameValueCollection
{
    ["quartz.scheduler.instanceName"] = "MyScheduler",
    ["quartz.scheduler.instanceId"] = "AUTO",
    ["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz",
    ["quartz.jobStore.dataSource"] = "default",
    ["quartz.jobStore.tablePrefix"] = "QRTZ_",
    ["quartz.dataSource.default.connectionString"] = "Server=localhost;Database=Quartz;...",
    ["quartz.dataSource.default.provider"] = "SqlServer",
    ["quartz.serializer.type"] = "json"
};

ISchedulerFactory schedulerFactory = new StdSchedulerFactory(properties);
IScheduler scheduler = await schedulerFactory.GetScheduler();
```

---

## Boas Práticas

### 1. Use Async/Await

```csharp
public async Task Execute(IJobExecutionContext context)
{
    await MinhaOperacaoAsync();
}
```

### 2. Implemente Logging

```csharp
Log.Information("Job {JobId} iniciado", context.JobDetail.Key);
```

### 3. Trate Exceções

```csharp
try
{
    // código do job
}
catch (Exception ex)
{
    Log.Error(ex, "Erro no job");
    throw new JobExecutionException(ex);
}
```

### 4. Use Identificadores Descritivos

```csharp
.WithIdentity("processarPedidos", "vendas")
.WithDescription("Processa pedidos pendentes")
```

---

## Recursos Adicionais

- **Site Oficial**: https://www.quartz-scheduler.net/
- **Documentação**: https://www.quartz-scheduler.net/documentation/
- **Cron Expression Generator**: https://www.freeformatter.com/cron-expression-generator-quartz.html

---

**Versão:** 1.0  
**Última atualização:** Novembro 2025
