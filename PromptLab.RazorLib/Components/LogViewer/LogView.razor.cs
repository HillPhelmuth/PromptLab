using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using PromptLab.Core.Models;
using Radzen;

namespace PromptLab.RazorLib.Components.LogViewer;

public partial class LogView
{
    [Parameter]
    public List<LogEntry> Logs { get; set; } = [];
    [Inject]
    private DialogService DialogService { get; set; } = default!;

    private async void OpenLogItem(LogEntry log)
    {
        await DialogService.OpenSideAsync<LogItem>("Log Details", new Dictionary<string, object> { ["Log"] = log }, new SideDialogOptions { Position = DialogPosition.Right, Style = "top:0", CloseDialogOnOverlayClick = true, ShowClose = false, ShowTitle = false });
	}
}