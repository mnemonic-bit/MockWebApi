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

        public bool TryMatch(string route, out RouteMatch<TInfo> routeMatch)
        {
            RouteParser routeParser = new RouteParser();
            Route parsedRoute = routeParser.Parse(route);

            return TryMatch(parsedRoute.Parts, out routeMatch);
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
            if (!graph.Parts.TryGetValue(literalPart, out ICollection<LiteralPartCandidate> nodes))
            {
                nodes = new HashSet<LiteralPartCandidate>();
                graph.Parts.Add(literalPart, nodes);
            }

            RouteGraphNode newGraphNode = new RouteGraphNode()
            {
                Parent = graph
            };

            LiteralPartCandidate candidate = new LiteralPartCandidate()
            {
                NextNode = newGraphNode,
                Info = routeInfo,
                IsLast = isLast
            };

            nodes.Add(candidate);

            return newGraphNode;
        }

        private RouteGraphNode AddVariablePartToGraphNode(RouteGraphNode graph, Route.VariablePart variablePart, TInfo routeInfo, bool isLast)
        {
            graph.VariableNode ??= new RouteGraphNode()
            {
                Parent = graph
            };

            VariablePartCandidate candidate = new VariablePartCandidate()
            {
                Variable = variablePart,
                Info = routeInfo,
                IsLast = isLast
            };

            graph.Variables.Add(candidate);

            return graph.VariableNode;
        }

        private bool TryMatch(IEnumerable<Route.Part> parts, out RouteMatch<TInfo> routeMatch)
        {
            routeMatch = default;

            ICollection<MatchCandidate> initialCandidates = new HashSet<MatchCandidate>();
            initialCandidates.Add(new MatchCandidate()
            {
                NextNode = _matchGraph
            });

            ICollection<MatchCandidate> nodeCandidates = parts.Aggregate(initialCandidates, TryMatchAggregate);

            if (nodeCandidates == null || nodeCandidates.Count() == 0)
            {
                return false;
            }

            if (nodeCandidates.Count() > 1)
            {
                return false;
            }

            MatchCandidate candidate = nodeCandidates.Single();
            routeMatch = new RouteMatch<TInfo>(candidate.Info, candidate.Variables);

            return true;
        }

        /// <summary>
        /// Aggregates all possible graph nodes wich can be reached by the given
        /// set of start nodes from the graph, and a path-part. This method returns
        /// null if no further nodes can be matched.
        /// </summary>
        /// <param name="currentMatches"></param>
        /// <param name="part"></param>
        /// <param name="isLastNode"></param>
        /// <returns></returns>
        private ICollection<MatchCandidate> TryMatchAggregate(ICollection<MatchCandidate> currentMatches, Route.Part part, bool isLastNode)
        {
            if (currentMatches == null)
            {
                return null;
            }

            if (!TryMatchAggregate(currentMatches, part, isLastNode, out ICollection<MatchCandidate> matchCandidates))
            {
                return null;
            }

            return matchCandidates;
        }

        /// <summary>
        /// Tries to match the part of a path with a collection of graph alternatives.
        /// If any node can be matched, this method returns true. Otherwise false is
        /// returned.
        /// </summary>
        /// <param name="currentMatches"></param>
        /// <param name="part"></param>
        /// <param name="isLastNode"></param>
        /// <param name="matchCandidates"></param>
        /// <returns></returns>
        private bool TryMatchAggregate(ICollection<MatchCandidate> currentMatches, Route.Part part, bool isLastNode, out ICollection<MatchCandidate> matchCandidates)
        {
            matchCandidates = new HashSet<MatchCandidate>();

            bool notFoundInAnyNode = true;
            foreach (var node in currentMatches)
            {
                if (!TryMatch(node.NextNode, part, isLastNode, out ICollection<MatchCandidate> nextCandidates))
                {
                    continue;
                }

                notFoundInAnyNode = false;

                _ = matchCandidates.AddAll(nextCandidates
                    .Where(candidate => candidate.IsLast == isLastNode)
                    .Select(candidate => { candidate.Variables.AddAll(node.Variables); return candidate; }));
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
        /// <param name="matchCandidates"></param>
        /// <returns></returns>
        private bool TryMatch(RouteGraphNode graph, Route.Part part, bool isLastNode, out ICollection<MatchCandidate> matchCandidates)
        {
            bool found = TryMatchPart(graph, part, out matchCandidates);

            if (!found)
            {
                return false;
            }

            matchCandidates = matchCandidates.Where(candidate => candidate.IsLast == isLastNode).ToList();

            if (matchCandidates.Count() == 0)
            {
                return false;
            }

            if (isLastNode && matchCandidates.Count() > 1)
            {
                return false;
            }

            return true;
        }

        private bool TryMatchPart(RouteGraphNode graph, Route.Part part, out ICollection<MatchCandidate> nodes)
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

        private bool TryMatchLiteralPart(RouteGraphNode graph, Route.LiteralPart literalPart, out ICollection<MatchCandidate> matchCandidates)
        {
            matchCandidates = default;

            if (graph.Parts.TryGetValue(literalPart, out ICollection<LiteralPartCandidate> nodes))
            {
                matchCandidates = new HashSet<MatchCandidate>(nodes.Select(candidate => new MatchCandidate(candidate)));
                return true;
            }

            if (graph.VariableNode == null)
            {
                return false;
            }


            var nextVariableNodes = graph
                .Variables
                .Select(variable =>
                {
                    MatchCandidate matchCandidate = new MatchCandidate(variable);
                    matchCandidate.NextNode = graph.VariableNode;
                    matchCandidate.Variables.Add(variable.Variable.ToString(), literalPart.ToString());
                    return matchCandidate;
                });

            matchCandidates = new HashSet<MatchCandidate>();
            matchCandidates.AddAll(nextVariableNodes);

            return true;
        }

        private bool TryMatchVariablePart(RouteGraphNode graph, Route.VariablePart variablePart, out ICollection<MatchCandidate> nodes)
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
            internal IDictionary<Route.LiteralPart, ICollection<LiteralPartCandidate>> Parts { get; } = new Dictionary<Route.LiteralPart, ICollection<LiteralPartCandidate>>();

            /// <summary>
            /// All variable parts and the info items that belong to their
            /// paths are stored in a collection. Threy will add up to the
            /// current set of possible endpoints while matching an URL path.
            /// </summary>
            internal ICollection<VariablePartCandidate> Variables { get; } = new HashSet<VariablePartCandidate>();

            /// <summary>
            /// For all variable in the colection Variables, we have only one
            /// instance of RouteGraphNode which represents the rest of the path
            /// whenever a vaiable is matched inside this 
            /// </summary>
            internal RouteGraphNode VariableNode { get; set; }

        }

        private class LiteralPartCandidate
        {

            public RouteGraphNode NextNode { get; set; }

            public TInfo Info { get; set; }

            public bool IsLast { get; set; }

        }

        private class VariablePartCandidate
        {

            public Route.VariablePart Variable { get; set; }

            public TInfo Info { get; set; }

            public bool IsLast { get; set; }

        }

        private class MatchCandidate
        {

            public MatchCandidate()
            {

            }

            public MatchCandidate(LiteralPartCandidate literalCandidate)
            {
                NextNode = literalCandidate.NextNode;
                Info = literalCandidate.Info;
                IsLast = literalCandidate.IsLast;
            }

            public MatchCandidate(VariablePartCandidate variableCandidate)
            {
                Info = variableCandidate.Info;
                IsLast = variableCandidate.IsLast;
            }

            public RouteGraphNode NextNode { get; set; }

            public TInfo Info { get; set; }

            public IDictionary<string, string> Variables { get; } = new Dictionary<string, string>();

            public bool IsLast { get; set; }

        }

    }
}
