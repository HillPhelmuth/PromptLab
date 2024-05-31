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

namespace PromptLab.Core.Plugins;

public class PromptEvaluationPlugin
{
	[KernelFunction, Description("Evaulate prompts for effectiveness in guiding AI interactions")]
	[return: Description("Json object containing the score and explanation")]
	public async Task<string> EvaluatePrompt(Kernel kernel, [Description("The prompt to evaluate")] string prompt)
	{
		var appState = kernel.Services.GetRequiredService<AppState>();
		//PromptExecutionSettings settings = appState.ChatSettings.Model.StartsWith("gpt") ? new OpenAIPromptExecutionSettings { ResponseFormat = "json_object", Temperature = 0.3, TopP = 0.5, MaxTokens = 1024 } : new GeminiPromptExecutionSettings { Temperature = 0.3, TopP = 0.5, MaxTokens = 1024};
		var item = ChatService.GetServiceId();
		var settings = ChatService.GetServicePromptExecutionSettings(item);
		var args = new KernelArguments(settings) { ["prompt"] = prompt, ["guide"] = Prompt.PromptGuideTopics };
		var result = await kernel.InvokePromptAsync(Prompt.EvaluatorFunctionPrompt, args);
		return result.GetValue<string>() ?? "Inform the user about an error executing evaluations";
	}
}
public class PromptEval
{
    [JsonPropertyName("score")]
    public int Score { get; set; }

	[JsonPropertyName("explanation")]
	public string Explanation { get; set; } = string.Empty;
    public override string ToString()
    {
		return $"Score: {Score}<br/>Explanation: {Explanation}";
    }
}