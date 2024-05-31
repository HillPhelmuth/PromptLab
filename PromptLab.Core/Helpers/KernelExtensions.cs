using Microsoft.SemanticKernel;
using PromptLab.Core.Models;

namespace PromptLab.Core.Helpers;

public static class KernelExtensions
{
	public static IKernelBuilder AddAICompletion(this IKernelBuilder kernelBuilder, ChatModelSettings modelSettings, string model = "gpt-3.5-turbo")
	{
		if (modelSettings.OpenAIModelType == OpenAIModelType.OpenAI)
		{
			kernelBuilder.AddOpenAIChatCompletion(model, modelSettings.OpenAIApiKey!, serviceId:"OpenAI");
		}
		else
		{
			var deployment = model.Contains('3') ? modelSettings.AzureOpenAIGpt35DeploymentName : modelSettings.AzureOpenAIGpt4DeploymentName;
			kernelBuilder.AddAzureOpenAIChatCompletion(deployment!, modelSettings.AzureOpenAIApiEndpoint!, modelSettings.AzureOpenAIApiKey!, serviceId:"OpenAI");
		}
		return kernelBuilder;
	}
	public static IKernelBuilder AddTextEmbeddings(this IKernelBuilder kernelBuilder, EmbeddingModelSettings settings)
	{
		if (settings.OpenAIModelType == OpenAIModelType.OpenAI)
		{
			kernelBuilder.AddOpenAITextEmbeddingGeneration("text-embedding-3-small", settings.OpenAIApiKey!);
		}
		else
		{
			kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(settings.AzureOpenAIEmbeddingsDeploymentName!, settings.AzureOpenAIApiEndpoint!, settings.AzureOpenAIApiKey!);
		}
		return kernelBuilder;
	}
}