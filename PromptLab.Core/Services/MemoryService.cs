using Microsoft.Extensions.Configuration;
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
using Microsoft.SemanticKernel;
using PromptFlowEvalsAsPlugins;
using PromptLab.Core.Models;

namespace PromptLab.Core.Services;

public class MemoryService
{
    private static IConfiguration _configuration;
    private static ILoggerFactory _loggerFactory;
    private readonly EvalService _evalService;
    public MemoryService(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _loggerFactory = loggerFactory;
        _evalService = new EvalService(ChatService.CreateKernel("gpt-3.5-turbo"));
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
    public async IAsyncEnumerable<MemoryQueryResult> SearchVectorStoreAsync(string query, int count = 10, double minSimilarity = 0.5, CollectionType collectionType = CollectionType.PromptEngineeringCollectionSmall, bool useHyde = false)
    {
        var kernel = ChatService.CreateKernel("gpt-3.5-turbo");
        var hydePromptResult = await kernel.InvokePromptAsync("""
                                                                    Please write a passage to answer the question
                                                                    Try to include as many key details as possible.

                                                                    {{$context_str}}

                                                                    Passage:
                                                                    """, new KernelArguments() { ["context_str"] = query });
        var hydePrompt = hydePromptResult.GetValue<string>() ?? "";
        query = useHyde ? hydePrompt : query;
        var model = collectionType == CollectionType.PromptEngineeringEnhancedCollection ? "text-embedding-3-large" : "text-embedding-3-small";
        var memory = await GetSqliteMemory(model);
        var collection = collectionType == CollectionType.PromptEngineeringCollectionSmall ? AppConstants.PromptEngineeringEnhancedCollectionSmall : AppConstants.PromptEngineeringEnhancedCollection;
        var results = memory.SearchAsync(collection, hydePrompt, count, minSimilarity);
        await foreach (var result in results)
        {
            yield return result;
        }
    }
    public async IAsyncEnumerable<InputModel> GenerateBenchmarkInputModels(bool useHyde = false, int count = 100)
    {
        var benchmarks = BenchmarkQandA.GeneratedQuestions.Take(count);
        var results = new List<InputModel>();
        foreach (var benchmark in benchmarks)
        {
            var inputModel = await GenerateContextInputModel(benchmark.Question, benchmark.GoldenAnswer ?? "", useHyde);
            foreach (var model in inputModel) yield return model;
            results.AddRange(inputModel);
        }
        var hdt = useHyde ? "_Hyde" : "";
        var benchmarkinputmodelsJson = $"BenchmarkInputModels{hdt}2.json";
        await File.WriteAllTextAsync(benchmarkinputmodelsJson, JsonSerializer.Serialize(results));
        
    }
    public async IAsyncEnumerable<EvalResultDisplay> GenerateEvalResults(List<InputModel> inputModels, bool useHyde = false)
    {
        var result = new List<EvalResultDisplay>();
        foreach (var model in inputModels)
        {
            var evalScore = await _evalService.ExecuteEval(model);

            var evalResultDisplay = new EvalResultDisplay(model, evalScore) { IsHyde = useHyde};
            yield return evalResultDisplay;
            result.Add(evalResultDisplay);
        }
        var hdt = useHyde ? "_Hyde" : "";
        var benchmarkinputmodelsJson = $"BenchmarkEvalResults{hdt}2.json";
        await File.WriteAllTextAsync(benchmarkinputmodelsJson, JsonSerializer.Serialize(result));
		//return result;
	}
    private async Task<List<InputModel>> GenerateContextInputModel(string question, string goldAnswer = "", bool useHyde = false)
    {
        var kernel = ChatService.CreateKernel("gpt-3.5-turbo");
        var hydePrompt = "";
        if (useHyde)
        {
            var hydePromptResult = await kernel.InvokePromptAsync("""
                                                                  Please write a passage to answer the question
                                                                  Try to include as many key details as possible.

                                                                  {{$context_str}}

                                                                  Passage:
                                                                  """,
                new KernelArguments() { ["context_str"] = question });
            hydePrompt = hydePromptResult.GetValue<string>() ?? "";
        }
        var query = useHyde ? hydePrompt : question;
        var contexts = await SearchVectorStoreAsync(query, count:3, useHyde: useHyde).ToListAsync();
        var contextBuilder = new StringBuilder();
        for (var i = 0; i < contexts.Count; i++)
        {
            contextBuilder.AppendLine($"Context {i + 1}: {contexts[i].Metadata.Text}");
        }
        var context = contextBuilder.ToString();
        var answer = await kernel.InvokePromptAsync("""
                                                    Context information is below.
                                                       ---------------------
                                                       {{$context_str}}
                                                       ---------------------
                                                       Given the context information and not prior knowledge, answer the query.
                                                       Query: {{$query_str}}
                                                       Answer:
                                                    """, new KernelArguments() { ["context_str"] = context, ["query_str"] = query });
        var answerVal = answer.GetValue<string>()!;
        var grounded = InputModel.GroundednessModel(answerVal, question, context);
        var grounded2 = InputModel.Groundedness2Model(answerVal, question, context);
        var relevance = InputModel.RelevanceModel(answerVal, context);
        var similarity = InputModel.SimilarityModel(answerVal, goldAnswer, question);
        var perceivedIntelligenceRag = InputModel.PerceivedIntelligenceModel(answerVal, question, context);
        return [grounded, grounded2, relevance, perceivedIntelligenceRag];
    }
    public async IAsyncEnumerable<MemoryQueryResult> GetAllVectorStoreContent(CollectionType collectionType = CollectionType.PromptEngineeringCollectionSmall)
    {
        var model = collectionType == CollectionType.PromptEngineeringEnhancedCollection ? "text-embedding-3-large" : "text-embedding-3-small";
        var memory = await GetSqliteMemory(model);

        var collection = collectionType == CollectionType.PromptEngineeringCollectionSmall ? AppConstants.PromptEngineeringEnhancedCollectionSmall : AppConstants.PromptEngineeringEnhancedCollection;
        var results = memory.SearchAsync(collection, "*", 500, 0.0, withEmbeddings:true);
        List<string> ids = [];
        var resultItems = new List<MemoryQueryResult>();
        await foreach (var result in results)
        {
            ids.Add(result.Metadata.Id);
            resultItems.Add(result);
            yield return result;
        }

        var fileName = $"AllMemorys_{collection}.json";
        await File.WriteAllTextAsync(fileName, JsonSerializer.Serialize(resultItems));
    }
}
public enum CollectionType
{
    PromptEngineeringCollectionSmall,
    PromptEngineeringEnhancedCollection
}
public enum RestateType
{
    Hyde, Keywords, Summary, Triplet
}
public record EvalResultDisplay(InputModel InputModel, ResultScore ResultScore)
{
    public string? Question => InputModel.RequiredInputs.Any(x => x.Key == "question") ? InputModel.RequiredInputs["question"]?.ToString() : "";
    public string? Answer => InputModel.RequiredInputs["answer"]?.ToString();
    public bool IsHyde { get; set; }
}