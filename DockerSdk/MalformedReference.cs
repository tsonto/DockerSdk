using System;

namespace DockerSdk
{
    [Serializable]
    public class MalformedReferenceException : DockerException
    {
        public MalformedReferenceException()
        {
        }

        public MalformedReferenceException(string message) : base(message)
        {
        }

        public MalformedReferenceException(string message, Exception inner) : base(message, inner)
        {
        }

        protected MalformedReferenceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
