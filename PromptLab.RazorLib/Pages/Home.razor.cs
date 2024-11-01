using Microsoft.AspNetCore.Components;
using PromptLab.Core.Helpers;
using PromptLab.Core.Services;
using PromptLab.RazorLib.Components.ChatComponents;
using ReverseMarkdown;
using System.Diagnostics;
using Markdig;
using PromptLab.RazorLib.ChatModels;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Radzen;
using PromptLab.RazorLib.Components.ModalWindows;
using PromptLab.RazorLib.Components.MenuComponents;
using Microsoft.SemanticKernel.ChatCompletion;
using PromptLab.Core.Models;
using System.Text;
using System.IO.Compression;
using OpenAI.Chat;

namespace PromptLab.RazorLib.Pages;

public partial class Home
{
    private ChatView _chatView;
    private bool _isBusy;
    [Inject]
    private BlobService BlobService { get; set; } = default!;
    [Inject]
    private ChatService ChatService { get; set; } = default!;
    [Inject]
    private PromptEngineerService PromptEngineerService { get; set; } = default!;
    [Inject]
    private DialogService DialogService { get; set; } = default!;

    private CancellationTokenSource _cancellationTokenSource = new();
    private class SystemPromptForm
    {
        public string SystemPromptHtml { get; set; } =
            """
            <h2>Instructions</h2>
            <p>You are a helpful AI assistant.</p>
            """;
    }
    private SystemPromptForm _systemPromptForm = new();
    private string _savedPrompt = "";
    private string _text;
    private string? _usageData;
    private string? _suggestedPrompt;
    protected override List<string> InterestingProperties => [nameof(AppState.IsLogProbView), nameof(AppState.ShowTimestamps), nameof(AppState.ActiveSystemPromptHtml)];
    protected override Task OnInitializedAsync()
    {
        _systemPromptForm.SystemPromptHtml = AppState.ActiveSystemPromptHtml;
        ChatService.OnChatTokenUsage += HandleChatTokenUsage;
        return base.OnInitializedAsync();
    }

    private async Task HandleImprovePrompt(string prompt)
    {
        var history = _chatView.ChatState.ChatHistory;
        history.AddSystemMessage(AppState.ActiveSystemPrompt);
        var token = _cancellationTokenSource.Token;
        _suggestedPrompt = await PromptEngineerService.EvaluateUserPrompt(prompt, history, token);
        StateHasChanged();
    }

    private void HandleChatTokenUsage(ChatTokenUsageArgs obj)
    {
        _usageData =
            $"""
			 **{nameof(obj.Usage.InputTokenCount)}:** {obj.Usage.InputTokenCount}, **{nameof(obj.Usage.OutputTokenCount)}:** {obj.Usage.OutputTokenCount}.

			 **Raw Response**:
			 ```json
			 {obj.RawResponseData}
			 ```
			 """;
        StateHasChanged();
    }

    protected override async void UpdateState(object? sender, PropertyChangedEventArgs args)
    {
        Logger.LogInformation("UpdateState triggered in {Home} from property {propName}", nameof(Home), args.PropertyName);
        switch (args.PropertyName)
        {
            case nameof(AppState.ActiveSystemPromptHtml):
                _systemPromptForm.SystemPromptHtml = AppState.ActiveSystemPromptHtml;
                AppState.ChatSettings.SystemPrompt = AppState.ActiveSystemPrompt;
                break;
            case nameof(AppState.PromptToSave):
                {
                    //var parameters = new Dictionary<string, object> { ["MessageText"] = _text };
                    //var dialogOptions = new DialogOptions { CloseDialogOnOverlayClick = true, Height = "40vh", Width = "40vw", Resizable = true, Draggable = true };
                    //await DialogService.OpenAsync<MessageModalWindow>("Response from Prompt Engineer Agent", parameters, dialogOptions);
                    Logger.LogInformation("Save Prompt dialog triggered");
                    var properties = new Dictionary<string, object> { ["MessageText"] = AppState.PromptToSave, ["ShowConfirmButton"] = true };
                    var save = await DialogService.OpenAsync<MessageModalWindow>("Save Prompt", properties, new DialogOptions { Height = "80vh", Width = "65vw" });
                    Logger.LogInformation("Save Prompt dialog result: {saveResult}", ((object)save)?.ToString());
                    if (save is bool && save == true)
                    {
                        AppState.ActiveSystemPromptHtml = AsHtml(AppState.PromptToSave);
                    }

                    break;
                }
        }

        base.UpdateState(sender, args);
    }
    private async Task PickFile()
    {
        var item = await FileService.OpenFileAsync();
        if (string.IsNullOrEmpty(item)) return;
        AppState.ActiveSystemPromptHtml = AsHtml(item);
    }
    private async Task ImprovePrompt()
    {
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        _text = await PromptEngineerService.HelpFromPromptEngineer(AppState.ActiveSystemPrompt);
        //var parameters = new Dictionary<string, object> { ["MessageText"] = _text };
        //var dialogOptions = new DialogOptions { CloseDialogOnOverlayClick = true, Height = "40vh", Width = "80vw", Resizable = true, Draggable = true };
        //DialogService.Open<MessageModalWindow>("Response from Prompt Engineer Agent", parameters, dialogOptions);
        _isBusy = false;
        StateHasChanged();
    }
    private async Task SavePrompt()
    {
        Logger.LogInformation("Saving system prompt");
        var item = await FileService.SaveFileAsync("system_prompt.md", AppState.ActiveSystemPrompt);
        _savedPrompt = item ?? "";
        StateHasChanged();
    }

