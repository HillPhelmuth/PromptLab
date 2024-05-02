namespace PromptLab.Core.Models;

public class FileUpload
{
    public string? FileBase64 { get; set; }
    public string? FileName { get; set; }
    public byte[] FileBytes { get; set; } = [];
}