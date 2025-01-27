using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.BedrockRuntime;
using HillPhelmuth.SemanticKernel.LlmAsJudgeEvals;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using PromptLab.Core.Helpers;
using PromptLab.Core.Models;
using PromptLab.Core.Models.HttpDelegateHandlers;

namespace PromptLab.Core.Services;

public class PromptEvalService
{
    private readonly IConfiguration _configuration;
    private static readonly JsonSerializerOptions s_options = new() { WriteIndented = true };
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<PromptEvalService> _logger;
    private readonly AppState _appState;
    private readonly StringEventWriter _stringEventWriter;

    public PromptEvalService(IConfiguration configuration, ILoggerFactory loggerFactory, AppState appState, StringEventWriter stringEventWriter)
    {
        _configuration = configuration;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<PromptEvalService>();
        _appState = appState;
        _stringEventWriter = stringEventWriter;
        
        
    }

    public async IAsyncEnumerable<EvalResultDisplay> ExecuteEvals(Dictionary<string, List<IInputModel>> modelInputModels)
    {
        foreach (var inputModel in modelInputModels)
        {
            var kernel = CreateKernel("gpt-4o-mini");
            var evalService = new EvalService(kernel);
            foreach (var input in inputModel.Value)
            {
                _logger.LogInformation($"Evaluating {JsonSerializer.Serialize(input, s_options)}");
                var result = await evalService.ExecuteEval(input);
                var question = input.RequiredInputs["question"]!.ToString();
                var hasAnswer = input.RequiredInputs.TryGetValue("answer", out var answer);
                var hasContext = input.RequiredInputs.TryGetValue("context", out var context);
                yield return new EvalResultDisplay(question, answer.ToString(), inputModel.Key, result)
                    { Context = context.ToString() };
            }
        }
    }
    private Kernel CreateKernel(string model)
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
        var handler = new LoggingHandler(new HttpClientHandler(), _stringEventWriter);
        var client = new HttpClient(handler);
        if (model.StartsWith("gemini"))
            kernelBuilder.AddGoogleAIGeminiChatCompletion(model, _appState.ChatModelSettings.GoogleGeminiApiKey!, httpClient: client);
        else if (model.Contains("mixtral") || model.Contains("mistral"))
            kernelBuilder.AddMistralChatCompletion(model, _appState.ChatModelSettings.MistralApiKey!);
        else if (model.Contains("claude"))
            kernelBuilder.AddVertexAIChatCompletion(model);
        else if (model.StartsWith("local"))
        {
            var endpoint = model.Contains("lmstudio") ? new Uri("http://localhost:1234/v1") : new Uri("http://localhost:11434/v1");
            kernelBuilder.AddOpenAIChatCompletion(modelId: model.Split('-')[2], apiKey: "", endpoint: endpoint, httpClient: client);
        }
        else
            kernelBuilder.AddAICompletion(_appState.ChatModelSettings, model, null);
        kernelBuilder.AddTextEmbeddings(_appState.EmbeddingModelSettings);
        var kernel = kernelBuilder.Build();

        return kernel;
    }

}
public record EvalResultDisplay(string Question, string Answer, string Model, ResultScore ResultScore)
{
    public string? Context { get; set; }
}