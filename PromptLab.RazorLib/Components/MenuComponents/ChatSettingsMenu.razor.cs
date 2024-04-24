using Microsoft.AspNetCore.Components;
using PromptLab.Core.Models;
using PromptLab.Core.Services;
using System.ComponentModel;

namespace PromptLab.RazorLib.Components.MenuComponents;

public partial class ChatSettingsMenu 
{
    [Parameter]
    public ChatSettings? ChatSettings { get; set; }
    [Parameter]
    public EventCallback<ChatSettings> ChatSettingsChanged { get; set; }

    private ChatSettings _chatSettings = new();
    private List<string> AvailableModels => AppState.UserProfile.AvailableModels();
	protected override void UpdateState(object? sender, PropertyChangedEventArgs args)
	{
        if (args.PropertyName == nameof(AppState.ChatSettings))
        {
	        _chatSettings = AppState.ChatSettings;
	        StateHasChanged();
		}
		base.UpdateState(sender, args);
	}
	protected override void OnParametersSet()
	{
		ChatSettings ??= AppState.ChatSettings;
        _chatSettings = AppState.ChatSettings;
        StateHasChanged();
		base.OnParametersSet();
	}
	private async void Submit(ChatSettings chatSettings)
    {
        AppState.ChatSettings = chatSettings;
        AppState.IsLogProbView = chatSettings.LogProbs && !chatSettings.Model.StartsWith("gemini");
        await FileService.SaveUserSettings(AppState.UserProfile);
        ChatSettings = chatSettings;
        await ChatSettingsChanged.InvokeAsync(chatSettings);
    }
}