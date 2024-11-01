using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.MistralAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PromptLab.Core.Helpers;

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

    public OpenAIPromptExecutionSettings AsOpenAIPromptExecutionSettings(bool allowToolCalls = true, bool? allowParallel = null, bool allowConcurrant = false)
    {
        var options = new FunctionChoiceBehaviorOptions { AllowConcurrentInvocation = allowConcurrant, AllowParallelCalls = allowParallel };
        return new OpenAIPromptExecutionSettings { ChatSystemPrompt = SystemPrompt, FrequencyPenalty = FrequencyPenalty.GetValueOrDefault(), MaxTokens = MaxTokens, PresencePenalty = PresencePenalty.GetValueOrDefault(), StopSequences = Stops, Temperature = Temperature.GetValueOrDefault(), TopP = TopP.GetValueOrDefault(), ResponseFormat = string.IsNullOrEmpty(ResponseFormat.GetDescription()) ? null : ResponseFormat.GetDescription(), FunctionChoiceBehavior = allowToolCalls ? FunctionChoiceBehavior.Auto(options:options) : null };
    }
    public GeminiPromptExecutionSettings AsGeminiPromptExecutionSettings(bool allowToolCalls = true)
    {
        return new GeminiPromptExecutionSettings
        {
            ToolCallBehavior = allowToolCalls ? GeminiToolCallBehavior.AutoInvokeKernelFunctions : null,
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
            ToolCallBehavior = allowToolCalls ? MistralAIToolCallBehavior.AutoInvokeKernelFunctions : null,
            MaxTokens = MaxTokens,
            Temperature = Temperature.GetValueOrDefault(),
            TopP = TopP.GetValueOrDefault()
        };
    }

}

public enum ResponseFormat
{
    [Description("")]
    Text,
    [Description("json_object")]
    Json_Object
}