using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using HttpClient.Stub.Exceptions;

namespace HttpClient.Stub
{
    public class DelegatingHandlerBuilder
    {
        private readonly IDictionary<string, KeyValuePair<Func<HttpRequestMessage, bool>, Func<HttpResponseMessage>>> _predicates
            = new Dictionary<string, KeyValuePair<Func<HttpRequestMessage, bool>, Func<HttpResponseMessage>>>();

        private DelegatingHandlerRequestListener _delegatingHandlerRequestListener = new();

        private DelegatingHandlerBuilder()
        {
        }

        public static DelegatingHandlerBuilder Create => new();

        public DelegatingHandlerBuilder With(DelegatingHandlerRequestListener listener)
        {
            _delegatingHandlerRequestListener = listener;
            return this;
        }

        /// <summary>
        /// Adds a SendAsync behaviour based on a behaviour.
        /// </summary>
        /// <param name="behaviour">The rules describing request scenario to respond to</param>
        /// <returns>The DelegatingHandlerBuilder allowing for fluent syntax</returns>
        public DelegatingHandlerBuilder With(Behaviour behaviour)
            => With(behaviour.Key,
                    behaviour.Predicate,
                    behaviour.Response);

        private DelegatingHandler Build()
            => new DelegateHandlerStub(_predicates.Values,
                                       _delegatingHandlerRequestListener,
                                       _predicates.Keys);

        public static implicit operator DelegatingHandler(DelegatingHandlerBuilder builder)
            => builder.Build();

        /// <summary>
        /// Adds a SendAsync behaviour based on a behaviour.
        /// </summary>
        /// <param name="key">The key the behaviour is registered as</param>
        /// <param name="predicate">The rules describing request scenario to respond to</param>
        /// <param name="response">the response returned when matched on predicate</param>
        /// <returns>The DelegateHandler allowing for fluent syntax</returns>
        public DelegatingHandlerBuilder With(string key,
                                             Func<HttpRequestMessage, bool> predicate,
                                             Func<HttpResponseMessage> response)
        {
            if(_predicates.ContainsKey(key))
            {
                throw DuplicateDelegatingHandlerBehaviour.Create(key);
            }

            _predicates.Add(key, new KeyValuePair<Func<HttpRequestMessage, bool>, Func<HttpResponseMessage>>(predicate, response));

            return this;
        }

        private class DelegateHandlerStub : DelegatingHandler
        {
            private readonly IEnumerable<KeyValuePair<Func<HttpRequestMessage, bool>, Func<HttpResponseMessage>>> _predicates;
            private readonly IEnumerable<string> _registeredKeys;
            private readonly DelegatingHandlerRequestListener _requestListener;

            public DelegateHandlerStub(IEnumerable<KeyValuePair<Func<HttpRequestMessage, bool>, Func<HttpResponseMessage>>> predicates,
                                       DelegatingHandlerRequestListener requestListener,
                                       IEnumerable<string> registeredKeys)
            {
                _predicates = predicates;
                _requestListener = requestListener;
                _registeredKeys = registeredKeys;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                         CancellationToken cancellationToken)
            {
                await _requestListener.With(request).ConfigureAwait(false);

                var relevantResponses = FindRelevantResponses(request).ToList();

                if(!relevantResponses.Any())
                    throw NoDelegatingHandlerBehaviour.Create(request, _registeredKeys);

                return relevantResponses.Last().Invoke();
            }

            private IEnumerable<Func<HttpResponseMessage>> FindRelevantResponses(HttpRequestMessage requestMessage)
                => _predicates.Where(pair => pair.Key.Invoke(requestMessage))
                              .Select(pair => pair.Value);
        }
    }
}