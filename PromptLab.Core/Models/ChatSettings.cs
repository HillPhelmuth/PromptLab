using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Amazon;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.MistralAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Anthropic;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using PromptLab.Core.Helpers;
using PromptLab.Core.Services;
using AnthropicPromptExecutionSettings = Microsoft.SemanticKernel.Connectors.Anthropic.AnthropicPromptExecutionSettings;

namespace PromptLab.Core.Models;

public class ChatSettings
{
    public int? MaxTokens { get; set; } = 256;

    public float? TopP { get; set; } = 1.0f;

    public IList<string>? Stops { get; set; }

    public float? PresencePenalty { get; set; } = 0;

    public float? FrequencyPenalty { get; set; } = 0;

    public float? Temperature { get; set; } = 1.0f;
    public bool LogProbs { get; set; }
    public bool Streaming { get; set; } = true;
    public ResponseFormat ResponseFormat { get; set; }

    public string SystemPrompt { get; set; } = "You are a helpful AI Assistant";
    public string Model { get; set; } = "gpt-4o";
    public bool EnableTools { get; set; }
    [JsonIgnore] 
    public List<KernelPlugin> CustomPlugins { get; set; } = [];
    public string? OpenApiPluginFileContent { get; set; }
    public List<CustomPluginPath> OpenApiPluginNamePaths { get; set; } = [];

    public async Task<List<KernelPlugin>> GetPluginsFromPaths(IFileService fileService)
    {
        var plugins = new List<KernelPlugin>();
        foreach (var filePath in OpenApiPluginNamePaths)
        {
            var fileContent = await fileService.OpenFileFromPathAsync(filePath.Path);
            if (string.IsNullOrEmpty(fileContent)) continue;
            var asByteArray = Encoding.UTF8.GetBytes(fileContent);
            var fileStream = new MemoryStream(asByteArray);
            var plugin = await OpenApiKernelPluginFactory.CreateFromOpenApiAsync(filePath.Name, fileStream);
            plugins?.Add(plugin);
        }
        return plugins;
    }

    public OpenAIPromptExecutionSettings AsOpenAIPromptExecutionSettings(bool allowToolCalls = true, bool? allowParallel = null, bool allowConcurrant = false)
    {
        var options = new FunctionChoiceBehaviorOptions { AllowConcurrentInvocation = allowConcurrant, AllowParallelCalls = allowParallel };
        return new OpenAIPromptExecutionSettings { FrequencyPenalty = FrequencyPenalty.GetValueOrDefault(), MaxTokens = MaxTokens, PresencePenalty = PresencePenalty.GetValueOrDefault(), StopSequences = Stops, Temperature = Temperature.GetValueOrDefault(), TopP = TopP.GetValueOrDefault(), ResponseFormat = string.IsNullOrEmpty(ResponseFormat.GetDescription()) ? null : ResponseFormat.GetDescription(), FunctionChoiceBehavior = allowToolCalls ? FunctionChoiceBehavior.Auto() : null, Store = true};
    }
    public GeminiPromptExecutionSettings AsGeminiPromptExecutionSettings(bool allowToolCalls = true)
    {
        return new GeminiPromptExecutionSettings
        {
            FunctionChoiceBehavior = allowToolCalls ? FunctionChoiceBehavior.Auto() : null,
            MaxTokens = MaxTokens,
            StopSequences = Stops,
            Temperature = Temperature.GetValueOrDefault(),
            TopP = TopP.GetValueOrDefault()
        };
    }
    public MistralAIPromptExecutionSettings AsMistralPromptExecutionSettings(bool allowToolCalls = true)
    {
        return new MistralAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = allowToolCalls ? FunctionChoiceBehavior.Auto() : null,
            MaxTokens = MaxTokens,
            Temperature = Temperature.GetValueOrDefault(),
            TopP = TopP.GetValueOrDefault()
        };
    }
    public AmazonClaudeExecutionSettings AsAnthropicPromptExecutionSettings(bool allowToolCalls = true, string systemPrompt = "")
    {
        return new AmazonClaudeExecutionSettings
        {
            FunctionChoiceBehavior = allowToolCalls ? FunctionChoiceBehavior.Auto() : null,
            MaxTokensToSample = MaxTokens.GetValueOrDefault(),
            Temperature = Temperature.GetValueOrDefault(),
            TopP = TopP.GetValueOrDefault()
        };
    }
    

}
public class CustomPluginPath
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
}
public enum ResponseFormat
{
    [Description("")]
    Text,
    [Description("json_object")]
    Json_Object
}