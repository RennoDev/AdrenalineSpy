# RestSharp + JSON - APIs e Serialização

## O que é RestSharp + JSON

**RestSharp** é uma biblioteca para consumo de APIs HTTP/REST, enquanto **Newtonsoft.Json** é o padrão para serialização JSON no .NET.

**Onde é usado no AdrenalineSpy:**
- Consumir APIs externas (se necessário para enriquecimento de dados)
- Deserializar configurações de `AutomationSettings.json`
- Serializar dados extraídos antes de salvar no banco
- Comunicação com APIs de terceiros (webhooks, notificações)
- Exportar dados coletados para APIs externas

## Como Instalar

### 1. Instalar RestSharp

```powershell
dotnet add package RestSharp
```

### 2. Instalar Newtonsoft.Json

```powershell
dotnet add package Newtonsoft.Json
```

### 3. Verificar .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="RestSharp" Version="110.2.0" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
</Project>
```

## Implementar no AutomationSettings.json

Adicione seção `API` para configurar APIs externas (se necessário):

```json
{
  "Navegacao": {
    "UrlBase": "https://www.adrenaline.com.br",
    "DelayEntrePaginas": 2000
  },
  "Database": {
    "ConnectionString": "Server=localhost;Database=AdrenalineSpy;..."
  },
  "API": {
    "HabilitarIntegracao": false,
    "BaseUrl": "https://api.exemplo.com",
    "ApiKey": "",
    "Timeout": 30000,
    "MaxRetries": 3,
    "Endpoints": {
      "EnviarNoticia": "/noticias",
      "ObterCategoria": "/categorias/{id}",
      "Webhook": "/webhook/adrenaline"
    }
  },
  "Logging": {
    "Nivel": "Information",
    "CaminhoArquivo": "logs/adrenaline-spy.log"
  }
}
```

**Configurações explicadas:**
- **`HabilitarIntegracao`**: Liga/desliga integração com APIs
- **`BaseUrl`**: URL base da API externa
- **`ApiKey`**: Chave de autenticação da API
- **`Timeout`**: Timeout em milissegundos para requests
- **`MaxRetries`**: Máximo de tentativas em caso de falha
- **`Endpoints`**: URLs específicas de cada operação

## Implementar no Config.cs

Adicione classe `APIConfig` ao `Config.cs`:

```csharp
public class APIConfig
{
    public bool HabilitarIntegracao { get; set; } = false;
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int Timeout { get; set; } = 30000;
    public int MaxRetries { get; set; } = 3;
    public Dictionary<string, string> Endpoints { get; set; } = new();
}

public class Config
{
    // ... outras propriedades existentes ...
    public APIConfig API { get; set; } = new();

    // ... métodos existentes ...
    
    /// <summary>
    /// Cria cliente RestSharp configurado para APIs externas
    /// </summary>
    public RestClient CriarClienteAPI()
    {
        if (!API.HabilitarIntegracao || string.IsNullOrWhiteSpace(API.BaseUrl))
        {
            LoggingTask.RegistrarAviso("API externa desabilitada ou não configurada");
            return null;
        }

        var options = new RestClientOptions(API.BaseUrl)
        {
            MaxTimeout = API.Timeout,
            ThrowOnAnyError = false
        };

        var client = new RestClient(options);
        
        // Adicionar header de autenticação se configurado
        if (!string.IsNullOrWhiteSpace(API.ApiKey))
        {
            client.AddDefaultHeader("Authorization", $"Bearer {API.ApiKey}");
        }

        LoggingTask.RegistrarInfo($"Cliente API criado para: {API.BaseUrl}");
        return client;
    }

    /// <summary>
    /// Obtém URL completa do endpoint
    /// </summary>
    public string ObterEndpoint(string nomeEndpoint, params object[] parametros)
    {
        if (!API.Endpoints.TryGetValue(nomeEndpoint, out var endpoint))
        {
            LoggingTask.RegistrarAviso($"Endpoint '{nomeEndpoint}' não encontrado na configuração");
            return string.Empty;
        }

        return string.Format(endpoint, parametros);
    }
}

