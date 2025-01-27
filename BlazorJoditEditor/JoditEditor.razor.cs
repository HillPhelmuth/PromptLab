using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ReverseMarkdown;

namespace BlazorJoditEditor;
public partial class JoditEditor
{
    private ElementReference editorElement;
    private IJSObjectReference? module;
    private DotNetObjectReference<JoditEditor>? dotNetHelper;
    private MarkdownPipeline _markdownPipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;
    [Inject]
    private JoditEditorInterop JoditEditorInterop { get; set; } = default!;
    [Parameter]
    public string ElementId { get; set; } = Guid.NewGuid().ToString();

    [Parameter]
    public string? HtmlContent { get; set; }

    [Parameter]
    public EventCallback<string> HtmlContentChanged { get; set; }
    [Parameter]
    public string? MarkdownContent { get; set; }
    [Parameter]
    public EventCallback<string> MarkdownContentChanged { get; set; }
    [Parameter]
    public EventCallback<string> EventInvoked { get; set; }
    [Parameter]
    [EditorRequired]
    public JoditEditorOptions Options { get; set; } = new();

    protected override Task OnInitializedAsync()
    {
        JoditEditorInterop.ContentChanged += OnContentChanged;
        JoditEditorInterop.EventInvoked += OnEventInvoked;
        return base.OnInitializedAsync();
    }

    private void OnEventInvoked(string obj)
    {
        EventInvoked.InvokeAsync(obj);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Load the JavaScript module for Jodit integration
            await JoditEditorInterop.InitJodit(editorElement, HtmlContent, Options);
            
        }
    }


    public async Task SetContentAsync(string content)
    {
        await JoditEditorInterop.SetContentAsync(content, editorElement);
        //if (module is not null)
        //{
        //    await module.InvokeVoidAsync("setJoditContent", editorElement, content);
        //}
    }

    public async Task<string> GetContentAsMarkdownAsync()
    {
        var content = await JoditEditorInterop.GetContentAsync(editorElement);
        
        var result = ContentAsMarkdown(content);
        return result;
        //if (module is not null)
        //{
        //    return await module.InvokeAsync<string>("getJoditContent", editorElement);
        //}
        //return string.Empty;
    }
    public async Task<string> GetContentAsync()
    {
        var content = await JoditEditorInterop.GetContentAsync(editorElement);
        return content;
        
    }

    public async void OnContentChanged(string content)
    {
        //var result = ContentAsMarkdown(content);
        await HtmlContentChanged.InvokeAsync(content);
    }
    public string ContentAsMarkdown(string input)
    {
        var config = new Config
        {
            // Include the unknown tag completely in the result (default as well)
            UnknownTags = Config.UnknownTagsOption.PassThrough,
            // generate GitHub flavoured markdown, supported for BR, PRE and table tags
            GithubFlavored = true,
            // will ignore all comments
            RemoveComments = true,
            // remove markdown output for links where appropriate
            SmartHrefHandling = true
        };

        var converter = new Converter(config);
        return converter.Convert(input);
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
            return text!;
        }

    }
    public async ValueTask DisposeAsync()
    {
        if (module is not null)
        {
            await JoditEditorInterop.DisposeAsync();
        }
        dotNetHelper?.Dispose();
    }
}
