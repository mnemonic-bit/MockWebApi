using System;
using System.Collections.Generic;
using System.Linq;

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

        public static string IndentLines(this string lines, string indention)
        {
            if (string.IsNullOrEmpty(lines))
            {
                return null;
            }

            IEnumerable<string> splitLines = lines
                .Split(new string[] { "\n\r", "\n", "\r" }, StringSplitOptions.None)
                .Select(l => $"{indention}{l}");

            return string.Join("\n", splitLines);
        }

        public static (string, string) SplitAt(this string str, int index)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str), $"No string instance is given to split.");
            }

            if (index < 0 || index > str.Length)
            {
                throw new ArgumentException(nameof(index), $"The position to split the given string, which is {index}, is outside the bounds of the string (0..{str.Length - 1}).");
            }

            return (str.Substring(0, index), str.Substring(index));
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

    }
}
