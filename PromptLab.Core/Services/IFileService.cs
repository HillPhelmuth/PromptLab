using PromptLab.Core.Models;

namespace PromptLab.Core.Services;

public interface IFileService
{
    Task<string?> OpenFileTextAsync(params string[] fileExts);
    Task<string?> SaveFileAsync(string fileName, string file);
    void ZoomChanged(double zoom);
    Task SaveUserSettings(UserProfile userProfile);
    Task<UserProfile> LoadUserSettings();
    Task<List<(string, byte[])>> OpenImageFileAsync();
    Task<byte[]> OpenFileDataAsync(params string[] fileExts);
    Task<string> OpenFileFromPathAsync(string filePath);
    Task<string> GetFilePathAsync(params string[] fileExts);
}