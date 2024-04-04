using Microsoft.AspNetCore.Components;
using PromptLab.Core.Models;

namespace PromptLab.RazorLib.Components.MenuComponents;

public partial class ChatSettingsMenu 
{
    [Parameter]
    public ChatSettings ChatSettings { get; set; } = new();
    [Parameter]
    public EventCallback<ChatSettings> ChatSettingsChanged { get; set; }

    private ChatSettings _chatSettings = new();
    private void Submit(ChatSettings chatSettings)
    {
        AppState.ChatSettings = chatSettings;
        AppState.IsLogProbView = chatSettings.LogProbs;
        ChatSettings = chatSettings;
        ChatSettingsChanged.InvokeAsync(chatSettings);
    }
}