## Montar nas Tasks

Crie a classe `ApiTask.cs` na pasta `Workflow/Tasks/`:

```csharp
using RestSharp;
using Newtonsoft.Json;

namespace AdrenalineSpy.Workflow.Tasks;

/// <summary>
/// Gerencia integração com APIs externas do AdrenalineSpy
/// </summary>
public static class ApiTask
{
    /// <summary>
    /// Envia notícia coletada para API externa
    /// </summary>
    public static async Task<bool> EnviarNoticia(Noticia noticia)
    {
        try
        {
            if (!Config.Instancia.API.HabilitarIntegracao)
            {
                LoggingTask.RegistrarInfo("🔌 API externa desabilitada - notícia não enviada");
                return true; // Não é erro, apenas desabilitado
            }

            using var client = Config.Instancia.CriarClienteAPI();
            if (client == null) return false;

            var endpoint = Config.Instancia.ObterEndpoint("EnviarNoticia");
            var request = new RestRequest(endpoint, Method.Post);

            // Serializar notícia para JSON
            var noticiaJson = new
            {
                titulo = noticia.Titulo,
                categoria = noticia.Categoria,
                url = noticia.Url,
                dataPublicacao = noticia.DataPublicacao,
                conteudo = noticia.Conteudo,
                fonte = "Adrenaline.com.br",
                coletadoEm = DateTime.Now
            };

            request.AddJsonBody(noticiaJson);

            var response = await ExecutarComRetry(client, request, Config.Instancia.API.MaxRetries);
            
            if (response.IsSuccessful)
            {
                LoggingTask.RegistrarInfo($"✅ Notícia enviada para API: {noticia.Titulo}");
                return true;
            }
            else
            {
                LoggingTask.RegistrarErro($"❌ Falha ao enviar notícia para API: {response.ErrorMessage}");
                return false;
            }
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao enviar notícia para API externa", ex);
            return false;
        }
    }

    /// <summary>
    /// Notifica API externa sobre conclusão de scraping
    /// </summary>
    public static async Task<bool> NotificarExecucaoCompleta(int totalNoticias, DateTime inicioExecucao)
    {
        try
        {
            if (!Config.Instancia.API.HabilitarIntegracao)
                return true;

            using var client = Config.Instancia.CriarClienteAPI();
            if (client == null) return false;

            var endpoint = Config.Instancia.ObterEndpoint("Webhook");
            var request = new RestRequest(endpoint, Method.Post);

            var webhook = new
            {
                evento = "scraping_completo",
                fonte = "AdrenalineSpy",
                dados = new
                {
                    totalNoticias = totalNoticias,
                    inicioExecucao = inicioExecucao,
                    fimExecucao = DateTime.Now,
                    duracao = (DateTime.Now - inicioExecucao).TotalMinutes,
                    site = "adrenaline.com.br"
                }
            };

            request.AddJsonBody(webhook);

            var response = await ExecutarComRetry(client, request, Config.Instancia.API.MaxRetries);
            
            if (response.IsSuccessful)
            {
                LoggingTask.RegistrarInfo($"🔔 Webhook enviado: {totalNoticias} notícias processadas");
                return true;
            }
            else
            {
                LoggingTask.RegistrarAviso($"⚠️ Falha ao enviar webhook: {response.ErrorMessage}");
                return false;
            }
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarErro("Erro ao enviar webhook", ex);
            return false;
        }
    }

    /// <summary>
    /// Testa conectividade com API externa
    /// </summary>
    public static async Task<bool> TestarConectividade()
    {
        try
        {
            if (!Config.Instancia.API.HabilitarIntegracao)
            {
                LoggingTask.RegistrarInfo("🔌 API externa desabilitada - teste ignorado");
                return true;
            }

            using var client = Config.Instancia.CriarClienteAPI();
            if (client == null) return false;

            var request = new RestRequest("/health", Method.Get);
            request.Timeout = TimeSpan.FromSeconds(10); // Timeout curto para teste

            var response = await client.ExecuteAsync(request);
            
            if (response.IsSuccessful)
            {
                LoggingTask.RegistrarInfo("✅ API externa disponível");
                return true;
            }
            else
            {
                LoggingTask.RegistrarAviso($"⚠️ API externa indisponível: {response.ErrorMessage}");
                return false;
            }
        }
        catch (Exception ex)
        {
            LoggingTask.RegistrarAviso("⚠️ Falha ao testar API externa", ex);
            return false;
        }
    }

    /// <summary>
    /// Executa request com retry automático
    /// </summary>
    private static async Task<RestResponse> ExecutarComRetry(RestClient client, RestRequest request, int maxTentativas)
    {
        RestResponse response = null;
        
        for (int tentativa = 1; tentativa <= maxTentativas; tentativa++)
        {
            try
            {
                response = await client.ExecuteAsync(request);
                
                if (response.IsSuccessful)
                {
                    if (tentativa > 1)
                        LoggingTask.RegistrarInfo($"✅ Request bem-sucedido na tentativa {tentativa}");
                    
                    return response;
                }
                
                if (tentativa < maxTentativas)
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, tentativa)); // Backoff exponencial
                    LoggingTask.RegistrarAviso($"⚠️ Tentativa {tentativa} falhou. Retry em {delay.TotalSeconds}s");
                    await Task.Delay(delay);
                }
            }
            catch (Exception ex) when (tentativa < maxTentativas)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, tentativa));
                LoggingTask.RegistrarAviso($"⚠️ Erro na tentativa {tentativa}: {ex.Message}. Retry em {delay.TotalSeconds}s");
                await Task.Delay(delay);
            }
        }
        
        LoggingTask.RegistrarErro($"❌ Request falhou após {maxTentativas} tentativas");
        return response;
    }
}

## Métodos Mais Usados

### Serializar JSON (Objeto → String)

```csharp
using Newtonsoft.Json;

