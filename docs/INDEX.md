# 📚 Índice de Ferramentas RPA - AdrenalineSpy

**Referência rápida de todas as ferramentas e pacotes NuGet documentados neste projeto.**

Use este índice para descobrir rapidamente qual ferramenta usar para cada necessidade.

---

## 🚀 Começando

- **Novo em RPA?** → [Quick Start](quickstart.md) - Tutorial de 10 minutos
- **Como organizar o código?** → [Arquitetura de Código](arquitetura-codigo.md) - Padrão Main → Workflow → Tasks
- **Quer versionar seu projeto?** → [Git, GitHub e GitLab](git-github-gitlab.md)

---

## 🤖 Automação

### [Microsoft.Playwright](playwright.md)
**O que faz:** Automação de navegadores web (Chrome, Firefox, Safari)  
**Quando usar:** Preencher formulários web, extrair dados de sites, testes automatizados  
**Instalação:** `dotnet add package Microsoft.Playwright`  
**⚠️ Importante:** Requer `pwsh bin/Debug/net9.0/playwright.ps1 install` após instalação

### [FlaUI](flaui.md)
**O que faz:** Automação de aplicações desktop Windows  
**Quando usar:** Interagir com softwares legados, ERPs, aplicações que não têm API  
**Instalação:** `dotnet add package FlaUI.UIA3`

### [InputSimulator](inputsimulator.md)
**O que faz:** Simulação de teclado e mouse  
**Quando usar:** Atalhos de teclado, cliques de mouse, quando outros métodos falham  
**Instalação:** `dotnet add package InputSimulatorStandard`

---

## 📄 Manipulação de Arquivos

### [EPPlus](epplus.md)
**O que faz:** Leitura e escrita de planilhas Excel (.xlsx)  
**Quando usar:** Relatórios, processar planilhas, importar/exportar dados  
**Instalação:** `dotnet add package EPPlus`  
**⚠️ Licença:** Uso comercial requer licença paga (PolyForm Noncommercial License)

### [CsvHelper](csvhelper.md)
**O que faz:** Leitura e escrita de arquivos CSV  
**Quando usar:** Processar dados tabulares simples, importar/exportar arquivos CSV  
**Instalação:** `dotnet add package CsvHelper`

### [iText7](itext7.md)
**O que faz:** Leitura e geração de arquivos PDF  
**Quando usar:** Criar relatórios em PDF, extrair texto de PDFs, preencher formulários PDF  
**Instalação:** `dotnet add package itext7`  
**⚠️ Licença:** Uso comercial requer licença paga (AGPL)

---

## 🌐 APIs e Integração

### [RestSharp](restsharp-json.md)
**O que faz:** Consumo de APIs REST  
**Quando usar:** Integrar com sistemas externos, consumir webservices, APIs RESTful  
**Instalação:** `dotnet add package RestSharp`

### [Newtonsoft.Json](restsharp-json.md)
**O que faz:** Serialização/deserialização JSON  
**Quando usar:** Trabalhar com dados JSON, APIs, arquivos de configuração  
**Instalação:** `dotnet add package Newtonsoft.Json`

### [MailKit](mailkit.md)
**O que faz:** Envio e recebimento de emails (SMTP, IMAP, POP3)  
**Quando usar:** Enviar notificações, processar emails recebidos, anexos  
**Instalação:** `dotnet add package MailKit`

---

## 🔧 Infraestrutura

