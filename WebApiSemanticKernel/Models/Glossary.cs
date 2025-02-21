namespace WebApiSemanticKernel.Models;

public sealed class Glossary
{
    public string Key { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Term { get; set; } = string.Empty;

    public ReadOnlyMemory<float> TermEmbedding { get; set; }

    public string Definition { get; set; } = string.Empty;

    public ReadOnlyMemory<float> DefinitionEmbedding { get; set; }
}