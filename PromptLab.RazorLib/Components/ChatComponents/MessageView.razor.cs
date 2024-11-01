using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel;
using PromptLab.RazorLib.ChatModels;
using AppJsInterop = PromptLab.RazorLib.Shared.AppJsInterop;

namespace PromptLab.RazorLib.Components.ChatComponents;

public partial class MessageView
{
	[Parameter]
	[EditorRequired]
	public Message Message { get; set; } = default!;
	private string _previousContent = "";
	[CascadingParameter(Name = "Timestamps")]
	public bool Timestamps { get; set; }
	[Parameter]
	public EventCallback<Message> OnRemove { get; set; }
	[Parameter]
	public EventCallback<Message> OnContentUpdate { get; set; }
	[CascadingParameter(Name = "AllowRemove")]
	public bool AllowRemove { get; set; }
	[Inject] private IJSRuntime JsRuntime { get; set; } = default!;

	private bool _shouldRender;
	private ElementReference _ref;
	private bool _isModify;
	private class ModifyForm
	{
		public string Content { get; set; } = "";
	}

	private ModifyForm _modifyForm = new();
	protected override List<string> InterestingProperties => [nameof(AppState.ShowTimestamps)];
	protected override void UpdateState(object? sender, PropertyChangedEventArgs args)
	{
		_shouldRender = true;
		base.UpdateState(sender, args);
	}
	protected override bool ShouldRender()
	{
		return _shouldRender;
	}
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
		OnContentUpdate.InvokeAsync(Message);
		StateHasChanged();
	}
	protected override Task OnParametersSetAsync()
	{
		//Console.WriteLine($"ALlow remove = {AllowRemove}");
		if (Message.Content != _previousContent)
		{
			_previousContent = Message.Content ?? "";
			_shouldRender = true;
		}
		if (Message.IsActiveStreaming || string.IsNullOrEmpty(Message.Content))
		{
			_shouldRender = true;
		}
		return base.OnParametersSetAsync();
	}
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (!Message.IsActiveStreaming)
		{
			_shouldRender = false;
			var appJsInterop = new AppJsInterop(JsRuntime);
			await appJsInterop.AddCodeStyle(_ref);
		}
		await base.OnAfterRenderAsync(firstRender);
	}
	private string AsHtml(string? text)
	{
		if (text == null) return "";
		var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
		var result = Markdown.ToHtml(text, pipeline);
		return result;

	}
}