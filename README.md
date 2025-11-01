# AdrenalineSpy - Projeto RPA em .NET

## 📋 Descrição

**AdrenalineSpy** é um projeto educacional de RPA (Robotic Process Automation) que demonstra na prática o uso de todas as ferramentas .NET documentadas neste repositório.

### O que ele faz:

- 🌐 **Web Scraping:** Acessa https://www.adrenaline.com.br/ e coleta notícias de tecnologia e jogos
- 💾 **Armazenamento:** Salva dados em banco de dados Docker (MySQL, PostgreSQL ou SQL Server)
- 🖥️ **Interface Gráfica:** Permite controle manual via GUI (WPF/WinForms/Avalonia)
- ⏰ **Agendamento:** Execução automática em intervalos configuráveis (Quartz.NET)
- 📊 **Exportação:** Gera relatórios em Excel/CSV dos dados coletados

### Objetivo:

Este é um projeto de **"universidade RPA .NET"** - uma aplicação real que utiliza as ferramentas documentadas, servindo como:
- ✅ Exemplo prático completo de RPA em .NET
- ✅ Referência para projetos futuros
- ✅ Validação da documentação técnica (17 guias completos)

## 🚀 Tecnologias Utilizadas

- **.NET 9.0** - Framework principal
- **Playwright** - Automação web (scraping do Adrenaline)
- **Entity Framework Core / Dapper** - Acesso ao banco de dados
- **Docker** - Banco de dados containerizado
- **Serilog** - Logging estruturado
- **Quartz.NET** - Agendamento de tarefas
- **WPF/WinForms/Avalonia** - Interface gráfica
- **EPPlus/CsvHelper** - Exportação de relatórios

## 📚 Documentação

**👉 [ÍNDICE DE FERRAMENTAS](docs/index.md)** - Referência rápida de todos os pacotes NuGet e quando usar cada um

Toda a documentação do projeto está na pasta `docs/`:

### 🚀 Para Iniciantes
- [**Quick Start**](docs/quickstart.md) - ⭐ **COMECE AQUI!** Seu primeiro RPA em 10 minutos
- [**Índice de Ferramentas**](docs/index.md) - Glossário completo: o que cada ferramenta faz e quando usar
- [**Arquitetura de Código**](docs/arquitetura-codigo.md) - Como organizar seu projeto (Main → Workflow → Tasks)
- [**Git, GitHub e GitLab**](docs/git-github-gitlab.md) - Como versionar seu projeto

### Automação
- [**Playwright**](docs/playwright.md) - Automação web
- [**FlaUI**](docs/flaui.md) - Automação desktop Windows
- [**InputSimulator**](docs/inputsimulator.md) - Simulação de teclado/mouse

### Manipulação de Arquivos
- [**EPPlus**](docs/epplus.md) - Planilhas Excel
- [**CsvHelper**](docs/csvhelper.md) - Arquivos CSV
- [**iText7**](docs/itext7.md) - Arquivos PDF

### APIs e Integração
- [**RestSharp e JSON**](docs/restsharp-json.md) - Consumo de APIs
- [**MailKit**](docs/mailkit.md) - Envio e recebimento de emails

### Infraestrutura
- [**Serilog**](docs/serilog.md) - Logging estruturado
- [**Quartz**](docs/quartz.md) - Agendamento de tarefas
- [**Docker**](docs/docker-setup.md) - Docker, WSL 2 e bancos de dados
- [**ORM**](docs/orm.md) - Entity Framework Core e Dapper

### Interface e Deploy
- [**GUI**](docs/gui.md) - Interfaces gráficas para automações
- [**Deploy**](docs/deploy.md) - Deploy gratuito para estudo

## 🛠️ Como Usar

### Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (para banco de dados)
- Windows 10/11 (recomendado)

### Instalação

```bash
# Clone o repositório
git clone https://github.com/seu-usuario/AdrenalineSpy.git
cd AdrenalineSpy

# Configurar credenciais
cp AutomationSettings.example.json AutomationSettings.json
# Edite AutomationSettings.json com suas credenciais reais

# Restaurar dependências
dotnet restore

# Instalar navegadores do Playwright
dotnet build
pwsh bin/Debug/net9.0/playwright.ps1 install

# Configurar banco de dados (Docker)
# Veja docs/docker-setup.md para instruções detalhadas
docker run -d --name adrenaline-db -e MYSQL_ROOT_PASSWORD=senha123 -p 3306:3306 mysql:latest

# Executar o projeto
dotnet run
```

