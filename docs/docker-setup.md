# Docker, WSL 2 e Bancos de Dados para RPA

## Índice
1. [Instalação do WSL 2](#instalação-do-wsl-2)
2. [Instalação do Docker](#instalação-do-docker)
3. [MySQL](#mysql)
4. [PostgreSQL](#postgresql)
5. [SQL Server](#sql-server)
6. [Integração com .NET](#integração-com-net)

---

## Instalação do WSL 2

### Passo 1: Habilitar WSL

```powershell
# Executar como Administrador no PowerShell
wsl --install
```

### Passo 2: Verificar Versão

```powershell
wsl --set-default-version 2
wsl --list --verbose
```

### Passo 3: Instalar Distribuição

```powershell
# Ubuntu (recomendado)
wsl --install -d Ubuntu

# Outras opções
wsl --list --online
```

### Passo 4: Configurar Usuário

Após instalação, configure usuário e senha quando solicitado.

---

## Instalação do Docker

### Passo 1: Download

- Baixe **Docker Desktop** em: https://www.docker.com/products/docker-desktop/
- Instale e reinicie o computador

### Passo 2: Configurar para WSL 2

1. Abra Docker Desktop
2. Settings → General → Use WSL 2 based engine ✅
3. Settings → Resources → WSL Integration
4. Habilite integração com Ubuntu ✅

### Passo 3: Verificar Instalação

Abra um terminal (PowerShell ou CMD) e teste:

```bash
docker --version
# Saída esperada: Docker version 24.x.x, build xxxxx

docker compose version
# Saída esperada: Docker Compose version v2.x.x

docker ps
# Saída esperada: Lista de containers (vazia inicialmente)
```

**Se der erro "docker: command not found":**
- Reinicie o computador
- Certifique-se que Docker Desktop está rodando
- Verifique se Docker está no PATH

---

## MySQL

### Opção 1: Comando Direto (Rápido para Testes)

```bash
# Criar e iniciar MySQL
docker run -d \
  --name mysql-rpa \
  -e MYSQL_ROOT_PASSWORD=senha123 \
  -e MYSQL_DATABASE=rpa_db \
  -p 3306:3306 \
  mysql:8.0
```

**No Windows PowerShell, use:**
```powershell
docker run -d `
  --name mysql-rpa `
  -e MYSQL_ROOT_PASSWORD=senha123 `
  -e MYSQL_DATABASE=rpa_db `
  -p 3306:3306 `
  mysql:8.0
```

### Opção 2: Docker Compose (Recomendado para Projetos)

Crie um arquivo `docker-compose.mysql.yml` na pasta do seu projeto:

```yaml
version: '3.8'

services:
  mysql:
    image: mysql:8.0
    container_name: mysql-rpa
    environment:
      MYSQL_ROOT_PASSWORD: senha123
      MYSQL_DATABASE: rpa_db
      MYSQL_USER: rpa_user
      MYSQL_PASSWORD: rpa_pass
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
    restart: unless-stopped

volumes:
  mysql_data:
```

```bash
# Iniciar
docker compose -f docker-compose.mysql.yml up -d

# Parar
docker compose -f docker-compose.mysql.yml down

# Ver logs
docker compose -f docker-compose.mysql.yml logs -f
```

### Conectar ao MySQL

```bash
# Via Docker
docker exec -it mysql-rpa mysql -u root -p

# Via cliente MySQL (se instalado)
mysql -h localhost -u root -p
```

### Connection String .NET

```csharp
// NuGet: dotnet add package MySql.Data
string connectionString = "Server=localhost;Port=3306;Database=rpa_db;User=root;Password=senha123;";
```

---

## PostgreSQL

### Criar Container PostgreSQL

```bash
docker run -d \
  --name postgres-rpa \
  -e POSTGRES_PASSWORD=senha123 \
  -e POSTGRES_DB=rpa_db \
  -p 5432:5432 \
  postgres:15
```

### Docker Compose

Crie `docker-compose.postgres.yml`:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15
    container_name: postgres-rpa
    environment:
      POSTGRES_USER: rpa_user
      POSTGRES_PASSWORD: rpa_pass
      POSTGRES_DB: rpa_db
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    restart: unless-stopped

volumes:
  postgres_data:
```

```bash
docker compose -f docker-compose.postgres.yml up -d
```

### Conectar ao PostgreSQL

```bash
# Via Docker
docker exec -it postgres-rpa psql -U rpa_user -d rpa_db

# Via psql (se instalado)
psql -h localhost -U rpa_user -d rpa_db
```

### Connection String .NET

```csharp
// NuGet: dotnet add package Npgsql
string connectionString = "Host=localhost;Port=5432;Database=rpa_db;Username=rpa_user;Password=rpa_pass;";
```

---

## SQL Server

### Criar Container SQL Server

```bash
docker run -d \
  --name sqlserver-rpa \
  -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=SenhaForte123!" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

### Docker Compose

Crie `docker-compose.sqlserver.yml`:

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: sqlserver-rpa
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "SenhaForte123!"
      MSSQL_PID: "Developer"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    restart: unless-stopped

volumes:
  sqlserver_data:
```

```bash
docker compose -f docker-compose.sqlserver.yml up -d
```

### Criar Banco de Dados

```bash
# Via Docker
docker exec -it sqlserver-rpa /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U SA -P "SenhaForte123!" \
  -Q "CREATE DATABASE rpa_db"
```

### Connection String .NET

```csharp
// NuGet: dotnet add package Microsoft.Data.SqlClient
string connectionString = "Server=localhost,1433;Database=rpa_db;User Id=SA;Password=SenhaForte123!;TrustServerCertificate=True;";
```

---

## Integração com .NET

### Exemplo: MySQL

```csharp
using MySql.Data.MySqlClient;

public class MySqlExample
{
    private string _connectionString = "Server=localhost;Database=rpa_db;User=root;Password=senha123;";
    
    public void TestarConexao()
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            Console.WriteLine("Conectado ao MySQL!");
            
            using (var command = new MySqlCommand("SELECT VERSION()", connection))
            {
                var version = command.ExecuteScalar();
                Console.WriteLine($"Versão: {version}");
            }
        }
    }
    
    public void InserirDados()
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            
            string sql = "INSERT INTO usuarios (nome, email) VALUES (@nome, @email)";
            using (var command = new MySqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@nome", "João");
                command.Parameters.AddWithValue("@email", "joao@email.com");
                command.ExecuteNonQuery();
            }
        }
    }
}
```

### Exemplo: PostgreSQL

```csharp
using Npgsql;

public class PostgreSqlExample
{
    private string _connectionString = "Host=localhost;Database=rpa_db;Username=rpa_user;Password=rpa_pass;";
    
    public void TestarConexao()
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            Console.WriteLine("Conectado ao PostgreSQL!");
            
            using (var command = new NpgsqlCommand("SELECT version()", connection))
            {
                var version = command.ExecuteScalar();
                Console.WriteLine($"Versão: {version}");
            }
        }
    }
}
```

### Exemplo: SQL Server

```csharp
using Microsoft.Data.SqlClient;

public class SqlServerExample
{
    private string _connectionString = "Server=localhost,1433;Database=rpa_db;User Id=SA;Password=SenhaForte123!;TrustServerCertificate=True;";
    
    public void TestarConexao()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            Console.WriteLine("Conectado ao SQL Server!");
            
            using (var command = new SqlCommand("SELECT @@VERSION", connection))
            {
                var version = command.ExecuteScalar();
                Console.WriteLine($"Versão: {version}");
            }
        }
    }
}
```

---

## Docker Compose Completo (Todos os BDs)

Crie `docker-compose.yml`:

```yaml
version: '3.8'

services:
  mysql:
    image: mysql:8.0
    container_name: mysql-rpa
    environment:
      MYSQL_ROOT_PASSWORD: senha123
      MYSQL_DATABASE: rpa_db
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql

  postgres:
    image: postgres:15
    container_name: postgres-rpa
    environment:
      POSTGRES_USER: rpa_user
      POSTGRES_PASSWORD: rpa_pass
      POSTGRES_DB: rpa_db
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: sqlserver-rpa
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "SenhaForte123!"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

volumes:
  mysql_data:
  postgres_data:
  sqlserver_data:
```

```bash
# Iniciar todos
docker compose up -d

# Parar todos
docker compose down

# Ver status
docker compose ps
```

---

## Comandos Docker Úteis

```bash
# Listar containers
docker ps
docker ps -a  # Todos, incluindo parados

# Iniciar/Parar
docker start mysql-rpa
docker stop mysql-rpa
docker restart mysql-rpa

# Logs
docker logs mysql-rpa
docker logs -f mysql-rpa  # Follow

# Remover
docker rm mysql-rpa
docker rm -f mysql-rpa  # Forçar

# Remover volumes
docker volume ls
docker volume rm mysql_data

# Limpar tudo
docker system prune -a  # Cuidado!
```

---

## Boas Práticas

### 1. Use Variáveis de Ambiente

```yaml
environment:
  MYSQL_ROOT_PASSWORD: ${MYSQL_PASSWORD:-senha_padrao}
```

### 2. Persista Dados com Volumes

```yaml
volumes:
  - mysql_data:/var/lib/mysql
```

### 3. Configure Restart Policy

```yaml
restart: unless-stopped
```

### 4. Limite Recursos (Opcional)

```yaml
deploy:
  resources:
    limits:
      cpus: '1'
      memory: 1G
```

### 5. Use Networks para Isolamento

```yaml
networks:
  rpa-network:
    driver: bridge
```

---

## Ferramentas Úteis

### Clientes de Banco de Dados

- **DBeaver**: Universal (gratuito) - https://dbeaver.io/
- **MySQL Workbench**: MySQL
- **pgAdmin**: PostgreSQL
- **Azure Data Studio**: SQL Server (multiplataforma)

### Extensões VS Code

- **Docker** (Microsoft)
- **MySQL** (cweijan)
- **PostgreSQL** (cweijan)
- **SQL Server** (Microsoft)

---

## Troubleshooting

### Porta já em uso

```bash
# Ver qual processo usa a porta
netstat -ano | findstr :3306

# Parar processo ou mudar porta do Docker
ports:
  - "3307:3306"  # Host:Container
```

### Container não inicia

```bash
# Ver logs
docker logs nome-container

# Verificar se porta está disponível
# Verificar se WSL 2 está ativo
wsl -l -v
```

---

**Versão:** 1.0  
**Última atualização:** Novembro 2025
