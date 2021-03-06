using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace HttpClient.Stub
{
    public class Behaviour
    {
        private Behaviour(string key,
                          Func<HttpRequestMessage, bool> predicate,
                          Func<HttpResponseMessage> response)
        {
            Key = key;
            Predicate = predicate;
            Response = response;
        }

        public string Key { get; }
        public Func<HttpRequestMessage, bool> Predicate { get; }
        public Func<HttpResponseMessage> Response { get; }

        public static Behaviour Create(string key,
                                       Func<HttpRequestMessage, bool> predicate,
                                       Func<HttpResponseMessage> response)
            => new(key, predicate, response);

        public static Behaviour CreateWithJsonResponse<T>(string key,
                                                          Func<HttpRequestMessage, bool> predicate,
                                                          T jsonContent)
            => Create(key,
                      predicate,
                      () => new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent(JsonSerializer.Serialize(jsonContent))});
    }
}