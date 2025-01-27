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
using Microsoft.Extensions.AI;
using Anthropic;
using PromptLab.Core.Plugins;
using Claude = Anthropic.AnthropicClient;
using ChatMessage = OpenAI.Chat.ChatMessage;
using System.Text.RegularExpressions;
using PromptLab.Core.Models.HttpDelegateHandlers;
using static Microsoft.SemanticKernel.Connectors.Amazon.Core.MistralResponse;

namespace PromptLab.Core.Services;
public record ReasoningResponse(string Think, string Answer);
public class ChatService
{
    private static IConfiguration _configuration;
    private static readonly JsonSerializerOptions s_options = new() { WriteIndented = true };
    private static ILoggerFactory _loggerFactory;
    private readonly ILogger<ChatService> _logger;
    private static AppState _appState;
    private static StringEventWriter _stringEventWriter;
    public event Action<string>? FunctionCallResult;
    public ChatService(IConfiguration configuration, ILoggerFactory loggerFactory, AppState appState, StringEventWriter stringEventWriter)
    {
        _configuration = configuration;
        _loggerFactory = loggerFactory;
        _appState = appState;
        _stringEventWriter = stringEventWriter;
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
        var settings = GetServicePromptExecutionSettings(item, _appState.ChatSettings.EnableTools);
        var kernel = CreateKernel(model);
        if (_appState.ChatSettings.EnableTools)
        {
            var filter = new AutoInvokeFilter(_loggerFactory);
            filter.FunctionResult += (result) => FunctionCallResult?.Invoke(result);
            kernel.AutoFunctionInvocationFilters.Add(filter);
            //kernel.ImportPluginFromType<WebResearchPlugin>();
            kernel.ImportPluginFromType<WebCrawlPlugin>();
            kernel.ImportPluginFromType<YouTubePlugin>();
            kernel.ImportPluginFromType<ArxivPlugin>();
            if (_appState.ChatSettings.CustomPlugins.Any())
            {
                kernel.Plugins.AddRange(_appState.ChatSettings.CustomPlugins);
            }
        }
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

    public async IAsyncEnumerable<string> Streamingo1ModelRespose(ChatHistory chatMessages, string devPrompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatSettings = _appState.ChatSettings;
        var options = new ChatCompletionOptions() { MaxOutputTokenCount = chatSettings.MaxTokens, StoredOutputEnabled = true };
        var messages = new List<ChatMessage> { ChatMessage.CreateUserMessage(devPrompt) };
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
        await foreach (var token in streaming)
        {
            if (token.ContentUpdate?.Count == 0) continue;
            var stringContent = token.ContentUpdate?[0]?.Text;
            if (string.IsNullOrEmpty(stringContent)) continue;
            yield return stringContent;
        }
    }
    public async Task<string?> ChatResponse(ChatHistory chatMessages, string systemPrompt, string model = "gpt-4o-mini", CancellationToken cancellationToken = default)
    {

        var item = GetServiceId();
        var settings = GetServicePromptExecutionSettings(item, _appState.ChatSettings.EnableTools);

        var kernel = CreateKernel(model);
        if (_appState.ChatSettings.EnableTools)
        {
            var filter = new AutoInvokeFilter(_loggerFactory);
            filter.FunctionResult += (result) => FunctionCallResult?.Invoke(result);
            kernel.AutoFunctionInvocationFilters.Add(filter);
            //kernel.ImportPluginFromType<WebResearchPlugin>();
            kernel.ImportPluginFromType<WebCrawlPlugin>();
            kernel.ImportPluginFromType<YouTubePlugin>();
            kernel.ImportPluginFromType<ArxivPlugin>();
            if (_appState.ChatSettings.CustomPlugins.Any())
            {
                kernel.Plugins.AddRange(_appState.ChatSettings.CustomPlugins);
            }
        }
        var chatService = kernel.Services.GetRequiredKeyedService<IChatCompletionService>(item);
        var chatHistory = new ChatHistory(systemPrompt);
        chatHistory.AddRange(chatMessages);
        var chatHistoryString = JsonSerializer.Serialize(chatHistory.Select(x => new { role = x.Role, content = x.Content, x.Items }), s_options);
        _logger.LogInformation("Chat History:\n {chatHistoryString}", chatHistoryString);

        var response = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel: kernel, cancellationToken: cancellationToken);
        _logger.LogInformation("Chat Response inner content:\n {response}", JsonSerializer.Serialize(new { response.InnerContent, response.Metadata }, s_options));
        var responseContent = response.Content;

        var reasoningResponse = ParseReasoningResponse(responseContent);
        if (string.IsNullOrEmpty(reasoningResponse.Think)) return responseContent;
        var thinkContent = ResultsExpando(reasoningResponse.Think);
        responseContent = thinkContent + reasoningResponse.Answer;
        return responseContent;
    }
    public static ReasoningResponse ParseReasoningResponse(string input)
    {
        // Define the regex to extract the content inside the <think> tags
        var thinkRegex = new Regex(@"<think>(.*?)<\/think>", RegexOptions.Singleline);
        var match = thinkRegex.Match(input);

        if (match.Success)
        {
            // Extract the content inside <think> tags
            string thinkContent = match.Groups[1].Value.Trim();
            // Remove the think section from the input to get the answer
            string answerContent = input.Replace(match.Value, "").Trim();
            return new ReasoningResponse(thinkContent, answerContent);
        }

        return new ReasoningResponse("", input);
    }
    private static string ResultsExpando(string result)
    {
        var resultsExpando = $"""

                              <details>
                                <summary>See Reasoning</summary>
                                
                                <h5>Thoughts</h5>
                                <p>
                                {result}
                                </p>
                              
                                <br/>
                              </details>
                              """;
        return resultsExpando;
    }

