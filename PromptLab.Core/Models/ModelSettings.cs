using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptLab.Core.Models;

public class ModelSettings
{
	public string? OpenAIApiKey { get; set; }
	public string? AzureOpenAIApiKey { get; set; }
	public string? AzureOpenAIApiEndpoint { get; set; }
	public string? AzureOpenAIGpt4DeploymentName { get; set; }
	public string? AzureOpenAIGpt35DeploymentName { get; set; }
	public string? GoogleGeminiApiKey { get; set; }
	public OpenAIModelType OpenAIModelType { get; set; }
}
public enum OpenAIModelType
{
	OpenAI,
	AzureOpenAI	
}