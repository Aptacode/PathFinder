using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aptacode.PathFinder.Geometry;

namespace Aptacode.PathFinder.Utilities
{
    public class MapBuilder
    {
        private readonly Dictionary<Guid, Obstacle> _obstacles;
        private Vector2 _dimensions;
        private Vector2 _end;
        private Vector2 _start;

        public MapBuilder()
        {
            _obstacles = new Dictionary<Guid, Obstacle>();
            _dimensions = new Vector2(0, 0);
            _start = Vector2.Zero;
            _end = Vector2.Zero;
        }

        public MapBuilder SetStart(int x, int y)
        {
            _start = new Vector2(x, y);
            return this;
        }

        public MapBuilder SetEnd(int x, int y)
        {
            _end = new Vector2(x, y);
            return this;
        }

        public MapBuilder SetDimensions(int width, int height)
        {
            _dimensions = new Vector2(width, height);
            return this;
        }

        public MapBuilder AddObstacle(int x, int y, int width, int height)
        {
            var newObstacle = new Obstacle(Guid.NewGuid(), new Vector2(x, y), new Vector2(width, height));
            _obstacles.Add(newObstacle.Id, newObstacle);
            return this;
        }

        public Map Build()
        {
            var map = new Map(_dimensions, _start, _end, _obstacles.Values.ToArray());
            Reset();
            return map;
        }

        public void Reset()
        {
            _obstacles.Clear();
            _dimensions = new Vector2(0, 0);
            _start = Vector2.Zero;
            _end = Vector2.Zero;
        }
    }
}