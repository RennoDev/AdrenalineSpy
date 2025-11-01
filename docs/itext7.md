# iText7 - Manipulação de PDFs

## Índice
1. [Introdução](#introdução)
2. [Instalação](#instalação)
3. [Licenciamento](#licenciamento)
4. [Criar PDF](#criar-pdf)
5. [Ler/Extrair Texto](#ler-extrair-texto)
6. [Modificar PDF](#modificar-pdf)
7. [Exemplos Práticos](#exemplos-práticos)

---

## Introdução

**iText7** é uma biblioteca poderosa para criar e manipular documentos PDF em .NET.

### Vantagens
- ✅ Criar PDFs do zero
- ✅ Extrair texto e dados
- ✅ Preencher formulários
- ✅ Adicionar imagens e tabelas
- ✅ Assinar digitalmente
- ✅ Criptografar PDFs

---

## Instalação

```bash
dotnet add package itext7
dotnet add package itext7.pdfhtml  # Para HTML to PDF
```

---

## Licenciamento

⚠️ **IMPORTANTE**: iText7 é AGPL (código aberto) mas requer licença comercial para uso comercial.

- **Uso pessoal/educacional**: Gratuito
- **Uso comercial**: Licença paga necessária
- **Alternativa gratuita**: QuestPDF, PdfSharp

---

## Criar PDF

### PDF Básico

```csharp
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

void CriarPDFSimples()
{
    string dest = "documento.pdf";
    
    using (PdfWriter writer = new PdfWriter(dest))
    using (PdfDocument pdf = new PdfDocument(writer))
    using (Document document = new Document(pdf))
    {
        // Adicionar parágrafo
        document.Add(new Paragraph("Olá, Mundo!"));
        
        // Adicionar texto formatado
        document.Add(new Paragraph("Este é um PDF criado com iText7")
            .SetFontSize(20)
            .SetBold());
    }
    
    Console.WriteLine($"PDF criado: {dest}");
}
```

### PDF com Tabelas

```csharp
using iText.Layout.Element;

void CriarPDFComTabela()
{
    using (PdfWriter writer = new PdfWriter("tabela.pdf"))
    using (PdfDocument pdf = new PdfDocument(writer))
    using (Document document = new Document(pdf))
    {
        // Criar tabela com 3 colunas
        Table table = new Table(3);
        
        // Cabeçalhos
        table.AddHeaderCell("Nome");
        table.AddHeaderCell("Idade");
        table.AddHeaderCell("Email");
        
        // Dados
        table.AddCell("João");
        table.AddCell("30");
        table.AddCell("joao@email.com");
        
        table.AddCell("Maria");
        table.AddCell("25");
        table.AddCell("maria@email.com");
        
        document.Add(table);
    }
}
```

### PDF com Imagens

```csharp
using iText.IO.Image;
using iText.Layout.Element;

void CriarPDFComImagem()
{
    using (PdfWriter writer = new PdfWriter("imagem.pdf"))
    using (PdfDocument pdf = new PdfDocument(writer))
    using (Document document = new Document(pdf))
    {
        document.Add(new Paragraph("Documento com Imagem"));
        
        // Adicionar imagem
        ImageData imageData = ImageDataFactory.Create("logo.png");
        Image image = new Image(imageData);
        
        // Redimensionar
        image.SetWidth(200);
        image.SetAutoScale(true);
        
        document.Add(image);
    }
}
```

---

## Ler/Extrair Texto

### Extrair Todo o Texto

```csharp
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

string ExtrairTextoPDF(string pdfPath)
{
    using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(pdfPath)))
    {
        var text = new StringBuilder();
        
        for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
        {
            var strategy = new LocationTextExtractionStrategy();
            string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);
            text.AppendLine(pageText);
        }
        
        return text.ToString();
    }
}
```

### Ler Metadata

```csharp
void LerMetadata(string pdfPath)
{
    using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(pdfPath)))
    {
        var info = pdfDoc.GetDocumentInfo();
        
        Console.WriteLine($"Título: {info.GetTitle()}");
        Console.WriteLine($"Autor: {info.GetAuthor()}");
        Console.WriteLine($"Criador: {info.GetCreator()}");
        Console.WriteLine($"Páginas: {pdfDoc.GetNumberOfPages()}");
    }
}
```

---

## Modificar PDF

### Adicionar Página a PDF Existente

```csharp
void AdicionarPagina(string pdfPath)
{
    using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(pdfPath), new PdfWriter("modificado.pdf")))
    using (Document document = new Document(pdfDoc))
    {
        // Adicionar nova página
        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
        document.Add(new Paragraph("Nova Página Adicionada"));
    }
}
```

### Mesclar PDFs

```csharp
using iText.Kernel.Utils;

void MesclarPDFs(string[] arquivos, string saida)
{
    using (PdfDocument pdfMerged = new PdfDocument(new PdfWriter(saida)))
    {
        PdfMerger merger = new PdfMerger(pdfMerged);
        
        foreach (string arquivo in arquivos)
        {
            using (PdfDocument pdf = new PdfDocument(new PdfReader(arquivo)))
            {
                merger.Merge(pdf, 1, pdf.GetNumberOfPages());
            }
        }
    }
    
    Console.WriteLine($"PDFs mesclados em: {saida}");
}
```

### Dividir PDF

```csharp
void DividirPDF(string pdfPath, string outputFolder)
{
    using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(pdfPath)))
    {
        for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
        {
            string outputPath = Path.Combine(outputFolder, $"pagina_{page}.pdf");
            
            using (PdfDocument newPdf = new PdfDocument(new PdfWriter(outputPath)))
            {
                pdfDoc.CopyPagesTo(page, page, newPdf);
            }
        }
    }
}
```

---

## Exemplos Práticos

### Exemplo 1: Relatório Formatado

```csharp
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

void GerarRelatorio(List<Venda> vendas)
{
    using (PdfWriter writer = new PdfWriter("relatorio_vendas.pdf"))
    using (PdfDocument pdf = new PdfDocument(writer))
    using (Document document = new Document(pdf))
    {
        // Título
        PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        Paragraph title = new Paragraph("Relatório de Vendas")
            .SetFont(boldFont)
            .SetFontSize(24)
            .SetTextAlignment(TextAlignment.CENTER);
        document.Add(title);
        
        // Data
        document.Add(new Paragraph($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}")
            .SetFontSize(10)
            .SetTextAlignment(TextAlignment.RIGHT));
        
        // Tabela
        Table table = new Table(new float[] { 2, 1, 1, 1 });
        table.SetWidth(UnitValue.CreatePercentValue(100));
        
        // Cabeçalhos
        table.AddHeaderCell(new Cell().Add(new Paragraph("Produto").SetBold())
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Qtd").SetBold())
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Preço").SetBold())
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Total").SetBold())
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY));
        
        // Dados
        decimal totalGeral = 0;
        foreach (var venda in vendas)
        {
            decimal total = venda.Quantidade * venda.Preco;
            totalGeral += total;
            
            table.AddCell(venda.Produto);
            table.AddCell(venda.Quantidade.ToString());
            table.AddCell($"R$ {venda.Preco:N2}");
            table.AddCell($"R$ {total:N2}");
        }
        
        // Total
        table.AddCell(new Cell(1, 3)
            .Add(new Paragraph("TOTAL GERAL").SetBold())
            .SetTextAlignment(TextAlignment.RIGHT));
        table.AddCell(new Cell()
            .Add(new Paragraph($"R$ {totalGeral:N2}").SetBold()));
        
        document.Add(table);
    }
}
```

### Exemplo 2: HTML para PDF

```csharp
using iText.Html2pdf;

void ConverterHTMLparaPDF(string htmlPath, string pdfPath)
{
    HtmlConverter.ConvertToPdf(new FileInfo(htmlPath), new FileInfo(pdfPath));
}

void HTMLStringParaPDF(string html, string pdfPath)
{
    using (FileStream pdfDest = File.Open(pdfPath, FileMode.Create))
    {
        ConverterProperties properties = new ConverterProperties();
        HtmlConverter.ConvertToPdf(html, pdfDest, properties);
    }
}
```

### Exemplo 3: Preencher Formulário PDF

```csharp
using iText.Forms;
using iText.Forms.Fields;

void PreencherFormulario(string templatePath, string outputPath)
{
    using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(templatePath), new PdfWriter(outputPath)))
    {
        PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);
        
        IDictionary<string, PdfFormField> fields = form.GetFormFields();
        
        // Preencher campos
        fields["nome"].SetValue("João Silva");
        fields["email"].SetValue("joao@email.com");
        fields["data"].SetValue(DateTime.Now.ToString("dd/MM/yyyy"));
        
        // Achatar formulário (tornar não editável)
        form.FlattenFields();
    }
}
```

### Exemplo 4: Adicionar Marca d'água

```csharp
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Extgstate;

void AdicionarMarcaDagua(string inputPath, string outputPath, string texto)
{
    using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(inputPath), new PdfWriter(outputPath)))
    {
        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
        {
            PdfPage page = pdfDoc.GetPage(i);
            Rectangle pageSize = page.GetPageSize();
            
            PdfCanvas canvas = new PdfCanvas(page);
            
            // Transparência
            canvas.SaveState();
            PdfExtGState gs = new PdfExtGState();
            gs.SetFillOpacity(0.2f);
            canvas.SetExtGState(gs);
            
            // Texto
            canvas.BeginText()
                .SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD), 60)
                .SetColor(ColorConstants.GRAY, true)
                .MoveText(pageSize.GetWidth() / 2 - 150, pageSize.GetHeight() / 2)
                .ShowText(texto)
                .EndText();
            
            canvas.RestoreState();
        }
    }
}
```

---

## Boas Práticas

### 1. Use using para Dispose

```csharp
using (PdfWriter writer = new PdfWriter(dest))
using (PdfDocument pdf = new PdfDocument(writer))
using (Document document = new Document(pdf))
{
    // trabalhar aqui
}
```

### 2. Defina Metadata

```csharp
var info = pdf.GetDocumentInfo();
info.SetTitle("Meu Documento");
info.SetAuthor("João Silva");
info.SetCreator("MeuApp v1.0");
```

### 3. Trate Exceções

```csharp
try
{
    // Operações PDF
}
catch (iText.IO.Exceptions.IOException ex)
{
    Console.WriteLine($"Erro de leitura: {ex.Message}");
}
catch (PdfException ex)
{
    Console.WriteLine($"Erro PDF: {ex.Message}");
}
```

---

## Alternativas Gratuitas

Se iText7 não for viável (licença), considere:

1. **QuestPDF** - Moderno, fluent API, totalmente gratuito
   ```bash
   dotnet add package QuestPDF
   ```

2. **PdfSharpCore** - Fork gratuito do PdfSharp
   ```bash
   dotnet add package PdfSharpCore
   ```

3. **Playwright/Puppeteer** - HTML to PDF via navegador

---

## Recursos Adicionais

- **Site Oficial**: https://itextpdf.com/
- **Documentação**: https://kb.itextpdf.com/home
- **GitHub**: https://github.com/itext/itext7-dotnet

---

**Versão:** 1.0  
**Última atualização:** Novembro 2025
