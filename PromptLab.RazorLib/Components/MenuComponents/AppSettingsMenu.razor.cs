using Microsoft.AspNetCore.Components;
using PromptLab.Core.Models;
using PromptLab.Core.Services;
using PromptLab.RazorLib.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PromptLab.RazorLib.Components.MenuComponents;

public partial class AppSettingsMenu
{
    [Inject]
    private AppJsInterop AppJsInterop { get; set; } = default!;
    [Inject]
    private IFileService FilePickerService { get; set; } = default!;
    private AppSettings _appSettings = new();
    protected override void OnInitialized()
    {
        _appSettings = AppState.AppSettings;
    }
    protected override void UpdateState(object? sender, PropertyChangedEventArgs args)
    {
	    if (args.PropertyName == nameof(AppState.AppSettings))
	    {
		    _appSettings = AppState.AppSettings;
		    StateHasChanged();
	    }
	    base.UpdateState(sender, args);
    }
	private async void Submit(AppSettings appSettings)
    {
        AppState.AppSettings = appSettings;
        await FilePickerService.SaveUserSettings(AppState.UserProfile);
        AppState.ShowTimestamps = appSettings.ShowTimestamps;
        FilePickerService.ZoomChanged(appSettings.ZoomFactor);
        await AppJsInterop.SetAppTheme(appSettings.Theme);
    }
}