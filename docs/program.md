# 📚 Program.cs - Ponto de Entrada da Aplicação

## Índice
1. [O que é o Program.cs](#o-que-é)
2. [Estrutura Básica Inicial](#estrutura-básica)
3. [Evoluindo com as Implementações](#evoluindo)
4. [Padrão de Desenvolvimento](#padrão)
5. [Exemplos de Evolução](#exemplos)
6. [Métodos Essenciais](#métodos)

---

## O que é o Program.cs {#o-que-é}

O **Program.cs** é o ponto de entrada da aplicação AdrenalineSpy. Começamos com uma estrutura básica e **evoluímos gradualmente** conforme implementamos cada tecnologia documentada.

### 🎯 Filosofia do Desenvolvimento
- **Comece simples**: Program.cs básico como o atual
- **Evolua gradualmente**: Cada `.md` mostra o que adicionar
- **Mantenha organizado**: Use o padrão `Config → Workflow → Tasks`

---

## Estrutura Básica Inicial {#estrutura-básica}

### Program.cs Inicial (Como Está Agora)
```csharp
using AdrenalineSpy;

namespace AdrenalineSpy
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Carregar configurações
            Config config = Config.Instancia;

            if (!config.Validar())
            {
                Console.WriteLine("❌ Configurações inválidas!");
                return;
            }

            // 2. Configurar logger
            LoggingTask.ConfigurarLogger();

            try
            {
                // 3. Usar logging
                LoggingTask.RegistrarInfo("=== Aplicação Iniciada ===");

                // Seu código aqui...

                LoggingTask.RegistrarInfo("=== Aplicação Finalizada ===");
            }
            catch (Exception ex)
            {
                LoggingTask.RegistrarErro(ex, "Program.Main");
            }
            finally
            {
                // 4. SEMPRE fechar
                LoggingTask.FecharLogger();
            }
        }
    }
}
```

### O que Já Temos
- ✅ **Config.Instancia**: Carregamento automático do `AutomationSettings.json`
- ✅ **LoggingTask**: Sistema de logging centralizado (Serilog)
- ✅ **Tratamento de exceções**: Try/catch/finally padrão
- ✅ **Validação básica**: Verifica se configurações estão válidas

---

## Evoluindo com as Implementações {#evoluindo}

### Como Cada Documentação Contribui

Cada arquivo `.md` na pasta `docs/` mostra exatamente **o que adicionar** no `Program.cs`:

| Documentação | O que Adiciona no Program.cs |
|-------------|----------------------------|
| **serilog.md** | ✅ `LoggingTask.ConfigurarLogger()` (já implementado) |
| **playwright.md** | `NavigationTask` para web scraping |
| **orm.md** | `MigrationTask` para banco de dados |
| **quartz.md** | Sistema de agendamento automático |
| **gui.md** | Interface gráfica (WPF/Avalonia) |
| **docker-setup.md** | Inicialização de containers |

### Padrão de Evolução
1. **Leia a documentação** da tecnologia que quer implementar
2. **Siga a seção "Montar nas Tasks"** para criar a Task
3. **Adicione no Program.cs** conforme mostrado no `.md`
4. **Teste isoladamente** antes de avançar

---

## Padrão de Desenvolvimento {#padrão}

### Estrutura Recomendada do Program.cs
```csharp
static void Main(string[] args)
{
    // 1. CONFIGURAÇÃO (sempre primeiro)
    Config config = Config.Instancia;
    if (!config.Validar()) return;
    
    // 2. LOGGING (sempre segundo)
    LoggingTask.ConfigurarLogger();
    
    try
    {
        // 3. INICIALIZAÇÃO
        LoggingTask.RegistrarInfo("=== Aplicação Iniciada ===");
        
        // 4. ARGUMENTOS (se necessário)
        ProcessarArgumentos(args);
        
        // 5. DEPENDÊNCIAS EXTERNAS (Docker, etc)
        InicializarDependencias();
        
        // 6. WORKFLOW PRINCIPAL
        ExecutarWorkflow();
        
        // 7. FINALIZAÇÃO
        LoggingTask.RegistrarInfo("=== Aplicação Finalizada ===");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Program.Main");
    }
    finally
    {
        // 8. LIMPEZA (sempre no finally)
        LimparRecursos();
        LoggingTask.FecharLogger();
    }
}
```

### Métodos Auxiliares Recomendados
```csharp
private static void ProcessarArgumentos(string[] args)
{
    // Processar --modo=gui, --modo=console, --modo=scheduler
}

private static void InicializarDependencias()
{
    // Docker containers, conexões de banco, etc
}

private static void ExecutarWorkflow()
{
    // Chamar Workflow.Executar() ou similar
}

private static void LimparRecursos()
{
    // Fechar conexões, limpar temporários, etc
}
```

---

## Exemplos de Evolução {#exemplos}

### Fase 1: Básico (Atual)
```csharp
static void Main(string[] args)
{
    Config config = Config.Instancia;
    LoggingTask.ConfigurarLogger();
    
    try
    {
        LoggingTask.RegistrarInfo("Aplicação iniciada");
        // TODO: Implementar funcionalidades
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Program.Main");
    }
    finally
    {
        LoggingTask.FecharLogger();
    }
}
```

### Fase 2: + Web Scraping (playwright.md)
```csharp
static void Main(string[] args)
{
    Config config = Config.Instancia;
    LoggingTask.ConfigurarLogger();
    
    try
    {
        LoggingTask.RegistrarInfo("Aplicação iniciada");
        
        // Adicionado após implementar playwright.md
        var navigationTask = new NavigationTask();
        var urls = await navigationTask.ColetarUrlsAsync();
        
        LoggingTask.RegistrarInfo($"Coletadas {urls.Count} URLs");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Program.Main");
    }
    finally
    {
        LoggingTask.FecharLogger();
    }
}
```

### Fase 3: + Banco de Dados (orm.md)
```csharp
static void Main(string[] args)
{
    Config config = Config.Instancia;
    LoggingTask.ConfigurarLogger();
    
    try
    {
        LoggingTask.RegistrarInfo("Aplicação iniciada");
        
        var navigationTask = new NavigationTask();
        var urls = await navigationTask.ColetarUrlsAsync();
        
        // Adicionado após implementar orm.md
        var migrationTask = new MigrationTask();
        await migrationTask.SalvarUrlsAsync(urls);
        
        LoggingTask.RegistrarInfo("Dados salvos no banco");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Program.Main");
    }
    finally
    {
        LoggingTask.FecharLogger();
    }
}
```

---

## Métodos Essenciais {#métodos}

### Validação de Configurações
```csharp
private static bool ValidarConfiguracoes()
{
    var config = Config.Instancia;
    
    if (!config.Validar())
    {
        Console.WriteLine("❌ Configurações inválidas no AutomationSettings.json");
        return false;
    }
    
    return true;
}
```

### Processamento de Argumentos
```csharp
private static void ProcessarArgumentos(string[] args)
{
    foreach (string arg in args)
    {
        if (arg.StartsWith("--modo="))
        {
            string modo = arg.Substring(7);
            ExecutarPorModo(modo);
        }
        else if (arg == "--help")
        {
            ExibirAjuda();
        }
    }
}

private static void ExecutarPorModo(string modo)
{
    switch (modo.ToLower())
    {
        case "console":
            ExecutarModoConsole();
            break;
        case "gui":
            ExecutarModoGUI();
            break;
        case "scheduler":
            ExecutarModoScheduler();
            break;
        default:
            Console.WriteLine($"❌ Modo '{modo}' não reconhecido");
            break;
    }
}
```

### Tratamento de Exceções Específicas
```csharp
private static void TratarExcecao(Exception ex)
{
    switch (ex)
    {
        case HttpRequestException httpEx:
            LoggingTask.RegistrarErro(httpEx, "Erro de conexão HTTP");
            break;
        case SqlException sqlEx:
            LoggingTask.RegistrarErro(sqlEx, "Erro de banco de dados");
            break;
        case PlaywrightException pwEx:
            LoggingTask.RegistrarErro(pwEx, "Erro de automação web");
            break;
        default:
            LoggingTask.RegistrarErro(ex, "Erro geral");
            break;
    }
}
```

### Limpeza de Recursos
```csharp
private static void LimparRecursos()
{
    try
    {
        // Fechar navegadores Playwright
        NavigationTask.FecharNavegadores();
        
        // Fechar conexões de banco
        MigrationTask.FecharConexoes();
        
        // Limpar arquivos temporários
        LimparArquivosTemporarios();
        
        LoggingTask.RegistrarInfo("Recursos limpos com sucesso");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro(ex, "Erro ao limpar recursos");
    }
}
```

---

## 💡 Próximos Passos

1. **Mantenha este Program.cs básico** como está
2. **Escolha uma tecnologia** para implementar (ex: `playwright.md`)
3. **Siga o guia** da documentação escolhida
4. **Adicione no Program.cs** conforme mostrado no `.md`
5. **Teste isoladamente** antes de avançar
6. **Repita o processo** para outras tecnologias

### Ordem Recomendada de Implementação
1. ✅ **Serilog** (já implementado)
2. 🎯 **Playwright** (web scraping básico)
3. 🎯 **ORM** (salvar dados)
4. 🎯 **Quartz** (agendamento)
5. 🎯 **GUI** (interface)
6. 🎯 **Deploy** (produção)

Cada implementação adiciona funcionalidade **incrementalmente** ao Program.cs, mantendo a base sólida e organizando o código de forma profissional.
