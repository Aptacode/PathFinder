using System.Numerics;

namespace Aptacode.PathFinder.Geometry.Neighbours
{
    public static class NeighbourKernels
    {
        public static readonly Vector2[] Diagonal =
        {
            new Vector2(-1, -1),
            new Vector2(1, -1),
            new Vector2(-1, 1),
            new Vector2(1, 1)
        };

        public static readonly Vector2[] Straight =
        {
            new Vector2(0, -1),
            new Vector2(-1, 0),
            new Vector2(1, 0),
            new Vector2(0, 1)
        };

        public static readonly Vector2[] All =
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

        public static Vector2[] GetNeighbours(AllowedDirections allowedDirections)
        {
            return allowedDirections switch
            {
                AllowedDirections.Straight => Straight,
                AllowedDirections.Diagonal => Diagonal,
                _ => All
            };
        }
    }
}