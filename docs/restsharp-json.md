# RestSharp e Newtonsoft.Json - APIs e HTTP

## Índice
1. [RestSharp](#restsharp)
2. [Newtonsoft.Json](#newtonsoftjson)
3. [Trabalhando Juntos](#trabalhando-juntos)
4. [Exemplos Práticos](#exemplos-práticos)

---

## RestSharp

### Instalação

```bash
dotnet add package RestSharp
```

### GET Request

```csharp
using RestSharp;

var client = new RestClient("https://api.example.com");
var request = new RestRequest("users", Method.Get);

var response = await client.ExecuteAsync(request);

if (response.IsSuccessful)
{
    Console.WriteLine(response.Content);
}
else
{
    Console.WriteLine($"Erro: {response.ErrorMessage}");
}
```

### POST Request

```csharp
var client = new RestClient("https://api.example.com");
var request = new RestRequest("users", Method.Post);

request.AddJsonBody(new
{
    nome = "João",
    email = "joao@email.com"
});

var response = await client.ExecuteAsync(request);
Console.WriteLine(response.Content);
```

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
