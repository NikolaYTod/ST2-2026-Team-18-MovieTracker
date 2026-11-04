
using System.Text.Json.Serialization;

namespace MovieTracker.Models;

/// <summary>
/// Структурирана заявка, генерирана от Gemini.
/// Описва филтрирането, сортирането и ограничаването
/// на резултатите от таблицата Movies.
/// </summary>
public sealed class JSONQuery
{
    [JsonPropertyName("logicalOperator")]
    public string LogicalOperator { get; set; } = "AND";

    [JsonPropertyName("conditions")]
    public List<QueryCondition> Conditions { get; set; } = new();

    [JsonPropertyName("orderBy")]
    public string? OrderBy { get; set; } = "Title";

    [JsonPropertyName("sortDirection")]
    public string? SortDirection { get; set; } = "ASC";

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    [JsonPropertyName("explanation")]
    public string? Explanation { get; set; }
}

/// <summary>
/// Условие за филтриране на филми.
/// </summary>
public sealed class QueryCondition
{
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [JsonPropertyName("operator")]
    public string Operator { get; set; } = "contains";

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}
