# 🐳 Docker Setup - Banco de Dados para AdrenalineSpy

## O que é

**Docker:** Plataforma de containerização para executar aplicações isoladas  
**WSL 2:** Subsistema Windows para Linux, necessário para Docker no Windows  
**Por que usar:** Banco de dados confiável e portátil para o projeto RPA

**Onde é usado no AdrenalineSpy:**
- MigrationTask.cs se conecta ao banco Docker para persistir dados
- Armazena notícias extraídas pelo Playwright do Adrenaline.com.br
- Suporte a MySQL, PostgreSQL ou SQL Server (configurável via JSON)
- Dados isolados em container, não interferem com sistema host
- Backup e migração simplificados (volumes Docker)

**Posição no fluxo:** Etapa 8 de 17 - implementar APÓS ORM (precisa das Models e MigrationTask)

## Como Instalar

### 1. Instalar WSL 2 (Windows Subsystem for Linux)

```powershell
# Executar PowerShell como Administrador
# Instalar WSL 2 com Ubuntu (necessário para Docker)
wsl --install

# Verificar se WSL 2 está ativo
wsl --set-default-version 2
wsl --list --verbose

# Se necessário, instalar Ubuntu especificamente
wsl --install -d Ubuntu

# Configurar usuário Ubuntu quando solicitado
# Exemplo: usuario=dev, senha=dev123
```

### 2. Instalar Docker Desktop

```powershell
# Baixar Docker Desktop de: https://www.docker.com/products/docker-desktop/
# Executar o instalador baixado
# Reiniciar o computador após instalação
```

### 3. Configurar Docker com WSL 2

1. **Abrir Docker Desktop**
2. **Settings → General:**
   - ✅ **"Use WSL 2 based engine"**
3. **Settings → Resources → WSL Integration:**
   - ✅ **"Enable integration with my default WSL distro"**
   - ✅ **Ubuntu** (selecionar a distribuição instalada)
4. **Apply & Restart**

### 4. Verificar Instalação

Abrir PowerShell e testar:

```powershell
# Verificar versões
docker --version
# Saída: Docker version 24.0.6, build ed223bc

docker compose version
# Saída: Docker Compose version v2.21.0

# Testar funcionamento
docker run hello-world
# Saída: Hello from Docker! ...

# Listar containers (deve estar vazio inicialmente)
docker ps
```

**Troubleshooting comum:**
```powershell
# Se comando docker não for reconhecido:
# 1. Reiniciar computador
# 2. Verificar se Docker Desktop está rodando (ícone na bandeja)
# 3. Abrir novo terminal PowerShell

# Se WSL 2 não estiver funcionando:
wsl --update
wsl --set-default-version 2
```

## Implementar no AutomationSettings.json

Configure o provider de banco desejado na seção `Database`:

```json
{
  "Database": {
    "Provider": "MySQL",
    "Host": "localhost", 
    "Port": 3306,
    "NomeBanco": "adrenalinespy_db",
    "Usuario": "adrenaline_user",
    "Senha": "SuaSenhaAqui123",
    "ConnectionTimeout": 30
  }
}
```

**Providers suportados:**

```json
// MySQL (padrão recomendado)
{
  "Provider": "MySQL",
  "Port": 3306
}

// PostgreSQL  
{
  "Provider": "PostgreSQL",
  "Port": 5432
}

// SQL Server
{
  "Provider": "SqlServer", 
  "Port": 1433
}
```

**⚠️ Importante:** Usar credenciais diferentes das padrão para segurança!

## Implementar no Config.cs

A integração já está pronta no `Config.cs` através do método `ObterConnectionString()`:

```csharp
// Método já implementado no Config.cs
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

**Uso no código:**
```csharp
// Obter connection string configurada
var config = Config.Instancia;
var connectionString = config.ObterConnectionString();

// A string já vem formatada para o provider escolhido no JSON
// MySQL: "Server=localhost;Port=3306;Database=adrenalinespy_db;..."
// PostgreSQL: "Host=localhost;Port=5432;Database=adrenalinespy_db;..."
// SQL Server: "Server=localhost,1433;Database=adrenalinespy_db;..."
```

## Montar nas Tasks

### 1. Criar docker-compose.yml para AdrenalineSpy

Crie na raiz do projeto `C:\Users\lucas\OneDrive\Documentos\CsharpProjects\AdrenalineSpy\docker-compose.yml`:

```yaml
# Docker Compose para AdrenalineSpy - Bancos de dados de desenvolvimento
version: '3.8'

