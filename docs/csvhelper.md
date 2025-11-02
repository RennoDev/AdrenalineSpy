# CsvHelper - Exportação de Dados CSV

## O que é CsvHelper

**CsvHelper** é uma biblioteca .NET para ler e escrever arquivos CSV de forma eficiente e flexível.

**Onde é usado no AdrenalineSpy:**
- Exportar notícias coletadas para planilhas CSV
- Gerar relatórios simples de dados extraídos
- Backup de dados em formato universalmente legível  
- Integração com ferramentas de análise de dados
- Relatórios para usuários finais em formato Excel-compatível

## Como Instalar

### 1. Instalar Pacote CsvHelper

```powershell
dotnet add package CsvHelper
```

### 2. Verificar .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="CsvHelper" Version="30.0.1" />
  </ItemGroup>
</Project>
```

## Implementar no AutomationSettings.json

Adicione seção `Relatorios` para configurar exportações CSV:

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
    "DiretorioExportacao": "exports/",
    "NomeArquivoNoticias": "noticias-{data}.csv",
    "NomeArquivoResumo": "resumo-{data}.csv",
    "IncluirCabecalho": true,
    "SeparadorCSV": ",",
    "Codificacao": "UTF-8",
    "FormatoData": "yyyy-MM-dd HH:mm:ss",
    "ExportarApósExecução": true,
    "ManterHistórico": true,
    "DiasManter": 30
  },
  "Logging": {
    "Nivel": "Information",
    "CaminhoArquivo": "logs/adrenaline-spy.log"
  }
}
```

**Configurações explicadas:**
- **`HabilitarExportacaoCSV`**: Liga/desliga exportação automática para CSV
- **`DiretorioExportacao`**: Pasta onde salvar arquivos CSV
- **`NomeArquivoNoticias`**: Template do nome (usa {data} como placeholder)
- **`IncluirCabecalho`**: Incluir nomes das colunas na primeira linha
- **`SeparadorCSV`**: Separador de colunas (vírgula, ponto-vírgula, etc.)
- **`Codificacao`**: Codificação do arquivo (UTF-8 recomendado)
- **`ExportarApósExecução`**: Gerar CSV automaticamente após scraping

## Implementar no Config.cs

Adicione classe `RelatoriosConfig` ao `Config.cs`:

```csharp
public class RelatoriosConfig
{
    public bool HabilitarExportacaoCSV { get; set; } = true;
    public string DiretorioExportacao { get; set; } = "exports/";
    public string NomeArquivoNoticias { get; set; } = "noticias-{data}.csv";
    public string NomeArquivoResumo { get; set; } = "resumo-{data}.csv";
    public bool IncluirCabecalho { get; set; } = true;
    public string SeparadorCSV { get; set; } = ",";
    public string Codificacao { get; set; } = "UTF-8";
    public string FormatoData { get; set; } = "yyyy-MM-dd HH:mm:ss";
    public bool ExportarApósExecução { get; set; } = true;
    public bool ManterHistórico { get; set; } = true;
    public int DiasManter { get; set; } = 30;
}

public class Config
{
    // ... outras propriedades existentes ...
    public RelatoriosConfig Relatorios { get; set; } = new();

    // ... métodos existentes ...
    
    /// <summary>
    /// Obtém caminho completo do arquivo CSV
    /// </summary>
    public string ObterCaminhoCSV(string tipoArquivo)
    {
        // Garantir que diretório existe
        Directory.CreateDirectory(Relatorios.DiretorioExportacao);
        
        var template = tipoArquivo.ToLower() switch
        {
            "noticias" => Relatorios.NomeArquivoNoticias,
            "resumo" => Relatorios.NomeArquivoResumo,
            _ => $"{tipoArquivo}-{{data}}.csv"
        };
        
        var nomeArquivo = template.Replace("{data}", DateTime.Now.ToString("yyyy-MM-dd"));
        return Path.Combine(Relatorios.DiretorioExportacao, nomeArquivo);
    }

    /// <summary>
    /// Limpa arquivos CSV antigos conforme configuração
    /// </summary>
    public void LimparCSVsAntigos()
    {
        if (!Relatorios.ManterHistórico)
            return;

        try
        {
            var diretorio = new DirectoryInfo(Relatorios.DiretorioExportacao);
            if (!diretorio.Exists) return;

            var dataLimite = DateTime.Now.AddDays(-Relatorios.DiasManter);
            var arquivosAntigos = diretorio.GetFiles("*.csv")
                .Where(f => f.CreationTime < dataLimite);

            foreach (var arquivo in arquivosAntigos)
            {
                arquivo.Delete();
                LoggingTask.RegistrarInfo($"📁 Arquivo CSV antigo removido: {arquivo.Name}");
            }
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao limpar CSVs antigos", ex);
        }
    }
}
```

