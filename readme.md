# .NET Aspire + Semantic Kernel + Web API

## Configuration

```shell
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:ChatDeploymentName" "gpt-4o"
dotnet user-secrets set "AzureOpenAI:EmbeddingDeploymentName" "text-embedding-ada-002"
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://xyz.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" ""

dotnet user-secrets set "AzureAISearch:Endpoint" ""
dotnet user-secrets set "AzureAISearch:ApiKey" ""
```

## Docs

- [Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/)
- [Semantic Kernel & Dependency Injection](https://learn.microsoft.com/en-us/semantic-kernel/concepts/kernel?pivots=programming-language-csharp)