using System.Net;
using System.Text.Json;

namespace ThreeDAiStudioMcp.Models.Api;

internal sealed class ThreeDAiStudioApiException(
    HttpStatusCode statusCode,
    string message,
    string? responseBody) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;

    public string? ResponseBody { get; } = responseBody;

    public static ThreeDAiStudioApiException Create(HttpStatusCode statusCode, string? responseBody)
    {
        var details = TryExtractDetails(responseBody);
        var message = details is null
            ? $"3D AI Studio API request failed with HTTP {(int)statusCode} ({statusCode})."
            : $"3D AI Studio API request failed with HTTP {(int)statusCode} ({statusCode}): {details}";

        return new ThreeDAiStudioApiException(statusCode, message, responseBody);
    }

    private static string? TryExtractDetails(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            var parts = new List<string>();

            if (TryGetString(root, "code", out var code) || TryGetString(root, "error_code", out code))
            {
                parts.Add(code!);
            }

            if (TryGetString(root, "message", out var message) || TryGetString(root, "detail", out message))
            {
                parts.Add(message!);
            }

            return parts.Count > 0 ? string.Join(" - ", parts) : responseBody.Trim();
        }
        catch (JsonException)
        {
            return responseBody.Trim();
        }
    }

    private static bool TryGetString(JsonElement element, string propertyName, out string? value)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
        {
            value = property.GetString();
            return !string.IsNullOrWhiteSpace(value);
        }

        value = null;
        return false;
    }
}
