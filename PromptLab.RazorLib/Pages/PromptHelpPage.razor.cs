﻿using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using PromptLab.Core.Models;
using PromptLab.Core.Plugins;
using PromptLab.Core.Services;
using PromptLab.RazorLib.ChatModels;
using PromptLab.RazorLib.Components.ChatComponents;
using PromptLab.RazorLib.Components.LogViewer;
using System.ComponentModel;
using PromptLab.RazorLib.Components;
using Radzen;

namespace PromptLab.RazorLib.Pages;

public partial class PromptHelpPage
{
	[Inject]
	private PromptEngineerService PromptEngineerService { get; set; } = default!;
	[Inject]
	private IConfiguration Configuration { get; set; } = default!;
	[Inject]
	private DialogService DialogService { get; set; } = default!;

	private ChatView _chatView;
	private bool _isBusy;
	private List<LogEntry> _logs = [];
	
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
		var chatStream = PromptEngineerService.ChatWithPromptEngineer(_chatView.ChatState.ChatHistory);
		await ExecuteChatSequence(chatStream);
		_isBusy = false;
		StateHasChanged();
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