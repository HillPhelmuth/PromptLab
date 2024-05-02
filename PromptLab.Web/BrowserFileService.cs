using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using PromptLab.Core.Models;
using PromptLab.Core.Services;
using PromptLab.Web.Components;
using Radzen;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PromptLab.Web
{
	public class BrowserFileService : IFileService
	{
		private readonly ProtectedLocalStorage _localStorage;
		private readonly DialogService _dialogService;
		private Dictionary<string, string> _prompts = [];
		private IJSRuntime _jsRuntime;
		public BrowserFileService(ProtectedLocalStorage localStorage, DialogService dialogService, IJSRuntime jsRuntime)
		{
			_localStorage = localStorage;
			_dialogService = dialogService;
			_jsRuntime = jsRuntime;
		}
		private const string Key = "PromptLabFiles";
		public async Task<string?> OpenFileAsync(string fileName = "")
		{
			var files = await _localStorage.GetAsync<Dictionary<string, string>>(Key);
			if (files is { Success: true, Value: not null })
			{
				_prompts = files.Value;
				var selectedFile = await _dialogService.OpenAsync<AvailableFilesWindow>("", new() { ["AvailableFiles"] = _prompts.Keys.ToList() }, new DialogOptions { Draggable = true, ShowClose = false, Resizable = true });
				if (selectedFile is not null)
				{
					return _prompts[selectedFile];
				}
			}
			return null;

		}
		public async Task<(string, byte[])> OpenImageFileAsync()
		{
			var fileContent = await _dialogService.OpenAsync<UploadImageWindow>("", options: new DialogOptions { Draggable = true, ShowClose = true, Resizable = true, ShowTitle = false, CloseDialogOnOverlayClick = true});
			if (fileContent is FileUpload file)
			{
				return (file.FileName!, file.FileBytes!);
			}
			return ("", Array.Empty<byte>());
		}
		public async Task<string?> SaveFileAsync(string fileName, string file)
		{
			var confirmed = await _dialogService.OpenAsync<SavePromptWindow>("", options: new DialogOptions { Draggable = true, ShowClose = false, Resizable = true, ShowTitle = false });
			if (string.IsNullOrWhiteSpace(confirmed?.ToString())) return null;
			_prompts = (await _localStorage.GetAsync<Dictionary<string, string>>(Key)).Value ?? [];
			_prompts[confirmed] = file;
			await _localStorage.SetAsync(Key, _prompts);
			return $"File {confirmed} Saved";
		}

		public async void ZoomChanged(double zoom)
		{
			await _jsRuntime.InvokeVoidAsync("toggleZoomScreen", $"{zoom}");
		}

		public async Task SaveUserSettings(UserProfile userProfile)
		{
			Console.WriteLine(
				$"User Profile Saved:\n{JsonSerializer.Serialize(userProfile, new JsonSerializerOptions {WriteIndented = true})}\n----------------\n");
			await _localStorage.SetAsync("UserProfile", userProfile);
		}

		public async Task<UserProfile> LoadUserSettings()
		{
			var profileResult = await _localStorage.GetAsync<UserProfile>("UserProfile");
			var profile = profileResult.Value;
			Console.WriteLine(
				$"User Profile Saved:\n{JsonSerializer.Serialize(profile, new JsonSerializerOptions {WriteIndented = true})}\n----------------\n");

			return profile ?? new UserProfile();

		}


	}
}
