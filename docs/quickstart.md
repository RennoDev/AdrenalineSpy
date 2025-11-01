# Quick Start - Seu Primeiro RPA em 10 Minutos

## 🎯 Objetivo

Criar sua primeira automação RPA que:
- ✅ Abre um navegador
- ✅ Acessa um site
- ✅ Faz uma busca
- ✅ Captura informações
- ✅ Salva em um arquivo

---

## ⚡ Pré-requisitos (5 minutos)

### 1. Instalar .NET SDK

**Windows:**
1. Acesse: https://dotnet.microsoft.com/download
2. Baixe o instalador do .NET 8 ou superior
3. Execute e siga o wizard
4. Reinicie o terminal

**Verificar:**
```bash
dotnet --version
# Deve mostrar algo como: 8.0.x ou 9.0.x
```

### 2. Escolher um Editor

**Opção 1: Visual Studio Code** (Recomendado para iniciantes)
- Download: https://code.visualstudio.com/
- Leve e fácil de usar
- Extensões úteis:
  - C# (Microsoft)
  - C# Dev Kit (Microsoft)

**Opção 2: Visual Studio Community**
- Download: https://visualstudio.microsoft.com/vs/community/
- IDE completa e profissional
- Gratuito para uso pessoal/educacional

---

## 🚀 Criar o Projeto (2 minutos)

### Passo 1: Abrir Terminal

**No VS Code:**
- Pressione `` Ctrl + ` `` (acento grave)
- Ou: Menu Terminal → New Terminal

**No Windows:**
- Pressione `Win + R`
- Digite: `powershell`
- Enter

### Passo 2: Criar Pasta e Projeto

```bash
# Navegar para onde quer criar o projeto
cd C:\Users\SeuNome\Documentos

# Criar pasta
mkdir MeuPrimeiroRPA

# Entrar na pasta
cd MeuPrimeiroRPA

# Criar projeto .NET
dotnet new console -o BuscadorWeb

# Entrar no projeto
cd BuscadorWeb
```

### Passo 3: Abrir no VS Code (opcional)

```bash
code .
```

---

## 📦 Instalar Playwright (3 minutos)

### Passo 1: Adicionar Pacote

```bash
dotnet add package Microsoft.Playwright
```

### Passo 2: Compilar Projeto

```bash
dotnet build
```

### Passo 3: Instalar Navegador

**Windows:**
```powershell
pwsh bin/Debug/net8.0/playwright.ps1 install chromium
```

**Se der erro no PowerShell, tente:**
```powershell
powershell -ExecutionPolicy Bypass -File bin/Debug/net8.0/playwright.ps1 install chromium
```

⏱️ Isso vai baixar o Chromium (~200MB). Aguarde uns 2-3 minutos.

---

## 💻 Escrever o Código (5 minutos)

### Substituir conteúdo do `Program.cs`

Abra o arquivo `Program.cs` e substitua TODO o conteúdo por:

```csharp
using Microsoft.Playwright;
using System.Text.Json;

Console.WriteLine("🤖 Iniciando Bot de Busca...\n");

// 1. Criar instância do Playwright
using var playwright = await Playwright.CreateAsync();

// 2. Abrir navegador (Headless = false para ver o que está acontecendo)
await using var browser = await playwright.Chromium.LaunchAsync(new()
{
    Headless = false,  // Vai mostrar o navegador
    SlowMo = 1000      // Vai devagar para você ver (1 segundo entre ações)
});

// 3. Criar nova página/aba
var page = await browser.NewPageAsync();

// 4. Navegar para o Google
Console.WriteLine("📍 Acessando Google...");
await page.GotoAsync("https://www.google.com");

// 5. Aceitar cookies (se aparecer)
try
{
    var acceptButton = page.Locator("button:has-text('Aceitar tudo'), button:has-text('Accept all')");
    if (await acceptButton.IsVisibleAsync())
    {
        await acceptButton.ClickAsync();
        Console.WriteLine("✅ Cookies aceitos");
    }
}
catch
{
    Console.WriteLine("⏭️  Sem popup de cookies");
}

// 6. Procurar a caixa de busca e digitar
Console.WriteLine("⌨️  Digitando busca...");
await page.FillAsync("textarea[name='q']", "RPA com .NET");

// 7. Pressionar Enter
await page.PressAsync("textarea[name='q']", "Enter");

// 8. Aguardar resultados carregarem
await page.WaitForSelectorAsync("div#search");
Console.WriteLine("✅ Resultados carregados!");

// 9. Capturar títulos dos primeiros 5 resultados
var resultados = await page.Locator("h3").AllAsync();
var lista = new List<string>();

Console.WriteLine("\n📋 Top 5 Resultados:\n");

for (int i = 0; i < Math.Min(5, resultados.Count); i++)
{
    var titulo = await resultados[i].InnerTextAsync();
    lista.Add(titulo);
    Console.WriteLine($"{i + 1}. {titulo}");
}

// 10. Tirar screenshot
await page.ScreenshotAsync(new() { Path = "resultados.png" });
Console.WriteLine("\n📸 Screenshot salvo em: resultados.png");

// 11. Salvar resultados em JSON
var json = JsonSerializer.Serialize(new
{
    Data = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
    Busca = "RPA com .NET",
    Resultados = lista
}, new JsonSerializerOptions { WriteIndented = true });