// Configurações do AdrenalineSpy para JSON
var settings = new JsonSerializerSettings
{
    NullValueHandling = NullValueHandling.Ignore,
    DateTimeZoneHandling = DateTimeZoneHandling.Local,
    Formatting = Formatting.Indented
};

// Serializar notícia extraída
var noticia = new Noticia
{
    Titulo = "Nova GPU lançada",
    Categoria = "Hardware",
    Url = "https://adrenaline.com.br/artigos/nova-gpu",
    DataPublicacao = DateTime.Now
};

string json = JsonConvert.SerializeObject(noticia, settings);
LoggingTask.RegistrarInfo($"Notícia serializada: {json}");
```

### Deserializar JSON (String → Objeto)

```csharp
// Carregar configurações do AutomationSettings.json
string configJson = File.ReadAllText("AutomationSettings.json");
var config = JsonConvert.DeserializeObject<AutomationSettings>(configJson);

// Deserializar resposta de API
string responseJson = @"{
    ""titulo"": ""Breaking News"",
    ""categoria"": ""Tecnologia"",
    ""dataPublicacao"": ""2024-11-02T10:00:00""
}";

var noticia = JsonConvert.DeserializeObject<Noticia>(responseJson);
LoggingTask.RegistrarInfo($"Notícia carregada: {noticia.Titulo}");
```

### GET Request com RestSharp

```csharp
// Verificar status de API externa
using var client = Config.Instancia.CriarClienteAPI();
if (client != null)
{
    var request = new RestRequest("/status", Method.Get);
    
    // Adicionar parâmetros de query se necessário
    request.AddParameter("source", "adrenaline-spy");
    request.AddParameter("version", "1.0");
    
    var response = await client.ExecuteAsync(request);
    
    if (response.IsSuccessful)
    {
        dynamic status = JsonConvert.DeserializeObject(response.Content);
        LoggingTask.RegistrarInfo($"API Status: {status.status}");
    }
}
```

### POST Request com JSON

```csharp
// Enviar dados coletados para API externa
var dadosEnvio = new
{
    fonte = "AdrenalineSpy",
    site = "adrenaline.com.br",
    timestamp = DateTime.UtcNow,
    noticias = new[]
    {
        new { titulo = "Notícia 1", categoria = "Tech" },
        new { titulo = "Notícia 2", categoria = "Games" }
    }
};

