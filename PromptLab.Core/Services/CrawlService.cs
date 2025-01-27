using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using PromptLab.Core.Helpers;
using PromptLab.Core.Models;
using ReverseMarkdown;

namespace PromptLab.Core.Services
{
    public class CrawlService
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<CrawlService> _logger;
        private HtmlWeb _htmlWeb = new();
        public CrawlService(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<CrawlService>();
        }
        public async Task<WebCrawlResult> CrawlAndExtractUrls(string url)
        {
            _logger.LogInformation($"Crawling url {url}");
            var doc = await _htmlWeb.LoadFromWebAsync(url);
            var urls = doc.DocumentNode?.Descendants("a").Select(x => x.GetAttributeValue("href", string.Empty)).ToList() ?? [];
            _logger.LogInformation($"Found {urls.Count} urls");
            return new WebCrawlResult(url) {ContentAsMarkdown = ConvertHtmlToMarkdown(url, doc)};
        }
        public async Task<WebCrawlResult> CrawlAsync(string url, bool fullParse = false)
        {
            _logger.LogInformation($"Crawling url {url}");
            var doc = await _htmlWeb.LoadFromWebAsync(url);
            var markdown = ConvertHtmlToMarkdown(url, doc);
            return new WebCrawlResult(url) { ContentAsMarkdown = markdown };
        }

        private string ConvertHtmlToMarkdown(string url, HtmlDocument doc)
        {
            var config = new Config
            {
                UnknownTags = Config.UnknownTagsOption.Bypass,
                GithubFlavored = true,
                RemoveComments = true,
                SmartHrefHandling = true
            };

            var converter = new Converter(config);
            var htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine(doc.DocumentNode.OuterHtml);
            //if (!fullParse)
            //{
            //    foreach (var child in doc.DocumentNode?.DescendantsAndSelf().Where(x => SupportedTags.Contains(x.Name.ToLower()) /*|| x.Name?.ToLower() == "p" || IsValidHeader(x.Name?.ToLower()) || x.Name == "table" || x.Name == "ol" || x.Name == "ul"*/) ?? new List<HtmlNode>())
            //    {
            //        htmlBuilder.Append(child.OuterHtml);
            //    }
            //}
            //else
            //{
            //    foreach (var child in doc.DocumentNode?.DescendantsAndSelf().Where(x => x.Name?.ToLower() != "script" && x.Name?.ToLower() != "style") ?? new List<HtmlNode>())
            //    {
            //        htmlBuilder.Append(child.OuterHtml);
            //    }
            //}
            var tidyHtml = Cleaner.PreTidy(htmlBuilder.ToString(), true);

            var mkdwnText = converter.Convert(tidyHtml);
           
            var cleanUpContent = CleanUpContent(mkdwnText);
            _logger.LogInformation($"{url}\n----------------\n Crawled for {TokenHelper.GetTokens(cleanUpContent)} Tokens");
            return cleanUpContent;
        }

        private static bool IsValidHeader(string? tagName)
        {
            if (tagName == null) return false;
            var pattern = new Regex("^h[1-6]$");
            return pattern.IsMatch(tagName);
        }

        private static string CleanUpContent(string content) => content.Replace("\t", " ");
        private static List<string> SupportedTags =>
        [
            "a",
            "aside",
            "blockquote",
            "br",
            "code",
            "dd",
            "div",
            "dl",
            "dt",
            "em",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "hr",
            "img",
            "li",
            "ol",
            "p",
            "pre",
            "s",
            "strong",
            "sup",
            "table",
            "td",
            "text",
            "tr"
        ];
    }
}
