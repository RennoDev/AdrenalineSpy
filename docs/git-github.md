# 🐙 Git e GitHub - Versionamento do AdrenalineSpy

## O que é

**Git:** Sistema de controle de versão distribuído que registra mudanças no código ao longo do tempo  
**GitHub:** Plataforma online para hospedar repositórios Git com recursos colaborativos

**Por que usar no AdrenalineSpy:**
- Versionar todo o desenvolvimento do projeto desde o início
- Backup automático do código na nuvem (GitHub)
- Histórico completo de mudanças para debugging e rollback
- Portfólio profissional demonstrando boas práticas de desenvolvimento
- Colaboração em equipe através de branches e Pull Requests

**Onde é usado no AdrenalineSpy:**
- Versionar arquivos: `.cs`, `.md`, `.csproj`, `.json` (exceto credenciais)
- Controlar diferentes versões do scraper (v1.0, v2.0, etc.)
- Documentar funcionalidades através de commits descritivos
- Backup da pasta `docs/` com todos os 17 guias
- Gerenciar features através de branches separadas

## Como Instalar

### 1. Instalar Git no Windows

```powershell
# Opção 1: Download direto
# Acesse: https://git-scm.com/download/win
# Baixe e execute o instalador

# Opção 2: Via winget (Windows Package Manager)
winget install --id Git.Git -e --source winget

# Opção 3: Via Chocolatey
choco install git

# Verificar instalação
git --version
# Saída esperada: git version 2.42.0.windows.1
```

### 2. Configuração Inicial Obrigatória

```powershell
# Configurar identidade global (aparece em todos os commits)
git config --global user.name "Seu Nome Completo"
git config --global user.email "seu.email@example.com"

# Configurações recomendadas para Windows
git config --global core.autocrlf true
git config --global core.editor "code --wait"  # VS Code como editor padrão
git config --global init.defaultBranch main

# Verificar todas as configurações
git config --list
git config --global --list
```

### 3. Configurar GitHub CLI (Opcional mas Recomendado)

```powershell
# Instalar GitHub CLI
winget install --id GitHub.cli

# Fazer login
gh auth login
# Siga as instruções interativas
```

## Implementar no AutomationSettings.json

Git não precisa de configurações no JSON - funciona diretamente via linha de comando e não se integra com a aplicação .NET.

## Implementar no Config.cs

Git não se integra com `Config.cs` - é uma ferramenta externa ao projeto .NET usado apenas para versionamento.

## Montar nas Tasks

### Estrutura de Versionamento do Projeto

```
AdrenalineSpy/
├── 📁 .git/                           # Pasta do Git (invisível)
├── 📄 .gitignore                      # Arquivos a ignorar
├── 📄 README.md                       # Documentação principal (versionado)
├── 📄 Program.cs                      # Código fonte (versionado)
├── 📄 Config.cs                       # Configurações (versionado)
├── 📄 AdrenalineSpy.csproj            # Projeto .NET (versionado)
├── 📄 AutomationSettings.example.json # Template público (versionado)
├── 🔒 AutomationSettings.json         # Credenciais reais (NÃO versionado)
├── 📁 Workflow/                       # Todas as Tasks (versionado)
│   ├── 📄 Workflow.cs
│   └── 📁 Tasks/
│       ├── 📄 NavigationTask.cs
│       ├── 📄 ExtractionTask.cs
│       ├── 📄 MigrationTask.cs
│       └── 📄 LoggingTask.cs
├── 📁 docs/                           # Documentação (versionado)
│   ├── 📄 index.md
│   ├── 📄 comecando.md
│   └── 📄 *.md (todos os 17 guias)
├── 🔒 logs/                           # Logs do Serilog (NÃO versionado)
├── 🔒 screenshots/                    # Capturas do Playwright (NÃO versionado)
├── 🔒 bin/                            # Build output (NÃO versionado)
└── 🔒 obj/                            # Build cache (NÃO versionado)
```

### Passo 1: Configuração Inicial do Repositório Local

#### 1.1 Inicializar Git na Pasta do Projeto

