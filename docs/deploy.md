# Deploy - Implantação e Distribuição

## O que é Deploy

**Deploy** é o processo de implantação e distribuição da aplicação em ambientes de produção, incluindo configuração de infraestrutura, CI/CD, monitoramento e manutenção contínua.

**Onde é usado no AdrenalineSpy:**
- Implantação automatizada em servidores de produção
- Deploy em nuvem (Azure, AWS, Railway) para execução 24/7
- Configuração como serviço Windows para automação contínua
- CI/CD via GitHub Actions para atualizações automáticas
- Containerização Docker para portabilidade
- Monitoramento e alertas em produção

**Cenários típicos**: Servidor dedicado com agendamento, nuvem com banco gerenciado, container local, serviço Windows.

## Como Instalar

### Opção 1: Publicação Self-Contained (Recomendado)

```powershell
# Publicar para Windows x64 (completo)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Publicar para Linux x64 (para containers)
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true

# Publicar multiplataforma (requer .NET instalado)
dotnet publish -c Release --self-contained false
```

### Opção 2: Container Docker

```powershell
# Instalar Docker Desktop (Windows)
winget install Docker.DockerDesktop

# Ou via Chocolatey
choco install docker-desktop
```

### Opção 3: Ferramentas de Deploy Nuvem

```powershell
# Azure CLI
winget install Microsoft.AzureCLI

# Railway CLI  
npm install -g @railway/cli

# GitHub CLI (para Actions)
winget install GitHub.cli
```

## Implementar no AutomationSettings.json

Adicione configurações de deployment na raiz do JSON:

```json
{
  "Navegacao": {
    "UrlBase": "https://www.adrenaline.com.br",
    "DelayEntrePaginas": 2000
  },
  "Deploy": {
    "Ambiente": "Production",
    "Plataforma": "Windows",
    "TipoExecucao": "Scheduler",
    "ConfiguracaoServico": {
      "NomeServico": "AdrenalineSpyService",
      "DisplayName": "AdrenalineSpy - Monitor de Notícias",
      "Descricao": "Serviço de coleta automática de notícias do Adrenaline.com.br",
      "InicioAutomatico": true,
      "ContaExecucao": "LocalSystem"
    },
    "ConfiguracaoDocker": {
      "ImagemBase": "mcr.microsoft.com/dotnet/aspnet:9.0",
      "PortaContainer": 8080,
      "VariaveisAmbiente": {
        "ASPNETCORE_ENVIRONMENT": "Production",
        "DOTNET_ENVIRONMENT": "Production"
      },
      "VolumesPersistentes": [
        "/app/logs:/var/logs/adrenalinespy",
        "/app/exports:/var/exports/adrenalinespy",
        "/app/data:/var/data/adrenalinespy"
      ]
    },
    "ConfiguracaoNuvem": {
      "Provedor": "Railway",
      "RegiaoPreferida": "us-east-1",
      "InstanciaMinima": "1GB-RAM",
      "EscalamentoAutomatico": false,
      "VariaveisAmbiente": {
        "DATABASE_URL": "${DATABASE_URL}",
        "ENVIRONMENT": "Production"
      }
    },
    "Monitoramento": {
      "HabilitarHealthCheck": true,
      "PortaHealthCheck": 8081,
      "IntervaloPing": 300000,
      "UrlWebhookAlertas": "",
      "NotificarFalhas": true
    }
  },
  "Database": {
    "ConnectionString": "${DATABASE_CONNECTION_STRING}"
  },
  "Logging": {
    "Nivel": "Information",
    "CaminhoArquivo": "logs/adrenaline-spy.log"
  }
}
```

**Configurações específicas de Deploy:**
- **`Ambiente`**: Development, Staging, Production
- **`ConfiguracaoServico`**: Para serviços Windows
- **`ConfiguracaoDocker`**: Containerização
- **`ConfiguracaoNuvem`**: Deploy em cloud providers

## Implementar no Config.cs

Adicione classes de configuração para deploy:

