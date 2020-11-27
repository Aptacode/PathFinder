using System;
using System.Collections.Generic;
using System.Numerics;

namespace Aptacode.PathFinder
{
    public static class PathFinder
    {
        public static IEnumerable<Vector2> FindPath(this Map map)
        {
            var sortedOpenNodes = new PriorityQueue<float, Node>();

            sortedOpenNodes.Enqueue(map.Start, map.Start.CostDistance);

            var closedNodes = new Dictionary<Vector2, Node>();
            var openNodes = new Dictionary<Vector2, Node>()
            {
                {map.Start.Position, map.Start}
            };

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

                closedNodes[currentNode.Position] = currentNode;
                openNodes.Remove(currentNode.Position);
                
                foreach (var node in currentNode.GetNeighbours(map, map.End))
                {
                    if (closedNodes.ContainsKey(node.Position)
                    ) //Don't need to recheck node if it's already be looked at
                    {
                        continue;
                    }

                                       
                    if (openNodes.TryGetValue(node.Position, out var existingOpenNode))
                    {
                        if (existingOpenNode?.CostDistance > node.CostDistance)
                        {
                            sortedOpenNodes.Remove(existingOpenNode, existingOpenNode.CostDistance);
                            sortedOpenNodes.Enqueue(node, node.CostDistance);
                            openNodes[node.Position] = node;
                            continue;
                        }

                        continue;
                    }
                    else
                    {
                        sortedOpenNodes.Enqueue(node, node.CostDistance);
                        openNodes[node.Position] = node;
                    }
                    
                    
                }
            }

            return new List<Vector2>();
        }
    }
}