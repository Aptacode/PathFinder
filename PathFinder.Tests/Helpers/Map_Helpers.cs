using System;
using System.Numerics;
using Aptacode.PathFinder.Geometry;
using Aptacode.PathFinder.Map;

namespace PathFinder.Tests.Helpers
{
    public static class Map_Helpers
    {
        public static Map StartPoint_OutOfBounds_Map =>
            new Map(new Vector2(10, 10), new Vector2(-1, 0), new Vector2(5, 5));

        public static Map EndPoint_OutOfBounds_Map =>
           new Map (new Vector2(10, 10), new Vector2(5, 0), new Vector2(-5, 5));

        public static Map StartPoint_HasCollision_WithObstacle_Map =>
            new Map(new Vector2(10, 10), new Vector2(1, 1), new Vector2(5, 5),
                new Obstacle(Guid.NewGuid(), new Vector2(1, 1), new Vector2(0, 0)));

        public static Map EndPoint_HasCollision_WithObstacle_Map =>
            new Map(new Vector2(10, 10), new Vector2(1, 1), new Vector2(5, 5),
                new Obstacle(Guid.NewGuid(), new Vector2(5, 5), new Vector2(0, 0)));
        public static Map Valid_No_Obstacles_Map =>
            new Map(new Vector2(10, 10), new Vector2(1, 0), new Vector2(5, 5));

    }
}