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

namespace PromptLab.Core.Services;

public class PromptEngineerService
{
	private readonly IConfiguration _configuration;
	private static string _apiKey;
	private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };
    public event Action<LogEntry>? LogItem;
	private readonly ILogger<PromptEngineerService> _logger;
	private readonly AppState _appState;
	
	public PromptEngineerService(IConfiguration configuration, ILoggerFactory loggerFactory, AppState appState)
	{
		_configuration = configuration;
		_appState = appState;
		_apiKey = _configuration["OpenAI:ApiKey"]!;
		_logger = loggerFactory.CreateLogger<PromptEngineerService>();
	}
	public async IAsyncEnumerable<string> ChatWithPromptEngineer(ChatHistory chatHistory)
	{
		chatHistory.AddSystemMessage(Prompt.PromptEngineerSystemPrompt);
		var kernel = ChatService.CreateKernel(_appState.ChatSettings.Model);
		AddPluginsAndFilters(kernel);
        var chatService = kernel.Services.GetRequiredService<IChatCompletionService>();
		var settings = new OpenAIPromptExecutionSettings { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions, MaxTokens = 1024 };
		await foreach (var update in chatService.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel))
		{
			if (string.IsNullOrEmpty(update.Content)) continue;
			yield return update.Content;
		}
	}

    private void AddPluginsAndFilters(Kernel kernel)
    {
        var filters = new FunctionFilter();
        var promptFilters = new PromptFilter();
        filters.FunctionInvoked += OnFiltersOnFunctionInvoked;
        promptFilters.PromptRendered += OnPromptFiltersOnPromptRendered;
		
        kernel.FunctionFilters.Add(filters);
        kernel.PromptFilters.Add(promptFilters);
        var evaluator = kernel.ImportPluginFromType<PromptEvaluationPlugin>();
        kernel.ImportPluginFromType<PromptExpertPlugin>();
        kernel.ImportPluginFromType<SavePromptPlugin>();
    }
	public async Task<string> HelpFromPromptEngineer(string prompt)
	{
        var kernel = ChatService.CreateKernel(_appState.ChatSettings.Model);
        AddPluginsAndFilters(kernel);
        var chatHistory = new ChatHistory(Prompt.PromptEngineerSystemPrompt);
		var input = $"Improve this prompt. Do not evaluate. Then Save the prompt with 'SavePrompt'.\n\n## Prompt\n\n{prompt}";
		chatHistory.AddUserMessage(input);
        var chatService = kernel.Services.GetRequiredService<IChatCompletionService>();
        var settings = new OpenAIPromptExecutionSettings { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions, MaxTokens = 1024 };
		var response = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
		_logger.LogInformation("Chat Response: {response}", response.Content);
		return response.Content ?? "";
    }
    private void OnPromptFiltersOnPromptRendered(object? sender, PromptRenderedContext context)
    {
        LogItem?.Invoke(new LogEntry(context.RenderedPrompt, DisplayType.Prompt, "Prompt Function", "The rendered prompt of a prompt function"));
    }

    private void OnFiltersOnFunctionInvoked(object? sender, FunctionInvokedContext context)
    {
        switch (context.Function.Name)
        {
            case "EvaluatePrompt":
            {
                var result = JsonSerializer.Deserialize<PromptEval>(context.Result.GetValue<string>()!);
                var jsonResult = JsonSerializer.Serialize(new {context.Function.Name, Result = result}, JsonSerializerOptions);
                LogItem?.Invoke(new LogEntry(jsonResult, DisplayType.Json, context.Function.Name, context.Function.Description));
                break;
            }
            case "PromptExpertAdvice":
            {
                var result = context.Result.GetValue<string>()!;
                LogItem?.Invoke(new LogEntry(result, DisplayType.Markdown, context.Function.Name, context.Function.Description));
                break;
            }
        }
    }
}
public class FunctionFilter : IFunctionFilter 
{
	public event EventHandler<FunctionInvokingContext>? FunctionInvoking;
	public event EventHandler<FunctionInvokedContext>? FunctionInvoked;
	public void OnFunctionInvoking(FunctionInvokingContext context)
	{
		
		FunctionInvoking?.Invoke(this, context);
	}

	public void OnFunctionInvoked(FunctionInvokedContext context)
	{
		
		FunctionInvoked?.Invoke(this, context);
	}
}
public class PromptFilter : IPromptFilter
{
	public event EventHandler<PromptRenderingContext>? PromptRendering;
	public event EventHandler<PromptRenderedContext>? PromptRendered;

	public void OnPromptRendering(PromptRenderingContext context)
	{
		PromptRendering?.Invoke(this, context);
	}

	public void OnPromptRendered(PromptRenderedContext context)
	{
		PromptRendered?.Invoke(this, context);
	}
}
