# 📚 Documentação AdrenalineSpy - Portal de Entrada

**Bem-vindo à documentação completa do AdrenalineSpy!** Este é seu ponto de partida para dominar RPA em .NET.

O **AdrenalineSpy** é um projeto educacional que demonstra todas as ferramentas RPA .NET documentadas aqui, através de um web scraper real do site Adrenaline.com.br.

---

## 🎯 Comece Aqui

### 👶 **Novo em RPA ou .NET?**
**→ [Começando](comecando.md)** - Ordem EXATA dos guias para sucesso garantido ⭐

### 🚀 **Já tem experiência?**
**→ Vá direto para:** [Configuração](configuracao.md) → [Playwright](playwright.md) → [Serilog](serilog.md)

---

## 📖 Documentação Completa

### 🏗️ **Fundação (Leia Primeiro)**
- **[Começando](comecando.md)** - 🎯 Ordem correta de estudos + primeiros passos
- **[Program.cs](program.md)** - Como evoluir o ponto de entrada gradualmente (ESSENCIAL)
- **[Configuração](configuracao.md)** - AutomationSettings.json + Config.cs (ESSENCIAL)
- **[Arquitetura de Código](arquitetura-codigo.md)** - Padrão Program → Workflow → Tasks
- **[Git/GitHub/GitLab](git-github-gitlab.md)** - Versionamento e colaboração

### 🤖 **Automação (Core do RPA)**
- **[Playwright](playwright.md)** - ⭐ Automação web moderna (Chrome, Firefox, Safari)
- **[FlaUI](flaui.md)** - Automação desktop Windows (ERPs, aplicações legadas)  
- **[InputSimulator](inputsimulator.md)** - Simulação de teclado e mouse

### 📊 **Manipulação de Dados**
- **[EPPlus](epplus.md)** - Planilhas Excel (.xlsx) - ⚠️ Licença comercial
- **[CsvHelper](csvhelper.md)** - Arquivos CSV simples e eficientes
- **[iText7](itext7.md)** - PDFs completos - ⚠️ Licença comercial
- **[ORM](orm.md)** - Entity Framework Core + Dapper (bancos de dados)

### 🌐 **Integração e APIs**
- **[RestSharp + JSON](restsharp-json.md)** - APIs REST + manipulação JSON
- **[MailKit](mailkit.md)** - Email completo (SMTP, IMAP, POP3)

### 🔧 **Infraestrutura (Essencial)**
- **[Serilog](serilog.md)** - ⭐ Logging estruturado (obrigatório em RPA)
- **[Quartz](quartz.md)** - Agendamento de tarefas (cron jobs .NET)
- **[Docker Setup](docker-setup.md)** - Bancos em containers (MySQL, PostgreSQL, SQL Server)

### 🖥️ **Interfaces de Usuário**
- **[GUI](gui.md)** - WPF, WinForms, Avalonia, Terminal.Gui, Electron.NET

### 🚀 **Produção e Deploy**
- **[Deploy](deploy.md)** - Local, Docker, GitHub Actions, Azure, Railway

---

## 📋 O que cada guia contém

Todos os guias seguem a **mesma estrutura de 6 seções**:

1. **O que é [Tecnologia]** - Introdução + por que usar no AdrenalineSpy
2. **Como Instalar** - Pacotes NuGet + configurações extras
3. **Implementar no AutomationSettings.json** - Configurações JSON
4. **Implementar no Config.cs** - Classes C# de configuração
5. **Montar nas Tasks** - Implementação completa nas Tasks
6. **Métodos Mais Usados** - Exemplos práticos essenciais

**Resultado:** Código 100% funcional, copy-paste ready, integrado ao projeto!