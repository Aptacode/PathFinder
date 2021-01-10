using System.Collections.Generic;
using System.Numerics;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components;
using Aptacode.Geometry.Primitives;
using Aptacode.PathFinder.Maps.Hpa;
using Xunit;

namespace PathFinder.Tests
{
    public class HPA_Tests
    {
        [Fact]
        public void HierachicalMap_Initialization_Test()
        {
            var mapIsConstructed = false;
            var map = new HierachicalMap(new Vector2(100, 100), new ComponentViewModel[0], 1);
            mapIsConstructed = true;
            Assert.True(mapIsConstructed);
        }

        [Fact]
        public void HierachicalMap_PathFinding_Test()
        {
            var map = new HierachicalMap(new Vector2(100, 100), new ComponentViewModel[0], 1);
            var path = map.FindPath(new Point(new Vector2(33, 33)), new Point(new Vector2(77, 77)), 1);

            Assert.True(path.Count > 0);
        }
    }
}