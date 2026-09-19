using Translumo.Translation.Configuration;
using Translumo.Utils.Http;

namespace Translumo.Translation.Google
{
    public sealed class GoogleSdkContainer : TranslationContainer
    {
        public HttpReader Reader { get; }

        public GoogleSdkContainer(Proxy proxy = null, bool isPrimary = false) : base(proxy, isPrimary)
        {
            Reader = new HttpReader
            {
                Accept = "application/json",
                ContentType = "application/json; charset=utf-8",
                UserAgent = "Translumo",
                ThrowExceptions = false,
                Proxy = proxy?.ToWebProxy()
            };
        }
    }
}
