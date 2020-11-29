using System;
using System.Collections.Generic;
using System.Numerics;

namespace Aptacode.PathFinder.Geometry.Neighbours
{
    public class JumpPointSearchNeighbourFinder : INeighbourFinder
    {
        public readonly float StraightCost;
        public readonly float DiagonalCost;
        public readonly Vector2[] AllowedNeighbours;


        internal JumpPointSearchNeighbourFinder(AllowedDirections allowedDirections, float straightCost, float diagonalCost)
        {
            AllowedNeighbours = NeighbourKernels.GetNeighbours(allowedDirections);
            StraightCost = straightCost;
            DiagonalCost = diagonalCost;
        }

        public static JumpPointSearchNeighbourFinder Diagonal(float cost)
        {
            return new JumpPointSearchNeighbourFinder(AllowedDirections.Diagonal, 0.0f, cost);
        }

        public static JumpPointSearchNeighbourFinder Straight(float cost)
        {
            return new JumpPointSearchNeighbourFinder(AllowedDirections.Straight, cost, 0.0f);
        }

        public static JumpPointSearchNeighbourFinder All(float straightCost, float diagonalCost)
        {
            return new JumpPointSearchNeighbourFinder(AllowedDirections.All, straightCost, diagonalCost);
        }

        public IEnumerable<Node> GetNeighbours(Map map, Node currentNode, Node targetNode)
        {
            var successors = new List<Node>();
            var forcedNeighbourCheck = Vector2.Zero;
            foreach (var neighbour in AllowedNeighbours)
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
        }

        private Node Jump(Map map, Node currentNode, Vector2 delta, Node start, Node end,
            Vector2 forcedNeighbourCheck)
        {
            var cost = delta.X != 0 && delta.Y != 0 ? DiagonalCost : StraightCost;

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
    }
}