using Markdig;
using Microsoft.AspNetCore.Components;
using PromptLab.Core.Services;
using Radzen.Blazor;

namespace PromptLab.RazorLib.Components.ModalWindows;

public partial class EvalDisplay
{
    [Parameter]
    public List<EvalResultDisplay> EvalResultDisplays { get; set; } = [];
    [Parameter]
    public Dictionary<string, double> AggregatedResults { get; set; } = [];
    [Parameter]
    public string Header { get; set; } = "";
    private RadzenDataGrid<EvalResultDisplay>? _grid;
    private string AsHtml(string? text)
    {
        if (text == null) return "";
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var result = Markdown.ToHtml(text, pipeline);
        return result;

    }
    public Task RefreshGrid()
    {
        return _grid?.Reload()!;
    }
}