
using MovieTracker.Models;

namespace MovieTracker.Services;

/// <summary>
/// Интерфейс за интелигентно търсене на филми.
/// </summary>
public interface IMovieQueryService
{
    Task<MovieQueryResult> SearchAsync(
        string userPrompt,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Резултат от AI търсенето.
/// </summary>
public sealed record MovieQueryResult(
    IReadOnlyList<Movie>? Movies,
    string? Explanation,
    string? Error)
{
    public bool IsSuccess => Movies is not null && Error is null;

    public static MovieQueryResult Ok(
        IReadOnlyList<Movie> movies,
        string? explanation)
        => new(movies, explanation, null);

    public static MovieQueryResult Fail(string error)
        => new(null, null, error);
}
