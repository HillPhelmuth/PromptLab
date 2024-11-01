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
public class AutoInvokeFunctionFilter: IAutoFunctionInvocationFilter
{
	public event EventHandler<AutoFunctionInvocationContext>? AutoFunctionInvoked;
	public event EventHandler<AutoFunctionInvocationContext>? AutoFunctionInvoking;
	private bool _hasTriggered;
	public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
	{
		AutoFunctionInvoking?.Invoke(this, context);
		if (_hasTriggered)
		{
			context.Terminate = true;
            _hasTriggered = false;
            return;
        }
		else
		{
			_hasTriggered = true;
		}
		await next(context);
		AutoFunctionInvoked?.Invoke(this, context);
		_hasTriggered = false;
		if (context.Function.Name == "SavePrompt")
			context.Terminate = true;
	}
}