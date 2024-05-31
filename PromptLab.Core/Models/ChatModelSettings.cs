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
	public string? MistralApiKey { get; set; }
	public (bool, string) Validate()
	{
		if (OpenAIModelType == OpenAIModelType.AzureOpenAI)
		{
			if (string.IsNullOrEmpty(AzureOpenAIApiKey))
			{
				return (false, "Azure OpenAI Api Key is required");
			}
			if (string.IsNullOrEmpty(AzureOpenAIApiEndpoint))
			{
				return (false, "Azure OpenAI Api Endpoint is required");
			}
			if (string.IsNullOrEmpty(AzureOpenAIGpt4DeploymentName) && string.IsNullOrEmpty(AzureOpenAIGpt35DeploymentName))
			{
				return (false, "Azure OpenAI Deployment Name is required");
			}
		}
		else
		{
			if (string.IsNullOrEmpty(OpenAIApiKey))
			{
				return (false, "OpenAI Api Key is required");
			}
		}
		return (true, string.Empty);
	}
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