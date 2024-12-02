using Microsoft.AspNetCore.Components;
using PromptLab.Core;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using PromptLab.Core.Services;
using Radzen;
using Markdig;

namespace PromptLab.RazorLib.Shared;

public class AppComponentBase : ComponentBase, IDisposable
{
    private MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    [Inject]
    protected AppState AppState { get; set; } = default!;
    [Inject]
    private ILoggerFactory LoggerFactory { get; set; } = default!;
    [Inject]
    protected IFileService FileService { get; set; } = default!;
    [Inject]
    protected NotificationService NotificationService { get; set; } = default!;
    [Inject]
    protected DialogService DialogService { get; set; } = default!;
    protected ILogger Logger => LoggerFactory.CreateLogger(GetType());

    protected override Task OnInitializedAsync()
    {
        AppState.PropertyChanged += UpdateState;
        return base.OnInitializedAsync();
    }
    protected virtual List<string> InterestingProperties => [];
    protected virtual void UpdateState(object? sender, PropertyChangedEventArgs args)
    {
        if (!InterestingProperties.Contains(args.PropertyName!)) return;
        InvokeAsync(StateHasChanged);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            AppState.PropertyChanged -= UpdateState;
        }
    }
    protected string AsHtml(string? text)
    {
        try
        {
            if (text == null) return "";
            var result = Markdown.ToHtml(text, _markdownPipeline);
            return result;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error converting markdown to html");
            return text!;
        }

    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}