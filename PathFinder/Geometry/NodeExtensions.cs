﻿using System.Numerics;
using Aptacode.Geometry.Primitives;
using Aptacode.PathFinder.Maps;

namespace Aptacode.PathFinder.Geometry
{
    public static class NodeExtensions
    {
        public static Node GetNeighbourNode(this Node currentNode, Map map, Node targetNode, Vector2 neighbour,
            float childCost)
        {
            var position = currentNode.Position + neighbour;
            return map.IsInvalidPosition(new Point(position))
                ? Node.Empty
                : new Node(currentNode, position, targetNode.Position, childCost);
        }
    }
}