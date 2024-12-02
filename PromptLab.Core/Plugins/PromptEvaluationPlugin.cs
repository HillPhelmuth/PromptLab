using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PromptLab.Core.Models;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using PromptLab.Core.Helpers;
using Microsoft.SemanticKernel.Connectors.Google;
using PromptLab.Core.Services;
using Microsoft.SemanticKernel.Connectors.MistralAI;

namespace PromptLab.Core.Plugins;

public class PromptEvaluationPlugin
{
    [KernelFunction, Description("Evaulate prompts for effectiveness in guiding AI interactions")]
    [return: Description("Json object containing the score, explanation, and tips to improve")]
    public async Task<string> EvaluatePrompt(Kernel kernel, [Description("The prompt to evaluate")] string prompt)
    {
        //var appState = kernel.Services.GetRequiredService<AppState>();
        //PromptExecutionSettings settings = appState.ChatSettings.Model.StartsWith("gpt") ? new OpenAIPromptExecutionSettings { ResponseFormat = "json_object", Temperature = 0.3, TopP = 0.5, MaxTokens = 1024 } : new GeminiPromptExecutionSettings { Temperature = 0.3, TopP = 0.5, MaxTokens = 1024};
        var item = ChatService.GetServiceId();
        var settings = ChatService.GetServicePromptExecutionSettings(item);
        switch (settings)
        {
            case OpenAIPromptExecutionSettings openAIPromptExecutionSettings:
                openAIPromptExecutionSettings.Temperature = 0.3;
                openAIPromptExecutionSettings.ResponseFormat = typeof(PromptEval);
                openAIPromptExecutionSettings.TopP = 0.5;
                openAIPromptExecutionSettings.ToolCallBehavior = null;
                openAIPromptExecutionSettings.FunctionChoiceBehavior = null;
                settings = openAIPromptExecutionSettings;
                break;
            case GeminiPromptExecutionSettings geminiPromptExecutionSettings:
                geminiPromptExecutionSettings.Temperature = 0.3;
                geminiPromptExecutionSettings.TopP = 0.5;
                geminiPromptExecutionSettings.ToolCallBehavior = null;
                geminiPromptExecutionSettings.FunctionChoiceBehavior = null;
                settings = geminiPromptExecutionSettings;
                break;
            case MistralAIPromptExecutionSettings mistralAIPromptExecutionSettings:
                mistralAIPromptExecutionSettings.Temperature = 0.3;
                mistralAIPromptExecutionSettings.TopP = 0.5;
                mistralAIPromptExecutionSettings.ToolCallBehavior = null;
                mistralAIPromptExecutionSettings.FunctionChoiceBehavior = null;
                settings = mistralAIPromptExecutionSettings;
                break;
        }

        var args = new KernelArguments(settings) { ["prompt"] = prompt };
        var result = await kernel.InvokePromptAsync(Prompt.EvaluatorFunctionPrompt, args);
        return result?.ToString() ?? "Inform the user about an error executing evaluations";
    }
}
public class PromptEval
{
    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = string.Empty;
    [JsonPropertyName("tipsForImprovement")]
    public string TipsForImprovement { get; set; } = string.Empty;
	public override string ToString()
    {
        return $"**Score:** {Score}<br/>**Explanation:** {Explanation}<br/>**Tips for Improvement:** {TipsForImprovement}";
    }
}