using PromptLab.Core.Models.HttpDelegateHandlers;

namespace PromptLab.Core.Models;

public class DelegateHandlerFactory
{

    public static DelegatingHandler GetDelegatingHandler<T>(StringEventWriter output)
    {
        if (typeof(T) == typeof(LoggingHandler))
            return new LoggingHandler(new HttpClientHandler(), output);
        if (typeof(T) == typeof(SystemToDeveloperRoleHandler))
            return new SystemToDeveloperRoleHandler(new HttpClientHandler(), output);
        if (typeof(T) == typeof(DeepseekReasoningContentHandler))
            return new DeepseekReasoningContentHandler(new HttpClientHandler(), output);
        if (typeof(T) == typeof(AddThinkingConfigHandler))
            return new AddThinkingConfigHandler(new HttpClientHandler(), output);
        throw new NotSupportedException($"The type {typeof(T).Name} is not supported.");
    }
}