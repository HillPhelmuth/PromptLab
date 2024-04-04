using Microsoft.AspNetCore.Components;
using PromptLab.Core.Helpers;
using PromptLab.Core.Services;
using PromptLab.RazorLib.Components.ChatComponents;
using ReverseMarkdown;
using System.Diagnostics;

namespace PromptLab.RazorLib.Pages;

public partial class Home
{
    private ChatView _chatView;
    private bool _isBusy;
    [Inject]
    private FilePickerService FilePickerService { get; set; } = default!;
    [Inject]
    private ChatService ChatService { get; set; } = default!;
    private class SystemPromptForm
    {
        public string SystemPromptHtml { get; set; } =
            """
            <h2>Instructions</h2>
            <p>You are a helpful AI assistant.</p>
            """;
    }
    private SystemPromptForm _systemPromptForm = new();
    protected override List<string> InterestingProperties => [nameof(AppState.IsLogProbView), nameof(AppState.ShowTimestamps)];
    private async Task PickFile()
    {
        var item = FilePickerService.PickFileAsync();
    }
    private void ClearChat()
    {
        _chatView.ChatState!.Reset();
    }
    private void SubmitSystemPrompt(SystemPromptForm systemPromptForm)
    {
        var config = new Config
        {
            // Include the unknown tag completely in the result (default as well)
            UnknownTags = Config.UnknownTagsOption.PassThrough,
            // generate GitHub flavoured markdown, supported for BR, PRE and table tags
            GithubFlavored = true,
            // will ignore all comments
            RemoveComments = true,
            // remove markdown output for links where appropriate
            SmartHrefHandling = true
        };

        var converter = new Converter(config);
        var systemPromptAsMarkdown = converter.Convert(systemPromptForm.SystemPromptHtml);
        Debug.WriteLine(systemPromptAsMarkdown);
        AppState.ChatSettings.SystemPrompt = systemPromptAsMarkdown;
    }
    private async void HandleInput(string input)
    {
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        _chatView.ChatState!.AddUserMessage(input);
        if (AppState.ChatSettings is {LogProbs: true, Streaming: false})
        {
            var choice = await ChatService.GetLogProbs(_chatView.ChatState!.ChatHistory,
                AppState.ChatSettings.Temperature.GetValueOrDefault(1.0f),
                AppState.ChatSettings.TopP.GetValueOrDefault(1.0f));
            var content = choice.Message.Content;
            var tokenStrings = choice.LogProbabilityInfo.TokenLogProbabilityResults.ToList().AsTokenStrings();
            AppState.TokenStrings = tokenStrings;
            _chatView.ChatState!.AddAssistantMessage(content, tokenStrings: tokenStrings);
            _isBusy = false;
            StateHasChanged();
        }
        else
        {
            var settings = AppState.ChatSettings.AsPromptExecutionSettings();
            var chatSequence = ChatService.StreamingChatResponse(_chatView.ChatState!.ChatHistory, settings,
                               AppState.ChatSettings.SystemPrompt, AppState.ChatSettings.Model);
            await ExecuteChatSequence(chatSequence);
            _isBusy = false;
            StateHasChanged();
        }
    }
    private async Task ExecuteChatSequence(IAsyncEnumerable<string> chatseq)
    {
        var hasStarted = false;
        await foreach (var response in chatseq)
        {
            if (!hasStarted)
            {
                hasStarted = true;
                _chatView!.ChatState!.AddAssistantMessage(response);
                _chatView!.ChatState!.ChatMessages.LastOrDefault(x => x.Role == Role.Assistant)!
                    .IsActiveStreaming = true;
                continue;
            }

            _chatView!.ChatState!.UpdateAssistantMessage(response);
        }

        var lastAsstMessage =
            _chatView!.ChatState!.ChatMessages.LastOrDefault(x => x.Role == Role.Assistant);
        if (lastAsstMessage is not null)
            lastAsstMessage.IsActiveStreaming = false;
    }
}