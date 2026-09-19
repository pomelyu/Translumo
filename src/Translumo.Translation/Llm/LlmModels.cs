using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Translumo.Translation.Llm
{
    public class LlmChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("messages")]
        public List<LlmChatMessage> Messages { get; set; }

        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }

        // llama.cpp/vLLM extension to disable reasoning on thinking models; must be omitted for OpenAI-hosted APIs
        [JsonPropertyName("chat_template_kwargs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object> ChatTemplateKwargs { get; set; }
    }

    public class LlmChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class LlmChatResponse
    {
        [JsonPropertyName("choices")]
        public List<LlmChatChoice> Choices { get; set; }
    }

    public class LlmChatChoice
    {
        [JsonPropertyName("message")]
        public LlmChatMessage Message { get; set; }
    }

    public class LlmModelsResponse
    {
        [JsonPropertyName("data")]
        public List<LlmModelInfo> Data { get; set; }
    }

    public class LlmModelInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("architecture")]
        public LlmModelArchitecture Architecture { get; set; }
    }

    public class LlmModelArchitecture
    {
        [JsonPropertyName("input_modalities")]
        public List<string> InputModalities { get; set; }

        [JsonPropertyName("output_modalities")]
        public List<string> OutputModalities { get; set; }
    }

    public class GeminiModelsResponse
    {
        [JsonPropertyName("models")]
        public List<GeminiModelInfo> Models { get; set; }

        [JsonPropertyName("nextPageToken")]
        public string NextPageToken { get; set; }
    }

    public class GeminiModelInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("supportedGenerationMethods")]
        public List<string> SupportedGenerationMethods { get; set; }
    }
}
