using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel;
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
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    private bool _shouldRender;
    private ElementReference _ref;
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

    protected override Task OnParametersSetAsync()
    {
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