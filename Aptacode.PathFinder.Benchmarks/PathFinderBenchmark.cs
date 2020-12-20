using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Aptacode.Geometry.Primitives;
using Aptacode.Geometry.Primitives.Polygons;
using Aptacode.PathFinder.Geometry;
using Aptacode.PathFinder.Geometry.Neighbours;
using Aptacode.PathFinder.Maps;
using BenchmarkDotNet.Attributes;

namespace Aptacode.PathFinder.Benchmarks
{
    public class PathFinderBenchmark
    {
        private readonly Map _noObstacles;
        private readonly Map _oneObstacle;
        private readonly Map _threeSmallCubes;
        private readonly Map _largeIShape;
        private readonly Map _smallIShape;
        private readonly Map _cubeField;
        private readonly Map _verticalBars;

        public PathFinderBenchmark()
        {
            _noObstacles = new Map(new Vector2(100, 100), new Vector2(0, 0), new Vector2(100, 100));
            
            _oneObstacle = new Map(new Vector2(100, 100), new Vector2(10, 20), new Vector2(10, 80),
                Rectangle.Create(new Vector2(0, 40), new Vector2(80, 20)));
            
            _threeSmallCubes = new Map(new Vector2(100, 100), new Vector2(5, 5), new Vector2(11, 22),
                Rectangle.Create( new Vector2(6, 6), new Vector2(5, 5)),
                Rectangle.Create( new Vector2(11, 11), new Vector2(5, 5)),
                Rectangle.Create( new Vector2(6, 16), new Vector2(5, 5)));

            _largeIShape = new Map(new Vector2(100, 100), new Vector2(49, 1), new Vector2(52, 1),
                Rectangle.Create(new Vector2(2, 2), new Vector2(96, 1)),
                 Rectangle.Create(  new Vector2(50, 0), new Vector2(1, 2)),
                 Rectangle.Create(  new Vector2(50, 3), new Vector2(1, 95)),
                 Rectangle.Create(new Vector2(2, 98), new Vector2(96, 1)));

                _smallIShape = new Map(new Vector2(100, 100), new Vector2(49, 1), new Vector2(52, 1),
                     Rectangle.Create(  new Vector2(2, 2), new Vector2(96, 1)),
                     Rectangle.Create(  new Vector2(50, 0), new Vector2(1, 2)),
                     Rectangle.Create(  new Vector2(50, 3), new Vector2(1, 95)),
                     Rectangle.Create(new Vector2(2, 98), new Vector2(96, 1)));

            _cubeField = GenerateCubeField();
            _verticalBars = GenerateVerticalBars();
        }

        public Map GenerateVerticalBars()
        {
            var mapBuilder = new MapBuilder();
            const int width = 100;
            const int height = 100;
            mapBuilder.SetDimensions(width, height);
            mapBuilder.SetStart(0, 0);
            mapBuilder.SetEnd(width, height);

            var count = 0;
            for (var i = 2; i < width - 2; i += 4)
            {
                var offset = count++ % 2 == 0 ? 2 : -2;
                mapBuilder.AddObstacle(Rectangle.Create(new Vector2(i, offset) , new Vector2( 2, height - 2)));
            }

            var mapResult = mapBuilder.Build();
            return mapResult.Map;
        }

        public Map GenerateCubeField()
        {
            var mapBuilder = new MapBuilder();
            const int width = 100;
            const int height = 100;
            mapBuilder.SetDimensions(width, height);
            mapBuilder.SetStart(0, 0);
            mapBuilder.SetEnd(100, 100);
            var count = 0;
            for (var i = -5; i < width + 5; i += 5)
            {
                var offset = count++ % 2 == 0 ? 3 : 0;

                for (var j = -5; j < height + 5; j += 5)
                {
                    mapBuilder.AddObstacle(Rectangle.Create(new Vector2(1 + i, offset + j), new Vector2(3, 3)));
                }
            }

            var mapResult = mapBuilder.Build();
            return mapResult.Map;
        }

        [Benchmark]
        public IList<Vector2> NoObstacles()
        {
            return new Algorithm.PathFinder(_noObstacles, DefaultNeighbourFinder.Straight(0.5f)).FindPath().ToList();
        }

        [Benchmark]
        public IList<Vector2> OneObstacle()
        {
            return new Algorithm.PathFinder(_oneObstacle, DefaultNeighbourFinder.Straight(0.5f)).FindPath().ToList();
        }

        [Benchmark]
        public IList<Vector2> ThreeObstacles()
        {
            return new Algorithm.PathFinder(_threeSmallCubes, DefaultNeighbourFinder.Straight(0.5f)).FindPath().ToList();
        }

        [Benchmark]
        public IList<Vector2> SmallIShape()
        {
            return new Algorithm.PathFinder(_smallIShape, DefaultNeighbourFinder.Straight(0.5f)).FindPath().ToList();
        }
        
        [Benchmark]
        public IList<Vector2> LargeIShape()
        {
            return new Algorithm.PathFinder(_largeIShape, DefaultNeighbourFinder.Straight(0.5f)).FindPath().ToList();
        }

        [Benchmark]
        public IList<Vector2> CubeField()
        {
            return new Algorithm.PathFinder(_cubeField, DefaultNeighbourFinder.Straight(0.5f)).FindPath().ToList();

        }

        [Benchmark]
        public IList<Vector2> VerticalBars()
        {
            return new Algorithm.PathFinder(_verticalBars, DefaultNeighbourFinder.Straight(0.5f)).FindPath().ToList();
        }
    }
}
