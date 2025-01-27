using PromptLab.Core.Models;
using Radzen.Blazor;

namespace PromptLab.RazorLib.Components.ModalWindows;
public partial class AddCustomPlugin
{
    private CustomPluginPath _customPluginPathForm = new();
    private RadzenDataGrid<CustomPluginPath>? _grid;
    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }
    private async void AddPluginPath(CustomPluginPath customPluginPathForm)
    {
        if (string.IsNullOrWhiteSpace(customPluginPathForm.Name) || string.IsNullOrWhiteSpace(customPluginPathForm.Path)) return;
        AppState.ChatSettings.OpenApiPluginNamePaths.Add(new CustomPluginPath() { Name = customPluginPathForm.Name, Path = customPluginPathForm.Path });
        AppState.ChatSettings.CustomPlugins = await AppState.ChatSettings.GetPluginsFromPaths(FileService);
        _customPluginPathForm = new CustomPluginPath();
        await _grid?.Reload();
        StateHasChanged();
    }
    private void RemovePluginPath(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        var pluginPath = AppState.ChatSettings.OpenApiPluginNamePaths.FirstOrDefault(p => p.Name == name);
        var plugin = AppState.ChatSettings.CustomPlugins.FirstOrDefault(p => p.Name == name);
        if (pluginPath != null) 
            AppState.ChatSettings.OpenApiPluginNamePaths.Remove(pluginPath);
        if (plugin is not null) 
            AppState.ChatSettings.CustomPlugins.Remove(plugin);
        _customPluginPathForm = new CustomPluginPath();
        StateHasChanged();
    }

    private async Task PickFile(string name)
    {
        _customPluginPathForm.Path = await FileService.GetFilePathAsync(".json", ".yaml");
        StateHasChanged();
    }
    private async Task Finish()
    {
        AppState.ChatSettings.CustomPlugins = await AppState.ChatSettings.GetPluginsFromPaths(FileService);
        DialogService.Close();
    }

}
