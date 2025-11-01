# ORMs para .NET - Entity Framework Core e Dapper

## Índice
1. [Entity Framework Core](#entity-framework-core)
2. [Dapper](#dapper)
3. [Comparação](#comparação)
4. [Recomendações por Banco](#recomendações-por-banco)

---

## Entity Framework Core

### Introdução

**Entity Framework Core** (EF Core) é um ORM completo e moderno da Microsoft.

**Vantagens:**
- ✅ LINQ type-safe
- ✅ Migrations automáticas
- ✅ Change tracking
- ✅ Lazy/Eager loading
- ✅ Suporte a múltiplos bancos

---

### Instalação

```bash
# Core
dotnet add package Microsoft.EntityFrameworkCore

# Provider MySQL
dotnet add package Pomelo.EntityFrameworkCore.MySql

# Provider PostgreSQL
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

# Provider SQL Server
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# Tools (migrations)
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design
```

---

### Exemplo Completo

```csharp
using Microsoft.EntityFrameworkCore;

// 1. Definir Modelos
public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public decimal Preco { get; set; }
    public int Estoque { get; set; }
}

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public List<Pedido> Pedidos { get; set; }
}

public class Pedido
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; }
}

// 2. DbContext
public class AppDbContext : DbContext
{
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // MySQL
        optionsBuilder.UseMySql(
            "Server=localhost;Database=rpa_db;User=root;Password=senha123;",
            ServerVersion.AutoDetect("Server=localhost;Database=rpa_db;User=root;Password=senha123;"));
        
        // PostgreSQL
        // optionsBuilder.UseNpgsql("Host=localhost;Database=rpa_db;Username=rpa_user;Password=rpa_pass;");
        
        // SQL Server
        // optionsBuilder.UseSqlServer("Server=localhost;Database=rpa_db;User Id=SA;Password=SenhaForte123!;TrustServerCertificate=True;");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurações adicionais
        modelBuilder.Entity<Produto>()
            .Property(p => p.Preco)
            .HasPrecision(18, 2);
    }
}

// 3. Uso
class Program
{
    static void Main()
    {
        using (var db = new AppDbContext())
        {
            // CREATE
            var produto = new Produto
            {
                Nome = "Mouse",
                Preco = 50.00m,
                Estoque = 10
            };
            db.Produtos.Add(produto);
            db.SaveChanges();
            
            // READ
            var produtos = db.Produtos.ToList();
            var produtosPorPreco = db.Produtos.Where(p => p.Preco > 30).ToList();
            var produtoPorId = db.Produtos.Find(1);
            
            // UPDATE
            produto.Preco = 45.00m;
            db.SaveChanges();
            
            // DELETE
            db.Produtos.Remove(produto);
            db.SaveChanges();
        }
    }
}
```

---

### Migrations

```bash
# Criar migration
dotnet ef migrations add InitialCreate

# Aplicar ao banco
dotnet ef database update

# Remover última migration
dotnet ef migrations remove

# Ver SQL que será executado
dotnet ef migrations script
```

---

### LINQ Queries

```csharp
using (var db = new AppDbContext())
{
    // Filtrar
    var caros = db.Produtos.Where(p => p.Preco > 100).ToList();
    
    // Ordenar
    var ordenados = db.Produtos.OrderBy(p => p.Nome).ToList();
    var desc = db.Produtos.OrderByDescending(p => p.Preco).ToList();
    
    // Paginação
    var pagina = db.Produtos.Skip(10).Take(10).ToList();
    
    // Primeiro/Único
    var primeiro = db.Produtos.FirstOrDefault(p => p.Id == 1);
    var unico = db.Produtos.SingleOrDefault(p => p.Nome == "Mouse");
    
    // Contar
    int total = db.Produtos.Count();
    int acima50 = db.Produtos.Count(p => p.Preco > 50);
    
    // Somar/Média
    decimal totalPreco = db.Produtos.Sum(p => p.Preco);
    decimal media = db.Produtos.Average(p => p.Preco);
    
    // Joins (Include)
    var pedidosComCliente = db.Pedidos
        .Include(p => p.Cliente)
        .ToList();
    
    // Select (projeção)
    var nomes = db.Produtos.Select(p => p.Nome).ToList();
    var dto = db.Produtos.Select(p => new { p.Nome, p.Preco }).ToList();
}
```

---

### Async/Await

```csharp
// Recomendado para aplicações modernas
await db.Produtos.AddAsync(produto);
await db.SaveChangesAsync();

var produtos = await db.Produtos.ToListAsync();
var produto = await db.Produtos.FindAsync(1);
var count = await db.Produtos.CountAsync();
```

---

## Dapper

### Introdução

**Dapper** é um micro-ORM ultra-performático desenvolvido pelo Stack Overflow.

**Vantagens:**
- ✅ Extremamente rápido
- ✅ Controle total do SQL
- ✅ Leve e simples
- ✅ Mapping automático

---

### Instalação

```bash
dotnet add package Dapper

# Provider (escolha um)
dotnet add package MySql.Data          # MySQL
dotnet add package Npgsql             # PostgreSQL
dotnet add package Microsoft.Data.SqlClient  # SQL Server
```

---

### Exemplo Completo

```csharp
using Dapper;
using MySql.Data.MySqlClient;

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public decimal Preco { get; set; }
    public int Estoque { get; set; }
}

class Program
{
    static string _connectionString = "Server=localhost;Database=rpa_db;User=root;Password=senha123;";
    
    static void Main()
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            // CREATE
            string insertSql = "INSERT INTO Produtos (Nome, Preco, Estoque) VALUES (@Nome, @Preco, @Estoque)";
            connection.Execute(insertSql, new { Nome = "Mouse", Preco = 50.00m, Estoque = 10 });
            
            // READ (Query)
            var produtos = connection.Query<Produto>("SELECT * FROM Produtos").ToList();
            
            // READ (QueryFirst)
            var produto = connection.QueryFirstOrDefault<Produto>(
                "SELECT * FROM Produtos WHERE Id = @Id", 
                new { Id = 1 });
            
            // UPDATE
            string updateSql = "UPDATE Produtos SET Preco = @Preco WHERE Id = @Id";
            connection.Execute(updateSql, new { Preco = 45.00m, Id = 1 });
            
            // DELETE
            string deleteSql = "DELETE FROM Produtos WHERE Id = @Id";
            connection.Execute(deleteSql, new { Id = 1 });
        }
    }
}
```

---

### Queries Avançadas

```csharp
using (var connection = new MySqlConnection(_connectionString))
{
    // Múltiplos resultados
    string sql = @"
        SELECT * FROM Produtos WHERE Preco > @Preco;
        SELECT COUNT(*) FROM Produtos;
    ";
    using (var multi = connection.QueryMultiple(sql, new { Preco = 50 }))
    {
        var produtos = multi.Read<Produto>().ToList();
        var count = multi.ReadFirst<int>();
    }
    
    // Stored Procedure
    var result = connection.Query<Produto>(
        "sp_GetProdutos", 
        new { MinPreco = 50 },
        commandType: CommandType.StoredProcedure);
    
    // Bulk Insert
    var produtos = new List<Produto>
    {
        new Produto { Nome = "Mouse", Preco = 50, Estoque = 10 },
        new Produto { Nome = "Teclado", Preco = 150, Estoque = 5 }
    };
    
    string sql = "INSERT INTO Produtos (Nome, Preco, Estoque) VALUES (@Nome, @Preco, @Estoque)";
    connection.Execute(sql, produtos);
}
```

---

### Async/Await

```csharp
var produtos = await connection.QueryAsync<Produto>("SELECT * FROM Produtos");
var produto = await connection.QueryFirstOrDefaultAsync<Produto>(
    "SELECT * FROM Produtos WHERE Id = @Id", 
    new { Id = 1 });

await connection.ExecuteAsync(
    "INSERT INTO Produtos (Nome, Preco) VALUES (@Nome, @Preco)", 
    new { Nome = "Mouse", Preco = 50 });
```

---

## Comparação

### Entity Framework Core

**Use quando:**
- ✅ Quer produtividade
- ✅ Precisa de migrations automáticas
- ✅ Tem relacionamentos complexos
- ✅ Quer LINQ type-safe
- ✅ Change tracking é útil

**Não use quando:**
- ❌ Performance é crítica
- ❌ Quer controle total do SQL
- ❌ Queries muito complexas

### Dapper

**Use quando:**
- ✅ Performance é prioridade
- ✅ Quer controle total do SQL
- ✅ Queries complexas/otimizadas
- ✅ Projeto simples

**Não use quando:**
- ❌ Quer migrations automáticas
- ❌ Tem muitos relacionamentos
- ❌ Precisa de change tracking

---

## Recomendações por Banco

### MySQL

**Entity Framework Core:**
```bash
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

```csharp
optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
```

**Dapper:**
```bash
dotnet add package MySql.Data
```

---

### PostgreSQL

**Entity Framework Core:**
```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

```csharp
optionsBuilder.UseNpgsql(connectionString);
```

**Dapper:**
```bash
dotnet add package Npgsql
```

---

### SQL Server

**Entity Framework Core:**
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

```csharp
optionsBuilder.UseSqlServer(connectionString);
```

**Dapper:**
```bash
dotnet add package Microsoft.Data.SqlClient
```

---

## Exemplo Híbrido (EF Core + Dapper)

```csharp
public class ProdutoRepository
{
    private readonly AppDbContext _context;
    private readonly string _connectionString;
    
    public ProdutoRepository(AppDbContext context, string connectionString)
    {
        _context = context;
        _connectionString = connectionString;
    }
    
    // EF Core para CRUD simples
    public async Task<Produto> AdicionarAsync(Produto produto)
    {
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();
        return produto;
    }
    
    // Dapper para queries complexas/performáticas
    public async Task<List<RelatorioVendas>> ObterRelatorioVendasAsync()
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            string sql = @"
                SELECT 
                    p.Nome as Produto,
                    SUM(pv.Quantidade) as TotalVendido,
                    SUM(pv.Quantidade * p.Preco) as ValorTotal
                FROM ProdutosVendas pv
                INNER JOIN Produtos p ON pv.ProdutoId = p.Id
                GROUP BY p.Id, p.Nome
                ORDER BY TotalVendido DESC";
            
            return (await connection.QueryAsync<RelatorioVendas>(sql)).ToList();
        }
    }
}
```

---

## Boas Práticas

### 1. Use Async/Await

```csharp
await db.SaveChangesAsync();
await connection.ExecuteAsync(sql, parameters);
```

### 2. Dispose Connections

```csharp
using (var connection = new MySqlConnection(connectionString))
{
    // trabalhar aqui
}
```

### 3. Use Transações quando necessário

```csharp
// EF Core
using (var transaction = db.Database.BeginTransaction())
{
    try
    {
        // operações
        await db.SaveChangesAsync();
        transaction.Commit();
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}

// Dapper
using (var connection = new MySqlConnection(connectionString))
{
    connection.Open();
    using (var transaction = connection.BeginTransaction())
    {
        try
        {
            connection.Execute(sql1, param1, transaction);
            connection.Execute(sql2, param2, transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
```

### 4. Configure Connection Pooling

```csharp
// Connection strings já habilitam pooling por padrão
"Server=localhost;Database=db;User=root;Password=pass;Pooling=true;Min Pool Size=0;Max Pool Size=100;"
```

---

## Recursos Adicionais

- **EF Core Docs**: https://docs.microsoft.com/ef/core/
- **Dapper GitHub**: https://github.com/DapperLib/Dapper
- **Dapper Tutorial**: https://dapper-tutorial.net/

---

**Versão:** 1.0  
**Última atualização:** Novembro 2025
