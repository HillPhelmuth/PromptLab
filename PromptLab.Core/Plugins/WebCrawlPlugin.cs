using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Apis.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Text;
using PromptLab.Core.Helpers;
using PromptLab.Core.Services;
using Serilog.Extensions.Logging;

namespace PromptLab.Core.Plugins;

public class WebCrawlPlugin
{

    private KernelFunction _summarizeWebContent;
    private const int MaxTokens = 1024;
    private Kernel _kernel;
    private readonly BingWebSearchService? _searchService;


    public WebCrawlPlugin(BingWebSearchService bingWebSearchService)
    {
        //var kernel = CreateKernel();
        //var summarizePlugin = kernel.ImportPluginFromPromptDirectoryYaml("SummarizePlugin");
        //_summarizeWebContent = summarizePlugin["SummarizeLong"];
        //_kernel = kernel;
        _searchService = bingWebSearchService;

    }


    [KernelFunction, Description("Extract a web search query from a question")]
    public async Task<string> ExtractWebSearchQuery(string input)
    {
        var extractPlugin = _kernel.CreateFunctionFromPrompt("Extract terms for a simple web search query from a question. Include no other content\nquestion:{{$input}}", executionSettings: new OpenAIPromptExecutionSettings { MaxTokens = 128, Temperature = 0.0, TopP = 0 });
        var result = await _kernel.InvokeAsync<string>(extractPlugin, new KernelArguments { { "input", input } });
        return result;
    }
    [KernelFunction, Description("Retrive the content from a url in markdown format")]
    public async Task<string> ConvertWebToMarkdown([Description("The url to retrieve as markdown")] string url)
    {
        var crawler = new CrawlService(new SerilogLoggerFactory());
        var markdown = await crawler.CrawlAsync(url);
        return markdown.ContentAsMarkdown!;
    }
    [KernelFunction, Description("Search Web, summarize the content of each result and generate citations.")]
    [return: Description("A json collection of objects designed to facilitate web citations including url, title, and content")]
    public async Task<string> SearchAndCiteWeb(Kernel kernel, [Description("Web search query")] string query, [Description("Number of web search results to use")] int resultCount = 3, [Description("Defines the freshness of the results. Options are 'Day', 'Week', or 'Month'")] string? freshness = null)
    {
        var appState = kernel.Services.GetRequiredService<AppState>();
        var chatSettings = appState.ChatSettings;
        var cloneKernel = kernel.Clone();
        if (chatSettings.Model.Contains("mini") || chatSettings.Model.Contains("flash"))
        {
            _kernel = cloneKernel;
        }
        else
        {
            var model = chatSettings.Model.Contains("gpt-4o") ? "gpt-4o-mini" : chatSettings.Model.Contains("gemini") ? "gemini-1.5-flash-002" : chatSettings.Model;
            _kernel = ChatService.CreateKernel(model);
        }
        
        //var summarizePlugin = kernel.ImportPluginFromPromptDirectoryYaml("SummarizePlugin");
        _summarizeWebContent = KernelFunctionFactory.CreateFromPrompt(SummarizePrompt, functionName: "SummarizeLong");
        var results = await _searchService!.SearchAsync(query, resultCount, freshness) ?? [];
        var scraperTaskList = new List<Task<List<SearchResultItem>>>();
        foreach (var result in results.Take(Math.Min(results.Count, 5)))
        {

            try
            {
                scraperTaskList.Add(ScrapeChunkAndSummarize(result.Url, result.Name, query, result.Snippet));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to scrape text from {result.Url}\n\n{ex.Message}");
            }

        }

        var scrapeResults = await Task.WhenAll(scraperTaskList);
        var searchResultItems = scrapeResults.SelectMany(x => x).ToList();
        var resultItems = new List<SearchResultItem>();
        foreach (var group in searchResultItems.GroupBy(x => x.Url))
        {
            var count = group.Count();
            if (count > 1)
            {
                var index = 1;
                var groupItem = new SearchResultItem(group.Key)
                {
                    Title = group.First().Title,
                    Content = ""
                };
                foreach (var item in group)
                {
                    groupItem.Content += $"{index++}. {item.Content}\n";
                }
                resultItems.Add(groupItem);
            }
            else
            {
                resultItems.Add(new SearchResultItem(group.Key) { Title = group.First().Title, Content = group.First().Content });
            }
        }
        var searchCiteJson = JsonSerializer.Serialize(resultItems, new JsonSerializerOptions { WriteIndented = true });
#if DEBUG
        await File.WriteAllTextAsync("searchCiteJson.json", searchCiteJson);
#endif
        return searchCiteJson;
    }
    

