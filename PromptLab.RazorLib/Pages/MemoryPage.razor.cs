using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using PromptLab.Core.Services;
using Microsoft.SemanticKernel.Memory;
using Markdig;
using PromptLab.Core.Plugins;
using Radzen.Blazor;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using Microsoft.SemanticKernel;
using PromptLab.Core.Models;
using Azure.AI.OpenAI;



#pragma warning disable SKEXP0001

namespace PromptLab.RazorLib.Pages;

public partial class MemoryPage
{
	[Inject]
	private MemoryService MemoryService { get; set; } = default!;
	[Inject]
	private MemorySave MemorySave { get; set; } = default!;

	private class MemorySearchForm
	{
		public string SearchText { get; set; } = "";
		public int Count { get; set; } = 5;
		public double MinThreshold { get; set; } = 0.5;
	}
	private MemorySearchForm _memorySearchForm = new();
	private List<MemoryQueryResult> _memoryQueryResults = [];
	private List<MemoryQueryResult> _memoryQueryResultsEnhanced = [];
	private RadzenDataGrid<MemoryQueryResult> _grid;
	private RadzenDataGrid<MemoryQueryResult> _grid2;
	private List<string> _logs = [];
	protected override Task OnInitializedAsync()
	{
		MemorySave.LogItem += AddLog;
		return base.OnInitializedAsync();
	}
	private async void AddLog(string log)
	{
		_logs.Add(log);
		await InvokeAsync(StateHasChanged);
	}
	private CancellationTokenSource _cancellationTokenSource = new();
	private bool _isBusy;
	private List<BatchRequestLine> _batchlines = [];

	private async Task SaveAllNew()
	{
		_isBusy = true;
		StateHasChanged();
		await Task.Delay(1);
		var token = _cancellationTokenSource.Token;
		await MemorySave.SavePromptGuide(token);
		_isBusy = false;
		StateHasChanged();
	}
	private void Cancel()
	{
		_cancellationTokenSource.Cancel();
		_isBusy = false;
		StateHasChanged();
	}
	private async void Submit(MemorySearchForm memorySearchForm)
	{
		ClearResults();
		var count = 0;
		await foreach (var result in MemoryService.SearchVectorStoreAsync(memorySearchForm.SearchText, memorySearchForm.Count, memorySearchForm.MinThreshold))
		{
			_memoryQueryResults.Add(result);
			count++;
			if (count % 5 == 0)
				await _grid.Reload();
		}
		count = 0;
		await foreach (var result in MemoryService.SearchVectorStoreAsync(memorySearchForm.SearchText, memorySearchForm.Count, memorySearchForm.MinThreshold, CollectionType.PromptEngineeringEnhancedCollection))
		{
			_memoryQueryResultsEnhanced.Add(result);
			count++;
			if (count % 5 == 0)
				await _grid2.Reload();
		}
		await _grid.Reload();
		await _grid2.Reload();
	}

	private void ClearResults()
	{
		_memoryQueryResults.Clear();
		_memoryQueryResultsEnhanced.Clear();
	}

	private async Task GetAll()
	{
		var count = 0;
		ClearResults();
		await foreach (var result in MemoryService.GetAllVectorStoreContent())
		{
			_memoryQueryResults.Add(result);
			count++;
			if (count % 10 == 0)
				await _grid.Reload();
		}

		await foreach (var result in MemoryService.GetAllVectorStoreContent(CollectionType.PromptEngineeringEnhancedCollection))
		{
			_memoryQueryResultsEnhanced.Add(result);
			count++;
			if (count % 10 == 0)
				await _grid2.Reload();
		}
		await _grid2.Reload();
	}
	private const string PromptHelperPromptGeneral =
		"""
		You are a prompt expert. Utilize the following Expert Knowledge to guide your response to the question provided.

		Expert Knowledge: {{ $expertKnowledge }}

		Question: {{ $input }}

		Task: Carefully respond to the question by applying the expert knowledge provided. Structure your response as follows:

		Identify the main question or challenge.
		Offer specific, actionable suggestions on prompt engineering best practices and techniques, categorizing them as needed.
		Justify each suggestion with direct references or logical connections to the Expert Knowledge. Ensure each part of your response is clear and comprehensible to someone unfamiliar with prompt engineering.

		""";
	private List<string> _questions = [];
	private async Task CreateQuestionBatch()
	{
		ClearResults();

		await GetAll();
		foreach (var result in _memoryQueryResults)
		{
			var sysPrompt = """
You are simulating a user that is asking questions about Prompt engineering.
Generate 1 question focused prompt engineering that can be answered by the user-provided text.
The text provided with a segment of a document on a prompt engineering topic. Be sure that the user-provided text could be used to largely answer the question.
Do not mention or reference the text specifically in the question.
""";
			var sysMessage = new Message { Role = "system", Content = sysPrompt };
			var userPrompt = result.Metadata.Text;
			var userMessage = new Message { Role = "user", Content = userPrompt };
			var body = new BatchLineBody { Messages = [sysMessage, userMessage] };
			var id = _memoryQueryResults.IndexOf(result) + 1;
			_batchlines.Add(new BatchRequestLine { CustomId = $"quest-gen-batch2-{id}", Body = body });
			StateHasChanged();
		}
		await File.WriteAllTextAsync("batchQuestions2.jsonl", BatchRequestLine.ToJsonLines(_batchlines));
	}

	private async Task CreateAnswerBatch()
	{
		ClearResults();
		var path = @"C:\Users\adamh\source\repos\PromptLab\PromptLab.Core\BatchFiles\batch_questionGen2_output.jsonl";
		var results = BatchResultsHelper.GetBatchResults(path);
		var kernel = Kernel.CreateBuilder().Build();
		foreach (var (id, question) in results)
		{
			_questions.Add(question);
			StateHasChanged();
			var expertBuilder = new StringBuilder();
			await foreach (var searchResult in MemoryService.SearchVectorStoreAsync(question, 7, 0.4))
			{
				expertBuilder.Append(searchResult.Metadata.Text);
			}
			var kernelArgs = new KernelArguments
			{
				["expertKnowledge"] = expertBuilder.ToString(),
				["input"] = question
			};
			var promptTemplateFactory = new KernelPromptTemplateFactory();
			var promptTemplate = promptTemplateFactory.Create(new PromptTemplateConfig(PromptHelperPromptGeneral));
			var sysPrompt = await promptTemplate.RenderAsync(kernel, kernelArgs);
			var sysMessage = new Message { Role = "system", Content = sysPrompt };
			var userMessage = new Message { Role = "user", Content = question };
			var body = new BatchLineBody {Model = "gpt-4-turbo", Messages = [sysMessage, userMessage] };
			_batchlines.Add(new BatchRequestLine { CustomId = id, Body = body });
		}
		await File.WriteAllTextAsync("batchAnswers2.jsonl", BatchRequestLine.ToJsonLines(_batchlines));
	}
	private List<Usage> _usages = [];
	private void GetUsages()
	{
		var path = @"C:\Users\adamh\source\repos\PromptLab\PromptLab.Core\BatchFiles\batch_YTQtehdzh2ATOAG9a29MDqvE_output.json";
		var results = BatchResultsHelper.GetUsageResults(path);
		_usages = results.Where(x => x != null).ToList()!;
		StateHasChanged();
	}
	private string AsHtml(string? text)
	{
		if (text == null) return "";
		var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
		var result = Markdown.ToHtml(text, pipeline);
		return result;

	}
}
