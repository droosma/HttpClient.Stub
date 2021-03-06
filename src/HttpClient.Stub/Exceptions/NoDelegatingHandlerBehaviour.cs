using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization;

namespace HttpClient.Stub.Exceptions
{
    [Serializable]
    public class NoDelegatingHandlerBehaviour : Exception
    {
        private NoDelegatingHandlerBehaviour(string key,
                                             IEnumerable<string> registeredKeys)
            : base($"No behaviour configured for given request: `{key}`{Environment.NewLine}" +
                   $"Configured routes:{Environment.NewLine}" +
                   $"{string.Join(Environment.NewLine, registeredKeys)}")
        {
        }

        protected NoDelegatingHandlerBehaviour(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public static NoDelegatingHandlerBehaviour Create(HttpRequestMessage request,
                                                          IEnumerable<string> registeredKeys)
            => new(request.RequestUri.ToString(), registeredKeys);
    }
}