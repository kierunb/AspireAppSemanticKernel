using WebApiSemanticKernel.Extensions;

namespace WebApiSemanticKernel.Endpoints;

public class HelloEnpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/hello", () => "Hello, World!");
    }
}