    private async Task EvaluatePrompt()
    {
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        var result = await PromptEngineerService.EvaluatePrompt(AppState.ActiveSystemPrompt);
        var properties = new Dictionary<string, object> { ["MessageText"] = result.ToString(), ["ShowConfirmButton"] = false };
        DialogService.Open<MessageModalWindow>("Evaluation Results", properties, new DialogOptions { Height = "60vh", Width = "55vw" });
        _isBusy = false;
        StateHasChanged();
    }
    private void ClearChat()
    {
        _chatView.ChatState.Reset();
    }
    private async Task SaveChat()
    {
        var sb = new StringBuilder();
        foreach (var message in _chatView.ChatState.ChatMessages)
        {
            sb.AppendLine($"{message.Role}:");
            sb.AppendLine(message.Content);
        }
        _savedPrompt = await FileService.SaveFileAsync("chat_history.txt", sb.ToString()) ?? "";
    }
    private async Task AddMessage()
    {
        var dialogResult = await DialogService.OpenAsync<AddMessageWindow>("Add Message", options: new DialogOptions { Height = "40vh", Width = "40vw" });
        if (dialogResult is NewMessageForm newMessage)
        {
            switch (newMessage.Role)
            {
                case Role.User:
                    _chatView.ChatState.AddUserMessage(newMessage.Content!);
                    break;
                case Role.Assistant:
                    _chatView.ChatState.AddAssistantMessage(newMessage.Content!);
                    break;
            }
            StateHasChanged();
        }
    }
    private async void HandleExecute(HtmlEditorExecuteEventArgs args)
    {
        switch (args.CommandName)
        {
            case "Save":
                await SavePrompt();
                break;
            case "Load":
                await PickFile();
                break;
            case "Improve":
                await ImprovePrompt();
                break;
            case "Evaluate":
                await EvaluatePrompt();
                break;
        }
    }
    private void Cancel()
    {
        _cancellationTokenSource.Cancel();
    }
    private async void HandleInput(string input)
    {
        if (!await ValidateSettings()) return;


        if (!string.IsNullOrWhiteSpace(input))
            _chatView.ChatState.AddUserMessage(input);

        await SubmitRequest();
    }