## Montar nas Tasks

Crie a classe `CsvExportTask.cs` na pasta `Workflow/Tasks/`:

```csharp
using CsvHelper;
using System.Globalization;
using System.Text;

namespace AdrenalineSpy.Workflow.Tasks;

/// <summary>
/// Gerencia exportação de dados para arquivos CSV no AdrenalineSpy
/// </summary>
public static class CsvExportTask
{
    /// <summary>
    /// Exporta notícias coletadas para CSV
    /// </summary>
    public static async Task<bool> ExportarNoticias(List<Noticia> noticias)
    {
        try
        {
            if (!Config.Instancia.Relatorios.HabilitarExportacaoCSV)
            {
                LoggingTask.RegistrarInfo("📄 Exportação CSV desabilitada nas configurações");
                return true;
            }

            if (noticias?.Any() != true)
            {
                LoggingTask.RegistrarAviso("📄 Nenhuma notícia para exportar");
                return false;
            }

            var caminhoArquivo = Config.Instancia.ObterCaminhoCSV("noticias");
            
            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            // Configurar separador personalizado
            csv.Context.Configuration.Delimiter = Config.Instancia.Relatorios.SeparadorCSV;
            
            // Escrever cabeçalho se configurado
            if (Config.Instancia.Relatorios.IncluirCabecalho)
            {
                csv.WriteHeader<NoticiaCSV>();
                csv.NextRecord();
            }

            // Converter notícias para formato CSV
            var noticiasCSV = noticias.Select(n => new NoticiaCSV
            {
                Titulo = n.Titulo,
                Categoria = n.Categoria,
                Url = n.Url,
                DataPublicacao = n.DataPublicacao.ToString(Config.Instancia.Relatorios.FormatoData),
                Conteudo = LimparTextoParaCSV(n.Conteudo),
                DataColeta = DateTime.Now.ToString(Config.Instancia.Relatorios.FormatoData),
                Fonte = "Adrenaline.com.br"
            });

            csv.WriteRecords(noticiasCSV);

            // Salvar arquivo com encoding configurado
            var encoding = Encoding.GetEncoding(Config.Instancia.Relatorios.Codificacao);
            await File.WriteAllTextAsync(caminhoArquivo, writer.ToString(), encoding);

            LoggingTask.RegistrarInfo($"📄 CSV exportado: {Path.GetFileName(caminhoArquivo)} ({noticias.Count} notícias)");
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao exportar notícias para CSV", ex);
            return false;
        }
    }

    /// <summary>
    /// Exporta resumo estatístico das categorias para CSV
    /// </summary>
    public static async Task<bool> ExportarResumo(DateTime dataExecucao, int totalNoticias, Dictionary<string, int> porCategoria)
    {
        try
        {
            if (!Config.Instancia.Relatorios.HabilitarExportacaoCSV)
                return true;

            var caminhoArquivo = Config.Instancia.ObterCaminhoCSV("resumo");
            
            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.Context.Configuration.Delimiter = Config.Instancia.Relatorios.SeparadorCSV;

            if (Config.Instancia.Relatorios.IncluirCabecalho)
            {
                csv.WriteHeader<ResumoCSV>();
                csv.NextRecord();
            }

            // Criar resumo geral
            var resumos = new List<ResumoCSV>
            {
                new ResumoCSV
                {
                    DataExecucao = dataExecucao.ToString(Config.Instancia.Relatorios.FormatoData),
                    Categoria = "TOTAL",
                    Quantidade = totalNoticias,
                    Percentual = 100.0
                }
            };

            // Adicionar resumo por categoria
            foreach (var categoria in porCategoria)
            {
                resumos.Add(new ResumoCSV
                {
                    DataExecucao = dataExecucao.ToString(Config.Instancia.Relatorios.FormatoData),
                    Categoria = categoria.Key,
                    Quantidade = categoria.Value,
                    Percentual = totalNoticias > 0 ? (categoria.Value * 100.0) / totalNoticias : 0
                });
            }

            csv.WriteRecords(resumos);

            var encoding = Encoding.GetEncoding(Config.Instancia.Relatorios.Codificacao);
            await File.WriteAllTextAsync(caminhoArquivo, writer.ToString(), encoding);

            LoggingTask.RegistrarInfo($"📊 Resumo CSV exportado: {Path.GetFileName(caminhoArquivo)}");
            return true;
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao exportar resumo para CSV", ex);
            return false;
        }
    }

    /// <summary>
    /// Executa limpeza de arquivos CSV antigos
    /// </summary>
    public static void LimparArquivosAntigos()
    {
        try
        {
            Config.Instancia.LimparCSVsAntigos();
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao limpar CSVs antigos", ex);
        }
    }

    /// <summary>
    /// Limpa texto para evitar problemas no CSV
    /// </summary>
    private static string LimparTextoParaCSV(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        // Remover quebras de linha e caracteres problemáticos
        return texto
            .Replace('\n', ' ')
            .Replace('\r', ' ')
            .Replace('\t', ' ')
            .Replace("\"", "'")
            .Trim();
    }
}

/// <summary>
/// Modelo para exportação de notícias em CSV
/// </summary>
public class NoticiaCSV
{
    public string Titulo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string DataPublicacao { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public string DataColeta { get; set; } = string.Empty;
    public string Fonte { get; set; } = string.Empty;
}

/// <summary>
/// Modelo para exportação de resumo em CSV
/// </summary>
public class ResumoCSV
{
    public string DataExecucao { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public double Percentual { get; set; }
}
```

