using Microsoft.AspNetCore.Components;
using PromptLab.Core.Models;
using Radzen;

namespace PromptLab.RazorLib.Components.ChatComponents.LogProbComponents;

public partial class AlternativesGrid
{
	[Parameter] public TokenString TokenString { get; set; }
	[Inject]
	private DialogService DialogService { get; set; } = default!;

	private void Select(TokenString tokenString)
	{
		DialogService.Close(tokenString);
	}
}