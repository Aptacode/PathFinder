namespace PathFinder.Demo
{
    public class PathFinderResult
    {
        public readonly string Name;
        public readonly int TotalLength;
        public readonly int TotalPoints;
        public readonly long TotalTime;

        public PathFinderResult(string name, long totalTime, int totalLength, int totalPoints)
        {
            Name = name;
            TotalTime = totalTime;
            TotalLength = totalLength;
            TotalPoints = totalPoints;
        }

        public override string ToString() =>
            $"Map: {Name}\t Duration: {TotalTime}ms\t Length: {TotalLength}\t Nodes: {TotalPoints}";
    }
}