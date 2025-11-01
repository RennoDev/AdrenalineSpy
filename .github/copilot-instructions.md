# AdrenalineSpy - AI Agent Instructions

## Project Overview

**AdrenalineSpy** is an educational RPA project that scrapes tech and gaming news from https://www.adrenaline.com.br/, storing data in Docker databases. It serves as a comprehensive learning platform ("universidade RPA .NET") demonstrating all documented tools in a real-world scenario.

**What it does:**
- Web scraping: Navigates Adrenaline.com.br to collect news from tech/gaming categories
- Data storage: Saves scraped data to Docker-hosted databases
- Dual operation: GUI interface + scheduled automation (user choice)
- Educational goal: Real implementation of all 16 documented RPA tools

**Target Framework:** .NET 9.0  
**Language:** Portuguese (Brazil) - all documentation is in pt-BR  
**Platform:** Windows (PowerShell commands)  
**Status:** Documentation complete, implementation in progress

## Architecture & Philosophy

### Educational Documentation + Real Implementation
- **Documentation:** `docs/` contains 16 comprehensive guides covering the entire .NET RPA ecosystem
- **Implementation:** Practical web scraper demonstrating all documented tools
- **Dual approach:** Users learn from docs, then see tools in action in the codebase
- `README.md` is the entry point with project description + categorized documentation links
- `docs/index.md` is the quick reference glossary for all tools

### Documentation Structure Pattern
All guides in `docs/` follow this template:
1. **Índice** (Table of contents)
2. **Introdução** (Why and advantages with ✅ bullets)
3. **Instalação** (Step-by-step with `dotnet add package` commands)
4. **Conceitos Básicos** (Core concepts before code)
5. **Exemplos Práticos** (Complete, runnable code)
6. **Boas Práticas** (Best practices section)
7. **Erros Comuns** (Common errors for critical tools like Playwright)

### Code Architecture
The project follows the **Program.cs → Workflow → Tasks** pattern:
- **Program.cs**: Entry point, orchestrates execution, loads Config
- **Config.cs**: Inherits settings from `AutomationSettings.json` and distributes to automation
- **Workflow/Workflow.cs**: Orchestrates sequences of tasks (singular - one workflow)
- **Workflow/Tasks/NavigationTask.cs**: Navigates Adrenaline.com.br and collects URLs
- **Workflow/Tasks/ExtractionTask.cs**: Extracts structured data from pages
- **Workflow/Tasks/MigrationTask.cs**: Migrates data to Docker database
- **Workflow/Tasks/LoggingTask.cs**: Helper for centralized exception logging (called in try/catch)
- See `docs/arquitetura-codigo.md` for complete implementation guide

### Key Files
- **`README.md`**: Project description (Adrenaline scraper) + gateway to all documentation
- **`docs/index.md`**: NuGet package glossary - quick reference for "what does X do?"
- **`docs/quickstart.md`**: 10-minute hands-on tutorial for absolute beginners
- **`docs/arquitetura-codigo.md`**: Code organization pattern (Program → Workflow → Tasks)
- **`AutomationSettings.example.json`**: Configuration template (versioned)
- **`AutomationSettings.json`**: Real credentials (git-ignored)
- **`Program.cs`**: Main implementation - web scraper + GUI + scheduling
- **`Config.cs`**: Inherits settings and distributes to automation
- **`Workflow/Workflow.cs`**: Single workflow orchestrator
- **`Workflow/Tasks/`**: NavigationTask, ExtractionTask, MigrationTask, LoggingTask

## Development Conventions

### File Naming
- Documentation: lowercase with hyphens (`git-github-gitlab.md`)
- `index.md` is lowercase (standard), `README.md` is uppercase (Git convention)
- Project files: PascalCase (`Program.cs`, `AdrenalineSpy.csproj`)

### Code Style in Documentation
- All code examples are in C# with .NET 9.0 syntax
- Use `ImplicitUsings` and `Nullable` enabled (see .csproj)
- PowerShell commands for Windows (primary platform)
- Complete, copy-paste-ready code snippets (no pseudo-code)
- Real package names, no placeholders

### Emoji Usage (Consistent Icons)
- 📚 Documentation sections
- 🚀 Quick start, deployment
- 🎯 Objectives, goals
- ✅ Completed, success, advantages
- ⚠️ Important warnings
- 🔧 Tools, configuration
- 💡 Tips, best practices
- 📝 Notes, context

## Critical Knowledge

