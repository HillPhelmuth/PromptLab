using Azure.AI.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PromptLab.Core.Models;
using Tiktoken;
using Encoding = Tiktoken.Encoding;

namespace PromptLab.Core.Helpers;

public static class LogProbExtensions
{
    public static List<TokenString> AsTokenStrings(this List<ChatTokenLogProbabilityResult> logProbContentItems)
    {
        var result = new List<TokenString>();
        foreach (var logProb in logProbContentItems)
        {
            var tokenString = new TokenString(logProb.Token, logProb.NormalizedLogProb());
            if (logProb.TopLogProbabilityEntries is { Count: > 0 })
            {
                var innerResult = logProb.TopLogProbabilityEntries.Select(item => new TokenString(item.Token, item.NormalizedLogProb())).ToList();
                tokenString.TopLogProbs = innerResult;
            }
            result.Add(tokenString);
        }
        return result;
    }
    public static double NormalizedLogProb(this ChatTokenLogProbabilityResult logProbabilityResult)
    {
        return Math.Exp(logProbabilityResult.LogProbability);
    }
    public static double NormalizedLogProb(this ChatTokenLogProbabilityInfo logProbInfo)
    {
        return Math.Exp(logProbInfo.LogProbability);
    }
    
}
public static class TokenHelper
{
    private static Encoding _encoding = Encoding.ForModel("gpt-4");
    public static int GetTokens(string text)
    {
        return _encoding.CountTokens(text);
    }
}
