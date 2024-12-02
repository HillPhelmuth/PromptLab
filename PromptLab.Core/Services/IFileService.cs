using PromptLab.Core.Models;

namespace PromptLab.Core.Services;

public interface IFileService
{
    Task<string?> OpenFileAsync(string fileName = "");
    Task<string?> SaveFileAsync(string fileName, string file);
    void ZoomChanged(double zoom);
    Task SaveUserSettings(UserProfile userProfile);
    Task<UserProfile> LoadUserSettings();
    Task<List<(string, byte[])>> OpenImageFileAsync();
}