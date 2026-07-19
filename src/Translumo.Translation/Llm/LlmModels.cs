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
}