await File.WriteAllTextAsync("resultados.json", json);
Console.WriteLine("💾 Resultados salvos em: resultados.json");

// 12. Fechar navegador
await browser.CloseAsync();

Console.WriteLine("\n✨ Automação concluída com sucesso!\n");
```

### Salvar o arquivo

- VS Code: `Ctrl + S`
- Outros editores: File → Save

---

## ▶️ Executar a Automação

No terminal, execute:

```bash
dotnet run
```

### O que vai acontecer:

1. ✅ Vai abrir o navegador Chromium
2. ✅ Vai acessar o Google
3. ✅ Vai fazer uma busca por "RPA com .NET"
4. ✅ Vai capturar os 5 primeiros resultados
5. ✅ Vai salvar um screenshot (`resultados.png`)
6. ✅ Vai salvar os dados em JSON (`resultados.json`)
7. ✅ Vai fechar o navegador

### Verificar os Arquivos Criados

```bash
# Listar arquivos na pasta
dir

# Você deve ver:
# - resultados.png
# - resultados.json
```

### Ver o JSON gerado

```bash
# Windows
notepad resultados.json

# Ou no VS Code
code resultados.json
```

Conteúdo exemplo:
```json
{
  "Data": "2025-11-01 14:30:00",
  "Busca": "RPA com .NET",
  "Resultados": [
    "RPA with .NET - Complete Guide",
    "Automating Tasks with C# and .NET",
    "Building Bots in .NET Core",
    "Microsoft Playwright for .NET",
    "C# Automation Tutorial"
  ]
}
```

---

## 🎨 Personalize Sua Automação

### Mudar a Busca

Linha 38:
```csharp
await page.FillAsync("textarea[name='q']", "SEU TERMO AQUI");
```

### Capturar Mais Resultados

Linha 47:
```csharp
for (int i = 0; i < Math.Min(10, resultados.Count); i++)  // 10 em vez de 5
```

### Executar Sem Abrir Navegador (Mais Rápido)

Linha 10:
```csharp
Headless = true,  // Não mostra o navegador
```

### Remover o Delay

Linha 11:
```csharp
// SlowMo = 1000  // Comentar ou remover esta linha
```

---

## ⚠️ Problemas Comuns

### Erro: "Executable doesn't exist"

**Causa:** Navegadores não foram instalados.

**Solução:**
```bash
# Compile primeiro
dotnet build

# Depois instale
pwsh bin/Debug/net8.0/playwright.ps1 install chromium
```

### Erro: "Timeout 30000ms exceeded"

**Causa:** Internet lenta ou seletor errado.

**Solução:** Aumentar timeout:
```csharp
await page.GotoAsync("https://google.com", new() { Timeout = 60000 });
```

### Erro: PowerShell execution policy

**Solução:**
```powershell
Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
```

Ou execute como administrador:
```powershell
powershell -ExecutionPolicy Bypass -File bin/Debug/net8.0/playwright.ps1 install chromium
```

### Navegador não abre (Headless = false)

Verifique se:
1. SlowMo está configurado
2. Não está rodando como serviço
3. Há interface gráfica disponível

---

## 🎯 Próximos Passos

### 1. Automatizar Outros Sites

Experimente:
- GitHub (fazer busca de repositórios)
- YouTube (buscar vídeos)
- E-commerce (buscar produtos)

### 2. Adicionar Mais Funcionalidades

- Login em sites
- Preencher formulários
- Baixar arquivos
- Enviar emails com os resultados

### 3. Estudar Mais

Continue com os guias avançados:
- [index.md](index.md) - Documentação completa de RPA em .NET
- [playwright.md](playwright.md) - Guia completo do Playwright
- [serilog.md](serilog.md) - Adicionar logs profissionais
- [csvhelper.md](csvhelper.md) - Salvar em CSV em vez de JSON
- [mailkit.md](mailkit.md) - Enviar resultados por email

### 4. Automatizar Excel

Veja: [epplus.md](epplus.md) para salvar em planilhas Excel

### 5. Agendar Execução

Veja: [quartz.md](quartz.md) para rodar automaticamente

---

## 📚 Resumo do que Você Aprendeu

✅ Criar projeto .NET Console  
✅ Instalar pacotes NuGet  
✅ Usar Playwright para automação web  
✅ Navegar e interagir com páginas  
✅ Capturar dados de elementos  
✅ Salvar screenshots  
✅ Exportar dados para JSON  
✅ Executar e debugar código C#  

---

## 💡 Dicas de Ouro

1. **Sempre use `await`** com métodos assíncronos do Playwright
2. **Use seletores específicos** para evitar capturar elementos errados
3. **Tire screenshots** quando der erro para debugar
4. **Adicione `try-catch`** em operações que podem falhar
5. **Teste incrementalmente** - adicione uma funcionalidade por vez

---

## 🎊 Parabéns!

Você acabou de criar sua primeira automação RPA em .NET!

**O que fazer agora:**

🔗 Compartilhe com amigos  
📖 Leia os guias avançados em [docs/](.)  
💻 Crie suas próprias automações  
🚀 Suba seu projeto no GitHub (veja [git-github-gitlab.md](git-github-gitlab.md))  

---

**Dúvidas?** Consulte os outros guias na pasta `docs/` ou procure a comunidade .NET!

**Boa sorte com suas automações! 🤖✨**
