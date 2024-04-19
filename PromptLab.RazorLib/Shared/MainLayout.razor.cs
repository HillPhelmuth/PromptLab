using Microsoft.AspNetCore.Components;
using Radzen;

namespace PromptLab.RazorLib.Shared;

public partial class MainLayout
{
    private bool sidebarExpanded = false;
    private bool rightSidebarExpanded = false;
    [Inject]
    private DialogService DialogService { get; set; } = default!;

    private void CloseSidebars()
    {
        sidebarExpanded = false;
        rightSidebarExpanded = false;
        StateHasChanged();
    }
}