```csharp
public class ConfiguracaoServicoConfig
{
    public string NomeServico { get; set; } = "AdrenalineSpyService";
    public string DisplayName { get; set; } = "AdrenalineSpy - Monitor de Notícias";
    public string Descricao { get; set; } = "Serviço de coleta automática de notícias do Adrenaline.com.br";
    public bool InicioAutomatico { get; set; } = true;
    public string ContaExecucao { get; set; } = "LocalSystem";
}

public class ConfiguracaoDockerConfig
{
    public string ImagemBase { get; set; } = "mcr.microsoft.com/dotnet/aspnet:9.0";
    public int PortaContainer { get; set; } = 8080;
    public Dictionary<string, string> VariaveisAmbiente { get; set; } = new();
    public List<string> VolumesPersistentes { get; set; } = new();
}

public class ConfiguracaoNuvemConfig
{
    public string Provedor { get; set; } = "Railway";
    public string RegiaoPreferida { get; set; } = "us-east-1";
    public string InstanciaMinima { get; set; } = "1GB-RAM";
    public bool EscalamentoAutomatico { get; set; } = false;
    public Dictionary<string, string> VariaveisAmbiente { get; set; } = new();
}

public class MonitoramentoConfig
{
    public bool HabilitarHealthCheck { get; set; } = true;
    public int PortaHealthCheck { get; set; } = 8081;
    public int IntervaloPing { get; set; } = 300000;
    public string UrlWebhookAlertas { get; set; } = string.Empty;
    public bool NotificarFalhas { get; set; } = true;
}

public class DeployConfig
{
    public string Ambiente { get; set; } = "Development";
    public string Plataforma { get; set; } = "Windows";
    public string TipoExecucao { get; set; } = "Console";
    public ConfiguracaoServicoConfig ConfiguracaoServico { get; set; } = new();
    public ConfiguracaoDockerConfig ConfiguracaoDocker { get; set; } = new();
    public ConfiguracaoNuvemConfig ConfiguracaoNuvem { get; set; } = new();
    public MonitoramentoConfig Monitoramento { get; set; } = new();
}

public class Config
{
    // ... propriedades existentes ...
    public DeployConfig Deploy { get; set; } = new();
    
    /// <summary>
    /// Verifica se está executando em produção
    /// </summary>
    public bool IsProducao()
    {
        return Deploy.Ambiente.Equals("Production", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Obtém string de conexão resolvendo variáveis de ambiente
    /// </summary>
    public string ObterConnectionStringProdução()
    {
        var connectionString = Database.ConnectionString;
        
        // Resolver variáveis de ambiente
        if (connectionString.Contains("${"))
        {
            connectionString = Environment.ExpandEnvironmentVariables(connectionString);
        }
        
        return connectionString;
    }

    /// <summary>
    /// Configura aplicação para ambiente de produção
    /// </summary>
    public void ConfigurarProducao()
    {
        if (IsProducao())
        {
            // Ajustar configurações para produção
            Logging.Nivel = "Warning"; // Menos logs em produção
            Navegacao.DelayEntrePaginas = Math.Max(Navegacao.DelayEntrePaginas, 3000); // Mais respeitoso
            
            LoggingTask.RegistrarInfo("🏭 Configurações de produção aplicadas");
        }
    }
}
```

## Montar nas Tasks

Crie a classe `DeploymentTask.cs` na pasta `Workflow/Tasks/`:

