using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aptacode.Geometry.Collision;
using Aptacode.Geometry.Primitives;
using Aptacode.PathFinder.Geometry;

namespace Aptacode.PathFinder.Maps
{
    public class Map
    {
        private readonly Dictionary<Guid, Primitive> _obstacles;
        public readonly Vector2 Dimensions;
        public readonly Node End;
        public readonly Node Start;
        public CollisionDetector CollisionDetector = new CoarseCollisionDetector();

        public Map(Vector2 dimensions, Vector2 start, Vector2 end, params Primitive[] obstacles)
        {
            Dimensions = dimensions;
            End = Node.EndNode(end);
            Start = Node.StartNode(start, end);

            _obstacles = new Dictionary<Guid, Primitive>();
            foreach (var obstacle in obstacles)
            {
                _obstacles.Add(Guid.NewGuid(), obstacle);
            }
        }

        public IEnumerable<Primitive> Obstacles => _obstacles.Values;

        public bool HasCollision(Point point)
        {
            return _obstacles.Values.Any(obstacle => obstacle.CollidesWith(point, CollisionDetector));
        }

        public bool IsOutOfBounds(Point point) =>
            point.Position.X < 0 || point.Position.Y < 0 || point.Position.X > Dimensions.X || point.Position.Y > Dimensions.Y;

        public bool IsInvalidPosition(Point point) => IsOutOfBounds(point) || HasCollision(point);
    }
}