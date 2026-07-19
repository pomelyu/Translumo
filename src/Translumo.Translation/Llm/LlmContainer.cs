using Translumo.Translation.Configuration;
using Translumo.Utils.Http;

namespace Translumo.Translation.Llm
{
    public sealed class LlmContainer : TranslationContainer
    {
        public HttpReader Reader { get; private set; }

        private readonly string _apiKey;

        public LlmContainer(string apiKey, Proxy proxy = null, bool isPrimary = false) : base(proxy, isPrimary)
        {
            _apiKey = apiKey;
            Reader = CreateReader(proxy);
        }

        private HttpReader CreateReader(Proxy proxy)
        {
            var reader = new HttpReader();
            reader.ContentType = "application/json";
            reader.Accept = "application/json";
            reader.UserAgent = "Translumo";
            reader.ThrowExceptions = false;
            if (!string.IsNullOrEmpty(_apiKey))
            {
                reader.OptionalHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }

            reader.Proxy = proxy?.ToWebProxy();

            return reader;
        }
    }
}
