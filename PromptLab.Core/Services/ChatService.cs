using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Polly;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using System.Runtime.CompilerServices;
using OpenAI;
using OpenAI.Chat;
using PromptLab.Core.Helpers;
using PromptLab.Core.Models;

namespace PromptLab.Core.Services;

public class ChatService
{
    private static IConfiguration _configuration;
    private static readonly JsonSerializerOptions s_options = new() { WriteIndented = true };
    private static ILoggerFactory _loggerFactory;
    private readonly ILogger<ChatService> _logger;
    private static AppState _appState;
    public ChatService(IConfiguration configuration, ILoggerFactory loggerFactory, AppState appState)
    {
        _configuration = configuration;
        _loggerFactory = loggerFactory;
        _appState = appState;
        _logger = loggerFactory.CreateLogger<ChatService>();

    }
    public async Task<IEnumerable<TokenString>> GetLogProbs(ChatHistory chatMessages, string systemPrompt = "You are a helpful AI model", string model = "gpt-4o-mini", CancellationToken cancellationToken = default)
    {
        var chatSettings = _appState.ChatSettings;
        var options = new ChatCompletionOptions() { TopLogProbabilityCount = 5, IncludeLogProbabilities = true, Temperature = chatSettings.Temperature, TopP = chatSettings.TopP, MaxOutputTokenCount = chatSettings.MaxTokens, StoredOutputEnabled = true };
        var messages = new List<ChatMessage> { ChatMessage.CreateSystemMessage(systemPrompt) };
        foreach (var message in chatMessages)
        {
            if (message.Role == AuthorRole.User)
                messages.Add(ChatMessage.CreateUserMessage(message.Content));
            if (message.Role == AuthorRole.Assistant)
                messages.Add(ChatMessage.CreateAssistantMessage(message.Content));
        }

        var openAiApiKey = _appState.ChatModelSettings.OpenAIApiKey ?? _configuration["OpenAI:ApiKey"];
        var chat = new OpenAIClient(openAiApiKey).GetChatClient(chatSettings.Model);
        var response = await chat.CompleteChatAsync(messages, options, cancellationToken);
        var chatChoice = response.Value.ContentTokenLogProbabilities;
        return chatChoice.AsTokenStrings();
    }
    public async IAsyncEnumerable<TokenString> GetLogProbsStreaming(ChatHistory chatMessages, string systemPrompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatSettings = _appState.ChatSettings;
        var options = new ChatCompletionOptions() { TopLogProbabilityCount = 5, IncludeLogProbabilities = true, Temperature = chatSettings.Temperature, TopP = chatSettings.TopP, MaxOutputTokenCount = chatSettings.MaxTokens, StoredOutputEnabled = true };
        var messages = new List<ChatMessage> { ChatMessage.CreateSystemMessage(systemPrompt) };
        foreach (var message in chatMessages)
        {
            if (message.Role == AuthorRole.User)
                messages.Add(ChatMessage.CreateUserMessage(message.Content));
            if (message.Role == AuthorRole.Assistant)
                messages.Add(ChatMessage.CreateAssistantMessage(message.Content));
        }

        var openAiApiKey = _appState.ChatModelSettings.OpenAIApiKey ?? _configuration["OpenAI:ApiKey"];
        var chat = new OpenAIClient(openAiApiKey).GetChatClient(chatSettings.Model);
        var streaming = chat.CompleteChatStreamingAsync(messages, options, cancellationToken);
        await foreach (var chunk in streaming)
        {
            if (chunk.ContentTokenLogProbabilities == null || chunk.ContentTokenLogProbabilities.Count == 0) continue;
            var logProb = chunk.ContentTokenLogProbabilities;
            yield return logProb.AsTokenString();
        }
    }

