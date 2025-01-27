using System.Text.Json;
using Microsoft.Bing.WebSearch;
using Microsoft.Bing.WebSearch.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PromptLab.Core.Models;
using ReverseMarkdown.Converters;
using WebSearchApiKeyServiceClientCredentials = Microsoft.Bing.WebSearch.ApiKeyServiceClientCredentials;

namespace PromptLab.Core.Services
{
    public class BingWebSearchService
    {
        //private readonly WebSearchClient _webSearchClient;
        //private readonly IHttpClientFactory _httpClientFactory;
        //private readonly HttpClient _httpClient;
        private readonly WebSearchClient _webSearchClient;
        private readonly ILogger<BingWebSearchService> _logger;
        public BingWebSearchService(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            //_httpClientFactory = httpClientFactory;
            _webSearchClient = new WebSearchClient(new WebSearchApiKeyServiceClientCredentials(configuration["Bing:ApiKey"]));
            _logger = loggerFactory.CreateLogger<BingWebSearchService>();
           
           

        }

        public async Task<List<WebPage>?> SearchAsync(string query, int answerCount = 10, string? freshness = null)
        {
            if (answerCount < 3) answerCount = 3;
            _logger.LogInformation("Searching Bing for {query} with answerCount {answerCount}", query, answerCount);

            SearchResponse serviceResponse = await _webSearchClient.Web.SearchAsync(query, answerCount: answerCount, freshness:freshness);
            var responseJson =
                JsonSerializer.Serialize(serviceResponse, new JsonSerializerOptions() { WriteIndented = true });
            _logger.LogInformation($"{Br}Bing Service Web Search Response:{Br}{responseJson}{Br}");
            var webPages = serviceResponse?.WebPages?.Value;
            //var bingWebSearchResults = webPages?.Select(x => new BingSearchResult(x.Url, x.Snippet, x.DisplayUrl, x.Name) { WebSearchUrl = x.WebSearchUrl, DateLastCrawled = DateTimeOffset.Parse(x.DateLastCrawled),DeepLinks = x.DeepLinks?.Select(y => new BingSearchResult(y.Url, y.Snippet, y.DisplayUrl, y.Name)).ToList()}).ToList();
            
            _logger.LogInformation("Search Bing Results:\n {result}",
                string.Join("\n", webPages?.Select(x => x.ToString()) ?? Array.Empty<string>()));
            return webPages.ToList();

        }

        public async Task<List<WebPage>> DeepSearchAsync(string query, int maxDepth = 2, int maxResults = 3)
        {
            var results = new List<WebPage>();
            var initialResults = (await SearchAsync(query, maxResults))?.ToList() ?? [];
            
            results.AddRange(initialResults);
            
            if (maxDepth > 1)
            {
                foreach (var result in initialResults)
                {
                    if (result.DeepLinks != null)
                    {
                        results.AddRange(result.DeepLinks);
                        //foreach (var deepLink in result.DeepLinks)
                        //{
                        //    var deepResults = await SearchAsync(deepLink.Name ?? query, 3);
                        //    if (deepResults != null)
                        //        results.AddRange(deepResults);
                        //}
                    }
                }
            }

            return results;
        }

        private const string Br = "\n--------------------------------\n";
    }
}
