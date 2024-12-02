using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using BlazorJoditEditor;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using PromptLab.Core.Models;
using PromptLab.Core.Services;
using PromptLab.RazorLib.Components.ModalWindows;
using Radzen;

namespace PromptLab.RazorLib.Pages;
public partial class ImprovePromptPage
{
    [Inject]
    public MetaPromptService MetaPromptService { get; set; } = default!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    [Inject]
    private ChatService ChatService { get; set; } = default!;
    [Inject]
    private PromptEngineerAgent PromptEngineerService { get; set; } = default!;
    private List<LogEntry> AnalysisSteps = [];
    private int _currentStep = 0;
    private bool _isBusy;
    private int _selectedIndex;
    private CancellationTokenSource _cts = new();
    private JoditEditor? _joditEditor;

    private class PromptForm
    {
        public string Prompt { get; set; } = string.Empty;
    }

    private class CreatePromptForm
    {
        public string Task { get; set; } = string.Empty;
        public string InputVariables { get; set; } = string.Empty;
    }
    private CreatePromptForm _createPromptForm = new();
    private double _progress;
    private PromptForm _promptForm = new();
    protected override List<string> InterestingProperties => [nameof(AppState.ActiveSystemPromptHtml)];
    private string _stepDescription = "";
    protected override Task OnInitializedAsync()
    {
        _promptForm.Prompt = AppState.ActiveSystemPromptHtml;
        MetaPromptService.LogEntryUpdate += HandleLogEntryUpdate;
        MetaPromptService.NewStep += HandleNewStep;
        return base.OnInitializedAsync();
    }

    private async void HandleNewStep(bool obj)
    {
        _progress = (double)AnalysisSteps.Count / 6 * 100;
        if (obj)
            _isNewPrompt = true;
        else
        {
            _isBusy = false;

        }
        StateHasChanged();
        await Task.Delay(1000);
        _selectedIndex = AnalysisSteps.Count - 1;
        StateHasChanged();
    }

    private void UpdatePrimaryPrompt()
    {
        AppState.ActiveSystemPromptHtml = AsHtml(AnalysisSteps.Last().Content.Replace("```markdown", "").Replace("```", "").Trim('\n'));
        NavigationManager.NavigateTo("/");
    }
    private bool _isNewPrompt;
    private void HandleLogEntryUpdate(LogEntry obj)
    {
        if (_isNewPrompt)
        {
            AnalysisSteps.Add(obj);
            _stepDescription = obj.Label;
            _isNewPrompt = false;
        }
        else if (AnalysisSteps.Count > 0)
        {
            AnalysisSteps.Last().Content += obj.Content;
        }
        Debug.WriteLine($"Is new:{_isNewPrompt}, Content: {obj.Content}");
        InvokeAsync(StateHasChanged);
    }
    private async Task HandleExecute(string argsCommandName)
    {
        switch (argsCommandName)
        {
            case "Save":
                await SavePrompt();
                break;
            case "Load":
                await PickFile();
                break;
            case "Improve":
                await ImprovePrompt();
                break;
            case "Evaluate":
                await EvaluatePrompt();
                break;
        }
    }

    private async Task PickFile()
    {
        var item = await FileService.OpenFileAsync();
        if (string.IsNullOrEmpty(item)) return;
        AppState.ActiveSystemPromptHtml = AsHtml(item);
        await _joditEditor.SetContentAsync(AppState.ActiveSystemPromptHtml);
        StateHasChanged();
    }
    private async Task ImprovePrompt()
    {
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        var text = await PromptEngineerService.HelpFromPromptEngineer(AppState.ActiveSystemPrompt);
        //var parameters = new Dictionary<string, object> { ["MessageText"] = _text };
        //var dialogOptions = new DialogOptions { CloseDialogOnOverlayClick = true, Height = "40vh", Width = "80vw", Resizable = true, Draggable = true };
        //DialogService.Open<MessageModalWindow>("Response from Prompt Engineer Agent", parameters, dialogOptions);
        _isBusy = false;
        StateHasChanged();
    }
    private async Task SavePrompt()
    {
        Logger.LogInformation("Saving system prompt");
        var item = await FileService.SaveFileAsync("system_prompt.md", AppState.ActiveSystemPrompt);
        var savedPrompt = item ?? "";
        StateHasChanged();
    }

