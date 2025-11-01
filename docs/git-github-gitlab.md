# Git, GitHub e GitLab - Guia Completo

## 📋 Índice

1. [Preparando o Git Local](#preparando-o-git-local)
2. [Conectando ao GitHub](#conectando-ao-github)
3. [Conectando ao GitLab](#conectando-ao-gitlab)
4. [Fluxo de Trabalho Comum](#fluxo-de-trabalho-comum)
5. [Branches e Estratégias](#branches-e-estratégias)
6. [Boas Práticas](#boas-práticas)
7. [Troubleshooting](#troubleshooting)

---

## 🎯 Preparando o Git Local

### Instalação do Git

**Windows:**
```powershell
# Baixe e instale de: https://git-scm.com/download/win
# Ou usando winget:
winget install --id Git.Git -e --source winget
```

**Verificar instalação:**
```bash
git --version
```

### Configuração Inicial

Configure seu nome e email (importantes para os commits):

```bash
# Configurar nome
git config --global user.name "Seu Nome"

# Configurar email
git config --global user.email "seu.email@example.com"

# Verificar configurações
git config --list
```

### Inicializar Repositório Local

No diretório do seu projeto:

```bash
# Navegar até o projeto
cd C:\Users\lucas\OneDrive\Documentos\CsharpProjects\AdrenalineSpy

# Inicializar Git
git init

# Verificar status
git status
```

### Criar .gitignore

Crie um arquivo `.gitignore` na raiz do projeto:

```gitignore
# .NET
bin/
obj/
*.user
*.suo
.vs/

# Build results
[Dd]ebug/
[Rr]elease/
x64/
x86/

# NuGet Packages
*.nupkg
*.nuget.props
*.nuget.targets
project.lock.json
artifacts/

# Logs
logs/
*.log

# OS
.DS_Store
Thumbs.db

# Configurações e Credenciais
AutomationSettings.json
appsettings.json
appsettings.Development.json
.env
*.db
*.sqlite

# Secrets
secrets.json
.env

# Playwright
.playwright/

# Outputs
screenshots/
downloads/
output/
data/
```

### Primeiro Commit

```bash
# Adicionar todos os arquivos
git add .

# Verificar o que será commitado
git status

# Fazer o commit inicial
git commit -m "Initial commit: RPA project with documentation"

# Ver histórico
git log --oneline
```

---

## 🐙 Conectando ao GitHub

### Criar Repositório no GitHub

1. Acesse [github.com](https://github.com)
2. Faça login na sua conta
3. Clique em **"New repository"** ou no ícone **"+"** → **"New repository"**
4. Preencha:
   - **Repository name:** `AdrenalineSpy`
   - **Description:** "Projeto RPA em .NET para automação de processos"
   - **Visibility:** Public ou Private
   - ⚠️ **NÃO** marque "Add a README file" (já temos localmente)
   - ⚠️ **NÃO** adicione .gitignore nem license
5. Clique em **"Create repository"**

### Autenticação no GitHub

#### Opção 1: Personal Access Token (Recomendado)

```bash
# 1. Gerar token no GitHub:
# Settings → Developer settings → Personal access tokens → Tokens (classic) → Generate new token

# 2. Selecione os escopos:
# - repo (full control)
# - workflow (se usar GitHub Actions)

# 3. Copie o token gerado (você só verá uma vez!)

# 4. Use o token como senha ao fazer push
```

#### Opção 2: SSH (Mais Seguro)

```bash
# 1. Gerar chave SSH
ssh-keygen -t ed25519 -C "seu.email@example.com"

# Pressione Enter para aceitar o local padrão
# Digite uma senha (opcional)

# 2. Copiar a chave pública
cat ~/.ssh/id_ed25519.pub

# No Windows PowerShell:
Get-Content ~/.ssh/id_ed25519.pub | clip

# 3. Adicionar no GitHub:
# Settings → SSH and GPG keys → New SSH key
# Cole a chave e dê um título

# 4. Testar conexão
ssh -T git@github.com
```

### Conectar Repositório Local ao GitHub

#### Se estiver usando HTTPS:

```bash
# Adicionar remote
git remote add origin https://github.com/SEU_USUARIO/AdrenalineSpy.git

# Verificar
git remote -v

# Enviar código
git branch -M main
git push -u origin main
```

#### Se estiver usando SSH:

```bash
# Adicionar remote
git remote add origin git@github.com:SEU_USUARIO/AdrenalineSpy.git

# Verificar
git remote -v

# Enviar código
git branch -M main
git push -u origin main
```

### Atualizar README no GitHub

Após o primeiro push, seu README.md já estará visível na página principal do repositório.

---

## 🦊 Conectando ao GitLab

### Criar Repositório no GitLab

1. Acesse [gitlab.com](https://gitlab.com)
2. Faça login na sua conta
3. Clique em **"New project"** → **"Create blank project"**
4. Preencha:
   - **Project name:** `AdrenalineSpy`
   - **Project URL:** Escolha o namespace
   - **Visibility Level:** Private, Internal ou Public
   - ⚠️ **DESMARQUE** "Initialize repository with a README"
5. Clique em **"Create project"**

### Autenticação no GitLab

#### Opção 1: Personal Access Token

```bash
# 1. Gerar token no GitLab:
# User Settings → Access Tokens → Add new token

# 2. Preencha:
# - Token name: "AdrenalineSpy Development"
# - Expiration date: (escolha uma data)
# - Select scopes:
#   ✅ api
#   ✅ read_repository
#   ✅ write_repository

# 3. Clique em "Create personal access token"

# 4. Copie o token (você só verá uma vez!)

# 5. Use o token como senha ao fazer push
```

#### Opção 2: SSH

```bash
# 1. Gerar chave SSH (se ainda não tiver)
ssh-keygen -t ed25519 -C "seu.email@example.com"

# 2. Copiar a chave pública
cat ~/.ssh/id_ed25519.pub

# No Windows PowerShell:
Get-Content ~/.ssh/id_ed25519.pub | clip

# 3. Adicionar no GitLab:
# User Settings → SSH Keys → Add new key
# Cole a chave e dê um título

# 4. Testar conexão
ssh -T git@gitlab.com
```

### Conectar Repositório Local ao GitLab

#### Se estiver usando HTTPS:

```bash
# Adicionar remote
git remote add origin https://gitlab.com/SEU_USUARIO/AdrenalineSpy.git

# Verificar
git remote -v

# Enviar código
git branch -M main
git push -u origin main
```

#### Se estiver usando SSH:

```bash
# Adicionar remote
git remote add origin git@gitlab.com:SEU_USUARIO/AdrenalineSpy.git

# Verificar
git remote -v

# Enviar código
git branch -M main
git push -u origin main
```

---

## 🔄 Fluxo de Trabalho Comum

### Dia a Dia com Git

```bash
# 1. Ver status atual
git status

# 2. Ver mudanças não commitadas
git diff

# 3. Adicionar arquivos modificados
git add .
# Ou arquivos específicos:
git add Program.cs docs/playwright.md

# 4. Commitar mudanças
git commit -m "feat: adiciona automação de login"

# 5. Enviar para o repositório remoto
git push

# 6. Baixar mudanças do remoto
git pull
```

### Trabalhando com Múltiplos Remotes

É possível ter GitHub e GitLab simultaneamente:

```bash
# Adicionar GitHub
git remote add github https://github.com/SEU_USUARIO/AdrenalineSpy.git

# Adicionar GitLab
git remote add gitlab https://gitlab.com/SEU_USUARIO/AdrenalineSpy.git

# Ver todos os remotes
git remote -v

# Push para GitHub
git push github main

# Push para GitLab
git push gitlab main

# Push para ambos ao mesmo tempo
git push github main && git push gitlab main
```

### Configurar Push para Múltiplos Remotes

```bash
# Configurar origin para fazer push em ambos
git remote set-url --add --push origin https://github.com/SEU_USUARIO/AdrenalineSpy.git
git remote set-url --add --push origin https://gitlab.com/SEU_USUARIO/AdrenalineSpy.git

# Agora git push enviará para ambos!
git push origin main
```

---

## 🌿 Branches e Estratégias

### Criando e Usando Branches

```bash
# Criar nova branch
git branch feature/nova-automacao

# Mudar para a branch
git checkout feature/nova-automacao

# Criar e mudar em um comando
git checkout -b feature/nova-automacao

# Ver todas as branches
git branch -a

# Deletar branch local
git branch -d feature/nome-branch

# Deletar branch remota
git push origin --delete feature/nome-branch
```

### Git Flow Simplificado

```bash
# Branch principal (produção)
main

# Branch de desenvolvimento
git checkout -b develop

# Feature branch
git checkout -b feature/login-automation develop

# Depois de concluir a feature:
git checkout develop
git merge feature/login-automation
git branch -d feature/login-automation

# Quando estiver pronto para produção:
git checkout main
git merge develop
git push origin main
```

### Estratégia Recomendada para RPA

```
main (produção)
├── develop (desenvolvimento)
│   ├── feature/excel-automation
│   ├── feature/email-notification
│   └── feature/database-integration
└── hotfix/critical-bug
```

---

## ✅ Boas Práticas

### Mensagens de Commit

Use o padrão **Conventional Commits**:

```bash
# Formato
<tipo>(<escopo>): <descrição>

# Tipos comuns:
feat:     # Nova funcionalidade
fix:      # Correção de bug
docs:     # Documentação
style:    # Formatação, ponto e vírgula, etc
refactor: # Refatoração de código
test:     # Adicionar testes
chore:    # Manutenção, dependências

# Exemplos:
git commit -m "feat(playwright): adiciona automação de login"
git commit -m "fix(excel): corrige leitura de células vazias"
git commit -m "docs(readme): atualiza instruções de instalação"
git commit -m "refactor(database): melhora performance de queries"
```

### Commits Frequentes

```bash
# ✅ BOM: Commits pequenos e focados
git commit -m "feat(login): adiciona validação de credenciais"
git commit -m "feat(login): adiciona tratamento de erro"
git commit -m "test(login): adiciona testes unitários"

# ❌ RUIM: Commit gigante
git commit -m "adiciona tudo"
```

### .gitignore Completo

Mantenha seu `.gitignore` atualizado:

```gitignore
# Nunca commite:
- Senhas e credenciais
- Tokens de API
- Arquivos de configuração local
- Dados sensíveis
- Arquivos grandes (>100MB)
- Dependências (restauráveis)
```

### Proteção de Branches

**No GitHub/GitLab:**
- Settings → Branches → Branch protection rules
- Proteja a branch `main`:
  - ✅ Require pull request reviews
  - ✅ Require status checks to pass
  - ✅ Require branches to be up to date

---

## 🔧 Troubleshooting

### Erro: Remote origin already exists

```bash
# Remover remote existente
git remote remove origin

# Adicionar novo remote
git remote add origin https://github.com/SEU_USUARIO/AdrenalineSpy.git
```

### Erro: Push rejected

```bash
# Se o remoto tem commits que você não tem localmente
git pull origin main --rebase
git push origin main
```

### Desfazer Último Commit (não enviado)

```bash
# Manter as mudanças
git reset --soft HEAD~1

# Descartar as mudanças
git reset --hard HEAD~1
```

### Desfazer Mudanças Não Commitadas

```bash
# Descartar mudanças em arquivo específico
git checkout -- Program.cs

# Descartar todas as mudanças
git reset --hard HEAD
```

### Alterar Mensagem do Último Commit

```bash
git commit --amend -m "Nova mensagem"

# Se já fez push:
git push --force-with-lease origin main
```

### Resolver Conflitos de Merge

```bash
# 1. Tentar fazer pull
git pull origin main

# 2. Se houver conflitos, edite os arquivos marcados
# Procure por:
<<<<<<< HEAD
seu código
=======
código do remoto
>>>>>>> branch-name

# 3. Escolha qual versão manter ou combine

# 4. Marque como resolvido
git add arquivo-resolvido.cs

# 5. Complete o merge
git commit -m "Resolve merge conflicts"
```

### Ver Histórico Detalhado

```bash
# Log visual
git log --oneline --graph --all --decorate

# Ver mudanças de um commit específico
git show <commit-hash>

# Ver quem mudou cada linha
git blame Program.cs
```

### Ignorar Arquivo Já Commitado

```bash
# 1. Adicione ao .gitignore
echo "AutomationSettings.json" >> .gitignore

# 2. Remova do tracking (mas mantenha localmente)
git rm --cached AutomationSettings.json

# 3. Commit
git commit -m "chore: remove credenciais do tracking"
```

### Gerenciar Credenciais com Arquivos de Exemplo

**Problema:** Como compartilhar estrutura de configuração sem expor credenciais?

**Solução:** Pattern `*.example.*`

```bash
# 1. Criar arquivo de exemplo (versionado)
cp AutomationSettings.json AutomationSettings.example.json

# 2. Editar AutomationSettings.example.json - substituir valores reais por placeholders
# Exemplo:
# "Usuario": "seu_usuario_aqui"
# "Senha": "sua_senha_aqui"

# 3. Adicionar AutomationSettings.json ao .gitignore
echo "AutomationSettings.json" >> .gitignore

# 4. Remover AutomationSettings.json do tracking
git rm --cached AutomationSettings.json

# 5. Adicionar exemplo ao Git
git add AutomationSettings.example.json
git add .gitignore
git commit -m "docs: adiciona template de configuração"

# 6. Outros desenvolvedores copiam o exemplo:
cp AutomationSettings.example.json AutomationSettings.json
# Depois editam com credenciais reais
```

**README.md deve ter:**
```markdown
## Configuração

1. Copie o arquivo de exemplo:
   ```bash
   cp AutomationSettings.example.json AutomationSettings.json
   ```

2. Edite `AutomationSettings.json` com suas credenciais reais

3. Execute o projeto:
   ```bash
   dotnet run
   ```
```

### Autenticação HTTPS com Credenciais Salvas

**Windows:**
```bash
# Git Credential Manager
git config --global credential.helper wincred

# Ou usando manager-core
git config --global credential.helper manager-core
```

---

## 📊 Workflows Úteis

### Workflow 1: Feature Branch

```bash
# 1. Criar branch
git checkout -b feature/excel-report

# 2. Desenvolver
# ... fazer mudanças ...

# 3. Commit frequentes
git add .
git commit -m "feat(excel): adiciona geração de relatório"

# 4. Push da feature
git push -u origin feature/excel-report

# 5. Abrir Pull Request no GitHub/GitLab

# 6. Após aprovação, merge no GitHub/GitLab

# 7. Atualizar main local
git checkout main
git pull origin main

# 8. Deletar branch local
git branch -d feature/excel-report
```

### Workflow 2: Hotfix Urgente

```bash
# 1. A partir da main
git checkout main
git checkout -b hotfix/critical-bug

# 2. Corrigir bug
# ... fazer correção ...

# 3. Commit
git commit -am "fix: corrige erro crítico na automação"

# 4. Push
git push -u origin hotfix/critical-bug

# 5. Merge direto na main (após revisar)
git checkout main
git merge hotfix/critical-bug
git push origin main

# 6. Merge também no develop
git checkout develop
git merge hotfix/critical-bug
git push origin develop

# 7. Deletar hotfix
git branch -d hotfix/critical-bug
git push origin --delete hotfix/critical-bug
```

---

## 📚 Recursos Adicionais

### Comandos Úteis Rápidos

```bash
# Ver configuração
git config --list

# Clonar repositório
git clone https://github.com/usuario/repo.git

# Ver branches remotas
git branch -r

# Limpar branches deletadas remotamente
git fetch --prune

# Guardar mudanças temporariamente
git stash
git stash pop

# Ver diferenças antes de commitar
git diff --staged

# Histórico de um arquivo
git log --follow Program.cs

# Buscar em commits
git log --grep="login"

# Ver tags
git tag
git tag -a v1.0.0 -m "Versão 1.0.0"
git push origin v1.0.0
```

### Links Úteis

- **Git Documentation:** https://git-scm.com/doc
- **GitHub Docs:** https://docs.github.com
- **GitLab Docs:** https://docs.gitlab.com
- **Git Cheat Sheet:** https://education.github.com/git-cheat-sheet-education.pdf
- **Conventional Commits:** https://www.conventionalcommits.org
- **Git Flow:** https://nvie.com/posts/a-successful-git-branching-model/

### Ferramentas Visuais

- **GitHub Desktop:** https://desktop.github.com
- **GitKraken:** https://www.gitkraken.com
- **Sourcetree:** https://www.sourcetreeapp.com
- **VS Code Git Integration:** Embutido no VS Code

---

## 🎯 Checklist: Preparar Projeto para GitHub/GitLab

- [ ] Git instalado e configurado
- [ ] Repositório inicializado (`git init`)
- [ ] `.gitignore` criado e configurado
- [ ] README.md criado com descrição do projeto
- [ ] Primeiro commit realizado
- [ ] Repositório criado no GitHub/GitLab
- [ ] Autenticação configurada (Token ou SSH)
- [ ] Remote adicionado (`git remote add origin`)
- [ ] Código enviado (`git push -u origin main`)
- [ ] Repositório acessível online
- [ ] Branch protection configurada (opcional)
- [ ] CI/CD configurado (opcional, veja [deploy.md](deploy.md))

---

**Pronto!** Agora seu projeto está versionado e sincronizado com GitHub e/ou GitLab! 🚀

**Próximos passos:**
- Configure GitHub Actions ou GitLab CI/CD (veja [deploy.md](deploy.md))
- Adicione badges ao README
- Configure Issues e Projects
- Convide colaboradores

