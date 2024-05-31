using System.ComponentModel;
using Microsoft.SemanticKernel;
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
	public bool Streaming { get; set; }
	public ResponseFormat ResponseFormat { get; set; }

	public string SystemPrompt { get; set; } = "You are a helpful AI Assistant";
	public string Model { get; set; } = "gpt-4o";

	public OpenAIPromptExecutionSettings AsPromptExecutionSettings()
	{
		return new OpenAIPromptExecutionSettings { ChatSystemPrompt = SystemPrompt, FrequencyPenalty = FrequencyPenalty.GetValueOrDefault(), MaxTokens = MaxTokens, PresencePenalty = PresencePenalty.GetValueOrDefault(), StopSequences = Stops, Temperature = Temperature.GetValueOrDefault(), TopP = TopP.GetValueOrDefault(), ResponseFormat = string.IsNullOrEmpty(ResponseFormat.GetDescription()) ? null : ResponseFormat.GetDescription(), ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
	}
	public GeminiPromptExecutionSettings AsGeminiPromptExecutionSettings()
	{
		return new GeminiPromptExecutionSettings
		{
			ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions,
			MaxTokens = MaxTokens,
			StopSequences = Stops,
			Temperature = Temperature.GetValueOrDefault(),
			TopP = TopP.GetValueOrDefault()
		};
	}
	public MistralAIPromptExecutionSettings AsMistralPromptExecutionSettings()
	{
		return new MistralAIPromptExecutionSettings
		{
			ToolCallBehavior = MistralAIToolCallBehavior.AutoInvokeKernelFunctions,
			MaxTokens = MaxTokens,
			Temperature = Temperature.GetValueOrDefault(),
			TopP = TopP.GetValueOrDefault()
		};
	}	

}
public class AppSettings
{
	public double ZoomFactor = 1.0;
	public StyleTheme Theme { get; set; }
	public bool ShowTimestamps { get; set; }
	public bool LogprobsView { get; set; }
	public bool Streaming { get; set; }

}
public enum ResponseFormat
{
	[Description("")]
	Text,
	[Description("json_object")]
	Json_Object
}