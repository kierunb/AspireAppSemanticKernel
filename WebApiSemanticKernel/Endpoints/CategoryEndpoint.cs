using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using OpenAI.Embeddings;
using System.Text;
using WebApiSemanticKernel.Extensions;
using WebApiSemanticKernel.Models;

namespace WebApiSemanticKernel.Endpoints;


public record CategoryRequest(string Query);

public class CategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/category", async (
            [FromServices] Kernel kernel,
            [FromServices] SearchClient searchClient,
            [FromBody] CategoryRequest categoryRequest,
            CancellationToken cancellationToken
        ) =>
        {
            
            string response = await SearchIndex(categoryRequest.Query, "glossary", kernel, searchClient);

            string prompt = @"
                Na podstawie pytania użytkownika podanego w sekcji [Pytanie] zaklasyfikuj je do odpowiedniej kategorii.

                W celu odpowiedniej klasyfikacji pytania do kategorii, możesz skorzystać z informacji podanych w sekcji [Opisy]:
                Opisy podane są w formie listy, gdzie każdy element listy składa się z nazwy kategorii terminu oraz opisu tej kategorii.  

                Wybierz pasujące kategorie z podanych kategorii: ['OZE', 'F-Gazy', 'Badania UC', 'Badania UTB']
                Zwróc odpowiedź w formacie JSON:
                {
                    'category': 'kategoria1'
                    'category': 'kategoria2'
                }

                [Opisy]
                {{$opisy}}

                [Pytanie] 
                {{$question}}
                ";


            KernelArguments arguments = new() { { "question", categoryRequest.Query }, { "opisy", response } };
            var result = await kernel.InvokePromptAsync(prompt, arguments, cancellationToken: cancellationToken);
            return Results.Ok(result.GetValue<string>());
        });
    }

    private async Task<ReadOnlyMemory<float>> GetEmbeddings(Kernel kernel, string text)
    {
        var textEmbeddingGenerationService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        ReadOnlyMemory<float> embeddings = await textEmbeddingGenerationService.GenerateEmbeddingAsync(text);

        return embeddings;
    }


    private async Task<string> SearchIndex(string question, string indexName, Kernel kernel, SearchClient searchClient)
    {
        ReadOnlyMemory<float> vectorizedQuestion = await GetEmbeddings(kernel, question);

        var sb = new StringBuilder();

        //var searchClient = searchIndexClient.GetSearchClient(indexName);

        // jak podam jeszcze jako pierwszy parametr metody SearchAsync "question", to będzie "Full-text search + Vector Search"

        SearchResults<Glossary> response = await searchClient.SearchAsync<Glossary>(question,
            new SearchOptions
            {
                VectorSearch = new()
                {
                    Queries = { new VectorizedQuery(vectorizedQuestion) { KNearestNeighborsCount = 3, Fields = { "TermEmbedding", "DefinitionEmbedding" } } }
                },
                //Filter = "Category eq 'OZE'", // FILTROWANIE PO POLU

            });

        int count = 0;
        Console.WriteLine($"Single Vector Search Results:\n\n");

        await foreach (SearchResult<Glossary> result in response.GetResultsAsync())
        {
            count++;
            Glossary doc = result.Document;

            sb.AppendLine($"Category: {doc.Category}");
            sb.AppendLine($"Term: {doc.Term}");
            sb.AppendLine($"Defintion: {doc.Definition}");
        }
        return sb.ToString();
    }
}
