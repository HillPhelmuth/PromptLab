using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PromptLab.Core.Models;

namespace PromptLab.RazorLib.Shared
{
    public class AppJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./appJsInterop.js").AsTask());

        public async ValueTask ScrollDown(ElementReference elementReference)
        {
            try
            {
                var module = await _moduleTask.Value;
                await module.InvokeVoidAsync("scrollDown", elementReference);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on ScrollDown: {ex.Message}");
            }
        }

        public async ValueTask AddCodeStyle(ElementReference element)
        {
            try
            {
                var module = await _moduleTask.Value;
                await module.InvokeVoidAsync("addCodeStyle", element);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on AddCodeStyle: {ex.Message}");
            }
        }
        public async ValueTask SetAppTheme(StyleTheme styleTheme)
        {
            try
            {
                var module = await _moduleTask.Value;
                await module.InvokeVoidAsync("setTheme", styleTheme.ToString().ToLower());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on SetAppTheme: {ex.Message}");
            }
        }
        public async ValueTask ShowJsonViewer(ElementReference element, object jsonObj)
        {
            try
            {
                await (await _moduleTask.Value).InvokeVoidAsync("showJsonViewer", element, jsonObj);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on ShowJsonViewer: {ex.Message}");
            }
        }
        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_moduleTask.IsValueCreated)
                {
                    var module = await _moduleTask.Value;
                    await module.DisposeAsync();
                }
            }
            catch (JSDisconnectedException ex)
            {
                Console.WriteLine($"Error on DisposeAsync: {ex.Message}");
            }
        }
    }
}