### Documentation Coverage (17 Guides)
**Getting Started:** Quick Start, Code Architecture (Main → Workflow → Tasks), Git/GitHub/GitLab  
**Automation:** Playwright (web), FlaUI (Windows desktop), InputSimulator (keyboard/mouse)  
**Files:** EPPlus (Excel), CsvHelper (CSV), iText7 (PDF)  
**Integration:** RestSharp (APIs), MailKit (email), Newtonsoft.Json  
**Infrastructure:** Serilog (logging), Quartz (scheduling)  
**Data:** Entity Framework Core, Dapper, Docker setup (MySQL, PostgreSQL, SQL Server)  
**UI:** WPF, WinForms, Avalonia, Terminal.Gui, Electron.NET  
**Production:** Deploy guide (GitHub Actions, Azure, Railway, local)

### Special Documentation Notes
- **EPPlus & iText7**: Include licensing warnings (commercial use restrictions)
- **Playwright**: Requires `playwright.ps1 install` after package install (commonly forgotten)
- **Docker setup**: Covers WSL 2 setup, essential for Windows developers

### Implementation Stack (Adrenaline Scraper)
- **Web scraping:** Playwright (navigate Adrenaline.com.br)
- **Data storage:** Entity Framework Core or Dapper + Docker databases
- **Logging:** Serilog (track scraping operations) + LoggingTask helper
- **Scheduling:** Quartz.NET (automated execution)
- **GUI:** WPF, WinForms, or Avalonia (user control interface)
- **Data export:** EPPlus/CsvHelper (optional reports)

### Workflow for Documentation Changes
1. Update the specific guide in `docs/`
2. Check if `README.md` links need updating
3. Ensure consistency with the documentation template pattern
4. If adding new tools, update `docs/index.md` glossary

## Common Tasks

### Adding a New Documentation Guide
```bash
# File should go in docs/ with lowercase-hyphen naming
# Follow the standard template structure (Índice → Introdução → Instalação...)
# Add link to README.md in appropriate category
# Update docs/index.md glossary with the new tool
```

### Adding NuGet Package References
```xml
<!-- In AdrenalineSpy.csproj, packages go inside <ItemGroup> -->
<!-- Currently only has framework references, no packages yet -->
```

### Testing Documentation Code
```bash
# Navigate to project root
cd C:\Users\lucas\OneDrive\Documentos\CsharpProjects\AdrenalineSpy

# Restore and build
dotnet restore
dotnet build

# Run (Program.cs is empty, so this does nothing yet)
dotnet run
```

## What NOT to Do

- ❌ Don't write documentation in English - all content is in Portuguese (Brazil)
- ❌ Don't use generic code examples in docs - reference specific packages from the ecosystem
- ❌ Don't skip the "Erros Comuns" section for complex tools
- ❌ Don't create new .md files outside `docs/` unless for project meta
- ❌ Don't hardcode credentials - use `AutomationSettings.json` (git-ignored)
- ❌ Don't implement web scraping that violates Adrenaline.com.br's terms of service
- ❌ Don't use plural for Task classes - use `NavigationTask.cs`, not `NavigationTasks.cs`

## Implementation Guidelines

### Web Scraping Best Practices
- Use Playwright with reasonable delays between requests
- Respect robots.txt if present
- Implement retry logic with exponential backoff
- Log all scraping operations with Serilog + LoggingTask
- Handle rate limiting gracefully

### Database Design
- Store: article title, category, URL, publication date, content, scraped timestamp
- Use Docker for database (MySQL, PostgreSQL, or SQL Server)
- Implement proper indexing for queries by category/date
- MigrationTask handles all database operations

### GUI Requirements
- Allow users to start/stop scraping manually
- Display scraping progress and statistics
- Configure scraping interval for scheduled mode
- View recent scraped articles
- Export data to Excel/CSV

### Task Organization
- **NavigationTask**: Browser initialization + URL collection from categories
- **ExtractionTask**: Parse individual pages and extract structured data
- **MigrationTask**: Save extracted data to Docker database
- **LoggingTask**: Centralized exception logging (call in all try/catch blocks)

## Next Steps

Short-term priorities:
1. ✅ Documentation complete (17 guides)
2. ✅ Project structure created (Workflow/, Tasks/)
3. 🔄 Implement NavigationTask.cs
4. 🔄 Implement ExtractionTask.cs
5. 🔄 Implement MigrationTask.cs
6. 🔄 Implement LoggingTask.cs
7. 🔄 Set up Docker database
8. 🔄 Create GUI interface
9. 🔄 Add Quartz.NET scheduling
6. 🔄 Implement data export features
