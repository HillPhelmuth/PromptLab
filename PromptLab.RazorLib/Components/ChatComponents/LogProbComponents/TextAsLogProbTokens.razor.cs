using System.Text;
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
    public EventCallback<Message> OnContentUpdate { get; set; }
	[Parameter]
    public List<TokenString>? SpecifiedTokens { get; set; }
    [Parameter]
    public EventCallback<Message> OnRemove { get; set; }
    [CascadingParameter(Name = "AllowRemove")]
    public bool AllowRemove { get; set; }
    [Inject]
    private DialogService DialogService { get; set; } = default!;
    private bool _isModify;
    private class ModifyForm
    {
	    public string Content { get; set; } = "";
    }

    private ModifyForm _modifyForm = new();
	private void Modify()
    {
	    _isModify = true;
	    _modifyForm.Content = Message.Content ?? "";
	    StateHasChanged();
    }
    private void Cancel()
    {
	    _isModify = false;
	    StateHasChanged();
    }
    private void Accept(ModifyForm modifyForm)
    {
	    Message.Content = modifyForm.Content;
	    _isModify = false;
	    //OnContentUpdate.InvokeAsync(Message);
	    StateHasChanged();
    }
	public async void HandleSelectedTokenString(TokenString token)
    {
        SelectedTokenString = token;
        await SelectedTokenStringChanged.InvokeAsync(token);
        //DialogService.Open<AlternativesGrid>("", new Dictionary<string, object> { ["TokenString"] = token }, new DialogOptions { ShowClose = false, ShowTitle = false, CloseDialogOnOverlayClick = true, CloseDialogOnEsc = true, Draggable = true, Resizable=true, Style= "padding:0"});
        var result = await DialogService.OpenAsync<AlternativesGrid>("", new Dictionary<string, object> { ["TokenString"] = token }, new DialogOptions { ShowClose = false, ShowTitle = false, CloseDialogOnOverlayClick = true, CloseDialogOnEsc = true, Draggable = true, Resizable = true, Style = "padding:0" });
        if (result != null)
        {
            if (result is TokenString tokenString && tokenString.StringValue != token.StringValue)
            {
	            var indexOf = Message.TokenStrings.IndexOf(token);
	            Message.TokenStrings[indexOf] = tokenString;
                Message.TokenStrings = Message.TokenStrings.Take(indexOf + 1).ToList();
                var contentBuilder = new StringBuilder();
                foreach (var item in Message.TokenStrings)
				{
	                contentBuilder.Append(item.StringValue);
				}
                Message.Content = contentBuilder.ToString();
                await OnContentUpdate.InvokeAsync(Message);
            }
        }
        StateHasChanged();
    }
    
}