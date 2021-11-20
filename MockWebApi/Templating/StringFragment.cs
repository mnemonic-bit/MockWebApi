

namespace MockWebApi.Templating
{
    /// <summary>
    /// This is the base class for the tokens generated by the script template parser.
    /// </summary>
    public class StringFragment : Fragment
    {

        private string _fragmentText;

        public string Text { get { return _fragmentText; } }

        public StringFragment(string value)
        {
            _fragmentText = value;
        }

    }
}