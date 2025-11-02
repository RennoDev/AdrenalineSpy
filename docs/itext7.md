# iText7 - Geração de Relatórios PDF

## O que é iText7

**iText7** é uma biblioteca .NET para criar e manipular documentos PDF programaticamente, com recursos avançados de formatação, imagens e interatividade.

**Onde é usado no AdrenalineSpy:**
- Gerar relatórios PDF executivos das notícias coletadas
- Criar documentos com screenshots das páginas capturadas
- Exportar relatórios de auditoria com links clicáveis
- Gerar PDFs para arquivo permanente das notícias
- Criar apresentações automáticas dos dados coletados
- Relatórios de conformidade e logs em formato oficial

⚠️ **IMPORTANTE - Licenciamento**: iText7 possui licença **AGPL v3** (open source) e licença comercial. Para uso comercial, uma licença paga é necessária.

## Como Instalar

### 1. Instalar Pacotes iText7

```powershell
dotnet add package itext7
dotnet add package itext7.bouncy-castle-adapter
```

### 2. Verificar .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="itext7" Version="8.0.2" />
    <PackageReference Include="itext7.bouncy-castle-adapter" Version="8.0.2" />
  </ItemGroup>
</Project>
```

### 3. Configurar Licença (OBRIGATÓRIO)

**Para uso não comercial (AGPL v3):**
```csharp
// Não há configuração especial necessária para AGPL v3
// Mas você deve cumprir os termos da licença AGPL
```

**Para uso comercial (licença paga):**
```csharp
// Configurar chave de licença comercial
LicenseKey.LoadLicenseFile("path/to/itextkey.xml");
```

## Implementar no AutomationSettings.json

Adicione configurações de PDF na seção `Relatorios`:

```json
{
  "Navegacao": {
    "UrlBase": "https://www.adrenaline.com.br",
    "DelayEntrePaginas": 2000
  },
  "Database": {
    "ConnectionString": "Server=localhost;Database=AdrenalineSpy;..."
  },
  "Relatorios": {
    "HabilitarExportacaoCSV": true,
    "HabilitarRelatorioExcel": true,
    "HabilitarRelatorioPDF": true,
    "DiretorioExportacao": "exports/",
    "NomeArquivoPDF": "relatorio-adrenaline-{data}.pdf",
    "ConfiguracaoPDF": {
      "TituloDocumento": "Relatório AdrenalineSpy - Monitoramento de Notícias",
      "Autor": "AdrenalineSpy RPA Bot",
      "Assunto": "Coleta automatizada de notícias do Adrenaline.com.br",
      "PalavrasChave": "adrenaline, tecnologia, games, automação, rpa",
      "IncluirScreenshots": false,
      "IncluirIndice": true,
      "IncluirCabecalhoRodape": true,
      "FontePadrao": "Arial",
      "TamanhoFonteTitulo": 16,
      "TamanhoFonteTexto": 11,
      "MargemPagina": 50,
      "OrientacaoPagina": "Portrait",
      "CompressaoImagens": true
    },
    "LayoutPDF": {
      "IncluirCapaDeFrente": true,
      "IncluirResumoExecutivo": true,
      "IncluirDetalhesNoticias": true,
      "IncluirEstatisticas": true,
      "IncluirAnexos": false,
      "NoticiasAgrupadasPorCategoria": true,
      "LimiteNoticiasDetalhadas": 50
    }
  },
  "Logging": {
    "Nivel": "Information",
    "CaminhoArquivo": "logs/adrenaline-spy.log"
  }
}
```

**Configurações específicas do iText7:**
- **`HabilitarRelatorioPDF`**: Liga/desliga geração de PDFs
- **`ConfiguracaoPDF`**: Metadados e formatação dos documentos
- **`LayoutPDF`**: Estrutura e conteúdo dos relatórios

## Implementar no Config.cs

Adicione classes de configuração para PDF:

```csharp
public class ConfiguracaoPDFConfig
{
    public string TituloDocumento { get; set; } = "Relatório AdrenalineSpy - Monitoramento de Notícias";
    public string Autor { get; set; } = "AdrenalineSpy RPA Bot";
    public string Assunto { get; set; } = "Coleta automatizada de notícias do Adrenaline.com.br";
    public string PalavrasChave { get; set; } = "adrenaline, tecnologia, games, automação, rpa";
    public bool IncluirScreenshots { get; set; } = false;
    public bool IncluirIndice { get; set; } = true;
    public bool IncluirCabecalhoRodape { get; set; } = true;
    public string FontePadrao { get; set; } = "Arial";
    public float TamanhoFonteTitulo { get; set; } = 16f;
    public float TamanhoFonteTexto { get; set; } = 11f;
    public float MargemPagina { get; set; } = 50f;
    public string OrientacaoPagina { get; set; } = "Portrait";
    public bool CompressaoImagens { get; set; } = true;
}

