# InputSimulatorStandard - Simulação de Teclado e Mouse

## Índice
1. [Introdução](#introdução)
2. [Instalação](#instalação)
3. [Conceitos Básicos](#conceitos-básicos)
4. [Simulação de Teclado](#simulação-de-teclado)
5. [Simulação de Mouse](#simulação-de-mouse)
6. [Exemplos Práticos](#exemplos-práticos)
7. [Boas Práticas](#boas-práticas)

---

## Introdução

**InputSimulatorStandard** é uma biblioteca .NET para simular entrada de teclado e mouse a nível de sistema operacional. É útil para:

- Automação de aplicações legadas sem API
- Jogos e aplicações que não suportam UI Automation
- Macros e atalhos personalizados
- Testes de interface

### Vantagens
- ✅ Simula entrada real do usuário
- ✅ Funciona com qualquer aplicação Windows
- ✅ API simples e intuitiva
- ✅ Suporta teclas modificadoras
- ✅ Controle preciso do mouse

---

## Instalação

```bash
dotnet add package InputSimulatorStandard
```

---

## Conceitos Básicos

### InputSimulator

Classe principal que fornece acesso aos simuladores:

```csharp
using WindowsInput;

var simulator = new InputSimulator();

// Simulador de teclado
var keyboard = simulator.Keyboard;

// Simulador de mouse
var mouse = simulator.Mouse;
```

### Virtual Key Codes

Códigos de teclas virtuais do Windows:
- `VirtualKeyCode.A` a `VirtualKeyCode.Z`
- `VirtualKeyCode.VK_0` a `VirtualKeyCode.VK_9`
- `VirtualKeyCode.RETURN`, `VirtualKeyCode.TAB`, `VirtualKeyCode.ESCAPE`
- `VirtualKeyCode.CONTROL`, `VirtualKeyCode.SHIFT`, `VirtualKeyCode.MENU` (Alt)
- E muitos outros...

---

## Simulação de Teclado

### Digitar Texto

```csharp
using WindowsInput;

var simulator = new InputSimulator();

// Digitar texto simples
simulator.Keyboard.TextEntry("Olá, mundo!");

// Digitar com delay entre teclas
foreach (char c in "Texto lento")
{
    simulator.Keyboard.TextEntry(c.ToString());
    Thread.Sleep(100);
}
```

### Pressionar Teclas

```csharp
// Pressionar uma tecla
simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN); // Enter

// Pressionar teclas especiais
simulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
simulator.Keyboard.KeyPress(VirtualKeyCode.ESCAPE);
simulator.Keyboard.KeyPress(VirtualKeyCode.DELETE);
simulator.Keyboard.KeyPress(VirtualKeyCode.BACK); // Backspace

// Pressionar teclas de função
simulator.Keyboard.KeyPress(VirtualKeyCode.F1);
simulator.Keyboard.KeyPress(VirtualKeyCode.F5);
```

### Teclas Modificadoras (Ctrl, Shift, Alt)

```csharp
// Ctrl+C (copiar)
simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);

// Ctrl+V (colar)
simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);

// Ctrl+A (selecionar tudo)
simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A);

// Ctrl+S (salvar)
simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S);

// Alt+F4 (fechar janela)
simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.MENU, VirtualKeyCode.F4);

// Shift+Tab
simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.SHIFT, VirtualKeyCode.TAB);
```

### Múltiplas Teclas Modificadoras

```csharp
// Ctrl+Shift+N
simulator.Keyboard.ModifiedKeyStroke(
    new[] { VirtualKeyCode.CONTROL, VirtualKeyCode.SHIFT },
    VirtualKeyCode.VK_N
);

// Ctrl+Alt+Delete
simulator.Keyboard.ModifiedKeyStroke(
    new[] { VirtualKeyCode.CONTROL, VirtualKeyCode.MENU },
    VirtualKeyCode.DELETE
);
```

### Pressionar e Segurar

```csharp
// Pressionar Shift
simulator.Keyboard.KeyDown(VirtualKeyCode.SHIFT);

// Digitar letras (serão maiúsculas)
simulator.Keyboard.KeyPress(VirtualKeyCode.VK_H);
simulator.Keyboard.KeyPress(VirtualKeyCode.VK_E);
simulator.Keyboard.KeyPress(VirtualKeyCode.VK_L);
simulator.Keyboard.KeyPress(VirtualKeyCode.VK_L);
simulator.Keyboard.KeyPress(VirtualKeyCode.VK_O);

// Soltar Shift
simulator.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
```

### Teclas do Numpad

```csharp
// Números do numpad
simulator.Keyboard.KeyPress(VirtualKeyCode.NUMPAD0);
simulator.Keyboard.KeyPress(VirtualKeyCode.NUMPAD1);
simulator.Keyboard.KeyPress(VirtualKeyCode.NUMPAD5);

// Operadores do numpad
simulator.Keyboard.KeyPress(VirtualKeyCode.ADD);      // +
simulator.Keyboard.KeyPress(VirtualKeyCode.SUBTRACT); // -
simulator.Keyboard.KeyPress(VirtualKeyCode.MULTIPLY); // *
simulator.Keyboard.KeyPress(VirtualKeyCode.DIVIDE);   // /
```

### Teclas de Navegação

```csharp
// Setas
simulator.Keyboard.KeyPress(VirtualKeyCode.UP);
simulator.Keyboard.KeyPress(VirtualKeyCode.DOWN);
simulator.Keyboard.KeyPress(VirtualKeyCode.LEFT);
simulator.Keyboard.KeyPress(VirtualKeyCode.RIGHT);

// Home, End
simulator.Keyboard.KeyPress(VirtualKeyCode.HOME);
simulator.Keyboard.KeyPress(VirtualKeyCode.END);

// Page Up, Page Down
simulator.Keyboard.KeyPress(VirtualKeyCode.PRIOR); // Page Up
simulator.Keyboard.KeyPress(VirtualKeyCode.NEXT);  // Page Down
```

### Teclas Especiais

```csharp
// Windows Key
simulator.Keyboard.KeyPress(VirtualKeyCode.LWIN);

// Print Screen
simulator.Keyboard.KeyPress(VirtualKeyCode.SNAPSHOT);

// Pause/Break
simulator.Keyboard.KeyPress(VirtualKeyCode.PAUSE);

// Insert
simulator.Keyboard.KeyPress(VirtualKeyCode.INSERT);

// Caps Lock, Num Lock, Scroll Lock
simulator.Keyboard.KeyPress(VirtualKeyCode.CAPITAL);
simulator.Keyboard.KeyPress(VirtualKeyCode.NUMLOCK);
simulator.Keyboard.KeyPress(VirtualKeyCode.SCROLL);
```

---

## Simulação de Mouse

### Movimentação

```csharp
// Mover para coordenadas absolutas (0-65535)
simulator.Mouse.MoveMouseTo(32768, 32768); // Centro da tela

// Mover relativo (pixels)
simulator.Mouse.MoveMouseBy(100, 50); // 100px direita, 50px baixo

// Mover para coordenadas de tela
// Converter de pixels para coordenadas absolutas
int screenX = 800;
int screenY = 600;
double absoluteX = (screenX * 65535) / Screen.PrimaryScreen.Bounds.Width;
double absoluteY = (screenY * 65535) / Screen.PrimaryScreen.Bounds.Height;
simulator.Mouse.MoveMouseTo(absoluteX, absoluteY);
```

### Cliques

```csharp
// Clique esquerdo
simulator.Mouse.LeftButtonClick();

// Duplo clique esquerdo
simulator.Mouse.LeftButtonDoubleClick();

// Clique direito
simulator.Mouse.RightButtonClick();

// Clique do meio
simulator.Mouse.MiddleButtonClick();
```

### Pressionar e Soltar Botões

```csharp
// Pressionar botão esquerdo
simulator.Mouse.LeftButtonDown();
Thread.Sleep(1000); // Segurar por 1 segundo
simulator.Mouse.LeftButtonUp();

// Botão direito
simulator.Mouse.RightButtonDown();
simulator.Mouse.RightButtonUp();

// Botão do meio
simulator.Mouse.MiddleButtonDown();
simulator.Mouse.MiddleButtonUp();
```

### Drag and Drop

```csharp
// Mover para origem
simulator.Mouse.MoveMouseTo(10000, 10000);
simulator.Mouse.LeftButtonDown();

// Mover para destino (arrastando)
Thread.Sleep(100);
simulator.Mouse.MoveMouseTo(50000, 50000);

// Soltar
simulator.Mouse.LeftButtonUp();
```

### Scroll do Mouse

```csharp
// Scroll vertical
simulator.Mouse.VerticalScroll(-10); // Scroll down (negativo)
simulator.Mouse.VerticalScroll(10);  // Scroll up (positivo)

// Scroll horizontal
simulator.Mouse.HorizontalScroll(10);  // Scroll right
simulator.Mouse.HorizontalScroll(-10); // Scroll left
```

### Botões Extras (X1, X2)

```csharp
// Botões laterais do mouse
simulator.Mouse.XButtonClick(1); // Botão X1 (geralmente "voltar")
simulator.Mouse.XButtonClick(2); // Botão X2 (geralmente "avançar")

simulator.Mouse.XButtonDown(1);
simulator.Mouse.XButtonUp(1);
```

---

## Exemplos Práticos

### Exemplo 1: Preencher Formulário Web

```csharp
using WindowsInput;

class WebFormFiller
{
    static void PreencherFormulario()
    {
        var simulator = new InputSimulator();
        
        // Focar navegador (Alt+Tab se necessário)
        Thread.Sleep(1000); // Tempo para focar manualmente
        
        // Nome
        simulator.Keyboard.TextEntry("João Silva");
        simulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
        
        // Email
        simulator.Keyboard.TextEntry("joao@email.com");
        simulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
        
        // Telefone
        simulator.Keyboard.TextEntry("(11) 98765-4321");
        simulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
        
        // Mensagem
        simulator.Keyboard.TextEntry("Esta é uma mensagem de teste.");
        
        // Submit (Enter ou Tab até botão)
        simulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
        simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
        
        Console.WriteLine("Formulário preenchido!");
    }
}
```

### Exemplo 2: Atalhos de Teclado

```csharp
class KeyboardShortcuts
{
    static void ExecutarAtalhos()
    {
        var simulator = new InputSimulator();
        
        // Abrir menu Arquivo no Notepad
        simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.MENU, VirtualKeyCode.VK_F);
        Thread.Sleep(200);
        
        // Novo arquivo (N)
        simulator.Keyboard.KeyPress(VirtualKeyCode.VK_N);
        Thread.Sleep(500);
        
        // Digitar texto
        simulator.Keyboard.TextEntry("Texto criado por automação");
        
        // Salvar (Ctrl+S)
        simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S);
        Thread.Sleep(500);
        
        // Digitar nome do arquivo
        simulator.Keyboard.TextEntry("arquivo_automatico.txt");
        
        // Enter para confirmar
        simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
    }
}
```

### Exemplo 3: Automação de Jogo Simples

```csharp
class GameAutomation
{
    static void AutomarJogo()
    {
        var simulator = new InputSimulator();
        
        Console.WriteLine("Iniciando automação em 3 segundos...");
        Console.WriteLine("Posicione a janela do jogo!");
        Thread.Sleep(3000);
        
        // Loop de ações
        for (int i = 0; i < 10; i++)
        {
            // Mover para frente
            simulator.Keyboard.KeyDown(VirtualKeyCode.VK_W);
            Thread.Sleep(500);
            simulator.Keyboard.KeyUp(VirtualKeyCode.VK_W);
            
            // Atacar (espaço)
            simulator.Keyboard.KeyPress(VirtualKeyCode.SPACE);
            Thread.Sleep(200);
            
            // Mover para direita
            simulator.Keyboard.KeyDown(VirtualKeyCode.VK_D);
            Thread.Sleep(300);
            simulator.Keyboard.KeyUp(VirtualKeyCode.VK_D);
            
            Thread.Sleep(1000);
        }
        
        Console.WriteLine("Automação concluída!");
    }
}
```

### Exemplo 4: Clique em Posição Específica

```csharp
class ClickAutomation
{
    static void ClicarNaPosicao(int x, int y)
    {
        var simulator = new InputSimulator();
        
        // Converter coordenadas de tela para absolutas
        int screenWidth = Screen.PrimaryScreen.Bounds.Width;
        int screenHeight = Screen.PrimaryScreen.Bounds.Height;
        
        double absoluteX = (x * 65535.0) / screenWidth;
        double absoluteY = (y * 65535.0) / screenHeight;
        
        // Mover e clicar
        simulator.Mouse.MoveMouseTo(absoluteX, absoluteY);
        Thread.Sleep(100);
        simulator.Mouse.LeftButtonClick();
    }
    
    static void Main()
    {
        Console.WriteLine("Clicando em (500, 300) em 2 segundos...");
        Thread.Sleep(2000);
        
        ClicarNaPosicao(500, 300);
        
        Console.WriteLine("Clique executado!");
    }
}
```

### Exemplo 5: Macro Complexa

```csharp
class MacroAutomation
{
    static void ExecutarMacro()
    {
        var simulator = new InputSimulator();
        
        // Selecionar tudo
        simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A);
        Thread.Sleep(100);
        
        // Copiar
        simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
        Thread.Sleep(100);
        
        // Abrir novo documento (Ctrl+N)
        simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_N);
        Thread.Sleep(500);
        
        // Colar
        simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
        Thread.Sleep(100);
        
        // Adicionar texto
        simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
        simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
        simulator.Keyboard.TextEntry("-- Processado automaticamente --");
        
        // Salvar
        simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S);
    }
}
```

### Exemplo 6: Captura de Tela Automática

```csharp
class ScreenshotAutomation
{
    static void TirarScreenshot(string nomeArquivo)
    {
        var simulator = new InputSimulator();
        
        // Print Screen
        simulator.Keyboard.KeyPress(VirtualKeyCode.SNAPSHOT);
        Thread.Sleep(500);
        
        // Abrir Paint (Win+R, mspaint)
        simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_R);
        Thread.Sleep(500);
        
        simulator.Keyboard.TextEntry("mspaint");
        simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
        Thread.Sleep(2000);
        
        // Colar (Ctrl+V)
        simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
        Thread.Sleep(500);
        
        // Salvar (Ctrl+S)
        simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S);
        Thread.Sleep(1000);
        
        // Nome do arquivo
        simulator.Keyboard.TextEntry(nomeArquivo);
        simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
    }
}
```

---

## Boas Práticas

### 1. Adicione Delays Apropriados

```csharp
// ✅ BOM - dá tempo para UI responder
simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
Thread.Sleep(500);

// ❌ RUIM - pode falhar se UI for lenta
simulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
simulator.Keyboard.TextEntry("texto");
```

### 2. Verifique Foco da Janela

```csharp
// Use FlaUI ou Win32 API para garantir foco
[DllImport("user32.dll")]
static extern bool SetForegroundWindow(IntPtr hWnd);

IntPtr hwnd = ...; // Handle da janela
SetForegroundWindow(hwnd);
Thread.Sleep(500);

// Agora simular entrada
simulator.Keyboard.TextEntry("texto");
```

### 3. Use TextEntry para Texto

```csharp
// ✅ BOM - mais rápido e confiável
simulator.Keyboard.TextEntry("email@example.com");

// ❌ RUIM - caractere por caractere
simulator.Keyboard.KeyPress(VirtualKeyCode.VK_E);
simulator.Keyboard.KeyPress(VirtualKeyCode.VK_M);
// ...
```

### 4. Trate Caracteres Especiais

```csharp
// Para caracteres acentuados, use TextEntry
simulator.Keyboard.TextEntry("São Paulo");

// Para símbolos que requerem Shift
simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.SHIFT, VirtualKeyCode.VK_1); // !
```

### 5. Libere Teclas Pressionadas

```csharp
try
{
    simulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
    // ... operações ...
}
finally
{
    simulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
}
```

### 6. Coordenadas Absolutas do Mouse

```csharp
// Sempre converta coordenadas corretamente
double GetAbsoluteX(int x)
{
    return (x * 65535.0) / Screen.PrimaryScreen.Bounds.Width;
}

double GetAbsoluteY(int y)
{
    return (y * 65535.0) / Screen.PrimaryScreen.Bounds.Height;
}
```

### 7. Teste em Aplicações Seguras

```csharp
// ⚠️ AVISO: InputSimulator pode não funcionar em:
// - Aplicações com privilégios elevados (admin)
// - Jogos com anti-cheat
// - Aplicações com proteção de segurança

// Solução: Execute seu programa como administrador
```

---

## Limitações

### 1. Aplicações Elevadas
Se a aplicação alvo roda como administrador, seu programa também precisa.

### 2. Sistemas de Segurança
Alguns sistemas (jogos, bancos) bloqueiam entrada simulada.

### 3. Foco de Janela
A janela alvo deve estar em foco para receber entrada.

### 4. Layout de Teclado
InputSimulator usa o layout de teclado do sistema.

### 5. Resolução de Tela
Coordenadas do mouse são relativas à resolução atual.

---

## Alternativas

Para casos onde InputSimulator não funciona:

1. **SendKeys** (nativo .NET)
```csharp
System.Windows.Forms.SendKeys.SendWait("Hello");
```

2. **Win32 API** (SendInput)
```csharp
[DllImport("user32.dll")]
static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
```

3. **UI Automation** (FlaUI)
- Mais confiável para aplicações Windows modernas

---

## Recursos Adicionais

- **GitHub**: https://github.com/TChatzigiannakis/InputSimulatorStandard
- **Virtual Key Codes**: https://docs.microsoft.com/windows/win32/inputdev/virtual-key-codes

---

**Versão:** 1.0  
**Última atualização:** Novembro 2025
