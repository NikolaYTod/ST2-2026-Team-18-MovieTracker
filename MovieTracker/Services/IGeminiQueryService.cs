using MovieTracker.Models;

namespace MovieTracker.Services;

/// <summary>
/// Резултат от обработката на заявка чрез Gemini.
/// </summary>
public sealed record GeminiQueryResult(
    JSONQuery? Query,
    string? RawJson,
    string? Error)
{
    public bool IsSuccess => Query is not null && Error is null;

    public static GeminiQueryResult Ok(
        JSONQuery query,
        string rawJson)
        => new(query, rawJson, null);

    public static GeminiQueryResult Fail(string error)
        => new(null, null, error);
}

/// <summary>
/// Интерфейс за комуникация с Gemini AI.
/// </summary>
public interface IGeminiQueryService
{
    Task<GeminiQueryResult> GenerateJsonQueryAsync(
        string naturalLanguageQuery,
        CancellationToken cancellationToken = default);
}
