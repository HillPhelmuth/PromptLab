namespace PromptLab.Core.Services;

public class FilePickerService
{
    public event Action? PickFile;
    public event Action? PickFolder;
    public event Action<double>? Zoom;
    private TaskCompletionSource<string?> Tcs { get; set; } = new();
    public void FilePicked(string filePath)
    {
        Tcs.TrySetResult(filePath);
    }
    public async Task<string?> PickFileAsync()
    {
        PickFile?.Invoke();
        var result = await Tcs.Task;
        Tcs = new TaskCompletionSource<string?>();
        return result;
    }
    public void FolderPicked(string folderPath)
    {
        Tcs.TrySetResult(folderPath);
    }
    public async Task<string?> PickFolderAsync()
    {
        PickFolder?.Invoke();
        var result = await Tcs.Task;
        Tcs = new TaskCompletionSource<string?>();
        return result;
    }
    public void ZoomChanged(double zoom)
    {
        Zoom?.Invoke(zoom);
    }
}