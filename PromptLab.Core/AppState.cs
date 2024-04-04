using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PromptLab.Core.Models;

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
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
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