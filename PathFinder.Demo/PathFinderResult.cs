using System.Collections.Generic;
using System.Numerics;
using Aptacode.PathFinder;

namespace PathFinder.Demo
{
    public class PathFinderResult
    {
        public readonly Map Map;
        public readonly string Name;
        public readonly IReadOnlyList<Vector2> Path;
        public readonly int TotalLength;
        public readonly int TotalPoints;
        public readonly long TotalTime;

        public PathFinderResult(string name, Map map, IReadOnlyList<Vector2> path, long totalTime, int totalLength,
            int totalPoints)
        {
            Name = name;
            TotalTime = totalTime;
            TotalLength = totalLength;
            TotalPoints = totalPoints;
            Map = map;
            Path = path;
        }

        public override string ToString() =>
            $"Map: {Name}\t Duration: {TotalTime}ms\t Length: {TotalLength}\t Nodes: {TotalPoints}";
    }
}