# Microsoft.Playwright - Automação Web

## Índice
1. [Introdução](#introdução)
2. [Instalação](#instalação)
3. [Conceitos Básicos](#conceitos-básicos)
4. [Primeiros Passos](#primeiros-passos)
5. [Seletores](#seletores)
6. [Interações](#interações)
7. [Navegação](#navegação)
8. [Screenshots e PDFs](#screenshots-e-pdfs)
9. [Esperas e Timeouts](#esperas-e-timeouts)
10. [Exemplos Práticos](#exemplos-práticos)
11. [Boas Práticas](#boas-práticas)

---

## Introdução

**Playwright** é uma biblioteca moderna para automação de navegadores desenvolvida pela Microsoft. Suporta Chromium, Firefox e WebKit (Safari) com uma única API.

### Vantagens
- ✅ Suporte a múltiplos navegadores
- ✅ Auto-waiting (espera automática por elementos)
- ✅ API assíncrona e performática
- ✅ Screenshots, PDFs e vídeos
- ✅ Interceptação de requisições de rede
- ✅ Emulação de dispositivos móveis
- ✅ Suporte a múltiplos contextos e páginas

---

## Instalação

### Passo a Passo Completo

#### 1. Adicionar o pacote NuGet

No terminal do seu projeto:

```bash
dotnet add package Microsoft.Playwright
```

#### 2. Instalar os navegadores

**Importante:** Após adicionar o pacote, você DEVE instalar os navegadores. Sem isso, o Playwright não funcionará!

**No Windows (PowerShell):**
```bash
# Navegue até a pasta do projeto
cd C:\CaminhoDoSeuProjeto

# Compile primeiro
dotnet build

# Instale os navegadores
pwsh bin/Debug/net9.0/playwright.ps1 install
```

**No Linux/Mac:**
```bash
# Compile primeiro
dotnet build

# Instale os navegadores
./bin/Debug/net9.0/playwright.sh install
```

#### 3. Instalar apenas um navegador específico (opcional)

Se você só precisa de um navegador específico para economizar espaço:
```bash
pwsh bin/Debug/net9.0/playwright.ps1 install chromium
```

Opções: `chromium`, `firefox`, `webkit`

---

## Conceitos Básicos

### Hierarquia de Objetos

```
Playwright
  └── Browser (navegador)
       └── BrowserContext (contexto isolado)
            └── Page (página/aba)
                 └── Frame (iframe)
                      └── Locator (elemento)
```

### Browser vs BrowserContext

- **Browser**: Instância do navegador (Chrome, Firefox, etc)
- **BrowserContext**: Sessão isolada com cookies, cache e storage próprios
- **Page**: Uma aba ou janela do navegador

---

## Primeiros Passos

### Exemplo Básico

```csharp
using Microsoft.Playwright;

class Program
{
    static async Task Main(string[] args)
    {
        // Instalar Playwright
        var exitCode = Microsoft.Playwright.Program.Main(new[] { "install" });
        if (exitCode != 0)
        {
            throw new Exception($"Falha ao instalar navegadores. Código: {exitCode}");
        }

        // Criar instância do Playwright
        using var playwright = await Playwright.CreateAsync();
        
        // Lançar navegador
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = false // false = mostra o navegador
        });
        
        // Criar contexto
        var context = await browser.NewContextAsync();
        
        // Criar página
        var page = await context.NewPageAsync();
        
        // Navegar
        await page.GotoAsync("https://www.google.com");
        
        // Interagir
        await page.FillAsync("input[name='q']", "Playwright .NET");
        await page.PressAsync("input[name='q']", "Enter");
        
        // Esperar
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Screenshot
        await page.ScreenshotAsync(new() { Path = "resultado.png" });
        
        Console.WriteLine("Automação concluída!");
    }
}
```

---

## Seletores

### Tipos de Seletores

#### 1. CSS Selector
```csharp
await page.ClickAsync("button.submit");
await page.ClickAsync("#loginBtn");
await page.ClickAsync("div > button:first-child");
```

#### 2. Text Selector
```csharp
await page.ClickAsync("text=Entrar");
await page.ClickAsync("text=/Cadastr(o|ar)/i"); // Regex
```

#### 3. XPath
```csharp
await page.ClickAsync("xpath=//button[@type='submit']");
```

#### 4. Role Selector (Recomendado)
```csharp
await page.GetByRole(AriaRole.Button, new() { Name = "Entrar" }).ClickAsync();
await page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("test@email.com");
```

#### 5. Test ID (Melhor Prática)
```csharp
// HTML: <button data-testid="submit-btn">Enviar</button>
await page.GetByTestId("submit-btn").ClickAsync();
```

#### 6. Label
```csharp
await page.GetByLabel("Nome completo").FillAsync("João Silva");
```

#### 7. Placeholder
```csharp
await page.GetByPlaceholder("Digite seu email").FillAsync("email@example.com");
```

### Combinando Seletores

```csharp
// Filho direto
await page.Locator("div.form >> button").ClickAsync();

// Múltiplos seletores
await page.Locator("button.submit, button.enviar").ClickAsync();

// Filtrar por texto
await page.Locator("li").Filter(new() { HasText = "Ativo" }).ClickAsync();
```

---

## Interações

### Cliques

```csharp
// Clique simples
await page.ClickAsync("button");

// Duplo clique
await page.DblClickAsync("div.item");

// Clique com botão direito
await page.ClickAsync("div", new() { Button = MouseButton.Right });

// Clique com modificadores
await page.ClickAsync("a", new() { Modifiers = new[] { KeyboardModifier.Control } });

// Forçar clique (ignora verificações)
await page.ClickAsync("button", new() { Force = true });
```

### Preencher Campos

```csharp
// Preencher texto
await page.FillAsync("input#name", "João Silva");

// Limpar e preencher
await page.FillAsync("input#email", "");
await page.FillAsync("input#email", "novo@email.com");

// Digitar caractere por caractere
await page.TypeAsync("input#search", "Buscar isto", new() { Delay = 100 });
```

### Teclas

```csharp
// Pressionar tecla
await page.PressAsync("input", "Enter");
await page.PressAsync("input", "Tab");

// Combinações
await page.PressAsync("input", "Control+A");
await page.PressAsync("body", "Control+S");

// Múltiplas teclas
await page.Keyboard.DownAsync("Shift");
await page.ClickAsync("text=Item 1");
await page.ClickAsync("text=Item 5");
await page.Keyboard.UpAsync("Shift");
```

### Select (Dropdown)

```csharp
// Por valor
await page.SelectOptionAsync("select#country", "BR");

// Por texto visível
await page.SelectOptionAsync("select#country", new SelectOptionValue { Label = "Brasil" });

// Múltiplas opções
await page.SelectOptionAsync("select#tags", new[] { "tag1", "tag2", "tag3" });
```

### Checkbox e Radio

```csharp
// Marcar
await page.CheckAsync("input#terms");

// Desmarcar
await page.UncheckAsync("input#newsletter");

// Alternar
if (await page.IsCheckedAsync("input#remember"))
    await page.UncheckAsync("input#remember");
else
    await page.CheckAsync("input#remember");
```

### Upload de Arquivos

```csharp
// Upload único
await page.SetInputFilesAsync("input[type='file']", "documento.pdf");

// Upload múltiplo
await page.SetInputFilesAsync("input[type='file']", new[] 
{ 
    "arquivo1.pdf", 
    "arquivo2.pdf" 
});

// Remover arquivo
await page.SetInputFilesAsync("input[type='file']", new string[0]);
```

### Hover (Passar o mouse)

```csharp
await page.HoverAsync("button.menu");
await page.ClickAsync("li.dropdown-item");
```

### Drag and Drop

```csharp
await page.DragAndDropAsync("#source", "#target");

// Ou manualmente
await page.HoverAsync("#source");
await page.Mouse.DownAsync();
await page.HoverAsync("#target");
await page.Mouse.UpAsync();
```

---

## Navegação

### Ir para URL

```csharp
// Navegar
await page.GotoAsync("https://example.com");

// Com opções
await page.GotoAsync("https://example.com", new()
{
    WaitUntil = WaitUntilState.NetworkIdle,
    Timeout = 30000 // 30 segundos
});
```

### Navegação do Navegador

```csharp
// Voltar
await page.GoBackAsync();

// Avançar
await page.GoForwardAsync();

// Recarregar
await page.ReloadAsync();
```

### Obter Informações

```csharp
// URL atual
string url = page.Url;

// Título da página
string title = await page.TitleAsync();
```

---

## Screenshots e PDFs

### Screenshots

```csharp
// Screenshot da página inteira
await page.ScreenshotAsync(new() 
{ 
    Path = "pagina.png",
    FullPage = true 
});

// Screenshot de um elemento
var element = page.Locator("div.content");
await element.ScreenshotAsync(new() { Path = "elemento.png" });

// Screenshot em memória
byte[] screenshot = await page.ScreenshotAsync();
```

### PDFs

```csharp
// Gerar PDF (apenas em Chromium)
await page.PdfAsync(new()
{
    Path = "documento.pdf",
    Format = "A4",
    PrintBackground = true,
    Margin = new()
    {
        Top = "1cm",
        Right = "1cm",
        Bottom = "1cm",
        Left = "1cm"
    }
});
```

### Vídeos

```csharp
var context = await browser.NewContextAsync(new()
{
    RecordVideoDir = "videos/",
    RecordVideoSize = new() { Width = 1280, Height = 720 }
});

var page = await context.NewPageAsync();
// ... automação ...

await context.CloseAsync();
// Vídeo salvo em videos/
```

---

## Esperas e Timeouts

### Esperas Automáticas

Playwright espera automaticamente por:
- Elemento estar visível
- Elemento estar habilitado
- Elemento estar estável (não animando)

### Esperas Explícitas

```csharp
// Esperar por seletor
await page.WaitForSelectorAsync("div.resultado");

// Esperar por estado de carregamento
await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

// Esperar por URL
await page.WaitForURLAsync("**/dashboard");

// Esperar por função
await page.WaitForFunctionAsync("() => document.readyState === 'complete'");

// Esperar por timeout
await page.WaitForTimeoutAsync(3000); // 3 segundos (evite usar)
```

### Configurar Timeouts

```csharp
// Timeout global do contexto
var context = await browser.NewContextAsync(new()
{
    DefaultTimeout = 60000 // 60 segundos
});

// Timeout de navegação
await page.GotoAsync("https://example.com", new()
{
    Timeout = 30000
});

// Timeout de ação
await page.ClickAsync("button", new()
{
    Timeout = 5000
});
```

---

## Exemplos Práticos

### Exemplo 1: Login em Sistema

```csharp
async Task FazerLogin(IPage page, string email, string senha)
{
    await page.GotoAsync("https://sistema.com/login");
    
    await page.FillAsync("input#email", email);
    await page.FillAsync("input#password", senha);
    await page.ClickAsync("button[type='submit']");
    
    // Esperar redirecionamento
    await page.WaitForURLAsync("**/dashboard");
    
    Console.WriteLine("Login realizado com sucesso!");
}
```

### Exemplo 2: Extrair Dados de Tabela

```csharp
async Task<List<Produto>> ExtrairProdutos(IPage page)
{
    await page.GotoAsync("https://loja.com/produtos");
    
    var produtos = new List<Produto>();
    var linhas = await page.Locator("table tbody tr").AllAsync();
    
    foreach (var linha in linhas)
    {
        var nome = await linha.Locator("td:nth-child(1)").TextContentAsync();
        var precoTexto = await linha.Locator("td:nth-child(2)").TextContentAsync();
        var preco = decimal.Parse(precoTexto.Replace("R$", "").Trim());
        
        produtos.Add(new Produto { Nome = nome, Preco = preco });
    }
    
    return produtos;
}
```

### Exemplo 3: Preencher Formulário Complexo

```csharp
async Task PreencherCadastro(IPage page)
{
    await page.GotoAsync("https://site.com/cadastro");
    
    // Dados pessoais
    await page.FillAsync("input#nome", "João Silva");
    await page.FillAsync("input#email", "joao@email.com");
    await page.FillAsync("input#telefone", "(11) 98765-4321");
    
    // Data de nascimento
    await page.FillAsync("input#data-nascimento", "01/01/1990");
    
    // Select
    await page.SelectOptionAsync("select#estado", "SP");
    
    // Radio button
    await page.ClickAsync("input[name='genero'][value='M']");
    
    // Checkboxes
    await page.CheckAsync("input#aceito-termos");
    await page.CheckAsync("input#receber-newsletter");
    
    // Upload
    await page.SetInputFilesAsync("input#documento", "identidade.pdf");
    
    // Submit
    await page.ClickAsync("button[type='submit']");
    
    // Esperar mensagem de sucesso
    await page.WaitForSelectorAsync("div.success");
    var mensagem = await page.TextContentAsync("div.success");
    Console.WriteLine($"Sucesso: {mensagem}");
}
```

### Exemplo 4: Automação com Múltiplas Páginas

```csharp
async Task ProcessarMultiplasPaginas()
{
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync();
    var context = await browser.NewContextAsync();
    
    // Abrir múltiplas páginas
    var page1 = await context.NewPageAsync();
    var page2 = await context.NewPageAsync();
    
    // Trabalhar em paralelo
    var task1 = page1.GotoAsync("https://site1.com");
    var task2 = page2.GotoAsync("https://site2.com");
    
    await Task.WhenAll(task1, task2);
    
    // Processar cada página
    var dados1 = await ExtrairDados(page1);
    var dados2 = await ExtrairDados(page2);
}
```

---

## Boas Práticas

### 1. Use Locators Resilientes
```csharp
// ✅ BOM - usa atributo de teste
await page.GetByTestId("submit-btn").ClickAsync();

// ✅ BOM - usa role semântico
await page.GetByRole(AriaRole.Button, new() { Name = "Enviar" }).ClickAsync();

// ❌ RUIM - frágil a mudanças de CSS
await page.ClickAsync("div > div:nth-child(3) > button");
```

### 2. Reutilize Contextos
```csharp
// Uma vez por sessão
var context = await browser.NewContextAsync(new()
{
    Locale = "pt-BR",
    Viewport = new() { Width = 1920, Height = 1080 },
    UserAgent = "..."
});

// Múltiplas páginas no mesmo contexto
var page1 = await context.NewPageAsync();
var page2 = await context.NewPageAsync();
```

### 3. Gerencie Recursos Corretamente
```csharp
using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync();
await using var context = await browser.NewContextAsync();
await using var page = await context.NewPageAsync();

// Recursos liberados automaticamente
```

### 4. Configure Timeouts Apropriados
```csharp
// Para ações rápidas
await page.ClickAsync("button", new() { Timeout = 5000 });

// Para operações lentas
await page.WaitForSelectorAsync("div.resultado", new() { Timeout = 60000 });
```

### 5. Trate Erros Específicos
```csharp
try
{
    await page.ClickAsync("button", new() { Timeout = 5000 });
}
catch (TimeoutException)
{
    Console.WriteLine("Elemento não encontrado no tempo esperado");
    await page.ScreenshotAsync(new() { Path = "erro.png" });
}
```

### 6. Use Mode Headless em Produção
```csharp
var browser = await playwright.Chromium.LaunchAsync(new()
{
    Headless = Environment.GetEnvironmentVariable("ENV") == "production"
});
```

---

## ⚠️ Erros Comuns e Soluções

### Erro: "Executable doesn't exist"

**Problema:** Navegadores não foram instalados.

**Solução:**
```bash
# Compile o projeto primeiro
dotnet build

# Depois instale os navegadores
pwsh bin/Debug/net9.0/playwright.ps1 install

# No Linux/Mac:
./bin/Debug/net9.0/playwright.sh install
```

### Erro: "Timeout 30000ms exceeded"

**Problema:** Elemento não foi encontrado ou página demorou muito para carregar.

**Soluções:**
1. Aumentar o timeout:
```csharp
await page.WaitForSelectorAsync("button", new() { Timeout = 60000 });
```

2. Verificar se o seletor está correto
3. Aguardar o carregamento da página primeiro:
```csharp
await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
```

### Erro: "Target closed"

**Problema:** Página foi fechada enquanto tentava interagir.

**Solução:**
```csharp
if (!page.IsClosed)
{
    await page.ClickAsync("button");
}
```

### Erro: Elemento não clicável

**Problema:** Elemento está coberto por outro elemento.

**Solução:**
```csharp
// Scroll até o elemento
await page.Locator("button").ScrollIntoViewIfNeededAsync();

// Depois clique
await page.ClickAsync("button");
```

---

## Recursos Adicionais

- **Documentação Oficial**: https://playwright.dev/dotnet/
- **API Reference**: https://playwright.dev/dotnet/docs/api/class-playwright
- **Exemplos**: https://github.com/microsoft/playwright-dotnet
- **Trace Viewer**: Para debug visual de automações

---

**Dica Final**: Use o **Codegen** do Playwright para gerar código automaticamente:
```bash
pwsh bin/Debug/net9.0/playwright.ps1 codegen https://exemplo.com
```

**Versão:** 1.0  
**Última atualização:** Novembro 2025
