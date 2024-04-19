using Microsoft.AspNetCore.Components;
using Radzen;

namespace PromptLab.RazorLib.Shared;

public partial class MainLayout
{
    private bool sidebarExpanded = false;
    private bool rightSidebarExpanded = false;
    [Inject]
    private DialogService DialogService { get; set; } = default!;
    private string _title = "Prompt Lab Playground";
    private void SetHeaderTitle(string title)
    {
        _title = title;
    }
    private void CloseSidebars()
    {
        sidebarExpanded = false;
        rightSidebarExpanded = false;
        StateHasChanged();
    }
}