```powershell
# Navegar até o projeto AdrenalineSpy
cd C:\Users\lucas\OneDrive\Documentos\CsharpProjects\AdrenalineSpy

# Inicializar repositório Git local
git init

# Verificar status (deve mostrar arquivos não rastreados)
git status
```

#### 1.2 Criar .gitignore Essencial

Crie arquivo `.gitignore` na raiz do projeto:

```gitignore
# .NET Build Outputs
bin/
obj/
*.user
*.suo
.vs/
.vscode/

# Build Results
[Dd]ebug/
[Rr]elease/
x64/
x86/
[Bb]uild/

# NuGet Packages
*.nupkg
packages/
*.nuget.props
*.nuget.targets
project.lock.json

# Logs do Serilog (AdrenalineSpy específico)
logs/
*.log

# Capturas de tela do Playwright
screenshots/
downloads/
exports/

# Credenciais SENSÍVEIS - NUNCA versionar
AutomationSettings.json
appsettings.json
.env
connectionstrings.json
secrets.json

# Dados temporários
temp/
tmp/
cache/

# Playwright específico
.playwright/
test-results/

# OS específico
.DS_Store
Thumbs.db
desktop.ini

# Docker local (se houver)
.docker-compose.override.yml
docker-compose.override.yml
```

#### 1.3 Preparar Template de Configuração Segura

```powershell
# Criar template sem credenciais reais
cp AutomationSettings.json AutomationSettings.example.json

# Editar AutomationSettings.example.json
# Substituir valores reais por placeholders:
```

```json
{
  "Navegacao": {
    "UrlBase": "https://www.adrenaline.com.br",
    "TimeoutSegundos": 30,
    "DelayEntreRequests": 2000,
    "UserAgent": "SEU_USER_AGENT_AQUI"
  },
  "Database": {
    "TipoConexao": "MySQL",
    "ConnectionString": "Server=localhost;Database=adrenaline_db;Uid=SEU_USUARIO;Pwd=SUA_SENHA;"
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "Usuario": "seu.email@gmail.com",
    "Senha": "SUA_APP_PASSWORD_AQUI"
  },
  "Logging": {
    "NivelLog": "Information",
    "CaminhoArquivos": "logs/"
  }
}
```

#### 1.4 Primeiro Commit Local

```powershell
# Adicionar todos os arquivos permitidos pelo .gitignore
git add .

# Verificar o que será commitado
git status

# Fazer commit inicial com mensagem descritiva
git commit -m "feat: setup inicial do projeto AdrenalineSpy RPA

- Adiciona estrutura base do projeto .NET 9
- Configura documentação completa (17 guias)
- Implementa padrão Program → Workflow → Tasks
- Adiciona template de configuração segura
- Setup completo de .gitignore para segurança"

# Verificar histórico
git log --oneline
```

### Passo 2: Configuração do GitHub

#### 2.1 Criar Repositório no GitHub

