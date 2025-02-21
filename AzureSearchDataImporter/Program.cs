
using Azure.Search.Documents.Indexes;
using Azure;
using Microsoft.Extensions.Configuration;
using ClosedXML.Excel;
using Azure.AI.OpenAI;
using OpenAI.Embeddings;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Azure.Search.Documents.Indexes.Models;

Console.WriteLine("Hello, World!");

IConfiguration configuration = GetAppConfiguration();

string azureAISearchUri = configuration["AzureAISearch:Endpoint"]!;
string azureAISearchSecret = configuration["AzureAISearch:ApiKey"]!;

string deploymentName = configuration["AzureOpenAI:EmbeddingDeploymentName"]!;
string endpoint = configuration["AzureOpenAI:Endpoint"]!;
string apiKey = configuration["AzureOpenAI:ApiKey"]!;


var openAIClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
var embeddingClient = openAIClient.GetEmbeddingClient(deploymentName);

var searchIndexClient = new SearchIndexClient(
    new Uri(azureAISearchUri),
    new AzureKeyCredential(azureAISearchSecret)
);

string indexName = "glossary";
string excelFilePath = @"d:\111111\wsad-do-bota-warsztaty.xlsx";

//await CreateOrUpdateIndexAsync(searchIndexClient, indexName);

//await LoadData(searchIndexClient, embeddingClient, excelFilePath, indexName);

await SearchIndex("Badania okresowe", indexName, embeddingClient, searchIndexClient);


static async Task SearchIndex(string question, string indexName, EmbeddingClient embeddingClient, SearchIndexClient searchIndexClient)
{
    ReadOnlyMemory<float> vectorizedQuestion = GetEmbeddings(embeddingClient, question);

    var searchClient = searchIndexClient.GetSearchClient(indexName);

    SearchResults<Glossary> response = await searchClient.SearchAsync<Glossary>(
        new SearchOptions
        {
            VectorSearch = new()
            {
                Queries = { new VectorizedQuery(vectorizedQuestion) { KNearestNeighborsCount = 3, Fields = { "TermEmbedding", "DefinitionEmbedding" }}}
            },
            Filter = "Category eq 'OZE'",

        });

    int count = 0;
    Console.WriteLine($"Single Vector Search Results:\n\n");

    await foreach (SearchResult<Glossary> result in response.GetResultsAsync())
    {
        count++;
        Glossary doc = result.Document;
        Console.WriteLine($"Category: {doc.Category}");
        Console.WriteLine($"Term: {doc.Term}");
        Console.WriteLine($"Defintion: {doc.Definition}");
        Console.WriteLine($"Score: {result.Score}");
        Console.WriteLine();
    }
    Console.WriteLine($"Total number of search results:{count}");
}

static ReadOnlyMemory<float> GetEmbeddings(EmbeddingClient embeddingClient, string input)
{
    OpenAIEmbedding embedding = embeddingClient.GenerateEmbedding(input);
    return embedding.ToFloats();
}


static async Task LoadData(SearchIndexClient searchIndexClient, EmbeddingClient embeddingClient, string filePath, string indexName)
{
    List<Glossary> excelRows = [];

    using var workbook = new XLWorkbook(filePath);
    var worksheet = workbook.Worksheet(1);
    var rows = worksheet.RowsUsed();
    var rowsCount = rows.Count();
    foreach (var row in rows.Skip(1))
    {
        var glossary = new Glossary
        {
            Key = Guid.NewGuid().ToString(),
            Category = row.Cell(1).Value.ToString(),
            Term = row.Cell(2).Value.ToString(),
            TermEmbedding = GetEmbeddings(embeddingClient, row.Cell(2).Value.ToString()),
            Definition = row.Cell(3).Value.ToString(),
            DefinitionEmbedding = GetEmbeddings(embeddingClient, row.Cell(3).Value.ToString())
        };
        excelRows.Add(glossary);
    }

    Console.WriteLine($"Uploading {excelRows.Count()} documents to index: {indexName}");

    var searchClient = searchIndexClient.GetSearchClient(indexName);
    await searchClient.UploadDocumentsAsync(excelRows);

    Console.WriteLine($"Documents uploaded to index: {indexName}");
}

static async Task CreateOrUpdateIndexAsync(SearchIndexClient searchIndexClient, string indexName)
{
    ArgumentException.ThrowIfNullOrEmpty(indexName);

    Console.WriteLine($"Creating index: {indexName}");

    string vectorSearchProfileName = "src-vector-profile";
    string vectorSearchHnswConfig = "src-hsnw-vector-config";
    // VectorSearchDimensions = 1536: text-embedding-ada-002, text-embedding-3-small
    // VectorSearchDimensions = 3072: text-embedding-3-large
    int vectorSearchDimensions = 1536;

    var index = new SearchIndex(indexName)
    {
        Fields =
        {
            new SimpleField("Key", SearchFieldDataType.String)
            {
                IsKey = true,
                IsFilterable = true,
            },
            new SearchableField("Category")
            {
                IsFilterable = true,
                IsSortable = true,
                IsFacetable = true,
            },
            new SearchableField("Term")
            {
                IsFilterable = true,
                IsSortable = true,
                IsFacetable = true,
            },
            new VectorSearchField(
                "TermEmbedding",
                vectorSearchDimensions,
                vectorSearchProfileName
            ),
            new SearchableField("Definition"),
            new VectorSearchField(
                "DefinitionEmbedding",
                vectorSearchDimensions,
                vectorSearchProfileName
            )
        },
        VectorSearch = new()
        {
            Profiles =
            {
                new VectorSearchProfile(
                    vectorSearchProfileName,
                    vectorSearchHnswConfig
                ),
            },
            Algorithms = { new HnswAlgorithmConfiguration(vectorSearchHnswConfig) },
        },
    };

    await searchIndexClient.CreateOrUpdateIndexAsync(index);

    Console.WriteLine($"Index created: {indexName}");
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


public sealed class Glossary
{
    public string Key { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Term { get; set; } = string.Empty;

    public ReadOnlyMemory<float> TermEmbedding { get; set; }

    public string Definition { get; set; } = string.Empty;

    public ReadOnlyMemory<float> DefinitionEmbedding { get; set; }
}