public class LayoutPDFConfig
{
    public bool IncluirCapaDeFrente { get; set; } = true;
    public bool IncluirResumoExecutivo { get; set; } = true;
    public bool IncluirDetalhesNoticias { get; set; } = true;
    public bool IncluirEstatisticas { get; set; } = true;
    public bool IncluirAnexos { get; set; } = false;
    public bool NoticiasAgrupadasPorCategoria { get; set; } = true;
    public int LimiteNoticiasDetalhadas { get; set; } = 50;
}

public class RelatoriosConfig
{
    // ... propriedades existentes (CSV, Excel) ...
    public bool HabilitarRelatorioPDF { get; set; } = true;
    public string NomeArquivoPDF { get; set; } = "relatorio-adrenaline-{data}.pdf";
    public ConfiguracaoPDFConfig ConfiguracaoPDF { get; set; } = new();
    public LayoutPDFConfig LayoutPDF { get; set; } = new();
}

public class Config
{
    // ... propriedades e métodos existentes ...
    
    /// <summary>
    /// Obtém caminho completo do arquivo PDF
    /// </summary>
    public string ObterCaminhoPDF()
    {
        Directory.CreateDirectory(Relatorios.DiretorioExportacao);
        
        var nomeArquivo = Relatorios.NomeArquivoPDF
            .Replace("{data}", DateTime.Now.ToString("yyyy-MM-dd"));
        
        return Path.Combine(Relatorios.DiretorioExportacao, nomeArquivo);
    }
}
```

## Montar nas Tasks

Crie a classe `PDFReportTask.cs` na pasta `Workflow/Tasks/`:

```csharp
using iText.Html2pdf;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace AdrenalineSpy.Workflow.Tasks;

/// <summary>
/// Gerencia geração de relatórios PDF para o AdrenalineSpy
/// </summary>
public static class PDFReportTask
{
    /// <summary>
    /// Gera relatório PDF completo das notícias coletadas
    /// </summary>
    public static async Task<bool> GerarRelatorioPDF(List<Noticia> noticias, DateTime dataExecucao)
    {
        try
        {
            if (!Config.Instancia.Relatorios.HabilitarRelatorioPDF)
            {
                LoggingTask.RegistrarInfo("📄 Relatório PDF desabilitado nas configurações");
                return true;
            }

            if (noticias?.Any() != true)
            {
                LoggingTask.RegistrarAviso("📄 Nenhuma notícia para gerar relatório PDF");
                return false;
            }

            var caminhoArquivo = Config.Instancia.ObterCaminhoPDF();
            var config = Config.Instancia.Relatorios.ConfiguracaoPDF;

            // Criar documento PDF
            using var writer = new PdfWriter(caminhoArquivo);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Configurar documento
            ConfigurarPropriedadesPDF(pdf, config, dataExecucao);
            ConfigurarLayoutDocumento(document, config);

            // Gerar conteúdo do relatório
            if (Config.Instancia.Relatorios.LayoutPDF.IncluirCapaDeFrente)
            {
                AdicionarCapaFrente(document, config, dataExecucao, noticias.Count);
            }

            if (Config.Instancia.Relatorios.LayoutPDF.IncluirResumoExecutivo)
            {
                AdicionarResumoExecutivo(document, noticias, dataExecucao);
            }

            if (Config.Instancia.Relatorios.LayoutPDF.IncluirEstatisticas)
            {
                AdicionarEstatisticas(document, noticias);
            }

            if (Config.Instancia.Relatorios.LayoutPDF.IncluirDetalhesNoticias)
            {
                AdicionarDetalhesNoticias(document, noticias);
            }

            LoggingTask.RegistrarInfo($"📄 Relatório PDF gerado: {Path.GetFileName(caminhoArquivo)} ({noticias.Count} notícias)");
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao gerar relatório PDF", ex);
            return false;
        }
    }

