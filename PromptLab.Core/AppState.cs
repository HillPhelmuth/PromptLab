using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PromptLab.Core.Models;
using ReverseMarkdown;

namespace PromptLab.Core;

public class AppState : INotifyPropertyChanged
{
	private string? _userName;
	private ChatHistory _chatMessages = [];
	private bool _isLogProbView;
	private List<TokenString> _tokenStrings = [];
	private AppSettings _appSettings = new();
	private bool _showTimestamps1;
	private ChatSettings _chatSettings = new();
	private string _activeSystemPrompt = """
                                        <h2>Instructions</h2>
                                        <p>You are a helpful AI assistant.</p>
                                        """;

	private string _activeSystemPromptMarkdown;
	private string _promptToSave = "";
	public event PropertyChangedEventHandler? PropertyChanged;

	public string? UserName
	{
		get => _userName;
		set => SetField(ref _userName, value);
	}

	public ChatHistory ChatMessages
	{
		get => _chatMessages;
		set => SetField(ref _chatMessages, value);
	}

	public bool IsLogProbView
	{
		get => _isLogProbView;
		set => SetField(ref _isLogProbView, value);
	}

	public bool ShowTimestamps
	{
		get => _showTimestamps1;
		set => SetField(ref _showTimestamps1, value);
	}

	public List<TokenString> TokenStrings
	{
		get => _tokenStrings;
		set => SetField(ref _tokenStrings, value);
	}

	public AppSettings AppSettings
	{
		get => _appSettings;
		set => SetField(ref _appSettings, value);
	}

	public ChatSettings ChatSettings
	{
		get => _chatSettings;
		set => SetField(ref _chatSettings, value);
	}

	public string ActiveSystemPromptHtml
	{
		get => _activeSystemPrompt;
		set 
		{
			SetField(ref _activeSystemPrompt, value);
			ChatSettings.SystemPrompt = ActiveSystemPromptAsMarkdown();
		}
	}

	public string PromptToSave
	{
		get => _promptToSave;
		set => SetField(ref _promptToSave, value);
	} 

	public string ActiveSystemPrompt => ActiveSystemPromptAsMarkdown();
	
	private string ActiveSystemPromptAsMarkdown()
	{
		var config = new Config
		{
			// Include the unknown tag completely in the result (default as well)
			UnknownTags = Config.UnknownTagsOption.PassThrough,
			// generate GitHub flavoured markdown, supported for BR, PRE and table tags
			GithubFlavored = true,
			// will ignore all comments
			RemoveComments = true,
			// remove markdown output for links where appropriate
			SmartHrefHandling = true
		};

		var converter = new Converter(config);
		return converter.Convert(ActiveSystemPromptHtml);
	}

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		Console.WriteLine($"AppState Property {propertyName} Changed");
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value)) return false;
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}