using System.Numerics;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components.Primitives;
using Aptacode.Geometry.Primitives.Polygons;
using Aptacode.PathFinder.Geometry.Neighbours;
using Aptacode.PathFinder.Maps;
using BenchmarkDotNet.Attributes;

namespace Aptacode.PathFinder.Benchmarks
{
    public class PathFinderBenchmark
    {
        private readonly Map _cubeField;
        private readonly Map _largeIShape;
        private readonly Map _noObstacles;
        private readonly Map _oneObstacle;
        private readonly Map _smallIShape;
        private readonly Map _threeSmallCubes;
        private readonly Map _verticalBars;

        public PathFinderBenchmark()
        {
            _noObstacles = new Map(new Vector2(100, 100), new Vector2(0, 0), new Vector2(100, 100));

            _oneObstacle = new Map(new Vector2(100, 100), new Vector2(10, 20), new Vector2(10, 80),
                Rectangle.Create(new Vector2(0, 40), new Vector2(80, 20)).ToViewModel());

            _threeSmallCubes = new Map(new Vector2(100, 100), new Vector2(5, 5), new Vector2(11, 22),
                Rectangle.Create(new Vector2(6, 6), new Vector2(5, 5)).ToViewModel(),
                Rectangle.Create(new Vector2(11, 11), new Vector2(5, 5)).ToViewModel(),
                Rectangle.Create(new Vector2(6, 16), new Vector2(5, 5)).ToViewModel());

            _largeIShape = new Map(new Vector2(100, 100), new Vector2(49, 1), new Vector2(52, 1),
                Rectangle.Create(new Vector2(2, 2), new Vector2(96, 1)).ToViewModel(),
                Rectangle.Create(new Vector2(50, 0), new Vector2(1, 2)).ToViewModel(),
                Rectangle.Create(new Vector2(50, 3), new Vector2(1, 95)).ToViewModel(),
                Rectangle.Create(new Vector2(2, 98), new Vector2(96, 1)).ToViewModel());

            _smallIShape = new Map(new Vector2(100, 100), new Vector2(49, 1), new Vector2(52, 1),
                Rectangle.Create(new Vector2(2, 2), new Vector2(96, 1)).ToViewModel(),
                Rectangle.Create(new Vector2(50, 0), new Vector2(1, 2)).ToViewModel(),
                Rectangle.Create(new Vector2(50, 3), new Vector2(1, 95)).ToViewModel(),
                Rectangle.Create(new Vector2(2, 98), new Vector2(96, 1)).ToViewModel());

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
                mapBuilder.AddObstacle(Rectangle.Create(new Vector2(i, offset), new Vector2(2, height - 2))
                    .ToViewModel());
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
                    mapBuilder.AddObstacle(Rectangle.Create(new Vector2(1 + i, offset + j), new Vector2(3, 3))
                        .ToViewModel());
                }
            }

            var mapResult = mapBuilder.Build();
            return mapResult.Map;
        }

        [Benchmark]
        public Vector2[] NoObstacles() =>
            new Algorithm.PathFinder(_noObstacles, DefaultNeighbourFinder.Straight(0.5f)).FindPath();

        [Benchmark]
        public Vector2[] OneObstacle() =>
            new Algorithm.PathFinder(_oneObstacle, DefaultNeighbourFinder.Straight(0.5f)).FindPath();

        [Benchmark]
        public Vector2[] ThreeObstacles() =>
            new Algorithm.PathFinder(_threeSmallCubes, DefaultNeighbourFinder.Straight(0.5f)).FindPath();

        [Benchmark]
        public Vector2[] SmallIShape() =>
            new Algorithm.PathFinder(_smallIShape, DefaultNeighbourFinder.Straight(0.5f)).FindPath();

        [Benchmark]
        public Vector2[] LargeIShape() =>
            new Algorithm.PathFinder(_largeIShape, DefaultNeighbourFinder.Straight(0.5f)).FindPath();

        [Benchmark]
        public Vector2[] CubeField() =>
            new Algorithm.PathFinder(_cubeField, DefaultNeighbourFinder.Straight(0.5f)).FindPath();

        [Benchmark]
        public Vector2[] VerticalBars() =>
            new Algorithm.PathFinder(_verticalBars, DefaultNeighbourFinder.Straight(0.5f)).FindPath();
    }
}