    /// <summary>
    /// Configura propriedades do documento PDF
    /// </summary>
    private static void ConfigurarPropriedadesPDF(PdfDocument pdf, ConfiguracaoPDFConfig config, DateTime dataExecucao)
    {
        var info = pdf.GetDocumentInfo();
        info.SetTitle(config.TituloDocumento);
        info.SetAuthor(config.Autor);
        info.SetSubject(config.Assunto);
        info.SetKeywords(config.PalavrasChave);
        info.SetCreator("AdrenalineSpy RPA - iText7");
        info.SetCreationDate(DateTimeOffset.Now);
    }

    /// <summary>
    /// Configura layout e margens do documento
    /// </summary>
    private static void ConfigurarLayoutDocumento(Document document, ConfiguracaoPDFConfig config)
    {
        var margem = config.MargemPagina;
        document.SetMargins(margem, margem, margem, margem);
        
        // Configurar fonte padrão
        try
        {
            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            document.SetFont(font).SetFontSize(config.TamanhoFonteTexto);
        }
        catch
        {
            // Usar fonte padrão se houver erro
            LoggingTask.RegistrarAviso("📄 Usando fonte padrão do PDF");
        }
    }

    /// <summary>
    /// Adiciona capa do relatório
    /// </summary>
    private static void AdicionarCapaFrente(Document document, ConfiguracaoPDFConfig config, DateTime dataExecucao, int totalNoticias)
    {
        // Título principal
        var titulo = new Paragraph(config.TituloDocumento)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(config.TamanhoFonteTitulo + 4)
            .SetBold()
            .SetFontColor(ColorConstants.DARK_GRAY)
            .SetMarginTop(100);

        document.Add(titulo);

        // Subtítulo
        var subtitulo = new Paragraph("Relatório Automatizado de Coleta de Notícias")
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(config.TamanhoFonteTexto + 2)
            .SetItalic()
            .SetMarginBottom(50);

        document.Add(subtitulo);

        // Informações da execução
        var tabela = new Table(2).UseAllAvailableWidth();
        tabela.SetMarginTop(50);

        AdicionarLinhaTabela(tabela, "🌐 Site Monitorado:", "Adrenaline.com.br");
        AdicionarLinhaTabela(tabela, "📅 Data de Execução:", dataExecucao.ToString("dd/MM/yyyy HH:mm:ss"));
        AdicionarLinhaTabela(tabela, "📰 Total de Notícias:", totalNoticias.ToString());
        AdicionarLinhaTabela(tabela, "🤖 Gerado por:", "AdrenalineSpy RPA");

        document.Add(tabela);

        // Quebra de página
        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
    }

    /// <summary>
    /// Adiciona resumo executivo
    /// </summary>
    private static void AdicionarResumoExecutivo(Document document, List<Noticia> noticias, DateTime dataExecucao)
    {
        // Título da seção
        var titulo = new Paragraph("📋 Resumo Executivo")
            .SetFontSize(Config.Instancia.Relatorios.ConfiguracaoPDF.TamanhoFonteTitulo)
            .SetBold()
            .SetMarginBottom(20);

        document.Add(titulo);

        // Linha separadora
        document.Add(new LineSeparator(new SolidLine()));
        document.Add(new Paragraph("\n"));

        // Estatísticas principais
        var categorias = noticias.GroupBy(n => n.Categoria)
            .Select(g => new { Categoria = g.Key, Quantidade = g.Count() })
            .OrderByDescending(x => x.Quantidade)
            .ToList();

        var resumo = new Paragraph()
            .Add($"Este relatório apresenta o resultado da coleta automática de notícias " +
                 $"realizada em {dataExecucao:dd/MM/yyyy} às {dataExecucao:HH:mm:ss}. ")
            .Add($"Foram coletadas {noticias.Count} notícias distribuídas em {categorias.Count} categorias diferentes.\n\n");

        document.Add(resumo);

        // Distribuição por categoria
        document.Add(new Paragraph("🏷️ Distribuição por Categoria:").SetBold().SetMarginTop(10));

        foreach (var categoria in categorias.Take(5)) // Top 5 categorias
        {
            var percentual = (categoria.Quantidade * 100.0 / noticias.Count);
            var linha = new Paragraph($"• {categoria.Categoria}: {categoria.Quantidade} notícias ({percentual:F1}%)")
                .SetMarginLeft(20);
            document.Add(linha);
        }

        document.Add(new Paragraph("\n"));
    }