```csharp
using System.Diagnostics;
using System.Net;
using System.ServiceProcess;
using System.Text.Json;

namespace AdrenalineSpy.Workflow.Tasks;

/// <summary>
/// Gerencia operações de deployment e monitoramento para o AdrenalineSpy
/// </summary>
public static class DeploymentTask
{
    private static HttpListener? _healthCheckListener;
    private static readonly CancellationTokenSource _cancellationTokenSource = new();

    /// <summary>
    /// Inicializa configurações de produção e monitoramento
    /// </summary>
    public static async Task<bool> InicializarAmbienteProducao()
    {
        try
        {
            LoggingTask.RegistrarInfo("🚀 Inicializando ambiente de produção");

            // Aplicar configurações de produção
            Config.Instancia.ConfigurarProducao();

            // Verificar dependências
            await VerificarDependencias();

            // Inicializar health check
            if (Config.Instancia.Deploy.Monitoramento.HabilitarHealthCheck)
            {
                await InicializarHealthCheck();
            }

            // Configurar handlers de sinalização
            ConfigurarSinalizacaoSistema();

            LoggingTask.RegistrarInfo("✅ Ambiente de produção inicializado com sucesso");
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao inicializar ambiente de produção", ex);
            return false;
        }
    }

    /// <summary>
    /// Verifica dependências necessárias para execução
    /// </summary>
    private static async Task VerificarDependencias()
    {
        try
        {
            LoggingTask.RegistrarInfo("🔍 Verificando dependências do sistema");

            // Verificar conectividade com banco
            var bancoDispo = await VerificarConectividadeBanco();
            if (!bancoDispo)
            {
                throw new Exception("Banco de dados não acessível");
            }

            // Verificar conectividade com site alvo
            var siteDisponivel = await VerificarConectividadeSite();
            if (!siteDisponivel)
            {
                LoggingTask.RegistrarAviso("⚠️ Site alvo não acessível no momento");
            }

            // Verificar espaço em disco
            var espacoOk = await VerificarEspacoDisco();
            if (!espacoOk)
            {
                LoggingTask.RegistrarAviso("⚠️ Pouco espaço em disco disponível");
            }

            LoggingTask.RegistrarInfo("✅ Verificação de dependências concluída");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro na verificação de dependências", ex);
            throw;
        }
    }

    /// <summary>
    /// Inicializa endpoint de health check
    /// </summary>
    private static async Task InicializarHealthCheck()
    {
        try
        {
            var porta = Config.Instancia.Deploy.Monitoramento.PortaHealthCheck;
            var prefixo = $"http://+:{porta}/";

            _healthCheckListener = new HttpListener();
            _healthCheckListener.Prefixes.Add(prefixo);
            _healthCheckListener.Start();

            LoggingTask.RegistrarInfo($"🏥 Health check ativo na porta {porta}");

            // Processar requisições em background
            _ = Task.Run(async () => await ProcessarHealthCheckRequests(), _cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Aviso ao inicializar health check: {ex.Message}");
        }
    }

    /// <summary>
    /// Processa requisições de health check
    /// </summary>
    private static async Task ProcessarHealthCheckRequests()
    {
        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested && _healthCheckListener?.IsListening == true)
            {
                var context = await _healthCheckListener.GetContextAsync();
                
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ProcessarHealthCheckRequest(context);
                    }
                    catch (Exception ex)
                    {
                        LoggingTask.RegistrarAviso($"Erro no health check: {ex.Message}");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Erro no loop de health check: {ex.Message}");
        }
    }

    /// <summary>
    /// Processa uma requisição individual de health check
    /// </summary>
    private static async Task ProcessarHealthCheckRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            if (request.Url?.AbsolutePath == "/health")
            {
                var status = await GerarStatusSaude();
                var json = JsonSerializer.Serialize(status, new JsonSerializerOptions { WriteIndented = true });

                response.ContentType = "application/json";
                response.StatusCode = status.Healthy ? 200 : 503;

                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                response.StatusCode = 404;
            }
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Erro ao processar health check: {ex.Message}");
            response.StatusCode = 500;
        }
        finally
        {
            response.Close();
        }
    }

    /// <summary>
    /// Gera status de saúde do sistema
    /// </summary>
    private static async Task<HealthStatus> GerarStatusSaude()
    {
        try
        {
            var status = new HealthStatus
            {
                Timestamp = DateTime.UtcNow,
                Uptime = DateTime.Now - Process.GetCurrentProcess().StartTime,
                Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "Unknown"
            };

            // Verificar componentes
            status.Components["Database"] = await VerificarConectividadeBanco();
            status.Components["TargetSite"] = await VerificarConectividadeSite();
            status.Components["DiskSpace"] = await VerificarEspacoDisco();
            status.Components["Scheduler"] = VerificarStatusScheduler();

            // Status geral
            status.Healthy = status.Components.Values.All(v => v);

            return status;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao gerar status de saúde", ex);
            return new HealthStatus { Healthy = false, Error = ex.Message };
        }
    }

    /// <summary>
    /// Verifica conectividade com banco de dados
    /// </summary>
    private static async Task<bool> VerificarConectividadeBanco()
    {
        try
        {
            // TODO: Implementar teste real de conexão com banco
            // return await MigrationTask.TestarConexao();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Verifica conectividade com site alvo
    /// </summary>
    private static async Task<bool> VerificarConectividadeSite()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            
            var response = await client.GetAsync(Config.Instancia.Navegacao.UrlBase);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Verifica espaço disponível em disco
    /// </summary>
    private static async Task<bool> VerificarEspacoDisco()
    {
        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:");
            var espacoLivreGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
            
            return espacoLivreGB > 1; // Mínimo 1GB livre
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Verifica status do scheduler Quartz
    /// </summary>
    private static bool VerificarStatusScheduler()
    {
        try
        {
            // TODO: Verificar se scheduler está ativo
            // return SchedulingTask.SchedulerAtivo();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Configura handlers para sinais do sistema
    /// </summary>
    private static void ConfigurarSinalizacaoSistema()
    {
        try
        {
            // Handler para CTRL+C
            Console.CancelKeyPress += (s, e) =>
            {
                LoggingTask.RegistrarInfo("🛑 Sinal de interrupção recebido, finalizando aplicação...");
                e.Cancel = true;
                _ = Task.Run(FinalizarAplicacaoGraciosamente);
            };

            // Handler para fechamento do processo
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                LoggingTask.RegistrarInfo("🔚 Processo finalizando, limpando recursos...");
                FinalizarRecursos();
            };

            LoggingTask.RegistrarInfo("📡 Handlers de sinalização configurados");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Erro ao configurar sinalização: {ex.Message}");
        }
    }

    /// <summary>
    /// Finaliza aplicação de forma graciosa
    /// </summary>
    private static async Task FinalizarAplicacaoGraciosamente()
    {
        try
        {
            LoggingTask.RegistrarInfo("🛑 Iniciando finalização graciosa...");

            // Parar scheduler se estiver ativo
            await SchedulingTask.FinalizarScheduler();

            // Finalizar health check
            FinalizarHealthCheck();

            // Liberar outros recursos
            FinalizarRecursos();

            LoggingTask.RegistrarInfo("✅ Finalização graciosa concluída");
            
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro na finalização graciosa", ex);
            Environment.Exit(1);
        }
    }

    /// <summary>
    /// Finaliza health check listener
    /// </summary>
    private static void FinalizarHealthCheck()
    {
        try
        {
            _cancellationTokenSource.Cancel();
            _healthCheckListener?.Stop();
            _healthCheckListener?.Close();
            
            LoggingTask.RegistrarInfo("🏥 Health check finalizado");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Aviso ao finalizar health check: {ex.Message}");
        }
    }

    /// <summary>
    /// Libera recursos gerais
    /// </summary>
    private static void FinalizarRecursos()
    {
        try
        {
            // Finalizar automação desktop
            DesktopAutomationTask.LimparRecursos();
            
            // Finalizar interface gráfica
            GUITask.FinalizarInterface();
            
            LoggingTask.RegistrarInfo("🧹 Recursos liberados");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Aviso ao liberar recursos: {ex.Message}");
        }
    }

    /// <summary>
    /// Instala aplicação como serviço Windows
    /// </summary>
    public static async Task<bool> InstalarServicoWindows(string caminhoExecutavel)
    {
        try
        {
            var config = Config.Instancia.Deploy.ConfiguracaoServico;
            
            LoggingTask.RegistrarInfo($"🔧 Instalando serviço Windows: {config.NomeServico}");

            var comando = $"""
                sc create "{config.NomeServico}" 
                binPath= "\"{caminhoExecutavel}\" --scheduler" 
                DisplayName= "{config.DisplayName}" 
                start= auto 
                obj= {config.ContaExecucao}
                """;

            var resultado = await ExecutarComandoSistema(comando);
            
            if (resultado.Sucesso)
            {
                LoggingTask.RegistrarInfo("✅ Serviço Windows instalado com sucesso");
                return true;
            }
            else
            {
                LoggingTask.RegistrarErro($"Erro ao instalar serviço: {resultado.Saida}");
                return false;
            }
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao instalar serviço Windows", ex);
            return false;
        }
    }

    /// <summary>
    /// Remove serviço Windows
    /// </summary>
    public static async Task<bool> RemoverServicoWindows()
    {
        try
        {
            var nomeServico = Config.Instancia.Deploy.ConfiguracaoServico.NomeServico;
            
            LoggingTask.RegistrarInfo($"🗑️ Removendo serviço Windows: {nomeServico}");

            var comando = $"sc delete \"{nomeServico}\"";
            var resultado = await ExecutarComandoSistema(comando);

            if (resultado.Sucesso)
            {
                LoggingTask.RegistrarInfo("✅ Serviço Windows removido com sucesso");
                return true;
            }
            else
            {
                LoggingTask.RegistrarErro($"Erro ao remover serviço: {resultado.Saida}");
                return false;
            }
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao remover serviço Windows", ex);
            return false;
        }
    }

    /// <summary>
    /// Executa comando do sistema
    /// </summary>
    private static async Task<(bool Sucesso, string Saida)> ExecutarComandoSistema(string comando)
    {
        try
        {
            var processo = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {comando}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            processo.Start();
            var saida = await processo.StandardOutput.ReadToEndAsync();
            var erro = await processo.StandardError.ReadToEndAsync();
            await processo.WaitForExitAsync();

            var sucesso = processo.ExitCode == 0;
            var resultado = sucesso ? saida : erro;

            return (sucesso, resultado);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}

/// <summary>
/// Modelo para status de saúde
/// </summary>
public class HealthStatus
{
    public bool Healthy { get; set; }
    public DateTime Timestamp { get; set; }
    public TimeSpan Uptime { get; set; }
    public string Version { get; set; } = string.Empty;
    public string? Error { get; set; }
    public Dictionary<string, bool> Components { get; set; } = new();
}
```

