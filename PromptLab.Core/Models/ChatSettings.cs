using System.ComponentModel;
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
	public string Model { get; set; } = "gpt-4-turbo";

	public OpenAIPromptExecutionSettings AsPromptExecutionSettings()
	{
		return new OpenAIPromptExecutionSettings { ChatSystemPrompt = SystemPrompt, FrequencyPenalty = FrequencyPenalty.GetValueOrDefault(), MaxTokens = MaxTokens, PresencePenalty = PresencePenalty.GetValueOrDefault(), StopSequences = Stops, Temperature = Temperature.GetValueOrDefault(), TopP = TopP.GetValueOrDefault(), ResponseFormat = string.IsNullOrEmpty(ResponseFormat.GetDescription()) ? null : ResponseFormat.GetDescription() };
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