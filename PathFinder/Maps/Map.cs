using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aptacode.PathFinder.Geometry;

namespace Aptacode.PathFinder.Maps
{
    public class Map
    {
        private readonly Dictionary<Guid, Obstacle> _obstacles;
        public readonly Vector2 Dimensions;
        public readonly Node End;
        public readonly Node Start;

        public Map(Vector2 dimensions, Vector2 start, Vector2 end, params Obstacle[] obstacles)
        {
            Dimensions = dimensions;
            End = Node.EndNode(end);
            Start = Node.StartNode(start, end);

            _obstacles = new Dictionary<Guid, Obstacle>();
            foreach (var obstacle in obstacles)
            {
                _obstacles.Add(obstacle.Id, obstacle);
            }
        }

        public IEnumerable<Obstacle> Obstacles => _obstacles.Values;

        public bool HasCollision(Vector2 point)
        {
            return _obstacles.Values.Any(obstacle => obstacle.CollidesWith(point));
        }

        public bool IsOutOfBounds(Vector2 point) =>
            point.X < 0 || point.Y < 0 || point.X > Dimensions.X || point.Y > Dimensions.Y;

        public bool IsInvalidPosition(Vector2 point) => IsOutOfBounds(point) || HasCollision(point);
    }
}