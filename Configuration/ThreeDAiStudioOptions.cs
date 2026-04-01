namespace ThreeDAiStudioMcp.Configuration;

internal sealed class ThreeDAiStudioOptions
{
    public const string ApiKeyEnvironmentVariable = "THREE_D_AI_STUDIO_API_KEY";
    public const string BaseUrlEnvironmentVariable = "THREE_D_AI_STUDIO_BASE_URL";
    public const string FailureLogPathEnvironmentVariable = "THREE_D_AI_STUDIO_FAILURE_LOG_PATH";
    public const string DefaultBaseUrl = "https://api.3daistudio.com";
    public const string DefaultFailureLogPath = "logs/three-d-ai-studio-api-failures.log";
    public const int DefaultPort = 8080;

    public string? ApiKey { get; set; }

    public string BaseUrl { get; set; } = DefaultBaseUrl;

    public string FailureLogPath { get; set; } = DefaultFailureLogPath;

    public static string NormalizeBaseUrl(string? baseUrl)
    {
        var value = string.IsNullOrWhiteSpace(baseUrl) ? DefaultBaseUrl : baseUrl.Trim();
        return value.EndsWith('/') ? value : $"{value}/";
    }

    public static string ResolveFailureLogPath(string? failureLogPath)
    {
        var value = string.IsNullOrWhiteSpace(failureLogPath)
            ? DefaultFailureLogPath
            : failureLogPath.Trim();

        return Path.IsPathRooted(value)
            ? value
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, value));
    }

}
