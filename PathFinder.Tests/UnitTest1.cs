using System;
using System.Diagnostics;
using System.Numerics;
using Aptacode.PathFinder.Geometry;
using Aptacode.PathFinder.Maps;
using Xunit;
using Xunit.Abstractions;

namespace PathFinder.Tests
{
    public class PerformanceTests
    {
        public PerformanceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private readonly ITestOutputHelper _testOutputHelper;

        [Fact]
        public void Test1()
        {
            var timer = new Stopwatch();
            timer.Start();

            var map = new Map(new Vector2(100, 100), new Vector2(10, 10), new Vector2(90, 90),
                new Obstacle(Guid.NewGuid(), new Vector2(30, 30), new Vector2(20, 50)));

            new Aptacode.PathFinder.Algorithm.PathFinder(map).FindPath();

            timer.Stop();
            var time = timer.ElapsedMilliseconds;

            _testOutputHelper.WriteLine(time.ToString());
        }
    }
}