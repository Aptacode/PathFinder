using System;
using System.Collections.Generic;
using System.Numerics;
using Aptacode.PathFinder.Maps;
using Aptacode.PathFinder.Utilities;

namespace Aptacode.PathFinder.Geometry.Neighbours
{
    public class JumpPointSearchNeighbourFinder : INeighbourFinder
    {
        public readonly Vector2[] AllowedNeighbours;
        public readonly float DiagonalCost;
        public readonly float StraightCost;


        internal JumpPointSearchNeighbourFinder(AllowedDirections allowedDirections, float straightCost,
            float diagonalCost)
        {
            AllowedNeighbours = NeighbourKernels.GetNeighbours(allowedDirections);
            StraightCost = straightCost;
            DiagonalCost = diagonalCost;
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

        public static JumpPointSearchNeighbourFinder Diagonal(float cost)
        {
            return new(AllowedDirections.Diagonal, 0.0f, cost);
        }

        public static JumpPointSearchNeighbourFinder Straight(float cost)
        {
            return new(AllowedDirections.Straight, cost, 0.0f);
        }

        public static JumpPointSearchNeighbourFinder All(float straightCost, float diagonalCost)
        {
            return new(AllowedDirections.All, straightCost, diagonalCost);
        }

        private Node Jump(Map map, Node currentNode, Vector2 delta, Node start, Node end,
            Vector2 forcedNeighbourCheck)
        {
            var cost =
                Math.Abs(delta.X) > Constants.Tolerance &&
                Math.Abs(delta.Y) > Constants.Tolerance
                    ? DiagonalCost
                    : StraightCost;

            var nextNode = currentNode.GetNeighbourNode(map, end, delta, currentNode.Cost + cost);

            if (nextNode == Node.Empty)
            {
                return Node.Empty;
            }

            if (nextNode.Position == end.Position)
            {
                return nextNode;
            }

            if (Math.Abs(delta.X) > Constants.Tolerance && Math.Abs(delta.Y) > Constants.Tolerance)
            {
                if (Math.Abs(forcedNeighbourCheck.X) > Constants.Tolerance &&
                    Math.Abs(forcedNeighbourCheck.Y) > Constants.Tolerance)
                {
                    return nextNode;
                }

                if (Jump(map, nextNode, new Vector2(delta.X, 0), start, end, forcedNeighbourCheck) != Node.Empty &&
                    Jump(map, nextNode, new Vector2(0, delta.Y), start, end, forcedNeighbourCheck) != Node.Empty)
                {
                    return nextNode;
                }
            }
            else if (Math.Abs(delta.X) > Constants.Tolerance || Math.Abs(delta.Y) > Constants.Tolerance)
            {
                if (Math.Abs(forcedNeighbourCheck.X) < Constants.Tolerance ||
                    Math.Abs(forcedNeighbourCheck.Y) < Constants.Tolerance)
                {
                    return nextNode;
                }
            }

            return Jump(map, nextNode, delta, start, end, forcedNeighbourCheck);
        }
    }
}