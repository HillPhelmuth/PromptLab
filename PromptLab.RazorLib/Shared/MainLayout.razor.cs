using Microsoft.AspNetCore.Components;
using PromptLab.Core;
using PromptLab.Core.Models;
using PromptLab.Core.Services;
using Radzen;

namespace PromptLab.RazorLib.Shared;

public partial class MainLayout
{
    private bool sidebarExpanded = false;
    private bool rightSidebarExpanded = false;
    [Inject]
    private DialogService DialogService { get; set; } = default!;
    [Inject]
    private AppState AppState { get; set; } = default!;
    [Inject]
    private IFileService FileService { get; set; } = default!;
    [Inject]
    private AppJsInterop AppJsInterop { get; set; } = default!;
	private string _title = "Prompt Lab Playground";

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
        if (firstRender)
        {
            var userSettings = await FileService.LoadUserSettings();

            AppState.ChatSettings = userSettings.ChatSettings;
            AppState.IsLogProbView = userSettings.ChatSettings.LogProbs;
            AppState.AppSettings = userSettings.AppSettings;
            AppState.ShowTimestamps = userSettings.AppSettings.ShowTimestamps;
            if (userSettings.AppSettings.Theme != StyleTheme.Standard)
                await AppJsInterop.SetAppTheme(userSettings.AppSettings.Theme);
			AppState.ModelSettings = userSettings.ModelSettings;
        }
		await base.OnAfterRenderAsync(firstRender);
	}
	private void SetHeaderTitle(string title)
    {
        _title = title;
    }
    private void CloseSidebars()
    {
        sidebarExpanded = false;
        rightSidebarExpanded = false;
        StateHasChanged();
    }
}