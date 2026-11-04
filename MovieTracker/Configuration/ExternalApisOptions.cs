namespace MovieTracker.Configuration;

/// <summary>
/// Настройки за външните API услуги.
/// Използва се Options Pattern.
/// </summary>
public sealed class ExternalApisOptions
{
    public const string SectionName = "ExternalApis";

    /// <summary>
    /// Gemini API ключ.
    /// </summary>
    public string? GeminiApiKey { get; set; }

    /// <summary>
    /// Име на използвания Gemini модел.
    /// </summary>
    public string GeminiModel { get; set; } = "gemini-2.5-flash";
}
