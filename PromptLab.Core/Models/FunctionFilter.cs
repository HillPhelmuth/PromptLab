using Microsoft.SemanticKernel;

namespace PromptLab.Core.Models;

public class FunctionFilter : IFunctionInvocationFilter
{
	public event EventHandler<FunctionInvocationContext>? FunctionInvoked;

	public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
	{
		await next(context);
		FunctionInvoked?.Invoke(this, context);
	}
}