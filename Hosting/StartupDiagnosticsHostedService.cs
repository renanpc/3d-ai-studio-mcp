using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThreeDAiStudioMcp.Configuration;

namespace ThreeDAiStudioMcp.Hosting;

internal sealed class StartupDiagnosticsHostedService(
    IOptions<ThreeDAiStudioOptions> options,
    ILogger<StartupDiagnosticsHostedService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
        {
            logger.LogWarning(
                "{EnvironmentVariable} is not configured. The MCP server will start, but 3D generation tools will fail until the API key is provided.",
                ThreeDAiStudioOptions.ApiKeyEnvironmentVariable);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
