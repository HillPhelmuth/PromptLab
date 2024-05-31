using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using PromptLab.Core.Models;
using PromptLab.Core.Plugins;
using PromptLab.Core.Services;
using PromptLab.RazorLib.ChatModels;
using PromptLab.RazorLib.Components.ChatComponents;
using PromptLab.RazorLib.Components.LogViewer;
using System.ComponentModel;
using Radzen;
using System.Threading;
using PromptLab.RazorLib.Components.ModalWindows;
using Microsoft.SemanticKernel;

namespace PromptLab.RazorLib.Pages;

public partial class PromptHelpPage
{
	[Inject]
	private PromptEngineerService PromptEngineerService { get; set; } = default!;
	[Inject]
	private IConfiguration Configuration { get; set; } = default!;
	[Inject]
	private DialogService DialogService { get; set; } = default!;
    [Inject]
    private BlobService BlobService { get; set; } = default!;
    private ChatView _chatView;
	private bool _isBusy;
	private List<LogEntry> _logs = [];
	private CancellationTokenSource _cancellationTokenSource = new();
	protected override Task OnInitializedAsync()
	{
		PromptEngineerService.LogItem += HandleLog;
		return base.OnInitializedAsync();
	}
	private void HandleLog(LogEntry log)
	{
		_logs.Add(log);
		InvokeAsync(StateHasChanged);
	}
	protected override async void UpdateState(object? sender, PropertyChangedEventArgs args)
	{
		if (args.PropertyName == nameof(AppState.PromptToSave))
		{
			var properties = new Dictionary<string, object> { ["MessageText"] = AppState.PromptToSave, ["ShowConfirmButton"] = true };
			bool save = await DialogService.OpenAsync<MessageModalWindow>("Save Prompt", properties);
			if (save)
			{
				AppState.ActiveSystemPromptHtml = AsHtml(AppState.PromptToSave);
			}
		}
		base.UpdateState(sender, args);
	}
	private void ClearChat()
	{
		_chatView.ChatState.Reset();
	}
	private async void HandleInput(string input)
	{
		_isBusy = true;
		StateHasChanged();
		await Task.Delay(1);
        _chatView.ChatState.AddUserMessage(input);
        await SubmitRequest();
        _isBusy = false;
		StateHasChanged();
	}

    private Task SubmitRequest()
    {
        _cancellationTokenSource = new();
        var token = _cancellationTokenSource.Token;		
        var chatStream = PromptEngineerService.ChatWithPromptEngineer(_chatView.ChatState.ChatHistory, token);
        return ExecuteChatSequence(chatStream);
    }

    private async void HandleRequest(UserInputRequest userInputRequest)
    {
        if (userInputRequest.FileUpload is null)
        {
            HandleInput(userInputRequest.ChatInput);
            return;
        }
        var text = new TextContent(userInputRequest.ChatInput);
        var url = await GetImageUrlFromBlobStorage(userInputRequest.FileUpload!);
        var image = new ImageContent(new Uri(url));
        _chatView.ChatState.AddUserMessage(text, image);
        await SubmitRequest();
    }
    private async Task<string> GetImageUrlFromBlobStorage(FileUpload file)
    {
        var url = await BlobService.UploadBlobFile(file.FileName!, file.FileBytes!);
        return url;
    }
    private void Cancel()
	{
		_cancellationTokenSource.Cancel();
	}
	private async Task ExecuteChatSequence(IAsyncEnumerable<string> chatseq)
	{
		var hasStarted = false;
		await foreach (var response in chatseq)
		{
			if (!hasStarted)
			{
				hasStarted = true;
				_chatView.ChatState.AddAssistantMessage(response);
				_chatView.ChatState.ChatMessages.LastOrDefault(x => x.Role == Role.Assistant)!
			 .IsActiveStreaming = true;
				continue;
			}

			_chatView.ChatState.UpdateAssistantMessage(response);
		}
		var lastAsstMessage =
			_chatView.ChatState.ChatMessages.LastOrDefault(x => x.Role == Role.Assistant);
		if (lastAsstMessage is not null)
			lastAsstMessage.IsActiveStreaming = false;
	}
	private string AsHtml(string? text)
	{
		if (text == null) return "";
		var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
		var result = Markdown.ToHtml(text, pipeline);
		return result;

	}
}