    /// <summary>
    /// Adiciona estatísticas detalhadas
    /// </summary>
    private static void AdicionarEstatisticas(Document document, List<Noticia> noticias)
    {
        var titulo = new Paragraph("📊 Estatísticas Detalhadas")
            .SetFontSize(Config.Instancia.Relatorios.ConfiguracaoPDF.TamanhoFonteTitulo)
            .SetBold()
            .SetMarginTop(30)
            .SetMarginBottom(20);

        document.Add(titulo);
        document.Add(new LineSeparator(new SolidLine()));

        // Tabela de estatísticas por categoria
        var tabela = new Table(3).UseAllAvailableWidth();
        tabela.SetMarginTop(20);

        // Cabeçalho
        tabela.AddHeaderCell(new Cell().Add(new Paragraph("Categoria").SetBold()));
        tabela.AddHeaderCell(new Cell().Add(new Paragraph("Quantidade").SetBold()));
        tabela.AddHeaderCell(new Cell().Add(new Paragraph("Percentual").SetBold()));

        var categorias = noticias.GroupBy(n => n.Categoria)
            .Select(g => new { Categoria = g.Key, Quantidade = g.Count() })
            .OrderByDescending(x => x.Quantidade);

        foreach (var categoria in categorias)
        {
            var percentual = (categoria.Quantidade * 100.0 / noticias.Count);
            
            tabela.AddCell(categoria.Categoria);
            tabela.AddCell(new Cell().Add(new Paragraph(categoria.Quantidade.ToString()))
                .SetTextAlignment(TextAlignment.CENTER));
            tabela.AddCell(new Cell().Add(new Paragraph($"{percentual:F1}%"))
                .SetTextAlignment(TextAlignment.CENTER));
        }

        document.Add(tabela);
    }

    /// <summary>
    /// Adiciona detalhes das notícias
    /// </summary>
    private static void AdicionarDetalhesNoticias(Document document, List<Noticia> noticias)
    {
        var config = Config.Instancia.Relatorios.LayoutPDF;
        
        if (config.NoticiasAgrupadasPorCategoria)
        {
            AdicionarNoticiasAgrupadasPorCategoria(document, noticias);
        }
        else
        {
            AdicionarNoticiasSequenciais(document, noticias);
        }
    }

    /// <summary>
    /// Adiciona notícias agrupadas por categoria
    /// </summary>
    private static void AdicionarNoticiasAgrupadasPorCategoria(Document document, List<Noticia> noticias)
    {
        var limite = Config.Instancia.Relatorios.LayoutPDF.LimiteNoticiasDetalhadas;
        var noticiasLimitadas = noticias.Take(limite).ToList();

        var categorias = noticiasLimitadas.GroupBy(n => n.Categoria).OrderBy(g => g.Key);

        foreach (var grupo in categorias)
        {
            // Nova página para cada categoria
            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

            // Título da categoria
            var tituloCategoria = new Paragraph($"📁 {grupo.Key}")
                .SetFontSize(Config.Instancia.Relatorios.ConfiguracaoPDF.TamanhoFonteTitulo)
                .SetBold()
                .SetMarginBottom(20);

            document.Add(tituloCategoria);
            document.Add(new LineSeparator(new SolidLine()));
            document.Add(new Paragraph("\n"));

            // Notícias da categoria
            foreach (var noticia in grupo.OrderByDescending(n => n.DataPublicacao))
            {
                AdicionarDetalheNoticia(document, noticia);
            }
        }
    }

