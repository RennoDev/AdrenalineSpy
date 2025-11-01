# EPPlus - Manipulação de Planilhas Excel

## Índice
1. [Introdução](#introdução)
2. [Instalação](#instalação)
3. [Licenciamento](#licenciamento)
4. [Conceitos Básicos](#conceitos-básicos)
5. [Criar e Salvar](#criar-e-salvar)
6. [Ler Dados](#ler-dados)
7. [Escrever Dados](#escrever-dados)
8. [Formatação](#formatação)
9. [Fórmulas](#fórmulas)
10. [Gráficos](#gráficos)
11. [Tabelas](#tabelas)
12. [Exemplos Práticos](#exemplos-práticos)
13. [Boas Práticas](#boas-práticas)

---

## Introdução

**EPPlus** é uma biblioteca .NET para criar, ler e manipular planilhas Excel (.xlsx) sem precisar do Microsoft Excel instalado.

### Vantagens
- ✅ Não requer Excel instalado
- ✅ Performance excelente
- ✅ Suporte completo a .xlsx
- ✅ Fórmulas, formatação, gráficos
- ✅ Trabalha com grandes volumes de dados
- ✅ Multiplataforma (.NET Core)

### Formatos Suportados
- **.xlsx** - Excel 2007+
- Não suporta .xls (formato antigo)

---

## Instalação

```bash
dotnet add package EPPlus
```

---

## Licenciamento

⚠️ **IMPORTANTE**: EPPlus 5+ requer licença para uso comercial.

### Configurar Licença (OBRIGATÓRIO)

**Adicione esta linha ANTES de usar qualquer funcionalidade do EPPlus:**

```csharp
using OfficeOpenXml;

// Para uso não comercial (estudo, projetos pessoais, open source)
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Para uso comercial (requer licença paga)
ExcelPackage.LicenseContext = LicenseContext.Commercial;
```

**Exemplo completo em `Program.cs`:**

```csharp
using OfficeOpenXml;

// ⚠️ CONFIGURE ISSO PRIMEIRO, logo no início do programa
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

Console.WriteLine("Iniciando manipulação de Excel...");

// Agora pode usar EPPlus normalmente
var arquivo = new FileInfo("planilha.xlsx");
using var package = new ExcelPackage(arquivo);
// ... resto do código ...
```

**Se você esquecer isso, receberá um erro:** `"Please set the ExcelPackage.LicenseContext property"`

---

## Conceitos Básicos

### Estrutura Hierárquica

```
ExcelPackage (arquivo .xlsx)
  └── Workbook
       └── Worksheets
            └── Worksheet (planilha/aba)
                 └── Cells (células)
```

### Namespaces Principais

```csharp
using OfficeOpenXml;              // Core
using OfficeOpenXml.Style;        // Estilos
using OfficeOpenXml.Drawing;      // Gráficos
using OfficeOpenXml.Drawing.Chart; // Gráficos específicos
using OfficeOpenXml.Table;        // Tabelas
```

---

## Criar e Salvar

### Criar Nova Planilha

```csharp
using OfficeOpenXml;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Criar novo arquivo
using (var package = new ExcelPackage())
{
    // Adicionar planilha
    var worksheet = package.Workbook.Worksheets.Add("MinhaAba");
    
    // Adicionar dados
    worksheet.Cells[1, 1].Value = "Nome";
    worksheet.Cells[1, 2].Value = "Idade";
    
    worksheet.Cells[2, 1].Value = "João";
    worksheet.Cells[2, 2].Value = 30;
    
    // Salvar arquivo
    FileInfo file = new FileInfo(@"C:\temp\planilha.xlsx");
    package.SaveAs(file);
}

Console.WriteLine("Planilha criada!");
```

### Abrir Arquivo Existente

```csharp
FileInfo file = new FileInfo(@"C:\temp\planilha.xlsx");

using (var package = new ExcelPackage(file))
{
    var worksheet = package.Workbook.Worksheets[0]; // Primeira aba
    
    // Ler dados
    var valor = worksheet.Cells[1, 1].Value;
    
    // Modificar
    worksheet.Cells[3, 1].Value = "Maria";
    
    // Salvar mudanças
    package.Save();
}
```

### Salvar em Stream

```csharp
using (var package = new ExcelPackage())
{
    var worksheet = package.Workbook.Worksheets.Add("Dados");
    worksheet.Cells[1, 1].Value = "Teste";
    
    // Salvar em MemoryStream
    using (var stream = new MemoryStream())
    {
        package.SaveAs(stream);
        byte[] excelBytes = stream.ToArray();
        
        // Enviar por email, upload, etc.
    }
}
```

---

## Ler Dados

### Ler Células

```csharp
var worksheet = package.Workbook.Worksheets["MinhaAba"];

// Por índice (linha, coluna) - baseado em 1
var valor1 = worksheet.Cells[1, 1].Value;

// Por endereço (A1, B2, etc)
var valor2 = worksheet.Cells["A1"].Value;

// Verificar tipo
if (worksheet.Cells[2, 2].Value is double numero)
{
    Console.WriteLine($"Número: {numero}");
}
else if (worksheet.Cells[2, 1].Value is string texto)
{
    Console.WriteLine($"Texto: {texto}");
}

// Ler como texto
string textoFormatado = worksheet.Cells[2, 2].Text;
```

### Ler Intervalo

```csharp
// Ler intervalo de células
var range = worksheet.Cells["A1:C10"];

foreach (var cell in range)
{
    Console.WriteLine($"{cell.Address}: {cell.Value}");
}
```

### Ler Dimensões

```csharp
// Obter dimensão usada
var start = worksheet.Dimension.Start;
var end = worksheet.Dimension.End;

int startRow = start.Row;
int startCol = start.Column;
int endRow = end.Row;
int endCol = end.Column;

Console.WriteLine($"Linhas: {startRow} até {endRow}");
Console.WriteLine($"Colunas: {startCol} até {endCol}");
```

### Iterar por Linhas e Colunas

```csharp
var worksheet = package.Workbook.Worksheets[0];

// Linhas e colunas com dados
int rowCount = worksheet.Dimension.Rows;
int colCount = worksheet.Dimension.Columns;

// Iterar por todas as células
for (int row = 1; row <= rowCount; row++)
{
    for (int col = 1; col <= colCount; col++)
    {
        var cellValue = worksheet.Cells[row, col].Value;
        Console.Write($"{cellValue}\t");
    }
    Console.WriteLine();
}
```

### Ler para Lista de Objetos

```csharp
public class Produto
{
    public string Nome { get; set; }
    public decimal Preco { get; set; }
    public int Quantidade { get; set; }
}

List<Produto> LerProdutos(ExcelWorksheet worksheet)
{
    var produtos = new List<Produto>();
    
    // Assumindo cabeçalho na linha 1
    int rowCount = worksheet.Dimension.Rows;
    
    for (int row = 2; row <= rowCount; row++)
    {
        var produto = new Produto
        {
            Nome = worksheet.Cells[row, 1].Value?.ToString(),
            Preco = Convert.ToDecimal(worksheet.Cells[row, 2].Value),
            Quantidade = Convert.ToInt32(worksheet.Cells[row, 3].Value)
        };
        
        produtos.Add(produto);
    }
    
    return produtos;
}
```

---

## Escrever Dados

### Escrever Valores Simples

```csharp
var worksheet = package.Workbook.Worksheets.Add("Dados");

// Texto
worksheet.Cells[1, 1].Value = "Nome";

// Número
worksheet.Cells[1, 2].Value = 42;

// Data
worksheet.Cells[1, 3].Value = DateTime.Now;

// Fórmula
worksheet.Cells[1, 4].Formula = "=SUM(A1:A10)";

// Boolean
worksheet.Cells[1, 5].Value = true;

// Null
worksheet.Cells[1, 6].Value = null;
```

### Escrever Intervalo

```csharp
// Escrever array de valores
object[,] valores = new object[3, 2]
{
    { "João", 30 },
    { "Maria", 25 },
    { "Pedro", 35 }
};

worksheet.Cells["A1"].LoadFromArrays(new List<object[]>
{
    new object[] { "João", 30 },
    new object[] { "Maria", 25 },
    new object[] { "Pedro", 35 }
});
```

### Escrever de Collection

```csharp
var produtos = new List<Produto>
{
    new Produto { Nome = "Mouse", Preco = 50.00m, Quantidade = 10 },
    new Produto { Nome = "Teclado", Preco = 150.00m, Quantidade = 5 },
    new Produto { Nome = "Monitor", Preco = 800.00m, Quantidade = 3 }
};

// LoadFromCollection com cabeçalhos
worksheet.Cells["A1"].LoadFromCollection(produtos, true);

// Sem cabeçalhos
// worksheet.Cells["A1"].LoadFromCollection(produtos, false);
```

### Auto Fit Colunas

```csharp
// Auto ajustar todas as colunas
worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

// Auto ajustar coluna específica
worksheet.Column(1).AutoFit();

// Com largura mínima e máxima
worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns(10, 50);
```

---

## Formatação

### Fonte

```csharp
// Cabeçalho
var headerRange = worksheet.Cells["A1:C1"];
headerRange.Style.Font.Bold = true;
headerRange.Style.Font.Size = 12;
headerRange.Style.Font.Color.SetColor(Color.White);
headerRange.Style.Font.Name = "Arial";

// Itálico, sublinhado
worksheet.Cells["A2"].Style.Font.Italic = true;
worksheet.Cells["A3"].Style.Font.UnderLine = true;
worksheet.Cells["A4"].Style.Font.Strike = true;
```

### Cor de Fundo

```csharp
using OfficeOpenXml.Style;

var cell = worksheet.Cells["A1"];

// Cor sólida
cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
cell.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

// Cabeçalho azul
var header = worksheet.Cells["A1:E1"];
header.Style.Fill.PatternType = ExcelFillStyle.Solid;
header.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
header.Style.Font.Color.SetColor(Color.White);
```

### Bordas

```csharp
var range = worksheet.Cells["A1:D10"];

// Bordas ao redor
range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

// Cor da borda
range.Style.Border.Top.Color.SetColor(Color.Black);

// Borda grossa
range.Style.Border.BorderAround(ExcelBorderStyle.Thick);
```

### Alinhamento

```csharp
var cell = worksheet.Cells["A1"];

// Horizontal
cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

// Vertical
cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
cell.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;

// Quebrar texto
cell.Style.WrapText = true;

// Rotação
cell.Style.TextRotation = 90; // 90 graus
```

### Formato de Número

```csharp
// Moeda
worksheet.Cells["B2"].Style.Numberformat.Format = "R$ #,##0.00";

// Percentual
worksheet.Cells["C2"].Style.Numberformat.Format = "0.00%";

// Data
worksheet.Cells["D2"].Style.Numberformat.Format = "dd/mm/yyyy";

// Data e hora
worksheet.Cells["E2"].Style.Numberformat.Format = "dd/mm/yyyy hh:mm:ss";

// Texto
worksheet.Cells["F2"].Style.Numberformat.Format = "@";

// Número com casas decimais
worksheet.Cells["G2"].Style.Numberformat.Format = "#,##0.00";
```

### Largura e Altura

```csharp
// Largura de coluna
worksheet.Column(1).Width = 20;
worksheet.Column(2).Width = 15.5;

// Altura de linha
worksheet.Row(1).Height = 30;

// Ocultar coluna/linha
worksheet.Column(3).Hidden = true;
worksheet.Row(5).Hidden = true;
```

---

## Fórmulas

### Fórmulas Básicas

```csharp
// Soma
worksheet.Cells["D2"].Formula = "=SUM(B2:C2)";

// Média
worksheet.Cells["E2"].Formula = "=AVERAGE(B2:B10)";

// Máximo/Mínimo
worksheet.Cells["F2"].Formula = "=MAX(B2:B10)";
worksheet.Cells["G2"].Formula = "=MIN(B2:B10)";

// Contar
worksheet.Cells["H2"].Formula = "=COUNT(B2:B10)";
worksheet.Cells["I2"].Formula = "=COUNTA(A2:A10)"; // Não vazias

// Se
worksheet.Cells["J2"].Formula = "=IF(B2>100,\"Alto\",\"Baixo\")";

// VLookup
worksheet.Cells["K2"].Formula = "=VLOOKUP(A2,Tabela!A:B,2,FALSE)";
```

### Calcular Fórmulas

```csharp
// Calcular todas as fórmulas
worksheet.Calculate();

// Ou calcular o workbook inteiro
package.Workbook.Calculate();

// Obter valor calculado
var resultado = worksheet.Cells["D2"].Value;
```

### Fórmulas com Referências

```csharp
// Referência relativa
worksheet.Cells["D2"].Formula = "=B2+C2";

// Referência absoluta
worksheet.Cells["D2"].Formula = "=$B$2+$C$2";

// Referência mista
worksheet.Cells["D2"].Formula = "=$B2+C$2";

// Outra planilha
worksheet.Cells["D2"].Formula = "=OutraAba!A1";
```

---

## Gráficos

### Gráfico de Colunas

```csharp
using OfficeOpenXml.Drawing.Chart;

var worksheet = package.Workbook.Worksheets.Add("Vendas");

// Dados
worksheet.Cells["A1"].Value = "Produto";
worksheet.Cells["B1"].Value = "Vendas";
worksheet.Cells["A2"].Value = "Mouse";
worksheet.Cells["B2"].Value = 100;
worksheet.Cells["A3"].Value = "Teclado";
worksheet.Cells["B3"].Value = 150;
worksheet.Cells["A4"].Value = "Monitor";
worksheet.Cells["B4"].Value = 80;

// Criar gráfico
var chart = worksheet.Drawings.AddChart("GraficoVendas", eChartType.ColumnClustered);

// Configurar série
var series = chart.Series.Add("B2:B4", "A2:A4");
series.Header = "Vendas por Produto";

// Posição e tamanho
chart.SetPosition(5, 0, 4, 0);
chart.SetSize(600, 400);

// Título
chart.Title.Text = "Vendas por Produto";

// Eixos
chart.XAxis.Title.Text = "Produtos";
chart.YAxis.Title.Text = "Quantidade";
```

### Gráfico de Linhas

```csharp
var chart = worksheet.Drawings.AddChart("GraficoLinhas", eChartType.Line);
var series = chart.Series.Add("B2:B10", "A2:A10");
series.Header = "Tendência";

chart.Title.Text = "Vendas ao Longo do Tempo";
chart.XAxis.Title.Text = "Meses";
chart.YAxis.Title.Text = "Vendas";
```

### Gráfico de Pizza

```csharp
var chart = worksheet.Drawings.AddChart("GraficoPizza", eChartType.Pie);
var series = chart.Series.Add("B2:B5", "A2:A5");

chart.Title.Text = "Distribuição de Vendas";
chart.Legend.Position = eLegendPosition.Right;

// Mostrar valores
var pieChart = chart as ExcelPieChart;
pieChart.DataLabel.ShowPercent = true;
pieChart.DataLabel.ShowLeaderLines = true;
```

---

## Tabelas

### Criar Tabela

```csharp
var worksheet = package.Workbook.Worksheets.Add("Dados");

// Adicionar dados
worksheet.Cells["A1"].Value = "Nome";
worksheet.Cells["B1"].Value = "Idade";
worksheet.Cells["C1"].Value = "Salário";

worksheet.Cells["A2"].Value = "João";
worksheet.Cells["B2"].Value = 30;
worksheet.Cells["C2"].Value = 5000;

// Criar tabela
var table = worksheet.Tables.Add(worksheet.Cells["A1:C2"], "TabelaPessoas");

// Estilo da tabela
table.TableStyle = TableStyles.Medium6;

// Mostrar linha de totais
table.ShowTotal = true;
table.Columns[2].TotalsRowFunction = RowFunctions.Sum;
table.Columns[2].TotalsRowLabel = "Total:";
```

### Filtros

```csharp
// Adicionar auto-filtro
worksheet.Cells["A1:C10"].AutoFilter = true;

// Aplicar filtro
// (Filtros são aplicados pelo usuário no Excel)
```

---

## Exemplos Práticos

### Exemplo 1: Relatório de Vendas

```csharp
using OfficeOpenXml;
using System.Drawing;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

class RelatorioVendas
{
    public static void GerarRelatorio(List<Venda> vendas, string caminho)
    {
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Vendas");
            
            // Cabeçalho
            worksheet.Cells["A1"].Value = "Data";
            worksheet.Cells["B1"].Value = "Produto";
            worksheet.Cells["C1"].Value = "Quantidade";
            worksheet.Cells["D1"].Value = "Valor Unitário";
            worksheet.Cells["E1"].Value = "Total";
            
            // Formatar cabeçalho
            using (var range = worksheet.Cells["A1:E1"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                range.Style.Font.Color.SetColor(Color.White);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }
            
            // Dados
            int row = 2;
            foreach (var venda in vendas)
            {
                worksheet.Cells[row, 1].Value = venda.Data;
                worksheet.Cells[row, 2].Value = venda.Produto;
                worksheet.Cells[row, 3].Value = venda.Quantidade;
                worksheet.Cells[row, 4].Value = venda.ValorUnitario;
                worksheet.Cells[row, 5].Formula = $"=C{row}*D{row}";
                
                row++;
            }
            
            // Formatação
            worksheet.Cells["A2:A" + (row - 1)].Style.Numberformat.Format = "dd/mm/yyyy";
            worksheet.Cells["D2:E" + (row - 1)].Style.Numberformat.Format = "R$ #,##0.00";
            
            // Totais
            worksheet.Cells[row, 4].Value = "TOTAL:";
            worksheet.Cells[row, 4].Style.Font.Bold = true;
            worksheet.Cells[row, 5].Formula = $"=SUM(E2:E{row - 1})";
            worksheet.Cells[row, 5].Style.Font.Bold = true;
            worksheet.Cells[row, 5].Style.Numberformat.Format = "R$ #,##0.00";
            
            // Bordas
            worksheet.Cells["A1:E" + row].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            worksheet.Cells["A1:E" + row].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            worksheet.Cells["A1:E" + row].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            worksheet.Cells["A1:E" + row].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            
            // Auto fit
            worksheet.Cells.AutoFitColumns();
            
            // Salvar
            FileInfo file = new FileInfo(caminho);
            package.SaveAs(file);
        }
        
        Console.WriteLine($"Relatório salvo em: {caminho}");
    }
}

public class Venda
{
    public DateTime Data { get; set; }
    public string Produto { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
}
```

### Exemplo 2: Importar CSV para Excel

```csharp
void ImportarCSVParaExcel(string csvPath, string excelPath)
{
    var linhas = File.ReadAllLines(csvPath);
    
    using (var package = new ExcelPackage())
    {
        var worksheet = package.Workbook.Worksheets.Add("Dados");
        
        for (int i = 0; i < linhas.Length; i++)
        {
            var colunas = linhas[i].Split(',');
            
            for (int j = 0; j < colunas.Length; j++)
            {
                worksheet.Cells[i + 1, j + 1].Value = colunas[j];
            }
        }
        
        // Formatação
        if (linhas.Length > 0)
        {
            worksheet.Cells[1, 1, 1, linhas[0].Split(',').Length].Style.Font.Bold = true;
        }
        
        worksheet.Cells.AutoFitColumns();
        
        package.SaveAs(new FileInfo(excelPath));
    }
}
```

### Exemplo 3: Dashboard com Múltiplas Abas

```csharp
void CriarDashboard(string caminho)
{
    using (var package = new ExcelPackage())
    {
        // Aba 1: Dados Brutos
        var dadosWs = package.Workbook.Worksheets.Add("Dados");
        dadosWs.Cells["A1"].LoadFromCollection(ObterDados(), true);
        
        // Aba 2: Resumo
        var resumoWs = package.Workbook.Worksheets.Add("Resumo");
        resumoWs.Cells["A1"].Value = "Total de Registros:";
        resumoWs.Cells["B1"].Formula = "=COUNTA(Dados!A:A)-1";
        
        resumoWs.Cells["A2"].Value = "Soma Total:";
        resumoWs.Cells["B2"].Formula = "=SUM(Dados!C:C)";
        
        resumoWs.Cells["A3"].Value = "Média:";
        resumoWs.Cells["B3"].Formula = "=AVERAGE(Dados!C:C)";
        
        // Aba 3: Gráficos
        var graficosWs = package.Workbook.Worksheets.Add("Gráficos");
        var chart = graficosWs.Drawings.AddChart("Grafico1", eChartType.ColumnClustered);
        chart.Series.Add("Dados!C2:C10", "Dados!A2:A10");
        chart.SetPosition(0, 0, 0, 0);
        chart.SetSize(800, 400);
        
        package.SaveAs(new FileInfo(caminho));
    }
}
```

---

## Boas Práticas

### 1. Use using para Dispose

```csharp
// ✅ BOM
using (var package = new ExcelPackage(file))
{
    // trabalhar com package
}

// ❌ RUIM
var package = new ExcelPackage(file);
// ... código sem dispose
```

### 2. Configure Licença

```csharp
// Sempre no início
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
```

### 3. Auto Fit Após Preencher

```csharp
// Preencher dados
worksheet.Cells["A1"].LoadFromCollection(dados, true);

// Depois auto fit
worksheet.Cells.AutoFitColumns();
```

### 4. Calcule Fórmulas se Necessário

```csharp
worksheet.Calculate();
// Agora pode ler valores calculados
var total = worksheet.Cells["E10"].Value;
```

### 5. Verifique Dimensões

```csharp
if (worksheet.Dimension != null)
{
    var rowCount = worksheet.Dimension.Rows;
    // processar
}
```

### 6. Performance com Grandes Volumes

```csharp
// Para grandes volumes, desabilite cálculo automático
package.Workbook.Calculate Mode = ExcelCalculationOption.Manual;

// ... adicionar dados ...

// Calcular no final
package.Workbook.Calculate();
```

---

## Recursos Adicionais

- **Site Oficial**: https://epplussoftware.com/
- **GitHub**: https://github.com/EPPlusSoftware/EPPlus
- **Documentação**: https://github.com/EPPlusSoftware/EPPlus/wiki
- **Exemplos**: https://github.com/EPPlusSoftware/EPPlus.Sample.NetCore

---

**Versão:** 1.0  
**Última atualização:** Novembro 2025
