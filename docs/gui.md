# Interfaces Gráficas para Automações RPA

## Índice
1. [WPF](#wpf)
2. [Windows Forms](#windows-forms)
3. [Avalonia UI](#avalonia-ui)
4. [Terminal.Gui](#terminalgui)
5. [Electron.NET](#electronnet)
6. [Comparação](#comparação)

---

## WPF

### Introdução

**Windows Presentation Foundation** (WPF) é o framework moderno da Microsoft para UIs desktop no Windows.

**Vantagens:**
- ✅ XAML para UI declarativa
- ✅ Data Binding poderoso
- ✅ Design moderno
- ✅ MVVM pattern
- ✅ Rico em recursos

**Limitações:**
- ❌ Apenas Windows

---

### Criar Projeto WPF

```bash
dotnet new wpf -o MeuRPA.UI
cd MeuRPA.UI
dotnet run
```

---

### Exemplo Simples

**MainWindow.xaml:**
```xml
<Window x:Class="MeuRPA.UI.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="RPA Control Panel" Height="450" Width="800">
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Título -->
        <TextBlock Grid.Row="0" Text="RPA Automation Dashboard" 
                   FontSize="24" FontWeight="Bold" Margin="0,0,0,20"/>
        
        <!-- Botões -->
        <StackPanel Grid.Row="1" Orientation="Horizontal" Margin="0,0,0,10">
            <Button x:Name="btnIniciar" Content="Iniciar RPA" 
                    Width="120" Height="40" Margin="0,0,10,0" 
                    Click="BtnIniciar_Click"/>
            <Button x:Name="btnParar" Content="Parar" 
                    Width="120" Height="40" Margin="0,0,10,0" 
                    Click="BtnParar_Click" IsEnabled="False"/>
            <Button Content="Limpar Log" 
                    Width="120" Height="40" 
                    Click="BtnLimpar_Click"/>
        </StackPanel>
        
        <!-- Log -->
        <ScrollViewer Grid.Row="2" Margin="0,10,0,10">
            <TextBox x:Name="txtLog" IsReadOnly="True" 
                     TextWrapping="Wrap" VerticalScrollBarVisibility="Auto"
                     FontFamily="Consolas" Background="#1e1e1e" Foreground="#d4d4d4"/>
        </ScrollViewer>
        
        <!-- Status -->
        <StatusBar Grid.Row="3">
            <StatusBarItem>
                <TextBlock x:Name="txtStatus" Text="Pronto"/>
            </StatusBarItem>
        </StatusBar>
    </Grid>
</Window>
```

**MainWindow.xaml.cs:**
```csharp
using System.Windows;
using System.Threading.Tasks;

namespace MeuRPA.UI
{
    public partial class MainWindow : Window
    {
        private bool _executando = false;
        
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private async void BtnIniciar_Click(object sender, RoutedEventArgs e)
        {
            btnIniciar.IsEnabled = false;
            btnParar.IsEnabled = true;
            _executando = true;
            txtStatus.Text = "Executando...";
            
            AdicionarLog("=== RPA Iniciado ===");
            
            await Task.Run(() => ExecutarRPA());
            
            AdicionarLog("=== RPA Finalizado ===");
            txtStatus.Text = "Pronto";
            btnIniciar.IsEnabled = true;
            btnParar.IsEnabled = false;
        }
        
        private void BtnParar_Click(object sender, RoutedEventArgs e)
        {
            _executando = false;
            AdicionarLog("Parando RPA...");
        }
        
        private void BtnLimpar_Click(object sender, RoutedEventArgs e)
        {
            txtLog.Clear();
        }
        
        private void ExecutarRPA()
        {
            for (int i = 1; i <= 10 && _executando; i++)
            {
                AdicionarLog($"[{DateTime.Now:HH:mm:ss}] Processando item {i}...");
                Thread.Sleep(1000);
            }
        }
        
        private void AdicionarLog(string mensagem)
        {
            Dispatcher.Invoke(() =>
            {
                txtLog.AppendText(mensagem + Environment.NewLine);
                txtLog.ScrollToEnd();
            });
        }
    }
}
```

---

## Windows Forms

### Introdução

**Windows Forms** é o framework clássico para UIs desktop no Windows.

**Vantagens:**
- ✅ Simples e direto
- ✅ Designer visual
- ✅ Muito estável
- ✅ Grande comunidade

---

### Criar Projeto Windows Forms

```bash
dotnet new winforms -o MeuRPA.WinForms
cd MeuRPA.WinForms
dotnet run
```

---

### Exemplo (via código)

```csharp
using System;
using System.Drawing;
using System.Windows.Forms;

public class RPAForm : Form
{
    private Button btnIniciar;
    private Button btnParar;
    private TextBox txtLog;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblStatus;
    
    public RPAForm()
    {
        Text = "RPA Control Panel";
        Size = new Size(800, 600);
        
        // Botão Iniciar
        btnIniciar = new Button
        {
            Text = "Iniciar RPA",
            Location = new Point(20, 20),
            Size = new Size(120, 40)
        };
        btnIniciar.Click += BtnIniciar_Click;
        
        // Botão Parar
        btnParar = new Button
        {
            Text = "Parar",
            Location = new Point(150, 20),
            Size = new Size(120, 40),
            Enabled = false
        };
        btnParar.Click += BtnParar_Click;
        
        // Log
        txtLog = new TextBox
        {
            Location = new Point(20, 70),
            Size = new Size(740, 450),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            ReadOnly = true,
            Font = new Font("Consolas", 10),
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.FromArgb(212, 212, 212)
        };
        
        // Status Bar
        statusStrip = new StatusStrip();
        lblStatus = new ToolStripStatusLabel("Pronto");
        statusStrip.Items.Add(lblStatus);
        
        Controls.Add(btnIniciar);
        Controls.Add(btnParar);
        Controls.Add(txtLog);
        Controls.Add(statusStrip);
    }
    
    private async void BtnIniciar_Click(object sender, EventArgs e)
    {
        btnIniciar.Enabled = false;
        btnParar.Enabled = true;
        lblStatus.Text = "Executando...";
        
        AdicionarLog("=== RPA Iniciado ===");
        
        await Task.Run(() =>
        {
            for (int i = 1; i <= 10; i++)
            {
                AdicionarLog($"[{DateTime.Now:HH:mm:ss}] Processando item {i}...");
                Thread.Sleep(1000);
            }
        });
        
        AdicionarLog("=== RPA Finalizado ===");
        lblStatus.Text = "Pronto";
        btnIniciar.Enabled = true;
        btnParar.Enabled = false;
    }
    
    private void BtnParar_Click(object sender, EventArgs e)
    {
        AdicionarLog("Parando...");
    }
    
    private void AdicionarLog(string mensagem)
    {
        if (txtLog.InvokeRequired)
        {
            txtLog.Invoke(new Action(() => AdicionarLog(mensagem)));
            return;
        }
        
        txtLog.AppendText(mensagem + Environment.NewLine);
    }
}

class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new RPAForm());
    }
}
```

---

## Avalonia UI

### Introdução

**Avalonia** é um framework multiplataforma para criar UIs modernas (Windows, Linux, macOS).

**Vantagens:**
- ✅ Multiplataforma
- ✅ Sintaxe similar a WPF
- ✅ Moderno e ativo
- ✅ MVVM

---

### Instalação

```bash
# Instalar template
dotnet new install Avalonia.Templates

# Criar projeto
dotnet new avalonia.app -o MeuRPA.Avalonia
cd MeuRPA.Avalonia
dotnet run
```

---

## Terminal.Gui

### Introdução

**Terminal.Gui** é um framework para criar UIs no terminal (console avançado).

**Vantagens:**
- ✅ Leve e rápido
- ✅ Multiplataforma
- ✅ Não requer GUI
- ✅ Perfeito para servidores

---

### Instalação

```bash
dotnet add package Terminal.Gui
```

---

### Exemplo

```csharp
using Terminal.Gui;

class Program
{
    static void Main()
    {
        Application.Init();
        
        var top = Application.Top;
        
        // Janela principal
        var win = new Window("RPA Control Panel")
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        
        // Botão Iniciar
        var btnIniciar = new Button("Iniciar RPA")
        {
            X = 2,
            Y = 2
        };
        btnIniciar.Clicked += () =>
        {
            MessageBox.Query("RPA", "Iniciando RPA...", "OK");
        };
        
        // Log (TextView)
        var txtLog = new TextView()
        {
            X = 2,
            Y = 5,
            Width = Dim.Fill() - 4,
            Height = Dim.Fill() - 2,
            ReadOnly = true
        };
        
        txtLog.Text = "=== Log do RPA ===\n";
        
        win.Add(btnIniciar, txtLog);
        top.Add(win);
        
        // Menu
        var menu = new MenuBar(new MenuBarItem[]
        {
            new MenuBarItem("_Arquivo", new MenuItem[]
            {
                new MenuItem("_Sair", "", () => Application.RequestStop())
            })
        });
        top.Add(menu);
        
        Application.Run();
        Application.Shutdown();
    }
}
```

---

## Electron.NET

### Introdução

**Electron.NET** permite criar aplicações desktop usando HTML/CSS/JS com backend .NET.

**Vantagens:**
- ✅ UI moderna (web tech)
- ✅ Multiplataforma
- ✅ Usa .NET no backend

**Limitações:**
- ❌ Apps grandes (Chromium embedded)

---

### Instalação

```bash
# Instalar ferramenta
dotnet tool install ElectronNET.CLI -g

# Criar projeto
dotnet new web -o MeuRPA.Electron
cd MeuRPA.Electron
electronize init
```

---

## Comparação

| Framework | Plataforma | Complexidade | Tamanho | Uso |
|-----------|------------|--------------|---------|-----|
| **WPF** | Windows | Média | Médio | Desktop Windows moderno |
| **WinForms** | Windows | Baixa | Pequeno | Desktop Windows simples |
| **Avalonia** | Multi | Média | Médio | Desktop multiplataforma |
| **Terminal.Gui** | Multi | Baixa | Pequeno | Console/Terminal avançado |
| **Electron.NET** | Multi | Alta | Grande | Web-like desktop apps |

---

## Recomendações para RPA

### Para Controle/Monitoramento

**Simples:**
- Windows Forms (se apenas Windows)
- Terminal.Gui (console avançado)

**Moderno:**
- WPF (Windows)
- Avalonia (multiplataforma)

### Para Configuração

- WPF ou Windows Forms com formulários
- JSON + Console (sem UI)

### Para Visualização de Logs

- TextBox/TextView com ScrollViewer
- Integração com Serilog

---

## Exemplo Completo: Dashboard RPA com WPF

```csharp
// ViewModel (MVVM pattern)
public class MainViewModel : INotifyPropertyChanged
{
    private string _log;
    private string _status;
    private bool _executando;
    
    public string Log
    {
        get => _log;
        set
        {
            _log = value;
            OnPropertyChanged();
        }
    }
    
    public string Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
        }
    }
    
    public bool Executando
    {
        get => _executando;
        set
        {
            _executando = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PodeIniciar));
        }
    }
    
    public bool PodeIniciar => !Executando;
    
    public ICommand IniciarCommand { get; }
    public ICommand PararCommand { get; }
    
    public MainViewModel()
    {
        IniciarCommand = new RelayCommand(Iniciar, () => PodeIniciar);
        PararCommand = new RelayCommand(Parar, () => Executando);
        Status = "Pronto";
    }
    
    private async void Iniciar()
    {
        Executando = true;
        Status = "Executando...";
        AdicionarLog("=== RPA Iniciado ===");
        
        await Task.Run(() =>
        {
            for (int i = 1; i <= 10 && Executando; i++)
            {
                AdicionarLog($"[{DateTime.Now:HH:mm:ss}] Item {i} processado");
                Thread.Sleep(1000);
            }
        });
        
        AdicionarLog("=== RPA Finalizado ===");
        Status = "Pronto";
        Executando = false;
    }
    
    private void Parar()
    {
        Executando = false;
        AdicionarLog("Parando...");
    }
    
    private void AdicionarLog(string mensagem)
    {
        Log += mensagem + Environment.NewLine;
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

---

## Recursos Adicionais

- **WPF**: https://docs.microsoft.com/dotnet/desktop/wpf/
- **WinForms**: https://docs.microsoft.com/dotnet/desktop/winforms/
- **Avalonia**: https://avaloniaui.net/
- **Terminal.Gui**: https://github.com/gui-cs/Terminal.Gui
- **Electron.NET**: https://github.com/ElectronNET/Electron.NET

---

**Versão:** 1.0  
**Última atualização:** Novembro 2025