1. **Acesse [github.com](https://github.com) e faça login**
2. **Clique em "New repository" (botão verde)**
3. **Preencha os dados:**
   - **Repository name:** `AdrenalineSpy`
   - **Description:** "🤖 Projeto RPA educacional em .NET 9 para scraping do Adrenaline.com.br - Universidade RPA completa"
   - **Visibility:** 
     - **Public:** Para portfólio profissional (recomendado)
     - **Private:** Se contém informações sensíveis
   - **⚠️ NÃO marque "Add a README file"** (já temos local)
   - **⚠️ NÃO escolha .gitignore** (já criamos customizado)
   - **License:** MIT ou Unlicense (para projetos educacionais)
4. **Clique "Create repository"**

#### 2.2 Configurar Autenticação GitHub

**Opção 1: Personal Access Token (Recomendado para HTTPS)**

```powershell
# 1. No GitHub: Avatar → Settings → Developer settings → Personal access tokens → Tokens (classic)
# 2. Generate new token (classic)
# 3. Configurar:
#    - Note: "AdrenalineSpy RPA Project"
#    - Expiration: 90 days (ou No expiration para uso pessoal)
#    - Select scopes: 
#      ✅ repo (Full control of private repositories)
#      ✅ workflow (Update GitHub Action workflows)
# 4. Generate token
# 5. ⚠️ COPIE O TOKEN IMEDIATAMENTE (só aparece uma vez!)
```

**Opção 2: SSH (Mais Seguro e Conveniente)**

```powershell
# 1. Gerar par de chaves SSH
ssh-keygen -t ed25519 -C "seu.email@example.com"
# Pressione Enter para usar local padrão (~/.ssh/id_ed25519)
# Digite passphrase opcional (recomendado para segurança extra)

# 2. Iniciar ssh-agent
Get-Service ssh-agent | Set-Service -StartupType Manual
Start-Service ssh-agent
ssh-add ~/.ssh/id_ed25519

# 3. Copiar chave pública
Get-Content ~/.ssh/id_ed25519.pub | Set-Clipboard

# 4. Adicionar no GitHub:
#    Avatar → Settings → SSH and GPG keys → New SSH key
#    Title: "AdrenalineSpy Development"
#    Key: Ctrl+V (colar chave)
#    Add SSH key

# 5. Testar conexão
ssh -T git@github.com
# Resposta esperada: "Hi username! You've successfully authenticated..."
```

#### 2.3 Conectar Repositório Local ao GitHub

**Para HTTPS (usando Personal Access Token):**

```powershell
# Adicionar remote origin
git remote add origin https://github.com/SEU_USUARIO/AdrenalineSpy.git

# Renomear branch principal para 'main' (padrão atual)
git branch -M main

# Enviar código para GitHub
git push -u origin main
# Digite seu username do GitHub
# Digite o Personal Access Token como senha
```

**Para SSH (mais conveniente após configuração):**

```powershell
# Adicionar remote origin
git remote add origin git@github.com:SEU_USUARIO/AdrenalineSpy.git

# Renomear branch para main
git branch -M main

# Enviar código
git push -u origin main
```

### Passo 3: Workflow com Branches e Pull Requests

#### 3.1 Entendendo o Fluxo de Branches

```
main branch (production)
├── 📁 feature/playwright-navigation    # Nova funcionalidade
├── 📁 feature/serilog-logging         # Outra funcionalidade
├── 📁 bugfix/config-loading-error     # Correção de bug
└── 📁 hotfix/critical-security-fix    # Correção urgente
```

#### 3.2 Criando Branch para Nova Funcionalidade

```powershell
# 1. Verificar branch atual
git branch
git status

# 2. Atualizar main local
git checkout main
git pull origin main

# 3. Criar nova branch para funcionalidade específica
git checkout -b feature/playwright-navigation

# 4. Verificar que mudou de branch
git branch
# * feature/playwright-navigation  ← branch ativa
#   main
```

#### 3.3 Desenvolvendo na Branch

```powershell
# Exemplo: implementando NavigationTask.cs
# Edite os arquivos normalmente...

# Verificar mudanças
git status
git diff

# Adicionar mudanças
git add Workflow/Tasks/NavigationTask.cs
git add Config.cs  # se modificado

# Commit com mensagem descritiva
git commit -m "feat(playwright): implementa NavigationTask para scraping Adrenaline

- Adiciona classe NavigationTask.cs com métodos de navegação
- Implementa retry automático em caso de timeout
- Configura delays entre requisições para evitar rate limiting
- Integra com Config.Navegacao para configurações dinâmicas"

# Continuar desenvolvendo...
git add .
git commit -m "test(navigation): adiciona validação de URL antes do scraping"
```

#### 3.4 Enviando Branch para GitHub

```powershell
# Enviar branch para GitHub pela primeira vez
git push -u origin feature/playwright-navigation

# Próximos pushes na mesma branch
git push
```

#### 3.5 Criando Pull Request no GitHub

1. **Acesse seu repositório no GitHub**
2. **Verá banner: "Compare & pull request"** → Clique
3. **Ou manualmente:** Aba "Pull requests" → "New pull request"
4. **Configurar o PR:**
   - **Base:** `main` ← **Compare:** `feature/playwright-navigation`
   - **Title:** "feat(playwright): implementa NavigationTask completa"
   - **Description:**
   ```markdown
   ## 🎯 O que faz
   
   Implementa a classe `NavigationTask.cs` para navegação automatizada no site Adrenaline.com.br
   
   ## ✅ Funcionalidades
   
   - [x] Navegação segura com retry automático
   - [x] Delays configuráveis entre requisições
   - [x] Validação de URL antes do scraping
   - [x] Integração com Config.Navegacao
   - [x] Logging detalhado de erros
   
   ## 🧪 Como testar
   
   ```powershell
   dotnet run
   # Verificar logs em logs/navigation-{data}.txt
   ```
   
   ## 📋 Checklist
   
   - [x] Código testado localmente
   - [x] Documentação atualizada
   - [x] Sem credenciais hardcoded
   - [x] Seguiu padrões do projeto
   ```

5. **Configurações adicionais:**
   - **Reviewers:** Adicionar colegas (se em equipe)
   - **Assignees:** Se responsável pelo merge
   - **Labels:** `enhancement`, `playwright`, `rpa`
   - **Projects:** Adicionar a projetos se usar

6. **Clique "Create pull request"**

#### 3.6 Processo de Review e Merge

**Como Reviewer:**
1. **Aba "Files changed"** → Revisar código linha por linha
2. **Adicionar comentários:** Clique na linha → Add comment
3. **Sugestões de mudança:** Use "Insert a suggestion"
4. **Aprovar ou Solicitar mudanças:** "Review changes" → Approve/Request changes

**Como Autor após Review:**
```powershell
# Se foram solicitadas mudanças
git checkout feature/playwright-navigation

# Fazer as correções solicitadas
# ... editar arquivos ...

git add .
git commit -m "fix(review): corrige timeout e adiciona validação extra"
git push  # Push automático para a mesma branch atualiza o PR
```

**Fazer Merge do Pull Request:**
1. **No GitHub, na página do PR**
2. **Escolher tipo de merge:**
   - **"Create a merge commit"** → Mantém histórico completo
   - **"Squash and merge"** → Combina commits em um só (recomendado)
   - **"Rebase and merge"** → Reaplica commits sem merge commit
3. **Clique "Merge pull request"**
4. **Confirme:** "Confirm merge"
5. **Delete branch:** "Delete branch" (limpar branches antigas)

#### 3.7 Atualizando Main Após Merge

```powershell
# Voltar para main local
git checkout main

# Baixar alterações do GitHub
git pull origin main

# Deletar branch local (já mergeada)
git branch -d feature/playwright-navigation

# Verificar que está atualizado
git log --oneline -5
```

### Passo 4: Padrões de Branches para AdrenalineSpy

#### 4.1 Convenção de Nomes

```powershell
# Features (novas funcionalidades)
git checkout -b feature/excel-export           # Exportar para Excel
git checkout -b feature/email-notifications    # Envio por email
git checkout -b feature/gui-wpf                # Interface WPF
git checkout -b feature/docker-database        # Configuração Docker

# Bugfixes (correções)
git checkout -b bugfix/config-loading          # Erro no Config
git checkout -b bugfix/playwright-timeout      # Timeout do navegador

# Hotfixes (correções urgentes em produção)
git checkout -b hotfix/security-vulnerability  # Vulnerabilidade

# Documentação
git checkout -b docs/update-readme             # Atualizar README
git checkout -b docs/api-documentation         # Documentar APIs

# Chores (manutenção)
git checkout -b chore/update-dependencies      # Atualizar NuGet
git checkout -b chore/refactor-logging         # Refatorar logs
```

#### 4.2 Proteção da Branch Main

**Configurar no GitHub:**
1. **Repositório → Settings → Branches**
2. **Add rule → Branch name pattern:** `main`
3. **Configurações recomendadas:**
   - ✅ **Require a pull request before merging**
   - ✅ **Require approvals:** 1 (mínimo)
   - ✅ **Dismiss stale PR approvals when new commits are pushed**
   - ✅ **Require status checks to pass before merging**
   - ✅ **Require branches to be up to date before merging**
   - ✅ **Include administrators** (aplicar regras para todos)

## Métodos Mais Usados

### Comandos do Dia a Dia

```powershell
# ========================================
# WORKFLOW DIÁRIO COMPLETO
# ========================================

# 1. Ver situação atual do projeto
git status                              # Status dos arquivos
git branch                              # Branch atual
git log --oneline -5                    # Últimos 5 commits

# 2. Baixar atualizações do GitHub
git pull origin main                    # Atualizar main
git pull                                # Atualizar branch atual

# 3. Ver mudanças feitas
git diff                                # Diferenças não commitadas
git diff --staged                       # Diferenças já em staging
git diff main..feature/minha-branch     # Diferença entre branches

# 4. Adicionar arquivos ao commit
git add .                               # Todos os arquivos
git add *.cs                            # Só arquivos .cs
git add Workflow/Tasks/NavigationTask.cs # Arquivo específico
git add -p                              # Interativo (escolher pedaços)

# 5. Fazer commit com mensagem
git commit -m "feat(navigation): implementa retry em caso de erro"
git commit -m "fix(config): corrige carregamento do AutomationSettings"
git commit -m "docs(readme): atualiza instruções de instalação"

# 6. Enviar para GitHub
git push                                # Branch atual
git push origin feature/minha-feature   # Branch específica
git push -u origin feature/nova-branch  # Primeira vez da branch
```

### Padrão de Commit Messages (Conventional Commits)

```powershell
# FORMATO: <tipo>(<escopo>): <descrição>
#
# <tipo>:
#   feat     - Nova funcionalidade
#   fix      - Correção de bug
#   docs     - Documentação
#   style    - Formatação (sem mudar lógica)
#   refactor - Refatoração de código
#   test     - Testes
#   chore    - Manutenção

# EXEMPLOS ESPECÍFICOS PARA ADRENALINSPY:

# Playwright/Navegação
git commit -m "feat(playwright): adiciona navegação no site Adrenaline"
git commit -m "fix(navigation): corrige timeout em páginas lentas"
git commit -m "feat(scraping): implementa extração de títulos e categorias"

# Configuração
git commit -m "feat(config): adiciona suporte para múltiplos bancos de dados"
git commit -m "fix(config): corrige carregamento do AutomationSettings.json"

# Logging com Serilog  
git commit -m "feat(serilog): implementa LoggingTask centralizado"
git commit -m "fix(logging): corrige duplicação de logs"

# Database/ORM
git commit -m "feat(efcore): implementa MigrationTask para salvar dados"
git commit -m "feat(docker): adiciona configuração MySQL para desenvolvimento"

# Interface/GUI
git commit -m "feat(wpf): implementa interface para controlar scraping"
git commit -m "feat(gui): adiciona indicador de progresso visual"

# Documentação
git commit -m "docs(quickstart): atualiza tutorial de instalação"
git commit -m "docs(playwright): adiciona exemplos de navegação"

# Estrutura
git commit -m "refactor(workflow): reorganiza Tasks em pastas separadas"
git commit -m "chore(deps): atualiza pacotes NuGet para versões mais recentes"
```

### Trabalhando com Branches

```powershell
# ========================================
# GERENCIAMENTO DE BRANCHES
# ========================================

# Listar branches
git branch                              # Branches locais
git branch -r                           # Branches remotas
git branch -a                           # Todas as branches

# Criar e trocar branches
git checkout -b feature/email-reports   # Criar nova branch
git checkout main                       # Trocar para main
git checkout feature/excel-export       # Trocar para branch existente

# Atualizar branch com main
git checkout feature/minha-feature
git merge main                          # Merge main na feature
# OU (mais limpo):
git rebase main                         # Reaplica commits da feature sobre main

# Deletar branches
git branch -d feature/finalizada        # Local (só se já mergeada)
git branch -D feature/cancelada         # Forçar delete local
git push origin --delete feature/old    # Delete remota

# Ver diferenças entre branches
git diff main..feature/minha-branch     # Mudanças na branch
git log main..feature/minha-branch      # Commits únicos da branch
```

### Desfazer Mudanças (Comandos de Emergência)

```powershell
# ========================================
# COMANDOS DE EMERGÊNCIA - DESFAZER
# ========================================

# Desfazer mudanças não commitadas
git checkout -- arquivo.cs             # Arquivo específico
git checkout .                          # Todos os arquivos
git reset --hard HEAD                   # PERIGOSO: perde tudo não commitado

# Desfazer commits (mantendo mudanças)
git reset --soft HEAD~1                # Desfaz último commit, mantém staging
git reset HEAD~1                       # Desfaz commit e staging, mantém arquivos
git reset --hard HEAD~1                # PERIGOSO: desfaz tudo completamente

# Alterar mensagem do último commit
git commit --amend -m "Nova mensagem corrigida"

# Desfazer arquivo específico do staging
git reset HEAD arquivo.cs              # Remove do staging
git restore --staged arquivo.cs        # Comando moderno (Git 2.23+)

# Reverter commit específico (cria novo commit)
git revert abc1234                      # Reverte commit abc1234
git revert HEAD                         # Reverte último commit

# Ignorar arquivo já commitado
echo "logs/" >> .gitignore
git rm --cached logs/ -r                # Remove do tracking
git commit -m "chore: adiciona logs/ ao .gitignore"
```

### Comandos para Colaboração

```powershell
# ========================================
# TRABALHO EM EQUIPE
# ========================================

# Baixar todas as atualizações
git fetch origin                        # Baixa refs sem merge
git pull origin main                    # Baixa e faz merge

# Ver quem modificou cada linha (blame)
git blame Program.cs                    # Ver autores linha por linha
git blame -L 10,20 Config.cs            # Linhas específicas

# Buscar em histórico
git log --grep="playwright"             # Commits que mencionam playwright
git log --author="João Silva"           # Commits de um autor
git log --since="2 weeks ago"           # Commits das últimas 2 semanas
git log --oneline --graph --all         # Histórico visual de todas as branches

# Stash (salvar trabalho temporariamente)
git stash                               # Salva mudanças temporariamente
git stash save "WIP: implementando login"  # Com mensagem
git stash list                          # Ver stashes salvos
git stash pop                           # Recupera último stash
git stash apply stash@{1}               # Recupera stash específico
```

### Configuração Avançada e Troubleshooting

```powershell
# ========================================
# CONFIGURAÇÕES ÚTEIS
# ========================================

# Aliases úteis para Git
git config --global alias.st status
git config --global alias.co checkout
git config --global alias.br branch
git config --global alias.ci commit
git config --global alias.unstage 'reset HEAD --'

# Configurar editor padrão
git config --global core.editor "code --wait"     # VS Code
git config --global core.editor "notepad"         # Notepad

# Configurar merge tool
git config --global merge.tool vscode
git config --global mergetool.vscode.cmd 'code --wait $MERGED'

# ========================================
# TROUBLESHOOTING COMUM
# ========================================

# Erro: remote origin already exists
git remote remove origin
git remote add origin https://github.com/usuario/repo.git

# Erro: push rejected (updates were rejected)
git pull origin main --rebase           # Rebase local sobre remoto
git push origin main                     # Tentar push novamente

# Erro: merge conflicts
# 1. Abrir arquivos com <<<<<<< ======= >>>>>>>
# 2. Editar manualmente para resolver conflito
# 3. Remover marcadores de conflito
# 4. git add arquivo-resolvido.cs
# 5. git commit -m "resolve: conflitos de merge"

# Problema: commit com arquivo errado
git reset --soft HEAD~1                # Desfaz commit mas mantém staging
# Editar .gitignore, remover arquivos incorretos
git add .
git commit -m "Commit corrigido"

# Credenciais do Windows (problema de cache)
git config --global credential.helper manager-core
# Ou remover credencial cached:
cmdkey /delete:git:https://github.com
```

### Fluxo Completo: Feature → Pull Request → Merge

```powershell
# ========================================
# FLUXO COMPLETO DE UMA FUNCIONALIDADE
# ========================================

# 1. PREPARAÇÃO
git checkout main
git pull origin main
git checkout -b feature/excel-export

# 2. DESENVOLVIMENTO  
# ... editar arquivos ...
git add .
git commit -m "feat(excel): implementa ExcelExportTask com EPPlus"

# ... continuar desenvolvendo ...
git add Workflow/Tasks/ExcelExportTask.cs
git commit -m "test(excel): adiciona validação de dados antes export"

# 3. ENVIAR PARA GITHUB
git push -u origin feature/excel-export

# 4. CRIAR PULL REQUEST NO GITHUB
# - Ir ao repositório
# - "Compare & pull request"
# - Preencher título e descrição
# - Create pull request

# 5. APÓS APROVAÇÃO E MERGE
git checkout main
git pull origin main
git branch -d feature/excel-export

# 6. PRÓXIMA FUNCIONALIDADE
git checkout -b feature/email-notifications
# ... repetir processo ...
```

### Padrão de Segurança para Credenciais

```powershell
# ========================================
# NUNCA VERSIONAR CREDENCIAIS
# ========================================

# 1. Criar template público (uma vez só)
cp AutomationSettings.json AutomationSettings.example.json

# 2. Editar template - substituir por placeholders
# Exemplo AutomationSettings.example.json:
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=adrenaline;User=SEU_USUARIO;Password=SUA_SENHA;"
  },
  "Email": {
    "Usuario": "seu.email@gmail.com",
    "Senha": "SUA_APP_PASSWORD_AQUI"
  }
}

# 3. Garantir que real está no .gitignore
echo "AutomationSettings.json" >> .gitignore

# 4. Remover do tracking se já foi commitado
git rm --cached AutomationSettings.json
git add .gitignore
git commit -m "security: remove credenciais do versionamento"

# 5. Versionar apenas o template
git add AutomationSettings.example.json
git commit -m "docs: adiciona template de configuração"

# 6. Documentar no README.md
echo "## Configuração" >> README.md
echo "1. Copie AutomationSettings.example.json para AutomationSettings.json" >> README.md
echo "2. Preencha suas credenciais reais" >> README.md
```

---

## 🎯 Resumo Executivo para AdrenalineSpy

### Setup Inicial (Fazer Uma Vez)

```powershell
# 1. Instalar e configurar Git
git config --global user.name "Seu Nome"
git config --global user.email "seu@email.com"

# 2. Inicializar projeto
cd C:\Users\lucas\OneDrive\Documentos\CsharpProjects\AdrenalineSpy
git init
# Criar .gitignore (logs/, AutomationSettings.json, bin/, obj/)
git add .
git commit -m "feat: setup inicial do AdrenalineSpy RPA"

# 3. Conectar ao GitHub
git remote add origin https://github.com/SEU_USUARIO/AdrenalineSpy.git
git push -u origin main
```

### Workflow Diário

```powershell
# Sempre antes de começar
git pull origin main

# Criar feature
git checkout -b feature/nome-da-funcionalidade

# Desenvolver
# ... código ...
git add .
git commit -m "feat(escopo): descrição clara"
git push -u origin feature/nome-da-funcionalidade

# Pull Request no GitHub → Review → Merge

# Finalizar
git checkout main
git pull origin main
git branch -d feature/nome-da-funcionalidade
```

### Três Regras de Ouro

1. **🔒 NUNCA versionar credenciais** - Use AutomationSettings.example.json
2. **🌿 SEMPRE usar branches** - Nunca commitar direto na main  
3. **📝 SEMPRE mensagens descritivas** - "feat(playwright): implementa navegação" vs "correção"

**Resultado:** Projeto profissionalmente versionado, backup automático, histórico completo, portfólio demonstrando boas práticas! 🚀
