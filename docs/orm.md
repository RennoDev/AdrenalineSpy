# 💾 ORM - Entity Framework Core e Dapper no AdrenalineSpy

## O que é

**ORM (Object-Relational Mapping):** Ferramenta que mapeia objetos C# para tabelas do banco de dados  
**Entity Framework Core:** ORM completo com LINQ, migrations e change tracking  
**Dapper:** Micro-ORM ultra-performático para queries SQL diretas

**Por que usar no AdrenalineSpy:**
- Persistir dados extraídos do Adrenaline.com.br no banco Docker
- MigrationTask.cs usa ORM para salvar notícias, categorias, timestamps
- Entity Framework para CRUD simples, Dapper para queries de performance
- Suporte aos 3 bancos: MySQL, PostgreSQL, SQL Server (configurável via JSON)

**Onde é usado no AdrenalineSpy:**
- MigrationTask.cs persiste dados extraídos pelo Playwright
- Models: Noticia.cs, Categoria.cs, LogExecucao.cs
- Relatórios e exportação de dados históricos
- Queries de análise (tendências, categorias mais populares)

**Posição no fluxo:** Etapa 7 de 17 - implementar APÓS Playwright (precisa dos dados extraídos)

## Como Instalar

### 1. Instalar Pacotes Entity Framework Core

```powershell
# Navegar até o projeto
cd C:\Users\lucas\OneDrive\Documentos\CsharpProjects\AdrenalineSpy

# Entity Framework Core base
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Tools

# Provider MySQL (padrão do projeto)
dotnet add package Pomelo.EntityFrameworkCore.MySql

# Provider PostgreSQL (alternativo)
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

# Provider SQL Server (alternativo)
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# Verificar instalação
dotnet list package | findstr EntityFrameworkCore
```

### 2. Instalar Dapper (Micro-ORM)

```powershell
# Dapper base
dotnet add package Dapper

# Providers de conexão (mesmo do EF Core)
dotnet add package MySql.Data              # MySQL
dotnet add package Npgsql                 # PostgreSQL  
dotnet add package Microsoft.Data.SqlClient # SQL Server

# Verificar instalação
dotnet list package | findstr Dapper
```

### 3. Verificar .csproj

Confirme que os pacotes foram adicionados:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- Entity Framework Core -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />
    <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.2" />
    
    <!-- Dapper -->
    <PackageReference Include="Dapper" Version="2.1.35" />
    <PackageReference Include="MySql.Data" Version="9.0.0" />
    
    <!-- Outros pacotes já existentes -->
    <PackageReference Include="Serilog" Version="4.0.2" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
</Project>
```

## Implementar no AutomationSettings.json

A seção `Database` no JSON já está configurada para múltiplos providers:

```json
{
  "Database": {
    "Provider": "MySQL",
    "Host": "localhost",
    "Port": 3306,
    "NomeBanco": "adrenalinespy_db",
    "Usuario": "seu_usuario_aqui",
    "Senha": "sua_senha_aqui",
    "ConnectionTimeout": 30
  }
}
```

**Configurações suportadas:**

- **`Provider`**: Tipo do banco (`"MySQL"`, `"PostgreSQL"`, `"SqlServer"`)
- **`Host`**: Endereço do servidor (localhost para Docker local)
- **`Port`**: Porta do banco (3306=MySQL, 5432=PostgreSQL, 1433=SQL Server)
- **`NomeBanco`**: Nome da database a ser criada/usada
- **`Usuario`**: Usuário com permissões de DDL/DML
- **`Senha`**: Senha do usuário (colocar no arquivo real, não no example)
- **`ConnectionTimeout`**: Timeout de conexão em segundos

**Exemplos para cada provider:**

```json
// MySQL (padrão)
{
  "Provider": "MySQL",
  "Host": "localhost",
  "Port": 3306,
  "NomeBanco": "adrenalinespy_db"
}

// PostgreSQL
{
  "Provider": "PostgreSQL", 
  "Host": "localhost",
  "Port": 5432,
  "NomeBanco": "adrenalinespy_db"
}

// SQL Server
{
  "Provider": "SqlServer",
  "Host": "localhost",
  "Port": 1433,
  "NomeBanco": "AdrenalineSpyDB"
}
```

## Implementar no Config.cs

A classe `DatabaseConfig` e `ObterConnectionString()` já estão implementadas:

```csharp
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

