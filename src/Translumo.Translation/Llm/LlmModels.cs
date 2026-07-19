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
    }
}
