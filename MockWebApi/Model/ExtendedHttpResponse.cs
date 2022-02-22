using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MockWebApi.Model
{
    internal class ExtendedHttpResponse : HttpResponse
    {
        public override Stream Body { get; set; } = Stream.Null;
        public override long? ContentLength { get; set; } = 0;
        public override string ContentType { get; set; } = "text/plain";

        public override IResponseCookies Cookies => throw new NotImplementedException();

        public override bool HasStarted => throw new NotImplementedException();

        public override IHeaderDictionary Headers => throw new NotImplementedException();

        public override HttpContext HttpContext => throw new NotImplementedException();

        public override int StatusCode { get; set; }

        public override void OnCompleted(Func<object, Task> callback, object state)
        {
            callback(state);
        }

        public override void OnStarting(Func<object, Task> callback, object state)
        {
            callback(state);
        }

        public override void Redirect(string location, bool permanent)
        {
            Redirect(location);
        }
    }
}
