
using Microsoft.Extensions.Configuration;
using Microsoft.KernelMemory;

Console.WriteLine("Hello, Kernel Memory!");

var configuration = GetAppConfiguration();

string azureAISearchUri = configuration["AzureAISearch:Endpoint"]!;
string azureAISearchSecret = configuration["AzureAISearch:ApiKey"]!;

string deploymentName = configuration["AzureOpenAI:EmbeddingDeploymentName"]!;
string endpoint = configuration["AzureOpenAI:Endpoint"]!;
string apiKey = configuration["AzureOpenAI:ApiKey"]!;

var azureOpenAITextConfig = new AzureOpenAIConfig
{
    Endpoint = configuration["AzureOpenAI:Endpoint"]!,
    APIKey = configuration["AzureOpenAI:ApiKey"]!,
    Deployment = configuration["AzureOpenAI:ChatDeploymentName"]!,
    Auth = AzureOpenAIConfig.AuthTypes.APIKey,
};

var azureOpenAIEmbeddingConfig = new AzureOpenAIConfig
{
    Endpoint = configuration["AzureOpenAI:Endpoint"]!,
    APIKey = configuration["AzureOpenAI:ApiKey"]!,
    Deployment = configuration["AzureOpenAI:EmbeddingDeploymentName"]!,
    Auth = AzureOpenAIConfig.AuthTypes.APIKey,
};
//KernelMemoryBuilderBuildOptions.AllowMixingVolatileAndPersistentData = true;

var memory = new KernelMemoryBuilder()
    .WithAzureOpenAITextGeneration(azureOpenAITextConfig)
    .WithAzureOpenAITextEmbeddingGeneration(azureOpenAIEmbeddingConfig)
    //.WithAzureAISearchMemoryDb(azureAISearchUri, azureAISearchSecret)
    .Build<MemoryServerless>();

string filepath = @"d:\ustawa.pdf";
string indexName = "documents";
Console.WriteLine($"Importing {filepath} to {indexName}...");

var id = await memory.ImportDocumentAsync(filepath);

await memory.ImportWebPageAsync("https://www.udt.gov.pl/kierownictwo-udt");
await memory.ImportWebPageAsync("https://www.udt.gov.pl/historia-udt");

Console.WriteLine(id);

var question = "Co zawiera wniosek, o którym mowa w ust. 2";

//var responses = await memory.SearchAsync(question);

//foreach (var result in responses.Results)
//{
//    foreach (var partition in result.Partitions)
//    {
//        Console.WriteLine($"Text: {partition.Text} Relevance: {partition.Relevance}");
//    }
//}

Console.WriteLine(await AskAsync("Inetgracja UDT z Unią europejską", memory));
Console.WriteLine(await AskAsync("Kto jest prezesem UDT", memory));
Console.WriteLine(await AskAsync("Kto jest wiceprezesem UDT", memory));

async static Task<string> AskAsync(string question, MemoryServerless memory)
{
    Console.WriteLine($"\n\nAsking: {question}");
    var answer = await memory.AskAsync(question);
    return answer.ToString();
}












static IConfiguration GetAppConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddUserSecrets<Program>();
    return builder.Build();
}