    private async Task SubmitRequest()
    {
        _isBusy = true;
        StateHasChanged();
        await Task.Delay(1);
        _cancellationTokenSource = new();
        var token = _cancellationTokenSource.Token;
        if (AppState.ChatSettings is { LogProbs: true })
        {
            if (AppState.ChatSettings.Streaming)
            {
                var tokens = ChatService.GetLogProbsStreaming(_chatView.ChatState.ChatHistory, AppState.ActiveSystemPrompt, token);
                await ExecuteTokenChatSequence(tokens);
                _isBusy = false;
                StateHasChanged();
            }
            else
            {
                await ExecuteNonStreamingLogProbs(token);
            }
            //var tokens = ChatService.GetLogProbsStreaming(_chatView.ChatState.ChatHistory, AppState.ActiveSystemPrompt, token);
            //await ExecuteTokenChatSequence(tokens);
            //_isBusy = false;


            _isBusy = false;
            StateHasChanged();
        }
        else
        {
            if (AppState.ChatSettings.Streaming)
            {
                var chatSequence = ChatService.StreamingChatResponse(_chatView.ChatState.ChatHistory, AppState.ActiveSystemPrompt, AppState.ChatSettings.Model, token);
                await ExecuteChatSequence(chatSequence);
                _isBusy = false;
                StateHasChanged();
            }
            else
            {
                await ExecuteNonStreamingChat(token);
            }

        }
    }
    private async Task ExecuteNonStreamingChat(CancellationToken cancellationToken)
    {
        var chatSequence = await ChatService.ChatResponse(_chatView.ChatState.ChatHistory, AppState.ActiveSystemPrompt, AppState.ChatSettings.Model, cancellationToken);
        _chatView.ChatState.AddAssistantMessage(chatSequence ?? "");

    }
    private async Task ExecuteNonStreamingLogProbs(CancellationToken cancellationToken)
    {
        var tokenStrings = await ChatService.GetLogProbs(_chatView.ChatState.ChatHistory, AppState.ActiveSystemPrompt, AppState.ChatSettings.Model, cancellationToken);
        var tokenList = tokenStrings.ToList();
        AppState.TokenStrings = tokenList;
        var content = string.Join("", AppState.TokenStrings.Select(x => x.StringValue));
        _chatView.ChatState.AddAssistantMessage(content, tokenStrings: tokenList);
    }

    private async void HandleRequest(UserInputRequest userInputRequest)
    {
        if (userInputRequest.FileUpload is null)
        {
            HandleInput(userInputRequest.ChatInput);
            return;
        }
        var text = new TextContent(userInputRequest.ChatInput);
        var url = await GetImageUrlFromBlobStorage(userInputRequest.FileUpload!);
        var image = new ImageContent(new Uri(url));
        _chatView.ChatState.AddUserMessage(text, image);
        await SubmitRequest();
    }
    private async Task<string> GetImageUrlFromBlobStorage(FileUpload file)
    {
        var url = await BlobService.UploadBlobFile(file.FileName!, file.FileBytes!);
        return url;
    }
    private void ShowSettings(NotificationMessage m)
    {
        DialogService.Open<ModelSettingsMenu>("Chat Settings", options: new DialogOptions { Height = "50vh", Width = "35vw", ShowTitle = false, CloseDialogOnOverlayClick = true });
    }
    private void ShowSettings()
    {
        DialogService.Open<ModelSettingsMenu>("Chat Settings", options: new DialogOptions { Height = "50vh", Width = "35vw", ShowTitle = false, CloseDialogOnOverlayClick = true });
    }
    private async Task ExecuteChatSequence(IAsyncEnumerable<string> chatseq)
    {
        var hasStarted = false;
        await foreach (var response in chatseq)
        {
            if (!hasStarted && !AppState.AppSettings.AutoCompleteView)
            {
                hasStarted = true;
                _chatView.ChatState.AddAssistantMessage(response);
                _chatView.ChatState.ChatMessages.LastOrDefault(x => x.Role == Role.Assistant)!
                    .IsActiveStreaming = true;
                continue;
            }

            _chatView.ChatState.UpsertAssistantMessage(response);
        }

        var lastAsstMessage =
            _chatView.ChatState.ChatMessages.LastOrDefault(x => x.Role == Role.Assistant);
        if (lastAsstMessage is not null)
            lastAsstMessage.IsActiveStreaming = false;
    }
    private async Task ExecuteTokenChatSequence(IAsyncEnumerable<TokenString> tokenStrings)
    {
        var hasStarted = false;
        await foreach (var tokenString in tokenStrings)
        {
            _chatView.ChatState.UpdateAssistantMessage(tokenString);
            _chatView.ChatState.ChatMessages.LastOrDefault(x => x.Role == Role.Assistant)!
                .IsActiveStreaming = true;
        }
        var lastAsstMessage =
            _chatView.ChatState.ChatMessages.LastOrDefault(x => x.Role == Role.Assistant);
        if (lastAsstMessage is not null)
            lastAsstMessage.IsActiveStreaming = false;
    }
    private string AsHtml(string? text)
    {
        if (text == null) return "";
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var result = Markdown.ToHtml(text, pipeline);
        return result;

    }
}