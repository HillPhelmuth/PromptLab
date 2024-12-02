using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorJoditEditor;

public class JoditEditorInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;
    public event Action<string>? ContentChanged;
    public event Action<string>? EventInvoked;
    private DotNetObjectReference<JoditEditorInterop> dotNetHelper;
    public JoditEditorInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/BlazorJoditEditor/jodit-js-module.js").AsTask());
    }
    public async ValueTask InitJodit(ElementReference elementReference, string? content)
    {
        var module = await moduleTask.Value;
        // Create a DotNet reference to pass back to JavaScript
        dotNetHelper = DotNetObjectReference.Create(this);

        // Initialize the Jodit editor
        await module.InvokeVoidAsync("initializeJoditEditor",
            elementReference,
            content ?? string.Empty,
            dotNetHelper);
    }
    
    public async ValueTask SetContentAsync(string content, ElementReference editorElement)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("setJoditContent", editorElement, content);
    }

    public async ValueTask<string> GetContentAsync(ElementReference editorElement)
    {
        var module = await moduleTask.Value;
        return await module.InvokeAsync<string>("getJoditContent", editorElement);
        //return string.Empty;
    }

    [JSInvokable]
    public void OnContentChanged(string content)
    {
        ContentChanged?.Invoke(content);
    }
    [JSInvokable]
    public void OnEventInvoked(string eventName)
    {
        EventInvoked?.Invoke(eventName);
    }


    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
        dotNetHelper?.Dispose();
    }
}