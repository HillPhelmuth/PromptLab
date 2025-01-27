using System.Text.Json;
using PromptLab.Core.Models;

namespace PromptLab.Core.Services;

public class LocalFileService : IFileService
{
    public event Action<string>? PickFile;
    public event Action? PickImageFile;
    public event Action<UserProfile>? SaveUserProfile;
    public event Action<string, string>? SaveFile;
    public event Action<double>? Zoom;
    public event Func<UserProfile>? LoadUserProfile;
    private TaskCompletionSource<string?> Tcs { get; set; } = new();
    
    private TaskCompletionSource<List<(string, byte[])>> ImageTcs { get; set; } = new();
    public void FilePicked(string filePath)
    {
        Tcs.TrySetResult(filePath);
    }
    public void ImagePicked(List<(string, byte[])> images)
    {
        ImageTcs.TrySetResult(images);
    }
    public async Task<string?> OpenFileTextAsync(params string[] fileExts)
    {
        const string defaultFilter = "txt files (*.txt)|*.txt|markdown files (*.md)|*.md|All files (*.*)|*.*";
        
        var filterString = fileExts.Length == 0 
            ? defaultFilter
            : string.Join("|", fileExts.Select(ext =>
              {
                  var extension = ext.TrimStart('.');
                  return $"{extension} files (*.{extension})|*.{extension}";
              })) + "|All files (*.*)|*.*";
        
        PickFile?.Invoke(filterString);
        var result = await Tcs.Task;
        Tcs = new TaskCompletionSource<string?>();
        if (string.IsNullOrEmpty(result)) return "";
        var text = await File.ReadAllTextAsync(result);
        return text;
    }

    public async Task<byte[]> OpenFileDataAsync(params string[] fileExts)
    {
        const string defaultFilter = "txt files (*.txt)|*.txt|markdown files (*.md)|*.md|All files (*.*)|*.*";

        var filterString = fileExts.Length == 0
            ? defaultFilter
            : string.Join("|", fileExts.Select(ext =>
            {
                var extension = ext.TrimStart('.');
                return $"{extension} files (*.{extension})|*.{extension}";
            })) + "|All files (*.*)|*.*";

        PickFile?.Invoke(filterString);
        var result = await Tcs.Task;
        Tcs = new TaskCompletionSource<string?>();
        if (string.IsNullOrEmpty(result)) return [];
        var fileBytes = await File.ReadAllBytesAsync(result);
        return fileBytes;
    }
    public Task SaveUserSettings(UserProfile userProfile)
    {
		SaveUserProfile?.Invoke(userProfile);
        return Task.CompletedTask;
	}
    public Task<UserProfile> LoadUserSettings()
    {
        var item = LoadUserProfile?.Invoke();
        return Task.FromResult(item ?? new UserProfile());
    }

    public async Task<List<(string, byte[])>> OpenImageFileAsync()
    {
        PickImageFile?.Invoke();
        var imageData = await ImageTcs.Task;
        ImageTcs = new TaskCompletionSource<List<(string, byte[])>>();
        return imageData;
    }

    public async Task<string?> SaveFileAsync(string fileName, string file)
    {
        SaveFile?.Invoke(fileName, file);
        var result = await Tcs.Task;
        Tcs = new TaskCompletionSource<string?>();
        return result;
    }
    public void ZoomChanged(double zoom)
    {
        Zoom?.Invoke(zoom);
    }

    public async Task<string> OpenFileFromPathAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return string.Empty;
            
        return await File.ReadAllTextAsync(filePath);
    }

    public async Task<string> GetFilePathAsync(params string[] fileExts)
    {
        const string defaultFilter = "txt files (*.txt)|*.txt|markdown files (*.md)|*.md|All files (*.*)|*.*";
        
        var filterString = fileExts.Length == 0 
            ? defaultFilter
            : string.Join("|", fileExts.Select(ext =>
              {
                  var extension = ext.TrimStart('.');
                  return $"{extension} files (*.{extension})|*.{extension}";
              })) + "|All files (*.*)|*.*";
        
        PickFile?.Invoke(filterString);
        var result = await Tcs.Task;
        Tcs = new TaskCompletionSource<string?>();
        return result ?? string.Empty;
    }
}