﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using PromptLab.Core.Models;

namespace PromptLab.RazorLib.ChatModels;

public class ChatState : INotifyPropertyChanged
{

    public event PropertyChangedEventHandler? PropertyChanged;

    public List<Message> ChatMessages { get; } = [];
    public ChatHistory ChatHistory { get; set; } = [];
    public int MessageCount => ChatMessages.Count;

    public void Reset()
    {
        ChatMessages.Clear();
        ChatHistory.Clear();
        MessagePropertyChanged();
    }
    public void AddUserMessage(string message, int? order = null)
    {
        order ??= MessageCount + 1;
        ChatMessages.Add(Message.UserMessage(message, order.Value));
        ChatHistory.AddUserMessage(message);
        MessagePropertyChanged();
    }
    public void AddUserMessage(TextContent text, ImageContent image, int? order = null)
    {
        order ??= MessageCount + 1;
        ChatMessages.Add(Message.UserMessage(text.Text!,image.Uri?.ToString() ?? "", order.Value));
        ChatHistory.Add(new ChatMessageContent(AuthorRole.User, [text, image]));
        MessagePropertyChanged();
    }
    public void AddAssistantMessage(string message, int? order = null, List<TokenString>? tokenStrings = null)
    {
        order ??= MessageCount + 1;
        var assistantMessage = Message.AssistantMessage(message, order.Value);
        if (tokenStrings is not null)
            assistantMessage.TokenStrings = tokenStrings;
        ChatMessages.Add(assistantMessage);
        ChatHistory.AddAssistantMessage(message);
        MessagePropertyChanged();
    }
    public void UpdateAssistantMessage(TokenString tokenString)
    {
	    if (ChatMessages.LastOrDefault()?.Role != Role.Assistant)
        {
            ChatMessages.Add(new Message(Role.Assistant, tokenString.StringValue, MessageCount + 1) { TokenStrings = [tokenString] });
            ChatHistory.AddAssistantMessage(tokenString.StringValue);
            MessagePropertyChanged();
            return;
		}
	    var lastMessage = ChatMessages.Last(x => x.Role == Role.Assistant);
        var lastHistory = ChatHistory.Last(x => x.Role == AuthorRole.Assistant);
        lastHistory.Content += tokenString.StringValue;
        lastMessage.TokenStrings.Add(tokenString);
        MessagePropertyChanged();
	}
    /// <summary>
    /// Updates the last assistant message with the token.
    /// </summary>
    /// <param name="token"></param>
    public void UpdateAssistantMessage(string token)
    {
        if (ChatMessages.All(x => x.Role != Role.Assistant)) throw new ArgumentOutOfRangeException(nameof(token),"No assistant message found.");
        var message = ChatMessages.Last(x => x.Role == Role.Assistant);
        message.Content += token;
        var lastChatMessageContent = ChatHistory.Last(x => x.Role == AuthorRole.Assistant);
        lastChatMessageContent.Content += token;
        MessagePropertyChanged();
    }
    /// <summary>
    /// Checks if last message is an assistant message and if so, appends the token to the message. Otherwise, adds a new assistant message.
    /// </summary>
    /// <param name="message"></param>
    public void UpsertAssistantMessage(string message)
    {
        if (ChatMessages.Last().Role == Role.Assistant)
        {
            UpdateAssistantMessage(message);
        }
        else
        {
            AddAssistantMessage(message);
        }
    }
    public void RemoveMessage(Message message)
    {
        var find = ChatMessages.Find(x => x.Order == message.Order);
        var removeIndex = ChatMessages.IndexOf(find ?? message);
        ChatHistory.RemoveAt(removeIndex);
        ChatMessages.Remove(find ?? message);
        MessagePropertyChanged();
    }
    private void MessagePropertyChanged()
    {
        OnPropertyChanged(nameof(ChatMessages));
        OnPropertyChanged(nameof(ChatHistory));
    }
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
       
}