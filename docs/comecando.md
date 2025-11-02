# 🎯 Começando - Ordem Correta para Sucesso em RPA

## 📋 Sequência Obrigatória

Siga esta ordem **exatamente** para garantir que cada etapa prepare a próxima:

### **1️⃣ [Configuração](configuracao.md)**
**Por que primeiro:** Sem AutomationSettings.json e Config.cs, nenhum código funciona.

### **2️⃣ [Arquitetura de Código](arquitetura-codigo.md)**
**Por que agora:** Define onde colocar cada código antes de escrever qualquer Task.

### **3️⃣ [RestSharp + JSON](restsharp-json.md)**
**Por que agora:** JSON é usado em AutomationSettings.json, Config.cs e todas as Tasks.

### **4️⃣ [Git e GitHub](git-github.md)**
**Por que agora:** Versionar desde o início evita perda de código.

### **5️⃣ [Serilog](serilog.md)**
**Por que agora:** RPA sem logs é impossível de debugar quando der erro, todas as etapas abaixo usarão logging.

### **6️⃣ [Playwright](playwright.md)**
**Por que agora:** É o coração do projeto - web scraping do Adrenaline.com.br.

### **7️⃣ [ORM](orm.md)**
**Por que agora:** Precisa salvar os dados extraídos pelo Playwright.

### **8️⃣ [Docker Setup](docker-setup.md)**
**Por que agora:** Banco de dados para o ORM funcionar.

---

## 🔧 Ferramentas Complementares (Ordem Flexível)

### **9️⃣ [EPPlus](epplus.md)** (Se precisar de Excel)
**Quando usar:** Relatórios e exportação de dados.

### **🔟 [CsvHelper](csvhelper.md)** (Se precisar de CSV)
**Quando usar:** Exportação simples de dados.

### **1️⃣1️⃣ [iText7](itext7.md)** (Se precisar de PDF)
**Quando usar:** Relatórios em PDF.

### **1️⃣2️⃣ [MailKit](mailkit.md)** (Se precisar de email)
**Quando usar:** Envio de relatórios por email.

### **1️⃣3️⃣ [InputSimulator](inputsimulator.md)** (Se precisar de teclado/mouse)
**Quando usar:** Automação que o Playwright não consegue fazer.

### **1️⃣4️⃣ [FlaUI](flaui.md)** (Se precisar de desktop)
**Quando usar:** Software Windows que não é web.

---

## 🚀 Finalização

### **1️⃣5️⃣ [Quartz](quartz.md)**
**Por que penúltimo:** Agendamento só faz sentido depois de tudo funcionando.

### **1️⃣6️⃣ [GUI](gui.md)** (Opcional)
**Quando fazer:** Interface para usuários finais.

### **1️⃣7️⃣ [Deploy](deploy.md)** ⭐ ÚLTIMO
**Por que último:** Só sobe para produção depois de tudo testado.

---

## ⚡ Resumo da Lógica

1. **Configuração** → Base para tudo funcionar
2. **Arquitetura** → Organização antes do caos
3. **JSON** → Serialização essencial para AutomationSettings e Tasks
4. **Git** → Versionar desde cedo
5. **Serilog** → Logs essenciais para debugging
6. **Playwright** → Coração do web scraping
7. **ORM + Docker** → Persistir os dados extraídos
8. **Ferramentas complementares** → Conforme necessidade
9. **Quartz** → Automatizar execução
10. **Deploy** → Colocar em produção

**Regra de ouro:** Cada guia assume que os anteriores já foram concluídos.