// Método para obter connection string baseado no provider
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
```

**Usar no código:**
```csharp
var config = Config.Instancia;
var connectionString = config.ObterConnectionString();
// connectionString já formatada para o provider configurado
```

## Montar nas Tasks

### 1. Criar Models do AdrenalineSpy

Crie a pasta `Models/` e os arquivos:

**Models/Noticia.cs:**
```csharp
namespace AdrenalineSpy.Models;

public class Noticia
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public string Resumo { get; set; } = string.Empty;
    public DateTime DataPublicacao { get; set; }
    public DateTime DataExtracao { get; set; }
    public string UrlImagem { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public int Visualizacoes { get; set; } = 0;
    
    // Navegação
    public List<TagNoticia> Tags { get; set; } = new();
}
```

**Models/Categoria.cs:**
```csharp
namespace AdrenalineSpy.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string UrlCategoria { get; set; } = string.Empty;
    public int TotalNoticias { get; set; } = 0;
    public DateTime UltimaAtualizacao { get; set; }
    
    // Navegação
    public List<Noticia> Noticias { get; set; } = new();
}
```

**Models/LogExecucao.cs:**
```csharp
namespace AdrenalineSpy.Models;

public class LogExecucao
{
    public int Id { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string Status { get; set; } = string.Empty; // Sucesso, Erro, Executando
    public int NoticiasExtraidas { get; set; } = 0;
    public int NoticiasNovas { get; set; } = 0;
    public string? MensagemErro { get; set; }
    public int TempoExecucaoSegundos { get; set; } = 0;
}
```

**Models/TagNoticia.cs:**
```csharp
namespace AdrenalineSpy.Models;

public class TagNoticia
{
    public int NoticiaId { get; set; }
    public string Tag { get; set; } = string.Empty;
    
    // Navegação
    public Noticia Noticia { get; set; } = null!;
}
```

### 2. Criar AdrenalineDbContext

**Data/AdrenalineDbContext.cs:**
```csharp
using Microsoft.EntityFrameworkCore;
using AdrenalineSpy.Models;

namespace AdrenalineSpy.Data;

public class AdrenalineDbContext : DbContext
{
    private readonly string _connectionString;

    public AdrenalineDbContext()
    {
        var config = Config.Instancia;
        _connectionString = config.ObterConnectionString();
    }

    public DbSet<Noticia> Noticias { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<LogExecucao> LogsExecucao { get; set; }
    public DbSet<TagNoticia> TagsNoticias { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = Config.Instancia;
        
        switch (config.Database.Provider.ToLower())
        {
            case "mysql":
                optionsBuilder.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString));
                break;
                
            case "postgresql":
                optionsBuilder.UseNpgsql(_connectionString);
                break;
                
            case "sqlserver":
                optionsBuilder.UseSqlServer(_connectionString);
                break;
                
            default:
                throw new NotSupportedException($"Provider '{config.Database.Provider}' não suportado");
        }
        
