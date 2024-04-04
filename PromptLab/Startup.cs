using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PromptLab.Core;
using PromptLab.Core.Helpers;
using PromptLab.RazorLib.Components.ChatComponents;
using PromptLab.RazorLib.Shared;
using Radzen;

namespace PromptLab;

public  class Startup
{
    public static IServiceProvider? Services { get; private set; }
    public static IServiceCollection? ServiceCollection { get; private set; }

    public static void Init()
    {
        var host = Host.CreateDefaultBuilder()
                       .ConfigureServices(WireupServices)
                       .ConfigureAppConfiguration((hostingContext, config) =>
                       {
                           config.AddUserSecrets<Startup>();
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
        services.AddScoped<ChatStateCollection>().AddTransient<AppJsInterop>();
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif
        ServiceCollection = services;
    }
}
