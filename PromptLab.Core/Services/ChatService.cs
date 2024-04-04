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

namespace PromptLab.Core.Services;

public class ChatService
{
    private readonly IConfiguration _configuration;
    private static string _apiKey;
    
    public ChatService(IConfiguration configuration)
    {
        _configuration = configuration;
        _apiKey = _configuration["OpenAI:ApiKey"]!;
    }
    public async Task<ChatChoice> GetLogProbs(ChatHistory chatMessages, float temp, float topP, string systemPrompt = "You are a helpful AI model", string model = "gpt-3.5-turbo")
    {
        var options = new ChatCompletionsOptions { LogProbabilitiesPerToken = 5, EnableLogProbabilities = true, Temperature = temp, NucleusSamplingFactor = topP, DeploymentName = model };
        options.Messages.Add(new ChatRequestSystemMessage(systemPrompt));
        foreach (var message in chatMessages)
        {
            if (message.Role == AuthorRole.User)
                options.Messages.Add(new ChatRequestUserMessage(message.Content));
            if (message.Role == AuthorRole.Assistant)
                options.Messages.Add(new ChatRequestAssistantMessage(message.Content));
        }
        var chat = new OpenAIClient(_configuration["OpenAI:ApiKey"]);
        var response = await chat.GetChatCompletionsAsync(options);
        var responses = await chat.GetChatCompletionsStreamingAsync(options);
        List<StreamingChatCompletionsUpdate> streamItem = await responses.ToListAsync();
        var chatChoice = response.Value.Choices[0];
        return chatChoice;
    }
    public async IAsyncEnumerable<string> StreamingChatResponse(ChatHistory chatMessages, OpenAIPromptExecutionSettings settings, string systemPrompt, string model = "gpt-3.5-turbo")
    {
        var chatService = CreateKernel(model).Services.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory(systemPrompt);
        chatHistory.AddRange(chatMessages);
        await foreach (var token in chatService.GetStreamingChatMessageContentsAsync(chatHistory, settings))
        {
            if (string.IsNullOrEmpty(token.Content)) continue;
            yield return token.Content;
        }
    }
    public Kernel CreateKernel(string model)
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddLogging(builder =>
        {
            builder.AddConsole();
        });
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
        var kernel = kernelBuilder
            .AddOpenAIChatCompletion(model, _apiKey)
            .Build();

        return kernel;
    }
}