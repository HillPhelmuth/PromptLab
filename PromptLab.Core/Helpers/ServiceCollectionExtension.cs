using Microsoft.Extensions.DependencyInjection;
using PromptLab.Core.Plugins;
using PromptLab.Core.Services;

namespace PromptLab.Core.Helpers
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddPromptLab(this IServiceCollection services)
        {
            return services.AddScoped<ChatService>().AddSingleton<LocalFileService>().AddScoped<PromptEngineerService>().AddSingleton<BlobService>();
        }
    }
}
