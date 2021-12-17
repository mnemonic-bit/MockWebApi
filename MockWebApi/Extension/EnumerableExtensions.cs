using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Primitives;

namespace MockWebApi.Extension
{
    public static class EnumerableExtensions
    {

        public static TAgg Aggregate<TElem, TAgg>(this IEnumerable<TElem> enumerable, TAgg seed, Func<TAgg, TElem, bool, TAgg> aggregateFn)
        {
            IEnumerator<TElem> enumerator = enumerable.GetEnumerator();

            bool hasNextElement = enumerator.MoveNext();
            while (hasNextElement)
            {
                TElem currentElement = enumerator.Current;
                hasNextElement = enumerator.MoveNext();
                seed = aggregateFn(seed, currentElement, !hasNextElement);
            }

            return seed;
        }

        public static Dictionary<string, string> ToDictionary(this IEnumerable<KeyValuePair<string, StringValues>> enumerable)
        {
            return new Dictionary<string, string>(enumerable.Select(q => new KeyValuePair<string, string>(q.Key, q.Value.ToString())));
        }

    }
}
