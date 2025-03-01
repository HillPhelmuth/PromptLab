﻿using Microsoft.SemanticKernel.Memory;
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
using Microsoft.SemanticKernel.Embeddings;
using PromptLab.Core.Models;
using static PromptLab.Core.Helpers.AppConstants;
using PromptLab.Core.Services;

namespace PromptLab.Core.Plugins;

public class PromptExpertPlugin
{
	internal static ISemanticTextMemory? _semanticTextMemory;
	internal static IConfiguration _configuration;
	private static ILoggerFactory _loggerFactory;
	private static string? _apiKey;
	internal const string PromptHelperCollection = "promptHelperCollection";
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
		You are a prompt engineering teacher. Utilize the following Expert Knowledge to guide your response to the question provided.

		Expert Knowledge: {{ $expertKnowledge }}

		Question: {{ $input }}

		Task: Carefully respond to the question by applying the expert knowledge provided. Structure your response as follows:

		Identify the main question or challenge.
		Offer specific, actionable suggestions on prompt engineering best practices and techniques, categorizing them as needed.
		Justify each suggestion with direct references or logical connections to the Expert Knowledge. Ensure each part of your response is clear and comprehensible to someone unfamiliar with prompt engineering.

		""";

    private const string UserPromptHelperGuidePrompt =
        $$$"""
        You are a prompt expert. Using the Chat Context and Expert Knowledge below, improve the provided prompt. Provide ONLY the improved prompt without preamble or explanation.

        ## Expert Knowledge
        1. 
        {{{Prompt.UserPromptGuide}}}
        2. 
        {{{Prompt.MasterChatGptPromptGuide}}}
        
        ## Chat Context
        {{ $chatHistory }}
        
        ## Prompt
        {{ $prompt }}
        """;

	public PromptExpertPlugin(IConfiguration configuration, ILoggerFactory loggerFactory)
	{
		_configuration = configuration;
		_apiKey = _configuration["OpenAI:ApiKey"];
		_loggerFactory = loggerFactory;		
	}
	[KernelFunction, Description("Get expert advice on advanced prompt engineering techniques to improve a complex prompt")]
	public async Task<string> PromptExpertAdvice(Kernel kernel,
        [Description("The prompt that needs improvement")] string prompt,
        [Description("Specific question for the expert")] string question,
        [Description("issues found by the evaluator (if applicable)")] string issues = "")
	{
		
		var embeddings = kernel.GetAllServices<ITextEmbeddingGenerationService>().FirstOrDefault();
		if (_semanticTextMemory is null && embeddings is not null) await CreateMemoryStore(embeddings);
		var plugin = KernelPluginFactory.CreateFromObject(new TextMemoryPlugin(_semanticTextMemory!), "TextMemory");
		if (kernel.Plugins.All(x => x.Name != "TextMemory")) 
			kernel.Plugins.Add(plugin);
			
		var settings = new OpenAIPromptExecutionSettings { Temperature = 0.7, MaxTokens = 512 };
		var args = new KernelArguments(settings)
		{
			["input"] = question,
			["limit"] = 7,
			["relevance"] = 0.45,
			["collection"] = PromptEngineeringCollection,
			["prompt"] = prompt,
			["issues"] = issues				
		};
		var result = await kernel.InvokePromptAsync(PromptHelperPrompt, args);
		kernel.Plugins.Remove(plugin);
		return result.GetValue<string>() ?? "Inform the user about an error executing Get Expert Advice and proceed without it.";

	}

	[KernelFunction, Description("Get expert advice on advanced prompt engineering techniques and use cases")]
	public async Task<string> GeneralExpertAdvice(Kernel kernel,
		[Description("Question for the expert")] string question)
	{
		var embeddings = kernel.GetAllServices<ITextEmbeddingGenerationService>().FirstOrDefault();
		if (_semanticTextMemory is null && embeddings is not null) await CreateMemoryStore(embeddings);
		var searchResults = await _semanticTextMemory!.SearchAsync(PromptEngineeringCollection, question, 7, 0.45).ToListAsync();
#if DEBUG
		await File.WriteAllTextAsync("SearchResultsJson.json", JsonSerializer.Serialize(searchResults, new JsonSerializerOptions() { WriteIndented = true }));
#endif
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
	[KernelFunction, Description("Improve a user's prompt using the various tips, tricks and prompting guides. NOTE: this is for user prompts not system prompts.")]
    public async Task<string> ImproveUserPrompt(Kernel kernel, string prompt, string chatHistory)
    {
        var item = ChatService.GetServiceId();
        var settings = ChatService.GetServicePromptExecutionSettings(item);
        var args = new KernelArguments(settings) { ["prompt"] = prompt, ["chatHistory"] = chatHistory };
		var result = await kernel.InvokePromptAsync(UserPromptHelperGuidePrompt, args);
        return result.ToString();
    }
	internal async Task CreateMemoryStore(ITextEmbeddingGenerationService embeddingGenerationService)
	{
		var sqliteConnect = await SqliteMemoryStore.ConnectAsync(_configuration["Sqlite:ConnectionString"]);
		var collections = await sqliteConnect.GetCollectionsAsync().ToListAsync();
		if (!collections.Contains(PromptEngineeringCollection))
		{
			await sqliteConnect.CreateCollectionAsync(PromptEngineeringCollection);
		}

		var memoryBuilder = new MemoryBuilder().WithLoggerFactory(_loggerFactory).WithMemoryStore(sqliteConnect);
		_semanticTextMemory = memoryBuilder.WithTextEmbeddingGeneration(embeddingGenerationService)
			.Build();
	}

}