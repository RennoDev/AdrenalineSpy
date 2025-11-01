# FlaUI - Automação Desktop Windows

## Índice
1. [Introdução](#introdução)
2. [Instalação](#instalação)
3. [Conceitos Básicos](#conceitos-básicos)
4. [Primeiros Passos](#primeiros-passos)
5. [Encontrar Elementos](#encontrar-elementos)
6. [Interações](#interações)
7. [Propriedades](#propriedades)
8. [Padrões de Controle](#padrões-de-controle)
9. [Exemplos Práticos](#exemplos-práticos)
10. [Boas Práticas](#boas-práticas)

---

## Introdução

**FlaUI** é uma biblioteca .NET para automação de aplicações desktop Windows, baseada na tecnologia **UI Automation** da Microsoft.

### Vantagens
- ✅ Suporta WPF, WinForms, Win32, UWP
- ✅ Acesso a elementos nativos da interface
- ✅ Não requer modificação da aplicação
- ✅ API fluente e intuitiva
- ✅ Inspeção de árvore de elementos
- ✅ Captura de screenshots

### Tecnologias Suportadas
- **UIA3**: Windows 10/11 (recomendado)
- **UIA2**: Windows 7/8
- **Mixed**: Combinação de ambos

---

## Instalação

```bash
# Pacote principal (UIA3 para Windows 10/11)
dotnet add package FlaUI.UIA3

# Ou UIA2 para Windows 7/8
dotnet add package FlaUI.UIA2

# Core (geralmente incluído automaticamente)
dotnet add package FlaUI.Core
```

---

## Conceitos Básicos

### Hierarquia de Objetos

```
Application
  └── MainWindow
       └── ControlType (Button, TextBox, etc)
            └── Patterns (InvokePattern, ValuePattern, etc)
```

### Automation Types

- **UIA3**: Mais moderno, melhor performance
- **UIA2**: Compatibilidade com sistemas antigos

### Control Types

Tipos de controles Windows:
- `Button`, `TextBox`, `ComboBox`
- `ListBox`, `TreeView`, `DataGrid`
- `Menu`, `MenuItem`, `Tab`
- `Window`, `Pane`, `Group`
- E muito mais...

---

## Primeiros Passos

### Exemplo Básico - Calculadora do Windows

```csharp
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        // Iniciar aplicação
        var app = Application.Launch("calc.exe");
        
        // Usar UIA3
        using var automation = new UIA3Automation();
        
        // Obter janela principal
        var window = app.GetMainWindow(automation);
        Console.WriteLine($"Título: {window.Title}");
        
        // Encontrar botões por texto
        var btn1 = window.FindFirstDescendant(cf => cf.ByText("Um"));
        var btn2 = window.FindFirstDescendant(cf => cf.ByText("Dois"));
        var btnPlus = window.FindFirstDescendant(cf => cf.ByText("Mais"));
        var btnEquals = window.FindFirstDescendant(cf => cf.ByText("Igual a"));
        
        // Executar operação: 1 + 2 =
        btn1?.Click();
        btnPlus?.Click();
        btn2?.Click();
        btnEquals?.Click();
        
        // Ler resultado
        var resultado = window.FindFirstDescendant(cf => cf.ByAutomationId("CalculatorResults"));
        Console.WriteLine($"Resultado: {resultado?.Name}");
        
        // Fechar aplicação
        app.Close();
    }
}
```

### Anexar a Aplicação em Execução

```csharp
// Por nome do processo
var processes = Process.GetProcessesByName("notepad");
if (processes.Length > 0)
{
    var app = Application.Attach(processes[0]);
    // ... trabalhar com a aplicação
}

// Por PID
var app = Application.Attach(12345);

// Por título da janela
using var automation = new UIA3Automation();
var window = automation.GetDesktop().FindFirstChild(cf => cf.ByName("Sem título - Bloco de Notas"));
```

---

## Encontrar Elementos

### Por Condições (Conditions)

```csharp
using FlaUI.Core.Definitions;

// Por AutomationId (melhor prática)
var element = window.FindFirstDescendant(cf => cf.ByAutomationId("txtUsername"));

// Por Name/Texto
var element = window.FindFirstDescendant(cf => cf.ByName("Salvar"));
var element = window.FindFirstDescendant(cf => cf.ByText("Cancelar"));

// Por ControlType
var button = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button));
var textbox = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit));

// Por ClassName
var element = window.FindFirstDescendant(cf => cf.ByClassName("TextBox"));

// Por múltiplas condições (AND)
var element = window.FindFirstDescendant(cf => 
    cf.ByControlType(ControlType.Button)
      .And(cf.ByName("OK")));

// Múltiplas condições (OR)
var element = window.FindFirstDescendant(cf =>
    cf.ByText("Salvar")
      .Or(cf.ByText("Save")));
```

### FindAll vs FindFirst

```csharp
// Encontrar o primeiro
var firstButton = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Button));

// Encontrar todos
var allButtons = window.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));
foreach (var button in allButtons)
{
    Console.WriteLine($"Botão: {button.Name}");
}
```

### Busca em Profundidade

```csharp
// Buscar apenas filhos diretos
var child = window.FindFirstChild(cf => cf.ByName("Panel"));

// Buscar em toda a árvore (descendentes)
var descendant = window.FindFirstDescendant(cf => cf.ByName("Botão"));

// Limitar níveis de busca
var element = window.FindFirstDescendant(cf => 
    cf.ByName("Elemento"), 
    maxDepth: 5);
```

### XPath (experimental)

```csharp
var element = window.FindFirstByXPath("//Button[@Name='OK']");
var elements = window.FindAllByXPath("//Button");
```

---

## Interações

### Cliques

```csharp
// Clique simples
button.Click();

// Clique no centro
button.Click(moveMouse: true);

// Duplo clique
button.DoubleClick();

// Clique com botão direito
button.RightClick();

// Clique em coordenada específica
var point = button.GetClickablePoint();
Mouse.Click(point);
```

### Preencher Texto

```csharp
// Usar ValuePattern (melhor)
var textbox = window.FindFirstDescendant(cf => cf.ByAutomationId("txtEmail"))
    .AsTextBox();
textbox.Enter("email@example.com");

// Ou diretamente
var element = window.FindFirstDescendant(cf => cf.ByAutomationId("txtNome"));
element.AsTextBox().Text = "João Silva";

// Limpar e preencher
textbox.Text = "";
textbox.Text = "Novo texto";
```

### Teclado

```csharp
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;

// Digitar texto
Keyboard.Type("Olá, mundo!");

// Pressionar teclas especiais
Keyboard.Press(VirtualKeyShort.ENTER);
Keyboard.Press(VirtualKeyShort.TAB);
Keyboard.Press(VirtualKeyShort.ESCAPE);

// Combinações
Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_A); // Ctrl+A
Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_C); // Ctrl+C

// Pressionar e segurar
Keyboard.Down(VirtualKeyShort.SHIFT);
// ... outras ações
Keyboard.Up(VirtualKeyShort.SHIFT);
```

### Mouse

```csharp
using FlaUI.Core.Input;

// Mover para elemento
Mouse.MoveTo(element.GetClickablePoint());

// Mover para coordenada
Mouse.MoveTo(x: 500, y: 300);

// Cliques
Mouse.LeftClick();
Mouse.RightClick();
Mouse.DoubleClick();

// Drag and Drop
Mouse.DragHorizontally(MouseButton.Left, startPoint, distance: 100);
Mouse.Drag(MouseButton.Left, startPoint, endPoint);

// Scroll
Mouse.Scroll(-120); // Scroll down
Mouse.Scroll(120);  // Scroll up
```

### ComboBox / Dropdown

```csharp
var comboBox = window.FindFirstDescendant(cf => cf.ByAutomationId("cmbEstado"))
    .AsComboBox();

// Expandir
comboBox.Expand();

// Selecionar por índice
comboBox.Select(2);

// Selecionar por texto
var items = comboBox.Items;
var item = items.First(i => i.Text == "São Paulo");
item.Select();

// Colapsar
comboBox.Collapse();

// Valor selecionado
var selected = comboBox.SelectedItem;
Console.WriteLine($"Selecionado: {selected.Text}");
```

### Checkbox e Radio Button

```csharp
var checkbox = window.FindFirstDescendant(cf => cf.ByAutomationId("chkAceito"))
    .AsCheckBox();

// Marcar
checkbox.IsChecked = true;

// Desmarcar
checkbox.IsChecked = false;

// Verificar estado
if (checkbox.IsChecked)
{
    Console.WriteLine("Checkbox marcado");
}

// Radio button (similar)
var radio = window.FindFirstDescendant(cf => cf.ByName("Masculino"))
    .AsRadioButton();
radio.IsChecked = true;
```

### DataGrid / Table

```csharp
var grid = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.DataGrid))
    .AsGrid();

// Obter linhas
var rows = grid.Rows;
Console.WriteLine($"Total de linhas: {rows.Length}");

// Iterar por linhas
foreach (var row in rows)
{
    var cells = row.Cells;
    foreach (var cell in cells)
    {
        Console.Write($"{cell.Value} | ");
    }
    Console.WriteLine();
}

// Selecionar linha
rows[0].Select();

// Obter célula específica
var cell = grid.Rows[2].Cells[1];
Console.WriteLine($"Valor: {cell.Value}");
```

### TreeView

```csharp
var tree = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Tree))
    .AsTree();

// Obter itens raiz
var rootItems = tree.Items;

// Expandir item
rootItems[0].Expand();

// Colapsar
rootItems[0].Collapse();

// Selecionar
rootItems[0].Select();

// Navegar hierarquia
var subItems = rootItems[0].Items;
subItems[2].Select();
```

### Menu

```csharp
// Encontrar menu
var menuBar = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.MenuBar));

// Menu item por texto
var fileMenu = menuBar.FindFirstDescendant(cf => cf.ByText("Arquivo"))
    .AsMenuItem();
fileMenu.Invoke();

// Submenu
var openItem = window.FindFirstDescendant(cf => cf.ByText("Abrir"))
    .AsMenuItem();
openItem.Invoke();

// Ou navegar por hierarquia
var menu = window.FindFirstDescendant(cf => cf.ByControlType(ControlType.Menu));
var items = menu.FindAllChildren(cf => cf.ByControlType(ControlType.MenuItem));
items[1].Click(); // Segundo item
```

---

## Propriedades

### Propriedades Comuns

```csharp
// Nome
string name = element.Name;

// AutomationId
string id = element.AutomationId;

// ControlType
var type = element.ControlType;

// ClassName
string className = element.ClassName;

// Visível
bool isVisible = element.IsEnabled;

// Habilitado
bool isEnabled = element.IsEnabled;

// BoundingRectangle
var bounds = element.BoundingRectangle;
Console.WriteLine($"X: {bounds.X}, Y: {bounds.Y}, Width: {bounds.Width}, Height: {bounds.Height}");

// Pai
var parent = element.Parent;

// Filhos
var children = element.FindAllChildren();
```

### Propriedades Específicas

```csharp
// TextBox
var textBox = element.AsTextBox();
string text = textBox.Text;
bool isReadOnly = textBox.IsReadOnly;

// Button
var button = element.AsButton();
string buttonText = button.Text;

// CheckBox
var checkbox = element.AsCheckBox();
bool isChecked = checkbox.IsChecked;

// Window
var window = element.AsWindow();
bool isModal = window.IsModal;
WindowVisualState state = window.WindowVisualState; // Normal, Maximized, Minimized
```

---

## Padrões de Controle

UI Automation usa **Control Patterns** para interagir com elementos.

### InvokePattern (Botões)

```csharp
if (element.Patterns.Invoke.IsSupported)
{
    element.Patterns.Invoke.Pattern.Invoke();
}
```

### ValuePattern (TextBox, Slider)

```csharp
if (element.Patterns.Value.IsSupported)
{
    var pattern = element.Patterns.Value.Pattern;
    pattern.SetValue("Novo valor");
    string value = pattern.Value;
}
```

### SelectionItemPattern (ComboBox, List)

```csharp
if (element.Patterns.SelectionItem.IsSupported)
{
    var pattern = element.Patterns.SelectionItem.Pattern;
    pattern.Select();
    bool isSelected = pattern.IsSelected;
}
```

### TogglePattern (CheckBox)

```csharp
if (element.Patterns.Toggle.IsSupported)
{
    var pattern = element.Patterns.Toggle.Pattern;
    pattern.Toggle();
    ToggleState state = pattern.ToggleState; // On, Off, Indeterminate
}
```

### WindowPattern

```csharp
if (window.Patterns.Window.IsSupported)
{
    var pattern = window.Patterns.Window.Pattern;
    
    pattern.SetWindowVisualState(WindowVisualState.Maximized);
    pattern.Close();
}
```

---

## Exemplos Práticos

### Exemplo 1: Automatizar Bloco de Notas

```csharp
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.UIA3;

class Program
{
    static void Main()
    {
        // Iniciar Notepad
        var app = Application.Launch("notepad.exe");
        using var automation = new UIA3Automation();
        
        var window = app.GetMainWindow(automation);
        window.Focus();
        
        // Digitar texto
        Keyboard.Type("Olá do FlaUI!");
        Keyboard.Press(VirtualKeyShort.ENTER);
        Keyboard.Type("Esta é uma automação desktop.");
        
        // Salvar arquivo (Ctrl+S)
        Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_S);
        
        // Aguardar janela de salvar
        Thread.Sleep(1000);
        var saveDialog = window.ModalWindows[0];
        
        // Preencher nome do arquivo
        var fileNameBox = saveDialog.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.Edit).And(cf.ByName("Nome do arquivo:")));
        fileNameBox.AsTextBox().Text = "teste_flaui.txt";
        
        // Clicar em Salvar
        var saveButton = saveDialog.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.Button).And(cf.ByName("Salvar")));
        saveButton.Click();
        
        // Fechar
        Thread.Sleep(1000);
        app.Close();
        
        Console.WriteLine("Automação concluída!");
    }
}
```

### Exemplo 2: Extrair Dados de Aplicação

```csharp
class DataExtractor
{
    static void ExtrairDadosGrid()
    {
        using var automation = new UIA3Automation();
        
        // Anexar a aplicação em execução
        var window = automation.GetDesktop()
            .FindFirstChild(cf => cf.ByName("Minha Aplicação"));
        
        if (window == null)
        {
            Console.WriteLine("Aplicação não encontrada!");
            return;
        }
        
        // Encontrar DataGrid
        var grid = window.FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.DataGrid)).AsGrid();
        
        // Extrair dados
        var dados = new List<Dictionary<string, string>>();
        
        foreach (var row in grid.Rows)
        {
            var linha = new Dictionary<string, string>();
            var cells = row.Cells;
            
            for (int i = 0; i < cells.Length; i++)
            {
                linha[$"Coluna{i}"] = cells[i].Value;
            }
            
            dados.Add(linha);
        }
        
        // Exibir
        foreach (var item in dados)
        {
            Console.WriteLine(string.Join(" | ", item.Values));
        }
    }
}
```

### Exemplo 3: Preencher Formulário Complexo

```csharp
class FormFiller
{
    static void PreencherCadastro()
    {
        using var automation = new UIA3Automation();
        var window = automation.GetDesktop()
            .FindFirstChild(cf => cf.ByName("Cadastro de Cliente"));
        
        // TextBoxes
        var txtNome = window.FindFirstDescendant(cf => 
            cf.ByAutomationId("txtNome")).AsTextBox();
        txtNome.Text = "João Silva";
        
        var txtEmail = window.FindFirstDescendant(cf => 
            cf.ByAutomationId("txtEmail")).AsTextBox();
        txtEmail.Text = "joao@email.com";
        
        // ComboBox
        var cmbEstado = window.FindFirstDescendant(cf => 
            cf.ByAutomationId("cmbEstado")).AsComboBox();
        cmbEstado.Select("SP");
        
        // CheckBox
        var chkAtivo = window.FindFirstDescendant(cf => 
            cf.ByAutomationId("chkAtivo")).AsCheckBox();
        chkAtivo.IsChecked = true;
        
        // Radio Button
        var rdMasculino = window.FindFirstDescendant(cf => 
            cf.ByAutomationId("rdMasculino")).AsRadioButton();
        rdMasculino.Select();
        
        // DatePicker (como TextBox)
        var dtNascimento = window.FindFirstDescendant(cf => 
            cf.ByAutomationId("dtNascimento")).AsTextBox();
        dtNascimento.Text = "01/01/1990";
        
        // Botão Salvar
        var btnSalvar = window.FindFirstDescendant(cf => 
            cf.ByAutomationId("btnSalvar"));
        btnSalvar.Click();
        
        Console.WriteLine("Formulário preenchido!");
    }
}
```

### Exemplo 4: Esperar Elemento

```csharp
static AutomationElement WaitForElement(
    AutomationElement parent, 
    Func<ConditionFactory, ConditionBase> condition,
    TimeSpan timeout)
{
    var endTime = DateTime.Now + timeout;
    
    while (DateTime.Now < endTime)
    {
        var element = parent.FindFirstDescendant(condition);
        if (element != null)
            return element;
        
        Thread.Sleep(500);
    }
    
    throw new TimeoutException("Elemento não encontrado");
}

// Uso
var button = WaitForElement(
    window, 
    cf => cf.ByName("Processar"),
    TimeSpan.FromSeconds(30)
);
```

---

## Boas Práticas

### 1. Use AutomationId Sempre que Possível

```csharp
// ✅ BOM - AutomationId é estável
var element = window.FindFirstDescendant(cf => cf.ByAutomationId("btnSalvar"));

// ❌ RUIM - Nome pode mudar (localização)
var element = window.FindFirstDescendant(cf => cf.ByName("Save"));
```

### 2. Verifique Existência Antes de Usar

```csharp
var element = window.FindFirstDescendant(cf => cf.ByAutomationId("btnOK"));

if (element != null && element.IsAvailable)
{
    element.Click();
}
else
{
    Console.WriteLine("Elemento não encontrado");
}
```

### 3. Use Padrões Quando Disponível

```csharp
// ✅ BOM - usa pattern
if (element.Patterns.Invoke.IsSupported)
{
    element.Patterns.Invoke.Pattern.Invoke();
}

// ❌ MENOS EFICIENTE
element.Click();
```

### 4. Libere Recursos

```csharp
using var automation = new UIA3Automation();
// ... código ...
// Liberado automaticamente
```

### 5. Adicione Esperas Apropriadas

```csharp
// Para janelas modais
Thread.Sleep(500);

// Para processamento
Wait.UntilInputIsProcessed();

// Para elementos
Retry.WhileException(() => element.Click(), TimeSpan.FromSeconds(5));
```

### 6. Capture Screenshots em Erros

```csharp
try
{
    element.Click();
}
catch (Exception ex)
{
    var screenshot = Capture.Screen();
    screenshot.ToFile("erro.png");
    throw;
}
```

### 7. Use Inspect.exe para Descobrir Elementos

O Windows SDK inclui `Inspect.exe` para explorar a árvore de elementos:
- Localizado em: `C:\Program Files (x86)\Windows Kits\10\bin\<version>\x64\inspect.exe`
- Mostra AutomationId, ControlType, Patterns disponíveis

---

## Ferramentas Auxiliares

### FlaUI Inspect

```bash
# Instalar globalmente
dotnet tool install -g FlaUI.Inspector

# Executar
flaui-inspect
```

### Capture (Screenshots)

```csharp
using FlaUI.Core.Capturing;

// Capturar tela inteira
var screenshot = Capture.Screen();
screenshot.ToFile("tela.png");

// Capturar elemento específico
var elementScreenshot = Capture.Element(element);
elementScreenshot.ToFile("elemento.png");

// Capturar janela
var windowScreenshot = Capture.Window(window);
```

---

## Troubleshooting

### Problema: "Element not found"

**Solução:**
1. Use `Inspect.exe` para verificar propriedades
2. Adicione esperas
3. Verifique se aplicação está em primeiro plano

### Problema: "Pattern not supported"

**Solução:**
```csharp
if (!element.Patterns.Value.IsSupported)
{
    // Usar alternativa (ex: teclado)
    element.Focus();
    Keyboard.TypeSimultaneously(VirtualKeyShort.CONTROL, VirtualKeyShort.KEY_A);
    Keyboard.Type("novo valor");
}
```

### Problema: Performance lenta

**Solução:**
- Use `FindFirstChild` em vez de `FindFirstDescendant` quando possível
- Cache elementos encontrados
- Limite profundidade de busca

---

## Recursos Adicionais

- **GitHub**: https://github.com/FlaUI/FlaUI
- **Documentação**: https://github.com/FlaUI/FlaUI/wiki
- **UI Automation**: https://docs.microsoft.com/windows/win32/winauto/entry-uiauto-win32

---

**Versão:** 1.0  
**Última atualização:** Novembro 2025
