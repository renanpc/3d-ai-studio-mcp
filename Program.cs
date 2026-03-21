using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ThreeDAiStudioMcp.Clients;
using ThreeDAiStudioMcp.Configuration;
using ThreeDAiStudioMcp.Hosting;
using ThreeDAiStudioMcp.Tools;

var builder = WebApplication.CreateBuilder(args);

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
