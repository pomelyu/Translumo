using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Translumo.Infrastructure.Language;
using Translumo.Translation.Configuration;
using Translumo.Translation.Exceptions;
using Translumo.Utils.Http;

namespace Translumo.Translation.Google
{
    // Cloud Translation Basic v2 REST API. "Google SDK" is the UI option name.
    public sealed class GoogleSdkTranslator : BaseTranslator<GoogleSdkContainer>
    {
        private const string TranslateUrl = "https://translation.googleapis.com/language/translate/v2";

        public GoogleSdkTranslator(TranslationConfiguration configuration, LanguageService languageService, ILogger logger)
            : base(configuration, languageService, logger)
        {
        }

        public override Task<string> TranslateTextAsync(string sourceText)
        {
            if (string.IsNullOrWhiteSpace(TranslationConfiguration.GoogleApiKey))
                throw new TranslationException("Google SDK API key is not configured. Enter API_KEY in the language settings.");

            return base.TranslateTextAsync(sourceText);
        }

        protected override async Task<string> TranslateTextInternal(GoogleSdkContainer container, string sourceText)
        {
            container.Reader.OptionalHeaders["x-goog-api-key"] = TranslationConfiguration.GoogleApiKey.Trim();
            var body = JsonSerializer.Serialize(new
            {
                q = new[] { sourceText },
                source = SourceLangDescriptor.Language == Languages.Chinese ? "zh-CN" : SourceLangDescriptor.IsoCode,
                target = TargetLangDescriptor.Language == Languages.Chinese ? "zh-CN" : TargetLangDescriptor.IsoCode,
                format = "text",
                model = "nmt"
            });
            var response = await container.Reader.RequestWebDataAsync(TranslateUrl, HttpMethods.POST, body)
                .ConfigureAwait(false);
            if (!response.IsSuccessful)
            {
                var status = (response.InnerException as WebException)?.Response as HttpWebResponse;
                var message = status == null
                    ? "Google SDK request failed. Check your connection and proxy settings."
                    : $"Google SDK request failed (HTTP {(int)status.StatusCode}). Check your API key, Cloud Translation API access and quota.";
                // Keep credentials and response bodies out of displayed/logged errors.
                throw new TranslationException(message);
            }

            try
            {
                using var document = JsonDocument.Parse(response.Body ?? string.Empty);
                if (document.RootElement.TryGetProperty("data", out var data) &&
                    data.ValueKind == JsonValueKind.Object &&
                    data.TryGetProperty("translations", out var translations) &&
                    translations.ValueKind == JsonValueKind.Array && translations.GetArrayLength() > 0 &&
                    translations[0].ValueKind == JsonValueKind.Object &&
                    translations[0].TryGetProperty("translatedText", out var text) &&
                    text.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(text.GetString()))
                {
                    return text.GetString();
                }
            }
            catch (JsonException)
            {
                throw new TranslationException("Google SDK returned an invalid JSON response.");
            }

            throw new TranslationException("Google SDK returned no translated text.");
        }

        protected override IList<GoogleSdkContainer> CreateContainers(TranslationConfiguration configuration)
        {
            var containers = configuration.ProxySettings.Select(proxy => new GoogleSdkContainer(proxy)).ToList();
            containers.Add(new GoogleSdkContainer(isPrimary: true));
            return containers;
        }
    }
}
