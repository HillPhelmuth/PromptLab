using Microsoft.AspNetCore.Components;
using PromptLab.Core;
using PromptLab.Core.Models;
using PromptLab.Core.Services;
using Radzen;

namespace PromptLab.RazorLib.Shared;

public partial class MainLayout
{
    private bool _sidebarExpanded;
    [Inject]
    private DialogService DialogService { get; set; } = default!;
    [Inject]
    private AppState AppState { get; set; } = default!;
    [Inject]
    private IFileService FileService { get; set; } = default!;
    [Inject]
    private AppJsInterop AppJsInterop { get; set; } = default!;

    protected override Task OnInitializedAsync()
    {
        AppState.PropertyChanged += UpdateState;
        return base.OnInitializedAsync();
    }

    private void UpdateState(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(AppState.CurrentPageTitle)) return;
        SetHeaderTitle(AppState.CurrentPageTitle);
        StateHasChanged();

    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
	{
        if (firstRender)
        {
            var userSettings = await FileService.LoadUserSettings();

            AppState.UserProfile = userSettings;
            FileService.ZoomChanged(userSettings.AppSettings.ZoomFactor);
            //AppState.ChatModelSettings = userSettings.ModelSettings;
            //AppState.ChatSettings = userSettings.ChatSettings;
            AppState.IsLogProbView = userSettings.ChatSettings.LogProbs;
            //AppState.EmbeddingModelSettings = userSettings.EmbeddingModelSettings;
            //AppState.AppSettings = userSettings.AppSettings;
            AppState.ShowTimestamps = userSettings.AppSettings.ShowTimestamps;
            if (userSettings.AppSettings.Theme != StyleTheme.Standard)
                await AppJsInterop.SetAppTheme(userSettings.AppSettings.Theme);
			
        }
		await base.OnAfterRenderAsync(firstRender);
	}
	private void SetHeaderTitle(string title)
    {
        AppState.CurrentPageTitle = title;
    }
    private void CloseSidebars()
    {
        _sidebarExpanded = false;
        StateHasChanged();
    }
}