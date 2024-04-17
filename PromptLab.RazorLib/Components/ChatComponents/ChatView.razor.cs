using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.SemanticKernel.ChatCompletion;
using PromptLab.RazorLib.ChatModels;
using Radzen.Blazor;
using AppJsInterop = PromptLab.RazorLib.Shared.AppJsInterop;

namespace PromptLab.RazorLib.Components.ChatComponents;

public partial class ChatView
{
	private RadzenColumn? _column;
	private ElementReference _chatColumn;
	public ChatState ChatState
	{
		get
		{
			if (string.IsNullOrEmpty(ViewId))
			{
				return ChatStateCollection.CreateChatState(ViewId);
			}
			return ChatStateCollection.TryGetChatState(ViewId, out var chatState) ? chatState! : ChatStateCollection.CreateChatState(ViewId);
		}		
	}
	[Inject]
	private ChatStateCollection ChatStateCollection { get; set; } = default!;
	//[Inject]
	private AppJsInterop AppJsInterop { get; set; } = default!;
	[Parameter] public string Height { get; set; } = "60vh";
	[Parameter] public bool ResetOnClose { get; set; } = true;
	[Parameter]
	public bool IsLogProbView { get; set; }
	[Parameter]
	public bool Timestamps { get; set; } = true;

	/// <summary>
	/// Unique Identifier for ChatView instance. If you have multiple ChatView components in your application,
	/// you need to provide unique ViewId for each of them. If left empty, ChatState will not persist when component is disposed.
	/// </summary>
	[Parameter]
	public string ViewId { get; set; } = "";

	private bool _generatedViewId;

	[Inject] private IJSRuntime JsRuntime { get; set; } = default!;

	protected override Task OnParametersSetAsync()
	{
		ChatState.PropertyChanged += ChatState_OnChatStateChanged;
		StateHasChanged();

		return base.OnParametersSetAsync();
	}

	public List<(string role, string? message)> GetMessageHistory()
	{
		return ChatState.ChatMessages.Select(x => (x.Role.ToString(), x.Content)).ToList();
	}
	public ChatHistory GetChatHistory()
	{
		return ChatState.ChatHistory;
	}
	private async void ChatState_OnChatStateChanged(object? sender, PropertyChangedEventArgs args)
	{
		if (args.PropertyName == nameof(ChatState.ChatMessages))
		{
			ChatStateCollection.ChatStates[ViewId] = ChatState;
			await InvokeAsync(StateHasChanged);
			AppJsInterop = new AppJsInterop(JsRuntime);
			await AppJsInterop.ScrollDown(_chatColumn);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (ResetOnClose) ChatState.Reset();
			if (_generatedViewId) ChatStateCollection.ChatStates.Remove(ViewId);
			ChatState.PropertyChanged -= ChatState_OnChatStateChanged;
		}
		base.Dispose(disposing);
	}
}