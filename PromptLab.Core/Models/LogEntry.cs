namespace PromptLab.Core.Models;

public record LogEntry(string Content, DisplayType DisplayType, string Label = "", string Description = "");
