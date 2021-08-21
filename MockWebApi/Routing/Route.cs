using System.Collections.Generic;
using System.Linq;

namespace MockWebApi.Routing
{
    /// <summary>
    /// A Route represents a path on a server. It serves as
    /// template for actual paths that are used when requesting
    /// a resource on the server.
    /// </summary>
    public class Route
    {

        public Route(IEnumerable<Part> parts, IDictionary<string, string> parameters)
        {
            Parts = parts.ToArray();
            Parameters = new Dictionary<string, string>(parameters);
        }

        public IEnumerable<Part> Parts { get; }

        public IDictionary<string, string> Parameters { get; }

        /// <summary>
        /// This class models a part of a route, which is either
        /// a constant string representing a literal part of the URL
        /// between two slashes, or a variable, representing a variable
        /// part between one or more slashes.
        /// </summary>
        public abstract class Part
        {

        }

        public class LiteralPart : Part
        {

            public string Literal { get; }

            public LiteralPart(string literal)
            {
                Literal = literal;
            }

            public override string ToString()
            {
                return Literal;
            }

            public override bool Equals(object obj)
            {
                if (obj is LiteralPart literalPart)
                {
                    return Literal.Equals(literalPart.Literal);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return Literal.GetHashCode();
            }

        }

        public class VariablePart : Part
        {

            public string VariableName { get; }

            public VariablePart(string variableName)
            {
                VariableName = variableName;
            }

            public override string ToString()
            {
                return $"{{{VariableName}}}";
            }

            public override bool Equals(object obj)
            {
                if (obj is VariablePart variablePart)
                {
                    return VariableName.Equals(variablePart.VariableName);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return VariableName.GetHashCode();
            }

        }

    }
}