## Métodos Mais Usados

### Build e Publicação Self-Contained

```powershell
# Build para produção (Windows x64)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/win-x64

# Build para Linux (containers)
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/linux-x64

# Build multiplataforma (menor, requer .NET)
dotnet publish -c Release --self-contained false -o ./publish/portable
```

### Dockerfile Completo

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Instalar dependências do Playwright
RUN apt-get update && apt-get install -y \
    libnss3 \
    libatk-bridge2.0-0 \
    libdrm2 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxrandr2 \
    libgbm1 \
    libxss1 \
    libasound2 \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["AdrenalineSpy.csproj", "."]
RUN dotnet restore "AdrenalineSpy.csproj"
COPY . .
RUN dotnet build "AdrenalineSpy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AdrenalineSpy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Criar diretórios para volumes
RUN mkdir -p /var/logs/adrenalinespy /var/exports/adrenalinespy /var/data/adrenalinespy

# Instalar browsers do Playwright
RUN dotnet AdrenalineSpy.dll --install-playwright || true

EXPOSE 8080 8081
ENTRYPOINT ["dotnet", "AdrenalineSpy.dll", "--scheduler"]
```

### Docker Compose para Stack Completa

```yaml
# docker-compose.yml
version: '3.8'
services:
  adrenalinespy:
    build: .
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - DATABASE_CONNECTION_STRING=Server=mysql;Database=adrenalinespy;User=root;Password=senha123;
    ports:
      - "8080:8080"  # Health check
      - "8081:8081"  # Monitoring
    volumes:
      - ./logs:/var/logs/adrenalinespy
      - ./exports:/var/exports/adrenalinespy
      - ./data:/var/data/adrenalinespy
    depends_on:
      - mysql
    restart: unless-stopped

  mysql:
    image: mysql:8.0
    environment:
      - MYSQL_ROOT_PASSWORD=senha123
      - MYSQL_DATABASE=adrenalinespy
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
    restart: unless-stopped

