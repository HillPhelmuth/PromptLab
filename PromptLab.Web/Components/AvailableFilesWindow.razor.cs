using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PromptLab.Web.Components
{
    public partial class AvailableFilesWindow : ComponentBase
    {
        [Parameter]
        public List<string> AvailableFiles { get; set; } = [];
        [Inject]
        private DialogService DialogService { get; set; } = default!;

        private string? _selectedFile;
        private void SelectFile()
        {
            if (!string.IsNullOrEmpty(_selectedFile))
            {
                DialogService.Close(_selectedFile);
            }
        }
        private void Cancel()
        {
            DialogService.Close();
        }
    }
}
