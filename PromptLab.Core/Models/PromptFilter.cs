using Microsoft.SemanticKernel;

namespace PromptLab.Core.Models;

public class PromptFilter : IPromptRenderFilter
{
	public event EventHandler<PromptRenderContext>? PromptRendered;


	public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
	{
		await next(context);
		PromptRendered?.Invoke(this, context);
	}
}