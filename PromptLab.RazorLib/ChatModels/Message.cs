using PromptLab.Core.Models;

namespace PromptLab.RazorLib.ChatModels;

public class Message(Role role, string content, int order)
{
    public int Order { get; set; } = order;
    public string? Content { get; set; } = content;
    public List<TokenString> TokenStrings { get; set; } = [];
    public Role Role { get; set; } = role;
    public DateTime TimeStamp { get; set; } = DateTime.Now;
    public bool IsActiveStreaming { get; set; }
    public string? ImageUrl { get; set; }
    public List<string> ImageUrls { get; set; } = [];
    //public bool IsModify { get; set; }

    public static Message UserMessage(string content, int order)
    {
        return new Message(Role.User, content, order);
    }
    public static Message UserMessage(string content, string imageUrl, int order)
    {
        return new Message(Role.User, content, order) { ImageUrl = imageUrl};
    }

    public static Message UserMessage(string content, List<string> imageUrls, int order)
    {
        return new Message(Role.User, content, order) { ImageUrls = imageUrls };
    }
    public static Message AssistantMessage(string content, int order)
    {
        return new Message(Role.Assistant, content, order);
    }

    public string CssClass => Role.ToString().ToLower();
}
public class NewMessageForm
{
	public Role Role { get; set; }
    public string? Content { get; set; }
}

public enum Role
{
    User, Assistant
}