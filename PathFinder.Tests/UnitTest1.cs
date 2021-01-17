using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Aptacode.AppFramework.Components.Primitives;
using Aptacode.Geometry.Primitives.Polygons;
using Aptacode.PathFinder.Maps;
using Aptacode.PathFinder.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace PathFinder.Tests
{
    public class PerformanceTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public PerformanceTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Test1()
        {
            var timer = new Stopwatch();
            timer.Start();

            var map = new Map(new Vector2(100, 100), new Vector2(10, 10), new Vector2(90, 90),
                Rectangle.Create(new Vector2(30, 30), new Vector2(20, 50)).ToViewModel());

            var path = new Aptacode.PathFinder.Algorithm.PathFinder(map).FindPath();

            timer.Stop();
            var time = timer.ElapsedMilliseconds;
            foreach (var point in path)
            {
                _testOutputHelper.WriteLine(point.ToString());
            }

            _testOutputHelper.WriteLine(time.ToString());
        }

        [Fact]
        public void PriorityQueueEnqueueTest()
        {
            var timer = new Stopwatch();
            timer.Start();
            var rand = new Random();
            var queue = new PriorityQueue<int, int>();
            var numbers = new List<int>();
            var itemCount = 10000;

            for (var i = 0; i < itemCount; i++)
            {
                var value = rand.Next(100);
                numbers.Add(value);
                queue.Enqueue(value, value);
            }

            foreach (var value in numbers)
            {
                queue.Dequeue(value);
            }

            timer.Stop();
        }
    }
}