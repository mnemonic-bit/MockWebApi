using System.Collections.Generic;

namespace MockWebApi.Middleware
{
    public class HttpHeadersPolicy
    {

        public static HttpHeadersPolicy Empty { get; } = new HttpHeadersPolicy();

        public IDictionary<string, string> HeadersToSet { get; } = new Dictionary<string, string>();

        public ISet<string> HeadersToRemove { get; } = new HashSet<string>();

    }
}
