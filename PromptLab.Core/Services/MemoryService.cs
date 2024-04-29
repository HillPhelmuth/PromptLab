﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Sqlite;
using Microsoft.SemanticKernel.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PromptLab.Core.Helpers;
using System.Collections;
using System.Text.Json;

namespace PromptLab.Core.Services;

public class MemoryService
{
  	private static IConfiguration _configuration;
	private static ILoggerFactory _loggerFactory;
	public MemoryService(IConfiguration configuration, ILoggerFactory loggerFactory)
	{
		_configuration = configuration;
		_loggerFactory = loggerFactory;
	}
	public static async Task<ISemanticTextMemory> GetSqliteMemory(string? model = null)
	{
		model ??= _configuration["OpenAI:EmbeddingModelId"]!;
		var sqliteConnect = await SqliteMemoryStore.ConnectAsync(_configuration["Sqlite:ConnectionString"]!);
		var memory = new MemoryBuilder().WithLoggerFactory(_loggerFactory)
			.WithOpenAITextEmbeddingGeneration(model, _configuration["OpenAI:ApiKey"]!)
			.WithMemoryStore(sqliteConnect).Build();
		return memory;
	}
	public async IAsyncEnumerable<MemoryQueryResult> SearchVectorStoreAsync(string query, int count = 10, double minSimilarity = 0.5, CollectionType collectionType = CollectionType.PromptEngineeringCollectionSmall)
	{
		var model = collectionType == CollectionType.PromptEngineeringEnhancedCollection ? "text-embedding-3-large" : "text-embedding-3-small";
		var memory = await GetSqliteMemory(model);
		var collection = collectionType == CollectionType.PromptEngineeringCollectionSmall ? AppConstants.PromptEngineeringEnhancedCollectionSmall : AppConstants.PromptEngineeringEnhancedCollection;
		var results = memory.SearchAsync(collection, query, count, minSimilarity);
		await foreach (var result in results)
		{
			yield return result;
		}
	}
	public async IAsyncEnumerable<MemoryQueryResult> GetAllVectorStoreContent(CollectionType collectionType = CollectionType.PromptEngineeringCollectionSmall)
	{
		var model = collectionType == CollectionType.PromptEngineeringEnhancedCollection ? "text-embedding-3-large" : "text-embedding-3-small";
		var memory = await GetSqliteMemory(model);

		var collection = collectionType == CollectionType.PromptEngineeringCollectionSmall ? AppConstants.PromptEngineeringEnhancedCollectionSmall : AppConstants.PromptEngineeringEnhancedCollection;
		var results = memory.SearchAsync(collection, "*", 500, 0.0);
		List<string> ids = [];
		await foreach (var result in results)
		{
			ids.Add(result.Metadata.Id);
			yield return result;
		}
		await File.WriteAllTextAsync($"AllMemoryIds_{collection}.json", JsonSerializer.Serialize(ids));
	}
}
public enum CollectionType
{
	PromptEngineeringCollectionSmall,
	PromptEngineeringEnhancedCollection
}