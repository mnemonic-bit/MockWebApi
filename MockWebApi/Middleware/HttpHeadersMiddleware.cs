using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MockWebApi.Middleware
{
    /// <summary>
    /// This middleware is intended to fill all HTTP headers.
    /// Currently this is not in use, and to make this of any
    /// value, there are at least two changes we have to make
    /// first:
    ///  1) instead of a HttpHeadersPolicy, we need a policy
    ///     provider, which differs by HTTP-route, because each
    ///     endpoint can have its own set of HTTP headers. This
    ///     is why it might be best to place this functionality
    ///     into the Mock-Rest-request middleware.
    ///  2) As HTTP responses are read-only once the header has
    ///     been written to the output stream, it will be too
    ///     late if the headers are added after the next middle-
    ///     ware was called.
    /// </summary>
    public class HttpHeadersMiddleware
    {

        public HttpHeadersMiddleware(RequestDelegate requestDelegate, HttpHeadersPolicy policy)
        {
            _requestDelegate = requestDelegate;
            _policy = policy;
        }

        public async Task Invoke(HttpContext context)
        {
            await Apply(context, _policy);
        }


        private readonly RequestDelegate _requestDelegate;
        private readonly HttpHeadersPolicy _policy;


        private async Task Apply(HttpContext context, HttpHeadersPolicy policy)
        {
            foreach (var headerPair in policy.HeadersToSet)
            {
                context.Response.Headers.Add(headerPair.Key, headerPair.Value);
            }

            foreach (var headerName in policy.HeadersToRemove)
            {
                context.Response.Headers.Remove(headerName);
            }

            await _requestDelegate(context);
        }

    }
}