    /// <summary>
    /// Adiciona notícias em sequência
    /// </summary>
    private static void AdicionarNoticiasSequenciais(Document document, List<Noticia> noticias)
    {
        var titulo = new Paragraph("📄 Detalhes das Notícias")
            .SetFontSize(Config.Instancia.Relatorios.ConfiguracaoPDF.TamanhoFonteTitulo)
            .SetBold()
            .SetMarginTop(30)
            .SetMarginBottom(20);

        document.Add(titulo);
        document.Add(new LineSeparator(new SolidLine()));

        var limite = Config.Instancia.Relatorios.LayoutPDF.LimiteNoticiasDetalhadas;
        var noticiasLimitadas = noticias.Take(limite)
            .OrderByDescending(n => n.DataPublicacao);

        foreach (var noticia in noticiasLimitadas)
        {
            AdicionarDetalheNoticia(document, noticia);
        }
    }

    /// <summary>
    /// Adiciona detalhe de uma notícia específica
    /// </summary>
    private static void AdicionarDetalheNoticia(Document document, Noticia noticia)
    {
        // Container da notícia com borda
        var div = new Div()
            .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1))
            .SetPadding(15)
            .SetMarginBottom(15);

        // Título da notícia
        var titulo = new Paragraph(noticia.Titulo)
            .SetBold()
            .SetFontSize(Config.Instancia.Relatorios.ConfiguracaoPDF.TamanhoFonteTexto + 1)
            .SetMarginBottom(5);

        div.Add(titulo);

        // Metadados
        var metadados = new Paragraph()
            .Add($"📅 {noticia.DataPublicacao:dd/MM/yyyy HH:mm} | ")
            .Add($"🏷️ {noticia.Categoria} | ")
            .Add("🔗 ")
            .Add(new Link("Ver no site", PdfAction.CreateURI(noticia.Url))
                .SetFontColor(ColorConstants.BLUE))
            .SetFontSize(Config.Instancia.Relatorios.ConfiguracaoPDF.TamanhoFonteTexto - 1)
            .SetMarginBottom(10);

        div.Add(metadados);

        // Conteúdo (primeiros 500 caracteres)
        if (!string.IsNullOrWhiteSpace(noticia.Conteudo))
        {
            var conteudoPreview = noticia.Conteudo.Length > 500 
                ? noticia.Conteudo.Substring(0, 500) + "..."
                : noticia.Conteudo;

            var conteudo = new Paragraph(conteudoPreview)
                .SetFontSize(Config.Instancia.Relatorios.ConfiguracaoPDF.TamanhoFonteTexto)
                .SetTextAlignment(TextAlignment.JUSTIFIED);

            div.Add(conteudo);
        }

        document.Add(div);
    }

    /// <summary>
    /// Adiciona linha à tabela de informações
    /// </summary>
    private static void AdicionarLinhaTabela(Table tabela, string label, string valor)
    {
        tabela.AddCell(new Cell().Add(new Paragraph(label).SetBold()));
        tabela.AddCell(new Cell().Add(new Paragraph(valor)));
    }
}
```

## Métodos Mais Usados

### Criar Documento PDF Básico

```csharp
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

// Criar PDF simples
var caminhoArquivo = "relatorio-adrenaline.pdf";
using var writer = new PdfWriter(caminhoArquivo);
using var pdf = new PdfDocument(writer);
using var document = new Document(pdf);

// Adicionar título
document.Add(new Paragraph("Relatório AdrenalineSpy")
    .SetFontSize(18)
    .SetBold()
    .SetTextAlignment(TextAlignment.CENTER));

LoggingTask.RegistrarInfo("📄 PDF básico criado");
```

### Adicionar Tabelas com Dados

```csharp
// Criar tabela de notícias
var tabela = new Table(3).UseAllAvailableWidth();

// Cabeçalhos
tabela.AddHeaderCell("Título");
tabela.AddHeaderCell("Categoria");  
tabela.AddHeaderCell("Data");

