using System.Collections.Generic;
using System.Numerics;
using Aptacode.PathFinder.Geometry;
using Aptacode.PathFinder.Geometry.Neighbours;
using Aptacode.PathFinder.Utilities;

namespace Aptacode.PathFinder.Algorithm
{
    public class PathFinder
    {
        private readonly Map _map;
        private readonly INeighbourFinder _neighbourFinder;

        public PathFinder(Map map, INeighbourFinder neighbourFinder)
        {
            _map = map;
            _neighbourFinder = neighbourFinder;
        }

        public PathFinder(Map map) : this(map, new JumpPointSearchNeighbourFinder(AllowedDirections.All)) { }

        public IEnumerable<Vector2> FindPath()
        {
            var sortedOpenNodes = new PriorityQueue<float, Node>();

            sortedOpenNodes.Enqueue(_map.Start, _map.Start.CostDistance);

            var closedNodes = new Dictionary<Vector2, Node>();
            var openNodes = new Dictionary<Vector2, Node>
            {
                {_map.Start.Position, _map.Start}
            };

            while (!sortedOpenNodes.IsEmpty())
            {
                var currentNode = sortedOpenNodes.Dequeue();
                if (currentNode.Position == _map.End.Position) //if we've reached the end node a path has been found.
                {
                    var node = currentNode;
                    var path = new List<Vector2>();
                    while (true)
                    {
                        path.Add(node.Position);
                        node = node.Parent;
                        if (node == Node.Empty)
                        {
                            return path;
                        }
                    }
                }

                closedNodes[currentNode.Position] = currentNode;
                openNodes.Remove(currentNode.Position);

                foreach (var node in _neighbourFinder.GetNeighbours(_map, currentNode, _map.End))
                {
                    if (closedNodes.ContainsKey(node.Position)
                    ) //Don't need to recheck node if it's already be looked at
                    {
                        continue;
                    }

                    if (openNodes.TryGetValue(node.Position, out var existingOpenNode))
                    {
                        if (!(existingOpenNode?.CostDistance > node.CostDistance))
                        {
                            continue;
                        }

                        sortedOpenNodes.Remove(existingOpenNode, existingOpenNode.CostDistance);
                    }

                    sortedOpenNodes.Enqueue(node, node.CostDistance);
                    openNodes[node.Position] = node;
                }
            }

            return new List<Vector2>();
        }
    }
}