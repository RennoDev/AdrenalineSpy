# 🔧 AutomationSettings.json e Config.cs - Configuração do Projeto

## Índice
1. [O que é o Sistema de Configuração](#o-que-é)
2. [Como Instalar as Dependências](#como-instalar)
3. [Implementar o AutomationSettings.json](#implementar-json)
4. [Implementar o Config.cs](#implementar-config)
5. [Integração com as Tasks](#integração-tasks)
6. [Configurações Mais Usadas](#configurações-usadas)

---

## O que é o Sistema de Configuração {#o-que-é}

### 🎯 Exemplo Prático do Que Você Vai Criar

Ao final desta configuração, você terá:

1. **AutomationSettings.json**: Arquivo com suas credenciais e configurações
2. **Config.cs**: Classe que carrega o JSON automaticamente  
3. **Uso nas Tasks**: Acesso às configurações via `Config.Instancia`

```csharp
// Exemplo de uso final nas Tasks
var url = Config.Instancia.Navegacao.UrlBase;          // "https://adrenaline.com.br"
var timeout = Config.Instancia.Navegacao.TimeoutSegundos; // 30
var connString = Config.Instancia.ObterConnectionString(); // Connection string do MySQL
```

### 🏗️ Arquitetura do Sistema

O sistema de configuração do **AdrenalineSpy** utiliza o padrão **JSON → Config.cs → Tasks** para centralizar todas as configurações da aplicação. Este sistema é fundamental pois:

- **Separa configurações do código**: Credentials, URLs e timouts ficam em arquivo externo
- **Facilita deploy**: Diferentes ambientes (dev, prod) têm seus próprios JSONs
- **Aumenta segurança**: AutomationSettings.json fica no `.gitignore` (não vai pro GitHub)
- **Padrão Singleton**: Uma única instância de Config carregada em toda aplicação

**Onde é usado no AdrenalineSpy:**
- `Program.cs` → carrega `Config.Instancia` na inicialização
- `NavigationTask.cs` → usa `Config.Instancia.Navegacao` para browser settings
- `MigrationTask.cs` → usa `Config.Instancia.Database.ObterConnectionString()`
- `LoggingTask.cs` → usa `Config.Instancia.Logging` para configurar Serilog

---

## Como Instalar as Dependências {#como-instalar}

O sistema de configuração usa **Newtonsoft.Json** para deserialização:

```powershell
# Instalar o pacote para manipulação de JSON
dotnet add package Newtonsoft.Json
```

**Não precisa instalar mais nada** - o sistema usa apenas:
- `System.IO` para leitura de arquivos (nativo .NET)
- `Newtonsoft.Json` para deserialização
- Padrão Singleton (nativo C#)

---

## Implementar o AutomationSettings.json {#implementar-json}

### 1. Copiar o Arquivo Template

⚠️ **IMPORTANTE**: O `AutomationSettings.json` NÃO vai para o GitHub (fica no `.gitignore`). Suas credenciais ficam seguras.

```powershell
# Navegar até a pasta do projeto
cd C:\Users\lucas\OneDrive\Documentos\CsharpProjects\AdrenalineSpy

# Copiar o template (cria sua cópia local)
Copy-Item "AutomationSettings.example.json" "AutomationSettings.json"

# Verificar se o arquivo foi criado
ls AutomationSettings.json
```

### 2. Editar Suas Configurações

🔧 **PASSO CRÍTICO**: Abra `AutomationSettings.json` no VS Code e configure suas credenciais.

⚠️ **ATENÇÃO**: JSON não aceita comentários! Use apenas o formato limpo abaixo:

```json
{
  "Navegacao": {
    "UrlBase": "https://www.adrenaline.com.br",
    
    "TimeoutSegundos": 30,
    "HeadlessMode": false,
    "NavegadorPadrao": "chromium",
    "ViewportWidth": 1920,
    "ViewportHeight": 1080,
    "UserAgent": "",
    "BloquearImagens": true,
    "BloquearCSS": false
  },
  "Categorias": {
    "Tecnologia": "/tecnologia/",
    "Games": "/games/",
    "Hardware": "/hardware/"
  },
  "Scraping": {
    "IntervaloEntreRequests": 2000,
    "MaximoTentativas": 3,
    "DelayAposErro": 5000
  },
  "Database": {
    "Provider": "MySQL",
    "Host": "localhost",
    "Port": 3306,
    "NomeBanco": "adrenalinespy_db",
    "Usuario": "seu_usuario_aqui",
    "Senha": "sua_senha_aqui",
    "ConnectionTimeout": 30
  },
  "Logging": {
    "DiretorioLogs": "logs",
    "NivelMinimo": "Information",
    "ArquivoSucesso": "sucesso/log-{Date}.txt",
    "ArquivoFalha": "falha/log-{Date}.txt"
  },
  "Agendamento": {
    "HabilitarScheduler": true,
    "IntervaloMinutos": 60,
    "ExecutarAoIniciar": false
  },
  "Exportacao": {
    "HabilitarExportacao": true,
    "FormatoPadrao": "Excel",
    "DiretorioSaida": "exports"
  }
}
```

### 3. Explicação das Configurações

**📍 Navegacao**: Controla o comportamento do browser
- `UrlBase`: Site principal para scraping
- `TimeoutSegundos`: Tempo limite para operações (30 segundos é bom)
- `HeadlessMode`: `false` = mostra browser, `true` = background
- `NavegadorPadrao`: Use "chromium" (mais estável)
- `BloquearImagens`: `true` = mais rápido (recomendado)

**📁 Categorias**: URLs que serão raspadas
- Formato: `"Nome": "/url-relativa/"`
- Adicione/remova conforme necessário

**⚡ Scraping**: Rate limiting (respeitar servidor)
- `IntervaloEntreRequests`: 2000ms = 2 segundos entre requests
- `MaximoTentativas`: Retry em caso de erro
- `DelayAposErro`: Pausa antes de retry

**💾 Database**: Configurações do banco de dados
- ⚠️ **ALTERE**: `Usuario` e `Senha` com suas credenciais reais
- `Provider`: "MySQL" (padrão), "PostgreSQL" ou "SqlServer"
- `Host`: "localhost" se usando Docker local

**📝 Outras seções**: Mantenha como está inicialmente

---

## Implementar o Config.cs {#implementar-config}

### 1. Criar a Classe Principal Config.cs

```csharp
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

    // Propriedades principais - uma para cada seção do JSON
    public NavegacaoConfig Navegacao { get; set; } = new();
    public Dictionary<string, string> Categorias { get; set; } = new();
    public ScrapingConfig Scraping { get; set; } = new();
    public DatabaseConfig Database { get; set; } = new();
    public LoggingConfig Logging { get; set; } = new();
    public AgendamentoConfig Agendamento { get; set; } = new();
    public ExportacaoConfig Exportacao { get; set; } = new();

    // Singleton - Instância única acessível via Config.Instancia
    public static Config Instancia
    {
        get
        {
            if (_instancia == null)
            {
                _instancia = new Config();
                _instancia.Carregar(); // Carrega JSON automaticamente
            }
            return _instancia;
        }
    }

    // Construtor privado (padrão Singleton)
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

            // Copiar propriedades do JSON deserializado para esta instância
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
            throw; // Re-lança para parar execução se config falhar
        }
    }

    /// <summary>
    /// Valida se as configurações obrigatórias estão preenchidas
    /// </summary>
    public bool Validar()
    {
        var erros = new List<string>();

        // Validações obrigatórias
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

        // Mostrar erros se existirem
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
    /// Obtém a connection string do banco de dados baseada no provider
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
```

### 2. Classes de Configuração (uma para cada seção JSON)

```csharp
// Classes auxiliares para organização e tipagem das configurações vindas do JSON
// Valores padrão garantem funcionamento mesmo se JSON estiver incompleto

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
    public int IntervaloEntreRequests { get; set; } = 2000;  // 2 segundos
    public int MaximoTentativas { get; set; } = 3;
    public int DelayAposErro { get; set; } = 5000;           // 5 segundos
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
```

---

## Integração com as Tasks {#integração-tasks}

### 1. Program.cs - Carregar e Validar Config

```csharp
using AdrenalineSpy;

// Carregar configurações no início da aplicação
try
{
    var config = Config.Instancia; // Carrega automaticamente o JSON
    
    if (!config.Validar())
    {
        Console.WriteLine("❌ Configuração inválida. Verifique AutomationSettings.json");
        return;
    }
    
    Console.WriteLine($"🎯 Scraping configurado para: {config.Navegacao.UrlBase}");
    Console.WriteLine($"🗂️ Categorias: {string.Join(", ", config.Categorias.Keys)}");
}
catch (Exception ex)
{
    Console.WriteLine($"💥 Erro fatal na configuração: {ex.Message}");
    return;
}

// Executar workflow com configurações carregadas...
```

### 2. NavigationTask.cs - Usar Config.Navegacao

```csharp
using Microsoft.Playwright;
using AdrenalineSpy;
using AdrenalineSpy.Workflow.Tasks;

namespace AdrenalineSpy.Workflow.Tasks;

public class NavigationTask
{
    private IBrowser? _browser;
    private IPage? _pagina;

    public async Task<IBrowser> InicializarNavegador()
    {
        try
        {
            // Usar configurações do JSON via Config.Instancia
            var config = Config.Instancia.Navegacao;
            
            var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            
            var opcoes = new BrowserTypeLaunchOptions
            {
                Headless = config.HeadlessMode,
                Timeout = config.TimeoutSegundos * 1000 // Converter para ms
            };

            _browser = config.NavegadorPadrao.ToLower() switch
            {
                "chromium" => await playwright.Chromium.LaunchAsync(opcoes),
                "firefox" => await playwright.Firefox.LaunchAsync(opcoes),
                "webkit" => await playwright.Webkit.LaunchAsync(opcoes),
                _ => await playwright.Chromium.LaunchAsync(opcoes)
            };

            LoggingTask.RegistrarInfo($"✅ Navegador {config.NavegadorPadrao} inicializado");
            return _browser;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, "NavigationTask.InicializarNavegador");
            throw;
        }
    }

    public async Task<IPage> CriarPagina()
    {
        try
        {
            if (_browser == null)
                throw new InvalidOperationException("Browser não inicializado");

            var config = Config.Instancia.Navegacao;
            
            _pagina = await _browser.NewPageAsync(new BrowserNewPageOptions
            {
                ViewportSize = new ViewportSize 
                { 
                    Width = config.ViewportWidth, 
                    Height = config.ViewportHeight 
                },
                UserAgent = string.IsNullOrWhiteSpace(config.UserAgent) ? null : config.UserAgent
            });

            // Otimizações de performance baseadas no config
            if (config.BloquearImagens)
            {
                await _pagina.RouteAsync("**/*.{png,jpg,jpeg,gif,webp,svg}", route => route.AbortAsync());
            }

            if (config.BloquearCSS)
            {
                await _pagina.RouteAsync("**/*.css", route => route.AbortAsync());
            }

            LoggingTask.RegistrarInfo($"✅ Página criada - Viewport: {config.ViewportWidth}x{config.ViewportHeight}");
            return _pagina;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, "NavigationTask.CriarPagina");
            throw;
        }
    }

    public async Task<IPage> NavegarPara(string url)
    {
        try
        {
            if (_pagina == null)
                throw new InvalidOperationException("Página não criada");

            var config = Config.Instancia.Navegacao;
            
            await _pagina.GotoAsync(url, new PageGotoOptions
            {
                Timeout = config.TimeoutSegundos * 1000,
                WaitUntil = WaitUntilState.Load
            });

            LoggingTask.RegistrarInfo($"✅ Navegou para: {url}");
            return _pagina;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, $"NavigationTask.NavegarPara - URL: {url}");
            throw;
        }
    }
}
```

### 3. MigrationTask.cs - Usar Config.Database

```csharp
using System.Data;
using MySql.Data.MySqlClient;
using AdrenalineSpy;
using AdrenalineSpy.Workflow.Tasks;

namespace AdrenalineSpy.Workflow.Tasks;

public class MigrationTask
{
    public async Task<IDbConnection> ObterConexao()
    {
        try
        {
            // Connection string vem das configurações
            string connectionString = Config.Instancia.ObterConnectionString();
            
            var conexao = new MySqlConnection(connectionString);
            await conexao.OpenAsync();
            
            LoggingTask.RegistrarInfo("✅ Conexão com banco estabelecida");
            return conexao;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, "MigrationTask.ObterConexao");
            throw;
        }
    }
}
```

---

## Configurações Mais Usadas {#configurações-usadas}

### 1. Acesso às Configurações (Padrão Singleton)

```csharp
// ✅ SEMPRE usar Config.Instancia - carrega automaticamente
var config = Config.Instancia;

// ✅ Acessar seções específicas
var navegacaoConfig = Config.Instancia.Navegacao;
var databaseConfig = Config.Instancia.Database;
var categorias = Config.Instancia.Categorias;

// ❌ NUNCA criar nova instância
// var config = new Config(); // ERRADO - quebra Singleton
```

### 2. Configurações de Navegação Essenciais

```csharp
var nav = Config.Instancia.Navegacao;

// Modo de execução
bool modoVisivel = !nav.HeadlessMode;  // false = background, true = visível

// Timeouts
int timeoutMs = nav.TimeoutSegundos * 1000;  // Converter para millisegundos

// Otimizações (acelerar scraping)
bool bloquearImagens = nav.BloquearImagens;   // true = mais rápido
bool bloquearCSS = nav.BloquearCSS;           // true = quebra layout

// URL base para construir URLs relativas
string urlCompleta = $"{nav.UrlBase}{Config.Instancia.Categorias["Games"]}";
// Resultado: "https://www.adrenaline.com.br/games/"
```

### 3. Configurações de Database

```csharp
var db = Config.Instancia.Database;

// Connection string automática baseada no provider
string connStr = Config.Instancia.ObterConnectionString();

// Verificar provider
bool isMySQL = db.Provider.Equals("MySQL", StringComparison.OrdinalIgnoreCase);
bool isPostgreSQL = db.Provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase);
```

### 4. Configurações de Scraping (Rate Limiting)

```csharp
var scraping = Config.Instancia.Scraping;

// Delays para respeitar rate limit
await Task.Delay(scraping.IntervaloEntreRequests);  // Entre requests

// Retry logic
for (int tentativa = 1; tentativa <= scraping.MaximoTentativas; tentativa++)
{
    try
    {
        // Sua operação aqui...
        break; // Sucesso - sair do loop
    }
    catch (Exception ex) when (tentativa < scraping.MaximoTentativas)
    {
        LoggingTask.RegistrarErro(ex, $"Tentativa {tentativa} falhou");
        await Task.Delay(scraping.DelayAposErro);  // Delay antes de retry
    }
}
```

### 5. Iteração em Categorias

```csharp
var categorias = Config.Instancia.Categorias;

// Iterar por todas as categorias configuradas
foreach (var categoria in categorias)
{
    string nomeCategoria = categoria.Key;      // "Games", "Tecnologia"
    string urlRelativa = categoria.Value;      // "/games/", "/tecnologia/"
    
    string urlCompleta = $"{Config.Instancia.Navegacao.UrlBase}{urlRelativa}";
    
    Console.WriteLine($"🗂️ Processando {nomeCategoria}: {urlCompleta}");
}
```

### 6. Validação Antes de Executar

```csharp
// ✅ SEMPRE validar config antes de usar
try
{
    var config = Config.Instancia;
    
    if (!config.Validar())
    {
        Console.WriteLine("❌ Configuração inválida - verifique AutomationSettings.json");
        return; // Parar execução
    }
    
    // Continuar apenas se config estiver válida...
}
catch (FileNotFoundException ex)
{
    Console.WriteLine("❌ AutomationSettings.json não encontrado!");
    Console.WriteLine("💡 Copie AutomationSettings.example.json para AutomationSettings.json");
    return;
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Erro na configuração: {ex.Message}");
    return;
}
```

---

## 💡 Dicas de Uso

### Estrutura de Arquivos
```
AdrenalineSpy/
├── AutomationSettings.example.json  ← Template (vai pro Git)
├── AutomationSettings.json          ← Suas configs (NÃO vai pro Git)
├── Config.cs                        ← Classe Singleton
├── Program.cs                       ← Carrega Config.Instancia
└── Workflow/Tasks/                  ← Usam Config.Instancia.*
```

### Git Ignore
Adicione ao `.gitignore`:
```
AutomationSettings.json
logs/
exports/
```

### 🧪 Teste Completo - Passo a Passo

Execute estes comandos **na ordem** para validar sua configuração:

```powershell
# 1. Ir para o diretório do projeto
cd C:\Users\lucas\OneDrive\Documentos\CsharpProjects\AdrenalineSpy

# 2. Copiar o template
Copy-Item "AutomationSettings.example.json" "AutomationSettings.json"

# 3. Instalar dependência se ainda não tiver
dotnet add package Newtonsoft.Json

# 4. Verificar se compila
dotnet build
```

**✅ Se build passou**: Edite `AutomationSettings.json` e altere:
- `Database.Usuario`: Coloque seu usuário real
- `Database.Senha`: Coloque sua senha real

```powershell
# 5. Testar se carrega sem erros
dotnet run
```

**📋 Resultado esperado:**
```
✅ Configurações carregadas com sucesso!
✅ Configurações validadas com sucesso!
🎯 Scraping configurado para: https://www.adrenaline.com.br
🗂️ Categorias: Tecnologia, Games, Hardware
```

**❌ Se deu erro**: Veja seção Troubleshooting abaixo ⬇️

### 🚨 Troubleshooting - Soluções Rápidas

#### Erro: "FileNotFoundException: AutomationSettings.json"
```powershell
# Solução: Copiar o template
Copy-Item "AutomationSettings.example.json" "AutomationSettings.json"
```

#### Erro: "Falha ao deserializar configurações"
**Causa**: JSON com sintaxe incorreta (vírgula extra, aspas malformadas)
```powershell
# Validar JSON online
# Abra: https://jsonlint.com/
# Cole o conteúdo do seu AutomationSettings.json
# Corrija os erros apontados
```

#### Erro: "Newtonsoft.Json não encontrado"
```powershell
# Instalar dependência
dotnet add package Newtonsoft.Json
dotnet restore
```

#### Erro: "Provider 'MySql' não suportado" 
**Causa**: Maiúscula/minúscula incorreta
```json
// ❌ Errado
"Provider": "MySql"

// ✅ Correto  
"Provider": "MySQL"
```

#### Erro: "Config.Instancia é null"
**Causa**: Problema no padrão Singleton
```csharp
// ❌ Nunca faça isso
var config = new Config();

// ✅ Sempre use assim
var config = Config.Instancia;
```

#### Programa compila mas não executa nada
**Causa**: `Program.cs` ainda está vazio
**Solução**: Isso é normal! Você ainda vai implementar o workflow nas próximas etapas.