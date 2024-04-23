using Markdig;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PromptLab.RazorLib.Components.ModalWindows
{
    public partial class MessageModalWindow
    {
        [Parameter]
        public string MessageText { get; set; } = "";
        [Parameter]
        public bool ShowConfirmButton { get; set; } = false;
        [Inject]
        private DialogService DialogService { get; set; } = default!;
        private string AsHtml(string? text)
        {
            if (text == null) return "";
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var result = Markdown.ToHtml(text, pipeline);
            return result;

        }
        private void Confirm()
        {
            DialogService.Close(true);
        }
        private void Cancel()
        {
            DialogService.Close(false);
        }
    }
}
