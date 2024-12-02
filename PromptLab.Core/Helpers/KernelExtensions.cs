using Microsoft.SemanticKernel;
using PromptLab.Core.Models;
using System.Reflection;
using System.Text.Json;

namespace PromptLab.Core.Helpers;

public static class KernelExtensions
{
	public static IKernelBuilder AddAICompletion(this IKernelBuilder kernelBuilder, ChatModelSettings modelSettings, string model = "gpt-4o-mini")
	{
		if (modelSettings.OpenAIModelType == OpenAIModelType.OpenAI)
		{
			kernelBuilder.AddOpenAIChatCompletion(model, modelSettings.OpenAIApiKey!, serviceId:"OpenAI");
			//kernelBuilder.AddOpenAIChatCompletion(model, modelSettings.OpenAIApiKey!);
		}
		else
		{
			var deployment = model.Contains("mini") ? modelSettings.AzureOpenAIGpt35DeploymentName : modelSettings.AzureOpenAIGpt4DeploymentName;
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
	private static readonly List<string> FunctionNames = ["AnalyzePrompt", "EvaluatePrompt", "WriteDraftPrompt", "EvaluateDraftPrompt", "WriteFinalPrompt", "CreatePrompt"];
	public static KernelPlugin ImportMetaPromptFunctions(this Kernel kernel)
	{
		var evalFunctions = new Dictionary<string, KernelFunction>();
		foreach (var eval in FunctionNames)
		{
			var yaml = ExtractFromAssembly<string>($"{eval}.yaml");
			var function = kernel.CreateFunctionFromPromptYaml(yaml);
			evalFunctions[eval] = function;
		}

		var plugin = KernelPluginFactory.CreateFromFunctions("MetaPromptingPlugin", evalFunctions.Values);
		kernel.Plugins.Add(plugin);
		return plugin;
	}
	internal static T ExtractFromAssembly<T>(string fileName)
	{
		var assembly = Assembly.GetExecutingAssembly();
		var jsonName = assembly.GetManifestResourceNames()
			.SingleOrDefault(s => s.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)) ?? "";
		using var stream = assembly.GetManifestResourceStream(jsonName);
		using var reader = new StreamReader(stream);
		object result = reader.ReadToEnd();
		if (typeof(T) == typeof(string))
			return (T)result;
		return JsonSerializer.Deserialize<T>(result.ToString());
	}
}