using var client = Config.Instancia.CriarClienteAPI();
if (client != null)
{
    var request = new RestRequest("/dados", Method.Post);
    request.AddJsonBody(dadosEnvio);
    
    var response = await client.ExecuteAsync(request);
    
    if (response.IsSuccessful)
    {
        LoggingTask.RegistrarInfo("✅ Dados enviados com sucesso");
    }
    else
    {
        LoggingTask.RegistrarErro($"❌ Erro ao enviar dados: {response.ErrorMessage}");
    }
}
```

### Trabalhar com JObject (JSON Dinâmico)

```csharp
using Newtonsoft.Json.Linq;

// Parse de JSON desconhecido/dinâmico
string jsonResponse = await ObterDadosDeAPI();
var jsonObj = JObject.Parse(jsonResponse);

// Navegar propriedades dinamicamente
if (jsonObj["success"]?.Value<bool>() == true)
{
    var dados = jsonObj["data"];
    
    foreach (var item in dados.Children())
    {
        string titulo = item["titulo"]?.Value<string>();
        string categoria = item["categoria"]?.Value<string>();
        
        LoggingTask.RegistrarInfo($"Item encontrado: {titulo} ({categoria})");
    }
}
```

### Headers e Autenticação

```csharp
// Configurar headers personalizados
var request = new RestRequest("/protected-endpoint", Method.Get);

// API Key no header
request.AddHeader("X-API-Key", Config.Instancia.API.ApiKey);

// User-Agent personalizado para identificar o AdrenalineSpy
request.AddHeader("User-Agent", "AdrenalineSpy/1.0 (+https://github.com/usuario/adrenaline-spy)");

// Content-Type específico
request.AddHeader("Accept", "application/json");
request.AddHeader("Content-Type", "application/json; charset=utf-8");

var response = await client.ExecuteAsync(request);
```

### Integração com Workflow

```csharp
// No Workflow.cs principal - integrar ApiTask
public async Task<bool> ExecutarScrapingCompleto()
{
    try
    {
        // 1. Testar API antes de começar
        await ApiTask.TestarConectividade();
        
        var inicioExecucao = DateTime.Now;
        var noticias = new List<Noticia>();
        
        // 2. Executar scraping normal...
        await NavigationTask.InicializarBrowser();
        noticias = await ExtractionTask.ColetarTodasNoticias();
        await MigrationTask.SalvarNoticias(noticias);
        
        // 3. Enviar para API externa (se configurado)
        foreach (var noticia in noticias)
        {
            await ApiTask.EnviarNoticia(noticia);
        }
        
        // 4. Notificar conclusão
        await ApiTask.NotificarExecucaoCompleta(noticias.Count, inicioExecucao);
        
        LoggingTask.RegistrarInfo($"🎯 Scraping completo: {noticias.Count} notícias processadas");
        return true;
    }
    catch (Exception ex)
    {
        LoggingTask.RegistrarErro("Erro no workflow completo", ex);
        return false;
    }
}

### Headers e Authentication

```csharp
var client = new RestClient("https://api.example.com");
var request = new RestRequest("protected", Method.Get);

// Header customizado
request.AddHeader("X-Api-Key", "minha-chave-api");

// Bearer Token
request.AddHeader("Authorization", "Bearer seu_token_aqui");

// Basic Auth
client.Authenticator = new HttpBasicAuthenticator("usuario", "senha");

var response = await client.ExecuteAsync(request);
```

### Query Parameters

```csharp
var request = new RestRequest("search", Method.Get);

request.AddParameter("q", "termo de busca");
request.AddParameter("page", 1);
request.AddParameter("limit", 10);

// URL: /search?q=termo+de+busca&page=1&limit=10

var response = await client.ExecuteAsync(request);
```

### PUT e DELETE

