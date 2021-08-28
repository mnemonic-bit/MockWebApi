using System;
using System.Collections.Generic;
using System.Linq;

namespace MockWebApi.Extension
{
    public static class StringExtensions
    {

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

    }
}
