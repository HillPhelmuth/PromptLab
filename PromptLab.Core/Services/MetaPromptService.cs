using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using PromptLab.Core.Helpers;
using PromptLab.Core.Models;
using PromptLab.Core.Plugins;

namespace PromptLab.Core.Services;


public class MetaPromptService(AppState appState)
{
    public event Action<bool>? NewStep;

    public event Action<LogEntry>? LogEntryUpdate;
   

    public async Task<List<LogEntry>> RunFullMetaPromptStack(string prompt, bool isCreatePrompt = false,
        string promptVariables = "", string modifyInstructions = "", CancellationToken token = default)
    {
        var originalPrompt = "";
        var analysisSteps = new List<LogEntry>();
        var kernel = ChatService.CreateKernel(appState.ChatSettings.Model);
        var plugin = kernel.ImportMetaPromptFunctions();
        var settings = ChatService.GetServicePromptExecutionSettings();
        if (isCreatePrompt)
        {
            var promptCreationLog = new LogEntry("", DisplayType.Markdown, "OriginalPrompt");
            var kernelArguments = new KernelArguments(settings) { ["task"] = prompt, ["inputVariables"] = promptVariables};
            NewStep?.Invoke(true);
            await foreach (var update in kernel.InvokeStreamingAsync(plugin["CreatePrompt"], kernelArguments, token))
            {
                LogEntryUpdate?.Invoke(new LogEntry(update.ToString(), DisplayType.Markdown, "OriginalPrompt"));
                promptCreationLog.Content += update.ToString();
                originalPrompt += update.ToString();
            }
            analysisSteps.Add(promptCreationLog);
            NewStep?.Invoke(true);
        }
        else
        {
            originalPrompt = prompt;
            NewStep?.Invoke(true);

            var ogPromptlogEntry = new LogEntry(originalPrompt, DisplayType.Markdown, "OriginalPrompt") { IsComplete = true };
            LogEntryUpdate?.Invoke(ogPromptlogEntry);
            analysisSteps.Add(ogPromptlogEntry);
            NewStep?.Invoke(true);
        }
        
        var analysisArgs = new KernelArguments(settings) { ["prompt"] = originalPrompt };
        var analysisResultItem = "";
        var eval = "";
        var analysisLog = new LogEntry("", DisplayType.Markdown, "AnalyzePrompt");
        await foreach (var update in kernel.InvokeStreamingAsync(plugin["AnalyzePrompt"], analysisArgs, token))
        {
            var analysisLogContent = update.ToString();
            var logEntry = new LogEntry(analysisLogContent, DisplayType.Markdown, "AnalyzePrompt");
            LogEntryUpdate?.Invoke(logEntry);
            analysisLog.Content += analysisLogContent;
            analysisResultItem += analysisLogContent;
		}
        analysisSteps.Add(analysisLog);
        NewStep?.Invoke(true);
        var evaluationArgs = new KernelArguments(settings) { ["prompt"] = originalPrompt };
        var evaluationLog = new LogEntry("", DisplayType.Json, "EvaluatePrompt");
        await foreach (var update in kernel.InvokeStreamingAsync(plugin["EvaluatePrompt"], evaluationArgs, token))
		{
			LogEntryUpdate?.Invoke(new LogEntry(update.ToString(), DisplayType.Json, "EvaluatePrompt"));
            evaluationLog.Content += update.ToString();
			eval += update.ToString();
		}
        analysisSteps.Add(evaluationLog);
		NewStep?.Invoke(true);
		var draftArgs = new KernelArguments(settings)
		{
			["prompt"] = originalPrompt,
			["analysis"] = analysisResultItem,
			["evaluation"] = eval
		};
		var draft = "";
        var draftLog = new LogEntry("", DisplayType.Markdown, "WriteDraftPrompt");
        await foreach (var update in kernel.InvokeStreamingAsync(plugin["WriteDraftPrompt"], draftArgs, token))
		{
			LogEntryUpdate?.Invoke(new LogEntry(update.ToString(), DisplayType.Markdown, "WriteDraftPrompt"));
            draftLog.Content += update.ToString();
            draft += update.ToString();
		}
        LogEntryUpdate?.Invoke(new LogEntry("", DisplayType.Markdown) { IsComplete = true });
        analysisSteps.Add(draftLog);
        NewStep?.Invoke(true);
		var draftEvalArgs = new KernelArguments(settings)
		{
			["originalPrompt"] = originalPrompt,
			["draftPrompt"] = draft,
			["analysis"] = analysisResultItem,
            ["instructions"] = modifyInstructions
        };
        var draftEval = "";
        var draftEvalLog = new LogEntry("", DisplayType.Markdown, "EvaluateDraftPrompt");
        await foreach (var update in kernel.InvokeStreamingAsync(plugin["EvaluateDraftPrompt"], draftEvalArgs, token))
		{
			LogEntryUpdate?.Invoke(new LogEntry(update.ToString(), DisplayType.Markdown, "EvaluateDraftPrompt"));
            draftEvalLog.Content += update.ToString();
            draftEval += update.ToString();
		}
        analysisSteps.Add(draftEvalLog);
        NewStep?.Invoke(true);
		var finalArgs = new KernelArguments(settings)
		{
			["originalPrompt"] = originalPrompt,
			["draftPrompt"] = draft,
			["evaluation"] = draftEval,
            ["analysis"] = analysisResultItem,
            ["instructions"] = modifyInstructions
        };
        var finalLog = new LogEntry("", DisplayType.Markdown, "WriteFinalPrompt");
        await foreach (var update in kernel.InvokeStreamingAsync(plugin["WriteFinalPrompt"], finalArgs, token))
		{
			LogEntryUpdate?.Invoke(new LogEntry(update.ToString(), DisplayType.Markdown, "WriteFinalPrompt"));
            finalLog.Content += update.ToString();
        }

        LogEntryUpdate?.Invoke(new LogEntry("", DisplayType.Markdown) { IsComplete = true });
        analysisSteps.Add(finalLog);
        NewStep?.Invoke(false);
        foreach (var step in analysisSteps)
        {
            step.Content = step.DisplayType == DisplayType.Json ? step.Content.Replace("```json","").Replace("```","").Trim('\n'): step.Content;
            step.IsComplete = true;
        }
        return analysisSteps;
        
    }
}