### [Serilog](serilog.md)
**O que faz:** Sistema de logging estruturado  
**Quando usar:** Registrar eventos, debug, monitoramento, auditoria  
**Instalação:**
```bash
dotnet add package Serilog
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

### [Quartz.NET](quartz.md)
**O que faz:** Agendamento de tarefas (cron jobs)  
**Quando usar:** Executar automações em horários específicos, tarefas recorrentes  
**Instalação:** `dotnet add package Quartz`

---

## 💾 Banco de Dados

### [Entity Framework Core](orm.md)
**O que faz:** ORM completo para acesso a bancos de dados  
**Quando usar:** Projetos complexos, migrações, relacionamentos complexos  
**Instalação:**
```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer  # ou .Sqlite, .MySql, .Npgsql
```

### [Dapper](orm.md)
**O que faz:** Micro ORM leve e performático  
**Quando usar:** Queries SQL diretas, alta performance, consultas simples  
**Instalação:** `dotnet add package Dapper`

### [Docker Setup](docker-setup.md)
**O que faz:** Bancos de dados em containers (MySQL, PostgreSQL, SQL Server)  
**Quando usar:** Ambiente de desenvolvimento, testes, evitar instalar bancos localmente  
**Pré-requisitos:** Docker Desktop + WSL 2 (Windows)

---

## 🖥️ Interfaces Gráficas

### [WPF (Windows Presentation Foundation)](gui.md)
**O que faz:** Interfaces gráficas modernas para Windows  
**Quando usar:** Aplicações desktop Windows com UI rica  
**Framework:** Nativo do .NET (já incluído)

### [Windows Forms](gui.md)
**O que faz:** Interfaces gráficas simples para Windows  
**Quando usar:** UIs simples, prototipagem rápida, legado  
**Framework:** Nativo do .NET (já incluído)

### [Avalonia](gui.md)
**O que faz:** Interfaces gráficas multiplataforma (Windows, Linux, macOS)  
**Quando usar:** Aplicações desktop que rodam em múltiplos sistemas operacionais  
**Instalação:** `dotnet add package Avalonia`

### [Terminal.Gui](gui.md)
**O que faz:** Interfaces de texto no terminal/console  
**Quando usar:** Ferramentas CLI interativas, servidores sem interface gráfica  
**Instalação:** `dotnet add package Terminal.Gui`

### [Electron.NET](gui.md)
**O que faz:** Aplicações desktop usando web technologies (HTML/CSS/JS)  
**Quando usar:** Aproveitar skills de web dev, UI complexa e moderna  
**Instalação:** Via template do Electron.NET

---

## 🚀 Deploy e Produção

### [Deploy](deploy.md)
**Opções documentadas:**
- 🏠 **Deploy Local:** Executável standalone, serviço Windows
- 🐳 **Docker:** Containerização e deploy via Docker
- ☁️ **GitHub Actions:** CI/CD automatizado
- ☁️ **Azure:** Azure App Service, Azure Functions, Azure Container Instances
- ☁️ **Railway:** Plataforma PaaS gratuita para estudos
- ☁️ **Outras plataformas:** Heroku, Render, Fly.io

---

## 📋 Tabela de Referência Rápida

| Ferramenta | Categoria | Uso Principal | Licença |
|------------|-----------|---------------|---------|
| **Playwright** | Automação | Web scraping, automação web | Apache 2.0 |
| **FlaUI** | Automação | Automação desktop Windows | MIT |
| **InputSimulator** | Automação | Simulação teclado/mouse | MIT |
| **EPPlus** | Arquivos | Planilhas Excel | ⚠️ Noncommercial |
| **CsvHelper** | Arquivos | Arquivos CSV | Apache 2.0 / MIT |
| **iText7** | Arquivos | PDFs | ⚠️ AGPL |
| **RestSharp** | Integração | APIs REST | Apache 2.0 |
| **Newtonsoft.Json** | Integração | JSON | MIT |
| **MailKit** | Integração | Email | MIT |
| **Serilog** | Infraestrutura | Logging | Apache 2.0 |
| **Quartz.NET** | Infraestrutura | Agendamento | Apache 2.0 |
| **EF Core** | Dados | ORM completo | MIT |
| **Dapper** | Dados | Micro ORM | Apache 2.0 |

---

## 🎯 Combinações Comuns

### RPA Web Completo
```bash
dotnet add package Microsoft.Playwright
dotnet add package EPPlus
dotnet add package Serilog
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

### RPA Desktop Windows
```bash
dotnet add package FlaUI.UIA3
dotnet add package InputSimulatorStandard
dotnet add package EPPlus
dotnet add package Serilog
```

### Integração com APIs
```bash
dotnet add package RestSharp
dotnet add package Newtonsoft.Json
dotnet add package MailKit
dotnet add package Serilog
```

### Processamento de Arquivos
```bash
dotnet add package EPPlus
dotnet add package CsvHelper
dotnet add package itext7
dotnet add package Serilog
```

---

## 💡 Dicas

- **Sempre comece pelo [Quick Start](quickstart.md)** se você é iniciante
- **EPPlus e iText7** têm restrições de licença - leia os guias antes de usar comercialmente
- **Playwright** é moderno e mais fácil que Selenium - prefira para automação web
- **Serilog** deve estar em TODOS os seus projetos RPA - logging é essencial
- **Docker** facilita muito o desenvolvimento com bancos de dados

---

## 📞 Precisa de Ajuda?

1. Consulte o guia específico da ferramenta clicando nos links acima
2. Veja o [Quick Start](quickstart.md) para exemplo prático completo
3. Leia o [README.md](../README.md) para visão geral do projeto

---

*Última atualização: 01/11/2025*
