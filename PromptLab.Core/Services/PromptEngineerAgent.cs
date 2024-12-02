using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using PromptLab.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PromptLab.Core.Models;
using System.Runtime.CompilerServices;
#pragma warning disable SKEXP0001

namespace PromptLab.Core.Services;

public class PromptEngineerAgent
{
    private readonly IConfiguration _configuration;
    private static string _apiKey;
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };
    public event Action<LogEntry>? LogItem;
    private readonly ILogger<PromptEngineerAgent> _logger;
    private readonly AppState _appState;

    public PromptEngineerAgent(IConfiguration configuration, ILoggerFactory loggerFactory, AppState appState)
    {
        _configuration = configuration;
        _appState = appState;
        _apiKey = _configuration["OpenAI:ApiKey"]!;
        _logger = loggerFactory.CreateLogger<PromptEngineerAgent>();
    }
    public async IAsyncEnumerable<string> ChatWithPromptEngineer(ChatHistory chatHistory, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var item = ChatService.GetServiceId();
        chatHistory.AddSystemMessage(Prompt.PromptEngineerSystemPrompt);
        var kernel = ChatService.CreateKernel(_appState.ChatSettings.Model);
        AddPluginsAndFilters(kernel);
        PromptExecutionSettings settings = item == "Google" ? _appState.ChatSettings.AsGeminiPromptExecutionSettings() : _appState.ChatSettings.AsOpenAIPromptExecutionSettings();
        var chatService = kernel.Services.GetRequiredKeyedService<IChatCompletionService>(item);
        //var settings = new OpenAIPromptExecutionSettings { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions, MaxTokens = 1024 };
        var chatStream = item == "OpenAI" ? OpenAIChatStream(chatHistory, cancellationToken, chatService as OpenAIChatCompletionService, _appState.ChatSettings, kernel) : GoogleChatStream(chatHistory, cancellationToken, chatService, _appState.ChatSettings, kernel);
        await foreach (var update in chatStream.WithCancellation(cancellationToken))
        {
            if (string.IsNullOrEmpty(update.Content)) continue;
            yield return update.Content;
        }
    }

    private static IAsyncEnumerable<StreamingChatMessageContent> OpenAIChatStream(ChatHistory chatHistory, CancellationToken cancellationToken,
        OpenAIChatCompletionService chatService, ChatSettings chatSettings, Kernel kernel)
    {
        var settings = chatSettings.AsOpenAIPromptExecutionSettings(allowParallel:true, allowConcurrant:true);
        var chatStream = chatService.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel, cancellationToken);
        return chatStream;
    }
    private static IAsyncEnumerable<StreamingChatMessageContent> GoogleChatStream(ChatHistory chatHistory, CancellationToken cancellationToken,
        IChatCompletionService chatService, ChatSettings chatSettings, Kernel kernel)
    {
        var settings = chatSettings.AsGeminiPromptExecutionSettings();
        var chatStream = chatService.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel, cancellationToken);
        return chatStream;
    }
    private void AddPluginsAndFilters(Kernel kernel)
    {
        AddFilters(kernel);
        kernel.ImportPluginFromType<PromptEvaluationPlugin>();
        kernel.ImportPluginFromType<PromptExpertPlugin>();
        kernel.ImportPluginFromType<SavePromptPlugin>();
    }

    private void AddFilters(Kernel kernel)
    {
        var filters = new FunctionFilter();
        var promptFilters = new PromptFilter();
        var autoInvoke = new AutoInvokeFunctionFilter();
        autoInvoke.AutoFunctionInvoking += OnAutoFunctionInvoking;
        //filters.FunctionInvoked += OnFunctionInvoked;
        promptFilters.PromptRendered += OnPromptRendered;
        autoInvoke.AutoFunctionInvoked += OnAutoFunctionInvoked;
        //kernel.FunctionInvocationFilters.Add(filters);
        kernel.PromptRenderFilters.Add(promptFilters);
        kernel.AutoFunctionInvocationFilters.Add(autoInvoke);
    }

    public async Task<string> HelpFromPromptEngineer(string prompt)
    {
        var kernel = ChatService.CreateKernel(_appState.ChatSettings.Model);
        AddPluginsAndFilters(kernel);
        var chatHistory = new ChatHistory(Prompt.PromptModificationPrompt);
        var input = $"""
		             Improve this prompt. Do not evaluate. Do not explain or ask for approval. Just think carefully about each change you will make to the prompt and then create the improved prompt and save the prompt with 'SavePrompt'.

		             ## Prompt
		             
		             ```markdown
		             {prompt}
		             ```
		             """;
        chatHistory.AddUserMessage(input);
        var key = ChatService.GetServiceId();
        var chatService = kernel.Services.GetRequiredKeyedService<IChatCompletionService>(key);
        var options = new FunctionChoiceBehaviorOptions()
            { AllowConcurrentInvocation = true, AllowParallelCalls = true };
        var settings = new OpenAIPromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options:options), MaxTokens = 1024 };
        var response = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
        _logger.LogInformation("Chat Response: {response}", response.Content);
        return response.Content ?? "";
    }

    public async Task<PromptEval> EvaluatePrompt(string prompt, CancellationToken token = default)
    {
        var chatSettingsModel = _appState.ChatSettings.Model.Contains("gpt-4") ? _appState.ChatSettings.Model : "gpt-4o-mini";
        var kernel = ChatService.CreateKernel(chatSettingsModel);
        var evalPlugin = kernel.ImportPluginFromType<PromptEvaluationPlugin>();
        var evalPromptFunction = evalPlugin["EvaluatePrompt"];
        var arguments = new KernelArguments(new OpenAIPromptExecutionSettings(){ResponseFormat = "json_object"}) { ["prompt"] = prompt };
        //var result = await kernel.InvokeAsync<string>(evalPromptFunction, arguments, token);
        var result2 = await evalPromptFunction.InvokeAsync<string>(kernel, arguments, token);
		var promptEval = JsonSerializer.Deserialize<PromptEval>(result2.Replace("```json","").Replace("```","").TrimStart('\n'));
		return promptEval;

	}
    public async Task<string> EvaluateUserPrompt(string prompt, ChatHistory chatHistory, CancellationToken token = default)
    {
        var kernel = ChatService.CreateKernel(_appState.ChatSettings.Model);
        var helperPlugin = kernel.ImportPluginFromType<PromptExpertPlugin>();
        var userPromptHelperFunction = helperPlugin["ImproveUserPrompt"];
        var sb = new StringBuilder();
        foreach (var message in chatHistory)
        {
            var text = $"Role: {message.Role}\nContent:\n{message.Content}";
            sb.AppendLine();
            sb.AppendLine(text);
        }
        var arguments = new KernelArguments() { ["prompt"] = prompt, ["chatHistory"] = sb.ToString() };
        var result = await userPromptHelperFunction.InvokeAsync<string>(kernel, arguments, token);
        
        return result ?? "Error evaluating user prompt";
    }
    private void OnPromptRendered(object? sender, PromptRenderContext context)
    {
        LogItem?.Invoke(new LogEntry(context.RenderedPrompt ?? "No RenderedPrompt", DisplayType.Prompt, "Prompt Function", "The rendered prompt of a prompt function"));
    }

    private void OnFunctionInvoked(object? sender, FunctionInvocationContext context)
    {
        var functionResult = context.Result;
        var functionName = context.Function.Name;
        var functionDescription = context.Function.Description;
        AddLogItems(functionName, functionResult, functionDescription);
    }
    private void OnAutoFunctionInvoked(object? sender, AutoFunctionInvocationContext context)
    {
        var functionResult = context.Result;
        var functionName = context.Function.Name;
        var functionDescription = context.Function.Description;
        AddLogItems(functionName, functionResult, functionDescription);
    }
    private void OnAutoFunctionInvoking(object? sender, AutoFunctionInvocationContext context)
    {
        //var functionResult = context.Result;
        var functionName = context.Function.Name;
        var functionDescription = context.Function.Description;
        var args = context.Arguments;
        LogItem?.Invoke(new LogEntry($"Function {functionName} - {functionDescription} Invoking\n```json\n{JsonSerializer.Serialize(args,new JsonSerializerOptions(){WriteIndented = true})}\n```", DisplayType.Markdown, functionName + " Invoking", functionDescription));
    }
    private void AddLogItems(string functionName, FunctionResult functionResult, string functionDescription)
    {
        var value = functionResult.GetValue<string>()!;
        value = value.Replace("```json", "").Replace("```", "").TrimStart('\n');
        switch (functionName)
        {
            case "EvaluatePrompt":
            {
                var result = JsonSerializer.Deserialize<PromptEval>(value);
                var jsonResult = JsonSerializer.Serialize(new {Name = functionName, Result = result }, JsonSerializerOptions);
                LogItem?.Invoke(new LogEntry(jsonResult, DisplayType.Json, functionName, functionDescription));
                break;
            }
            case "GeneralExpertAdvice":
            case "PromptExpertAdvice":
            {
                var result = value;
                LogItem?.Invoke(new LogEntry(result, DisplayType.Markdown, functionName, functionDescription));
                break;
            }
            case "SavePrompt":
            {
                break;
            }
            case "Recall":
            {
                var result = functionResult.GetValue<object>()!;
                var jsonResult = JsonSerializer.Serialize(new {Name = functionName, Result = result }, JsonSerializerOptions);
                LogItem?.Invoke(new LogEntry(jsonResult, DisplayType.Json, functionName, functionDescription));
                break;
            }

        }
    }
}