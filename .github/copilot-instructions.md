# AdrenalineSpy - AI Agent Instructions

## Project Overview

**AdrenalineSpy** is a revolutionary educational RPA project that serves as a complete **"Universidade RPA .NET"** - teaching all major .NET RPA technologies through incremental Program.cs evolution.

**What it does:**
- **Primary Goal:** Educational platform demonstrating gradual Program.cs evolution from basic console to complete RPA system
- **Secondary Goal:** Web scraper collecting tech/gaming news from https://www.adrenaline.com.br/
- **Revolutionary Approach:** Each technology documentation shows exactly how to add to Program.cs incrementally
- **Complete Stack:** 17 technologies with perfect pedagogical integration order

**Target Framework:** .NET 9.0  
**Language:** Portuguese (Brazil) - all documentation is in pt-BR  
**Platform:** Windows (PowerShell commands)  
**Status:** 🎉 DOCUMENTATION REVOLUTION COMPLETED! Ready for implementation testing phase

## 🚀 MAJOR BREAKTHROUGH (November 2025)

### Revolutionary Documentation Philosophy Implemented
**BEFORE:** Each technology had complex standalone documentation  
**NOW:** Each technology shows **exactly how to evolve Program.cs** step-by-step

**BEFORE:** Program.cs was a complex monolithic file  
**NOW:** Program.cs starts simple and evolves naturally through documentation

## Architecture & Philosophy

### 🎓 "Universidade RPA .NET" - Revolutionary Pedagogical Approach
- **Core Innovation:** Each technology shows **incremental Program.cs evolution**
- **Perfect Learning Path:** Scientific order from `docs/comecando.md` ensures each step builds on previous
- **Complete Coverage:** 17 RPA technologies with seamless integration 
- **Zero to Hero:** From 5-line Program.cs to complete RPA system with GUI + Scheduler + APIs

### Documentation Structure Revolution (COMPLETED - November 2025)
All guides now follow **enhanced 7-section template** with Program.cs integration:

1. **O que é [Tecnologia]** - Brief intro + why use in AdrenalineSpy + where used in project
2. **Como Instalar** - NuGet packages + additional setup (e.g., Playwright motors)
3. **Implementar no AutomationSettings.json** - JSON section with all config options explained
4. **Implementar no Config.cs** - Show Config class integration or create dedicated class
5. **Montar nas Tasks** - Complete Task implementation with all methods
6. **🆕 Como Adicionar no Program.cs** - **REVOLUTIONARY SECTION** showing gradual evolution
7. **Métodos Mais Usados** - Essential methods with practical examples

### 🚀 Program.cs Evolution Philosophy
**Starting Point (5 lines):**
```csharp
Config config = Config.Instancia;
LoggingTask.ConfigurarLogger();
// TODO: Implementar funcionalidades
```

**Final Destination (Complete RPA System):**
```csharp
var modo = DetectarModoExecucao(args);
await ExecutarConforme(modo, config, args);
// Console | GUI | Scheduler | Service
```

**Key principles:**
- ✅ **Incremental Evolution:** Each technology adds specific functionality to Program.cs
- ✅ **Scientific Order:** Following `comecando.md` sequence for optimal learning
- ✅ **Complete Code:** 100% copy-paste ready, no pseudo-code or placeholders
- ✅ **Real Integration:** Every example works in actual AdrenalineSpy context
- ✅ **Multiple Modes:** Console → Scheduler → Service → GUI progression

### Code Architecture - Evolutionary Pattern
**Phase 1 (Basic):** Program.cs → Config.cs → LoggingTask  
**Phase 2 (Core):** + NavigationTask (Playwright) → ExtractionTask  
**Phase 3 (Data):** + MigrationTask (ORM) → ExportTask (Excel/CSV/PDF)  
**Phase 4 (Communication):** + EmailTask (MailKit) → NotificationTask  
**Phase 5 (Automation):** + QuartzScheduler → WindowsService  
**Phase 6 (Interface):** + GUI (WPF/Avalonia) → Complete RPA System  

### 🗂️ Key Files Structure
- **`README.md`**: Project showcase + gateway to all documentation
- **`docs/program.md`**: **ESSENTIAL** - Shows how to evolve Program.cs from basic to complete
- **`docs/comecando.md`**: **SCIENTIFIC ORDER** - Exact sequence for guaranteed success
- **`docs/index.md`**: Quick reference glossary for all 17 technologies
- **`AutomationSettings.example.json`**: Configuration template (versioned)
- **`AutomationSettings.json`**: Real credentials (git-ignored)
- **Current Status**: `feature/reinicio` branch - clean slate for testing "Universidade"
- **Workflow/Tasks/MigrationTask.cs**: Migrates data to Docker database
- **Workflow/Tasks/LoggingTask.cs**: Helper for centralized exception logging (called in try/catch)
- See `docs/arquitetura-codigo.md` for complete implementation guide

## Critical Knowledge

### Documentation Coverage (17 Guides) - Restructuring in Progress
**Getting Started:** Quick Start, Code Architecture (Main → Workflow → Tasks), Git/GitHub/GitLab  
**Automation:** Playwright ✅ (web), FlaUI (Windows desktop), InputSimulator (keyboard/mouse)  
**Files:** EPPlus (Excel), CsvHelper (CSV), iText7 (PDF)  
**Integration:** RestSharp (APIs), MailKit (email), Newtonsoft.Json  
**Infrastructure:** Serilog ✅ (logging), Quartz (scheduling)  
**Data:** Entity Framework Core + Dapper ✅ (ORM), Docker setup (MySQL, PostgreSQL, SQL Server)  
**UI:** WPF, WinForms, Avalonia, Terminal.Gui, Electron.NET  
**Production:** Deploy guide (GitHub Actions, Azure, Railway, local)

