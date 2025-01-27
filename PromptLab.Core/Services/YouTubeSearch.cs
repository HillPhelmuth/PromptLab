using System.ComponentModel;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;

namespace PromptLab.Core.Services;

public class YouTubeSearch
{
    private readonly YouTubeService _youtubeService;
    private readonly IConfiguration _configuration;
    private static readonly HttpClient HttpClient = new();
    public YouTubeSearch(IConfiguration configuration)
    {
        var apiKey = configuration["YouTubeSearch:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            throw new ArgumentNullException(nameof(apiKey));
        _configuration = configuration;

        _youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = apiKey,
            ApplicationName = GetType().ToString(),
        });
    }

    public async Task<List<YouTubeSearchResult>> Search(string keyWords, int count = 10)
    {
        if (string.IsNullOrEmpty(keyWords))
            throw new ArgumentNullException(nameof(keyWords));


        try
        {
            var searchListRequest = _youtubeService.Search.List("snippet");
            searchListRequest.Type = "video";
            searchListRequest.MaxResults = count;
            searchListRequest.Q = keyWords;

            SearchListResponse? searchListResponse = await searchListRequest.ExecuteAsync();
            var results = searchListResponse.Items.Select(searchResult => new YouTubeSearchResult(searchResult.Id.VideoId, searchResult.Snippet.Description, keyWords)).ToList();
            return results;
            //var options = new JsonSerializerOptions { WriteIndented = true };
            //result = JsonSerializer.Serialize(searchListResponse.Items, options);

        }
        catch (Exception e)
        {
            //result = string.Empty;
            Console.WriteLine($"Youtube search error:\n{e}");
            return new List<YouTubeSearchResult>();
        }

        //return result;
    }
    public async Task<string> TranscribeVideo(string videoId, string language = "en")
    {
        try
        {
            var watchUrl = $"https://www.youtube.com/watch?v={videoId}";
            var response = await HttpClient.GetStringAsync(watchUrl);

            var doc = new HtmlDocument();
            doc.LoadHtml(response);

            var scriptTags = doc.DocumentNode.SelectNodes("//script");
            if (scriptTags == null) return "Transcript not found";
            Console.WriteLine($"Script tags count: {scriptTags.Count}");
            foreach (var scriptTag in scriptTags)
            {
                if (!scriptTag.InnerText.Contains("captionTracks")) continue;
                var regex = new Regex("\"captionTracks\":(\\[.*?\\])");
                var match = regex.Match(scriptTag.InnerText);

                if (match.Groups.Count <= 1) continue;
                var captionTracks = JsonSerializer.Deserialize<CaptionTrack[]>(match.Groups[1].Value);

                if (captionTracks is not { Length: > 0 }) continue;
                var transcriptUrl = captionTracks[0].BaseUrl;
                Console.WriteLine($"Caption tracks count: {captionTracks.Length}");
                foreach (var track in captionTracks)
                {
                    var parsedUrl = new Uri(track.BaseUrl);
                    var queryParams = System.Web.HttpUtility.ParseQueryString(parsedUrl.Query);

                    if (queryParams["lang"] != language) continue;
                    transcriptUrl = track.BaseUrl;
                    break;
                }

                var transcript = await HttpClient.GetStringAsync(transcriptUrl);
                return ExtractPlainTextFromTranscript(transcript);
            }

            return "Transcript not found";
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
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

[TypeConverter(typeof(GenericTypeConverter<YouTubeSearchResult>))]
public record YouTubeSearchResult(string Id, string Description, string Query)
{
    public string WatchUrl => $"https://www.youtube.com/watch?v={Id}";
    public string? Transcription { get; set; }
}

internal class GenericTypeConverter<T> : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => true;

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        Console.WriteLine($"Converting {value} to {typeof(T)}");
        return JsonSerializer.Deserialize<T>((string)value);
    }
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        Console.WriteLine($"Converting {typeof(T)} to {value}");
        return JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });
    }
}
