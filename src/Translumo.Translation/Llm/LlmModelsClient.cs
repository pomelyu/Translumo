using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Translumo.Utils.Http;

namespace Translumo.Translation.Llm
{
    public static class LlmModelsClient
    {
        public static async Task<IReadOnlyList<string>> GetAvailableModelsAsync(string serverUrl, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                return Array.Empty<string>();
            }

            try
            {
                var reader = new HttpReader()
                {
                    Accept = "application/json",
                    ContentType = "application/json",
                    UserAgent = "Translumo",
                    ThrowExceptions = false
                };
                if (!string.IsNullOrEmpty(apiKey))
                {
                    reader.OptionalHeaders.Add("Authorization", $"Bearer {apiKey}");
                }

                HttpResponse response = await reader.RequestWebDataAsync(LlmProviderDescriptor.BuildModelsUrl(serverUrl), HttpMethods.GET)
                    .ConfigureAwait(false);
                if (!response.IsSuccessful || string.IsNullOrWhiteSpace(response.Body))
                {
                    return Array.Empty<string>();
                }

                LlmModelsResponse parsed = JsonSerializer.Deserialize<LlmModelsResponse>(response.Body);

                return parsed?.Data?.Select(model => model.Id)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToArray() ?? Array.Empty<string>();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }
    }
}