// Dados
foreach (var noticia in noticias)
{
    tabela.AddCell(noticia.Titulo);
    tabela.AddCell(noticia.Categoria);
    tabela.AddCell(noticia.DataPublicacao.ToString("dd/MM/yyyy"));
}

document.Add(tabela);
LoggingTask.RegistrarInfo("📊 Tabela de notícias adicionada ao PDF");
```

### Adicionar Links Clicáveis

```csharp
// Link para URL da notícia
var linkTexto = new Link("🔗 Ver notícia completa", PdfAction.CreateURI(noticia.Url))
    .SetFontColor(ColorConstants.BLUE)
    .SetUnderline();

var paragrafo = new Paragraph()
    .Add("Acesse: ")
    .Add(linkTexto);

document.Add(paragrafo);
```

### Configurar Propriedades do Documento

```csharp
// Metadados do PDF
var info = pdf.GetDocumentInfo();
info.SetTitle("Relatório AdrenalineSpy");
info.SetAuthor("AdrenalineSpy RPA");
info.SetSubject("Coleta automatizada de notícias");
info.SetKeywords("adrenaline, tecnologia, automação");
info.SetCreationDate(DateTimeOffset.Now);

LoggingTask.RegistrarInfo("📝 Propriedades do PDF configuradas");
```

### Adicionar Imagens/Screenshots

```csharp
// Adicionar screenshot se disponível
if (File.Exists(caminhoScreenshot))
{
    var imageData = ImageDataFactory.Create(caminhoScreenshot);
    var image = new Image(imageData);
    
    // Redimensionar para caber na página
    image.SetWidth(400);
    image.SetAutoScale(true);
    
    document.Add(image);
    LoggingTask.RegistrarInfo("📸 Screenshot adicionado ao PDF");
}
```

### Integração com Workflow Principal

```csharp
// No Workflow.cs - adicionar geração de PDF
public async Task<bool> ExecutarScrapingCompleto()
{
    try
    {
        var inicioExecucao = DateTime.Now;
        
        // 1. Executar scraping...
        var noticias = await ExtractionTask.ColetarTodasNoticias();
        await MigrationTask.SalvarNoticias(noticias);
        
        // 2. Gerar todos os tipos de relatório
        if (Config.Instancia.Relatorios.ExportarApósExecução)
        {
            var tasks = new List<Task<bool>>();
            
            // CSV rápido
            if (Config.Instancia.Relatorios.HabilitarExportacaoCSV)
            {
                tasks.Add(CsvExportTask.ExportarNoticias(noticias));
            }
            
            // Excel com gráficos
            if (Config.Instancia.Relatorios.HabilitarRelatorioExcel)
            {
                tasks.Add(ExcelReportTask.GerarRelatorioCompleto(noticias, inicioExecucao));
            }
            
            // PDF executivo
            if (Config.Instancia.Relatorios.HabilitarRelatorioPDF)
            {
                tasks.Add(PDFReportTask.GerarRelatorioPDF(noticias, inicioExecucao));
            }
            
            // Executar relatórios em paralelo
            var resultados = await Task.WhenAll(tasks);
            var sucessos = resultados.Count(r => r);
            
            LoggingTask.RegistrarInfo($"📊 Relatórios gerados: {sucessos}/{tasks.Count} formatos");
        }
        
        return true;
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro("Erro no workflow com relatórios", ex);
        return false;
    }
}
```

### Formatação Avançada e Estilos

```csharp
// Parágrafos com formatação rica
var titulo = new Paragraph("📊 Estatísticas de Coleta")
    .SetFontSize(16)
    .SetBold()
    .SetFontColor(ColorConstants.DARK_BLUE)
    .SetTextAlignment(TextAlignment.CENTER)
    .SetMarginBottom(20);

// Bordas e backgrounds
var div = new Div()
    .SetBorder(new SolidBorder(ColorConstants.GRAY, 1))
    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
    .SetPadding(10)
    .SetMarginBottom(15);

div.Add(new Paragraph("Conteúdo destacado"));
document.Add(div);

LoggingTask.RegistrarInfo("🎨 Formatação avançada aplicada");
```
