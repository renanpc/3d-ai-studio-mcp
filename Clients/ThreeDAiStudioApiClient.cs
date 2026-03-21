using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThreeDAiStudioMcp.Configuration;
using ThreeDAiStudioMcp.Models.Account;
using ThreeDAiStudioMcp.Models.Api;
using ThreeDAiStudioMcp.Models.Hunyuan;
using ThreeDAiStudioMcp.Models.Tasks;

namespace ThreeDAiStudioMcp.Clients;

internal sealed class ThreeDAiStudioApiClient(
    HttpClient httpClient,
    IOptions<ThreeDAiStudioOptions> options,
    ILogger<ThreeDAiStudioApiClient> logger)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly SemaphoreSlim FailureLogLock = new(1, 1);

    public Task<TaskSubmissionResponse> SubmitTaskAsync(
        string relativePath,
        object request,
        CancellationToken cancellationToken = default) =>
        SendAsync<TaskSubmissionResponse>(
            HttpMethod.Post,
            relativePath,
            request,
            cancellationToken);

    public Task<TaskSubmissionResponse> SubmitHunyuanProAsync(
        HunyuanProGenerationApiRequest request,
        CancellationToken cancellationToken = default) =>
        SubmitTaskAsync(
            "v1/3d-models/tencent/generate/pro/",
            request,
            cancellationToken);

    public Task<TaskSubmissionResponse> SubmitHunyuanRapidAsync(
        HunyuanRapidGenerationApiRequest request,
        CancellationToken cancellationToken = default) =>
        SubmitTaskAsync(
            "v1/3d-models/tencent/generate/rapid/",
            request,
            cancellationToken);

    public Task<GenerationStatusResult> GetGenerationStatusAsync(
        string taskId,
        CancellationToken cancellationToken = default) =>
        SendAsync<GenerationStatusResult>(
            HttpMethod.Get,
            $"v1/generation-request/{Uri.EscapeDataString(taskId)}/status/",
            body: null,
            cancellationToken);

    public Task<CreditBalanceResult> GetCreditBalanceAsync(
        CancellationToken cancellationToken = default) =>
        SendAsync<CreditBalanceResult>(
            HttpMethod.Get,
            "account/user/wallet/",
            body: null,
            cancellationToken);

    private async Task<TResponse> SendAsync<TResponse>(
        HttpMethod method,
        string relativePath,
        object? body,
        CancellationToken cancellationToken)
    {
        EnsureApiKeyConfigured();

        var requestBody = body is null ? null : JsonSerializer.Serialize(body, JsonSerializerOptions);

        using var request = new HttpRequestMessage(method, relativePath);
        request.Headers.Authorization = new("Bearer", options.Value.ApiKey);

        if (requestBody is not null)
        {
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var requestUri = request.RequestUri ?? new Uri(httpClient.BaseAddress!, relativePath);
            await TryWriteFailureLogAsync(
                requestUri,
                method,
                request,
                requestBody,
                response,
                responseBody,
                cancellationToken);

            logger.LogWarning(
                "3D AI Studio API returned {StatusCode} for {Method} {Path}. Failure details were appended to {LogPath}. Response body: {Body}",
                (int)response.StatusCode,
                method,
                relativePath,
                options.Value.FailureLogPath,
                responseBody);

            throw ThreeDAiStudioApiException.Create(response.StatusCode, responseBody);
        }

        var result = JsonSerializer.Deserialize<TResponse>(responseBody, JsonSerializerOptions);
        if (result is null)
        {
            throw new InvalidOperationException("3D AI Studio API returned an empty or invalid JSON response.");
        }

        return result;
    }

    private void EnsureApiKeyConfigured()
    {
        if (!string.IsNullOrWhiteSpace(options.Value.ApiKey))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Set the {ThreeDAiStudioOptions.ApiKeyEnvironmentVariable} environment variable before calling 3D AI Studio tools.");
    }

    private async Task TryWriteFailureLogAsync(
        Uri requestUri,
        HttpMethod method,
        HttpRequestMessage request,
        string? requestBody,
        HttpResponseMessage response,
        string responseBody,
        CancellationToken cancellationToken)
    {
        try
        {
            await AppendFailureLogAsync(
                requestUri,
                method,
                request,
                requestBody,
                response,
                responseBody,
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Failed to append 3D AI Studio failure details to {LogPath}.",
                options.Value.FailureLogPath);
        }
    }

    private async Task AppendFailureLogAsync(
        Uri requestUri,
        HttpMethod method,
        HttpRequestMessage request,
        string? requestBody,
        HttpResponseMessage response,
        string responseBody,
        CancellationToken cancellationToken)
    {
        var logPath = options.Value.FailureLogPath;
        var directory = Path.GetDirectoryName(logPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var entry = BuildFailureLogEntry(
            requestUri,
            method,
            request,
            requestBody,
            response,
            responseBody);

        await FailureLogLock.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(logPath, entry, cancellationToken);
        }
        finally
        {
            FailureLogLock.Release();
        }
    }

    private static string BuildFailureLogEntry(
        Uri requestUri,
        HttpMethod method,
        HttpRequestMessage request,
        string? requestBody,
        HttpResponseMessage response,
        string responseBody)
    {
        var builder = new StringBuilder();
        builder.AppendLine(new string('=', 100));
        builder.Append("Timestamp (UTC): ").AppendLine(DateTimeOffset.UtcNow.ToString("O"));
        builder.Append("Request: ").Append(method).Append(' ').AppendLine(requestUri.ToString());
        builder.Append("Response Status: ").Append((int)response.StatusCode).Append(' ').AppendLine(response.StatusCode.ToString());
        builder.AppendLine("Request Headers:");
        AppendHeaders(builder, request.Headers, redactAuthorization: true);

        if (request.Content?.Headers is { } requestContentHeaders)
        {
            AppendHeaders(builder, requestContentHeaders, redactAuthorization: false);
        }

        builder.AppendLine("Request JSON Body:");
        builder.AppendLine(string.IsNullOrWhiteSpace(requestBody) ? "(none)" : requestBody);
        builder.AppendLine("Response Headers:");
        AppendHeaders(builder, response.Headers, redactAuthorization: false);

        if (response.Content?.Headers is { } responseContentHeaders)
        {
            AppendHeaders(builder, responseContentHeaders, redactAuthorization: false);
        }

        builder.AppendLine("Response Body:");
        builder.AppendLine(string.IsNullOrWhiteSpace(responseBody) ? "(empty)" : responseBody);
        builder.AppendLine();

        return builder.ToString();
    }

    private static void AppendHeaders(
        StringBuilder builder,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers,
        bool redactAuthorization)
    {
        foreach (var header in headers)
        {
            var value = string.Join(", ", header.Value);
            if (redactAuthorization && header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                value = "Bearer [redacted]";
            }

            builder.Append("  ").Append(header.Key).Append(": ").AppendLine(value);
        }
    }
}
