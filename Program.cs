using System.Net.Http.Headers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ThreeDAiStudioMcp.Clients;
using ThreeDAiStudioMcp.Configuration;
using ThreeDAiStudioMcp.Hosting;
using ThreeDAiStudioMcp.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseWindowsService();

if (!HasConfiguredUrls(builder.Configuration))
{
    var port = ResolvePort(builder.Configuration);
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(port);
    });
}

builder.Services
    .AddOptions<ThreeDAiStudioOptions>()
    .Configure<IConfiguration>(
        (options, configuration) =>
        {
            options.ApiKey = configuration[ThreeDAiStudioOptions.ApiKeyEnvironmentVariable];
            options.BaseUrl = configuration[ThreeDAiStudioOptions.BaseUrlEnvironmentVariable]
                ?? ThreeDAiStudioOptions.DefaultBaseUrl;
            options.FailureLogPath = ThreeDAiStudioOptions.ResolveFailureLogPath(
                configuration[ThreeDAiStudioOptions.FailureLogPathEnvironmentVariable]);
        });

builder.Services.AddHttpClient<ThreeDAiStudioApiClient>(
    (serviceProvider, httpClient) =>
    {
        var options = serviceProvider.GetRequiredService<IOptions<ThreeDAiStudioOptions>>().Value;
        httpClient.BaseAddress = new Uri(ThreeDAiStudioOptions.NormalizeBaseUrl(options.BaseUrl));
        httpClient.Timeout = TimeSpan.FromMinutes(10);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("3d-ai-studio-mcp/0.1.0");
    });

builder.Services.AddHostedService<StartupDiagnosticsHostedService>();

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithTools<TencentHunyuanTools>()
    .WithTools<ThreeDAiStudioModelTools>()
    .WithTools<ThreeDAiStudioImageTools>();

var app = builder.Build();

app.MapMcp();

app.Run();

static bool HasConfiguredUrls(IConfiguration configuration) =>
    !string.IsNullOrWhiteSpace(configuration["urls"]) ||
    !string.IsNullOrWhiteSpace(configuration["URLS"]) ||
    !string.IsNullOrWhiteSpace(configuration["ASPNETCORE_URLS"]);

static int ResolvePort(IConfiguration configuration)
{
    foreach (var key in new[] { "HTTP_PORT", "PORT", "port" })
    {
        if (int.TryParse(configuration[key], out var port) && port > 0 && port <= 65535)
        {
            return port;
        }
    }

    return ThreeDAiStudioOptions.DefaultPort;
}