    public async IAsyncEnumerable<string> StreamingChatResponse(ChatHistory chatMessages, string systemPrompt,
        string model = "gpt-4o-mini", [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var item = GetServiceId();
        var settings = GetServicePromptExecutionSettings(item);
        var kernel = CreateKernel(model);
        var chatService = kernel.Services.GetRequiredKeyedService<IChatCompletionService>(item);
        var chatHistory = new ChatHistory(systemPrompt);
        chatHistory.AddRange(chatMessages);
        var chatHistoryString = JsonSerializer.Serialize(chatHistory.Select(x => new { role = x.Role, content = x.Content }), s_options);
        _logger.LogInformation("Chat History:\n {chatHistoryString}", chatHistoryString);
        
        await foreach (var token in chatService.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel, cancellationToken: cancellationToken))
        {
            if (string.IsNullOrEmpty(token.Content)) continue;
            yield return token.Content;
        }
    }
    public async Task<string?> ChatResponse(ChatHistory chatMessages, string systemPrompt, string model = "gpt-4o-mini", CancellationToken cancellationToken = default)
    {
        var item = GetServiceId();
        var settings = GetServicePromptExecutionSettings(item);
        var chatService = CreateKernel(model).Services.GetRequiredKeyedService<IChatCompletionService>(item);
        var chatHistory = new ChatHistory(systemPrompt);
        chatHistory.AddRange(chatMessages);
        var chatHistoryString = JsonSerializer.Serialize(chatHistory.Select(x => new { role = x.Role, content = x.Content, x.Items }), s_options);
        _logger.LogInformation("Chat History:\n {chatHistoryString}", chatHistoryString);
        var response = await chatService.GetChatMessageContentAsync(chatHistory, settings, cancellationToken: cancellationToken);
        return response.Content;
    }
    public async Task<string?> ChatResponse(ChatHistory chatMessages, PromptExecutionSettings? settings = null, string model = "gpt-4o-mini", CancellationToken cancellationToken = default)
    {
        var item = GetServiceId();
        settings ??= GetServicePromptExecutionSettings(item);
        var chatService = CreateKernel(model).Services.GetRequiredKeyedService<IChatCompletionService>(item);
        var chatHistory = new ChatHistory();
        chatHistory.AddRange(chatMessages);
        var chatHistoryString = JsonSerializer.Serialize(chatHistory.Select(x => new { role = x.Role, content = x.Content }), s_options);
        _logger.LogInformation("Chat History:\n {chatHistoryString}", chatHistoryString);
        var response = await chatService.GetChatMessageContentAsync(chatHistory, settings, cancellationToken: cancellationToken);
        return response.Content;
    }
    public static PromptExecutionSettings GetServicePromptExecutionSettings(string item, bool allowToolCalls = false)
    {
        PromptExecutionSettings settings = item switch
        {
            "Google" => _appState.ChatSettings.AsGeminiPromptExecutionSettings(allowToolCalls),
            "Mistral" => _appState.ChatSettings.AsMistralPromptExecutionSettings(allowToolCalls),
            _ => _appState.ChatSettings.AsOpenAIPromptExecutionSettings(allowToolCalls)
        };
        return settings;
    }

    public static PromptExecutionSettings GetServicePromptExecutionSettings(bool allowToolCalls = false)
    {
        var item = GetServiceId();
        return GetServicePromptExecutionSettings(item, allowToolCalls);
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
            kernelBuilder.AddGoogleAIGeminiChatCompletion(model, _appState.ChatModelSettings.GoogleGeminiApiKey!, serviceId: "Google");
        else if (model.Contains("mixtral") || model.Contains("mistral"))
            kernelBuilder.AddMistralChatCompletion(model, _appState.ChatModelSettings.MistralApiKey!, serviceId: "Mistral");
        else
            kernelBuilder.AddAICompletion(_appState.ChatModelSettings, model);
        kernelBuilder.AddTextEmbeddings(_appState.EmbeddingModelSettings);
        var kernel = kernelBuilder.Build();

        return kernel;
    }
}