✅ = Restructured to new 7-section pattern with Program.cs integration

### Revolutionary Implementation Status (November 2025)
**COMPLETED TECHNOLOGIES WITH PROGRAM.CS INTEGRATION:**
- ✅ **4️⃣ RestSharp + JSON** - AutomationSettings.json automation + API integration
- ✅ **6️⃣ Serilog** - Centralized logging system with error/success separation
- ✅ **7️⃣ Playwright** - Web scraping core with NavigationTask evolution
- ✅ **8️⃣ ORM** - Database persistence with MigrationTask + health checks
- ✅ **🔟-1️⃣2️⃣ EPPlus/CSV/PDF** - Complete export system integration
- ✅ **1️⃣3️⃣ MailKit** - Intelligent notifications (errors, reports, scheduling)
- ✅ **1️⃣6️⃣ Quartz.NET** - Complete automation with scheduler + Windows Service
- ✅ **1️⃣7️⃣ GUI** - Final interface with automatic mode detection

**BRANCH STATUS:**
- **main**: Complete documentation with Program.cs evolution guides
- **feature/reinicio**: Clean implementation slate for testing the "Universidade"

**COMMIT HISTORY:**
- `b31b039` - Revolutionary Program.cs evolution documentation (2,871 lines added)
- All 17 technologies now show incremental Program.cs integration

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

### Documentation Coverage (17 Guides) - Restructuring in Progress
**Getting Started:** Quick Start, Code Architecture (Main → Workflow → Tasks), Git/GitHub/GitLab  
**Automation:** Playwright ✅ (web), FlaUI (Windows desktop), InputSimulator (keyboard/mouse)  
**Files:** EPPlus (Excel), CsvHelper (CSV), iText7 (PDF)  
**Integration:** RestSharp (APIs), MailKit (email), Newtonsoft.Json  
**Infrastructure:** Serilog ✅ (logging), Quartz (scheduling)  
**Data:** Entity Framework Core + Dapper ✅ (ORM), Docker setup (MySQL, PostgreSQL, SQL Server)  
**UI:** WPF, WinForms, Avalonia, Terminal.Gui, Electron.NET  
**Production:** Deploy guide (GitHub Actions, Azure, Railway, local)

✅ = Restructured to new 6-section pattern

### Special Documentation Notes
- **Serilog (DONE)**: Integrated with Config.Logging, LoggingTask.cs static helper, automatic success/failure log separation
- **Playwright (DONE)**: Requires `playwright.ps1 install` after package install, integrated with Config.Navegacao, NavigationTask.cs complete with retry logic
- **ORM (DONE)**: Both EF Core and Dapper patterns, integrated with Config.Database.ObterConnectionString(), MigrationTask.cs examples
- **EPPlus & iText7**: Include licensing warnings (commercial use restrictions)
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

### 🧪 TESTING PHASE - "Universidade RPA .NET" Validation
**CURRENT MISSION:** Test if the documentation revolution actually works in practice!

**Phase Status:**
1. ✅ **Documentation Revolution Complete** - 17 guides with Program.cs evolution
2. ✅ **Clean Slate Prepared** - `feature/reinicio` branch with fresh start
3. ⏳ **Implementation Testing** - Follow `docs/comecando.md` step-by-step
4. ⏳ **Validate Learning Path** - Ensure smooth progression from basic to complete RPA

**Testing Protocol:**
1. **Start Simple** - Create basic Program.cs (5 lines)
2. **Follow Order** - Implement technologies in exact `comecando.md` sequence
3. **Verify Integration** - Each step should build naturally on previous
4. **Document Issues** - Note any gaps or problems in the learning path
5. **Prove Concept** - Achieve complete RPA system through incremental evolution

**Success Criteria:**
- ✅ Developer can go from zero to complete RPA system
- ✅ Each documentation step works as written
- ✅ Program.cs evolves naturally without confusion
- ✅ Final result: GUI + Scheduler + APIs + Database + Reports

### 🎯 Implementation Order (From comecando.md)
1. **Configuração** → AutomationSettings.json + Config.cs
2. **Arquitetura** → Folder structure + patterns  
3. **Program.cs** → Basic entry point (THIS FILE!)
4. **RestSharp + JSON** → API foundation
5. **Git/GitHub** → Version control
6. **Serilog** → Logging system
7. **Playwright** → Web scraping core
8. **ORM** → Database persistence
9. **Docker** → Database infrastructure
10. **Export Tools** → Reports (Excel, CSV, PDF)
11. **MailKit** → Notifications
12. **Quartz** → Automation scheduler
13. **GUI** → User interface
14. **Deploy** → Production deployment

### � Validation Questions to Answer
- Does the documentation provide 100% working code?
- Is the learning progression natural and logical?
- Can a developer truly go from basic to advanced following the guides?
- Are there any missing pieces or confusing transitions?
- Does the final Program.cs achieve the promised functionality?

**This testing phase will prove whether AdrenalineSpy is truly a "Universidade RPA .NET" or needs further refinement!** 🚀
