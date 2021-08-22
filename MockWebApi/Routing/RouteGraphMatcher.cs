using MockWebApi.Extension;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MockWebApi.Routing
{
    public class RouteGraphMatcher<TInfo> : IRouteMatcher<TInfo>
    {

        private RouteGraphNode _matchGraph;
        private IDictionary<string, TInfo> _routes;

        public RouteGraphMatcher()
        {
            _matchGraph = new RouteGraphNode();
            _routes = new Dictionary<string, TInfo>();
        }

        public void AddRoute(string routeTemplate, TInfo routeInfo)
        {
            lock (_routes)
            {
                if (ContainsRoute(routeTemplate))
                {
                    return;
                }

                RouteParser routeParser = new RouteParser();
                Route route = routeParser.Parse(routeTemplate);
                AddRouteToMatchGraph(route, routeInfo);
                _routes.Add(routeTemplate, routeInfo);
            }
        }

        public bool ContainsRoute(string routeTemplate)
        {
            lock(_routes)
            {
                return _routes.ContainsKey(routeTemplate);
            }
        }

        public IEnumerable<TInfo> GetAllRoutes()
        {
            return _routes.Values;
        }

        public bool Remove(string routeTemplate)
        {
            lock(_routes)
            {
                if (!ContainsRoute(routeTemplate))
                {
                    return false;
                }

                RouteParser routeParser = new RouteParser();
                Route route = routeParser.Parse(routeTemplate);
                RemoveRouteFromMatchGraph(route);

                return _routes.Remove(routeTemplate);
            }
        }

        public bool TryMatch(string path, out RouteMatch<TInfo> routeMatch)
        {
            lock(_routes)
            {
                RouteParser routeParser = new RouteParser();
                Route parsedRoute = routeParser.Parse(path);

                if (!parsedRoute.Parts.All(part => part is Route.LiteralPart))
                {
                    throw new ArgumentException($"Given path contains variable definitions, wich is not allowed when matching (offending path='{path}')");
                }

                if (!TryMatch(parsedRoute.Parts, out routeMatch))
                {
                    return false;
                }

                if (!TryMatchParameters(parsedRoute, routeMatch.Parameters, out IDictionary<string, string> variablesFromParameters))
                {
                    return false;
                }

                routeMatch.Variables.AddAll(variablesFromParameters);

                return true;
            }
        }

        private void AddRouteToMatchGraph(Route route, TInfo routeInfo)
        {
            RouteGraphNode leafNode = route.Parts.Aggregate(_matchGraph, (graph, part, isLast) => AddPartToGraphNode(graph, part, routeInfo, isLast));
            leafNode.Parameters = route.Parameters;
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
            if (!graph.Literals.TryGetValue(literalPart, out ICollection<LiteralPartCandidate> nodes))
            {
                nodes = new HashSet<LiteralPartCandidate>();
                graph.Literals.Add(literalPart, nodes);
            }

            RouteGraphNode newGraphNode = new RouteGraphNode()
            {
                Parent = graph
            };

            LiteralPartCandidate candidate = new LiteralPartCandidate()
            {
                LiteralPart = literalPart,
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

        private bool RemoveRouteFromMatchGraph(Route route)
        {
            RouteGraphNode leafNode = route.Parts.Aggregate(_matchGraph, RemoveRouteFromMatchGraph);
            return true;
        }

        private RouteGraphNode RemoveRouteFromMatchGraph(RouteGraphNode graph, Route.Part part)
        {
            if (part is Route.LiteralPart literalPart)
            {
                return RemoveLiteralPartFromMatchGraph(graph, literalPart);
            }
            else if (part is Route.VariablePart variablePart)
            {
                return RemoveVariablePartFromMatchGraph(graph, variablePart);
            }
            else
            {
                throw new InvalidOperationException($"Internal error: Type of route part is not supported.");
            }
        }

        private RouteGraphNode RemoveLiteralPartFromMatchGraph(RouteGraphNode graph, Route.LiteralPart part)
        {
            if (!graph.Literals.TryGetValue(part, out ICollection<LiteralPartCandidate> candidates))
            {
                throw new InvalidOperationException($"Internal Error: Trying to remove part of route which does not exist.");
            }

            LiteralPartCandidate candidateToRemove = candidates.Single(candidate => candidate.LiteralPart.Equals(part)); // TODO, add the part to this class to check and filter for it.
            candidates.Remove(candidateToRemove);

            return candidateToRemove.NextNode;
        }

        private RouteGraphNode RemoveVariablePartFromMatchGraph(RouteGraphNode graph, Route.VariablePart part)
        {
            VariablePartCandidate candidateToRemove = graph.Variables.FirstOrDefault(candidate => candidate.IsLast); // TODO, add the part to this class to check and filter for it.

            if (candidateToRemove == null)
            {
                throw new InvalidOperationException($"Internal Error: Trying to remove part of route which does not exist.");
            }

            graph.Variables.Remove(candidateToRemove);

            return graph.VariableNode;
        }

        private bool TryMatch(IEnumerable<Route.Part> parts, out RouteMatch<TInfo> match)
        {
            match = default;

            ICollection<MatchCandidate> initialCandidates = new HashSet<MatchCandidate>();
            initialCandidates.Add(new MatchCandidate()
            {
                NextNode = _matchGraph,
                IsSpecific = true
            });

            ICollection<MatchCandidate> candidates = parts.Aggregate(initialCandidates, TryMatchAggregate);

            if (candidates == null || candidates.Count() == 0)
            {
                return false;
            }

            if (TryGetFixedCandidate(candidates, out match))
            {
                return true;
            }

            if (TryGetVariableCandidate(candidates, out match))
            {
                return true;
            }

            return false;
        }

        private bool TryGetFixedCandidate(ICollection<MatchCandidate> candidates, out RouteMatch<TInfo> match)
        {
            return TryFilterCandidate(candidates, c => c.IsSpecific, out match);

        }

        private bool TryGetVariableCandidate(ICollection<MatchCandidate> candidates, out RouteMatch<TInfo> match)
        {
            return TryFilterCandidate(candidates, c => !c.IsSpecific, out match);
        }

        private bool TryFilterCandidate(ICollection<MatchCandidate> candidates, Func<MatchCandidate, bool> predicate, out RouteMatch<TInfo> match)
        {
            match = default;

            ICollection<MatchCandidate> filteredCandidates = candidates.Where(predicate).ToHashSet();

            if (filteredCandidates.Count() != 1)
            {
                return false;
            }

            MatchCandidate candidate = filteredCandidates.Single();
            match = new RouteMatch<TInfo>(candidate.Info, candidate.Variables, candidate.NextNode.Parameters);

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
            foreach (var match in currentMatches)
            {
                if (!TryMatch(match.NextNode, part, isLastNode, out ICollection<MatchCandidate> nextCandidates))
                {
                    continue;
                }

                notFoundInAnyNode = false;

                _ = matchCandidates.AddAll(nextCandidates
                    .Where(candidate => candidate.IsLast == isLastNode)
                    .Select(candidate => { candidate.Variables.AddAll(match.Variables); candidate.IsSpecific &= match.IsSpecific; return candidate; }));
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
            if (!TryMatchPart(graph, part, out matchCandidates))
            {
                return false;
            }

            matchCandidates = matchCandidates.Where(candidate => candidate.IsLast == isLastNode).ToHashSet();

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
            if (!(part is Route.LiteralPart literalPart))
            {
                throw new InvalidOperationException($"This sub-class of Route.Part is not handled by this function.");
            }

            return TryMatchLiteralPart(graph, literalPart, out nodes);
        }

        private bool TryMatchLiteralPart(RouteGraphNode graph, Route.LiteralPart literalPart, out ICollection<MatchCandidate> matchCandidates)
        {
            matchCandidates = default;

            if (graph.Literals.TryGetValue(literalPart, out ICollection<LiteralPartCandidate> nodes))
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
                    matchCandidate.Variables.Add(GetVariableName(variable.Variable), literalPart.ToString());
                    matchCandidate.IsSpecific = false;
                    return matchCandidate;
                });

            matchCandidates = new HashSet<MatchCandidate>();
            matchCandidates.AddAll(nextVariableNodes);

            return true;
        }

        private bool TryMatchParameters(Route route, IDictionary<string, string> parameters, out IDictionary<string, string> matchedParameters)
        {
            matchedParameters = new Dictionary<string, string>();

            foreach (var param in parameters)
            {
                if (!TryMatchParameter(param.Key, param.Value, route.Parameters, out KeyValuePair<string,string>? matchedParameter))
                {
                    return false;
                }

                if (matchedParameter.HasValue)
                {
                    matchedParameters.Add(matchedParameter.Value);
                }
            }

            return true;
        }

        private bool TryMatchParameter(string variableName, string variableValue, IDictionary<string, string> parameters, out KeyValuePair<string, string>? matchedParameter)
        {
            matchedParameter = null;

            if (!parameters.TryGetValue(variableName, out string parameterValue))
            {
                return false;
            }

            if (!VariableValueIsParameterName(variableValue, out string parameterName))
            {
                return variableValue == parameterValue;
            }

            matchedParameter = new KeyValuePair<string, string>(parameterName, parameterValue);
            return true;
        }

        private bool VariableValueIsParameterName(string variableValue, out string parameterName)
        {
            if (variableValue.StartsWith("{") && variableValue.EndsWith("}"))
            {
                parameterName = GetVariableName(variableValue);
                return true;
            }

            parameterName = null;
            return false;
        }

        private string GetVariableName(Route.VariablePart variablePart)
        {
            return GetVariableName(variablePart.ToString());
        }

        private string GetVariableName(string variable)
        {
            return variable.Substring(1, variable.Length - 2);
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
            /// Stores the dictionary of all parameters of the route this
            /// graph node represents.
            /// </summary>
            internal IDictionary<string, string> Parameters { get; set; }

            /// <summary>
            /// Each part of a path which is a literal will create a vertice
            /// to another RouteGraphNode.
            /// </summary>
            internal IDictionary<Route.LiteralPart, ICollection<LiteralPartCandidate>> Literals { get; } = new Dictionary<Route.LiteralPart, ICollection<LiteralPartCandidate>>();

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

            public Route.LiteralPart LiteralPart { get; set; }

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
                IsSpecific = true;
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

            /// <summary>
            /// Marks this match candidate as being for a specific
            /// route, i.e. one that has no variables in it along the path.
            /// </summary>
            public bool IsSpecific { get; set; }

        }

    }
}
