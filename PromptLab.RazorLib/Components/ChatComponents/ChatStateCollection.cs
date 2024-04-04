using Microsoft.Extensions.Logging;

namespace PromptLab.RazorLib.Components.ChatComponents;

public class ChatStateCollection(ILoggerFactory loggerFactory)
{
    private readonly ILogger<ChatStateCollection> _logger = loggerFactory.CreateLogger<ChatStateCollection>();

    public Dictionary<string, ChatState> ChatStates { get; } = [];

    public ChatState CreateChatState(string viewId)
    {
        ChatStates[viewId] = new ChatState();
        _logger.LogInformation("Created ChatState for viewId {viewId}", viewId);
        return ChatStates[viewId];
    }

    public ChatState GetChatState(string viewId)
    {
        if (ChatStates.TryGetValue(viewId, out var value))
        {
            return value;
        }

        _logger.LogError("ChatState for viewId {viewId} not found", viewId);
        throw new ArgumentException($"ChatState for viewId {viewId} not found");
    }

    public bool TryGetChatState(string viewId, out ChatState? chatState)
    {
        var chatViewIds = ChatStates.Keys;
        var chatViewIdsString = string.Join(", ", chatViewIds);
        _logger.LogInformation("TryGet for ViewId: {viewId}\nChatViewIds available: {chatViewIdsString}",viewId, chatViewIdsString);
        var tryGetChatState = ChatStates.TryGetValue(viewId, out var chat);
        chatState = chat;
        return tryGetChatState;
    }

}