```csharp
// PUT
var putRequest = new RestRequest("users/123", Method.Put);
putRequest.AddJsonBody(new { nome = "João Atualizado" });
await client.ExecuteAsync(putRequest);

// DELETE
var deleteRequest = new RestRequest("users/123", Method.Delete);
await client.ExecuteAsync(deleteRequest);
```

### Upload de Arquivo

```csharp
var request = new RestRequest("upload", Method.Post);

request.AddFile("file", "C:\\caminho\\arquivo.pdf", "application/pdf");
request.AddParameter("description", "Meu arquivo");

var response = await client.ExecuteAsync(request);
```

### Download de Arquivo

```csharp
var request = new RestRequest("download/arquivo.pdf", Method.Get);
var response = await client.ExecuteAsync(request);

if (response.IsSuccessful)
{
    File.WriteAllBytes("downloaded.pdf", response.RawBytes);
}
```

### Deserialização Automática

```csharp
public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
}

var request = new RestRequest("users/1", Method.Get);
var response = await client.ExecuteAsync<Usuario>(request);

if (response.IsSuccessful)
{
    var usuario = response.Data;
    Console.WriteLine($"{usuario.Nome} - {usuario.Email}");
}
```

### Timeout e Retry

```csharp
var options = new RestClientOptions("https://api.example.com")
{
    MaxTimeout = 30000, // 30 segundos
    ThrowOnAnyError = false
};

var client = new RestClient(options);

// Retry manual
int maxRetries = 3;
for (int i = 0; i < maxRetries; i++)
{
    var response = await client.ExecuteAsync(request);
    
    if (response.IsSuccessful)
        break;
    
    if (i < maxRetries - 1)
        await Task.Delay(1000); // 1 segundo entre tentativas
}
```

---

## Newtonsoft.Json

### Instalação

```bash
dotnet add package Newtonsoft.Json
```

### Serializar (Objeto → JSON)

```csharp
using Newtonsoft.Json;

var pessoa = new Pessoa
{
    Nome = "João",
    Idade = 30,
    Email = "joao@email.com"
};

// Serializar
string json = JsonConvert.SerializeObject(pessoa);
Console.WriteLine(json);
// {"Nome":"João","Idade":30,"Email":"joao@email.com"}

// Serializar com indentação
string jsonFormatado = JsonConvert.SerializeObject(pessoa, Formatting.Indented);
Console.WriteLine(jsonFormatado);
```

### Deserializar (JSON → Objeto)

```csharp
string json = @"{
    ""Nome"": ""Maria"",
    ""Idade"": 25,
    ""Email"": ""maria@email.com""
}";

var pessoa = JsonConvert.DeserializeObject<Pessoa>(json);
Console.WriteLine($"{pessoa.Nome}, {pessoa.Idade} anos");
```

### Array/Lista

```csharp
// Lista para JSON
var pessoas = new List<Pessoa>
{
    new Pessoa { Nome = "João", Idade = 30 },
    new Pessoa { Nome = "Maria", Idade = 25 }
};

string json = JsonConvert.SerializeObject(pessoas);

// JSON para Lista
var listaPessoas = JsonConvert.DeserializeObject<List<Pessoa>>(json);
```

### Dynamic/Anonymous

```csharp
// JSON para dynamic
string json = @"{ ""nome"": ""João"", ""idade"": 30 }";
dynamic obj = JsonConvert.DeserializeObject(json);
Console.WriteLine(obj.nome); // João

// Anonymous object
var anonimo = new { Nome = "João", Idade = 30 };
string jsonAnonimo = JsonConvert.SerializeObject(anonimo);
```

### Atributos JSON

```csharp
using Newtonsoft.Json;

public class Produto
{
    [JsonProperty("id")]
    public int Id { get; set; }
    
    [JsonProperty("nome_produto")]
    public string Nome { get; set; }
    
    [JsonIgnore]
    public string PropriedadeInterna { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Descricao { get; set; }
}
```

### Configurações de Serialização

