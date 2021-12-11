using System.Collections.Generic;

namespace MockWebApi.Templating
{
    public class TemplateParser : ITemplateParser
    {

        public Template Parse(string text)
        {
            Fragment[] fragments = ParseIt(text);

            Template template = new Template()
            {
                Fragments = fragments
            };

            return template;
        }

        private Fragment[] ParseIt(string text)
        {
            List<Fragment> fragments = new List<Fragment>();

            int tempEndIndex = 0;
            int tempStartIndex = text.IndexOf("{{");
            while (tempStartIndex >= 0)
            {
                fragments.Add(new StringFragment(text.Substring(tempEndIndex, tempStartIndex - tempEndIndex)));

                tempEndIndex = text.IndexOf("}}", tempStartIndex);

                if (tempEndIndex < tempStartIndex)
                {
                    throw new MalformedTemplateException($"There is an opening double-brace ({{{{) at position { tempStartIndex } but no corresponding closing double brance (}}}})");
                }

                string script = text.Substring(tempStartIndex + 2, tempEndIndex - tempStartIndex - 2);
                fragments.Add(new ScriptFragment(script.Trim()));

                tempEndIndex += 2;
                tempStartIndex = text.IndexOf("{{", tempEndIndex);
            }

            fragments.Add(new StringFragment(text.Substring(tempEndIndex, text.Length - tempEndIndex)));

            return fragments.ToArray();
        }

    }
}