volumes:
  mysql_data:
```

### Deploy em Railway

```bash
# railway.toml
[build]
builder = "DOCKERFILE"
buildCommand = "dotnet publish -c Release -o /app/publish"

[deploy]
healthcheckPath = "/health"
healthcheckTimeout = 300
restartPolicyType = "ON_FAILURE"

[[deploy.environmentVariables]]
name = "ASPNETCORE_ENVIRONMENT"
value = "Production"

[[deploy.environmentVariables]]
name = "DATABASE_CONNECTION_STRING"
value = "${{DATABASE_URL}}"
```

```powershell
# Comandos Railway
railway login
railway init
railway add --database mysql
railway deploy
railway logs
```

### GitHub Actions CI/CD

```yaml
# .github/workflows/deploy.yml
name: Deploy AdrenalineSpy

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-test:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore -c Release
      
    - name: Test
      run: dotnet test --no-build -c Release --verbosity normal
      
    - name: Publish
      run: dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
      
    - name: Upload artifacts
      uses: actions/upload-artifact@v3
      with:
        name: adrenalinespy-win-x64
        path: ./publish/

  deploy-production:
    needs: build-and-test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    
    steps:
    - name: Deploy to Railway
      run: |
        curl -X POST ${{ secrets.RAILWAY_WEBHOOK_URL }}
