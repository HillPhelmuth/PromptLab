using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptLab.Core.Helpers
{
    public class SettingsOptions
    {
        public static List<string> OpenAIAvailableModels => ["gpt-3.5-turbo","gpt-4o", "gpt-4-turbo","gpt-3.5-turbo-1106","gpt-4-0125-preview", "gpt-4-1106-preview"];
        public static List<string> MistralAIAvailableModels => ["open-mistral-7b", "open-mixtral-8x7b", "open-mixtral-8x22b", "mistral-small-latest", "mistral-medium-latest", "mistral-large-latest"];
    }
}