services:
  # MySQL (padrão recomendado)
  mysql:
    image: mysql:8.0
    container_name: adrenalinespy-mysql
    environment:
      MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD:-AdrenalineSpy2024!}
      MYSQL_DATABASE: ${MYSQL_DATABASE:-adrenalinespy_db}
      MYSQL_USER: ${MYSQL_USER:-adrenaline_user}
      MYSQL_PASSWORD: ${MYSQL_PASSWORD:-AdrenalineUser2024!}
    ports:
      - "${MYSQL_PORT:-3306}:3306"
    volumes:
      - mysql_data:/var/lib/mysql
      - ./docker/mysql-init:/docker-entrypoint-initdb.d
    restart: unless-stopped
    networks:
      - adrenaline-network

  # PostgreSQL (alternativo)
  postgres:
    image: postgres:15
    container_name: adrenalinespy-postgres
    environment:
      POSTGRES_DB: ${POSTGRES_DB:-adrenalinespy_db}
      POSTGRES_USER: ${POSTGRES_USER:-adrenaline_user}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-AdrenalineUser2024!}
    ports:
      - "${POSTGRES_PORT:-5432}:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./docker/postgres-init:/docker-entrypoint-initdb.d
    restart: unless-stopped
    networks:
      - adrenaline-network

  # SQL Server (alternativo)
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: adrenalinespy-sqlserver
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: ${SQLSERVER_SA_PASSWORD:-AdrenalineSA2024!}
      MSSQL_PID: "Developer"
    ports:
      - "${SQLSERVER_PORT:-1433}:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    restart: unless-stopped
    networks:
      - adrenaline-network

volumes:
  mysql_data:
    name: adrenalinespy_mysql_data
  postgres_data:
    name: adrenalinespy_postgres_data
  sqlserver_data:
    name: adrenalinespy_sqlserver_data

networks:
  adrenaline-network:
    name: adrenalinespy-network
    driver: bridge
```

### 2. Criar .env para Variáveis (Opcional)

Crie arquivo `.env` na raiz do projeto:

```bash
# Configurações MySQL
MYSQL_ROOT_PASSWORD=AdrenalineSpy2024!
MYSQL_DATABASE=adrenalinespy_db
MYSQL_USER=adrenaline_user
MYSQL_PASSWORD=AdrenalineUser2024!
MYSQL_PORT=3306

# Configurações PostgreSQL
POSTGRES_DB=adrenalinespy_db
POSTGRES_USER=adrenaline_user
POSTGRES_PASSWORD=AdrenalineUser2024!
POSTGRES_PORT=5432

# Configurações SQL Server
SQLSERVER_SA_PASSWORD=AdrenalineSA2024!
SQLSERVER_PORT=1433
```

**⚠️ Importante:** Adicione `.env` no `.gitignore` (nunca versionar credenciais!)

### 3. Comandos Docker para AdrenalineSpy

```powershell
# Navegar até pasta do projeto
cd C:\Users\lucas\OneDrive\Documentos\CsharpProjects\AdrenalineSpy

# Iniciar apenas MySQL (recomendado)
docker compose up mysql -d

# Ou PostgreSQL
docker compose up postgres -d

# Ou SQL Server  
docker compose up sqlserver -d

# Iniciar todos os bancos (para testes)
docker compose up -d

# Ver status dos containers
docker compose ps

# Ver logs em tempo real
docker compose logs -f mysql

# Parar containers
docker compose down

# Parar e remover volumes (CUIDADO: apaga dados!)
docker compose down -v
```

### 4. Testar Conexão no AdrenalineSpy

Adicione método de teste na `MigrationTask.cs`:

```csharp
namespace AdrenalineSpy.Workflow.Tasks;

