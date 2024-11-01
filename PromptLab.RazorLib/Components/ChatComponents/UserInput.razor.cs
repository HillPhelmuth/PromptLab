using Microsoft.AspNetCore.Components;
using PromptLab.Core.Helpers;
using PromptLab.Core.Models;
using PromptLab.Core.Services;
using PromptLab.RazorLib.ChatModels;

namespace PromptLab.RazorLib.Components.ChatComponents;

public partial class UserInput : ComponentBase
{
    [Parameter]
    public string HelperText { get; set; } = "";
    [Parameter]
    public bool IsBusy { get; set; }

    [Parameter]
    public string ButtonLabel { get; set; } = "Submit";
    [Parameter]
    public EventCallback<string> MessageSubmit { get; set; }
    [Parameter]
    public EventCallback<string> MessageChanged { get; set; }
    [Parameter]
    public UserInputType UserInputType { get; set; }
    [Parameter]
    public UserInputFieldType UserInputFieldType { get; set; }
    [Parameter]
    public EventCallback<UserInputRequest> UserInputSubmit { get; set; }
    [Parameter]
    public EventCallback CancelRequest { get; set; }
    [Parameter]
    public bool IsRequired { get; set; } = true;
    [Parameter]
    public string? ImprovedPrompt { get; set; }
    [Parameter]
    public EventCallback<string> ImprovedPromptRequest { get; set; }
    [Inject]
    private IFileService FileService { get; set; } = default!;

    private bool _isPromptImproveRequested;
    protected override Task OnParametersSetAsync()
    {
        if (_isPromptImproveRequested && !string.IsNullOrEmpty(ImprovedPrompt))
        {
            _requestForm.UserInputRequest.ChatInput = ImprovedPrompt;
        }
        _requestForm.UserInputRequest.UserInputType = UserInputType;
        return base.OnParametersSetAsync();
    }
    
    private bool _isDisabled = false;

    private class RequestForm
    {
        public string? Content { get; set; }
        public bool ShowImageInput { get; set; }
        public UserInputRequest UserInputRequest { get; set; } = new();
    }

    private RequestForm _requestForm = new();
    private void Cancel()
    {
        CancelRequest.InvokeAsync();
    }
    private void ToggleInputType()
    {
        UserInputFieldType = UserInputFieldType == UserInputFieldType.TextBox
            ? UserInputFieldType.TextArea
            : UserInputFieldType.TextBox;
        StateHasChanged();
    }
    private async Task AddImage()
    {
        var (fileName, bytes) = await FileService.OpenImageFileAsync();
        if (bytes.Length > 0)
        {
            _requestForm.UserInputRequest.FileUpload = new FileUpload
            {
                FileName = fileName,
                FileBytes = bytes,
                FileBase64 = Convert.ToBase64String(bytes)
            };
        }
    }

    private async Task ImprovePrompt()
    {
        var currentPrompt = _requestForm.UserInputRequest.ChatInput;
        _isPromptImproveRequested = true;
        await ImprovedPromptRequest.InvokeAsync(currentPrompt);

    }
    private void SubmitRequest(RequestForm form)
    {
        MessageSubmit.InvokeAsync(form.UserInputRequest.ChatInput);
        UserInputSubmit.InvokeAsync(form.UserInputRequest);
        _requestForm = new RequestForm
        {
            UserInputRequest =
            {
                UserInputType = UserInputType
            }
        };
        StateHasChanged();
    }
}