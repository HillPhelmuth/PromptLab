using PromptLab.Core.Helpers;

namespace PromptLab.Core.Models
{
    public class UserProfile
    {
        public string? ProfilePath { get; set; }
        public ChatSettings ChatSettings { get; set; } = new();
        public AppSettings AppSettings { get; set; } = new();
        public ModelSettings ModelSettings { get; set; } = new();
        public List<string> AvailableModels()
        {
            var models = new List<string>();
            if (ModelSettings.OpenAIModelType == OpenAIModelType.OpenAI)
            {
                models.AddRange(SettingsOptions.AvailableModels);
			}
			else
            {
	            models.AddRange([ModelSettings.AzureOpenAIGpt35DeploymentName ?? "", ModelSettings.AzureOpenAIGpt4DeploymentName ?? ""]);
			}
            if (!string.IsNullOrEmpty(ModelSettings.GoogleGeminiApiKey))
            {
                models.Add("gemini-1.0-pro");
            }
            return models;
        }
    }
}
