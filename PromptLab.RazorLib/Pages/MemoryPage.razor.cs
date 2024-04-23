using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using PromptLab.Core.Services;
using Microsoft.SemanticKernel.Memory;
using Markdig;
using PromptLab.Core.Plugins;
using Radzen.Blazor;

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
	private async Task GetAll()
	{
		var count = 0;
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
	private string AsHtml(string? text)
	{
		if (text == null) return "";
		var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
		var result = Markdown.ToHtml(text, pipeline);
		return result;

	}
}