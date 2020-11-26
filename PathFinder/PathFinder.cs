using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Aptacode.PathFinder
{
    public static class PathFinder
    {
        public static IEnumerable<Vector2> FindPath(this Map map)
        {
            var openNodes = new List<Node> {map.Start};

            var closedNodes = new List<Node>();

            while (openNodes.Count > 0)
            {
                var currentNode = openNodes.OrderBy(x => x.CostDistance).ThenBy(x => x.Cost).First();
                if (currentNode.Position == map.End.Position) //if we've reached the end node a path has been found.
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

                closedNodes.Add(currentNode);
                openNodes.Remove(currentNode);

                foreach (var node in currentNode.GetNeighbours(map, map.End))
                {
                    if (closedNodes.Any(x => x.Position == node.Position)
                    ) //Don't need to recheck node if it's already be looked at
                    {
                        continue;
                    }

                    var currentBestNode = openNodes.Find(x => x.Position == node.Position);
                    if (currentBestNode?.CostDistance > currentNode.CostDistance)
                    {
                        continue;
                    }

                    openNodes.Remove(currentBestNode);
                    openNodes.Add(node);
                }
            }

            return new List<Vector2>();
        }
    }
}