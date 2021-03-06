

namespace MockWebApi.Templating
{
    /// <summary>
    /// This is the base class for the tokens generated by the script template parser.
    /// </summary>
    public class StringFragment : Fragment
    {

        private readonly string _fragmentText;

        public string Text { get { return _fragmentText; } }

        public StringFragment(string value)
        {
            _fragmentText = value;
        }

        public override bool Equals(object? obj)
        {
            if (obj is StringFragment stringFragment)
            {
                return _fragmentText?.Equals(stringFragment._fragmentText) ?? false;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _fragmentText?.GetHashCode() ?? base.GetHashCode();
        }

    }
}