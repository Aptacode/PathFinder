using System.Numerics;

namespace Aptacode.PathFinder.Maps.Hpa
{
    public readonly struct EdgePoint
    {
        public readonly Direction AdjacencyDirection;

        public readonly Vector2 Position;

        public EdgePoint(Direction adjacencyDirection, Vector2 position)
        {
            Position = position;
            AdjacencyDirection = adjacencyDirection;
        }

        public static readonly EdgePoint Empty = new(Direction.None, Vector2.Zero);
    }
}