public class MigrationTask
{
    /// <summary>
    /// Testa conexão com banco Docker configurado
    /// </summary>
    public async Task<bool> TestarConexaoAsync()
    {
        try
        {
            LoggingTask.RegistrarInfo("Testando conexão com banco Docker...");
            
            var config = Config.Instancia;
            var connectionString = config.ObterConnectionString();
            
            LoggingTask.RegistrarInfo($"Provider: {config.Database.Provider}, Host: {config.Database.Host}:{config.Database.Port}");

            using var context = new AdrenalineDbContext();
            
            // Tentar conectar
            var canConnect = await context.Database.CanConnectAsync();
            
            if (canConnect)
            {
                LoggingTask.RegistrarInfo("✅ Conexão com banco Docker bem-sucedida!");
                
                // Verificar se database existe, se não criar
                await context.Database.EnsureCreatedAsync();
                LoggingTask.RegistrarInfo("✅ Database e tabelas criadas/verificadas");
                
                return true;
            }
            else
            {
                LoggingTask.RegistrarAviso("❌ Falha na conexão com banco Docker", "TestarConexao");
                return false;
            }
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, "MigrationTask.TestarConexao");
            return false;
        }
    }
}
```

### 5. Integrar Teste no Program.cs

```csharp
namespace AdrenalineSpy;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            LoggingTask.ConfigurarLogger();
            LoggingTask.RegistrarInfo("=== AdrenalineSpy RPA Iniciado ===");

            // Carregar e validar configurações
            var config = Config.Instancia;
            if (!config.Validar())
            {
                LoggingTask.RegistrarErro(new Exception("Configurações inválidas"), "Program");
                return;
            }

            // Testar conexão com banco Docker
            var migrationTask = new MigrationTask();
            bool conexaoOk = await migrationTask.TestarConexaoAsync();
            
            if (!conexaoOk)
            {
                LoggingTask.RegistrarErro(new Exception("Falha na conexão com banco"), "Program");
                LoggingTask.RegistrarInfo("Verifique se o Docker está rodando: docker compose up mysql -d");
                return;
            }

            LoggingTask.RegistrarInfo("Sistema pronto - Docker + Config + Logging funcionando!");

            // Aqui virão NavigationTask, ExtractionTask, etc.

        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro(ex, "Program - Erro Fatal");
        }
        finally
        {
            LoggingTask.FecharLogger();
        }
    }
}
```

### 6. Scripts de Inicialização (Opcional)

**docker/mysql-init/01-create-tables.sql:**
```sql
-- Executado automaticamente na primeira inicialização do MySQL
USE adrenalinespy_db;

-- Criar usuário específico para a aplicação (se necessário)
CREATE USER IF NOT EXISTS 'app_user'@'%' IDENTIFIED BY 'AppPassword2024!';
GRANT SELECT, INSERT, UPDATE, DELETE ON adrenalinespy_db.* TO 'app_user'@'%';
FLUSH PRIVILEGES;

-- Configurações de otimização
SET GLOBAL innodb_buffer_pool_size = 268435456; -- 256MB
SET GLOBAL max_connections = 100;
```

## Métodos Mais Usados

### Comandos Docker para Desenvolvimento

```powershell
# Iniciar banco para desenvolvimento
cd C:\Users\lucas\OneDrive\Documentos\CsharpProjects\AdrenalineSpy
docker compose up mysql -d

# Verificar se está rodando
docker compose ps
# Saída esperada: adrenalinespy-mysql running

# Ver logs do banco
docker compose logs -f mysql

# Parar quando terminar desenvolvimento
docker compose stop mysql

# Reiniciar banco
docker compose restart mysql
```

### Comandos para Cada Provider

```powershell
# MySQL (padrão recomendado)
docker compose up mysql -d
docker compose exec mysql mysql -u adrenaline_user -p adrenalinespy_db

# PostgreSQL
docker compose up postgres -d  
docker compose exec postgres psql -U adrenaline_user -d adrenalinespy_db

# SQL Server
docker compose up sqlserver -d
docker compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "AdrenalineSA2024!"
```

### Gerenciar Dados e Volumes

```powershell
# Ver volumes criados
docker volume ls | findstr adrenalinespy

# Backup de dados MySQL
docker compose exec mysql mysqldump -u root -p adrenalinespy_db > backup.sql

# Restaurar backup MySQL
docker compose exec -T mysql mysql -u root -p adrenalinespy_db < backup.sql

# Limpar dados (CUIDADO: apaga tudo!)
docker compose down -v
docker volume rm adrenalinespy_mysql_data

# Recrear container do zero
docker compose down
docker volume rm adrenalinespy_mysql_data
docker compose up mysql -d
```

### Debugging de Conexão

```powershell
# Verificar se container está rodando
docker compose ps

# Testar conectividade de rede
docker compose exec mysql ping localhost

# Ver configurações de rede
docker network ls | findstr adrenalinespy
docker network inspect adrenalinespy-network

# Verificar portas ocupadas
netstat -ano | findstr :3306
```

### Monitoramento e Performance

```powershell
# Ver uso de recursos
docker stats adrenalinespy-mysql

# Ver logs com timestamp
docker compose logs --timestamps mysql

# Limpar logs antigos
docker system prune -f

# Ver espaço usado pelos volumes
docker system df
```

### Rotina de Desenvolvimento

```powershell
# 1. Iniciar trabalho
cd C:\Users\lucas\OneDrive\Documentos\CsharpProjects\AdrenalineSpy
docker compose up mysql -d

