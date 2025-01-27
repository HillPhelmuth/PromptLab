using Azure.AI.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAI.Chat;
using PromptLab.Core.Models;
using Tiktoken;
using Encoder = Tiktoken.Encoder;

namespace PromptLab.Core.Helpers;

public static class LogProbExtensions
{
    public static List<TokenString> AsTokenStrings(this IReadOnlyList<ChatTokenLogProbabilityDetails> logProbContentItems)
    {
        var result = new List<TokenString>();
        foreach (var logProb in logProbContentItems)
        {
            var tokenString = new TokenString(logProb.Token, logProb.NormalizedLogProb());
            if (logProb.TopLogProbabilities is { Count: > 0 })
            {
                var innerResult = logProb.TopLogProbabilities.Select(item => new TokenString(item.Token, item.NormalizedLogProb())).ToList();
                tokenString.TopLogProbs = innerResult;
            }
            result.Add(tokenString);
        }
        return result;
    }
    public static TokenString AsTokenString(this IReadOnlyList<ChatTokenLogProbabilityDetails> logProbabilityInfo)
    {
	    var probabilityResult = logProbabilityInfo[0];
        var tokenString = new TokenString(probabilityResult.Token, probabilityResult.NormalizedLogProb());
        foreach (var logProb in probabilityResult.TopLogProbabilities)
		{
			tokenString.TopLogProbs.Add(new TokenString(logProb.Token, logProb.NormalizedLogProb()));
		}
        return tokenString;
    }
    public static double NormalizedLogProb(this ChatTokenTopLogProbabilityDetails logProbabilityResult)
    {
        return Math.Exp(logProbabilityResult.LogProbability);
    }
    public static double NormalizedLogProb(this ChatTokenLogProbabilityDetails logProbInfo)
    {
        return Math.Exp(logProbInfo.LogProbability);
    }
    
}
public static class TokenHelper
{
    private static Encoder _encoding = ModelToEncoder.For("gpt-4o");
    public static int GetTokens(string text)
    {
        return _encoding.CountTokens(text);
    }
}
