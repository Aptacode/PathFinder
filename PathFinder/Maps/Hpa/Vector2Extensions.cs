using System.Numerics;

namespace Aptacode.PathFinder.Maps.Hpa
{
    public static class Vector2Extensions
    {
        private static readonly Vector2 Up = new(0, -1);
        private static readonly Vector2 Down = new(0, 1);
        private static readonly Vector2 Left = new(-1, 0);
        private static readonly Vector2 Right = new(1, 0);

        public static Vector2 GetAdjacentPoint(this Vector2 point, Direction adjacencyDirection)
        {
            return adjacencyDirection switch
            {
                Direction.Up => point + Up,
                Direction.Right => point + Right,
                Direction.Down => point + Down,
                Direction.Left => point + Left,
                _ => point
            };
        }
    }
}