    [KernelFunction, Description("Analyze an image from a URL and provide a description")]
    public async Task<string> AnalyzeWebImage(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return string.Empty;
        try
        {
            return await DescribeImageAsync(imageUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to analyze image from {imageUrl}: {ex.Message}");
            return string.Empty;
        }
    }
    private async Task<List<SearchResultItem>> ScrapeChunkAndSummarize(string url, string title, string input, string summary)
    {

        try
        {
            var crawler = new CrawlService(new SerilogLoggerFactory());
            var text = await crawler.CrawlAsync(url);
            var summarizeWebContent = await SummarizeContent(url, title, input, text.ContentAsMarkdown);
            return summarizeWebContent;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to scrape text from {url}\n\n{ex.Message}\n{ex.StackTrace}");
            return [new SearchResultItem(url) { Title = title, Content = summary }];
        }

    }
    private async Task<List<SearchResultItem>> SummarizeContent(string url, string title, string input, string text)
    {
        List<SearchResultItem> scrapeChunkAndSummarize;
        var tokens = TokenHelper.GetTokens(text);
        var count = tokens / 8192;
        var segments = ChunkIntoSegments(text, Math.Max(count, 1), 8192, title).ToList();
        Console.WriteLine($"Segment count: {segments.Count}");
        var argList = segments.Select(segment => new KernelArguments { ["input"] = segment, ["query"] = input }).ToList();
        var summaryResults = new List<SearchResultItem>();
        foreach (var arg in argList)
        {
            var result = await _kernel.InvokeAsync<string>(_summarizeWebContent, arg);
            summaryResults.Add(new SearchResultItem(url) { Title = title, Content = result });
        }

        scrapeChunkAndSummarize = summaryResults;
        return summaryResults;
    }

    private static IEnumerable<string> ChunkIntoSegments(string text, int segmentCount, int maxPerSegment = 4096, string description = "", bool ismarkdown = true)
    {
        var total = TokenHelper.GetTokens(text);
        var perSegment = total / segmentCount;
        var totalPerSegment = perSegment > maxPerSegment ? maxPerSegment : perSegment;
        List<string> paragraphs;
        if (ismarkdown)
        {
            var lines = TextChunker.SplitMarkDownLines(text, 200, TokenHelper.GetTokens);
            paragraphs = TextChunker.SplitMarkdownParagraphs(lines, totalPerSegment, 0, description, TokenHelper.GetTokens);
        }
        else
        {
            var lines = TextChunker.SplitPlainTextLines(text, 200, TokenHelper.GetTokens);
            paragraphs = TextChunker.SplitPlainTextParagraphs(lines, totalPerSegment, 0, description, TokenHelper.GetTokens);
        }
        return paragraphs.Take(segmentCount);
    }

    private async Task<string> DescribeImageAsync(string imageUrl)
    {
        var history = new ChatHistory
        {
            new(AuthorRole.User,
            [
                new ImageContent(new Uri(imageUrl)),
                new TextContent("describe and summarize image")
            ])
        };
        var chatService = _kernel.Services.GetRequiredService<IChatCompletionService>();
        var result = await chatService.GetChatMessageContentAsync(history);
        return result.Content;
    }
    public const string SummarizePrompt = """
                                           Create a detailed and comprehensive outline of a given document by structuring and paraphrasing its contents. The outline should retain all relevant information and present the ideas clearly and cohesively while maintaining the original context. Consider the original user query `{{ $query }}` for context in this outline.
                                           
                                           # Steps
                                           
                                           1. **Read the Document Thoroughly**: Understand the main ideas, key points, and supporting details.
                                           2. **Identify Key Information**: Select important facts, concepts, and arguments that are essential to the understanding of the document.
                                           3. **Organize Content into Sections**: Create sections and subsections reflecting the structure of the document. 
                                           4. **Paraphrase Content**: Rewrite the identified information of each section and sub-section in your own words, ensuring to maintain the original meaning and context.
                                           5. **Provide Subheadings**: Use subheadings to clearly define each section, ensuring logical progression and clarity.
                                           6. **Review and Revise**: Ensure the outline is coherent, free of errors, and accurately reflects the source material.
                                           
                                           # Output Format
                                           
                                           - The outline should include all crucial points structured logically with clear subheadings.
                                           - The language should be clear, concise, and free of unnecessary jargon or filler.
                                           - Outlines should succinctly cover the full scope of the document while adhering to the document's original structure.
                                           
                                           # Notes
                                           
                                           - Ensure that no significant details are omitted.
                                           - Organize information in a way that maintains relevance and coherence.
                                           - For large and complex documents, make use of detailed subheadings to enhance readability and understanding.
                                           - Incorporate `{{ $query }}` to provide context and focus for the outline.
                                           
                                           Outline this:
                                           {{ $input }}
                                           """;



}

public record SearchResultItem(string Url)
{
    [JsonPropertyName("content"), JsonPropertyOrder(3)]
    public string? Content { get; set; }
    [JsonPropertyName("title"), JsonPropertyOrder(1)]
    public string? Title { get; set; }
    [JsonPropertyName("url"), JsonPropertyOrder(2)]
    public string Url { get; set; } = Url;
}
