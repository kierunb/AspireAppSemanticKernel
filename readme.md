# .NET Aspire + Semantic Kernel + Web API

## Configuration

```shell
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:ChatDeploymentName" "gpt-4o"
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://xyz.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" ""
```

## Docs

- [Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/)
- [Semantic Kernel & Dependency Injection](https://learn.microsoft.com/en-us/semantic-kernel/concepts/kernel?pivots=programming-language-csharp)