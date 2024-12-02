namespace PromptLab.Core.Models;

public record LogEntry(string Content, DisplayType DisplayType, string Label = "", string Description = "")
{
    public string Content { get; set; } = Content;
    public DisplayType DisplayType { get; set; } = DisplayType;
    public bool IsComplete { get; set; }
}
