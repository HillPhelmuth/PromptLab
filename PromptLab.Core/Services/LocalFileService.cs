namespace PromptLab.Core.Services;

public class LocalFileService : IFileService
{
    public event Action? PickFile;
    public event Action? PickFolder;
    public event Action<string, string>? SaveFile;
    public event Action<double>? Zoom;
    private TaskCompletionSource<string?> Tcs { get; set; } = new();
    public void FilePicked(string filePath)
    {
        Tcs.TrySetResult(filePath);
    }
    public async Task<string?> OpenFileAsync(string fileName = "")
    {
        PickFile?.Invoke();
        var result = await Tcs.Task;
        Tcs = new TaskCompletionSource<string?>();
        if (string.IsNullOrEmpty(result)) return "";
        var text = await File.ReadAllTextAsync(result);
        return text;
    }
    //public void FolderPicked(string folderPath)
    //{
    //    Tcs.TrySetResult(folderPath);
    //}
    //public async Task<string?> PickFolderAsync()
    //{
    //    PickFolder?.Invoke();
    //    var result = await Tcs.Task;
    //    Tcs = new TaskCompletionSource<string?>();
    //    return result;
    //}
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
}