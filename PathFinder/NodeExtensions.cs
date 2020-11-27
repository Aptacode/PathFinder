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

        public static readonly Vector2[] Neighbours =
        {
            new Vector2(0, -1),
            new Vector2(-1, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(-1, -1),
            new Vector2(1, -1),
            new Vector2(-1, 1),
            new Vector2(1, 1)
        };

        public static IEnumerable<Node> GetNeighbours(this Node currentNode, Map map, Node targetNode)
        {
            var successors = new List<Node>();
            var forcedNeighbourCheck = Vector2.Zero;
            foreach (var neighbour in Neighbours)
            {
                var jumpPoint = Jump(map, currentNode, neighbour, map.Start, map.End, forcedNeighbourCheck);
                if (jumpPoint != Node.Empty)
                {
                    forcedNeighbourCheck = Vector2.Zero;
                    successors.Add(jumpPoint);
                }
                else
                {
                    forcedNeighbourCheck = neighbour;
                }
            }

            return successors;

            //var straightNeighbourCost = currentNode.Cost + 1;
            //foreach (var neighbour in StraightNeighbours)
            //{
            //    var neighbourNode = currentNode.GetNeighbourNode(map, targetNode, neighbour, straightNeighbourCost);

            //    if (neighbourNode != Node.Empty)
            //    {
            //        yield return neighbourNode;
            //    }
            //}

            //var diagonalNeighbourCost = currentNode.Cost + Root2;
            //foreach (var neighbour in DiagonalNeighbours)
            //{
            //    var neighbourNode = currentNode.GetNeighbourNode(map, targetNode, neighbour, diagonalNeighbourCost);

            //    if (neighbourNode != Node.Empty)
            //    {
            //        yield return neighbourNode;
            //    }
            //}
        }

        private static Node Jump(Map map, Node currentNode, Vector2 delta, Node start, Node end,
            Vector2 forcedNeighbourCheck)
        {
            var cost = delta.X != 0 && delta.Y != 0 ? Root2 : 1;

            var nextNode = currentNode.GetNeighbourNode(map, end, delta, currentNode.Cost + cost);

            if (nextNode == Node.Empty)
            {
                return Node.Empty;
            }

            if (nextNode.Position == end.Position)
            {
                return nextNode;
            }

            if (delta.X != 0 && delta.Y != 0)
            {
                if (forcedNeighbourCheck.X != 0 && forcedNeighbourCheck.Y != 0)
                {
                    return nextNode;
                }

                if (Jump(map, nextNode, new Vector2(delta.X, 0), start, end, Vector2.Zero) != Node.Empty &&
                    Jump(map, nextNode, new Vector2(0, delta.Y), start, end, Vector2.Zero) != Node.Empty)
                {
                    return nextNode;
                }
            }
            else if (delta.X != 0 || delta.Y != 0)
            {
                if (forcedNeighbourCheck.X == 0 || forcedNeighbourCheck.Y == 0)
                {
                    return nextNode;
                }
            }

            return Jump(map, nextNode, delta, start, end, Vector2.Zero);
        }

        public static Node GetNeighbourNode(this Node currentNode, Map map, Node targetNode, Vector2 neighbour,
            float childCost)
        {
            var position = currentNode.Position + neighbour;
            return map.HasCollision(position)
                ? Node.Empty
                : new Node(position, currentNode, childCost, targetNode.Position);
        }
    }
}