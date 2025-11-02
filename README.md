# 🕷️ AdrenalineSpy

> **A Universidade RPA .NET em Ação** - Sistema completo de coleta automatizada de notícias do [Adrenaline.com.br](https://www.adrenaline.com.br) demonstrando **todas as ferramentas do ecossistema .NET** em um projeto real.

[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20Docker-blue.svg)](https://github.com/RennoDev/AdrenalineSpy)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Status](https://img.shields.io/badge/Status-Em%20Desenvolvimento-yellow.svg)](https://github.com/RennoDev/AdrenalineSpy/projects)

## 🎯 O Que é o AdrenalineSpy?

**AdrenalineSpy** é mais que um simples web scraper - é um **laboratório educacional completo** que demonstra como construir soluções RPA profissionais usando .NET 9.0. O projeto coleta notícias de tecnologia e games do Adrenaline.com.br enquanto ensina você a usar **17 tecnologias diferentes** em um cenário real.

### 🚀 Funcionalidades Principais

- 🌐 **Web Scraping Inteligente**: Coleta automática de notícias com Playwright
- 🗄️ **Armazenamento Robusto**: Banco de dados via Entity Framework Core + Dapper
- 📊 **Relatórios Profissionais**: CSV, Excel com gráficos, e PDFs executivos
- 🖼️ **Interface Gráfica**: Controle total via WPF com monitoramento em tempo real
- ⏰ **Agendamento Avançado**: Execução automática com Quartz.NET (3x por dia)
- 🐳 **Deploy Completo**: Docker, serviços Windows, nuvem (Railway/Azure)
- 🏥 **Monitoramento**: Health checks, logs estruturados, alertas automáticos

### 🎓 Objetivo Educacional

Este projeto serve como **"universidade RPA .NET"** demonstrando na prática:
- ✅ **Documentação Completa**: 17 guias detalhados com implementação real
- ✅ **Código de Produção**: Padrões profissionais, tratamento de erros, logs
- ✅ **Arquitetura Escalável**: Program → Workflow → Tasks → Config
- ✅ **Deploy Real**: Do desenvolvimento à produção 24/7

## 🛠️ Tecnologias Utilizadas

### 🌐 **Automação Web**
- **[Playwright](docs/playwright.md)** - Web scraping principal com retry inteligente
- **[FlaUI](docs/flaui.md)** - Backup desktop para automação Windows
- **[InputSimulator](docs/inputsimulator.md)** - Simulação humana de teclado/mouse

### 📊 **Dados e Relatórios**
- **[Entity Framework Core + Dapper](docs/orm.md)** - ORM dual para flexibilidade máxima
- **[CsvHelper](docs/csvhelper.md)** - Exportação CSV rápida e configurável
- **[EPPlus](docs/epplus.md)** - Relatórios Excel com gráficos e formatação
- **[iText7](docs/itext7.md)** - PDFs executivos profissionais

### 🔗 **Integração e Comunicação**
- **[RestSharp + Newtonsoft.Json](docs/restsharp-json.md)** - APIs REST e manipulação JSON
- **[MailKit](docs/mailkit.md)** - Notificações por email automáticas

### 🖥️ **Interface e Experiência**
- **[WPF + Avalonia + Terminal.Gui](docs/gui.md)** - Múltiplas opções de interface
- **[Serilog](docs/serilog.md)** - Logging estruturado e configurável

### ⚙️ **Infraestrutura e Deploy**
- **[Quartz.NET](docs/quartz.md)** - Agendamento de jobs com cron expressions
- **[Docker](docs/docker-setup.md)** - Containerização completa com MySQL
- **[Deploy Avançado](docs/deploy.md)** - GitHub Actions, Railway, serviços Windows

## 🚀 Quick Start

### Pré-requisitos
- [.NET 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (opcional)
- Windows 10+ (recomendado) ou Linux

### ⚡ Instalação Rápida (5 minutos)

```powershell
# 1. Clonar o repositório
git clone https://github.com/RennoDev/AdrenalineSpy.git
cd AdrenalineSpy

# 2. Restaurar dependências
dotnet restore

# 3. Configurar Playwright
dotnet build
pwsh bin/Debug/net9.0/playwright.ps1 install

# 4. Configurar banco Docker (opcional)
docker-compose up -d

# 5. Executar primeira coleta
dotnet run
```

**🎉 Pronto!** Em poucos minutos você terá notícias sendo coletadas e relatórios sendo gerados.

### 📋 Tutorial Completo
Para uma experiência guiada passo-a-passo, siga o **[📚 Quick Start Guide](docs/quickstart.md)** que te leva do zero ao funcionamento completo em 10 minutos.

## 📁 Estrutura do Projeto

```
AdrenalineSpy/
├── 📁 docs/                    # 📚 17 guias completos
│   ├── quickstart.md          # 🚀 Início rápido (10min)
│   ├── arquitetura-codigo.md  # 🏗️ Padrões de código
│   └── playwright.md          # 🌐 Web scraping
├── 📁 Workflow/               # 🔧 Lógica principal
│   ├── Workflow.cs           # 🎯 Orquestrador
│   └── Tasks/                # 📋 Tarefas específicas
│       ├── NavigationTask.cs # 🧭 Navegação web
│       ├── ExtractionTask.cs # 📊 Extração dados
│       ├── MigrationTask.cs  # 🗄️ Banco de dados
│       └── LoggingTask.cs    # 📝 Logs centralizados
├── Program.cs                # 🚪 Ponto de entrada
├── Config.cs                 # ⚙️ Configurações
└── AutomationSettings.json   # 🔐 Credenciais (gitignored)
```

## 🎮 Modos de Execução

O AdrenalineSpy oferece **3 modos de operação** para diferentes necessidades:

### 💻 **Modo Console** (Execução Única)
```powershell
dotnet run --console
```
- Executa uma coleta completa e finaliza
- Ideal para testes e execuções pontuais
- Gera relatórios automáticos se configurado

### 🖼️ **Modo Interface Gráfica** (Controle Manual)
```powershell
dotnet run  # Modo padrão se GUI habilitado
```
- Interface WPF completa com controles visuais
- Monitoramento em tempo real do progresso
- Botões para execução manual e geração de relatórios
- Visualização de logs e estatísticas

### ⏰ **Modo Agendador** (Produção 24/7)
```powershell
dotnet run --scheduler
```
- Execução contínua com agendamento automático
- Jobs pré-configurados (scraping 3x/dia, relatórios noturnos)
- Health check endpoint em `/health`
- Ideal para servidores e produção

## 📊 Exemplo de Saída

Após uma execução típica, o AdrenalineSpy gera:

### 📈 **Estatísticas**
```
🎯 Scraping Completo - Resumo da Execução
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📅 Data: 02/11/2025 14:30:15
🌐 Site: Adrenaline.com.br
📰 Notícias coletadas: 127
⏱️  Tempo total: 2m 34s
📊 Categorias: Hardware (45), Games (38), Mobile (28), Reviews (16)
```

### 📁 **Arquivos Gerados**
```
exports/
├── 📊 relatorio-adrenaline-2025-11-02.csv    # Dados tabulares
├── 📈 relatorio-adrenaline-2025-11-02.xlsx   # Excel com gráficos
└── 📄 relatorio-adrenaline-2025-11-02.pdf    # Relatório executivo
```

### 🗄️ **Banco de Dados**
- Notícias armazenadas com estrutura completa
- Histórico de execuções e estatísticas
- Índices otimizados para consultas rápidas

## 📚 Documentação Completa

### 🚀 **Primeiros Passos**
1. **[Quick Start](docs/quickstart.md)** - Tutorial de 10 minutos
2. **[Arquitetura do Código](docs/arquitetura-codigo.md)** - Padrões e organização
3. **[Git/GitHub/GitLab](docs/git-github-gitlab.md)** - Controle de versão

### 🌐 **Automação Web**
4. **[Playwright](docs/playwright.md)** - Web scraping principal
5. **[FlaUI](docs/flaui.md)** - Automação desktop Windows
6. **[InputSimulator](docs/inputsimulator.md)** - Simulação de input humano

### 📊 **Manipulação de Dados**
7. **[RestSharp + JSON](docs/restsharp-json.md)** - APIs e serialização
8. **[Entity Framework + Dapper](docs/orm.md)** - Banco de dados dual
9. **[CsvHelper](docs/csvhelper.md)** - Exportação CSV
10. **[EPPlus](docs/epplus.md)** - Relatórios Excel avançados
11. **[iText7](docs/itext7.md)** - PDFs profissionais

### 🔗 **Integração e Comunicação**
12. **[MailKit](docs/mailkit.md)** - Notificações por email
13. **[Serilog](docs/serilog.md)** - Logging estruturado

### 🖥️ **Interface e Deploy**
14. **[GUI](docs/gui.md)** - Interfaces gráficas (WPF/Avalonia)
15. **[Quartz.NET](docs/quartz.md)** - Agendamento de tarefas
16. **[Docker Setup](docs/docker-setup.md)** - Containerização
17. **[Deploy](docs/deploy.md)** - Produção completa

### 📖 **Referência Rápida**
- **[Índice Geral](docs/index.md)** - Glossário de todas as tecnologias

## 🎨 Exemplos de Uso

### 🔍 **Monitoramento de Tendências**
```csharp
// Agendar coleta a cada 4 horas
"0 0 */4 * * ?" // Cron expression

// Análise automática de tendências
var trending = noticias
    .GroupBy(n => n.Categoria)
    .Select(g => new { 
        Categoria = g.Key, 
        Crescimento = CalcularCrescimento(g) 
    })
    .OrderByDescending(x => x.Crescimento);
```

### 📈 **Relatórios Executivos**
- **Dashboard visual** com gráficos de distribuição por categoria
- **Alertas inteligentes** para picos de atividade
- **Comparativos temporais** (semanal, mensal)
- **Export personalizado** em múltiplos formatos

### 🔄 **Integração com Outros Sistemas**
```csharp
// Webhook para integração
await ApiTask.EnviarNoticia(new {
    titulo = noticia.Titulo,
    categoria = noticia.Categoria,
    timestamp = noticia.DataPublicacao
});

// Sincronização com CRM/ERP
await SincronizarComSistemaExterno(noticias);
```

## 🚢 Deploy em Produção

### 🐳 **Docker (Recomendado)**
```bash
# Stack completa com banco
docker-compose up -d

# Health check
curl http://localhost:8081/health
```

### 🚂 **Railway (Nuvem)**
```bash
railway login
railway init
railway deploy
```

### 🔧 **Serviço Windows**
```powershell
# Instalar como serviço
.\AdrenalineSpy.exe --install-service

# Verificar status
Get-Service "AdrenalineSpyService"
```

### 🚀 **CI/CD Automático**
- **GitHub Actions** configurado para build/test/deploy
- **Releases automáticos** com versionamento semântico
- **Health checks** integrados no pipeline

## 🤝 Contribuindo

### 💡 **Como Contribuir**
1. **Fork** este repositório
2. **Crie uma branch** para sua feature (`git checkout -b feature/nova-funcionalidade`)
3. **Commit suas mudanças** (`git commit -am 'Adiciona nova funcionalidade'`)
4. **Push para a branch** (`git push origin feature/nova-funcionalidade`)
5. **Abra um Pull Request**

### 🐛 **Reportar Bugs**
- Use as **[GitHub Issues](https://github.com/RennoDev/AdrenalineSpy/issues)**
- Inclua logs detalhados e steps para reproduzir
- Screenshots sempre ajudam!

### 📋 **Roadmap**
- [ ] **Machine Learning**: Análise de sentimento das notícias
- [ ] **API REST**: Endpoints para consulta externa
- [ ] **Mobile App**: Companion app para monitoramento
- [ ] **Integração IA**: Resumos automáticos com GPT
- [ ] **Multi-sites**: Suporte a outros portais de tecnologia

## ⚖️ Licença

Este projeto está licenciado sob a **MIT License** - veja o arquivo [LICENSE](LICENSE) para detalhes.

### 🚨 **Importante - Uso Ético**
- ✅ **Respeite** o robots.txt do site alvo
- ✅ **Use delays** apropriados entre requisições
- ✅ **Não sobrecarregue** os servidores
- ✅ **Fins educacionais** e de demonstração técnica

## 🏆 Reconhecimentos

### 💝 **Inspirações**
- **Adrenaline.com.br** - Excelente portal de tecnologia e games
- **Comunidade .NET** - Por manter um ecossistema incrível
- **Open Source** - Por tornar conhecimento acessível a todos

### 🛠️ **Tecnologias Utilizadas**
Agradecimentos especiais aos maintainers de todas as 17 bibliotecas que tornam este projeto possível!

---

## 🚀 **Comece Agora!**

```powershell
# Clone e execute em 3 comandos!
git clone https://github.com/RennoDev/AdrenalineSpy.git
cd AdrenalineSpy
dotnet run
```

**📚 Quer aprender RPA .NET?** Este projeto é seu **laboratório completo!**

**🤖 Precisa de automação profissional?** Use este código como **base sólida!**

**💼 Buscando referências de arquitetura?** Temos **padrões de produção!**

---

<div align="center">

### 🌟 **Se este projeto te ajudou, deixe uma estrela!** ⭐

**[📖 Documentação Completa](docs/README.md)** | **[🚀 Quick Start](docs/quickstart.md)** | **[🐛 Report Issues](https://github.com/RennoDev/AdrenalineSpy/issues)** | **[💬 Discussions](https://github.com/RennoDev/AdrenalineSpy/discussions)**

</div>