# 2. Verificar se está conectando
dotnet run  # Testa conexão via MigrationTask

# 3. Durante desenvolvimento (se necessário)
docker compose logs -f mysql  # Ver logs em tempo real

# 4. Finalizar trabalho
docker compose stop mysql  # Para container (dados permanecem)
```

### Comandos de Manutenção

```powershell
# Atualizar image do MySQL
docker compose pull mysql
docker compose up mysql -d

# Ver informações do container
docker compose exec mysql mysql -V  # Versão MySQL
docker compose exec mysql free -h   # Memória disponível

# Executar comandos SQL diretos
docker compose exec mysql mysql -u root -p -e "SHOW DATABASES;"
docker compose exec mysql mysql -u root -p adrenalinespy_db -e "SHOW TABLES;"

# Criar backup agendado (pode adicionar ao Windows Task Scheduler)
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm"
docker compose exec mysql mysqldump -u root -p --all-databases > "backup_$timestamp.sql"
```

### Configuração de Produção

```powershell
# Arquivo docker-compose.prod.yml para produção
version: '3.8'
services:
  mysql:
    image: mysql:8.0
    container_name: adrenalinespy-mysql-prod
    environment:
      MYSQL_ROOT_PASSWORD_FILE: /run/secrets/mysql_root_password
      MYSQL_DATABASE: adrenalinespy_prod
      MYSQL_USER_FILE: /run/secrets/mysql_user  
      MYSQL_PASSWORD_FILE: /run/secrets/mysql_password
    ports:
      - "3306:3306"
    volumes:
      - mysql_prod_data:/var/lib/mysql
    deploy:
      resources:
        limits:
          memory: 512M
          cpus: '0.5'
    restart: always
    secrets:
      - mysql_root_password
      - mysql_user
      - mysql_password
      
secrets:
  mysql_root_password:
    file: ./secrets/mysql_root_password.txt
  mysql_user:
    file: ./secrets/mysql_user.txt  
  mysql_password:
    file: ./secrets/mysql_password.txt

volumes:
  mysql_prod_data:
    external: true
```

### Troubleshooting Específico do AdrenalineSpy

```powershell
# Erro: "Can't connect to MySQL server"
# 1. Verificar se Docker está rodando
docker --version

# 2. Verificar se container MySQL está up
docker compose ps

# 3. Verificar portas
netstat -ano | findstr :3306

# 4. Testar conexão manual
docker compose exec mysql mysql -u adrenaline_user -p

# Erro: "Access denied for user"
# 1. Verificar credenciais no AutomationSettings.json
# 2. Resetar senha se necessário:
docker compose exec mysql mysql -u root -p
# MySQL> ALTER USER 'adrenaline_user'@'%' IDENTIFIED BY 'NovaSenha123!';

# Erro: "Database 'adrenalinespy_db' doesn't exist"
# 1. Criar database manualmente
docker compose exec mysql mysql -u root -p -e "CREATE DATABASE adrenalinespy_db;"

# 2. Ou deixar Entity Framework criar:
dotnet ef database update

# Container não inicia
# 1. Ver logs detalhados
docker compose logs mysql

# 2. Verificar arquivo docker-compose.yml
# 3. Verificar se WSL 2 está funcionando
wsl -l -v
```

---

## 💡 Resumo para AdrenalineSpy

**Setup único (fazer uma vez):**
1. Instalar WSL 2 (`wsl --install`) + reiniciar
2. Instalar Docker Desktop + configurar WSL 2 integration
3. Criar `docker-compose.yml` na pasta do projeto
4. Configurar credenciais no `AutomationSettings.json`
5. Executar `docker compose up mysql -d`

**Uso diário:**
- **Iniciar:** `docker compose up mysql -d`  
- **Parar:** `docker compose stop mysql`
- **Logs:** `docker compose logs -f mysql`
- **Teste:** Executar `dotnet run` (MigrationTask testa conexão)

**Integração com projeto:**
- `Config.ObterConnectionString()` → connection string automática
- `MigrationTask.TestarConexaoAsync()` → validação de conexão
- Entity Framework migrations → criação automática das tabelas

**Resultado:**
- Banco de dados MySQL/PostgreSQL/SQL Server rodando em Docker
- Dados do Adrenaline.com.br persistidos de forma confiável
- Ambiente isolado e portátil para desenvolvimento
- Backup e migração simplificados

**Próxima etapa:** Implementar ferramentas complementares (EPPlus, CsvHelper, etc.)! 🚀
