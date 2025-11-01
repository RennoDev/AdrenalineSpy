# Deploy de Automações RPA - Opções Gratuitas

## Índice
1. [Deploy Local](#deploy-local)
2. [GitHub Actions](#github-actions)
3. [Docker](#docker)
4. [Azure (Free Tier)](#azure-free-tier)
5. [Outras Opções Gratuitas](#outras-opções-gratuitas)

---

## Deploy Local

### Publicar Aplicação

```bash
# Publicar para Windows x64
dotnet publish -c Release -r win-x64 --self-contained true

# Publicar para Linux
dotnet publish -c Release -r linux-x64 --self-contained true

# Single File (tudo em um executável)
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

# Trimmed (reduzir tamanho)
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishTrimmed=true
```

Executável estará em: `bin/Release/net9.0/win-x64/publish/`

---

### Agendador do Windows (Task Scheduler)

**1. Publicar aplicação**

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

**2. Mover para pasta definitiva**

```
C:\RPAs\MeuRPA\MeuRPA.exe
```

**3. Criar tarefa no Task Scheduler**

- Abra "Agendador de Tarefas" (Task Scheduler)
- Criar Tarefa Básica
- Nome: "RPA Diário"
- Gatilho: Diariamente às 8:00
- Ação: Iniciar programa
- Programa: `C:\RPAs\MeuRPA\MeuRPA.exe`

**4. Configurações avançadas**

- Executar se o usuário está conectado ou não ✅
- Executar com privilégios mais altos (se necessário) ✅
- Se falhar, reiniciar a cada: 5 minutos (3 tentativas)

---

### Serviço Windows

```csharp
// Instalar pacote
dotnet add package Microsoft.Extensions.Hosting.WindowsServices

// Program.cs
using Microsoft.Extensions.Hosting;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "MeuRPAService";
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<RPAWorker>();
    })
    .Build();

await host.RunAsync();

// RPAWorker.cs
public class RPAWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Executar RPA
            await ExecutarRPA();
            
            // Aguardar próxima execução
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
```

**Instalar serviço:**

```powershell
# Publicar
dotnet publish -c Release

# Instalar (como admin)
sc create MeuRPAService binPath="C:\caminho\MeuRPA.exe"

# Iniciar
sc start MeuRPAService

# Parar
sc stop MeuRPAService

# Desinstalar
sc delete MeuRPAService
```

---

## GitHub Actions

### CI/CD Gratuito

**Vantagens:**
- ✅ 2000 minutos/mês grátis
- ✅ Automação completa
- ✅ Integração com GitHub

---

### Workflow de Build e Test

Crie `.github/workflows/build.yml`:

```yaml
name: Build and Test

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore -c Release
    
    - name: Test
      run: dotnet test --no-build -c Release --verbosity normal
```

---

### Workflow de Release

Crie `.github/workflows/release.yml`:

```yaml
name: Release

on:
  release:
    types: [created]

jobs:
  release:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Publish Windows x64
      run: dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o ./publish/win-x64
    
    - name: Publish Linux x64
      run: dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true -o ./publish/linux-x64
    
    - name: Upload Windows Artifact
      uses: actions/upload-artifact@v3
      with:
        name: MeuRPA-Windows
        path: ./publish/win-x64/*
    
    - name: Upload Linux Artifact
      uses: actions/upload-artifact@v3
      with:
        name: MeuRPA-Linux
        path: ./publish/linux-x64/*
```

---

### Workflow Agendado (Executar RPA)

```yaml
name: Run RPA

on:
  schedule:
    # Executar todo dia às 8:00 UTC (5:00 Brasília)
    - cron: '0 8 * * *'
  workflow_dispatch:  # Permitir execução manual

jobs:
  run-rpa:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Install dependencies
      run: dotnet restore
    
    - name: Run RPA
      run: dotnet run --project ./MeuRPA/MeuRPA.csproj
      env:
        API_KEY: ${{ secrets.API_KEY }}
        DB_CONNECTION: ${{ secrets.DB_CONNECTION }}
    
    - name: Upload logs
      if: always()
      uses: actions/upload-artifact@v3
      with:
        name: rpa-logs
        path: ./logs/*.log
```

---

## Docker

### Dockerfile

Crie `Dockerfile`:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy and restore
COPY *.csproj ./
RUN dotnet restore

# Copy everything and build
COPY . ./
RUN dotnet publish -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Instalar Playwright (se necessário)
# RUN pwsh /app/playwright.ps1 install

ENTRYPOINT ["dotnet", "MeuRPA.dll"]
```

---

### Build e Run

```bash
# Build
docker build -t meu-rpa:latest .

# Run
docker run -d \
  --name meu-rpa \
  -e API_KEY=sua_chave \
  -e DB_CONNECTION=sua_connection_string \
  -v $(pwd)/logs:/app/logs \
  meu-rpa:latest

# Ver logs
docker logs -f meu-rpa

# Parar
docker stop meu-rpa
```

---

### Docker Compose

Crie `docker-compose.yml`:

```yaml
version: '3.8'

services:
  rpa:
    build: .
    container_name: meu-rpa
    environment:
      - API_KEY=${API_KEY}
      - DB_CONNECTION=${DB_CONNECTION}
    volumes:
      - ./logs:/app/logs
      - ./data:/app/data
    restart: unless-stopped
    depends_on:
      - mysql
  
  mysql:
    image: mysql:8.0
    container_name: mysql-rpa
    environment:
      MYSQL_ROOT_PASSWORD: ${MYSQL_PASSWORD}
      MYSQL_DATABASE: rpa_db
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql

volumes:
  mysql_data:
```

```bash
# Iniciar tudo
docker compose up -d

# Ver logs
docker compose logs -f

# Parar
docker compose down
```

---

## Azure (Free Tier)

### Azure App Service (Free)

**Limitações:**
- 60 minutos/dia de CPU
- 1 GB RAM
- 1 GB storage

**Deploy:**

```bash
# Instalar Azure CLI
# https://docs.microsoft.com/cli/azure/install-azure-cli

# Login
az login

# Criar Resource Group
az group create --name rpa-rg --location brazilsouth

# Criar App Service Plan (Free)
az appservice plan create \
  --name rpa-plan \
  --resource-group rpa-rg \
  --sku FREE \
  --is-linux

# Criar Web App
az webapp create \
  --name meu-rpa-app \
  --resource-group rpa-rg \
  --plan rpa-plan \
  --runtime "DOTNET|9.0"

# Deploy
dotnet publish -c Release
cd bin/Release/net9.0/publish
zip -r app.zip .
az webapp deployment source config-zip \
  --resource-group rpa-rg \
  --name meu-rpa-app \
  --src app.zip
```

---

### Azure Functions (Free)

**Vantagens:**
- 1 milhão de execuções/mês grátis
- Ideal para RPAs agendados

```csharp
// Instalar
dotnet add package Microsoft.Azure.Functions.Worker
dotnet add package Microsoft.Azure.Functions.Worker.Sdk

// Function.cs
[Function("RPADiario")]
public void Run([TimerTrigger("0 0 8 * * *")] TimerInfo timer)
{
    Log.Information("RPA executando...");
    
    // Sua automação aqui
    ExecutarRPA();
    
    Log.Information("RPA concluído");
}
```

---

## Outras Opções Gratuitas

### 1. Google Cloud (Free Tier)

- Cloud Run: 2 milhões de requests/mês
- Cloud Functions: 2 milhões de invocações/mês

### 2. Railway.app

- $5/mês de crédito grátis
- Deploy fácil via GitHub
- Suporta Docker

### 3. Render.com

- Web Services gratuitos (com limitações)
- Cron Jobs gratuitos

### 4. Fly.io

- 3 VMs pequenas gratuitas
- Suporta Docker

### 5. Oracle Cloud (Always Free)

- 2 VMs gratuitas para sempre
- 1/8 OCPU e 1 GB RAM cada

---

## Configuração de Secrets

### GitHub Secrets

1. Repositório → Settings → Secrets → Actions
2. New repository secret
3. Nome: `API_KEY`, Valor: `sua_chave`

**Usar no workflow:**

```yaml
env:
  API_KEY: ${{ secrets.API_KEY }}
```

---

### Azure Key Vault

```csharp
// Instalar
dotnet add package Azure.Identity
dotnet add package Azure.Security.KeyVault.Secrets

// Usar
var client = new SecretClient(
    new Uri("https://seu-keyvault.vault.azure.net/"),
    new DefaultAzureCredential());

var secret = await client.GetSecretAsync("ApiKey");
string apiKey = secret.Value.Value;
```

---

## Monitoramento

### Application Insights (Azure - Free Tier)

```csharp
dotnet add package Microsoft.ApplicationInsights

// Configurar
services.AddApplicationInsightsTelemetry();

// Usar
_telemetryClient.TrackEvent("RPAIniciado");
_telemetryClient.TrackException(ex);
```

---

### Healthchecks

```csharp
// Endpoint de health
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow
}));
```

---

## Recomendações

### Para Estudo/Testes

1. **GitHub Actions** - CI/CD grátis
2. **Docker local** - Fácil de replicar
3. **Task Scheduler** - Windows simples

### Para Produção (Pequeno)

1. **Azure Functions** - Agendado, barato
2. **Docker + VPS barato** - Controle total
3. **Oracle Cloud Free Tier** - VMs gratuitas

### Para Produção (Médio)

1. **Azure App Service** - Escalável
2. **AWS EC2** - Controle total
3. **Kubernetes** - Para múltiplos RPAs

---

## Boas Práticas

### 1. Use Variáveis de Ambiente

```csharp
string apiKey = Environment.GetEnvironmentVariable("API_KEY");
```

### 2. Implemente Logs Estruturados

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/rpa-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

### 3. Configure Retry Logic

```csharp
for (int i = 0; i < 3; i++)
{
    try
    {
        // operação
        break;
    }
    catch when (i < 2)
    {
        await Task.Delay(1000);
    }
}
```

### 4. Monitore Saúde

- Healthcheck endpoint
- Logs centralizados
- Alertas de erro

---

## Recursos Adicionais

- **GitHub Actions**: https://docs.github.com/actions
- **Docker**: https://docs.docker.com/
- **Azure Free**: https://azure.microsoft.com/free/
- **Railway**: https://railway.app/
- **Render**: https://render.com/

---

**Versão:** 1.0  
**Última atualização:** Novembro 2025
