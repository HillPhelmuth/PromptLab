using Azure.AI.OpenAI;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Polly;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;
using System.Runtime.CompilerServices;
using Microsoft.SemanticKernel.Connectors.Google;
using PromptLab.Core.Helpers;
using PromptLab.Core.Models;

namespace PromptLab.Core.Services;

public class ChatService
{
	private static IConfiguration _configuration;
	private static string _openaiApiKey;
	private static string _googleApiKey;
	private static string _openAiEmbeddingsModel;
	private static readonly JsonSerializerOptions s_options = new() { WriteIndented = true };
	private static ILoggerFactory _loggerFactory;
	private readonly ILogger<ChatService> _logger;
	private static AppState _appState;
	public ChatService(IConfiguration configuration, ILoggerFactory loggerFactory, AppState appState)
	{
		_configuration = configuration;
		_loggerFactory = loggerFactory;
		_appState = appState;
		_openaiApiKey = _configuration["OpenAI:ApiKey"]!;
		_googleApiKey = _configuration["GoogleAI:ApiKey"]!;
		_openAiEmbeddingsModel = _configuration["OpenAI:EmbeddingModelId"]!;
		_logger = loggerFactory.CreateLogger<ChatService>();

	}
	public async Task<ChatChoice> GetLogProbs(ChatHistory chatMessages, float temp, float topP, string systemPrompt = "You are a helpful AI model", string model = "gpt-3.5-turbo", string format = "", CancellationToken cancellationToken = default)
	{
		var responseFormat = string.IsNullOrEmpty(format) ? ChatCompletionsResponseFormat.Text : ChatCompletionsResponseFormat.JsonObject;
		var options = new ChatCompletionsOptions { LogProbabilitiesPerToken = 5, EnableLogProbabilities = true, Temperature = temp, NucleusSamplingFactor = topP, DeploymentName = model, ResponseFormat = responseFormat };
		options.Messages.Add(new ChatRequestSystemMessage(systemPrompt));
		foreach (var message in chatMessages)
		{
			if (message.Role == AuthorRole.User)
				options.Messages.Add(new ChatRequestUserMessage(message.Content));
			if (message.Role == AuthorRole.Assistant)
				options.Messages.Add(new ChatRequestAssistantMessage(message.Content));
		}
		var chat = new OpenAIClient(_configuration["OpenAI:ApiKey"]);
		var response = await chat.GetChatCompletionsAsync(options, cancellationToken);

		var chatChoice = response.Value.Choices[0];
		return chatChoice;
	}
	public async IAsyncEnumerable<string> StreamingChatResponse(ChatHistory chatMessages, string systemPrompt,
		string model = "gpt-3.5-turbo", [EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var item = GetServiceId();
		var settings = GetServicePromptExecutionSettings(item);
		var chatService = CreateKernel(model).Services.GetRequiredKeyedService<IChatCompletionService>(item);
		var chatHistory = new ChatHistory(systemPrompt);
		chatHistory.AddRange(chatMessages);
		var chatHistoryString = JsonSerializer.Serialize(chatHistory.Select(x => new { role = x.Role, content = x.Content }), s_options);
		_logger.LogInformation("Chat History:\n {chatHistoryString}", chatHistoryString);
		await foreach (var token in chatService.GetStreamingChatMessageContentsAsync(chatHistory, settings, cancellationToken: cancellationToken))
		{
			if (string.IsNullOrEmpty(token.Content)) continue;
			yield return token.Content;
		}
	}

	public static PromptExecutionSettings GetServicePromptExecutionSettings(string item)
	{
		PromptExecutionSettings settings = item switch
		{
			"Google" => _appState.ChatSettings.AsGeminiPromptExecutionSettings(),
			"Mistral" => _appState.ChatSettings.AsMistralPromptExecutionSettings(),
			_ => _appState.ChatSettings.AsPromptExecutionSettings()
		};
		return settings;
	}

	public static string GetServiceId()
	{
		var model = _appState.ChatSettings.Model;
		if (model.StartsWith("gemini"))
			return "Google";
		if (model.Contains("mixtral") || model.Contains("mistral"))
			return "Mistral";
		return "OpenAI";
	}

	
	public static Kernel CreateKernel(string model)
	{
		var kernelBuilder = Kernel.CreateBuilder();
		kernelBuilder.Services.AddLogging(builder =>
		{
			builder.AddConsole();
			builder.Services.AddSingleton(_loggerFactory);

		});
		kernelBuilder.Services.AddSingleton(_appState);
		kernelBuilder.Services.AddSingleton(_loggerFactory);
		kernelBuilder.Services.ConfigureHttpClientDefaults(c =>
		{
			c.AddStandardResilienceHandler().Configure(o =>
			{
				o.Retry.ShouldHandle = args => ValueTask.FromResult(args.Outcome.Result?.StatusCode is HttpStatusCode.TooManyRequests);
				o.Retry.BackoffType = DelayBackoffType.Exponential;
				o.AttemptTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromSeconds(150) };
				o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(600);
				o.TotalRequestTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromMinutes(15) };
			});
		});
		kernelBuilder.Services.AddSingleton(_configuration);
		if (model.StartsWith("gemini"))
			kernelBuilder.AddGoogleAIGeminiChatCompletion(model, _googleApiKey, serviceId: "Google");
		else if (model.Contains("mixtral") || model.Contains("mistral"))
			kernelBuilder.AddMistralChatCompletion(model, _appState.ChatModelSettings.MistralApiKey!, serviceId: "Mistral");
		else
			kernelBuilder.AddAICompletion(_appState.ChatModelSettings, model);
		kernelBuilder.AddTextEmbeddings(_appState.EmbeddingModelSettings);
		var kernel = kernelBuilder.Build();

		return kernel;
	}
}
