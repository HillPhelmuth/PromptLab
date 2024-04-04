using Microsoft.AspNetCore.Components;
using PromptLab.Core;
using System.ComponentModel;

namespace PromptLab.RazorLib.Shared;

public class AppComponentBase : ComponentBase, IDisposable
{
    [Inject]
    protected AppState AppState { get; set; } = default!;

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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        AppState.PropertyChanged -= UpdateState;
    }
}