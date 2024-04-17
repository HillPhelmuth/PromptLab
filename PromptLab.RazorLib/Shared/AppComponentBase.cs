using Microsoft.AspNetCore.Components;
using PromptLab.Core;
using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace PromptLab.RazorLib.Shared;

public class AppComponentBase : ComponentBase, IDisposable
{
    [Inject]
    protected AppState AppState { get; set; } = default!;
    [Inject]
    private ILoggerFactory LoggerFactory { get; set; } = default!;
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

    public void Dispose()
    {
	    Dispose(true);
	    GC.SuppressFinalize(this);
    }
}