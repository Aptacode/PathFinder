using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PathFinder
{
    public class Map
    {
        private readonly Dictionary<Guid, Obstacle> _obstacles;
        public readonly Vector2 Dimensions;
        public readonly Node End;

        public readonly Node Start;

        public Map(int width, int height, Vector2 start, Vector2 end, params Obstacle[] obstacles)
        {
            Dimensions = new Vector2(width, height);
            End = Node.EndNode(end);
            Start = Node.StartNode(start, end);

            _obstacles = new Dictionary<Guid, Obstacle>();
            foreach (var obstacle in obstacles)
            {
                _obstacles.Add(obstacle.Id, obstacle);
            }

            //ToDo: error handle edge cases
        }

        public IEnumerable<Obstacle> Obstacles => _obstacles.Values;

        public bool HasCollision(Vector2 point)
        {
            return _obstacles.Values.Any(obstacle => obstacle.CollidesWith(point));
        }
    }
}