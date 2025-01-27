using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptLab.Core.Helpers;

public class SettingsOptions
{
    public static List<string> OpenAIAvailableModels => ["gpt-4o-mini", "gpt-4o",  "o1-mini", "o1", "gpt-4o-2024-11-20", "chatgpt-4o-latest", "o1-preview-2024-09-12", "gpt-3.5-turbo"];
    public static List<string> MistralAIAvailableModels => ["open-mistral-nemo", "mistral-small-latest", "pixtral-large-latest", "mistral-medium-latest", "mistral-large-latest"];
    public static List<string> GeminiAIAvailableModels => ["gemini-1.5-pro-002",  "gemini-1.5-flash-002", "gemini-1.5-pro", "gemini-exp-1206", "gemini-2.0-flash-thinking-exp", "gemini-2.0-flash-exp"];

    public static List<string> AnthropicAIAvailableModels =>
        ["anthropic.claude-3-5-sonnet-20241022-v2:0", "anthropic.claude-3-5-haiku-20241022-v1:0"];
    public static List<string> LocalOllamaModels => [ "local-ollama-llama3.2", "local-ollama-llama3"];
    public static List<string> LocalLMStudioModels => ["local-lmstudio-model"];
    public static List<string> DeepseekModels => ["deepseek-chat", "deepseek-reasoner"];
}