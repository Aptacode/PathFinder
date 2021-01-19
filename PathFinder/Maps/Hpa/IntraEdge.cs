using System;
using System.Numerics;

namespace Aptacode.PathFinder.Maps.Hpa
{
    public record IntraEdge(Direction AdjacencyDirection, Vector2[] Path)
    {
        public static IntraEdge Empty = new(Direction.None, Array.Empty<Vector2>());
    }
}