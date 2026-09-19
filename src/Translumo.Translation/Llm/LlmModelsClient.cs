using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Translumo.Utils.Http;

namespace Translumo.Translation.Llm
{
    public static class LlmModelsClient
    {
        public static Task<IReadOnlyList<string>> GetAvailableModelsAsync(string serverUrl, string apiKey)
        {
            return GetAvailableModelsAsync(LlmProviders.Custom, serverUrl, apiKey);
        }

        public static async Task<IReadOnlyList<string>> GetAvailableModelsAsync(LlmProviders provider, string serverUrl, string apiKey)
        {
            if (provider == LlmProviders.Custom && string.IsNullOrWhiteSpace(serverUrl))
            {
                throw new InvalidOperationException("Server URL is required.");
            }
            if (provider != LlmProviders.Custom && string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("API key is required.");
            }

            var url = provider switch
            {
                LlmProviders.Gemini => "https://generativelanguage.googleapis.com/v1beta/models?pageSize=1000",
                LlmProviders.OpenAI => "https://api.openai.com/v1/models",
                LlmProviders.OpenRouter => "https://openrouter.ai/api/v1/models",
                LlmProviders.Custom => LlmProviderDescriptor.BuildModelsUrl(serverUrl),
                _ => throw new ArgumentOutOfRangeException(nameof(provider))
            };
            var reader = new HttpReader()
            {
                Accept = "application/json",
                ContentType = "application/json",
                UserAgent = "Translumo",
                ThrowExceptions = false
            };
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                reader.OptionalHeaders.Add(provider == LlmProviders.Gemini ? "x-goog-api-key" : "Authorization",
                    provider == LlmProviders.Gemini ? apiKey.Trim() : $"Bearer {apiKey.Trim()}");
            }

            var models = new List<string>();
            var pageTokens = new HashSet<string>(StringComparer.Ordinal);
            var nextUrl = url;
            do
            {
                HttpResponse response = await reader.RequestWebDataAsync(nextUrl, HttpMethods.GET)
                    .ConfigureAwait(false);
                if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Body))
                {
                    var status = (response.InnerException as WebException)?.Response as HttpWebResponse;
                    // Do not expose response bodies or exception messages that might contain credentials.
                    throw new InvalidOperationException(status == null
                        ? "Unable to retrieve models. Check your connection and settings."
                        : $"Unable to retrieve models (HTTP {(int)status.StatusCode}).");
                }

                nextUrl = null;
                if (provider == LlmProviders.Gemini)
                {
                    var parsed = JsonSerializer.Deserialize<GeminiModelsResponse>(response.Body);
                    if (parsed?.Models == null)
                        throw new JsonException();
                    models.AddRange(parsed.Models
                        .Where(model => model?.SupportedGenerationMethods?.Contains("generateContent") == true)
                        .Select(model => model.Name?.StartsWith("models/", StringComparison.Ordinal) == true
                            ? model.Name.Substring("models/".Length) : model.Name));
                    if (!string.IsNullOrEmpty(parsed.NextPageToken))
                    {
                        if (!pageTokens.Add(parsed.NextPageToken))
                            throw new JsonException();
                        nextUrl = $"{url}&pageToken={Uri.EscapeDataString(parsed.NextPageToken)}";
                    }
                }
                else
                {
                    var parsed = JsonSerializer.Deserialize<LlmModelsResponse>(response.Body);
                    if (parsed?.Data == null)
                        throw new JsonException();
                    models.AddRange(parsed.Data
                        .Where(model => model != null && (provider != LlmProviders.OpenRouter ||
                            (model.Architecture?.InputModalities?.Contains("text") == true &&
                             model.Architecture?.OutputModalities?.Contains("text") == true)))
                        .Select(model => model.Id));
                }
            } while (nextUrl != null);

            return models.Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal).ToArray();
        }
    }
}
