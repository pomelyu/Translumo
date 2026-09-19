using System;
using System.Collections.Generic;

namespace Translumo.Translation.Llm
{
    public class LlmProviderDescriptor
    {
        public LlmProviders Provider { get; }
        public string ChatCompletionsUrl { get; }
        public IReadOnlyList<string> PresetModels { get; }
        public string DefaultModel => PresetModels.Count > 0 ? PresetModels[0] : string.Empty;
        public bool RequiresServerUrl => ChatCompletionsUrl == null;

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

        public static string BuildChatCompletionsUrl(string serverUrl)
        {
            return $"{NormalizeBaseUrl(serverUrl)}/chat/completions";
        }

        public static string BuildModelsUrl(string serverUrl)
        {
            return $"{NormalizeBaseUrl(serverUrl)}/models";
        }

        private static string NormalizeBaseUrl(string serverUrl)
        {
            var baseUrl = serverUrl.Trim().TrimEnd('/');

            return baseUrl.EndsWith("/v1", StringComparison.OrdinalIgnoreCase) ? baseUrl : $"{baseUrl}/v1";
        }

        private static readonly Dictionary<LlmProviders, LlmProviderDescriptor> Descriptors = new()
        {
            [LlmProviders.OpenAI] = new LlmProviderDescriptor(LlmProviders.OpenAI,
                "https://api.openai.com/v1/chat/completions",
                new[] { "gpt-5.4-nano" }),
            [LlmProviders.Gemini] = new LlmProviderDescriptor(LlmProviders.Gemini,
                "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions",
                new[] { "gemini-flash-lite-latest" }),
            [LlmProviders.OpenRouter] = new LlmProviderDescriptor(LlmProviders.OpenRouter,
                "https://openrouter.ai/api/v1/chat/completions",
                new[] { "deepseek/deepseek-v4-flash-0731:free" }),
            [LlmProviders.Custom] = new LlmProviderDescriptor(LlmProviders.Custom,
                chatCompletionsUrl: null,
                Array.Empty<string>())
        };
    }
}
