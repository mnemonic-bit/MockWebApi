using MockWebApi.Extension;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MockWebApi.Routing
{
    public class RouteGraphMatcher<TInfo> : IRouteMatcher<TInfo>
    {

        private RouteGraphNode _matchGraph;

        public RouteGraphMatcher()
        {
            _matchGraph = new RouteGraphNode();
        }

        public void AddRoute(string routeTemplate, TInfo routeInfo)
        {
            RouteParser routeParser = new RouteParser();
            Route route = routeParser.Parse(routeTemplate);
            AddRouteToMatchGraph(route, routeInfo);
        }

        public bool Remove(string routeTemplate)
        {
            throw new NotImplementedException();
        }

        public bool TryMatch(string route, out TInfo routeInfo)
        {
            RouteParser routeParser = new RouteParser();
            Route parsedRoute = routeParser.Parse(route);

            RouteGraphNode leafNode = parsedRoute.Parts.Aggregate(_matchGraph, (graph, part) => TryMatchPart(graph, part, out RouteGraphNode leafNode, out routeInfo));

            return false;
        }

        private void AddRouteToMatchGraph(Route route, TInfo routeInfo)
        {
            RouteGraphNode leafNode = route.Parts.Aggregate(_matchGraph, (graph, part, isLast) => AddPartToGraphNode(graph, part, routeInfo, isLast));
        }

        private RouteGraphNode AddPartToGraphNode(RouteGraphNode graph, Route.Part part, TInfo routeInfo, bool isLast)
        {
            if (part is Route.LiteralPart literalPart)
            {
                return AddLiteralPartToGraphNode(graph, literalPart, routeInfo, isLast);
            }
            else if (part is Route.VariablePart variablePart)
            {
                return AddVariablePartToGraphNode(graph, variablePart, routeInfo, isLast);
            }
            else
            {
                throw new InvalidOperationException($"This sub-class of Route.Part is not handled by this function.");
            }
        }

        private RouteGraphNode AddLiteralPartToGraphNode(RouteGraphNode graph, Route.LiteralPart literalPart, TInfo routeInfo, bool isLast)
        {
            if (!graph.Parts.TryGetValue(literalPart, out ICollection<(RouteGraphNode, TInfo, bool)> nodes))
            {
                nodes = new HashSet<(RouteGraphNode, TInfo, bool)>();
            }

            RouteGraphNode newGraphNode = new RouteGraphNode()
            {
                Parent = graph
            };
            nodes.Add((newGraphNode, routeInfo, isLast));

            return newGraphNode;
        }

        private RouteGraphNode AddVariablePartToGraphNode(RouteGraphNode graph, Route.VariablePart variablePart, TInfo routeInfo, bool isLast)
        {
            RouteGraphNode result = null;

            result = new RouteGraphNode()
            {
                Parent = graph
            };
            graph.Variables.Add((variablePart, routeInfo, isLast));
            graph.VariableNode ??= result;

            return result;
        }

        private bool TryMatchPart(RouteGraphNode graph, Route.Part part, out RouteGraphNode leafNode, out TInfo routeInfo)
        {
            if (part is Route.LiteralPart literalPart)
            {
                return TryMatchLiteralPart(graph, literalPart, out leafNode, out routeInfo);
            }
            else if (part is Route.VariablePart variablePart)
            {
                return TryMatchVariablePart(graph, variablePart, out leafNode, out routeInfo);
            }
            else
            {
                throw new InvalidOperationException($"This sub-class of Route.Part is not handled by this function.");
            }
        }

        private bool TryMatchLiteralPart(RouteGraphNode graph, Route.LiteralPart literalPart, out RouteGraphNode leafNode, out TInfo routeInfo)
        {
            leafNode = null;
            routeInfo = default(TInfo);

            if (!graph.Parts.TryGetValue(literalPart, out ICollection<(RouteGraphNode, TInfo, bool)> nodes))
            {
                return false;
            }



            return true;
        }

        private bool TryMatchVariablePart(RouteGraphNode graph, Route.VariablePart variablePart, out RouteGraphNode leafNode, out TInfo routeInfo)
        {
            leafNode = null;
            routeInfo = default(TInfo);


            return false;
        }

        /// <summary>
        /// A node in the node-graph is an instance which holds references
        /// to sub-nodes with each vertice being a labeled edge with an
        /// instance of Route.Part.
        /// </summary>
        private class RouteGraphNode
        {

            /// <summary>
            /// This is the reference to the parent node in this route graph.
            /// </summary>
            internal RouteGraphNode Parent { get; set; }

            /// <summary>
            /// Each part of a path which is a literal will create a vertice
            /// to another RouteGraphNode.
            /// </summary>
            internal IDictionary<Route.LiteralPart, ICollection<(RouteGraphNode, TInfo, bool)>> Parts { get; } = new Dictionary<Route.LiteralPart, ICollection<(RouteGraphNode, TInfo, bool)>>();

            /// <summary>
            /// All variable parts and the info items that belong to their
            /// paths are stored in a collection. Threy will add up to the
            /// current set of possible endpoints while matching an URL path.
            /// </summary>
            internal ICollection<(Route.VariablePart, TInfo, bool)> Variables { get; } = new HashSet<(Route.VariablePart, TInfo, bool)>();

            /// <summary>
            /// For all variable in the colection Variables, we have only one
            /// instance of RouteGraphNode which represents the rest of the path
            /// whenever a vaiable is matched inside this 
            /// </summary>
            internal RouteGraphNode VariableNode { get; set; }

        }

    }
}