    public static PromptExecutionSettings GetServicePromptExecutionSettings(string item, bool allowToolCalls = false, string systemPrompt = "")
    {
        PromptExecutionSettings settings = item switch
        {
            "Google" => _appState.ChatSettings.AsGeminiPromptExecutionSettings(allowToolCalls),
            "Mistral" => _appState.ChatSettings.AsMistralPromptExecutionSettings(allowToolCalls),
            "Anthropic" => _appState.ChatSettings.AsAnthropicPromptExecutionSettings(allowToolCalls, systemPrompt),
            _ => _appState.ChatSettings.AsOpenAIPromptExecutionSettings(allowToolCalls, true, true)
        };
        return settings;
    }

    public static PromptExecutionSettings GetServicePromptExecutionSettings(bool allowToolCalls = false)
    {
        var item = GetServiceId();
        return GetServicePromptExecutionSettings(item, allowToolCalls);
    }

    public static string GetServiceId(string? model = null)
    {
        model ??= _appState.ChatSettings.Model;
        if (model.StartsWith("gemini"))
            return "Google";
        if (model.Contains("mixtral") || model.Contains("mistral"))
            return "Mistral";
        if (model.Contains("claude"))
            return "Anthropic";
        if (model.Contains("deepseek"))
            return "Deepseek";
        return "OpenAI";
    }

    private const string BedrockProfile = "";
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
        kernelBuilder.Services.AddSingleton<BingWebSearchService>();
        //kernelBuilder.Services.AddSqliteVectorStore(@"Data Source = .\Data\PromptLab.db", serviceId:"research");
        kernelBuilder.AddPineconeVectorStore(_configuration["Pinecone:ApiKey"]!);

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

        AddChatService(model, kernelBuilder);
        kernelBuilder.AddTextEmbeddings(_appState.EmbeddingModelSettings);
        var kernel = kernelBuilder.Build();

        return kernel;
    }

    private static void AddChatService(string model, IKernelBuilder kernelBuilder)
    {
        var loggingHandler = DelegateHandlerFactory.GetDelegatingHandler<LoggingHandler>(_stringEventWriter);
        var loggingClient = new HttpClient(loggingHandler) { Timeout = TimeSpan.FromMinutes(5) };
        if (model.Contains("mixtral") || model.Contains("mistral"))
            kernelBuilder.AddMistralChatCompletion(model, _appState.ChatModelSettings.MistralApiKey!, serviceId: "Mistral");
        else if (model.Contains("anthropic"))
            //kernelBuilder.AddAnthropicChatCompletion(model, _appState.ChatModelSettings.AnthropicApiKey!, serviceId: "Anthropic");
            kernelBuilder.AddBedrockChatCompletionService($"{BedrockProfile}{_appState.ChatSettings.Model}", serviceId: "Anthropic");
        else if (model.Contains("claude"))
            kernelBuilder.AddVertexAIChatCompletion(model);
        if (model.Contains("flash-thinking"))
        {
            var addThinkingHandler = DelegateHandlerFactory.GetDelegatingHandler<AddThinkingConfigHandler>(_stringEventWriter);
            var addThinkingClient = new HttpClient(addThinkingHandler) { Timeout = TimeSpan.FromMinutes(5) };
            kernelBuilder.AddGoogleAIGeminiChatCompletion(model, _appState.ChatModelSettings.GoogleGeminiApiKey!,
                serviceId: "Google", httpClient: addThinkingClient);
        }
        else if (model.StartsWith("gemini"))
        {
            
            kernelBuilder.AddGoogleAIGeminiChatCompletion(model, _appState.ChatModelSettings.GoogleGeminiApiKey!,
                serviceId: "Google", httpClient: loggingClient);
        }
        else if (model.StartsWith("local"))
        {
            var endpoint = model.Contains("lmstudio") ? new Uri("http://localhost:1234/v1") : new Uri("http://localhost:11434/v1");
            kernelBuilder.AddOpenAIChatCompletion(modelId: model.Split('-')[2], apiKey: "", endpoint: endpoint, httpClient: loggingClient, serviceId: "OpenAI");
        }
        else if (model.Contains("deepseek"))
        {
            var reasoningClientHanlHandler = new DeepseekReasoningContentHandler(new HttpClientHandler(), _stringEventWriter);
            var reasoningClient = new HttpClient(reasoningClientHanlHandler) { Timeout = TimeSpan.FromMinutes(5) };
            var endpoint = new Uri("https://api.deepseek.com/v1");
            kernelBuilder.AddOpenAIChatCompletion(modelId: model, apiKey: _appState.ChatModelSettings.DeepseekApiKey!, endpoint: endpoint, httpClient: reasoningClient, serviceId: "Deepseek");
        }
        else if (model.Contains("o1"))
        {
            var systemToDevHandler =
                DelegateHandlerFactory.GetDelegatingHandler<SystemToDeveloperRoleHandler>(_stringEventWriter);
            var systemToDevClient = new HttpClient(systemToDevHandler) { Timeout = TimeSpan.FromMinutes(5) };
            kernelBuilder.AddOpenAIChatCompletion(modelId: model, apiKey: _appState.ChatModelSettings.OpenAIApiKey!, httpClient: systemToDevClient, serviceId: "OpenAI");
        }
        else
            kernelBuilder.AddAICompletion(_appState.ChatModelSettings, model);
    }
}