using System.Numerics;
using Aptacode.Geometry.Primitives;
using Aptacode.PathFinder.Maps;

namespace PathFinder.Tests.Helpers
{
    public static class Map_Helpers
    {
        public static Map StartPoint_OutOfBounds_Map =>
            new(new Vector2(10, 10), new Vector2(-1, 0), new Vector2(5, 5));

        public static Map EndPoint_OutOfBounds_Map =>
            new(new Vector2(10, 10), new Vector2(5, 0), new Vector2(-5, 5));

        public static Map StartPoint_HasCollision_WithObstacle_Map =>
            new(new Vector2(10, 10), new Vector2(1, 1), new Vector2(5, 5),
                new Point(new Vector2(1, 1)));

        public static Map EndPoint_HasCollision_WithObstacle_Map =>
            new(new Vector2(10, 10), new Vector2(1, 1), new Vector2(5, 5),
                new Point(new Vector2(5, 5)));

        public static Map Valid_No_Obstacles_Map =>
            new(new Vector2(10, 10), new Vector2(1, 0), new Vector2(5, 5));
    }
}