using Aptacode.PathFinder.Geometry;
using Aptacode.PathFinder.Resources;
using PathFinder.Tests.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace PathFinder.Tests
{
    public class MapTests_ExceptionTests_Data : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new List<object[]>
        {
            new object[]
            {
                ExceptionMessages.StartPointOutOfBounds,
                new Vector2(10, 10), new Vector2(-1, 0), new Vector2(5, 5)
            },
            new object[]
            {
                 ExceptionMessages.EndPointOutOfBounds,
                new Vector2(10, 10), new Vector2(5, 0), new Vector2(-5, 5)
            },
            new object[]
            {
                ExceptionMessages.StartPointHasCollisionWithObstacle,
                new Vector2(10, 10), new Vector2(1, 1), new Vector2(5, 5),
                new Obstacle(Guid.NewGuid(), new Vector2(1, 1), new Vector2(0, 0))
                
            },
            new object[]
            {
                ExceptionMessages.EndPointHasCollisionWithObstacle,
                new Vector2(10, 10), new Vector2(1, 1), new Vector2(5, 5),
                new Obstacle(Guid.NewGuid(), new Vector2(5, 5), new Vector2(0, 0))               
            }
        };

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}