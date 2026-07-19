using Translumo.Translation.Llm;
using Translumo.Utils;

namespace Translumo.Translation.Configuration
{
    public class LlmTranslationConfiguration : BindableBase
    {
        public const string DEFAULT_SYSTEM_PROMPT = "Translate the user's text from {from} to {to}. Keep character names and terms consistent, " +
                                                    "fix obvious OCR artifacts, and output ONLY the translation with no explanations.";

        public const string DEFAULT_SERVER_URL = "http://127.0.0.1:8080";

        public static LlmTranslationConfiguration Default => new LlmTranslationConfiguration()
        {
            Provider = LlmProviders.Gemini,
            Model = "gemini-2.5-flash-lite",
            ApiKey = string.Empty,
            ServerUrl = DEFAULT_SERVER_URL,
            SystemPromptTemplate = DEFAULT_SYSTEM_PROMPT
        };

        public LlmProviders Provider
        {
            get => _provider;
            set
            {
                SetProperty(ref _provider, value);
            }
        }

        public string Model
        {
            get => _model;
            set
            {
                SetProperty(ref _model, value);
            }
        }

        public string ApiKey
        {
            get => _apiKey;
            set
            {
                SetProperty(ref _apiKey, value);
            }
        }

        public string ServerUrl
        {
            get => _serverUrl;
            set
            {
                SetProperty(ref _serverUrl, value);
            }
        }

        public string SystemPromptTemplate
        {
            get => _systemPromptTemplate;
            set
            {
                SetProperty(ref _systemPromptTemplate, value);
            }
        }

        private LlmProviders _provider;
        private string _model;
        private string _apiKey;
        private string _serverUrl;
        private string _systemPromptTemplate;
    }
}
