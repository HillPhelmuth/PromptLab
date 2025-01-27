using System.Text.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using PromptLab.Core.Models;
using PromptLab.Core.Services;
using PromptLab.Web.Components;
using Radzen;


namespace PromptLab.Web;

public class BrowserFileService(ProtectedLocalStorage localStorage, DialogService dialogService, IJSRuntime jsRuntime) : IFileService
{
    private Dictionary<string, string> _prompts = [];
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions {WriteIndented = true};

    private const string Key = "PromptLabFiles";
    public async Task<string?> OpenFileTextAsync(params string[] fileExts)
    {
        var files = await localStorage.GetAsync<Dictionary<string, string>>(Key);
        if (files is { Success: true, Value: not null })
        {
            _prompts = files.Value;
            var selectedFile = await dialogService.OpenAsync<AvailableFilesWindow>("", new() { ["AvailableFiles"] = _prompts.Keys.ToList() }, new DialogOptions { Draggable = true, ShowClose = false, Resizable = true });
            if (selectedFile is not null)
            {
                return _prompts[selectedFile];
            }
        }
        return null;

    }
    public async Task<List<(string, byte[])>> OpenImageFileAsync()
    {
        var fileContent = await dialogService.OpenAsync<UploadImageWindow>("", options: new DialogOptions { Draggable = true, ShowClose = true, Resizable = true, ShowTitle = false, CloseDialogOnOverlayClick = true});
        if (fileContent is MultiFileUpload file)
        {
            return file.FileUploads.Select(x => (x.FileName, x.FileBytes)).ToList()!;
        }
        return [];
    }

    public async Task<byte[]> OpenFileDataAsync(params string[] fileExts)
    {
        var fileContent = await OpenFileTextAsync(fileExts);
        return fileContent is not null ? System.Text.Encoding.UTF8.GetBytes(fileContent) : [];
    }

    public async Task<string?> SaveFileAsync(string fileName, string file)
    {
        var confirmed = await dialogService.OpenAsync<SavePromptWindow>("", options: new DialogOptions { Draggable = true, ShowClose = false, Resizable = true, ShowTitle = false });
        if (string.IsNullOrWhiteSpace(confirmed?.ToString())) return null;
        _prompts = (await localStorage.GetAsync<Dictionary<string, string>>(Key)).Value ?? [];
        _prompts[confirmed] = file;
        await localStorage.SetAsync(Key, _prompts);
        return $"File {confirmed} Saved";
    }

    public async void ZoomChanged(double zoom)
    {
        await jsRuntime.InvokeVoidAsync("toggleZoomScreen", $"{zoom}");
    }

    public async Task SaveUserSettings(UserProfile userProfile)
    {
        Console.WriteLine(
            $"User Profile Saved:\n{JsonSerializer.Serialize(userProfile, _jsonSerializerOptions)}\n----------------\n");
        await localStorage.SetAsync("UserProfile", userProfile);
    }

    public async Task<UserProfile> LoadUserSettings()
    {
        var profileResult = await localStorage.GetAsync<UserProfile>("UserProfile");
        var profile = profileResult.Value;
        Console.WriteLine(
            $"User Profile Loaded:\n{JsonSerializer.Serialize(profile, _jsonSerializerOptions)}\n----------------\n");

        return profile ?? new UserProfile();

    }


}