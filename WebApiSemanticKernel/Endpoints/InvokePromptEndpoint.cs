using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using WebApiSemanticKernel.Extensions;

namespace WebApiSemanticKernel.Endpoints;

public record InvokePromptRequest(string Question);

public class InvokePromptEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/invoke-prompt", async (
            [FromServices] Kernel kernel,
            [FromBody] InvokePromptRequest invokePromptRequest,
            CancellationToken cancellationToken
        ) =>
        {
            string prompt = $"""
                You are a helpful assistant. Answer the question as truthfully as possible, and if you don't know the answer, say "I don't know".
                Question: {invokePromptRequest.Question}
                Answer:
                """;

            KernelArguments arguments = new() { { "question", invokePromptRequest.Question } };

            var result = await kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
            return Results.Ok(result.GetValue<string>());
        });
    }
}
