using System.Collections.Generic;

namespace Translumo.Translation.Llm
{
    public class LlmProviderDescriptor
    {
        public LlmProviders Provider { get; }
        public string ChatCompletionsUrl { get; }
        public IReadOnlyList<string> PresetModels { get; }

        private LlmProviderDescriptor(LlmProviders provider, string chatCompletionsUrl, IReadOnlyList<string> presetModels)
        {
            Provider = provider;
            ChatCompletionsUrl = chatCompletionsUrl;
            PresetModels = presetModels;
        }

        public static LlmProviderDescriptor Get(LlmProviders provider)
        {
            return Descriptors[provider];
        }

        private static readonly Dictionary<LlmProviders, LlmProviderDescriptor> Descriptors = new()
        {
            [LlmProviders.OpenAI] = new LlmProviderDescriptor(LlmProviders.OpenAI,
                "https://api.openai.com/v1/chat/completions",
                new[] { "gpt-4.1-nano", "gpt-4.1-mini", "gpt-4o-mini" }),
            [LlmProviders.Gemini] = new LlmProviderDescriptor(LlmProviders.Gemini,
                "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions",
                new[] { "gemini-2.5-flash-lite", "gemini-2.5-flash", "gemini-2.0-flash" }),
            [LlmProviders.OpenRouter] = new LlmProviderDescriptor(LlmProviders.OpenRouter,
                "https://openrouter.ai/api/v1/chat/completions",
                new[] { "google/gemini-2.5-flash-lite", "openai/gpt-4.1-nano", "deepseek/deepseek-chat-v3-0324:free", "qwen/qwen3-235b-a22b:free" })
        };
    }
}
