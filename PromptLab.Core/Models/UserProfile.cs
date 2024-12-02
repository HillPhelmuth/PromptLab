using PromptLab.Core.Helpers;

namespace PromptLab.Core.Models
{
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
            if (!string.IsNullOrEmpty(ModelSettings.MistralApiKey))
            {
                models.AddRange(SettingsOptions.MistralAIAvailableModels);
            }
            return models;
        }
    }
}
