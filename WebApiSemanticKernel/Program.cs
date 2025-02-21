using Azure;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Scalar.AspNetCore;
using WebApiSemanticKernel.Extensions;
using WebApiSemanticKernel.Plugins;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddEndpoints(typeof(Program).Assembly);

// SearchClient
builder.Services.AddAzureClients(config =>
{
    config.AddSearchClient(new Uri(builder.Configuration["AzureAISearch:Endpoint"]!),
        indexName: "glossary",
        new AzureKeyCredential(builder.Configuration["AzureAISearch:ApiKey"]!));
});

#region Semantic Kernel

builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: builder.Configuration["AzureOpenAI:ChatDeploymentName"]!,
    endpoint: builder.Configuration["AzureOpenAI:Endpoint"]!,
    apiKey: builder.Configuration["AzureOpenAI:ApiKey"]!
);

builder.Services.AddAzureOpenAITextEmbeddingGeneration(
    deploymentName: builder.Configuration["AzureOpenAI:EmbeddingDeploymentName"]!,
    endpoint: builder.Configuration["AzureOpenAI:Endpoint"]!,
    apiKey: builder.Configuration["AzureOpenAI:ApiKey"]!
);

//builder.Services.AddSingleton(() => new DatePlugin());

builder.Services.AddSingleton<KernelPluginCollection>((serviceProvider) =>
    [
        //KernelPluginFactory.CreateFromObject(serviceProvider.GetRequiredService<DatePlugin>())
    ]
);

builder.Services.AddTransient((serviceProvider) => {
    KernelPluginCollection pluginCollection = serviceProvider.GetRequiredService<KernelPluginCollection>();
    //pluginCollection.AddFromType<DatePlugin>();
    return new Kernel(serviceProvider, pluginCollection);
});

#endregion


var app = builder.Build();
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(_ => _.Servers = []);
}

app.UseHttpsRedirection();

app.MapEndpoints();

app.Run();
