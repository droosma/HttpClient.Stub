using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClient.Stub
{
    public class DelegatingHandlerRequestListener
    {
        private readonly Dictionary<HttpRequestMessage, string> _dictionary;

        public DelegatingHandlerRequestListener()
        {
            _dictionary = new Dictionary<HttpRequestMessage, string>();
        }

        public IReadOnlyDictionary<HttpRequestMessage, string> RequestMessages => _dictionary;

        public async Task With(HttpRequestMessage requestMessage)
        {
            if(requestMessage.Content == null)
                return;

            var content = await requestMessage.Content
                                              .ReadAsStringAsync()
                                              .ConfigureAwait(false);
            _dictionary.Add(requestMessage, content);
        }
    }
}