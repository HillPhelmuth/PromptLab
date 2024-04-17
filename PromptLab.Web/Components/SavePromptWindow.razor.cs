using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PromptLab.Web.Components
{
    public partial class SavePromptWindow : ComponentBase
    {
        [Inject]
        private DialogService DialogService { get; set; } = default!;

        private class SavePromptForm
        {
            public string? Name { get; set; }
        }
        private SavePromptForm _savePromptForm = new();
        private void Submit(SavePromptForm savePromptForm)
        {
            DialogService.Close(savePromptForm.Name);
        }
        private void Cancel()
        {
            DialogService.Close();
        }
    }
}