### ⚠️ Configuração de Credenciais

O projeto usa `AutomationSettings.json` para configurações da automação (ignorado pelo Git). 

1. **Copie o template:**
   ```bash
   cp AutomationSettings.example.json AutomationSettings.json
   ```

2. **Edite `AutomationSettings.json` com seus dados:**
   - String de conexão do banco de dados
   - Credenciais de login (se necessário)
   - Configurações de scraping (delays, retries, headless mode)

3. **Nunca commite `AutomationSettings.json`** - ele já está no `.gitignore`

### Uso

**Modo GUI:**
- Execute o programa e controle manualmente pelo interface
- Inicie/pause scraping
- Configure intervalos
- Visualize estatísticas

**Modo Agendado:**
- Configure o intervalo desejado (ex: a cada 6 horas)
- O Quartz.NET executará automaticamente o scraping

## 📦 Pacotes NuGet Utilizados

Pacotes principais usados no projeto:

```bash
# Web Scraping
dotnet add package Microsoft.Playwright

# Banco de Dados
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer  # ou MySql.EntityFrameworkCore
# OU
dotnet add package Dapper

# Logging
dotnet add package Serilog
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File

# Agendamento
dotnet add package Quartz

# Interface Gráfica (escolha uma)
# WPF/WinForms já vem com .NET
dotnet add package Avalonia  # se preferir multiplataforma

# Exportação (opcional)
dotnet add package EPPlus
dotnet add package CsvHelper
```

Veja [docs/index.md](docs/index.md) para referência completa de todos os pacotes disponíveis.

## 🎯 Estrutura do Projeto

```
AdrenalineSpy/
├── docs/                       # 📚 Documentação completa de RPA em .NET
│   ├── index.md                # Índice de ferramentas (glossário)
│   ├── quickstart.md           # Tutorial de 10 minutos
│   ├── playwright.md           # Automação web
│   ├── serilog.md              # Logging
│   ├── quartz.md               # Agendamento
│   ├── orm.md                  # Entity Framework / Dapper
│   ├── docker-setup.md         # Docker e bancos de dados
│   ├── gui.md                  # Interfaces gráficas
│   └── ... (16 guias no total)
├── Program.cs                  # 🚀 Implementação do scraper
├── AdrenalineSpy.csproj        # Configuração do projeto
└── README.md                   # Este arquivo
```

## 📖 Começando

**Novo em RPA com .NET?**
1. 📖 Leia o [Quick Start](docs/quickstart.md) - seu primeiro RPA em 10 minutos
2. 📚 Consulte o [Índice de Ferramentas](docs/index.md) - descubra o que cada pacote faz
3. 🔍 Aprofunde nos guias específicos conforme necessário
4. 💻 Estude o código do AdrenalineSpy como referência prática

**Quer implementar algo similar?**
- Use este projeto como template
- A documentação cobre TODAS as ferramentas que você precisa
- Código real demonstrando boas práticas

## 🎓 Sobre o Projeto

Este é um projeto **educacional** ("universidade RPA .NET") que combina:
- ✅ **Documentação completa** de 16 ferramentas RPA
- ✅ **Implementação real** demonstrando todas elas
- ✅ **Código bem documentado** como referência
- ✅ **Boas práticas** de desenvolvimento

Ideal para:
- 🎯 Aprender RPA com .NET do zero
- 🎯 Referência para projetos futuros
- 🎯 Entender como integrar múltiplas ferramentas
- 🎯 Ver código real, não apenas tutoriais básicos

## 📝 Licença

Este é um projeto de estudo. Use como referência para seus próprios projetos.

**Atenção:** Respeite os termos de uso do site Adrenaline.com.br ao usar este projeto.

## 🤝 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para:

- Reportar bugs
- Sugerir melhorias
- Adicionar exemplos
- Melhorar a documentação

## 📧 Contato

Para dúvidas ou sugestões, abra uma issue no repositório.

---

**Desenvolvido com ❤️ para automação com .NET**

**Última atualização:** Novembro 2025
