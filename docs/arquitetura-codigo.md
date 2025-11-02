# 🏗️ Arquitetura de Código - Estrutura do AdrenalineSpy

## 📁 Estrutura de Pastas

```
AdrenalineSpy/
├── 📄 Program.cs                    # Entry point da aplicação
├── 📄 Config.cs                     # Configurações centralizadas (Singleton)
├── 📄 AutomationSettings.json       # 🔐 Credenciais reais (git-ignored)
├── 📄 AutomationSettings.example.json # 📋 Template para outros devs
├── 📄 Playwright.cs                 # Helper estático do Playwright
├── 📄 GlobalUsings.cs              # Usings globais do projeto
├── 📄 AdrenalineSpy.csproj         # Configuração do projeto .NET
│
├── 📁 Workflow/                     # Pasta do workflow único
│   ├── 📄 Workflow.cs              # Orquestrador principal
│   └── 📁 Tasks/                   # Tasks específicas do projeto
│       ├── 📄 NavigationTask.cs    # Navegação no Adrenaline.com.br
│       ├── 📄 ExtractionTask.cs    # Extração de dados das páginas
│       ├── 📄 MigrationTask.cs     # Migração para banco Docker
│       ├── 📄 LoggingTask.cs       # Helper centralizado de logs
│       └── 📄 NavigationGoogle.cs  # Navegação alternativa (Google)
│
├── 📁 logs/                        # 📝 Logs gerados automaticamente
│   ├── 📁 sucesso/                 # Logs de execuções bem-sucedidas
│   └── 📁 falha/                   # Logs de execuções com erro
│
├── 📁 bin/                         # Arquivos compilados (git-ignored)
├── 📁 obj/                         # Cache de compilação (git-ignored)
└── 📁 .git/                        # Controle de versão Git
```

## 🔄 Fluxograma de Execução

```mermaid
graph TD
    A[Program.cs] --> B{Config.Instancia}
    B --> C[Config.Validar()]
    C --> D[LoggingTask.ConfigurarLogger()]
    D --> E[new Workflow()]
    
    E --> F[Workflow.ExecutarWorkflowCompleto()]
    
    F --> G[NavigationTask.InicializarNavegador()]
    G --> H[NavigationTask.ColetarUrlsCategoria()]
    H --> I[ExtractionTask.ExtrairDadosNoticias()]
    I --> J[MigrationTask.SalvarNoticias()]
    
    J --> K[✅ Sucesso]
    
    B --> L[❌ Config Inválida]
    G --> M[❌ Erro Navegação]
    I --> N[❌ Erro Extração]
    J --> O[❌ Erro Banco]
    
    L --> P[LoggingTask.RegistrarErro()]
    M --> P
    N --> P
    O --> P
    
    P --> Q[Program.cs trata erro fatal]
```

## 📊 Fluxo de Dados

```
1. Program.cs
   ├── 📥 Carrega AutomationSettings.json → Config.Instancia
   ├── 🔧 Configura Serilog via LoggingTask
   └── 🚀 Inicia Workflow

2. Workflow.cs
   ├── 📋 Obtém categorias via Config.Instancia.Categorias
   ├── 🌐 NavigationTask: Navega e coleta URLs
   ├── 📊 ExtractionTask: Extrai dados estruturados
   └── 💾 MigrationTask: Salva no banco Docker

3. Tasks Individuais
   ├── 📝 Todas usam LoggingTask para logs centralizados
   ├── ⚙️ Todas acessam Config.Instancia para configurações
   └── 🔄 Propagam erros para Workflow tratar
```

## 🎯 Responsabilidades por Arquivo

### Entry Point
- **Program.cs**: Inicialização, tratamento de erros fatais, coordenação geral

### Configurações
- **Config.cs**: Padrão Singleton, carregamento de AutomationSettings.json
- **AutomationSettings.json**: Credenciais e configurações (não versionado)

### Orchestração
- **Workflow/Workflow.cs**: Coordena Tasks na sequência correta

### Automação Específica
- **NavigationTask.cs**: Playwright, navegação no site Adrenaline
- **ExtractionTask.cs**: Parsing de HTML, extração de dados
- **MigrationTask.cs**: Entity Framework/Dapper, salvamento no banco
- **LoggingTask.cs**: Helper estático, centralização de logs

### Suporte
- **Playwright.cs**: Configurações estáticas do Playwright
- **logs/**: Serilog gera automaticamente separado por sucesso/falha

## ⚡ Fluxo Típico de Execução

```
🚀 Início
├── Config carregado uma vez (Singleton)
├── Logger configurado para sucesso/falha  
├── Workflow iniciado

🔄 Loop Principal
├── Para cada categoria configurada:
│   ├── NavigationTask navega na categoria
│   ├── ExtractionTask extrai dados de cada URL
│   └── Rate limiting aplicado entre requests
└── MigrationTask salva tudo no banco

✅ Finalização
├── Recursos liberados (browser fechado)
├── Logs finalizados
└── Aplicação encerrada
```

## 🛡️ Tratamento de Erros

```
Program.cs
└── try/catch FATAL
    └── Workflow.cs  
        └── try/catch PROCESSO
            └── NavigationTask.cs
                └── try/catch NAVEGAÇÃO
            └── ExtractionTask.cs  
                └── try/catch EXTRAÇÃO
            └── MigrationTask.cs
                └── try/catch BANCO
            
🔄 Todos os erros → LoggingTask.RegistrarErro()
📝 Logs separados automaticamente: sucesso/ e falha/
```

## 📋 Arquivos de Configuração

### .gitignore
```
# Credenciais (não versionar)
AutomationSettings.json

# Logs (não versionar)  
logs/

# Compilação .NET (não versionar)
bin/
obj/
```

### Versionados no Git
- `AutomationSettings.example.json` (template)
- Todos os `.cs` (código fonte)
- `AdrenalineSpy.csproj` (configuração do projeto)
- `README.md` e `docs/` (documentação)