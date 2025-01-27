using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace PromptLab.Core.Models;

public class AutoInvokeFilter(ILoggerFactory loggerFactory) : IAutoFunctionInvocationFilter
{
    public Dictionary<string, int> FunctionInvocationCounts { get; private set; } = [];
    public event Action<string>? FunctionResult;
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        var logger = loggerFactory.CreateLogger<AutoInvokeFilter>();
        var functionName = context.Function.Name;
        FunctionInvocationCounts.TryAdd(functionName, 0);
        logger.LogInformation($"AutoInvokeFilter Invoking: {functionName}");

        var invocationCount = FunctionInvocationCounts.GetValueOrDefault(functionName, 0);
        var functionSeqIndex = context.FunctionSequenceIndex;
        var requestSeqIndex = context.RequestSequenceIndex;
        //if (invocationCount > 0)
        //{
        logger.LogInformation("AutoInvokeFilter Skipping: {functionName}", new { functionName, invocationCount, functionSeqIndex, requestSeqIndex });

        //}
        await next(context);
        //if (functionName is "TranscribeVideo" or "TranscribeAndOutlineVideo")
        FunctionResult?.Invoke(context.Result.ToString());
        FunctionInvocationCounts[functionName]++;
        logger.LogInformation($"AutoInvokeFilter Invoked: {functionName},\nResult:\n{context.Result}");
    }
}