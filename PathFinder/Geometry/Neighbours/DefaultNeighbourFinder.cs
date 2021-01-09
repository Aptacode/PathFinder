using System.Collections.Generic;
using Aptacode.PathFinder.Maps;

namespace Aptacode.PathFinder.Geometry.Neighbours
{
    public class DefaultNeighbourFinder : INeighbourFinder
    {
        public readonly AllowedDirections AllowedDirections;
        public readonly float DiagonalCost;
        public readonly float StraightCost;

        internal DefaultNeighbourFinder(AllowedDirections allowedDirections, float straightCost, float diagonalCost)
        {
            AllowedDirections = allowedDirections;
            StraightCost = straightCost;
            DiagonalCost = diagonalCost;
        }

        public IEnumerable<Node> GetNeighbours(Map map, Node currentNode, Node targetNode)
        {
            if (AllowedDirections == AllowedDirections.All || AllowedDirections == AllowedDirections.Straight)
            {
                var straightNeighbourCost = currentNode.Cost + StraightCost;
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
                var diagonalNeighbourCost = currentNode.Cost + DiagonalCost;
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

        public static DefaultNeighbourFinder Diagonal(float cost)
        {
            return new(AllowedDirections.Diagonal, 0.0f, cost);
        }

        public static DefaultNeighbourFinder Straight(float cost)
        {
            return new(AllowedDirections.Straight, cost, 0.0f);
        }

        public static DefaultNeighbourFinder All(float straightCost, float diagonalCost)
        {
            return new(AllowedDirections.All, straightCost, diagonalCost);
        }
    }
}