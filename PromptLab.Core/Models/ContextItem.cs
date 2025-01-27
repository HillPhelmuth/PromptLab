using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Data;

namespace PromptLab.Core.Models;

public class ContextItem(string id)
{
    public ContextItem() : this(Guid.NewGuid().ToString())
    {
    }
    [VectorStoreRecordKey]
    [TextSearchResultName]
    public string Id { get; set; } = id;
    public string? MemoryId { get; set; }
    [VectorStoreRecordData]
    [TextSearchResultValue]
    public string? Content { get; set; }
       
}

public class VectorStoreContextItem : ContextItem
{
    [VectorStoreRecordData(IsFilterable = true)]
    public string? Tag { get; init; }

    [VectorStoreRecordVector(1536)]
    public ReadOnlyMemory<float> Embedding { get; set; }
}

public class ResearchVectorStoreContextItem : VectorStoreContextItem
{
    public ResearchVectorStoreContextItem(string link, string? title, string? content, string? source)
    {
        Link = link;
        Title = title;
        Content = content;
        Source = source;
    }
    [JsonConstructor]
    public ResearchVectorStoreContextItem()
    {
    }
    [VectorStoreRecordData(IsFullTextSearchable = true)]
    public string? Title { get; init; }
    [VectorStoreRecordData(IsFilterable = true)]
    public string? Source { get; set; }
    [VectorStoreRecordData]
    [TextSearchResultLink]
    public string? Link { get; init; }
}
