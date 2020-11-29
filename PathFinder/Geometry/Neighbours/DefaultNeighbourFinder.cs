using System;
using System.Collections.Generic;

namespace Aptacode.PathFinder.Geometry.Neighbours
{
    public class DefaultNeighbourFinder : INeighbourFinder
    {
        public static readonly float Root2 = (float) Math.Sqrt(2);
        public readonly AllowedDirections AllowedDirections;

        public DefaultNeighbourFinder(AllowedDirections allowedDirections)
        {
            AllowedDirections = allowedDirections;
        }

        public IEnumerable<Node> GetNeighbours(Map map, Node currentNode, Node targetNode)
        {
            if (AllowedDirections == AllowedDirections.All || AllowedDirections == AllowedDirections.Straight)
            {
                var straightNeighbourCost = currentNode.Cost + 1;
                foreach (var neighbour in NeighbourKernels.Straight)
                {
                    var neighbourNode = currentNode.GetNeighbourNode(map, targetNode, neighbour, straightNeighbourCost);

                    if (neighbourNode != Node.Empty)
                    {
                        yield return neighbourNode;
                    }
                }
            }


            if (AllowedDirections == AllowedDirections.All || AllowedDirections == AllowedDirections.Diagonal)
            {
                var diagonalNeighbourCost = currentNode.Cost + Root2;
                foreach (var neighbour in NeighbourKernels.Diagonal)
                {
                    var neighbourNode = currentNode.GetNeighbourNode(map, targetNode, neighbour, diagonalNeighbourCost);

                    if (neighbourNode != Node.Empty)
                    {
                        yield return neighbourNode;
                    }
                }
            }
        }
    }
}