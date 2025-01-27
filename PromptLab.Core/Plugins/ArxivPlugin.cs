using System.ComponentModel;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Sqlite;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Text;
using PromptLab.Core.Helpers;
using PromptLab.Core.Models;
using PromptLab.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.Pinecone;
using PineconeClient = Pinecone.PineconeClient;

namespace PromptLab.Core.Plugins;

public class ArxivPlugin(ILoggerFactory loggerFactory)
{
    private readonly ArxivApiService _arxivApiService = new(loggerFactory);
    [KernelFunction, Description("Searches arXiv for papers")]
    [return:Description("Metadata for the papers that meet the search criteria")]
    public async Task<string> SearchArxiv(
        [Description("The search query")] string query,
        [Description("The number of results to return")] int numResults = 20)
    {
        var searchResults = await _arxivApiService.QueryAsync(new ArxivQueryParameters { SearchQuery = query, MaxResults = numResults });
        return searchResults?.ToString() ?? "Search Failed!";

    }
    //[KernelFunction, Description("Full content for selected arXiv paper")]
    public async Task<string> GetArxivPaper(
        [Description("The arXiv metadata for the paper")] ArxivEntry arxivEntry)
    {
        var pdf = await _arxivApiService.GetContentAsync(arxivEntry);
        return pdf ?? "Failed to retrieve PDF!";
    }
    [KernelFunction, Description("Saves most relevant arXiv papers for later retrieval")]
    public async Task SaveArxivPapers(Kernel kernel,
        [Description("The arXiv metadata for the papers to save")] List<ArxivEntry> arxivEntries)
    {
        var appState = kernel.Services.GetRequiredService<AppState>();
        var chatModelSettings = appState.ChatModelSettings;
        foreach (var entry in arxivEntries)
        {
            var pdf = await _arxivApiService.GetContentAsync(entry);
            var segments = ChunkIntoSegments(pdf, 3, 1096, entry.Title, false);
            var url = entry.PdfLink;
            var title = entry.Title;
            var searchResultItems = segments.Select(segment => new SearchResultItem(url) { Content = segment, Title = title }).ToList();
            await SaveResearchItems(searchResultItems, chatModelSettings, kernel);
        }
    }
    private static IEnumerable<string> ChunkIntoSegments(string text, int segmentCount, int maxPerSegment = 1096, string description = "", bool ismarkdown = true)
    {
        var total = TokenHelper.GetTokens(text);
        var perSegment = total / segmentCount;
        
        List<string> paragraphs;
        if (ismarkdown)
        {
            var lines = TextChunker.SplitMarkDownLines(text, 200, TokenHelper.GetTokens);
            paragraphs = TextChunker.SplitMarkdownParagraphs(lines, maxPerSegment, 200, description, TokenHelper.GetTokens);
        }
        else
        {
            var lines = TextChunker.SplitPlainTextLines(text, 200, TokenHelper.GetTokens);
            paragraphs = TextChunker.SplitPlainTextParagraphs(lines, maxPerSegment, 200, description, TokenHelper.GetTokens);
        }
        return paragraphs;
    }
    private IVectorStoreRecordCollection<string, ResearchVectorStoreContextItem>? _pineconeCollection;
    public const string CollectionName = "promptlab-research";
    private async Task<List<string>> SaveResearchItems(List<SearchResultItem> searchResultItems, ChatModelSettings settings, Kernel kernel)
    {
        var textEmbeddingGeneration = new OpenAITextEmbeddingGenerationService(
            modelId: "text-embedding-3-small",
            apiKey: settings.OpenAIApiKey!);
        //DbConnection connection = new SqliteConnection(@".\Data\PromptLab.db");
        var config = kernel.Services.GetRequiredService<IConfiguration>();
        var vectorStore = new InMemoryVectorStore()/*new PineconeVectorStore(new PineconeClient(config["Pinecone:ApiKey"]))*/;
        _pineconeCollection = vectorStore.GetCollection<string, ResearchVectorStoreContextItem>(CollectionName);
        await _pineconeCollection.CreateCollectionIfNotExistsAsync();
        var contextItems = searchResultItems.Select(x => new ResearchVectorStoreContextItem(x.Url, x.Title, x.Content, "WebContent")).ToList();
        var embeddingTasks = contextItems.Select(entry => Task.Run(async () =>
        {
            entry.Embedding = await textEmbeddingGeneration.GenerateEmbeddingAsync(entry.Content!);
        }));
        await Task.WhenAll(embeddingTasks);
        var result = new List<string>();
        foreach (var item in contextItems)
        {
            item.MemoryId = await _pineconeCollection.UpsertAsync(item);
            result.Add(JsonSerializer.Serialize(new { item.Source, item.Title, item.Link }));
        }
        return result;
    }
}