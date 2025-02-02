using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;

namespace Workshop.ConsoleApp.Models;

public class DataModel
{
    [VectorStoreRecordKey]
    [TextSearchResultName]
    public Guid Key { get; init; }

    [VectorStoreRecordData]
    [TextSearchResultValue]
    public string? Text { get; init; }

    [VectorStoreRecordData]
    [TextSearchResultLink]
    public string? Link { get; init; }

    [VectorStoreRecordData(IsFilterable = true)]
    public required string Tag { get; init; }

    [VectorStoreRecordVector]
    public ReadOnlyMemory<float> Embedding { get; init; }
}
