# FlaUI - Automação de Interface Windows

## O que é FlaUI

**FlaUI** é uma biblioteca .NET para automação de interfaces gráficas Windows (desktop apps), usando tecnologias de acessibilidade do Windows.

**Onde é usado no AdrenalineSpy:**
- Automatizar browsers alternativos caso Playwright falhe
- Interagir com janelas de sistema (Windows Explorer, diálogos)
- Controlar aplicações desktop que complementam o scraping
- Automatizar notificações do Windows (Action Center)
- Backup de automação quando a interface web não responde
- Controlar aplicações de terceiros (editores, clientes FTP, etc.)

⚠️ **IMPORTANTE**: FlaUI funciona apenas no Windows e requer UI Automation habilitado no sistema.

## Como Instalar

### 1. Instalar Pacotes FlaUI

```powershell
dotnet add package FlaUI.Core
dotnet add package FlaUI.UIA3
```

### 2. Verificar .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <UseWPF>true</UseWPF>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="FlaUI.Core" Version="4.0.0" />
    <PackageReference Include="FlaUI.UIA3" Version="4.0.0" />
  </ItemGroup>
</Project>
```

### 3. Verificar UI Automation (Windows)

```powershell
# Verificar se UI Automation está habilitado
Get-Service -Name "UI0Detect" | Select-Object Status, StartType
```

Se não estiver executando:
```powershell
# Habilitar UI Automation (executar como Administrador)
Set-Service -Name "UI0Detect" -StartupType Automatic
Start-Service -Name "UI0Detect"
```

## Implementar no AutomationSettings.json

Adicione configurações de automação desktop na raiz do JSON:

```json
{
  "Navegacao": {
    "UrlBase": "https://www.adrenaline.com.br",
    "DelayEntrePaginas": 2000
  },
  "AutomacaoDesktop": {
    "HabilitarFlaUI": true,
    "TimeoutPadrao": 30000,
    "DelayEntreAcoes": 500,
    "TentativasMaximas": 3,
    "CapturarScreenshots": true,
    "DiretorioScreenshots": "screenshots/",
    "ConfiguracaoBrowser": {
      "BrowserAlternativo": "chrome.exe",
      "ArgumentosLancamento": "--start-maximized --disable-web-security",
      "TimeoutLancamento": 10000
    },
    "ConfiguracaoSistema": {
      "AutomatizarNotificacoes": false,
      "InteragirActionCenter": false,
      "GerenciarArquivos": true,
      "TimeoutJanelaSistema": 15000
    },
    "ElementosComuns": {
      "SeletorBotaoOk": "Button[@Name='OK']",
      "SeletorBotaoCancelar": "Button[@Name='Cancelar']",
      "SeletorCaixaTexto": "Edit[@AutomationId='textBox']",
      "SeletorJanelaPrincipal": "Window[@ClassName='Chrome_WidgetWin_1']"
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

**Configurações específicas do FlaUI:**
- **`HabilitarFlaUI`**: Liga/desliga automação desktop
- **`ConfiguracaoBrowser`**: Para controlar browsers alternativos
- **`ConfiguracaoSistema`**: Interação com Windows (notificações, arquivos)
- **`ElementosComuns`**: Seletores reutilizáveis para elementos UI

## Implementar no Config.cs

Adicione classes de configuração para FlaUI:

```csharp
public class ConfiguracaoBrowserConfig
{
    public string BrowserAlternativo { get; set; } = "chrome.exe";
    public string ArgumentosLancamento { get; set; } = "--start-maximized --disable-web-security";
    public int TimeoutLancamento { get; set; } = 10000;
}

public class ConfiguracaoSistemaConfig
{
    public bool AutomatizarNotificacoes { get; set; } = false;
    public bool InteragirActionCenter { get; set; } = false;
    public bool GerenciarArquivos { get; set; } = true;
    public int TimeoutJanelaSistema { get; set; } = 15000;
}

public class ElementosComunsConfig
{
    public string SeletorBotaoOk { get; set; } = "Button[@Name='OK']";
    public string SeletorBotaoCancelar { get; set; } = "Button[@Name='Cancelar']";
    public string SeletorCaixaTexto { get; set; } = "Edit[@AutomationId='textBox']";
    public string SeletorJanelaPrincipal { get; set; } = "Window[@ClassName='Chrome_WidgetWin_1']";
}

public class AutomacaoDesktopConfig
{
    public bool HabilitarFlaUI { get; set; } = true;
    public int TimeoutPadrao { get; set; } = 30000;
    public int DelayEntreAcoes { get; set; } = 500;
    public int TentativasMaximas { get; set; } = 3;
    public bool CapturarScreenshots { get; set; } = true;
    public string DiretorioScreenshots { get; set; } = "screenshots/";
    public ConfiguracaoBrowserConfig ConfiguracaoBrowser { get; set; } = new();
    public ConfiguracaoSistemaConfig ConfiguracaoSistema { get; set; } = new();
    public ElementosComunsConfig ElementosComuns { get; set; } = new();
}

public class Config
{
    // ... propriedades existentes ...
    public AutomacaoDesktopConfig AutomacaoDesktop { get; set; } = new();
    
    /// <summary>
    /// Obtém caminho para screenshots do FlaUI
    /// </summary>
    public string ObterCaminhoScreenshot(string nomeArquivo)
    {
        Directory.CreateDirectory(AutomacaoDesktop.DiretorioScreenshots);
        
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var nomeComTimestamp = $"{timestamp}_{nomeArquivo}.png";
        
        return Path.Combine(AutomacaoDesktop.DiretorioScreenshots, nomeComTimestamp);
    }

    /// <summary>
    /// Verifica se FlaUI está habilitado e disponível
    /// </summary>
    public bool FlaUIDisponivel()
    {
        try
        {
            return AutomacaoDesktop.HabilitarFlaUI && 
                   Environment.OSVersion.Platform == PlatformID.Win32NT;
        }
        catch
        {
            return false;
        }
    }
}
```

## Montar nas Tasks

Crie a classe `DesktopAutomationTask.cs` na pasta `Workflow/Tasks/`:

```csharp
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace AdrenalineSpy.Workflow.Tasks;

/// <summary>
/// Gerencia automação de interfaces desktop Windows para o AdrenalineSpy
/// </summary>
public static class DesktopAutomationTask
{
    private static UIA3Automation? _automation;
    private static readonly object _lock = new();

    /// <summary>
    /// Obtém instância singleton do UIA3Automation
    /// </summary>
    private static UIA3Automation ObterAutomation()
    {
        if (_automation == null)
        {
            lock (_lock)
            {
                _automation ??= new UIA3Automation();
            }
        }
        return _automation;
    }

    /// <summary>
    /// Abre browser alternativo caso Playwright falhe
    /// </summary>
    public static async Task<bool> AbrirBrowserAlternativo(string url)
    {
        try
        {
            if (!Config.Instancia.FlaUIDisponivel())
            {
                LoggingTask.RegistrarAviso("🖥️ FlaUI não disponível ou desabilitado");
                return false;
            }

            var config = Config.Instancia.AutomacaoDesktop.ConfiguracaoBrowser;
            
            // Iniciar processo do browser
            var processInfo = new ProcessStartInfo
            {
                FileName = config.BrowserAlternativo,
                Arguments = $"{config.ArgumentosLancamento} \"{url}\"",
                UseShellExecute = true
            };

            var processo = Process.Start(processInfo);
            if (processo == null)
            {
                LoggingTask.RegistrarErro($"Falha ao iniciar browser alternativo: {config.BrowserAlternativo}");
                return false;
            }

            // Aguardar janela aparecer
            await Task.Delay(config.TimeoutLancamento);

            // Verificar se janela está aberta
            var automation = ObterAutomation();
            var janelas = automation.GetDesktop().FindAllChildren(cf => 
                cf.ByControlType(ControlType.Window).And(
                cf.ByProcessId(processo.Id)));

            if (janelas.Any())
            {
                LoggingTask.RegistrarInfo($"🖥️ Browser alternativo aberto: PID {processo.Id}");
                return true;
            }

            LoggingTask.RegistrarAviso("🖥️ Browser alternativo iniciado mas janela não detectada");
            return false;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao abrir browser alternativo", ex);
            return false;
        }
    }

    /// <summary>
    /// Captura screenshot da tela inteira ou janela específica
    /// </summary>
    public static async Task<string?> CapturarScreenshot(string? nomeJanela = null, string? sufixo = null)
    {
        try
        {
            if (!Config.Instancia.AutomacaoDesktop.CapturarScreenshots)
            {
                return null;
            }

            var automation = ObterAutomation();
            var nomeArquivo = $"screenshot{(string.IsNullOrWhiteSpace(sufixo) ? "" : $"_{sufixo}")}";
            var caminhoArquivo = Config.Instancia.ObterCaminhoScreenshot(nomeArquivo);

            if (string.IsNullOrWhiteSpace(nomeJanela))
            {
                // Screenshot da tela inteira
                Capture.Screen().ToFile(caminhoArquivo, ImageFormat.Png);
            }
            else
            {
                // Screenshot de janela específica
                var janela = EncontrarJanela(nomeJanela);
                if (janela != null)
                {
                    Capture.Element(janela).ToFile(caminhoArquivo, ImageFormat.Png);
                }
                else
                {
                    LoggingTask.RegistrarAviso($"🖥️ Janela '{nomeJanela}' não encontrada para screenshot");
                    return null;
                }
            }

            LoggingTask.RegistrarInfo($"📸 Screenshot salvo: {Path.GetFileName(caminhoArquivo)}");
            return caminhoArquivo;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao capturar screenshot", ex);
            return null;
        }
    }

    /// <summary>
    /// Encontra janela por nome ou classe
    /// </summary>
    public static Window? EncontrarJanela(string identificador, int timeoutMs = 0)
    {
        try
        {
            var automation = ObterAutomation();
            var timeout = timeoutMs > 0 ? timeoutMs : Config.Instancia.AutomacaoDesktop.TimeoutPadrao;

            return Retry.WhileNull(() =>
            {
                // Tentar por nome da janela
                var janelasPorNome = automation.GetDesktop().FindAllChildren(cf => 
                    cf.ByControlType(ControlType.Window).And(
                    cf.ByName(identificador)));

                if (janelasPorNome.Any())
                    return janelasPorNome.First().AsWindow();

                // Tentar por classe
                var janelasPorClasse = automation.GetDesktop().FindAllChildren(cf => 
                    cf.ByControlType(ControlType.Window).And(
                    cf.ByClassName(identificador)));

                return janelasPorClasse.FirstOrDefault()?.AsWindow();

            }, TimeSpan.FromMilliseconds(timeout), TimeSpan.FromMilliseconds(500));
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro($"Erro ao encontrar janela '{identificador}'", ex);
            return null;
        }
    }

    /// <summary>
    /// Clica em elemento por seletor XPath
    /// </summary>
    public static async Task<bool> ClicarElemento(Window janela, string seletor, int timeoutMs = 0)
    {
        try
        {
            var timeout = timeoutMs > 0 ? timeoutMs : Config.Instancia.AutomacaoDesktop.TimeoutPadrao;

            var elemento = Retry.WhileNull(() =>
            {
                return janela.FindFirstByXPath(seletor);
            }, TimeSpan.FromMilliseconds(timeout), TimeSpan.FromMilliseconds(500));

            if (elemento == null)
            {
                LoggingTask.RegistrarAviso($"🖥️ Elemento não encontrado: {seletor}");
                return false;
            }

            // Aguardar elemento estar clicável
            await Task.Delay(Config.Instancia.AutomacaoDesktop.DelayEntreAcoes);

            if (elemento.IsEnabled && elemento.IsOffscreen == false)
            {
                elemento.Click();
                LoggingTask.RegistrarInfo($"🖱️ Clique realizado: {seletor}");
                
                await Task.Delay(Config.Instancia.AutomacaoDesktop.DelayEntreAcoes);
                return true;
            }

            LoggingTask.RegistrarAviso($"🖥️ Elemento não está clicável: {seletor}");
            return false;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro($"Erro ao clicar elemento '{seletor}'", ex);
            return false;
        }
    }

    /// <summary>
    /// Preenche texto em campo de entrada
    /// </summary>
    public static async Task<bool> PreencherTexto(Window janela, string seletor, string texto, bool limparAntes = true)
    {
        try
        {
            var elemento = janela.FindFirstByXPath(seletor);
            if (elemento == null)
            {
                LoggingTask.RegistrarAviso($"🖥️ Campo de texto não encontrado: {seletor}");
                return false;
            }

            var campoTexto = elemento.AsTextBox();
            if (campoTexto == null)
            {
                LoggingTask.RegistrarAviso($"🖥️ Elemento não é campo de texto: {seletor}");
                return false;
            }

            await Task.Delay(Config.Instancia.AutomacaoDesktop.DelayEntreAcoes);

            if (limparAntes)
            {
                campoTexto.Text = string.Empty;
            }

            campoTexto.Text = texto;
            LoggingTask.RegistrarInfo($"⌨️ Texto preenchido no campo: {seletor}");

            await Task.Delay(Config.Instancia.AutomacaoDesktop.DelayEntreAcoes);
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro($"Erro ao preencher texto no campo '{seletor}'", ex);
            return false;
        }
    }

    /// <summary>
    /// Gerencia arquivos via Windows Explorer (backup se web scraping falhar)
    /// </summary>
    public static async Task<bool> GerenciarArquivosExportacao(string diretorioDestino)
    {
        try
        {
            if (!Config.Instancia.AutomacaoDesktop.ConfiguracaoSistema.GerenciarArquivos)
            {
                LoggingTask.RegistrarInfo("🖥️ Gerenciamento de arquivos via FlaUI desabilitado");
                return true;
            }

            // Abrir Windows Explorer no diretório
            Process.Start("explorer.exe", diretorioDestino);
            await Task.Delay(2000);

            // Encontrar janela do Explorer
            var explorer = EncontrarJanela("File Explorer") ?? EncontrarJanela("Windows Explorer");
            if (explorer == null)
            {
                LoggingTask.RegistrarAviso("🖥️ Windows Explorer não encontrado");
                return false;
            }

            LoggingTask.RegistrarInfo($"📁 Windows Explorer aberto: {diretorioDestino}");

            // Capturar screenshot do diretório
            await CapturarScreenshot("File Explorer", "gerenciamento_arquivos");

            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao gerenciar arquivos via Explorer", ex);
            return false;
        }
    }

    /// <summary>
    /// Executa retry de operação com FlaUI
    /// </summary>
    public static async Task<T?> ExecutarComRetry<T>(Func<Task<T>> operacao, string descricaoOperacao)
    {
        var maxTentativas = Config.Instancia.AutomacaoDesktop.TentativasMaximas;

        for (int tentativa = 1; tentativa <= maxTentativas; tentativa++)
        {
            try
            {
                var resultado = await operacao();
                if (resultado != null)
                {
                    if (tentativa > 1)
                    {
                        LoggingTask.RegistrarInfo($"🔄 {descricaoOperacao} sucesso na tentativa {tentativa}");
                    }
                    return resultado;
                }
            }
            catch (Exception ex)
            {
                LoggingTask.RegistrarAviso($"🔄 {descricaoOperacao} falhou (tentativa {tentativa}/{maxTentativas}): {ex.Message}");
                
                if (tentativa < maxTentativas)
                {
                    await Task.Delay(1000 * tentativa); // Backoff exponencial
                }
            }
        }

        LoggingTask.RegistrarErro($"❌ {descricaoOperacao} falhou após {maxTentativas} tentativas");
        return default(T);
    }

    /// <summary>
    /// Limpa recursos do FlaUI
    /// </summary>
    public static void LimparRecursos()
    {
        try
        {
            lock (_lock)
            {
                _automation?.Dispose();
                _automation = null;
            }
            
            LoggingTask.RegistrarInfo("🧹 Recursos FlaUI liberados");
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Aviso ao liberar recursos FlaUI: {ex.Message}");
        }
    }
}
```

## Métodos Mais Usados

### Inicializar FlaUI e Encontrar Janelas

```csharp
using FlaUI.UIA3;
using FlaUI.Core.AutomationElements;

// Criar automação
var automation = new UIA3Automation();

// Encontrar janela por nome
var janela = automation.GetDesktop()
    .FindFirstChild(cf => cf.ByName("Google Chrome"))
    ?.AsWindow();

if (janela != null)
{
    LoggingTask.RegistrarInfo("✅ Janela Chrome encontrada");
}
```

### Interagir com Elementos (Click, Texto)

```csharp
// Clicar em botão
var botao = janela.FindFirstByXPath("//Button[@Name='OK']");
if (botao != null && botao.IsEnabled)
{
    botao.Click();
    LoggingTask.RegistrarInfo("🖱️ Botão OK clicado");
}

// Preencher campo de texto
var campoTexto = janela.FindFirstByXPath("//Edit[@AutomationId='searchBox']");
if (campoTexto != null)
{
    campoTexto.AsTextBox().Text = "https://www.adrenaline.com.br";
    LoggingTask.RegistrarInfo("⌨️ URL preenchida na barra de endereço");
}
```

### Capturar Screenshots para Debug

```csharp
using FlaUI.Core.Tools;

// Screenshot da tela inteira
var caminhoScreenshot = Config.Instancia.ObterCaminhoScreenshot("erro_scraping");
Capture.Screen().ToFile(caminhoScreenshot, ImageFormat.Png);

// Screenshot de janela específica
if (janela != null)
{
    var caminhoJanela = Config.Instancia.ObterCaminhoScreenshot("janela_browser");
    Capture.Element(janela).ToFile(caminhoJanela, ImageFormat.Png);
}

LoggingTask.RegistrarInfo("📸 Screenshots de debug capturados");
```

### Aguardar Elementos com Timeout

```csharp
using FlaUI.Core.Tools;

// Aguardar elemento aparecer (com retry)
var elemento = Retry.WhileNull(() => 
{
    return janela.FindFirstByXPath("//Button[@Name='Entrar']");
}, TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(500));

if (elemento != null)
{
    elemento.Click();
    LoggingTask.RegistrarInfo("✅ Elemento encontrado e clicado após retry");
}
else
{
    LoggingTask.RegistrarErro("❌ Timeout: elemento não encontrado");
}
```

### Integração como Backup do Playwright

```csharp
// No NavigationTask.cs - usar FlaUI como fallback
public async Task<bool> Navegar(string url)
{
    try
    {
        // Tentar Playwright primeiro
        var sucessoPlaywright = await NavegacaoPlaywright(url);
        if (sucessoPlaywright)
        {
            return true;
        }

        LoggingTask.RegistrarAviso("🌐 Playwright falhou, tentando FlaUI como backup");
        
        // Fallback: FlaUI
        if (Config.Instancia.FlaUIDisponivel())
        {
            var sucessoFlaUI = await DesktopAutomationTask.AbrirBrowserAlternativo(url);
            if (sucessoFlaUI)
            {
                LoggingTask.RegistrarInfo("🖥️ Navegação via FlaUI bem-sucedida");
                return true;
            }
        }

        LoggingTask.RegistrarErro("❌ Falha na navegação: Playwright e FlaUI falharam");
        return false;
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro("Erro geral na navegação", ex);
        return false;
    }
}
```

### Limpeza de Recursos

```csharp
// Sempre limpar recursos no final ou em using
public void Dispose()
{
    try
    {
        DesktopAutomationTask.LimparRecursos();
        LoggingTask.RegistrarInfo("🧹 Recursos de automação liberados");
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarAviso($"Aviso na limpeza: {ex.Message}");
    }
}
```
