using System;
using System.Numerics;

namespace Aptacode.PathFinder.Geometry
{
    public struct Obstacle
    {
        public Obstacle(Guid id, Vector2 position, Vector2 dimensions)
        {
            Id = id;
            Position = position;
            Dimensions = dimensions;
        }

        public readonly Guid Id;
        public readonly Vector2 Position;
        public readonly Vector2 Dimensions;

        public bool CollidesWith(Vector2 point)
        {
            var delta = Position + Dimensions;
            return point.X >= Position.X && point.Y >= Position.Y && point.X <= delta.X && point.Y <= delta.Y;
        }
    }
}