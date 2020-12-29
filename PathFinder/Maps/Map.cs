using System.Numerics;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components;
using Aptacode.Geometry.Collision;
using Aptacode.Geometry.Primitives;
using Aptacode.PathFinder.Geometry;

namespace Aptacode.PathFinder.Maps
{
    public class Map
    {
        public readonly Vector2 Dimensions;
        public readonly Node End;
        public readonly Node Start;
        public CollisionDetector CollisionDetector = new FineCollisionDetector();

        public Map(Vector2 dimensions, Vector2 start, Vector2 end, params ComponentViewModel[] obstacles)
        {
            Dimensions = dimensions;
            End = Node.EndNode(end);
            Start = Node.StartNode(start, end);
            Obstacles = obstacles;
        }

        public ComponentViewModel[] Obstacles { get; }

        public bool HasCollision(Point point)
        {
            for (var i = 0; i < Obstacles.Length; i++)
            {
                if (Obstacles[i].CollidesWith(point, CollisionDetector))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsOutOfBounds(Point point) =>
            point.Position.X < 0 || point.Position.Y < 0 || point.Position.X > Dimensions.X ||
            point.Position.Y > Dimensions.Y;

        public bool IsInvalidPosition(Point point) => IsOutOfBounds(point) || HasCollision(point);
    }
}