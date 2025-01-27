namespace PromptLab.Core.Models;

public class WebCrawlResult(string url)
{
    public string Url { get; } = url;
    public string? ContentAsMarkdown { get; set; }
    public List<string> LinkUrls { get; set; } = [];
    public string? ContentSummary { get; set; }
}

