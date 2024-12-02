using BlazorJoditEditor;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PromptLab.Core;
using PromptLab.Core.Helpers;
using PromptLab.Core.Plugins;
using PromptLab.RazorLib.ChatModels;
using PromptLab.RazorLib.Components.ChatComponents;
using PromptLab.RazorLib.Shared;
using Radzen;
using Serilog;
using Serilog.Core;

namespace PromptLab;

public  class Startup
{
    public static IServiceProvider? Services { get; private set; }
    public static IServiceCollection? ServiceCollection { get; private set; }
    private static IConfiguration? _config;

    public static void Init()
    {
        var host = Host.CreateDefaultBuilder()
                       .ConfigureServices(WireupServices)
                       .ConfigureAppConfiguration((hostingContext, config) =>
                       {
                           config.AddUserSecrets<Startup>();
                           config.AddEnvironmentVariables();
                           _config = config.Build();
                       })
                       .Build();
        Services = host.Services;
    }

    private static void WireupServices(IServiceCollection services)
    {
        services.AddWindowsFormsBlazorWebView();
        services.AddSingleton<AppState>();
        services.AddPromptLab();
        services.AddRadzenComponents();
        services.AddScoped<ChatStateCollection>().AddTransient<AppJsInterop>().AddTransient<JoditEditorInterop>();
        Log.Logger = new LoggerConfiguration()
	        .MinimumLevel.Debug()
	        .WriteTo.File("logs/myapp.txt", restrictedToMinimumLevel:Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Day)
	        .CreateLogger();

		services.AddLogging(c =>
        {
            c.Services.AddSingleton(Log.Logger);
        });
        services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddBlobServiceClient(_config!["ConnectionString:blob"]!, preferMsi: true);
        });
        services.AddHttpClient();
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
        //services.AddSingleton<MemorySave>();
#endif
        ServiceCollection = services;
    }
}
