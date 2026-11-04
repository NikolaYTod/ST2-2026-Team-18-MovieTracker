
using Microsoft.EntityFrameworkCore;
using MovieTracker.Data;

namespace MovieTracker.Services;

/// <summary>
/// Обединява Gemini AI, SQL преводача и базата данни.
/// </summary>
public sealed class MovieQueryService : IMovieQueryService
{
    private readonly IGeminiQueryService _geminiService;
    private readonly JsonQuerySqlTranslator _translator;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MovieQueryService> _logger;

    public MovieQueryService(
        IGeminiQueryService geminiService,
        JsonQuerySqlTranslator translator,
        ApplicationDbContext context,
        ILogger<MovieQueryService> logger)
    {
        _geminiService = geminiService;
        _translator = translator;
        _context = context;
        _logger = logger;
    }

    public async Task<MovieQueryResult> SearchAsync(
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        // 1. Изпращаме въпроса към Gemini.
        var aiResult = await _geminiService.GenerateJsonQueryAsync(
            userPrompt,
            cancellationToken);

        if (!aiResult.IsSuccess)
        {
            return MovieQueryResult.Fail(
                aiResult.Error ?? "Грешка при обработката от Gemini.");
        }

        try
        {
            // 2. Преобразуваме JSON към параметризирана SQL заявка.
            var translatedQuery = _translator.Translate(aiResult.Query!);

            // 3. Изпълняваме SELECT заявката чрез EF Core.
            var movies = await _context.Movies
                .FromSqlRaw(
                    translatedQuery.Sql,
                    translatedQuery.Parameters.Cast<object>().ToArray())
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // 4. Връщаме намерените филми.
            return MovieQueryResult.Ok(
                movies,
                aiResult.Query!.Explanation);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Gemini generated an invalid movie query.");

            return MovieQueryResult.Fail(
                "Генерираната заявка не е валидна: " + ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while executing AI movie search.");

            return MovieQueryResult.Fail(
                "Възникна грешка при търсенето на филми.");
        }
    }
}
