# CsvHelper - Manipulação de Arquivos CSV

## Índice
1. [Introdução](#introdução)
2. [Instalação](#instalação)
3. [Conceitos Básicos](#conceitos-básicos)
4. [Ler CSV](#ler-csv)
5. [Escrever CSV](#escrever-csv)
6. [Mapeamento](#mapeamento)
7. [Configurações](#configurações)
8. [Exemplos Práticos](#exemplos-práticos)
9. [Boas Práticas](#boas-práticas)

---

## Introdução

**CsvHelper** é a biblioteca mais popular para ler e escrever arquivos CSV em .NET. Oferece suporte robusto para diferentes formatos e delimitadores.

### Vantagens
- ✅ Simples e intuitivo
- ✅ Mapeamento automático de objetos
- ✅ Suporte a diferentes delimitadores
- ✅ Configurável e extensível
- ✅ Performance excelente
- ✅ Suporte a encoding

---

## Instalação

```bash
dotnet add package CsvHelper
```

---

## Conceitos Básicos

### Estrutura Básica

```
CSV File → StreamReader → CsvReader → Records
Records → CsvWriter → StreamWriter → CSV File
```

### Namespaces

```csharp
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
```

---

## Ler CSV

### Ler para Lista de Objetos

```csharp
using CsvHelper;
using System.Globalization;

public class Pessoa
{
    public string Nome { get; set; }
    public int Idade { get; set; }
    public string Email { get; set; }
}

// Ler CSV
using (var reader = new StreamReader("pessoas.csv"))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    var pessoas = csv.GetRecords<Pessoa>().ToList();
    
    foreach (var pessoa in pessoas)
    {
        Console.WriteLine($"{pessoa.Nome}, {pessoa.Idade}, {pessoa.Email}");
    }
}
```

### Ler sem Classe (Dynamic)

```csharp
using (var reader = new StreamReader("dados.csv"))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    var records = csv.GetRecords<dynamic>().ToList();
    
    foreach (var record in records)
    {
        Console.WriteLine($"{record.Nome} - {record.Idade}");
    }
}
```

### Ler Linha por Linha

```csharp
using (var reader = new StreamReader("dados.csv"))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    csv.Read();
    csv.ReadHeader();
    
    while (csv.Read())
    {
        var nome = csv.GetField<string>("Nome");
        var idade = csv.GetField<int>("Idade");
        
        Console.WriteLine($"{nome}: {idade} anos");
    }
}
```

### Ler por Índice

```csharp
using (var reader = new StreamReader("dados.csv"))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    while (csv.Read())
    {
        var campo0 = csv.GetField(0);  // Primeira coluna
        var campo1 = csv.GetField(1);  // Segunda coluna
        var campo2 = csv.GetField<int>(2);  // Terceira coluna como int
        
        Console.WriteLine($"{campo0}, {campo1}, {campo2}");
    }
}
```

### Ler CSV Sem Cabeçalho

```csharp
var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    HasHeaderRecord = false
};

using (var reader = new StreamReader("sem-cabecalho.csv"))
using (var csv = new CsvReader(reader, config))
{
    while (csv.Read())
    {
        var valor1 = csv.GetField(0);
        var valor2 = csv.GetField(1);
        Console.WriteLine($"{valor1}, {valor2}");
    }
}
```

---

## Escrever CSV

### Escrever Lista de Objetos

```csharp
var pessoas = new List<Pessoa>
{
    new Pessoa { Nome = "João", Idade = 30, Email = "joao@email.com" },
    new Pessoa { Nome = "Maria", Idade = 25, Email = "maria@email.com" },
    new Pessoa { Nome = "Pedro", Idade = 35, Email = "pedro@email.com" }
};

using (var writer = new StreamWriter("saida.csv"))
using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
{
    csv.WriteRecords(pessoas);
}
```

### Escrever Campo por Campo

```csharp
using (var writer = new StreamWriter("saida.csv"))
using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
{
    // Cabeçalho
    csv.WriteField("Nome");
    csv.WriteField("Idade");
    csv.WriteField("Email");
    csv.NextRecord();
    
    // Dados
    csv.WriteField("João");
    csv.WriteField(30);
    csv.WriteField("joao@email.com");
    csv.NextRecord();
    
    csv.WriteField("Maria");
    csv.WriteField(25);
    csv.WriteField("maria@email.com");
    csv.NextRecord();
}
```

### Escrever Dynamic

```csharp
var records = new List<dynamic>
{
    new { Nome = "João", Idade = 30 },
    new { Nome = "Maria", Idade = 25 }
};

using (var writer = new StreamWriter("dinamico.csv"))
using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
{
    csv.WriteRecords(records);
}
```

### Append (Adicionar ao Final)

```csharp
var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    HasHeaderRecord = false  // Não escrever cabeçalho novamente
};

using (var stream = File.Open("arquivo.csv", FileMode.Append))
using (var writer = new StreamWriter(stream))
using (var csv = new CsvWriter(writer, config))
{
    csv.WriteRecord(new Pessoa { Nome = "Novo", Idade = 40, Email = "novo@email.com" });
    csv.NextRecord();
}
```

---

## Mapeamento

### Class Map Customizado

```csharp
using CsvHelper.Configuration;

public class PessoaMap : ClassMap<Pessoa>
{
    public PessoaMap()
    {
        // Mapear por nome de coluna
        Map(m => m.Nome).Name("NomeCompleto");
        Map(m => m.Idade).Name("Anos");
        Map(m => m.Email).Name("EmailPessoal");
        
        // Mapear por índice
        // Map(m => m.Nome).Index(0);
        // Map(m => m.Idade).Index(1);
        
        // Ignorar propriedade
        // Map(m => m.PropriedadeIgnorada).Ignore();
    }
}

// Usar o mapeamento
using (var reader = new StreamReader("pessoas.csv"))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    csv.Context.RegisterClassMap<PessoaMap>();
    var pessoas = csv.GetRecords<Pessoa>().ToList();
}
```

### Map com Conversão

```csharp
public class ProdutoMap : ClassMap<Produto>
{
    public ProdutoMap()
    {
        Map(m => m.Nome);
        
        // Converter string para decimal
        Map(m => m.Preco).Convert(args =>
        {
            var valor = args.Row.GetField("Preco");
            valor = valor.Replace("R$", "").Replace(",", "").Trim();
            return decimal.Parse(valor) / 100;
        });
        
        // Data customizada
        Map(m => m.DataCadastro).Convert(args =>
        {
            var data = args.Row.GetField("Data");
            return DateTime.ParseExact(data, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        });
    }
}
```

### Map Opcional

```csharp
public class ClienteMap : ClassMap<Cliente>
{
    public ClienteMap()
    {
        Map(m => m.Nome);
        Map(m => m.Email).Optional();  // Coluna pode não existir
        Map(m => m.Telefone).Default("Não informado");  // Valor padrão
    }
}
```

---

## Configurações

### Delimitador Customizado

```csharp
// Ponto e vírgula
var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    Delimiter = ";"
};

// Tab
var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    Delimiter = "\t"
};

// Pipe
var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    Delimiter = "|"
};

using (var reader = new StreamReader("dados.csv"))
using (var csv = new CsvReader(reader, config))
{
    var records = csv.GetRecords<MyClass>().ToList();
}
```

### Configurações Comuns

```csharp
var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    // Delimitador
    Delimiter = ",",
    
    // Cabeçalho
    HasHeaderRecord = true,
    
    // Encoding
    Encoding = Encoding.UTF8,
    
    // Ignorar linhas vazias
    ShouldSkipRecord = args => args.Row.Parser.Record.All(string.IsNullOrWhiteSpace),
    
    // Modo de leitura
    Mode = CsvMode.RFC4180,
    
    // Trim spaces
    TrimOptions = TrimOptions.Trim,
    
    // Ignorar referências vazias
    IgnoreBlankLines = true,
    
    // Case insensitive headers
    PrepareHeaderForMatch = args => args.Header.ToLower(),
    
    // Quote character
    Quote = '"',
    
    // Detectar delimitador automaticamente
    DetectColumnCountChanges = true
};
```

### Culture Info (Formato Regional)

```csharp
// Brasil (pt-BR)
var config = new CsvConfiguration(new CultureInfo("pt-BR"))
{
    Delimiter = ";"
};

// EUA (en-US)
var config = new CsvConfiguration(new CultureInfo("en-US"))
{
    Delimiter = ","
};

// Invariant (padrão)
var config = new CsvConfiguration(CultureInfo.InvariantCulture);
```

---

## Exemplos Práticos

### Exemplo 1: Converter CSV para JSON

```csharp
using System.Text.Json;

void ConverterCSVparaJSON(string csvPath, string jsonPath)
{
    using (var reader = new StreamReader(csvPath))
    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    {
        var records = csv.GetRecords<dynamic>().ToList();
        
        var json = JsonSerializer.Serialize(records, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        File.WriteAllText(jsonPath, json);
    }
    
    Console.WriteLine($"Convertido para: {jsonPath}");
}
```

### Exemplo 2: Filtrar e Exportar

```csharp
void FiltrarEExportar(string inputPath, string outputPath, int idadeMinima)
{
    using (var reader = new StreamReader(inputPath))
    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    {
        var pessoas = csv.GetRecords<Pessoa>()
            .Where(p => p.Idade >= idadeMinima)
            .ToList();
        
        using (var writer = new StreamWriter(outputPath))
        using (var csvOut = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csvOut.WriteRecords(pessoas);
        }
    }
    
    Console.WriteLine($"Filtrados: {pessoas.Count} registros");
}
```

### Exemplo 3: Mesclar Múltiplos CSVs

```csharp
void MesclarCSVs(string[] arquivos, string saida)
{
    var todosRegistros = new List<Pessoa>();
    
    foreach (var arquivo in arquivos)
    {
        using (var reader = new StreamReader(arquivo))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var registros = csv.GetRecords<Pessoa>().ToList();
            todosRegistros.AddRange(registros);
        }
    }
    
    using (var writer = new StreamWriter(saida))
    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
    {
        csv.WriteRecords(todosRegistros);
    }
    
    Console.WriteLine($"Total mesclado: {todosRegistros.Count} registros");
}
```

### Exemplo 4: CSV com Validação

```csharp
void ImportarComValidacao(string path)
{
    var erros = new List<string>();
    var validos = new List<Pessoa>();
    int linha = 1;
    
    using (var reader = new StreamReader(path))
    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    {
        csv.Read();
        csv.ReadHeader();
        
        while (csv.Read())
        {
            linha++;
            
            try
            {
                var pessoa = new Pessoa
                {
                    Nome = csv.GetField<string>("Nome"),
                    Idade = csv.GetField<int>("Idade"),
                    Email = csv.GetField<string>("Email")
                };
                
                // Validações
                if (string.IsNullOrWhiteSpace(pessoa.Nome))
                {
                    erros.Add($"Linha {linha}: Nome vazio");
                    continue;
                }
                
                if (pessoa.Idade < 0 || pessoa.Idade > 150)
                {
                    erros.Add($"Linha {linha}: Idade inválida ({pessoa.Idade})");
                    continue;
                }
                
                if (!pessoa.Email.Contains("@"))
                {
                    erros.Add($"Linha {linha}: Email inválido ({pessoa.Email})");
                    continue;
                }
                
                validos.Add(pessoa);
            }
            catch (Exception ex)
            {
                erros.Add($"Linha {linha}: {ex.Message}");
            }
        }
    }
    
    Console.WriteLine($"Válidos: {validos.Count}");
    Console.WriteLine($"Erros: {erros.Count}");
    
    foreach (var erro in erros)
    {
        Console.WriteLine(erro);
    }
}
```

### Exemplo 5: Exportar com Formatação

```csharp
public class RelatorioVendas
{
    public DateTime Data { get; set; }
    public string Produto { get; set; }
    public decimal Valor { get; set; }
}

public class RelatorioVendasMap : ClassMap<RelatorioVendas>
{
    public RelatorioVendasMap()
    {
        Map(m => m.Data).Name("Data").TypeConverterOption.Format("dd/MM/yyyy");
        Map(m => m.Produto).Name("Produto");
        Map(m => m.Valor).Name("Valor (R$)").TypeConverterOption.Format("N2");
    }
}

void ExportarRelatorio()
{
    var vendas = new List<RelatorioVendas>
    {
        new RelatorioVendas { Data = DateTime.Now, Produto = "Mouse", Valor = 50.00m },
        new RelatorioVendas { Data = DateTime.Now, Produto = "Teclado", Valor = 150.00m }
    };
    
    using (var writer = new StreamWriter("relatorio.csv"))
    using (var csv = new CsvWriter(writer, new CultureInfo("pt-BR")))
    {
        csv.Context.RegisterClassMap<RelatorioVendasMap>();
        csv.WriteRecords(vendas);
    }
}
```

### Exemplo 6: Ler CSV Grande (Streaming)

```csharp
// Processar arquivo grande sem carregar tudo na memória
void ProcessarCSVGrande(string path)
{
    int processados = 0;
    
    using (var reader = new StreamReader(path))
    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    {
        var records = csv.GetRecords<Pessoa>();
        
        foreach (var pessoa in records)
        {
            // Processar registro por registro
            ProcessarPessoa(pessoa);
            
            processados++;
            
            if (processados % 1000 == 0)
            {
                Console.WriteLine($"Processados: {processados}");
            }
        }
    }
    
    Console.WriteLine($"Total processado: {processados}");
}

void ProcessarPessoa(Pessoa pessoa)
{
    // Lógica de processamento
    // Ex: salvar no banco, enviar email, etc.
}
```

---

## Boas Práticas

### 1. Use using para Dispose

```csharp
// ✅ BOM
using (var reader = new StreamReader("dados.csv"))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    // código
}

// ❌ RUIM - sem dispose
var reader = new StreamReader("dados.csv");
var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
```

### 2. Especifique Culture

```csharp
// ✅ BOM - específico
var csv = new CsvReader(reader, new CultureInfo("pt-BR"));

// ✅ BOM - invariante
var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

// ❌ EVITE - pode causar problemas
var csv = new CsvReader(reader, CultureInfo.CurrentCulture);
```

### 3. Valide Dados

```csharp
try
{
    var idade = csv.GetField<int>("Idade");
}
catch (Exception ex)
{
    Console.WriteLine($"Erro ao ler idade: {ex.Message}");
    // Usar valor padrão ou pular registro
}
```

### 4. Use ClassMap para CSVs Complexos

```csharp
// Para CSVs com colunas não padrão, sempre use ClassMap
csv.Context.RegisterClassMap<MyClassMap>();
```

### 5. Trate Encoding

```csharp
// UTF-8 com BOM
using (var reader = new StreamReader(path, Encoding.UTF8))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    // ...
}

// Latin1 / ISO-8859-1
using (var reader = new StreamReader(path, Encoding.Latin1))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    // ...
}
```

### 6. Para Arquivos Grandes, Use Streaming

```csharp
// ✅ BOM - streaming, não carrega tudo na memória
var records = csv.GetRecords<Pessoa>();
foreach (var pessoa in records)
{
    ProcessarPessoa(pessoa);
}

// ❌ RUIM para arquivos grandes - carrega tudo
var todasPessoas = csv.GetRecords<Pessoa>().ToList();
```

---

## Troubleshooting

### Problema: "Header matching ['Nome'] names at index 0 was not found"

**Solução**: Nome da propriedade não corresponde ao cabeçalho do CSV.
```csharp
// Opção 1: Usar ClassMap
Map(m => m.Nome).Name("NomeCompleto");

// Opção 2: Case insensitive
var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    PrepareHeaderForMatch = args => args.Header.ToLower()
};
```

### Problema: Encoding errado (caracteres estranhos)

**Solução**:
```csharp
using (var reader = new StreamReader(path, Encoding.UTF8))
// ou
using (var reader = new StreamReader(path, Encoding.Latin1))
```

### Problema: Delimitador errado

**Solução**:
```csharp
var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    Delimiter = ";" // ou "\t" ou "|"
};
```

---

## Recursos Adicionais

- **GitHub**: https://github.com/JoshClose/CsvHelper
- **Documentação**: https://joshclose.github.io/CsvHelper/
- **Examples**: https://joshclose.github.io/CsvHelper/examples

---

**Versão:** 1.0  
**Última atualização:** Novembro 2025
