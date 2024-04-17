namespace PromptLab.RazorLib.ChatModels;

public class UserInputRequest
{
	public string? AskInput { get; set; }
	public string? ChatInput { get; set; }
	public UserInputType UserInputType { get; set;}
	public UserInputFieldType UserInputFieldType { get; set; }
}