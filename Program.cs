// Carregar configurações
Config config = Config.Instancia;

// Validar
if (!config.Validar() || config == null)
{
    Console.WriteLine("❌ Configurações inválidas ou não carregadas!");
    return;
}

// Usar em qualquer lugar
Console.WriteLine($"URL Base: {config.Navegacao.UrlBase}\n");
Console.WriteLine($"Banco: {config.Database.NomeBanco}\n");

// Obter connection string
string connectionString = config.ObterConnectionString();
Console.WriteLine($"Connection String: {connectionString}\n");

// Acessar categorias
foreach (var categoria in config.Categorias)
{
    Console.WriteLine($"Categoria: {categoria.Key} → {categoria.Value}\n");
}