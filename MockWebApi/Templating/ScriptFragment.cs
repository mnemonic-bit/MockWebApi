

namespace MockWebApi.Templating
{
    /// <summary>
    /// This is the base class for the tokens generated by the script template parser.
    /// </summary>
    public class ScriptFragment : Fragment
    {

        private readonly string _scriptText;

        public string ScriptText { get { return _scriptText; } }

        public ScriptFragment(string scriptText)
        {
            _scriptText = scriptText;
        }

        public override bool Equals(object obj)
        {
            if (obj is ScriptFragment scriptFragment)
            {
                return _scriptText?.Equals(scriptFragment._scriptText) ?? false;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _scriptText?.GetHashCode() ?? base.GetHashCode();
        }

    }
}