using Microsoft.SemanticKernel;
using PromptLab.Core.Models;
using System.Reflection;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Claude = Anthropic.AnthropicClient;

namespace PromptLab.Core.Helpers;

public static class KernelExtensions
{
	public static IKernelBuilder AddAICompletion(this IKernelBuilder kernelBuilder, ChatModelSettings modelSettings,
        string model = "gpt-4o-mini", string? serviceId = "OpenAI")
	{
		if (modelSettings.OpenAIModelType == OpenAIModelType.OpenAI)
		{
			kernelBuilder.AddOpenAIChatCompletion(model, modelSettings.OpenAIApiKey!, serviceId:serviceId);
			//kernelBuilder.AddOpenAIChatCompletion(model, modelSettings.OpenAIApiKey!);
		}
		else
		{
			var deployment = model.Contains("mini") ? modelSettings.AzureOpenAIGpt35DeploymentName : modelSettings.AzureOpenAIGpt4DeploymentName;
			kernelBuilder.AddAzureOpenAIChatCompletion(deployment!, modelSettings.AzureOpenAIApiEndpoint!, modelSettings.AzureOpenAIApiKey!, serviceId:serviceId);
		}
		return kernelBuilder;
	}
    public static IKernelBuilder AddAICompletion(this IKernelBuilder kernelBuilder, Claude chatClient)
    {
        var chatCompletion = chatClient.AsChatCompletionService();
        kernelBuilder.Services.AddKeyedSingleton(typeof(IChatCompletionService),serviceKey:"Anthropic", chatCompletion);
        return kernelBuilder;
    }

    public static IKernelBuilder AddVertexAIChatCompletion(this IKernelBuilder kernelBuilder, string model,
        string location = "us-central1", string projectId = "vertexexperiments-447502")
    {
        return kernelBuilder.AddVertexAIGeminiChatCompletion(
            modelId: model,
			bearerTokenProvider:GetToken,
            //bearerKey: bearerKey,
            location: location,
            projectId: projectId);

            async ValueTask<string> GetToken()
            {
#if DEBUG
			var credential = GoogleCredential.FromFile("vertexexperiments-447502-f71e46ba9055.json");
            var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            return accessToken;
#else
			return "";
#endif

            }
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
		foreach (var functionName in FunctionNames)
		{
			var yaml = ExtractFromAssembly<string>($"{functionName}.yaml");
			var function = kernel.CreateFunctionFromPromptYaml(yaml);
			evalFunctions[functionName] = function;
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