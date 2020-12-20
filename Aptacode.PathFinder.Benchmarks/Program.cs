using BenchmarkDotNet.Running;

namespace Aptacode.PathFinder.Benchmarks
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<PathFinderBenchmark>();
        }
    }
}