using Markdig;
using Microsoft.AspNetCore.Components;
using PromptLab.RazorLib.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PromptLab.Core.Models;
using Radzen;

namespace PromptLab.RazorLib.Components.LogViewer;

public partial class LogItem
{
    [Parameter]
    public LogEntry? Log { get; set; }
    [Inject]
    private AppJsInterop AppJsInterop { get; set; } = default!;
    [Inject]
    private DialogService DialogService { get; set; } = default!;
    private ElementReference _ref;
    protected override async Task OnParametersSetAsync()
    {
        if (Log?.DisplayType == DisplayType.Json)
        {
            //await AppJsInterop.ShowJsonViewer(_ref, JsonSerializer.Deserialize<object>(Log?.Content));
        }
        await base.OnParametersSetAsync();
    }
    private void Close()
    {
        DialogService.CloseSide();
    }
    private string AsHtml(string? text)
    {
        try
        {
            if (text == null) return "";
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var result = Markdown.ToHtml(text, pipeline);
            return result;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error converting markdown to HTML");
            return text;
        }

    }
}