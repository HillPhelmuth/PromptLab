using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using PromptLab.RazorLib.ChatModels;

namespace PromptLab.RazorLib.Components.ModalWindows;

public partial class AddMessageWindow
{
	[Inject]
	private DialogService DialogService { get; set; } = default!;
	private NewMessageForm _newMessageForm = new();

	private void Submit(NewMessageForm newMessageForm)
	{
		DialogService.Close(newMessageForm);
	}
}