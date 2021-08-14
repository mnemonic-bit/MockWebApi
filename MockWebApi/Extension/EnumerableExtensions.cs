using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;

namespace MockWebApi.Extension
{
    public static class EnumerableExtensions
    {

        public static Dictionary<string, string> ToDictionary(this IEnumerable<KeyValuePair<string, StringValues>> enumerable)
        {
            return new Dictionary<string, string>(enumerable.Select(q => new KeyValuePair<string, string>(q.Key, q.Value.ToString())));
        }

    }
}
