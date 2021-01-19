using System.Numerics;

namespace Aptacode.PathFinder.Maps.Hpa
{
    public static class Vector2Extensions
    {
        private static readonly Vector2 _up = new(0, -1);
        private static readonly Vector2 _down = new(0, 1);
        private static readonly Vector2 _left = new(-1, 0);
        private static readonly Vector2 _right = new(1, 0);

        public static Vector2 GetAdjacentPoint(this Vector2 point, Direction adjacencyDirection)
        {
            return adjacencyDirection switch
            {
                Direction.Up => point + _up,
                Direction.Right => point + _right,
                Direction.Down => point + _down,
                Direction.Left => point + _left,
                _ => point
            };
        }
    }
}