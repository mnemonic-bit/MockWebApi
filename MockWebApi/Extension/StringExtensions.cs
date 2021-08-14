using System;

namespace MockWebApi.Extension
{
    public static class StringExtensions
    {

        public static T ParseInto<T>(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default(T);
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

    }
}
