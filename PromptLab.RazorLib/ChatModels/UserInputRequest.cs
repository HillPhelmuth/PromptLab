using PromptLab.Core.Models;

namespace PromptLab.RazorLib.ChatModels;

public class UserInputRequest
{
	public string? AskInput { get; set; }
	public string? ChatInput { get; set; }
	public UserInputType UserInputType { get; set;}
	public UserInputFieldType UserInputFieldType { get; set; }
	public string? ImageUrlInput { get; set; }
	public FileUpload? FileUpload { get; set; }
}