## Métodos Mais Usados

### Escrever Lista de Objetos para CSV

```csharp
using CsvHelper;
using System.Globalization;

// Exportar dados coletados do AdrenalineSpy
var noticias = await MigrationTask.ObterNoticias(DateTime.Today);

using var writer = new StringWriter();
using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

// Configurar separador (vírgula, ponto-vírgula, etc.)
csv.Context.Configuration.Delimiter = ";";

// Escrever cabeçalho
csv.WriteHeader<Noticia>();
csv.NextRecord();

// Escrever dados
csv.WriteRecords(noticias);

// Salvar arquivo
await File.WriteAllTextAsync("noticias.csv", writer.ToString());
LoggingTask.RegistrarInfo($"✅ Exportadas {noticias.Count} notícias para CSV");
```

### Ler CSV para Lista de Objetos

```csharp
// Ler arquivo CSV de configurações ou dados externos
using var reader = new StringReader(await File.ReadAllTextAsync("dados.csv"));
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

csv.Context.Configuration.Delimiter = ";";

var registros = csv.GetRecords<Categoria>().ToList();
LoggingTask.RegistrarInfo($"📖 Carregados {registros.Count} registros do CSV");
```

### Configuração Personalizada

```csharp
// Configuração avançada para CSVs específicos do projeto
var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
{
    Delimiter = Config.Instancia.Relatorios.SeparadorCSV,
    HasHeaderRecord = Config.Instancia.Relatorios.IncluirCabecalho,
    MissingFieldFound = null, // Ignora campos faltantes
    BadDataFound = null,      // Ignora dados mal formatados
    Encoding = Encoding.GetEncoding(Config.Instancia.Relatorios.Codificacao)
};

using var csv = new CsvWriter(writer, config);
```

### Mapeamento de Campos Customizado

```csharp
// Classe de mapeamento para controlar nomes das colunas
public class NoticiaCsvMap : ClassMap<Noticia>
{
    public NoticiaCsvMap()
    {
        Map(n => n.Titulo).Name("Título da Notícia");
        Map(n => n.Categoria).Name("Categoria");
        Map(n => n.Url).Name("Link");
        Map(n => n.DataPublicacao).Name("Data de Publicação");
        Map(n => n.Conteudo).Name("Conteúdo");
    }
}

// Usar o mapeamento
csv.Context.Configuration.RegisterClassMap<NoticiaCsvMap>();
```

### Integração com Workflow Principal

```csharp
// No Workflow.cs principal - integrar CsvExportTask
public async Task<bool> ExecutarScrapingCompleto()
{
    try
    {
        // 1. Limpar arquivos antigos
        CsvExportTask.LimparArquivosAntigos();
        
        var inicioExecucao = DateTime.Now;
        
        // 2. Executar scraping normal...
        var noticias = await ExtractionTask.ColetarTodasNoticias();
        await MigrationTask.SalvarNoticias(noticias);
        
        // 3. Exportar para CSV se habilitado
        if (Config.Instancia.Relatorios.ExportarApósExecução)
        {
            await CsvExportTask.ExportarNoticias(noticias);
            
            // Gerar resumo por categoria
            var porCategoria = noticias.GroupBy(n => n.Categoria)
                .ToDictionary(g => g.Key, g => g.Count());
            
            await CsvExportTask.ExportarResumo(inicioExecucao, noticias.Count, porCategoria);
        }
        
        LoggingTask.RegistrarInfo($"🎯 Scraping + Export completo: {noticias.Count} notícias");
        return true;
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro("Erro no workflow completo", ex);
        return false;
    }
}
```
