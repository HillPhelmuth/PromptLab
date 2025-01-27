using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Anthropic;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using PromptLab.Core.Services;
//using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PromptLab.Core.Plugins;

public class YouTubePlugin
{
    private readonly YouTubeSearch _youTubeSearch;
    private Kernel? _kernel;
    public YouTubePlugin(IConfiguration configuration)
    {
        _youTubeSearch = new YouTubeSearch(configuration);
    }

    [KernelFunction, Description("Search YouTube for videos. Outputs a json array of youtube video descriptions and Ids")]
    [return: Description("YouTube search results")]
    public async Task<string> SearchVideos([Description("Youtube search query")] string query,
        [Description("Number of results")] int count = 10)
    {
        List<YouTubeSearchResult> results = await _youTubeSearch.Search(query, count);
        return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });

    }
    private HttpClient _httpClient = new HttpClient();
    [KernelFunction, Description("Transcribe a YouTube video. Outputs the full transcript of the video")]
    [return: Description("Youtube Video transcript")]
    public async Task<string> TranscribeVideo(string videoId, string language = "en")
    {
        var transcription = await _youTubeSearch.TranscribeVideo(videoId, language);
        return transcription;
    }
    //[KernelFunction, Description("Transcribe a YouTube video. Outputs a short outline of the transcript of the video. Use this function to get more information about a video without the expense of a full transcipt")]
    //[return: Description("Youtube Video transcript outline")]
    //public async Task<string> TranscribeAndOutlineVideo(Kernel kernel, string videoId, string language = "en")
    //{
    //    var transcription = await _youTubeSearch.TranscribeVideo(videoId, language);
    //    var kernelArgs = new KernelArguments() { ["input"] = transcription, ["query"] = "{no query}" };
    //    var transcriptionSummary = await _kernel.InvokePromptAsync<string>(WebCrawlPlugin.SummarizePrompt, kernelArgs);
    //    return transcriptionSummary ?? "";
    //}
    //[KernelFunction, Description("Search YouTube for videos, transcribe them and return the plain text transcript")]
    //[return: Description("A json collection of objects designed to facilitate youtube citations including watch url, title, and transcription summaries")]
    //public async Task<string> SearchTranscribeAndCiteYoutubeVideos(Kernel kernel, [Description("Youtube search query")] string query, [Description("Number of results")] int count = 10)
    //{
    //    var results = await _youTubeSearch.Search(query, count);
    //    _kernel = kernel.Clone();
    //    if (results?.Count == 0)
    //    {
    //        return "No videos found";
    //    }

    //    var transcriptionResults = await AddTransciptionResults(results);
    //    return JsonSerializer.Serialize(transcriptionResults, new JsonSerializerOptions() { WriteIndented = true });
    //}
    private async Task<IEnumerable<YouTubeSearchResult>> AddTransciptionResults(List<YouTubeSearchResult> results)
    {
        var tasks = results.Select(AddTranscription).ToList();
        return await Task.WhenAll(tasks);
    }
    private async Task<YouTubeSearchResult> AddTranscription(YouTubeSearchResult result)
    {
        var transcribeVideo = await _youTubeSearch.TranscribeVideo(result.Id);
        var kernelArgs = new KernelArguments() { ["input"] = transcribeVideo, ["query"] = result.Query };
        var transcriptionSummary = await _kernel.InvokePromptAsync<string>(WebCrawlPlugin.SummarizePrompt, kernelArgs);
        result.Transcription = transcriptionSummary;
        return result;
    }
    private string ExtractPlainTextFromTranscript(string xmlTranscript)
    {
        try
        {
            var doc = XDocument.Parse(xmlTranscript);
            var textElements = doc.Descendants("text");
            
            // Join all text elements with spaces to ensure words don't run together
            return string.Join(" ", textElements.Select(x => x.Value));
        }
        catch (Exception)
        {
            return "Failed to parse transcript XML";
        }
    }

    
    private class CaptionTrack
    {
        [JsonPropertyName("baseUrl")]
        public string BaseUrl { get; set; }
    }
}
