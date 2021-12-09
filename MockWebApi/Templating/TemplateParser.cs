using System.Collections.Generic;

namespace MockWebApi.Templating
{
    public class TemplateParser : ITemplateParser
    {

        public Template Parse(string text)
        {
            (string templateText, string[] parameters) = ParseIt(text);

            Template template = new Template()
            {
                FormatString = templateText,
                Parameters = parameters
            };

            return template;
        }

        private (string, string[]) ParseIt(string text)
        {
            List<string> parameters = new List<string>();
            string newText = "";

            int tempEndIndex = 0;
            int tempStartIndex = text.IndexOf("{{");
            while (tempStartIndex >= 0)
            {
                newText += text.Substring(tempEndIndex, tempStartIndex - tempEndIndex); // start and end are switch here, because we use the former end as a start.

                tempEndIndex = text.IndexOf("}}", tempStartIndex);

                if (tempEndIndex < tempStartIndex)
                {
                    throw new MalformedTemplateException($"There is an opening double-brace ({{{{) at position { tempStartIndex } but no corresponding closing double brance (}}}})");
                }

                newText += $"{{{ parameters.Count }}}";

                string parameter = text.Substring(tempStartIndex + 2, tempEndIndex - tempStartIndex - 2);
                parameters.Add(parameter.Trim());

                tempEndIndex += 2;
                tempStartIndex = text.IndexOf("{{", tempEndIndex);
            }

            newText += text.Substring(tempEndIndex, text.Length - tempEndIndex); // add the rest after the last occurance of the double-brace to the format string

            return (newText, parameters.ToArray());
        }

    }
}
