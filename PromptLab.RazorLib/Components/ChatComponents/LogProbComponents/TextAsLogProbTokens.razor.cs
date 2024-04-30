using System.Text.Json;
using Microsoft.AspNetCore.Components;
using PromptLab.Core.Models;
using PromptLab.RazorLib.ChatModels;
using Radzen;
using Message = PromptLab.RazorLib.ChatModels.Message;

namespace PromptLab.RazorLib.Components.ChatComponents.LogProbComponents;

public partial class TextAsLogProbTokens : ComponentBase
{

    public List<TokenString> TokenStrings => Message.TokenStrings;
    [Parameter]
    [EditorRequired]
    public Message Message { get; set; } = default!;
	[Parameter]
    public string FontSize { get; set; } = "1.1rem";
    [Parameter]
    public TokenString SelectedTokenString { get; set; } = default!;
    [Parameter]
    public EventCallback<TokenString> SelectedTokenStringChanged { get; set; }
    [Parameter]
    public List<TokenString>? SpecifiedTokens { get; set; }
    [Parameter]
    public EventCallback<Message> OnRemove { get; set; }
    [CascadingParameter(Name = "AllowRemove")]
    public bool AllowRemove { get; set; }
    [Inject]
    private DialogService DialogService { get; set; } = default!;
    public void HandleSelectedTokenString(TokenString token)
    {
        SelectedTokenString = token;
        SelectedTokenStringChanged.InvokeAsync(token);
        DialogService.Open<AlternativesGrid>("", new Dictionary<string, object> { ["TokenString"] = token }, new DialogOptions { ShowClose = false, ShowTitle = false, CloseDialogOnOverlayClick = true, CloseDialogOnEsc = true, Draggable = true, Resizable=true, Style= "padding:0"});
        StateHasChanged();
    }
    
}