```

### Instalar como Serviço Windows

```powershell
# Script PowerShell para instalação
param(
    [string]$CaminhoExecutavel = ".\AdrenalineSpy.exe",
    [switch]$Instalar,
    [switch]$Remover,
    [switch]$Status
)

$NomeServico = "AdrenalineSpyService"

if ($Instalar) {
    Write-Host "🔧 Instalando serviço $NomeServico..."
    
    sc.exe create $NomeServico binPath= "`"$CaminhoExecutavel`" --scheduler" DisplayName= "AdrenalineSpy Monitor" start= auto
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Serviço instalado com sucesso!"
        Write-Host "▶️ Iniciando serviço..."
        sc.exe start $NomeServico
    }
}

if ($Remover) {
    Write-Host "🛑 Parando serviço $NomeServico..."
    sc.exe stop $NomeServico
    
    Write-Host "🗑️ Removendo serviço $NomeServico..."
    sc.exe delete $NomeServico
    
    Write-Host "✅ Serviço removido!"
}

if ($Status) {
    Write-Host "📊 Status do serviço $NomeServico:"
    sc.exe query $NomeServico
}
```

### Monitoramento e Health Checks

```csharp
// Endpoint de health check personalizado
public static async Task<HealthStatus> VerificarSaudeCompleta()
{
    var status = new HealthStatus
    {
        Timestamp = DateTime.UtcNow,
        Healthy = true,
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown"
    };

    // Verificar banco de dados
    try
    {
        status.Components["Database"] = await MigrationTask.TestarConexao();
    }
    catch
    {
        status.Components["Database"] = false;
        status.Healthy = false;
    }

    // Verificar site alvo
    try
    {
        using var client = new HttpClient();
        var response = await client.GetAsync(Config.Instancia.Navegacao.UrlBase);
        status.Components["TargetSite"] = response.IsSuccessStatusCode;
    }
    catch
    {
        status.Components["TargetSite"] = false;
    }

    // Verificar scheduler
    status.Components["Scheduler"] = SchedulingTask.SchedulerAtivo();

    // Verificar uso de memória
    var processo = Process.GetCurrentProcess();
    var memoriaUsadaMB = processo.WorkingSet64 / (1024 * 1024);
    status.Components["Memory"] = memoriaUsadaMB < 512; // Limite 512MB

    LoggingTask.RegistrarInfo($"🏥 Health check: {(status.Healthy ? "Saudável" : "Com problemas")}");
    return status;
}
```

### Script de Deploy Automático

```powershell
# deploy.ps1 - Script completo de deploy
param(
    [string]$Ambiente = "Production",
    [switch]$Docker,
    [switch]$Servico,
    [switch]$Railway
)

Write-Host "🚀 Iniciando deploy do AdrenalineSpy para $Ambiente" -ForegroundColor Green

# 1. Build da aplicação
Write-Host "🔨 Building aplicação..."
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./deploy

if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Falha no build"
    exit 1
}

# 2. Deploy baseado no tipo escolhido
if ($Docker) {
    Write-Host "🐳 Deploy com Docker..."
    docker build -t adrenalinespy:latest .
    docker-compose up -d
}
elseif ($Servico) {
    Write-Host "🔧 Instalando como serviço Windows..."
    .\deploy\AdrenalineSpy.exe --install-service
}
elseif ($Railway) {
    Write-Host "🚂 Deploy no Railway..."
    railway deploy
}
else {
    Write-Host "📁 Deploy local concluído em ./deploy/"
}

Write-Host "✅ Deploy concluído com sucesso!" -ForegroundColor Green
```
