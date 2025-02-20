using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using WebApiSemanticKernel.Extensions;

namespace WebApiSemanticKernel.Endpoints;

public record ChatRequest(string Message);

public class ChatEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/chat", async (
            [FromServices] Kernel kernel,
            [FromBody] ChatRequest chatRequest,
            CancellationToken cancellationToken
        ) =>
        {
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            OpenAIPromptExecutionSettings openAIPromptExecutionSettings =
                new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

            var result = await chatCompletionService.GetChatMessageContentAsync(
            chatRequest.Message,
            executionSettings: openAIPromptExecutionSettings,
            kernel: kernel,
            cancellationToken: cancellationToken
            );

            return Results.Ok(result);
        });
    }
}
