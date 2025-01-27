using Microsoft.AspNetCore.Components;
using PromptLab.Core.Models;
using PromptLab.Core.Services;
using System.ComponentModel;
using System.Text;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using PromptLab.RazorLib.Components.ModalWindows;

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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            AppState.ChatSettings.CustomPlugins = await AppState.ChatSettings.GetPluginsFromPaths(FileService);
            StateHasChanged();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task OpenPluginFile()
    {
       var item = await DialogService.OpenAsync<AddCustomPlugin>("Add Custom Plugin");
       StateHasChanged();
        //var content = await FileService.OpenFileTextAsync(".yaml", ".json");
        //if (content != null)
        //{
        //    _chatSettings.OpenApiPluginFileContent = content;
        //    var asByteArray = Encoding.UTF8.GetBytes(content);
        //    var fileStream = new MemoryStream(asByteArray);
        //    var plugin = await OpenApiKernelPluginFactory.CreateFromOpenApiAsync("CustomPlugin", fileStream);
        //    _chatSettings.CustomPlugins = plugin;
        //}
    }
	private async void Submit(ChatSettings chatSettings)
    {
        //if (!string.IsNullOrWhiteSpace(chatSettings.OpenApiPluginFileContent))
        //{
        //    var asByteArray = Encoding.UTF8.GetBytes(chatSettings.OpenApiPluginFileContent);
        //    var fileStream = new MemoryStream(asByteArray);
        //    var plugin = await OpenApiKernelPluginFactory.CreateFromOpenApiAsync("CustomPlugin", fileStream);
        //    chatSettings.CustomPlugins = plugin;
        //}
        AppState.ChatSettings = chatSettings;
        
        AppState.IsLogProbView = chatSettings.LogProbs && chatSettings.Model.StartsWith("gpt");
        await FileService.SaveUserSettings(AppState.UserProfile);
        ChatSettings = chatSettings;
        await ChatSettingsChanged.InvokeAsync(chatSettings);
    }
}