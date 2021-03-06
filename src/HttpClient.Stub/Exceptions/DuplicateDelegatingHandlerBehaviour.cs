using System;
using System.Runtime.Serialization;

namespace HttpClient.Stub.Exceptions
{
    [Serializable]
    public class DuplicateDelegatingHandlerBehaviour : Exception
    {
        protected DuplicateDelegatingHandlerBehaviour(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private DuplicateDelegatingHandlerBehaviour(string key)
            : base($"behaviour with `{key}` already defined")
        {
        }

        public static DuplicateDelegatingHandlerBehaviour Create(Behaviour behaviour) => new(behaviour.Key);
        public static DuplicateDelegatingHandlerBehaviour Create(string key) => new(key);
    }
}