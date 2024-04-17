﻿using Microsoft.AspNetCore.Components;
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
    protected override Task OnParametersSetAsync()
    {
        _requestForm.UserInputRequest.UserInputType = UserInputType;
        return base.OnParametersSetAsync();
    }


    private bool _isDisabled = false;

    private class RequestForm
    {
        public string? Content { get; set; }
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