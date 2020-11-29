using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Aptacode.PathFinder.Geometry;
using Aptacode.PathFinder.Geometry.Neighbours;
using Aptacode.PathFinder.Utilities;

namespace PathFinder.Demo
{
    internal class PathFinderDemo
    {
        private readonly IReadOnlyList<(string name, Map map)> _maps;
        private readonly List<List<PathFinderResult>> _pathFinderResults = new List<List<PathFinderResult>>();

        public PathFinderDemo()
        {
            _maps = new List<(string name, Map map)>
            {
                ("1",
                    new Map(new Vector2(100, 100), new Vector2(0, 0), new Vector2(100, 100))),
                ("2",
                    new Map(new Vector2(100, 100), new Vector2(0, 0), new Vector2(100, 100),
                        new Obstacle(Guid.NewGuid(), new Vector2(40, 40), new Vector2(20, 20)))),
                ("3",
                    new Map(new Vector2(100, 100), new Vector2(10, 20), new Vector2(10, 80),
                        new Obstacle(Guid.NewGuid(), new Vector2(0, 40), new Vector2(80, 20)))),
                ("4",
                    new Map(new Vector2(100, 100), new Vector2(5, 5), new Vector2(90, 90),
                        new Obstacle(Guid.NewGuid(), new Vector2(5, 20), new Vector2(40, 50)),
                        new Obstacle(Guid.NewGuid(), new Vector2(50, 10), new Vector2(20, 60)),
                        new Obstacle(Guid.NewGuid(), new Vector2(75, 75), new Vector2(10, 10)))),
                ("5",
                    new Map(new Vector2(100, 100), new Vector2(5, 5), new Vector2(11, 22),
                        new Obstacle(Guid.NewGuid(), new Vector2(6, 6), new Vector2(5, 5)),
                        new Obstacle(Guid.NewGuid(), new Vector2(11, 11), new Vector2(5, 5)),
                        new Obstacle(Guid.NewGuid(), new Vector2(6, 16), new Vector2(5, 5)))),
                ("6",
                    new Map(new Vector2(100, 100), new Vector2(49, 1), new Vector2(52, 1),
                        new Obstacle(Guid.NewGuid(), new Vector2(2, 2), new Vector2(96, 1)),
                        new Obstacle(Guid.NewGuid(), new Vector2(50, 0), new Vector2(1, 2)),
                        new Obstacle(Guid.NewGuid(), new Vector2(50, 3), new Vector2(1, 95)),
                        new Obstacle(Guid.NewGuid(), new Vector2(2, 98), new Vector2(96, 1)))),
                ("7",
                    new Map(new Vector2(100, 100), new Vector2(4, 1), new Vector2(7, 1),
                        new Obstacle(Guid.NewGuid(), new Vector2(1, 2), new Vector2(9, 1)),
                        new Obstacle(Guid.NewGuid(), new Vector2(5, 0), new Vector2(1, 2)),
                        new Obstacle(Guid.NewGuid(), new Vector2(5, 3), new Vector2(1, 6)),
                        new Obstacle(Guid.NewGuid(), new Vector2(1, 9), new Vector2(9, 1))))
            };
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
                DefaultNeighbourFinder.Straight(1.0f)).FindPath().ToList();
            timer.Stop();

            var totalLength = path.Zip(path.Skip(1), (a, b) => a - b).Select(s => s.Length()).Sum();

            return new PathFinderResult(name, map, path, timer.ElapsedMilliseconds, (int) totalLength, path.Count);
        }
    }
}