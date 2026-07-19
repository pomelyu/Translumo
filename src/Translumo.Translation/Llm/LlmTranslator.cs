using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Language;
using Translumo.Translation.Configuration;
using Translumo.Translation.Exceptions;
using Translumo.Utils.Http;

namespace Translumo.Translation.Llm
{
    public sealed class LlmTranslator : BaseTranslator<LlmContainer>
    {
        private readonly LlmTranslationConfiguration _llmConfiguration;
        private readonly LlmProviderDescriptor _providerDescriptor;
        private readonly string _systemPrompt;

        public LlmTranslator(TranslationConfiguration translationConfiguration, LlmTranslationConfiguration llmConfiguration,
            LanguageService languageService, ILogger logger)
            : base(translationConfiguration, languageService, logger)
        {
            _llmConfiguration = llmConfiguration;
            _providerDescriptor = LlmProviderDescriptor.Get(llmConfiguration.Provider);
            _systemPrompt = BuildSystemPrompt(llmConfiguration.SystemPromptTemplate);
        }

        protected override async Task<string> TranslateTextInternal(LlmContainer container, string sourceText)
        {
            var requestUrl = GetChatCompletionsUrl();

            var request = new LlmChatRequest()
            {
                Model = _llmConfiguration.Model,
                Temperature = 0.2f,
                ChatTemplateKwargs = _providerDescriptor.RequiresServerUrl
                    ? new Dictionary<string, object>() { ["enable_thinking"] = false }
                    : null,
                Messages = new List<LlmChatMessage>()
                {
                    new LlmChatMessage() { Role = "system", Content = _systemPrompt },
                    new LlmChatMessage() { Role = "user", Content = sourceText }
                }
            };

            string dataIn = JsonSerializer.Serialize(request);
            HttpResponse httpResponse = await container.Reader.RequestWebDataAsync(requestUrl, HttpMethods.POST, dataIn)
                .ConfigureAwait(false);

            if (!httpResponse.IsSuccessful)
            {
                throw new TranslationException(BuildErrorMessage(httpResponse), httpResponse.InnerException);
            }

            LlmChatResponse response = JsonSerializer.Deserialize<LlmChatResponse>(httpResponse.Body);
            var translation = response?.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrWhiteSpace(translation))
            {
                throw new TranslationException($"Unexpected LLM response body: '{httpResponse.Body}'");
            }

            return translation.Trim();
        }

        protected override IList<LlmContainer> CreateContainers(TranslationConfiguration configuration)
        {
            var result = configuration.ProxySettings.Select(proxy => new LlmContainer(_llmConfiguration.ApiKey, proxy)).ToList();
            result.Add(new LlmContainer(_llmConfiguration.ApiKey, isPrimary: true));

            return result;
        }

        private string GetChatCompletionsUrl()
        {
            if (_providerDescriptor.RequiresServerUrl)
            {
                if (string.IsNullOrWhiteSpace(_llmConfiguration.ServerUrl))
                {
                    throw new TranslationException("LLM server URL is not configured. Enter it in the LLM settings tab.");
                }

                return LlmProviderDescriptor.BuildChatCompletionsUrl(_llmConfiguration.ServerUrl);
            }

            if (string.IsNullOrWhiteSpace(_llmConfiguration.ApiKey))
            {
                throw new TranslationException("LLM API key is not configured. Enter it in the LLM settings tab.");
            }

            return _providerDescriptor.ChatCompletionsUrl;
        }

        private string BuildSystemPrompt(string template)
        {
            var prompt = string.IsNullOrWhiteSpace(template) ? LlmTranslationConfiguration.DEFAULT_SYSTEM_PROMPT : template;

            return prompt.Replace("{from}", GetLanguageName(SourceLangDescriptor))
                .Replace("{to}", GetLanguageName(TargetLangDescriptor));
        }

        private static string GetLanguageName(LanguageDescriptor descriptor)
        {
            return Regex.Replace(descriptor.Language.ToString(), @"(\B[A-Z])", " $1");
        }

        private string BuildErrorMessage(HttpResponse httpResponse)
        {
            var message = $"{_llmConfiguration.Provider} API request failed";
            if (httpResponse.InnerException is WebException webEx)
            {
                if (webEx.Response is HttpWebResponse webResponse)
                {
                    message += $" (HTTP {(int)webResponse.StatusCode})";
                }

                var errorBody = TryReadErrorBody(webEx);
                if (!string.IsNullOrEmpty(errorBody))
                {
                    message += $": {errorBody}";
                }
            }
            else if (httpResponse.InnerException != null)
            {
                message += $": {httpResponse.InnerException.Message}";
            }

            return message;
        }

        private static string TryReadErrorBody(WebException webEx)
        {
            const int MAX_ERROR_BODY_LENGTH = 300;
            try
            {
                using var stream = webEx.Response?.GetResponseStream();
                if (stream == null)
                {
                    return null;
                }

                using var reader = new StreamReader(stream);
                var body = reader.ReadToEnd();

                return body.Length > MAX_ERROR_BODY_LENGTH ? body.Substring(0, MAX_ERROR_BODY_LENGTH) : body;
            }
            catch
            {
                return null;
            }
        }
    }
}
