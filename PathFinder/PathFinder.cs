using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Aptacode.PathFinder
{
    public static class PathFinder
    {
        public static IEnumerable<Vector2> FindPath(this Map map)
        {
            var sortedOpenNodes = new PriorityQueue<float, Node>();
            sortedOpenNodes.Enqueue(map.Start, map.Start.CostDistance);

            var closedNodes = new List<Node>();
            var openNodes = new List<Node> {map.Start};

            while (!sortedOpenNodes.IsEmpty())
            {
                var currentNode = sortedOpenNodes.Dequeue();
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
                    sortedOpenNodes.Enqueue(node, node.CostDistance);
                }
            }

            return new List<Vector2>();
        }
    }
}