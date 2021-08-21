using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.Extension
{
    public static class CollectionExtensions
    {

        public static ICollection<TElem> AddAll<TElem>(this ICollection<TElem> collection, IEnumerable<TElem> enumerable)
        {
            foreach(TElem elem in enumerable)
            {
                collection.Add(elem);
            }

            return collection;
        }


    }
}
