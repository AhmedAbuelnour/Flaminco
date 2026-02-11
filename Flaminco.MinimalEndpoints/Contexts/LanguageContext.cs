namespace Flaminco.MinimalEndpoints.Contexts
{
    public static class LanguageContext
    {
        private static readonly AsyncLocal<string> _currentLanguage = new();

        public static string GetCurrent() => _currentLanguage.Value!;

        public static void SetCurrent(string language) => _currentLanguage.Value = language;
    }
}
