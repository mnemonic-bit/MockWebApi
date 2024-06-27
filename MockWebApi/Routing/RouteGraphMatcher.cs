using System;
using System.Collections.Generic;
using System.Linq;
using MockWebApi.Extension;

namespace MockWebApi.Routing
{
    public class RouteGraphMatcher<TInfo> : IRouteMatcher<TInfo>
        where TInfo : class
    {

        private readonly IRouteParser _routeParser;

        private readonly RouteGraphNode _matchGraph;

        public RouteGraphMatcher(IRouteParser routeParser)
        {
            _routeParser = routeParser;
            _matchGraph = new RouteGraphNode();
        }

        public void AddRoute(string routeTemplate, TInfo routeInfo)
        {
            lock (_matchGraph)
            {
                RouteParser routeParser = new RouteParser();
                Route route = routeParser.Parse(routeTemplate);

                if (TryFindRoute(route, out MatchCandidate? candidate) && candidate != null)
                {
                    RouteInfo foundRouteInfo = candidate.PartCandidate?.Infos.First(routeInfo => routeInfo.Info == candidate.Info)!;
                    foundRouteInfo.Info = routeInfo;
                    return;
                }

                AddRouteToMatchGraph(route, routeInfo);
            }
        }

        public bool ContainsRoute(string routeTemplate)
        {
            lock (_matchGraph)
            {
                return TryFindRoute(routeTemplate, out TInfo? _);
            }
        }

        public IEnumerable<TInfo> GetAllRoutes()
        {
            lock (_matchGraph)
            {
                return EnumerateAllCandidates(_matchGraph);
            }
        }

        public bool Remove(string routeTemplate)
        {
            lock (_matchGraph)
            {
                if (!ContainsRoute(routeTemplate))
                {
                    return false;
                }

                RouteParser routeParser = new RouteParser();
                Route route = routeParser.Parse(routeTemplate);
                RemoveRouteFromMatchGraph(route);

                return true;
            }
        }

        public void RemoveAll()
        {
            lock (_matchGraph)
            {
                _matchGraph.Literals.Clear();
                _matchGraph.Parent = null;
                _matchGraph.Variables.Clear();
            }
        }

        public bool TryFindRoute(string path, out TInfo? info)
        {
            lock (_matchGraph)
            {
                RouteParser routeParser = new RouteParser();
                Route parsedRoute = routeParser.Parse(path);

                if (!TryFindRoute(parsedRoute, out MatchCandidate? candidate) || candidate == null)
                {
                    info = default;
                    return false;
                }

                info = candidate.Info;
                return true;
            }
        }

        public bool TryMatch(string path, out RouteMatch<TInfo>? routeMatch)
        {
            lock (_matchGraph)
            {
                RouteParser routeParser = new RouteParser();
                Route parsedRoute = routeParser.Parse(path);

                if (!parsedRoute.Parts.All(part => part is Route.LiteralPart))
                {
                    throw new ArgumentException($"Given path contains variable definitions, wich is not allowed when matching (offending path='{path}')");
                }

                return TryMatchRoute(parsedRoute, out routeMatch);
            }
        }


        #region GetAllRoutes

        private IEnumerable<TInfo> EnumerateAllCandidates(RouteGraphNode? graphNode)
        {
            if (graphNode == null)
            {
                return Enumerable.Empty<TInfo>();
            }

            var literalCandidates = graphNode.Literals
                .SelectMany(literal => literal.Value.Infos)
                .Select(info => info.Info);

            var recursiveLiteralCandidates = graphNode.Literals
                .Select(literal => literal.Value)
                .SelectMany(graphNode => EnumerateAllCandidates(graphNode.NextNode));

            var variableCandidates = graphNode.Variables
                .SelectMany(variable => variable.Infos)
                .Select(info => info.Info);

            var recursiveVariableCandidates = graphNode.Variables
                .Select(variable => variable.NextNode)
                .SelectMany(graphNode => EnumerateAllCandidates(graphNode));

            return literalCandidates
                .Concat(recursiveLiteralCandidates)
                .Concat(variableCandidates)
                .Concat(recursiveVariableCandidates);
        }

        #endregion


        #region TryFindRoute

        private bool TryFindRoute(Route route, out MatchCandidate? candidate)
        {
            candidate = default;

            ICollection<MatchCandidate>? initialCandidates = new List<MatchCandidate>
            {
                new MatchCandidate()
                {
                    NextNode = _matchGraph
                }
            };

            IEnumerable<Route.Part> parts = route.Parts;
            ICollection<MatchCandidate>? candidates = parts.Aggregate<Route.Part, ICollection<MatchCandidate>?>(initialCandidates, TryFindRouteAggregate);

            if (candidates.IsNullOrEmpty())
            {
                return false;
            }

            if (!TryMatchParameters(route, candidates, out candidates))
            {
                return false;
            }

            if (candidates.Count != 1)
            {
                return false;
            }

            candidate = candidates.Single();

            return true;
        }

        private ICollection<MatchCandidate>? TryFindRouteAggregate(ICollection<MatchCandidate>? currentMatches, Route.Part part, bool isLastNode)
        {
            if (currentMatches == null)
            {
                return null;
            }

            if (!TryFindRouteAggregate(currentMatches, part, isLastNode, out ICollection<MatchCandidate> matchCandidates))
            {
                return null;
            }

            return matchCandidates;
        }

        private bool TryFindRouteAggregate(ICollection<MatchCandidate> currentMatches, Route.Part part, bool isLastNode, out ICollection<MatchCandidate> matchCandidates)
        {
            matchCandidates = new List<MatchCandidate>();

            bool notFoundInAnyNode = true;
            foreach (MatchCandidate matchCandidate in currentMatches)
            {
                if (!TryFindRoute(matchCandidate.NextNode!, part, isLastNode, out ICollection<MatchCandidate>? nextCandidates) || nextCandidates == null)
                {
                    continue;
                }

                notFoundInAnyNode = false;

                matchCandidates.AddAll(nextCandidates);
            }

            return !notFoundInAnyNode;
        }

        private bool TryFindRoute(RouteGraphNode nextNode, Route.Part part, bool isLastNode, out ICollection<MatchCandidate>? nextCandidates)
        {
            if (part is Route.LiteralPart literalPart)
            {
                return TryFindLiteralPartRoute(nextNode, literalPart, isLastNode, out nextCandidates);
            }
            else if (part is Route.VariablePart variablePart)
            {
                return TryFindVariablePartRoute(nextNode, variablePart, isLastNode, out nextCandidates);
            }
            else
            {
                throw new NotImplementedException($"The type of route-part {part.GetType().Name} is not implemented.");
            }
        }

        private bool TryFindLiteralPartRoute(RouteGraphNode nextNode, Route.LiteralPart literalPart, bool isLastNode, out ICollection<MatchCandidate>? nextCandidates)
        {
            nextCandidates = default;

            if (!nextNode.Literals.TryGetValue(literalPart, out LiteralPartCandidates? literalPartCandidates) || literalPartCandidates == null)
            {
                return false;
            }

            nextCandidates = literalPartCandidates
                .Infos
                .Select(info =>
                {
                    MatchCandidate candidate = new MatchCandidate();
                    candidate.Info = info.Info;
                    candidate.Parameters.AddAll(info.Parameters);
                    candidate.NextNode = literalPartCandidates.NextNode;
                    candidate.PartCandidate = literalPartCandidates;

                    return candidate;
                })
                .ToList();

            if (!isLastNode)
            {
                MatchCandidate singleMatchCandidate = new MatchCandidate();
                singleMatchCandidate.Info = default;
                singleMatchCandidate.NextNode = literalPartCandidates.NextNode;
                singleMatchCandidate.PartCandidate = literalPartCandidates;

                nextCandidates.Add(singleMatchCandidate);
            }

            return true;
        }

        private bool TryFindVariablePartRoute(RouteGraphNode nextNode, Route.VariablePart variablePart, bool isLastNode, out ICollection<MatchCandidate>? nextCandidates)
        {
            nextCandidates = default;

            nextCandidates = nextNode
                .Variables
                .SelectMany(variablePart =>
                {
                    ICollection<MatchCandidate> newCandidates = variablePart
                        .Infos
                        .Select(info =>
                        {
                            MatchCandidate candidate = new MatchCandidate();
                            candidate.Info = info.Info;
                            candidate.Parameters.AddAll(info.Parameters);
                            candidate.NextNode = variablePart.NextNode;
                            candidate.PartCandidate = variablePart;

                            return candidate;
                        })
                        .ToList();

                    if (!isLastNode)
                    {
                        MatchCandidate singleMatchCandidate = new MatchCandidate();
                        singleMatchCandidate.Info = default;
                        singleMatchCandidate.NextNode = variablePart.NextNode;
                        singleMatchCandidate.PartCandidate = variablePart;

                        newCandidates.Add(singleMatchCandidate);
                    }

                    return newCandidates;
                })
                .ToList();

            return true;
        }

        #endregion


        #region TryMatch

        private void AddRouteToMatchGraph(Route route, TInfo routeInfo)
        {
            PartCandidate? firstCandidate = null;
            RouteGraphNode? graph = _matchGraph;

            (RouteGraphNode? leafNode, PartCandidate? partCandidate) = route.Parts
                .Aggregate<Route.Part, (RouteGraphNode?, PartCandidate?)>(
                    (graph, firstCandidate),
                    (seed, part, isLast) =>
                    {
                        PartCandidate partCandidate = GetPartCandidateFromGraphNode(seed.Item1!, part);
                        return (partCandidate.NextNode, partCandidate);
                    });

            AddRouteInfoToPartCandidate(partCandidate!, routeInfo, route.Parameters);
        }

        private void AddRouteInfoToPartCandidate(PartCandidate partCandidate, TInfo routeInfo, IDictionary<string, string> parameters)
        {
            RouteInfo routeInfoItem = new RouteInfo(routeInfo);
            routeInfoItem.Parameters.AddAll(parameters);

            partCandidate.Infos.Add(routeInfoItem);
        }

        private PartCandidate GetPartCandidateFromGraphNode(RouteGraphNode graph, Route.Part part)
        {
            if (part is Route.LiteralPart literalPart)
            {
                return GetLiteralPartGraphNode(graph, literalPart);
            }
            else if (part is Route.VariablePart variablePart)
            {
                return GetVariablePartGraphNode(graph, variablePart);
            }
            else
            {
                throw new InvalidOperationException($"This sub-class of Route.Part is not handled by this function.");
            }
        }

        private LiteralPartCandidates GetLiteralPartGraphNode(RouteGraphNode graph, Route.LiteralPart literalPart)
        {
            if (!graph.Literals.TryGetValue(literalPart, out LiteralPartCandidates? candidates) || candidates == null)
            {
                RouteGraphNode nextGraphNode = new RouteGraphNode()
                {
                    Parent = graph
                };

                candidates = new LiteralPartCandidates()
                {
                    LiteralPart = literalPart,
                    NextNode = nextGraphNode
                };

                graph.Literals.Add(literalPart, candidates);
            }

            return candidates;
        }

        private VariablePartCandidates GetVariablePartGraphNode(RouteGraphNode graph, Route.VariablePart variablePart)
        {
            RouteGraphNode nextNode = new RouteGraphNode()
            {
                Parent = graph
            };

            VariablePartCandidates candidate = new VariablePartCandidates()
            {
                Variable = variablePart,
                NextNode = nextNode
            };

            graph.Variables.Add(candidate);

            return candidate;
        }

        private bool RemoveRouteFromMatchGraph(Route route)
        {
            if (!TryFindRoute(route, out MatchCandidate? candidate) || candidate == null)
            {
                return false;
            }

            RouteGraphNode lastNode = candidate.NextNode?.Parent!;

            IEnumerable<RouteGraphNode> reversedNodePath = EnumerateGraphNodes(lastNode);
            IEnumerable<Route.Part> reversedRouteParts = route.Parts.Reverse();

            // Remove the info item from the last node
            RemoveInfoItemFromMatchGraphNode(lastNode, reversedRouteParts.First());

            List<RouteGraphNode?> result = Enumerable.Zip(reversedRouteParts, reversedNodePath)
                .Select(pair => RemoveRouteFromMatchGraph(pair.Second, pair.First, false))
                .ToList();

            return true;
        }

        private IEnumerable<RouteGraphNode> EnumerateGraphNodes(RouteGraphNode? graphNode)
        {
            while (graphNode != null)
            {
                RouteGraphNode nextGraphNode = graphNode;
                graphNode = graphNode.Parent;
                yield return nextGraphNode;
            }
        }

        private RouteGraphNode? RemoveInfoItemFromMatchGraphNode(RouteGraphNode graph, Route.Part part)
        {
            if (part is Route.LiteralPart literalPart)
            {
                return RemoveInfoItemFromLiteralPartOfMatchGraphNode(graph, literalPart);
            }
            else if (part is Route.VariablePart variablePart)
            {
                return RemoveInfoItemFromVariablePartOfMatchGraphNode(graph, variablePart);
            }
            else
            {
                throw new InvalidOperationException($"Internal error: Type of route part is not supported.");
            }
        }

        private RouteGraphNode? RemoveInfoItemFromLiteralPartOfMatchGraphNode(RouteGraphNode graph, Route.LiteralPart part)
        {
            if (!graph.Literals.TryGetValue(part, out LiteralPartCandidates? candidates) || candidates == null)
            {
                throw new InvalidOperationException($"Internal Error: Trying to remove part of route which does not exist.");
            }

            RouteInfo? infoToRemove = candidates.Infos.FirstOrDefault(); //TODO: fix this, we have to identify the routing info that belongs to the path that is removed.

            if (infoToRemove != null)
            {
                candidates.Infos.Remove(infoToRemove);
            }

            return candidates.NextNode;
        }

        private RouteGraphNode RemoveInfoItemFromVariablePartOfMatchGraphNode(RouteGraphNode graph, Route.VariablePart part)
        {
            VariablePartCandidates candidateToRemove = graph.Variables.FirstOrDefault() ?? throw new InvalidOperationException($"Internal Error: Trying to remove part of route which does not exist."); ;

            graph.Variables.Remove(candidateToRemove);

            // TODO: this is not correct and was changed due to a rewrite of the algorithm
            return graph;
        }

        private RouteGraphNode? RemoveRouteFromMatchGraph(RouteGraphNode graph, Route.Part part, bool isLast)
        {
            if (part is Route.LiteralPart literalPart)
            {
                return RemoveLiteralPartFromMatchGraph(graph, literalPart, isLast);
            }
            else if (part is Route.VariablePart variablePart)
            {
                return RemoveVariablePartFromMatchGraph(graph, variablePart, isLast);
            }
            else
            {
                throw new InvalidOperationException($"Internal error: Type of route part is not supported.");
            }
        }

        private RouteGraphNode? RemoveLiteralPartFromMatchGraph(RouteGraphNode graph, Route.LiteralPart part, bool isLast)
        {
            if (!graph.Literals.TryGetValue(part, out LiteralPartCandidates? candidates) || candidates == null)
            {
                throw new InvalidOperationException($"Internal Error: Trying to remove part of route which does not exist.");
            }

            // If the follow-up graph node is empty, we can remove
            // the part from the collection of literals, which unlinks
            // the graph at this node.
            if (candidates.NextNode?.IsEmpty ?? true)
            {
                if (!graph.Literals.Remove(part))
                {
                    throw new InvalidOperationException($"Internal Error: Trying to remove literal part, but removing it from the list returned false.");
                }
            }

            return candidates.NextNode;
        }

        private RouteGraphNode RemoveVariablePartFromMatchGraph(RouteGraphNode graph, Route.VariablePart part, bool isLast)
        {
            VariablePartCandidates? candidateToRemove = graph.Variables.FirstOrDefault();

            if (candidateToRemove == null)
            {
                throw new InvalidOperationException($"Internal Error: Trying to remove part of route which does not exist.");
            }

            graph.Variables.Remove(candidateToRemove);

            // TODO: this is not correct and was changed due to a rewrite of the algorithm
            return graph;
        }

        private bool TryMatchRoute(Route route, out RouteMatch<TInfo>? match)
        {
            match = default;

            ICollection<MatchCandidate> initialCandidates = new List<MatchCandidate>()
            {
                new MatchCandidate()
                {
                    NextNode = _matchGraph
                }
            };

            IEnumerable<Route.Part> parts = route.Parts;
            ICollection<MatchCandidate>? candidates = parts.Aggregate<Route.Part, ICollection<MatchCandidate>?>(initialCandidates, TryMatchAggregate);

            if (candidates.IsNullOrEmpty())
            {
                return false;
            }

            if (!TryMatchParameters(route, candidates, out candidates))
            {
                return false;
            }

            //TODO: the next two if-statements are special handlings of a more
            // general case, which is ordering the route matches according to
            // their specificy, and choosing that candidate which has the most
            // specific route.

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

        private bool TryGetFixedCandidate(ICollection<MatchCandidate> candidates, out RouteMatch<TInfo>? match)
        {
            return TryFilterCandidate(candidates, c => c.IsSpecific, out match);
        }

        private bool TryGetVariableCandidate(ICollection<MatchCandidate> candidates, out RouteMatch<TInfo>? match)
        {
            return TryFilterCandidate(candidates, c => !c.IsSpecific, out match);
        }

        private bool TryFilterCandidate(ICollection<MatchCandidate> candidates, Func<MatchCandidate, bool> predicate, out RouteMatch<TInfo>? match)
        {
            match = default;

            ICollection<MatchCandidate> filteredCandidates = candidates
                .Select(candidate =>
                {
                    return new MatchCandidate(candidate);
                })
                .Where(predicate)
                .Where(candidate => candidate.Info != null)
                .ToList();

            //TODO: this is the place where we have to implement the feature
            // which is ecoded in the unit-test
            // RouteGraphMatherTests.TryMatch_ShouldFindMostSpecificMatch()
            if (filteredCandidates.Count() != 1)
            {
                return false;
            }

            MatchCandidate candidate = filteredCandidates.Single();
            match = ConvertToRouteMatch(candidate);

            return true;
        }

        private RouteMatch<TInfo> ConvertToRouteMatch(MatchCandidate candidate)
        {
            if (candidate.Info == null)
            {
                throw new ArgumentException($"The number of info-items in a {nameof(MatchCandidate)} must be set, but currently it is null", nameof(candidate));
            }

            return new RouteMatch<TInfo>(candidate.Info, candidate.Variables, candidate.Parameters);
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
        private ICollection<MatchCandidate>? TryMatchAggregate(ICollection<MatchCandidate>? currentMatches, Route.Part part, bool isLastNode)
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
            matchCandidates = new List<MatchCandidate>();

            bool notFoundInAnyNode = true;
            foreach (MatchCandidate match in currentMatches)
            {
                if (!TryMatch(match.NextNode!, part, isLastNode, out ICollection<MatchCandidate> nextCandidates))
                {
                    continue;
                }

                notFoundInAnyNode = false;

                // accumulate the variable bindings found so far
                _ = matchCandidates.AddAll(nextCandidates
                    .Where(candidate => isLastNode ? candidate.Info != null : candidate.Info == null)
                    .Select(candidate =>
                    {
                        candidate.Variables.AddAll(match.Variables);
                        return candidate;
                    }));
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
            if (!TryMatchPart(graph, part, isLastNode, out matchCandidates))
            {
                return false;
            }

            matchCandidates = matchCandidates
                .Where(candidate => isLastNode ? candidate.Info != null : candidate.Info == null)
                .ToList();

            if (matchCandidates.Count() == 0)
            {
                return false;
            }

            //TODO: Check if removing this will not break all other tests
            //if (isLastNode && matchCandidates.Count() > 1)
            //{
            //    return false;
            //}

            return true;
        }

        private bool TryMatchPart(RouteGraphNode graph, Route.Part part, bool isLastNode, out ICollection<MatchCandidate> nodes)
        {
            if (!(part is Route.LiteralPart literalPart))
            {
                throw new InvalidOperationException($"This sub-class of Route.Part is not handled by this function.");
            }

            return TryMatchLiteralPart(graph, literalPart, isLastNode, out nodes);
        }

        private bool TryMatchLiteralPart(RouteGraphNode graph, Route.LiteralPart literalPart, bool isLastNode, out ICollection<MatchCandidate> matchCandidates)
        {
            matchCandidates = new List<MatchCandidate>();

            if (graph.Literals.TryGetValue(literalPart, out LiteralPartCandidates? literalPartCandidates))
            {
                if (!isLastNode)
                {
                    MatchCandidate matchCandidate = new MatchCandidate();
                    matchCandidate.NextNode = literalPartCandidates.NextNode;
                    matchCandidates.Add(matchCandidate);

                    return true;
                }
                if (literalPartCandidates.Infos.Count > 0)
                {
                    matchCandidates = literalPartCandidates.Infos
                        .Select(info =>
                        {
                            MatchCandidate matchCandidate = new MatchCandidate();
                            matchCandidate.Parameters.AddAll(info.Parameters);
                            matchCandidate.Info = info.Info; // TODO: fix this, this is most likely not correct.
                            return matchCandidate;
                        })
                        .ToList();

                    MatchCandidate matchCandidate = new MatchCandidate();
                    matchCandidate.NextNode = literalPartCandidates.NextNode;
                    matchCandidates.Add(matchCandidate);

                    return true;
                }
            }

            IEnumerable<MatchCandidate> nextVariableNodes = graph
                .Variables
                .SelectMany(variable =>
                {
                    // This are all matches that end in this node
                    var matchCandidates = variable
                        .Infos
                        .Select(info =>
                        {
                            MatchCandidate matchCandidate = new MatchCandidate();
                            matchCandidate.Info = info.Info; // TODO: refactor this?
                            matchCandidate.Variables.Add(GetVariableName(variable.Variable!), literalPart.ToString());

                            return matchCandidate;
                        })
                        .ToList(); // TODO: why does this not work without ToList() and matchCandidates.Append(singleMatchCandidate) at the end

                    // This is for all matches that go beyond this node
                    MatchCandidate singleMatchCandidate = new MatchCandidate();
                    singleMatchCandidate.NextNode = variable.NextNode;
                    singleMatchCandidate.Variables.Add(GetVariableName(variable.Variable!), literalPart.ToString());

                    matchCandidates.Add(singleMatchCandidate);

                    return matchCandidates;
                });

            matchCandidates.AddAll(nextVariableNodes);

            return true;
        }

        #endregion TryMatch


        #region Common Matcher

        private bool TryMatchParameters(Route route, ICollection<MatchCandidate> currentCandidates, out ICollection<MatchCandidate> candidates)
        {
            candidates = currentCandidates
                .Select(candidate =>
                {
                    if (!TryMatchParameters(route, candidate.Parameters, out IDictionary<string, string> variablesFromParameters))
                    {
                        return null;
                    }

                    candidate.Variables.AddAll(variablesFromParameters);

                    MatchCandidate newMatchCandidate = new MatchCandidate(candidate);
                    newMatchCandidate.NextNode = candidate.NextNode;
                    newMatchCandidate.PartCandidate = candidate.PartCandidate;

                    return newMatchCandidate;
                })
                .NotNull()
                .ToList();

            return true;
        }

        private bool TryMatchParameters(Route route, IDictionary<string, string> parameters, out IDictionary<string, string> matchedParameters)
        {
            matchedParameters = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> param in parameters)
            {
                if (!TryMatchSingleParameter(param.Key, param.Value, route.Parameters, out KeyValuePair<string, string>? matchedParameter))
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

        private bool TryMatchSingleParameter(string variableName, string variableValue, IDictionary<string, string> parameters, out KeyValuePair<string, string>? matchedParameter)
        {
            matchedParameter = null;

            bool variableValueIsParameter = VariableValueIsParameterName(variableValue, out string? parameterName);

            if (!parameters.TryGetValue(variableName, out string? parameterValue) || parameterValue == null)
            {
                if (variableValueIsParameter && parameterName != null)
                {
                    matchedParameter = new KeyValuePair<string, string>(parameterName, string.Empty);
                    return true;
                }
                return false;
            }

            if (!variableValueIsParameter)
            {
                return variableValue == parameterValue;
            }

            matchedParameter = new KeyValuePair<string, string>(parameterName!, parameterValue);
            return true;
        }

        private bool VariableValueIsParameterName(string variableValue, out string? parameterName)
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

        #endregion


        /// <summary>
        /// A node in the node-graph is an instance which holds references
        /// to sub-nodes with each vertice being a labeled edge with an
        /// instance of Route.Part.
        /// </summary>
        private class RouteGraphNode
        {

            /// <summary>
            /// Determines whether the graph node is empty or contains any
            /// links to child-nodes.
            /// </summary>
            /// <returns></returns>
            internal bool IsEmpty
            {
                get
                {
                    return Literals.Count == 0 && Variables.Count == 0;
                }
            }

            /// <summary>
            /// This is the reference to the parent node in this route graph.
            /// </summary>
            internal RouteGraphNode? Parent { get; set; }

            /// <summary>
            /// Each part of a path which is a literal will create a vertice
            /// to another RouteGraphNode.
            /// </summary>
            internal IDictionary<Route.LiteralPart, LiteralPartCandidates> Literals { get; } = new Dictionary<Route.LiteralPart, LiteralPartCandidates>();

            /// <summary>
            /// All variable parts and the info items that belong to their
            /// paths are stored in a collection. Threy will add up to the
            /// current set of possible endpoints while matching an URL path.
            /// </summary>
            internal ICollection<VariablePartCandidates> Variables { get; } = new List<VariablePartCandidates>();

        }

        private class RouteInfo
        {

            private TInfo _info;

            public RouteInfo(TInfo info)
            {
                _info = info ?? throw new ArgumentException($"Cannot set null as info-item.");
            }

            public TInfo Info
            {
                get
                {
                    return _info;
                }
                internal set
                {
                    _info = value ?? throw new ArgumentException($"Cannot set null as info-item.");
                }
            }

            public IDictionary<string, string> Parameters { get; } = new Dictionary<string, string>(); // TODO: fix this, this might not belong to a route information item.

            public override bool Equals(object? obj)
            {
                if (!(obj is RouteInfo routeInfo))
                {
                    return false;
                }

                return routeInfo.Info == Info;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

        }

        private class PartCandidate
        {

            public RouteGraphNode? NextNode { get; set; }

            public ICollection<RouteInfo> Infos { get; } = new List<RouteInfo>();

        }

        private class LiteralPartCandidates : PartCandidate
        {

            public Route.LiteralPart? LiteralPart { get; set; }

        }

        private class VariablePartCandidates : PartCandidate
        {

            public Route.VariablePart? Variable { get; set; }

        }

        private class MatchCandidate
        {

            /// <summary>
            /// Creates an empty MatchCandidate.
            /// </summary>
            public MatchCandidate()
            {

            }

            /// <summary>
            /// The copy-constructor.
            /// </summary>
            /// <param name="matchCandidate"></param>
            internal MatchCandidate(MatchCandidate matchCandidate)
            {
                Info = matchCandidate.Info;
                Parameters.AddAll(matchCandidate.Parameters);
                Variables.AddAll(matchCandidate.Variables);
            }

            /// <summary>
            /// Marks this match candidate as being for a specific
            /// route, i.e. one that has no variables in it along the path.
            /// </summary>
            public bool IsSpecific
            {
                get
                {
                    return Variables.Count == 0;
                }
            }

            public RouteGraphNode? NextNode { get; set; }

            public PartCandidate? PartCandidate { get; set; }

            public TInfo? Info { get; set; }

            public IDictionary<string, string> Parameters { get; } = new Dictionary<string, string>();

            public IDictionary<string, string> Variables { get; } = new Dictionary<string, string>();

            public override string ToString()
            {
                return $"{nameof(MatchCandidate)}: {Info?.ToString()}";
            }

        }

    }
}
