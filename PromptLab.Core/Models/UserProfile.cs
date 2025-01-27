using PromptLab.Core.Helpers;
using System.Diagnostics;
namespace PromptLab.Core.Models;

public class UserProfile
{
    public string? ProfilePath { get; set; }
    public ChatSettings ChatSettings { get; set; } = new();
    public AppSettings AppSettings { get; set; } = new();
    public ChatModelSettings ModelSettings { get; set; } = new();
    public EmbeddingModelSettings EmbeddingModelSettings { get; set; } = new();
    public List<string> AvailableModels()
    {
        var models = new List<string>();
        if (ModelSettings.OpenAIModelType == OpenAIModelType.OpenAI)
        {
            models.AddRange(SettingsOptions.OpenAIAvailableModels);
        }
        else
        {
            models.AddRange([ModelSettings.AzureOpenAIGpt35DeploymentName ?? "", ModelSettings.AzureOpenAIGpt4DeploymentName ?? ""]);
        }
        if (!string.IsNullOrEmpty(ModelSettings.GoogleGeminiApiKey))
        {
            models.AddRange(SettingsOptions.GeminiAIAvailableModels);
        }
        if (!string.IsNullOrEmpty(ModelSettings.AnthropicApiKey))
        {
            models.AddRange(SettingsOptions.AnthropicAIAvailableModels);
        }
        if (!string.IsNullOrEmpty(ModelSettings.MistralApiKey))
        {
            models.AddRange(SettingsOptions.MistralAIAvailableModels);
        }
        if (!string.IsNullOrEmpty(ModelSettings.DeepseekApiKey))
        {
            models.AddRange(SettingsOptions.DeepseekModels);
        }
        if (HasOllama())
        {
            models.AddRange(SettingsOptions.LocalOllamaModels);
        }
        return models;
    }

    private bool HasOllama()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ollama",
                    Arguments = "-v",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}