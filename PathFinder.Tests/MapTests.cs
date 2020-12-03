using Aptacode.PathFinder.Geometry;
using System;
using System.Numerics;
using Xunit;

namespace PathFinder.Tests
{
    public class MapTests
    {
        [Theory]
        [ClassData(typeof(MapTests_ExceptionTests_Data))]
        public void Map_Constructor_ExceptionTests(string exceptionMessage, Vector2 size, Vector2 start, Vector2 end, params Obstacle[] obstacles)
        {
            var exception = Assert.Throws<ArgumentException>(() => { var sut = new Map(size, start, end, obstacles); });
            Assert.Equal(exceptionMessage, exception.Message);
        }
    }
}