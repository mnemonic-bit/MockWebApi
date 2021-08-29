using System;
using System.Runtime.Serialization;

namespace MockWebApi.Templating
{
    public class MalformedTemplateException : Exception
    {

        public MalformedTemplateException() : base() { }

        public MalformedTemplateException(string message) : base(message) { }

        public MalformedTemplateException(string message, Exception innerException) : base(message, innerException) { }

        public MalformedTemplateException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    }
}
