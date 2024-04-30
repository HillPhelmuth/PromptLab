using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptLab.Core.Models;

public class ChatModelSettings : ModelSettings
{
	public string? AzureOpenAIGpt4DeploymentName { get; set; }
	public string? AzureOpenAIGpt35DeploymentName { get; set; }
	
	public string? GoogleGeminiApiKey { get; set; }
}
public class EmbeddingModelSettings : ModelSettings
{
	public string? AzureOpenAIEmbeddingsDeploymentName { get; set; }
}
public abstract class ModelSettings
{
	public OpenAIModelType OpenAIModelType { get; set; }
	public string? OpenAIApiKey { get; set; }
	public string? AzureOpenAIApiKey { get; set; }
	public string? AzureOpenAIApiEndpoint { get; set; }
}
public enum OpenAIModelType
{
	OpenAI,
	AzureOpenAI	
}