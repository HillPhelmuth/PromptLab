using Markdig;
using Microsoft.AspNetCore.Components;
using PromptLab.RazorLib.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
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
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;
    [Parameter]
    public bool HideClose { get; set; }
    private ElementReference _ref;
    private ElementReference _markdownReference;
    protected override async Task OnParametersSetAsync()
    {
        if (Log?.DisplayType == DisplayType.Json)
        {
           
        }
        await base.OnParametersSetAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Log is { IsComplete: true, DisplayType: DisplayType.Markdown })
        {
            await JsRuntime.InvokeVoidAsync("addCopyButtons", _markdownReference);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private object GetObjectFromContent()
    {
        try
        {
            return JsonSerializer.Deserialize<object>(Log?.Content.Trim('\n'));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            var content = Log?.Content.Replace("```json","").Replace("```","").Trim('\n');
            var asObject = new {LogContent = content};
            var json = JsonSerializer.Serialize(asObject);
            return JsonSerializer.Deserialize<object>(json)!;
        }
        
    }
    private void Close()
    {
        DialogService.CloseSide();
    }
    
}