using Microsoft.AspNetCore.Components;
using PromptLab.Core.Models;
using Radzen;
using System.ComponentModel;

namespace PromptLab.RazorLib.Components.MenuComponents;

public partial class ModelSettingsMenu
{
	[Parameter]
	public ChatModelSettings? ChatModelSettings { get; set; }
	[Parameter]
	public EmbeddingModelSettings? EmbeddingModelSettings { get; set; }
	[Parameter]
	public EventCallback<ChatModelSettings> ModelSettingsChanged { get; set; }
	[Inject]
	private DialogService DialogService { get; set; } = default!;

	private ChatModelSettings _chatModelSettings= new();
	private EmbeddingModelSettings _embeddingsModelSettings = new();
	protected override void OnParametersSet()
	{
		ChatModelSettings ??= AppState.ChatModelSettings;
		_chatModelSettings = ChatModelSettings;
		EmbeddingModelSettings ??= AppState.EmbeddingModelSettings;
		_embeddingsModelSettings = EmbeddingModelSettings;
		StateHasChanged();
		base.OnParametersSet();
	}
	protected override void UpdateState(object? sender, PropertyChangedEventArgs args)
	{
		if (args.PropertyName == nameof(AppState.ChatModelSettings))
		{
			_chatModelSettings = AppState.ChatModelSettings;
			StateHasChanged();
		}
		base.UpdateState(sender, args);
	}
	private async void Submit(ChatModelSettings modelSettings)
	{
		AppState.ChatModelSettings = modelSettings;
		if (!string.IsNullOrEmpty(modelSettings.OpenAIApiKey))
		{
			var confirm = await DialogService.Confirm("Use same OpenAI ApiKey for Embeddings (text-embedding-3-small)?", "Confirm", new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });
			if (confirm == true)
			{
				_embeddingsModelSettings.OpenAIApiKey = modelSettings.OpenAIApiKey;
				EmbeddingModelSettings!.OpenAIApiKey = modelSettings.OpenAIApiKey;
				AppState.EmbeddingModelSettings.OpenAIApiKey = modelSettings.OpenAIApiKey;
			}
		}
		await FileService.SaveUserSettings(AppState.UserProfile);
		ChatModelSettings = modelSettings;
		await ModelSettingsChanged.InvokeAsync(modelSettings);
	}
	private async void EmbeddingSubmit(EmbeddingModelSettings settings)
	{
		AppState.EmbeddingModelSettings = settings;

		await FileService.SaveUserSettings(AppState.UserProfile);
		EmbeddingModelSettings = settings;

	}
}