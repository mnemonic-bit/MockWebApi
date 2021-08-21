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

            routeInfo = default(TInfo);
            //bool found = parsedRoute.Parts.Aggregate(_matchGraph, (graph, part) => TryMatchPart(graph, part, out ICollection<(RouteGraphNode, TInfo, bool)> nodes));
            Route.Part firstPart = parsedRoute.Parts.First();
            bool found = TryMatchPart(_matchGraph, firstPart, out ICollection<(RouteGraphNode, TInfo, bool)> nodes);

            if (nodes.Count() == 0)
            {
                throw new Exception($"No route matches the pattern");
            }

            if (nodes.Count() > 1)
            {
                throw new Exception($"More than one route match the pattern");
            }

            bool isLastNode;
            (_, routeInfo, isLastNode) = nodes.First();

            return isLastNode;
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

        private bool TryMatch(IEnumerable<Route.Part> parts, bool isLastNode, out (RouteGraphNode, TInfo, bool) node)
        {
            node = (null, default(TInfo), false);

            ICollection<(RouteGraphNode, TInfo, bool)> nodeCandidates = parts.Aggregate(new HashSet<(RouteGraphNode, TInfo, bool)>(), (a, b, c) => a);

            if (nodeCandidates.Count() == 0)
            {
                return false;
            }

            if (nodeCandidates.Count() > 1)
            {
                return false;
            }

            node = nodeCandidates.Single();

            return true;
        }

        /// <summary>
        /// Aggregates all possible graph nodes wich can be reached by the given
        /// set of start nodes from the graph, and a path-part. This method returns
        /// null if no further nodes can be matched.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="part"></param>
        /// <param name="isLastNode"></param>
        /// <returns></returns>
        private ICollection<(RouteGraphNode, TInfo, bool)> TryMatchAggregate(ICollection<(RouteGraphNode, TInfo, bool)> nodes, Route.Part part, bool isLastNode)
        {
            if (nodes == null)
            {
                return null;
            }

            if (!TryMatchAggregate(nodes, part, isLastNode, out ICollection<(RouteGraphNode, TInfo, bool)> nextNodes))
            {
                return null;
            }

            return nextNodes;
        }

        /// <summary>
        /// Tries to match the part of a path with a collection of graph alternatives.
        /// If any node can be matched, this method returns true. Otherwise false is
        /// returned.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="part"></param>
        /// <param name="isLastNode"></param>
        /// <param name="nextNodes"></param>
        /// <returns></returns>
        private bool TryMatchAggregate(ICollection<(RouteGraphNode, TInfo, bool)> nodes, Route.Part part, bool isLastNode, out ICollection<(RouteGraphNode, TInfo, bool)> nextNodes)
        {
            nextNodes = nodes;

            bool notFoundInAnyNode = true;
            foreach (var node in nodes)
            {
                if (!TryMatch(node.Item1, part, isLastNode, out ICollection<(RouteGraphNode, TInfo, bool)> nextNodeCandidates))
                {
                    continue;
                }

                notFoundInAnyNode = false;

                _ = nextNodes.AddAll(nextNodeCandidates.Where(((RouteGraphNode, TInfo, bool) tuple) => tuple.Item3 == isLastNode));
            }

            return !notFoundInAnyNode;
        }

        /// <summary>
        /// Tries to match a single part of the path with a single node of the
        /// match-graph. Filters the result, according to the position of the
        /// path-part inside the path (i.e. if the part is the last part of the
        /// path), and returns only those nodes which are meeting this condition.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="part"></param>
        /// <param name="isLastNode"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        private bool TryMatch(RouteGraphNode graph, Route.Part part, bool isLastNode, out ICollection<(RouteGraphNode, TInfo, bool)> nodes)
        {
            bool found = TryMatchPart(_matchGraph, part, out nodes);

            if (!found)
            {
                return false;
            }

            nodes = nodes.Where(((RouteGraphNode, TInfo, bool) tuple) => tuple.Item3 == isLastNode).ToList();

            if (nodes.Count() == 0)
            {
                return false;
            }

            if (isLastNode && nodes.Count() > 1)
            {
                return false;
            }

            return true;
        }

        private bool TryMatchPart(RouteGraphNode graph, Route.Part part, out ICollection<(RouteGraphNode, TInfo, bool)> nodes)
        {
            if (part is Route.LiteralPart literalPart)
            {
                return TryMatchLiteralPart(graph, literalPart, out nodes);
            }
            else if (part is Route.VariablePart variablePart)
            {
                //TODO: refactor, this should not happen
                return TryMatchVariablePart(graph, variablePart, out nodes);
            }
            else
            {
                throw new InvalidOperationException($"This sub-class of Route.Part is not handled by this function.");
            }
        }

        private bool TryMatchLiteralPart(RouteGraphNode graph, Route.LiteralPart literalPart, out ICollection<(RouteGraphNode, TInfo, bool)> nodes)
        {
            if (!graph.Parts.TryGetValue(literalPart, out nodes))
            {
                return false;
            }

            // extens this by loading all variable-bindings to the
            // list of nodes before returning

            return true;
        }

        private bool TryMatchVariablePart(RouteGraphNode graph, Route.VariablePart variablePart, out ICollection<(RouteGraphNode, TInfo, bool)> nodes)
        {
            nodes = null;

            //TODO: refactor, this should not happen
            // This case may not happen, because a match must only
            // have literals.

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
