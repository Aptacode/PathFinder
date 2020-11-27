using System;
using System.Collections.Generic;
using System.Numerics;

namespace Aptacode.PathFinder
{
    public static class NodeExtensions
    {
        public static readonly float Root2 = (float) Math.Sqrt(2);

        public static readonly Vector2[] DiagonalNeighbours =
        {
            new Vector2(-1, -1),
            new Vector2(1, -1),
            new Vector2(-1, 1),
            new Vector2(1, 1)
        };

        public static readonly Vector2[] StraightNeighbours =
        {
            new Vector2(0, -1),
            new Vector2(-1, 0),
            new Vector2(1, 0),
            new Vector2(0, 1)
        };

        public static IEnumerable<Node> GetNeighbours(this Node currentNode, Map map, Node targetNode)
        {
            var straightNeighbourCost = currentNode.Cost + 1;
            foreach (var neighbour in StraightNeighbours)
            {
                var neighbourNode = currentNode.GetNeighbourNode(map, targetNode, neighbour, straightNeighbourCost);

                if (neighbourNode != Node.Empty)
                {
                    yield return neighbourNode;
                }
            }

            var diagonalNeighbourCost = currentNode.Cost + Root2;
            foreach (var neighbour in DiagonalNeighbours)
            {
                var neighbourNode = currentNode.GetNeighbourNode(map, targetNode, neighbour, diagonalNeighbourCost);

                if (neighbourNode != Node.Empty)
                {
                    yield return neighbourNode;
                }
            }
        }

        public static Node GetNeighbourNode(this Node currentNode, Map map, Node targetNode, Vector2 neighbour,
            float childCost)
        {
            var position = currentNode.Position + neighbour;
            if (position.X < 0 || position.Y < 0 || position.X > map.Dimensions.X || position.Y > map.Dimensions.Y ||
                map.HasCollision(position))
            {
                return Node.Empty;
            }

            return new Node(position, currentNode, childCost, targetNode.Position);
        }
    }
}