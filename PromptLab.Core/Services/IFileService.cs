namespace PromptLab.Core.Services;

public interface IFileService
{
    Task<string?> OpenFileAsync(string fileName = "");
    Task<string?> SaveFileAsync(string fileName, string file);
    void ZoomChanged(double zoom);
}