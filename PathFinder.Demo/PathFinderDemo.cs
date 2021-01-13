using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Aptacode.AppFramework.Components.Primitives;
using Aptacode.Geometry.Primitives.Polygons;
using Aptacode.PathFinder.Geometry.Neighbours;
using Aptacode.PathFinder.Maps;

namespace PathFinder.ConsoleDemo
{
    internal class PathFinderDemo
    {
        private readonly List<(string name, Map map)> _maps;
        private readonly List<List<PathFinderResult>> _pathFinderResults = new();

        public PathFinderDemo()
        {
            _maps = new List<(string name, Map map)>
            {
                ("1",
                    new Map(new Vector2(100, 100), new Vector2(0, 0), new Vector2(100, 100))),
                ("2",
                    new Map(new Vector2(100, 100), new Vector2(0, 0), new Vector2(100, 100),
                        Rectangle.Create(new Vector2(40, 40), new Vector2(20, 20)).ToViewModel())),
                ("3",
                    new Map(new Vector2(100, 100), new Vector2(10, 20), new Vector2(10, 80),
                        Rectangle.Create(new Vector2(0, 40), new Vector2(80, 20)).ToViewModel())),
                ("4",
                    new Map(new Vector2(100, 100), new Vector2(5, 5), new Vector2(90, 90),
                        Rectangle.Create(new Vector2(5, 20), new Vector2(40, 50)).ToViewModel(),
                        Rectangle.Create(new Vector2(50, 10), new Vector2(20, 60)).ToViewModel(),
                        Rectangle.Create(new Vector2(75, 75), new Vector2(10, 10)).ToViewModel())),
                ("5",
                    new Map(new Vector2(100, 100), new Vector2(5, 5), new Vector2(11, 22),
                        Rectangle.Create(new Vector2(6, 6), new Vector2(5, 5)).ToViewModel(),
                        Rectangle.Create(new Vector2(11, 11), new Vector2(5, 5)).ToViewModel(),
                        Rectangle.Create(new Vector2(6, 16), new Vector2(5, 5)).ToViewModel())),
                ("6",
                    new Map(new Vector2(100, 100), new Vector2(49, 1), new Vector2(52, 1),
                        Rectangle.Create(new Vector2(2, 2), new Vector2(96, 1)).ToViewModel(),
                        Rectangle.Create(new Vector2(50, 0), new Vector2(1, 2)).ToViewModel(),
                        Rectangle.Create(new Vector2(50, 3), new Vector2(1, 95)).ToViewModel(),
                        Rectangle.Create(new Vector2(2, 98), new Vector2(96, 1)).ToViewModel())),
                ("7",
                    new Map(new Vector2(100, 100), new Vector2(4, 1), new Vector2(7, 1),
                        Rectangle.Create(new Vector2(1, 2), new Vector2(9, 1)).ToViewModel(),
                        Rectangle.Create(new Vector2(5, 0), new Vector2(1, 2)).ToViewModel(),
                        Rectangle.Create(new Vector2(5, 3), new Vector2(1, 6)).ToViewModel(),
                        Rectangle.Create(new Vector2(1, 9), new Vector2(9, 1)).ToViewModel())),
                ("8", GenerateCubeField()),
                ("9", GenerateVerticalBars())
            };
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

        public void RunAll()
        {
            Console.WriteLine("====================================================================");

            var pathFinderResults = new List<PathFinderResult>();
            var timer = new Stopwatch();
            timer.Start();

            foreach (var (name, map) in _maps)
            {
                var output = Run(name, map);
                pathFinderResults.Add(output);
                Console.WriteLine(output.ToString());
            }

            timer.Stop();
            Console.WriteLine($"Total Elapsed: {timer.ElapsedMilliseconds}ms");
            _pathFinderResults.Add(pathFinderResults);
        }

        public List<List<PathFinderResult>> RunAll(int times)
        {
            var timer = new Stopwatch();

            timer.Start();
            for (var i = 0; i < times; i++)
            {
                RunAll();
            }

            timer.Stop();

            Console.WriteLine("====================================================================");
            Console.WriteLine("Average");

            for (var i = 0; i < _maps.Count; i++)
            {
                var name = _maps[i].name;
                var averageTime = 0L;
                var length = 0;
                var points = 0;

                var minTime = long.MaxValue;
                var maxTime = 0L;

                foreach (var result in _pathFinderResults.Select(resultList => resultList[i]))
                {
                    averageTime += result.TotalTime;
                    length = result.TotalLength;
                    points = result.TotalPoints;

                    if (result.TotalTime < minTime)
                    {
                        minTime = result.TotalTime;
                    }

                    if (result.TotalTime > maxTime)
                    {
                        maxTime = result.TotalTime;
                    }
                }

                averageTime /= times;
                Console.WriteLine(
                    $"Map: {name}\t Duration: min {minTime}ms av {averageTime}ms max {maxTime}ms\t Length: {length}\t Nodes: {points}");
            }

            Console.WriteLine("====================================================================");
            Console.WriteLine($"Total Elapsed: {timer.ElapsedMilliseconds}ms");
            Console.WriteLine("====================================================================");

            return _pathFinderResults;
        }

        public PathFinderResult Run(string name, Map map)
        {
            var timer = new Stopwatch();
            timer.Start();
            var path = new Aptacode.PathFinder.Algorithm.PathFinder(map,
                DefaultNeighbourFinder.Straight(0.5f)).FindPath().ToList();
            timer.Stop();

            var totalLength = path.Zip(path.Skip(1), (a, b) => a - b).Select(s => s.Length()).Sum();

            return new PathFinderResult(name, map, path, timer.ElapsedMilliseconds, (int) totalLength, path.Count);
        }
    }
}