```csharp
var settings = new JsonSerializerSettings
{
    // Formatação
    Formatting = Formatting.Indented,
    
    // Ignorar valores null
    NullValueHandling = NullValueHandling.Ignore,
    
    // Ignorar valores padrão
    DefaultValueHandling = DefaultValueHandling.Ignore,
    
    // Formato de data
    DateFormatString = "dd/MM/yyyy HH:mm:ss",
    
    // Case insensitive
    ContractResolver = new DefaultContractResolver
    {
        NamingStrategy = new CamelCaseNamingStrategy()
    }
};

string json = JsonConvert.SerializeObject(obj, settings);
```

### JObject (Manipulação Dinâmica)

```csharp
using Newtonsoft.Json.Linq;

// Criar JObject
var jobj = new JObject
{
    ["nome"] = "João",
    ["idade"] = 30,
    ["ativo"] = true
};

string json = jobj.ToString();

// Parse JSON
var parsed = JObject.Parse(json);
string nome = (string)parsed["nome"];
int idade = (int)parsed["idade"];

// Modificar
parsed["email"] = "joao@email.com";
parsed["idade"] = 31;

// Navegar em objetos aninhados
var jsonComplexo = @"{
    ""usuario"": {
        ""nome"": ""João"",
        ""contato"": {
            ""email"": ""joao@email.com""
        }
    }
}";

var obj = JObject.Parse(jsonComplexo);
string email = (string)obj["usuario"]["contato"]["email"];
```

### LINQ to JSON

```csharp
string json = @"[
    { ""nome"": ""João"", ""idade"": 30 },
    { ""nome"": ""Maria"", ""idade"": 25 },
    { ""nome"": ""Pedro"", ""idade"": 35 }
]";

var array = JArray.Parse(json);

// Filtrar
var maioresDe25 = array.Where(p => (int)p["idade"] > 25);

foreach (var pessoa in maioresDe25)
{
    Console.WriteLine(pessoa["nome"]);
}

// Selecionar propriedade
var nomes = array.Select(p => (string)p["nome"]).ToList();
```

---

## Trabalhando Juntos

### API Client Completo

```csharp
using RestSharp;
using Newtonsoft.Json;

public class ApiClient
{
    private readonly RestClient _client;
    
    public ApiClient(string baseUrl)
    {
        _client = new RestClient(baseUrl);
    }
    
    public async Task<T> GetAsync<T>(string endpoint)
    {
        var request = new RestRequest(endpoint, Method.Get);
        var response = await _client.ExecuteAsync(request);
        
        if (!response.IsSuccessful)
        {
            throw new Exception($"Erro na API: {response.ErrorMessage}");
        }
        
        return JsonConvert.DeserializeObject<T>(response.Content);
    }
    
    public async Task<T> PostAsync<T>(string endpoint, object body)
    {
        var request = new RestRequest(endpoint, Method.Post);
        request.AddJsonBody(JsonConvert.SerializeObject(body));
        
        var response = await _client.ExecuteAsync(request);
        
        if (!response.IsSuccessful)
        {
            throw new Exception($"Erro na API: {response.ErrorMessage}");
        }
        
        return JsonConvert.DeserializeObject<T>(response.Content);
    }
}

// Uso
var api = new ApiClient("https://api.example.com");
var usuario = await api.GetAsync<Usuario>("users/1");
```

---

## Exemplos Práticos

### Exemplo 1: Consumir API Pública (ViaCEP)

```csharp
public class Endereco
{
    [JsonProperty("cep")]
    public string CEP { get; set; }
    
    [JsonProperty("logradouro")]
    public string Logradouro { get; set; }
    
    [JsonProperty("bairro")]
    public string Bairro { get; set; }
    
    [JsonProperty("localidade")]
    public string Cidade { get; set; }
    
    [JsonProperty("uf")]
    public string Estado { get; set; }
}

async Task<Endereco> BuscarCEP(string cep)
{
    var client = new RestClient("https://viacep.com.br");
    var request = new RestRequest($"ws/{cep}/json", Method.Get);
    
    var response = await client.ExecuteAsync(request);
    
    if (response.IsSuccessful)
    {
        return JsonConvert.DeserializeObject<Endereco>(response.Content);
    }
    
    return null;
}

// Uso
var endereco = await BuscarCEP("01310-100");
Console.WriteLine($"{endereco.Logradouro}, {endereco.Cidade} - {endereco.Estado}");
```

