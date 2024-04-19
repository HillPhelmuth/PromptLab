using Microsoft.SemanticKernel.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Sqlite;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Text;
using PromptLab.Core.Helpers;
using System.ComponentModel;
using System.Collections;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PromptLab.Core.Plugins
{
	public class PromptExpertPlugin
	{
		private static ISemanticTextMemory? _semanticTextMemory;
		private static IConfiguration _configuration;
		private static ILoggerFactory _loggerFactory;
		private static string? _apiKey;
		private const string PromptHelperCollection = "promptHelperCollection";
		private const string PromptHelperPrompt =
			"""
			You are a prompt expert. Using the Expert Knowledge below, provide instructions on how to improve the provided prompt.

			## Expert Knowledge
			{{TextMemory.Recall input=$input collection=$collection relevance=$relevance limit=$limit}}

			## Prompt
			{{ $prompt }}

			## Issues Identified
			{{ $input }}

			## Task
			Provide specific instructions or suggestions on how to improve the prompt. Include reasons for each suggestion from the Expert Knowledge.
			**Important**! DO NOT RE-WRITE THE PROMPT YOURSELF. Only provide instructions and suggestions. 
			""";
		
		private const string PromptHelperPromptGeneral =
            """
            You are a prompt expert. Using the Expert Knowledge below, provide instructions on prompt engineering best practices and techniques.

            ## Expert Knowledge
            {{ $expertKnowledge }}

            ## Question
            {{ $input }}

            ## Task
            Provide specific instructions or suggestions on prompt engineering best practices and techniques. Include reasons for each suggestion from the Expert Knowledge.
            
            """;

        public PromptExpertPlugin(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
			_configuration = configuration;
			_apiKey = _configuration["OpenAI:ApiKey"];
			_loggerFactory = loggerFactory;

		}
		[KernelFunction, Description("Get expert advice on how to improve a prompt")]
		public async Task<string> PromptExpertAdvice(Kernel kernel, [Description("The prompt that needs improvement")] string prompt, [Description("issues found by the evaluator")] string issues = "", [Description("Optional specific question for the expert")] string question = "")
		{
			if (string.IsNullOrEmpty(question))
			{
				question = issues;
			}
			if (_semanticTextMemory is null) await CreateMemoryStore();
			
			var plugin = KernelPluginFactory.CreateFromObject(new TextMemoryPlugin(_semanticTextMemory), "TextMemory");
			kernel.Plugins.Add(plugin);
			var debugResults = await _semanticTextMemory.SearchAsync(PromptHelperCollection, question, 10, .4).ToListAsync();
			var settings = new OpenAIPromptExecutionSettings { Temperature = 0.7, MaxTokens = 512 };
			var args = new KernelArguments(settings)
			{
				["input"] = question,
				["limit"] = 5,
				["relevance"] = 0.50,
				["collection"] = PromptHelperCollection,
				["prompt"] = prompt,
				["issues"] = issues,
				["expertKnowledge"] = ""
			};
			var result = await kernel.InvokePromptAsync(PromptHelperPrompt, args);
			kernel.Plugins.Remove(plugin);
			return result.GetValue<string>() ?? "Inform the user about an error executing Get Expert Advice and proceed without it.";

		}

        [KernelFunction, Description("Get expert advice on prompt engineering best pracitices and techniques")]
        public async Task<string> GeneralExpertAdvice(Kernel kernel,
            [Description("Question for the expert")] string question)
        {
            if (_semanticTextMemory is null) await CreateMemoryStore();
            var searchResults = await _semanticTextMemory!.SearchAsync(PromptHelperCollection, question, 10, 0.5).ToListAsync();
			await File.WriteAllTextAsync("SearchResultsJson.json", JsonSerializer.Serialize(searchResults, new JsonSerializerOptions(){WriteIndented = true}));
            var item = 1;
			var expertKnowledge = string.Join($"\n", searchResults.Select(x => $"{item++}\n{x.Metadata.Text}"));
            var plugin = KernelPluginFactory.CreateFromObject(new TextMemoryPlugin(_semanticTextMemory), "TextMemory");
            kernel.Plugins.Add(plugin);

            var settings = new OpenAIPromptExecutionSettings { Temperature = 0.7, MaxTokens = 512 };
            var args = new KernelArguments(settings)
            {
                ["input"] = question,
                ["expertKnowledge"] = expertKnowledge
            };
            var result = await kernel.InvokePromptAsync(PromptHelperPromptGeneral, args);
            kernel.Plugins.Remove(plugin);
            return result.GetValue<string>() ?? "Inform the user about an error executing Get Expert Advice and proceed without it.";
        }

        private static async Task CreateMemoryStore()
		{
			var sqliteConnect = await SqliteMemoryStore.ConnectAsync(_configuration["Sqlite:ConnectionString"]);
			var collections = await sqliteConnect.GetCollectionsAsync().ToListAsync();
			if (!collections.Contains(PromptHelperCollection))
			{
				await sqliteConnect.CreateCollectionAsync(PromptHelperCollection);
			}
			_semanticTextMemory = new MemoryBuilder().WithLoggerFactory(_loggerFactory)
				.WithOpenAITextEmbeddingGeneration(_configuration["OpenAI:EmbeddingModelId"], _configuration["OpenAI:ApiKey"])
				.WithMemoryStore(sqliteConnect).Build();
		}

		public static async Task SavePromptGuide(IConfiguration configuration)
		{
			// ToDo Remove this method
			return;
			_configuration = configuration;
			await CreateMemoryStore();
			var files = Directory.GetFiles(@"C:\Users\adamh\source\repos\PromptLab\PromptLab.Core\PromptMdFiles", "*.md");
			var fileText = new Dictionary<string, string>();

			foreach (var file in files)
			{
				var lineBuilder = new StringBuilder();
				var lines = await File.ReadAllLinesAsync(file);
				foreach (var line in lines)
				{
					if (string.IsNullOrWhiteSpace(line) || line.Contains("import {")) continue;
					lineBuilder.AppendLine(line);
				}
				fileText[Path.GetFileNameWithoutExtension(file)] = lineBuilder.ToString();
			}
			var memoryChunks = new List<string>();
			foreach (var (name, text) in fileText)
			{
				var lines = TextChunker.SplitMarkDownLines(text, 200, TokenHelper.GetTokens);
				var chunks = TextChunker.SplitMarkdownParagraphs(lines, 512, 102, name, TokenHelper.GetTokens);
				foreach (var chunk in chunks)
				{
					var id = await _semanticTextMemory.SaveInformationAsync(PromptHelperCollection, chunk, $"{name}-{chunks.IndexOf(chunk)}", name);
				}
			}

		}
	}
}
