using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptLab.Core.Helpers
{
    public class SettingsOptions
    {
        public static List<string> OpenAIAvailableModels => ["gpt-4o-mini", "gpt-4o", "gpt-4o-2024-11-20", "chatgpt-4o-latest", "gpt-3.5-turbo"];
        public static List<string> MistralAIAvailableModels => ["open-mistral-nemo", "open-mixtral-8x7b", "open-mixtral-8x22b", "mistral-small-latest", "mistral-medium-latest", "mistral-large-latest"];
        public static List<string> GeminiAIAvailableModels => ["gemini-1.5-pro-002",  "gemini-1.5-flash-002", "gemini-1.5-pro", "gemini-exp-1121"];
    }
}