        // Configurações gerais
        optionsBuilder.EnableSensitiveDataLogging(false);
        optionsBuilder.EnableServiceProviderCaching(true);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurar Noticia
        modelBuilder.Entity<Noticia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titulo).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Url).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Categoria).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Conteudo).HasColumnType("LONGTEXT");
            entity.Property(e => e.Resumo).HasMaxLength(1000);
            entity.Property(e => e.UrlImagem).HasMaxLength(1000);
            entity.Property(e => e.Autor).HasMaxLength(200);
            
            entity.HasIndex(e => e.Url).IsUnique();
            entity.HasIndex(e => e.Categoria);
            entity.HasIndex(e => e.DataPublicacao);
        });

        // Configurar Categoria
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UrlCategoria).HasMaxLength(500).IsRequired();
            
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // Configurar LogExecucao
        modelBuilder.Entity<LogExecucao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.MensagemErro).HasMaxLength(2000);
        });

        // Configurar TagNoticia (chave composta)
        modelBuilder.Entity<TagNoticia>(entity =>
        {
            entity.HasKey(e => new { e.NoticiaId, e.Tag });
            entity.Property(e => e.Tag).HasMaxLength(100).IsRequired();
            
            entity.HasOne(e => e.Noticia)
                  .WithMany(n => n.Tags)
                  .HasForeignKey(e => e.NoticiaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

### 3. Criar MigrationTask.cs

**Workflow/Tasks/MigrationTask.cs:**
```csharp
using Microsoft.EntityFrameworkCore;
using AdrenalineSpy.Data;
using AdrenalineSpy.Models;
using Dapper;
using MySql.Data.MySqlConnection;

namespace AdrenalineSpy.Workflow.Tasks;

public class MigrationTask
{
    private readonly AdrenalineDbContext _context;
    private readonly string _connectionString;

    public MigrationTask()
    {
        _context = new AdrenalineDbContext();
        var config = Config.Instancia;
        _connectionString = config.ObterConnectionString();
    }

    /// <summary>
    /// Garante que o banco de dados existe e está atualizado
    /// </summary>
    public async Task InicializarBancoAsync()
    {
        try
        {
            LoggingTask.RegistrarInfo("Inicializando banco de dados...");
            
            // Criar banco se não existir e aplicar migrations
            await _context.Database.EnsureCreatedAsync();
            
            // Verificar se há migrations pendentes
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                LoggingTask.RegistrarInfo($"Aplicando {pendingMigrations.Count()} migrations pendentes...");
                await _context.Database.MigrateAsync();
            }
            
            LoggingTask.RegistrarInfo("Banco de dados inicializado com sucesso");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, "MigrationTask.InicializarBanco");
            throw;
        }
    }

    /// <summary>
    /// Salva uma notícia extraída no banco (Entity Framework)
    /// </summary>
    public async Task<Noticia> SalvarNoticiaAsync(Noticia noticia)
    {
        try
        {
            LoggingTask.RegistrarInfo($"Salvando notícia: {noticia.Titulo}");
            
            // Verificar se já existe (por URL)
            var existente = await _context.Noticias
                .FirstOrDefaultAsync(n => n.Url == noticia.Url);
                
            if (existente != null)
            {
                LoggingTask.RegistrarInfo("Notícia já existe - atualizando dados");
                
                // Atualizar dados
                existente.Titulo = noticia.Titulo;
                existente.Conteudo = noticia.Conteudo;
                existente.Resumo = noticia.Resumo;
                existente.DataExtracao = DateTime.Now;
                existente.Visualizacoes = noticia.Visualizacoes;
                
                await _context.SaveChangesAsync();
                return existente;
            }
            else
            {
                // Inserir nova notícia
                noticia.DataExtracao = DateTime.Now;
                _context.Noticias.Add(noticia);
                await _context.SaveChangesAsync();
                
                LoggingTask.RegistrarInfo($"Nova notícia salva com ID: {noticia.Id}");
                return noticia;
            }
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, $"MigrationTask.SalvarNoticia - URL: {noticia.Url}");
            throw;
        }
    }

    /// <summary>
    /// Salva lote de notícias de forma otimizada (Dapper)
    /// </summary>
    public async Task<int> SalvarLoteNoticiasAsync(List<Noticia> noticias)
    {
        try
        {
            LoggingTask.RegistrarInfo($"Salvando lote de {noticias.Count} notícias...");
            
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            string sqlInsert = @"
                INSERT IGNORE INTO Noticias 
                (Titulo, Url, Categoria, Conteudo, Resumo, DataPublicacao, DataExtracao, UrlImagem, Autor, Visualizacoes)
                VALUES 
                (@Titulo, @Url, @Categoria, @Conteudo, @Resumo, @DataPublicacao, @DataExtracao, @UrlImagem, @Autor, @Visualizacoes)";
            
            // Preparar dados
            var dadosInsert = noticias.Select(n => new
            {
                n.Titulo,
                n.Url,
                n.Categoria,
                n.Conteudo,
                n.Resumo,
                n.DataPublicacao,
                DataExtracao = DateTime.Now,
                n.UrlImagem,
                n.Autor,
                Visualizacoes = n.Visualizacoes
            }).ToArray();
            
            // Executar insert em lote
            int inseridas = await connection.ExecuteAsync(sqlInsert, dadosInsert);
            
            LoggingTask.RegistrarInfo($"Lote processado - {inseridas} notícias inseridas");
            return inseridas;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, "MigrationTask.SalvarLoteNoticias");
            throw;
        }
    }

    /// <summary>
    /// Registra log de execução do scraping
    /// </summary>
    public async Task<LogExecucao> RegistrarExecucaoAsync(DateTime dataInicio, string status, 
        int noticiasExtraidas = 0, int noticiasNovas = 0, string? mensagemErro = null)
    {
        try
        {
            var log = new LogExecucao
            {
                DataInicio = dataInicio,
                DataFim = DateTime.Now,
                Status = status,
                NoticiasExtraidas = noticiasExtraidas,
                NoticiasNovas = noticiasNovas,
                MensagemErro = mensagemErro,
                TempoExecucaoSegundos = (int)(DateTime.Now - dataInicio).TotalSeconds
            };
            
            _context.LogsExecucao.Add(log);
            await _context.SaveChangesAsync();
            
            LoggingTask.RegistrarInfo($"Log de execução salvo - Status: {status}, Tempo: {log.TempoExecucaoSegundos}s");
            return log;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, "MigrationTask.RegistrarExecucao");
            throw;
        }
    }

    /// <summary>
    /// Obter estatísticas usando Dapper para performance
    /// </summary>
    public async Task<Dictionary<string, object>> ObterEstatisticasAsync()
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            
            string sql = @"
                SELECT 
                    COUNT(*) as TotalNoticias,
                    COUNT(DISTINCT Categoria) as TotalCategorias,
                    MAX(DataExtracao) as UltimaExtracao,
                    COUNT(CASE WHEN DATE(DataExtracao) = CURDATE() THEN 1 END) as NoticiasHoje
                FROM Noticias";
            
            var resultado = await connection.QueryFirstOrDefaultAsync(sql);
            
            return new Dictionary<string, object>
            {
                ["TotalNoticias"] = resultado.TotalNoticias,
                ["TotalCategorias"] = resultado.TotalCategorias,
                ["UltimaExtracao"] = resultado.UltimaExtracao,
                ["NoticiasHoje"] = resultado.NoticiasHoje
            };
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, "MigrationTask.ObterEstatisticas");
            throw;
        }
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
```

### 4. Comandos de Migration (Entity Framework)

```powershell
# Navegar até o projeto
cd C:\Users\lucas\OneDrive\Documentos\CsharpProjects\AdrenalineSpy

# Criar primeira migration
dotnet ef migrations add InitialCreate

# Aplicar migration ao banco
dotnet ef database update

# Ver migrations pendentes
dotnet ef migrations list

# Gerar script SQL (para produção)
dotnet ef migrations script

# Remover última migration (se necessário)
dotnet ef migrations remove

# Resetar banco (CUIDADO: apaga tudo)
dotnet ef database drop --force
dotnet ef database update
```

## Métodos Mais Usados

### Entity Framework Core - CRUD Básico

```csharp
// Inicializar contexto
using var context = new AdrenalineDbContext();

// CREATE - Inserir nova notícia
var noticia = new Noticia
{
    Titulo = "Nova notícia sobre gaming",
    Url = "https://www.adrenaline.com.br/games/nova-noticia",
    Categoria = "Games",
    Conteudo = "Conteúdo extraído pelo Playwright...",
    DataPublicacao = DateTime.Now,
    DataExtracao = DateTime.Now
};

context.Noticias.Add(noticia);
await context.SaveChangesAsync();

// READ - Buscar notícias
var todasNoticias = await context.Noticias.ToListAsync();
var noticiasPorCategoria = await context.Noticias
    .Where(n => n.Categoria == "Games")
    .OrderByDescending(n => n.DataPublicacao)
    .Take(10)
    .ToListAsync();

var noticiaEspecifica = await context.Noticias
    .FirstOrDefaultAsync(n => n.Url == "https://...");

// UPDATE - Atualizar notícia existente
if (noticiaEspecifica != null)
{
    noticiaEspecifica.Visualizacoes++;
    noticiaEspecifica.DataExtracao = DateTime.Now;
    await context.SaveChangesAsync();
}

// DELETE - Remover notícias antigas
var noticiasAntigas = await context.Noticias
    .Where(n => n.DataPublicacao < DateTime.Now.AddMonths(-6))
    .ToListAsync();

context.Noticias.RemoveRange(noticiasAntigas);
await context.SaveChangesAsync();
```

### Queries LINQ Úteis para AdrenalineSpy

```csharp
using var context = new AdrenalineDbContext();

// Estatísticas por categoria
var estatisticasCategorias = await context.Noticias
    .GroupBy(n => n.Categoria)
    .Select(g => new
    {
        Categoria = g.Key,
        Total = g.Count(),
        UltimaNoticias = g.Max(n => n.DataPublicacao)
    })
    .OrderByDescending(e => e.Total)
    .ToListAsync();

// Notícias mais recentes
var noticiasRecentes = await context.Noticias
    .OrderByDescending(n => n.DataPublicacao)
    .Take(20)
    .Select(n => new { n.Titulo, n.Categoria, n.DataPublicacao })
    .ToListAsync();

// Busca de texto no conteúdo
var noticiasComPalavra = await context.Noticias
    .Where(n => n.Conteudo.Contains("inteligência artificial"))
    .ToListAsync();

// Paginação para interface web
int pagina = 1;
int itensPorPagina = 10;
var noticiasPaginadas = await context.Noticias
    .OrderByDescending(n => n.DataPublicacao)
    .Skip((pagina - 1) * itensPorPagina)
    .Take(itensPorPagina)
    .ToListAsync();

// Contadores
int totalNoticias = await context.Noticias.CountAsync();
int noticiasHoje = await context.Noticias
    .CountAsync(n => n.DataExtracao.Date == DateTime.Today);
```

### Dapper - Queries de Performance

```csharp
using var connection = new MySqlConnection(Config.Instancia.ObterConnectionString());

// Relatório de atividade por dia
string sql = @"
    SELECT 
        DATE(DataPublicacao) as Data,
        COUNT(*) as Quantidade,
        GROUP_CONCAT(DISTINCT Categoria) as Categorias
    FROM Noticias 
    WHERE DataPublicacao >= @DataInicio
    GROUP BY DATE(DataPublicacao)
    ORDER BY Data DESC";

var relatorioAtividade = await connection.QueryAsync(sql, 
    new { DataInicio = DateTime.Now.AddDays(-30) });

// Top autores por quantidade de notícias
string sqlAutores = @"
    SELECT 
        Autor,
        COUNT(*) as TotalNoticias,
        MAX(DataPublicacao) as UltimaPublicacao
    FROM Noticias 
    WHERE Autor IS NOT NULL AND Autor != ''
    GROUP BY Autor
    HAVING COUNT(*) >= 5
    ORDER BY TotalNoticias DESC
    LIMIT 10";

var topAutores = await connection.QueryAsync(sqlAutores);

// Inserção em lote (mais rápida que EF Core)
var loteNoticias = new List<object>();
foreach (var noticia in noticiasExtraidas)
{
    loteNoticias.Add(new
    {
        noticia.Titulo,
        noticia.Url,
        noticia.Categoria,
        noticia.Conteudo,
        DataExtracao = DateTime.Now
    });
}

string sqlInsertLote = @"
    INSERT INTO Noticias (Titulo, Url, Categoria, Conteudo, DataExtracao)
    VALUES (@Titulo, @Url, @Categoria, @Conteudo, @DataExtracao)";

int inseridas = await connection.ExecuteAsync(sqlInsertLote, loteNoticias);
```

### Padrão de Uso na MigrationTask

```csharp
public class MigrationTask
{
    // EF Core para operações simples
    public async Task<bool> NoticiaExisteAsync(string url)
    {
        using var context = new AdrenalineDbContext();
        return await context.Noticias.AnyAsync(n => n.Url == url);
    }

    // Dapper para inserção em lote (performance)
    public async Task<int> SalvarLoteAsync(List<Noticia> noticias)
    {
        var config = Config.Instancia;
        using var connection = new MySqlConnection(config.ObterConnectionString());
        
        // SQL otimizado com INSERT IGNORE para evitar duplicatas
        string sql = @"
            INSERT IGNORE INTO Noticias 
            (Titulo, Url, Categoria, Conteudo, DataPublicacao, DataExtracao)
            VALUES 
            (@Titulo, @Url, @Categoria, @Conteudo, @DataPublicacao, @DataExtracao)";
        
        var parametros = noticias.Select(n => new
        {
            n.Titulo,
            n.Url,
            n.Categoria,
            n.Conteudo,
            n.DataPublicacao,
            DataExtracao = DateTime.Now
        });
        
        int resultado = await connection.ExecuteAsync(sql, parametros);
        LoggingTask.RegistrarInfo($"Lote processado: {resultado} notícias inseridas");
        
        return resultado;
    }
}
```

### Transações para Operações Críticas

```csharp
// Salvar execução completa com transação
public async Task RegistrarExecucaoCompletaAsync(List<Noticia> noticias, LogExecucao log)
{
    using var context = new AdrenalineDbContext();
    using var transaction = await context.Database.BeginTransactionAsync();
    
    try
    {
        // 1. Salvar notícias
        context.Noticias.AddRange(noticias);
        await context.SaveChangesAsync();
        
        // 2. Atualizar log com resultado
        log.NoticiasExtraidas = noticias.Count;
        log.DataFim = DateTime.Now;
        log.Status = "Sucesso";
        
        context.LogsExecucao.Add(log);
        await context.SaveChangesAsync();
        
        // 3. Confirmar transação
        await transaction.CommitAsync();
        
        LoggingTask.RegistrarInfo("Execução registrada com sucesso - transação commitada");
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        LoggingTask.RegistrarErro(ex, "MigrationTask.RegistrarExecucaoCompleta - Rollback realizado");
        throw;
    }
}
```

### Queries de Análise e Relatórios

```csharp
// Dashboard de estatísticas (Dapper para performance)
public async Task<object> ObterDashboardAsync()
{
    using var connection = new MySqlConnection(Config.Instancia.ObterConnectionString());
    
    string sql = @"
        SELECT 
            -- Totais gerais
            COUNT(*) as TotalNoticias,
            COUNT(DISTINCT Categoria) as TotalCategorias,
            COUNT(DISTINCT DATE(DataPublicacao)) as DiasAtivos,
            
            -- Atividade recente
            SUM(CASE WHEN DataExtracao >= @UltimaSemana THEN 1 ELSE 0 END) as NoticiasUltimaSemana,
            SUM(CASE WHEN DataExtracao >= @Hoje THEN 1 ELSE 0 END) as NoticiasHoje,
            
            -- Estatísticas temporais
            MIN(DataPublicacao) as PrimeiraNoticia,
            MAX(DataPublicacao) as UltimaNoticia,
            AVG(CHAR_LENGTH(Conteudo)) as MediaTamanhoConteudo
            
        FROM Noticias";
    
    var resultado = await connection.QueryFirstAsync(sql, new
    {
        UltimaSemana = DateTime.Now.AddDays(-7),
        Hoje = DateTime.Today
    });
    
    return resultado;
}

// Tendências por categoria
public async Task<List<object>> ObterTendenciasCategoriaAsync()
{
    using var connection = new MySqlConnection(Config.Instancia.ObterConnectionString());
    
    string sql = @"
        SELECT 
            Categoria,
            COUNT(*) as Total,
            COUNT(CASE WHEN DataPublicacao >= @UltimoMes THEN 1 END) as UltimoMes,
            COUNT(CASE WHEN DataPublicacao >= @UltimaSemana THEN 1 END) as UltimaSemana,
            MAX(DataPublicacao) as UltimaPublicacao,
            AVG(Visualizacoes) as MediaVisualizacoes
        FROM Noticias 
        GROUP BY Categoria
        ORDER BY Total DESC";
    
    var tendencias = await connection.QueryAsync(sql, new
    {
        UltimoMes = DateTime.Now.AddMonths(-1),
        UltimaSemana = DateTime.Now.AddDays(-7)
    });
    
    return tendencias.ToList();
}
```

---

## 💡 Resumo para AdrenalineSpy

**Setup único (fazer uma vez):**
1. `dotnet add package` Entity Framework + Dapper + providers
2. Configuração `Database` no `AutomationSettings.json` (já existe)
3. Método `ObterConnectionString()` no `Config.cs` (já existe)
4. Criar Models: `Noticia.cs`, `Categoria.cs`, `LogExecucao.cs`
5. Criar `AdrenalineDbContext.cs` com configurações
6. Criar `MigrationTask.cs` para operações de banco
7. Executar `dotnet ef migrations add InitialCreate` e `dotnet ef database update`

**Padrão de uso:**
- **Entity Framework:** CRUD simples, relacionamentos, migrations
- **Dapper:** Inserção em lote, relatórios complexos, queries de performance
- **MigrationTask:** Centraliza todas as operações de banco
- **Logging:** Sempre usar `LoggingTask` em operações de banco

**Resultado:**
- Dados do Adrenaline.com.br persistidos em banco Docker
- Histórico completo de execuções e extrações
- Relatórios e estatísticas de tendências
- Suporte a MySQL, PostgreSQL e SQL Server

**Próxima etapa:** Docker Setup para subir o banco de dados! 🚀
