using System.Text.Json;
using Microsoft.AspNetCore.Components;
using PromptLab.Core.Models;
using Radzen;

namespace PromptLab.RazorLib.Components.ChatComponents.LogProbComponents;

public partial class TextAsLogProbTokens : ComponentBase
{
    [Parameter]
    public List<TokenString> TokenStrings { get; set; } = [];
    [Parameter]
    public string FontSize { get; set; } = "1.1rem";
    [Parameter]
    public TokenString SelectedTokenString { get; set; } = default!;
    [Parameter]
    public EventCallback<TokenString> SelectedTokenStringChanged { get; set; }
    [Parameter]
    public List<TokenString>? SpecifiedTokens { get; set; }
    [Inject]
    private DialogService DialogService { get; set; } = default!;
    public void HandleSelectedTokenString(TokenString token)
    {
        SelectedTokenString = token;
        SelectedTokenStringChanged.InvokeAsync(token);
        DialogService.Open<AlternativesGrid>("", new Dictionary<string, object> { ["TokenString"] = token }, new DialogOptions { ShowClose = false, ShowTitle = false, CloseDialogOnOverlayClick = true, CloseDialogOnEsc = true, Draggable = true, Resizable=true, Style= "padding:0"});
        StateHasChanged();
    }
    protected override Task OnParametersSetAsync()
    {
        foreach (var tokenString in TokenStrings)
        {
            Console.WriteLine("Token LogProbs: " + JsonSerializer.Serialize(tokenString, new JsonSerializerOptions { WriteIndented=true}));
        }
        return base.OnParametersSetAsync();
    }
}