    private async Task EvaluatePrompt()
    {
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        var result = await PromptEngineerService.EvaluatePrompt(AppState.ActiveSystemPrompt);
        var properties = new Dictionary<string, object> { ["MessageText"] = result.ToString(), ["ShowConfirmButton"] = false };
        DialogService.Open<MessageModalWindow>("Evaluation Results", properties, new DialogOptions { Height = "60vh", Width = "55vw" });
        _isBusy = false;
        StateHasChanged();
    }
    private async void SelectedStepChanged(int step)
    {
        _currentStep = step;
        if (step == 1 && AnalysisSteps.Count == 0)
        {
            await ExecuteMetaPrompt();
        }
        StateHasChanged();
    }

    private bool _showCarosel = true;
    private async Task ExecuteMetaPrompt()
    {

        AnalysisSteps.Clear();
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        var steps = await MetaPromptService.RunFullMetaPromptStack(AppState.ActiveSystemPrompt);
        await File.WriteAllTextAsync("MetaPromptSteps.json", JsonSerializer.Serialize(steps));
        _selectedIndex = 0;
        StateHasChanged();
        await Task.Delay(1);
        AnalysisSteps = steps;
        _selectedIndex = AnalysisSteps.Count - 1;
        StateHasChanged();
    }

    private async void UpdatePrompt(PromptForm form)
    {

        //AppState.ActiveSystemPromptHtml = form.Prompt;
        _currentStep = 1;
        await ExecuteMetaPrompt();
        StateHasChanged();
    }
    private async void UpdateCreatePrompt(CreatePromptForm form)
    {
        _currentStep = 1;
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        var steps = await MetaPromptService.RunFullMetaPromptStack(form.Task, true, form.InputVariables);
        await File.WriteAllTextAsync("MetaPromptSteps.json", JsonSerializer.Serialize(steps));
        _selectedIndex = 0;
        StateHasChanged();
        AnalysisSteps = steps;
        _selectedIndex = AnalysisSteps.Count - 1;
        StateHasChanged();
    }
    private void Cancel()
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        _isBusy = false;
        StateHasChanged();
    }

    protected override async void UpdateState(object? sender, PropertyChangedEventArgs args)
    {
        Logger.LogInformation("UpdateState triggered in {Home} from property {propName}", nameof(ImprovePromptPage), args.PropertyName);
        switch (args.PropertyName)
        {
            case nameof(AppState.ActiveSystemPromptHtml):
                //_systemPromptForm.SystemPromptHtml = AppState.ActiveSystemPromptHtml;
                AppState.ChatSettings.SystemPrompt = AppState.ActiveSystemPrompt;
                _promptForm.Prompt = AppState.ActiveSystemPromptHtml;
                break;
            case nameof(AppState.PromptToSave):
                {
                    //var parameters = new Dictionary<string, object> { ["MessageText"] = _text };
                    //var dialogOptions = new DialogOptions { CloseDialogOnOverlayClick = true, Height = "40vh", Width = "40vw", Resizable = true, Draggable = true };
                    //await DialogService.OpenAsync<MessageModalWindow>("Response from Prompt Engineer Agent", parameters, dialogOptions);
                    Logger.LogInformation("Save Prompt dialog triggered");
                    var properties = new Dictionary<string, object> { ["MessageText"] = AppState.PromptToSave, ["ShowConfirmButton"] = true };
                    var save = await DialogService.OpenAsync<MessageModalWindow>("Save Prompt", properties, new DialogOptions { Height = "80vh", Width = "65vw" });
                    Logger.LogInformation("Save Prompt dialog result: {saveResult}", ((object)save)?.ToString());
                    if (save is bool && save == true)
                    {
                        AppState.ActiveSystemPromptHtml = AsHtml(AppState.PromptToSave);
                    }

                    break;
                }
        }

        base.UpdateState(sender, args);
    }

    protected override void Dispose(bool disposing)
    {
        MetaPromptService.LogEntryUpdate -= HandleLogEntryUpdate;
        MetaPromptService.NewStep -= HandleNewStep;
        _ = _joditEditor?.DisposeAsync();
        base.Dispose(disposing);
    }
}
