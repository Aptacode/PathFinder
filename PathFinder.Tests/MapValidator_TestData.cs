using System.Collections;
using System.Collections.Generic;
using Aptacode.PathFinder.Resources;
using PathFinder.Tests.Helpers;

namespace PathFinder.Tests
{
    public class MapValidator_TestData : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new()
        {
            new object[]
            {
                ExceptionMessages.StartPointOutOfBounds,
                Map_Helpers.StartPoint_OutOfBounds_Map
            },
            new object[]
            {
                ExceptionMessages.EndPointOutOfBounds,
                Map_Helpers.EndPoint_OutOfBounds_Map
            },
            new object[]
            {
                ExceptionMessages.StartPointHasCollisionWithObstacle,
                Map_Helpers.StartPoint_HasCollision_WithObstacle_Map
            },
            new object[]
            {
                ExceptionMessages.EndPointHasCollisionWithObstacle,
                Map_Helpers.EndPoint_HasCollision_WithObstacle_Map
            },
            new object[]
            {
                GeneralMessages.Success,
                Map_Helpers.Valid_No_Obstacles_Map
            }
        };

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}