### Exemplo 2: CRUD Completo

```csharp
public class UsuarioService
{
    private readonly RestClient _client;
    
    public UsuarioService()
    {
        _client = new RestClient("https://api.example.com");
    }
    
    // CREATE
    public async Task<Usuario> CriarAsync(Usuario usuario)
    {
        var request = new RestRequest("users", Method.Post);
        request.AddJsonBody(usuario);
        
        var response = await _client.ExecuteAsync(request);
        return JsonConvert.DeserializeObject<Usuario>(response.Content);
    }
    
    // READ (listar)
    public async Task<List<Usuario>> ListarAsync()
    {
        var request = new RestRequest("users", Method.Get);
        var response = await _client.ExecuteAsync(request);
        return JsonConvert.DeserializeObject<List<Usuario>>(response.Content);
    }
    
    // READ (por ID)
    public async Task<Usuario> ObterAsync(int id)
    {
        var request = new RestRequest($"users/{id}", Method.Get);
        var response = await _client.ExecuteAsync(request);
        return JsonConvert.DeserializeObject<Usuario>(response.Content);
    }
    
    // UPDATE
    public async Task<Usuario> AtualizarAsync(int id, Usuario usuario)
    {
        var request = new RestRequest($"users/{id}", Method.Put);
        request.AddJsonBody(usuario);
        
        var response = await _client.ExecuteAsync(request);
        return JsonConvert.DeserializeObject<Usuario>(response.Content);
    }
    
    // DELETE
    public async Task<bool> DeletarAsync(int id)
    {
        var request = new RestRequest($"users/{id}", Method.Delete);
        var response = await _client.ExecuteAsync(request);
        return response.IsSuccessful;
    }
}
```

### Exemplo 3: Tratamento de Erros

```csharp
public class ApiResponse<T>
{
    public bool Sucesso { get; set; }
    public T Dados { get; set; }
    public string Mensagem { get; set; }
    public int StatusCode { get; set; }
}

async Task<ApiResponse<T>> ExecutarAsync<T>(RestRequest request)
{
    try
    {
        var response = await _client.ExecuteAsync(request);
        
        if (response.IsSuccessful)
        {
            return new ApiResponse<T>
            {
                Sucesso = true,
                Dados = JsonConvert.DeserializeObject<T>(response.Content),
                StatusCode = (int)response.StatusCode
            };
        }
        
        return new ApiResponse<T>
        {
            Sucesso = false,
            Mensagem = $"Erro {response.StatusCode}: {response.ErrorMessage}",
            StatusCode = (int)response.StatusCode
        };
    }
    catch (Exception ex)
    {
        return new ApiResponse<T>
        {
            Sucesso = false,
            Mensagem = $"Exceção: {ex.Message}",
            StatusCode = 0
        };
    }
}
```

---

## Boas Práticas

### 1. Reutilize RestClient

```csharp
// ✅ BOM - uma instância
private static readonly RestClient _client = new RestClient("https://api.example.com");

// ❌ RUIM - nova instância a cada request
var client = new RestClient("https://api.example.com");
```

### 2. Use Async/Await

```csharp
// ✅ BOM
var response = await client.ExecuteAsync(request);

// ❌ RUIM
var response = client.Execute(request);
```

### 3. Configure Timeout

```csharp
var options = new RestClientOptions(url)
{
    MaxTimeout = 30000
};
```

### 4. Trate Erros Específicos

```csharp
if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
{
    // 404 - Recurso não encontrado
}
else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
{
    // 401 - Não autorizado
}
```

---

## Recursos Adicionais

- **RestSharp**: https://restsharp.dev/
- **Newtonsoft.Json**: https://www.newtonsoft.com/json

---

**Versão:** 1.0  
**Última atualização:** Novembro 2025
