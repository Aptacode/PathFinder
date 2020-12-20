using System;
using BenchmarkDotNet.Running;

namespace Aptacode.PathFinder.Benchmarks
{
    class Program
    {
        private static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<PathFinderBenchmark>();
        }
    }
}
