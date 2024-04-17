using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdig;
using Microsoft.SemanticKernel;

namespace PromptLab.Core.Plugins;

public class SavePromptPlugin
{
    private readonly AppState _appState;
    public SavePromptPlugin(AppState appState)
    {
        _appState = appState;
    }
    [KernelFunction, Description("Save the precise text of the prompt")]
    public async Task<string> SavePrompt(Kernel kernel, [Description("The prompt to save")] string prompt)
    {

        _appState.PromptToSave = AsHtml(prompt);
        return "Prompt saved successfully!";
    }
    private string AsHtml(string? text)
    {
        if (text == null) return "";
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var result = Markdown.ToHtml(text, pipeline);
        return result;

    }


}