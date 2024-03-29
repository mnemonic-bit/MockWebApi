﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MockWebApi.Extension
{
    public static class CollectionExtensions
    {

        public static ICollection<TElem>? AddAll<TElem>(this ICollection<TElem>? collection, IEnumerable<TElem> enumerable)
        {
            if (collection == null)
            {
                return null;
            }

            if (enumerable == null)
            {
                return collection;
            }

            foreach (TElem elem in enumerable)
            {
                collection.Add(elem);
            }

            return collection;
        }

        public static bool IsNullOrEmpty<TElem>([NotNullWhen(false)] this ICollection<TElem>? collection)
        {
            return collection == null || collection.Count == 0;
        }

    }
}
