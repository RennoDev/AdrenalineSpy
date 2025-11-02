# InputSimulator - Simulação de Teclado e Mouse

## O que é InputSimulator

**InputSimulator** é uma biblioteca .NET para simular entrada de teclado e mouse programaticamente no Windows, enviando eventos diretamente ao sistema operacional.

**Onde é usado no AdrenalineSpy:**
- Simular teclas de atalho quando elementos não respondem (Ctrl+F5, F12)
- Contornar captchas ou pop-ups que bloqueiam Playwright
- Simular scroll da página para carregar conteúdo dinâmico
- Automatizar ações de copiar/colar durante extração de dados
- Backup de interação quando FlaUI ou Playwright falham
- Simular comportamento humano (delays, movimentos do mouse)

⚠️ **IMPORTANTE**: Funciona apenas no Windows e pode ser detectado por sistemas anti-bot mais avançados.

## Como Instalar

### 1. Instalar Pacote InputSimulator

```powershell
dotnet add package InputSimulator
```

### 2. Verificar .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="InputSimulator" Version="1.0.4" />
  </ItemGroup>
</Project>
```

### 3. Verificar Permissões Windows

O InputSimulator não requer configurações especiais, mas pode ser bloqueado por antivírus ou UAC em algumas situações.

```powershell
# Verificar se aplicação tem permissões para simular entrada
# (Executar como teste simples)
dotnet run # Testar se simulação funciona
```

## Implementar no AutomationSettings.json

Adicione configurações de simulação na seção `AutomacaoDesktop`:

```json
{
  "Navegacao": {
    "UrlBase": "https://www.adrenaline.com.br",
    "DelayEntrePaginas": 2000
  },
  "AutomacaoDesktop": {
    "HabilitarFlaUI": true,
    "HabilitarInputSimulator": true,
    "SimulacaoInput": {
      "HabilitarTeclado": true,
      "HabilitarMouse": true,
      "DelayEntreTeclas": 50,
      "DelayEntreCliques": 200,
      "VelocidadeDigitacao": 80,
      "SimularComportamentoHumano": true,
      "VariacaoDelay": 30
    },
    "TeclasAtalho": {
      "Refresh": "F5",
      "RefreshForcado": "Ctrl+F5",
      "DevTools": "F12",
      "NovaAba": "Ctrl+T",
      "FecharAba": "Ctrl+W",
      "Copiar": "Ctrl+C",
      "Colar": "Ctrl+V"
    },
    "ConfiguracaoMouse": {
      "VelocidadeMovimento": 100,
      "PrecisaoClique": true,
      "ScrollVelocidade": 3,
      "SimularMovimentoNatural": true
    },
    "AcoesEmergencia": {
      "UsarQuandoPlaywrightFalha": true,
      "UsarQuandoFlaUIFalha": true,
      "TentativasMaximas": 5,
      "IntervalEntreAcoes": 1000
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

**Configurações específicas do InputSimulator:**
- **`SimulacaoInput`**: Controles gerais de teclado e mouse
- **`TeclasAtalho`**: Mapeamento de atalhos importantes
- **`ConfiguracaoMouse`**: Comportamento de cliques e movimento
- **`AcoesEmergencia`**: Quando usar InputSimulator como fallback

## Implementar no Config.cs

Expanda as classes de configuração desktop:

```csharp
public class SimulacaoInputConfig
{
    public bool HabilitarTeclado { get; set; } = true;
    public bool HabilitarMouse { get; set; } = true;
    public int DelayEntreTeclas { get; set; } = 50;
    public int DelayEntreCliques { get; set; } = 200;
    public int VelocidadeDigitacao { get; set; } = 80;
    public bool SimularComportamentoHumano { get; set; } = true;
    public int VariacaoDelay { get; set; } = 30;
}

public class TeclasAtalhoConfig
{
    public string Refresh { get; set; } = "F5";
    public string RefreshForcado { get; set; } = "Ctrl+F5";
    public string DevTools { get; set; } = "F12";
    public string NovaAba { get; set; } = "Ctrl+T";
    public string FecharAba { get; set; } = "Ctrl+W";
    public string Copiar { get; set; } = "Ctrl+C";
    public string Colar { get; set; } = "Ctrl+V";
}

public class ConfiguracaoMouseConfig
{
    public int VelocidadeMovimento { get; set; } = 100;
    public bool PrecisaoClique { get; set; } = true;
    public int ScrollVelocidade { get; set; } = 3;
    public bool SimularMovimentoNatural { get; set; } = true;
}

public class AcoesEmergenciaConfig
{
    public bool UsarQuandoPlaywrightFalha { get; set; } = true;
    public bool UsarQuandoFlaUIFalha { get; set; } = true;
    public int TentativasMaximas { get; set; } = 5;
    public int IntervalEntreAcoes { get; set; } = 1000;
}

public class AutomacaoDesktopConfig
{
    // ... propriedades existentes do FlaUI ...
    public bool HabilitarInputSimulator { get; set; } = true;
    public SimulacaoInputConfig SimulacaoInput { get; set; } = new();
    public TeclasAtalhoConfig TeclasAtalho { get; set; } = new();
    public ConfiguracaoMouseConfig ConfiguracaoMouse { get; set; } = new();
    public AcoesEmergenciaConfig AcoesEmergencia { get; set; } = new();
}

public class Config
{
    // ... propriedades e métodos existentes ...
    
    /// <summary>
    /// Verifica se InputSimulator está disponível
    /// </summary>
    public bool InputSimulatorDisponivel()
    {
        try
        {
            return AutomacaoDesktop.HabilitarInputSimulator && 
                   Environment.OSVersion.Platform == PlatformID.Win32NT;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Calcula delay com variação humana
    /// </summary>
    public int ObterDelayComVariacao(int delayBase)
    {
        if (!AutomacaoDesktop.SimulacaoInput.SimularComportamentoHumano)
        {
            return delayBase;
        }

        var variacao = AutomacaoDesktop.SimulacaoInput.VariacaoDelay;
        var random = new Random();
        var variacaoRange = random.Next(-variacao, variacao + 1);
        
        return Math.Max(10, delayBase + variacaoRange); // Mínimo 10ms
    }
}
```

## Montar nas Tasks

Crie a classe `InputSimulationTask.cs` na pasta `Workflow/Tasks/`:

```csharp
using WindowsInput;
using WindowsInput.Native;
using System.Drawing;

namespace AdrenalineSpy.Workflow.Tasks;

/// <summary>
/// Gerencia simulação de entrada de teclado e mouse para o AdrenalineSpy
/// </summary>
public static class InputSimulationTask
{
    private static readonly InputSimulator _inputSimulator = new();
    private static readonly Random _random = new();

    /// <summary>
    /// Simula refresh forçado da página (Ctrl+F5)
    /// </summary>
    public static async Task<bool> RefreshForcadoPagina()
    {
        try
        {
            if (!Config.Instancia.InputSimulatorDisponivel())
            {
                LoggingTask.RegistrarAviso("⌨️ InputSimulator não disponível");
                return false;
            }

            LoggingTask.RegistrarInfo("🔄 Executando refresh forçado via InputSimulator");

            // Simular Ctrl+F5
            _inputSimulator.Keyboard.KeyDown(VirtualKeyCode.LCONTROL);
            await Task.Delay(Config.Instancia.ObterDelayComVariacao(50));
            
            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.F5);
            await Task.Delay(Config.Instancia.ObterDelayComVariacao(50));
            
            _inputSimulator.Keyboard.KeyUp(VirtualKeyCode.LCONTROL);

            LoggingTask.RegistrarInfo("✅ Refresh forçado simulado com sucesso");
            
            // Aguardar página recarregar
            await Task.Delay(Config.Instancia.ObterDelayComVariacao(3000));
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao simular refresh forçado", ex);
            return false;
        }
    }

    /// <summary>
    /// Simula scroll da página para carregar conteúdo dinâmico
    /// </summary>
    public static async Task<bool> ScrollPaginaParaCarregarConteudo(int quantidade = 3)
    {
        try
        {
            if (!Config.Instancia.InputSimulatorDisponivel() || 
                !Config.Instancia.AutomacaoDesktop.SimulacaoInput.HabilitarMouse)
            {
                return false;
            }

            LoggingTask.RegistrarInfo($"📜 Iniciando scroll automático ({quantidade} scrolls)");

            var velocidade = Config.Instancia.AutomacaoDesktop.ConfiguracaoMouse.ScrollVelocidade;

            for (int i = 0; i < quantidade; i++)
            {
                // Scroll para baixo
                _inputSimulator.Mouse.VerticalScroll(-velocidade);
                
                var delay = Config.Instancia.ObterDelayComVariacao(800);
                await Task.Delay(delay);

                // Pequena pausa para simular leitura
                if (Config.Instancia.AutomacaoDesktop.ConfiguracaoMouse.SimularMovimentoNatural)
                {
                    await Task.Delay(_random.Next(200, 600));
                }
            }

            // Scroll leve para cima para simular ajuste
            _inputSimulator.Mouse.VerticalScroll(1);
            
            LoggingTask.RegistrarInfo("✅ Scroll automático concluído");
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro durante scroll automático", ex);
            return false;
        }
    }

    /// <summary>
    /// Simula tecla F12 para abrir DevTools (debug)
    /// </summary>
    public static async Task<bool> AbrirDevTools()
    {
        try
        {
            if (!Config.Instancia.InputSimulatorDisponivel())
            {
                return false;
            }

            LoggingTask.RegistrarInfo("🛠️ Abrindo DevTools via F12");

            _inputSimulator.Keyboard.KeyPress(VirtualKeyCode.F12);
            
            await Task.Delay(Config.Instancia.ObterDelayComVariacao(2000));
            
            LoggingTask.RegistrarInfo("✅ Comando F12 enviado");
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao abrir DevTools", ex);
            return false;
        }
    }

    /// <summary>
    /// Simula digitação natural de texto
    /// </summary>
    public static async Task<bool> DigitarTextoNatural(string texto)
    {
        try
        {
            if (!Config.Instancia.InputSimulatorDisponivel() || 
                !Config.Instancia.AutomacaoDesktop.SimulacaoInput.HabilitarTeclado)
            {
                return false;
            }

            LoggingTask.RegistrarInfo($"⌨️ Digitando texto natural: '{texto.Substring(0, Math.Min(20, texto.Length))}...'");

            var velocidade = Config.Instancia.AutomacaoDesktop.SimulacaoInput.VelocidadeDigitacao;
            var delayBase = Math.Max(10, 1000 / velocidade); // ms por caractere

            foreach (char caractere in texto)
            {
                _inputSimulator.Keyboard.TextEntry(caractere);
                
                var delay = Config.Instancia.ObterDelayComVariacao(delayBase);
                await Task.Delay(delay);
            }

            LoggingTask.RegistrarInfo("✅ Digitação natural concluída");
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro na digitação natural", ex);
            return false;
        }
    }

    /// <summary>
    /// Simula clique em coordenada específica
    /// </summary>
    public static async Task<bool> ClicarCoordenada(int x, int y, bool cliqueDirecto = true)
    {
        try
        {
            if (!Config.Instancia.InputSimulatorDisponivel() || 
                !Config.Instancia.AutomacaoDesktop.SimulacaoInput.HabilitarMouse)
            {
                return false;
            }

            LoggingTask.RegistrarInfo($"🖱️ Clicando na coordenada ({x}, {y})");

            if (!cliqueDirecto && Config.Instancia.AutomacaoDesktop.ConfiguracaoMouse.SimularMovimentoNatural)
            {
                // Mover mouse de forma natural antes do clique
                await MoverMouseNatural(x, y);
            }
            else
            {
                // Mover direto para a posição
                _inputSimulator.Mouse.MoveMouseTo(x * 65535.0 / Screen.PrimaryScreen.Bounds.Width, 
                                                  y * 65535.0 / Screen.PrimaryScreen.Bounds.Height);
            }

            await Task.Delay(Config.Instancia.ObterDelayComVariacao(200));

            // Executar clique
            _inputSimulator.Mouse.LeftButtonClick();
            
            await Task.Delay(Config.Instancia.ObterDelayComVariacao(
                Config.Instancia.AutomacaoDesktop.SimulacaoInput.DelayEntreCliques));

            LoggingTask.RegistrarInfo("✅ Clique executado com sucesso");
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro($"Erro ao clicar coordenada ({x}, {y})", ex);
            return false;
        }
    }

    /// <summary>
    /// Move mouse de forma natural (curva suave)
    /// </summary>
    private static async Task MoverMouseNatural(int targetX, int targetY)
    {
        try
        {
            var posicaoAtual = Control.MousePosition;
            var distanciaX = targetX - posicaoAtual.X;
            var distanciaY = targetY - posicaoAtual.Y;
            
            var passos = Math.Max(10, (int)(Math.Sqrt(distanciaX * distanciaX + distanciaY * distanciaY) / 20));
            var velocidade = Config.Instancia.AutomacaoDesktop.ConfiguracaoMouse.VelocidadeMovimento;

            for (int i = 0; i <= passos; i++)
            {
                var progresso = (double)i / passos;
                
                // Curva suave (ease-out)
                var progressoSuave = 1 - Math.Pow(1 - progresso, 3);
                
                var x = posicaoAtual.X + (int)(distanciaX * progressoSuave);
                var y = posicaoAtual.Y + (int)(distanciaY * progressoSuave);
                
                _inputSimulator.Mouse.MoveMouseTo(x * 65535.0 / Screen.PrimaryScreen.Bounds.Width,
                                                  y * 65535.0 / Screen.PrimaryScreen.Bounds.Height);
                
                await Task.Delay(Math.Max(1, 100 / velocidade));
            }
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso($"Aviso no movimento natural do mouse: {ex.Message}");
        }
    }

    /// <summary>
    /// Simula combinação de teclas (Ctrl+C, Ctrl+V, etc.)
    /// </summary>
    public static async Task<bool> SimularAtalhoTeclado(string atalho)
    {
        try
        {
            if (!Config.Instancia.InputSimulatorDisponivel())
            {
                return false;
            }

            LoggingTask.RegistrarInfo($"⌨️ Simulando atalho: {atalho}");

            var partes = atalho.Split('+');
            var teclaModificadora = partes.Length > 1 ? partes[0].Trim().ToUpper() : null;
            var teclaPrincipal = partes.Length > 1 ? partes[1].Trim().ToUpper() : partes[0].Trim().ToUpper();

            // Pressionar tecla modificadora
            if (!string.IsNullOrEmpty(teclaModificadora))
            {
                var keyCodeModificadora = ObterVirtualKeyCode(teclaModificadora);
                if (keyCodeModificadora.HasValue)
                {
                    _inputSimulator.Keyboard.KeyDown(keyCodeModificadora.Value);
                    await Task.Delay(Config.Instancia.ObterDelayComVariacao(50));
                }
            }

            // Pressionar tecla principal
            var keyCodePrincipal = ObterVirtualKeyCode(teclaPrincipal);
            if (keyCodePrincipal.HasValue)
            {
                _inputSimulator.Keyboard.KeyPress(keyCodePrincipal.Value);
                await Task.Delay(Config.Instancia.ObterDelayComVariacao(50));
            }

            // Soltar tecla modificadora
            if (!string.IsNullOrEmpty(teclaModificadora))
            {
                var keyCodeModificadora = ObterVirtualKeyCode(teclaModificadora);
                if (keyCodeModificadora.HasValue)
                {
                    _inputSimulator.Keyboard.KeyUp(keyCodeModificadora.Value);
                }
            }

            LoggingTask.RegistrarInfo($"✅ Atalho '{atalho}' simulado");
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro($"Erro ao simular atalho '{atalho}'", ex);
            return false;
        }
    }

    /// <summary>
    /// Converte string para VirtualKeyCode
    /// </summary>
    private static VirtualKeyCode? ObterVirtualKeyCode(string tecla)
    {
        return tecla.ToUpper() switch
        {
            "CTRL" => VirtualKeyCode.LCONTROL,
            "ALT" => VirtualKeyCode.LMENU,
            "SHIFT" => VirtualKeyCode.LSHIFT,
            "F5" => VirtualKeyCode.F5,
            "F12" => VirtualKeyCode.F12,
            "T" => VirtualKeyCode.VK_T,
            "W" => VirtualKeyCode.VK_W,
            "C" => VirtualKeyCode.VK_C,
            "V" => VirtualKeyCode.VK_V,
            "ENTER" => VirtualKeyCode.RETURN,
            "ESC" => VirtualKeyCode.ESCAPE,
            "TAB" => VirtualKeyCode.TAB,
            _ => null
        };
    }

    /// <summary>
    /// Execução de emergência quando outras automações falham
    /// </summary>
    public static async Task<bool> ExecutarAcaoEmergencia(string acao, object? parametros = null)
    {
        try
        {
            if (!Config.Instancia.AutomacaoDesktop.AcoesEmergencia.UsarQuandoPlaywrightFalha)
            {
                return false;
            }

            LoggingTask.RegistrarInfo($"🚨 Ação de emergência: {acao}");

            var sucesso = acao.ToLower() switch
            {
                "refresh" => await RefreshForcadoPagina(),
                "scroll" => await ScrollPaginaParaCarregarConteudo(),
                "devtools" => await AbrirDevTools(),
                "click" when parametros is Point ponto => await ClicarCoordenada(ponto.X, ponto.Y),
                _ => false
            };

            if (sucesso)
            {
                LoggingTask.RegistrarInfo($"✅ Ação de emergência '{acao}' executada com sucesso");
            }
            else
            {
                LoggingTask.RegistrarAviso($"⚠️ Ação de emergência '{acao}' falhou");
            }

            return sucesso;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro($"Erro na ação de emergência '{acao}'", ex);
            return false;
        }
    }
}
```

## Métodos Mais Usados

### Simulação de Teclas Básicas

```csharp
using WindowsInput;
using WindowsInput.Native;

var inputSimulator = new InputSimulator();

// Tecla simples
inputSimulator.Keyboard.KeyPress(VirtualKeyCode.F5);
await Task.Delay(1000);

// Combinação de teclas (Ctrl+F5)
inputSimulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.LCONTROL, VirtualKeyCode.F5);
LoggingTask.RegistrarInfo("🔄 Refresh forçado executado");
```

### Digitação Natural com Delays

```csharp
// Digitação caractere por caractere
string texto = "https://www.adrenaline.com.br";
foreach (char c in texto)
{
    inputSimulator.Keyboard.TextEntry(c);
    await Task.Delay(Config.Instancia.ObterDelayComVariacao(80)); // Velocidade humana
}

// Ou usar método direto
inputSimulator.Keyboard.TextEntry(texto);
LoggingTask.RegistrarInfo("⌨️ Texto digitado naturalmente");
```

### Simulação de Mouse e Cliques

```csharp
// Mover mouse para coordenada
var x = 500;
var y = 300;
inputSimulator.Mouse.MoveMouseTo(x * 65535.0 / Screen.PrimaryScreen.Bounds.Width,
                                 y * 65535.0 / Screen.PrimaryScreen.Bounds.Height);

// Clique simples
inputSimulator.Mouse.LeftButtonClick();

// Clique duplo
inputSimulator.Mouse.LeftButtonDoubleClick();

// Scroll vertical
inputSimulator.Mouse.VerticalScroll(-3); // Scroll para baixo
LoggingTask.RegistrarInfo("🖱️ Interações de mouse executadas");
```

### Scroll Automático para Carregar Conteúdo

```csharp
// Scroll graduall para carregar conteúdo AJAX
for (int i = 0; i < 5; i++)
{
    inputSimulator.Mouse.VerticalScroll(-2);
    await Task.Delay(Config.Instancia.ObterDelayComVariacao(1000));
    
    // Simular pausa de leitura
    if (i % 2 == 0)
    {
        await Task.Delay(Random.Shared.Next(300, 800));
    }
}

LoggingTask.RegistrarInfo("📜 Scroll automático para carregar conteúdo dinâmico");
```

### Integração como Fallback Multi-nível

```csharp
// No NavigationTask.cs - cascata de fallbacks
public async Task<bool> InteragirComElemento(string seletor)
{
    try
    {
        // 1º: Tentar Playwright
        var sucessoPlaywright = await TentarPlaywright(seletor);
        if (sucessoPlaywright) return true;

        LoggingTask.RegistrarAviso("🌐 Playwright falhou, tentando FlaUI");

        // 2º: Tentar FlaUI
        if (Config.Instancia.FlaUIDisponivel())
        {
            var sucessoFlaUI = await TentarFlaUI(seletor);
            if (sucessoFlaUI) return true;
        }

        LoggingTask.RegistrarAviso("🖥️ FlaUI falhou, usando InputSimulator emergencial");

        // 3º: InputSimulator como último recurso
        if (Config.Instancia.InputSimulatorDisponivel())
        {
            // Tentar atalhos de teclado genéricos
            await InputSimulationTask.SimularAtalhoTeclado("F5"); // Refresh
            await Task.Delay(3000);
            
            return true; // Assumir que refresh ajudou
        }

        LoggingTask.RegistrarErro("❌ Todos os métodos de automação falharam");
        return false;
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro("Erro na interação multi-nível", ex);
        return false;
    }
}
```

### Simulação de Comportamento Humano

```csharp
// Delays com variação natural
public static async Task SimularLeituraHumana(int tempoBaseMs = 2000)
{
    var random = new Random();
    
    // Simular movimento de olhos (pequenos scrolls)
    for (int i = 0; i < 3; i++)
    {
        inputSimulator.Mouse.VerticalScroll(random.Next(-1, 2));
        await Task.Delay(random.Next(200, 500));
    }
    
    // Pausa de "leitura"
    var tempoLeitura = Config.Instancia.ObterDelayComVariacao(tempoBaseMs);
    await Task.Delay(tempoLeitura);
    
    LoggingTask.RegistrarInfo("👤 Comportamento humano simulado");
}
```

### Ações de Emergência Configuráveis

```csharp
// Sistema de recuperação quando automação trava
public static async Task<bool> RecuperarAutomacao()
{
    try
    {
        LoggingTask.RegistrarInfo("🚨 Iniciando recuperação de automação");

        // 1. Tentar refresh
        await InputSimulationTask.RefreshForcadoPagina();
        await Task.Delay(3000);

        // 2. Abrir DevTools para debug
        await InputSimulationTask.AbrirDevTools();
        await Task.Delay(1000);

        // 3. Fechar DevTools
        await InputSimulationTask.SimularAtalhoTeclado("F12");
        await Task.Delay(1000);

        // 4. Scroll para garantir conteúdo carregado
        await InputSimulationTask.ScrollPaginaParaCarregarConteudo(3);

        LoggingTask.RegistrarInfo("✅ Recuperação de automação concluída");
        return true;
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro("Falha na recuperação de automação", ex);
        return false;
    }
}
```
