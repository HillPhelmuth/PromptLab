using Microsoft.AspNetCore.Components;
using PromptLab.Core.Models;
using System.ComponentModel;

namespace PromptLab.RazorLib.Components.MenuComponents;

public partial class ModelSettingsMenu
{
	[Parameter]
	public ModelSettings? ModelSettings { get; set; }
	[Parameter]
	public EventCallback<ModelSettings> ModelSettingsChanged { get; set; }

	private ModelSettings _modelSettings = new();
	protected override void OnParametersSet()
	{
		ModelSettings ??= AppState.ModelSettings;
		_modelSettings = AppState.ModelSettings;
		StateHasChanged();
		base.OnParametersSet();
	}
	protected override void UpdateState(object? sender, PropertyChangedEventArgs args)
	{
		if (args.PropertyName == nameof(AppState.ModelSettings))
		{
			_modelSettings = AppState.ModelSettings;
			StateHasChanged();
		}
		base.UpdateState(sender, args);
	}
	private async void Submit(ModelSettings modelSettings)
	{
		AppState.ModelSettings = modelSettings;
		await FileService.SaveUserSettings(AppState.UserProfile);
		ModelSettings = modelSettings;
		await ModelSettingsChanged.InvokeAsync(modelSettings);
	}
}