namespace MovieTracker.Services
{
    public class AppSettings
    {
        public string ApplicationName { get; } = "MovieTracker";

        public string Version { get; } = "1.0.0";

        public int DefaultPageSize { get; } = 10;
    }
}
