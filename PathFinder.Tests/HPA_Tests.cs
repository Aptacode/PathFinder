using System.Collections.Generic;
using System.Numerics;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components.Primitives;
using Aptacode.Geometry.Primitives;
using Aptacode.Geometry.Primitives.Polygons;
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
            var map = new HierachicalMap(new Vector2(200, 200), new List<ComponentViewModel>(), 2);
            mapIsConstructed = true;
            Assert.True(mapIsConstructed);
        }

        [Fact]
        public void HierachicalMap_PathFinding_Test()
        {
            var map = new HierachicalMap(new Vector2(100, 100), new List<ComponentViewModel>(), 1);
            var path = map.FindPath(new Point(new Vector2(33, 33)), new Point(new Vector2(77, 77)), 1);

            Assert.True(path.Count > 0);
        }
        
        [Fact]
        public void HierachicalMap_PathFinding_WithObstacles_Test()
        {
            var map = new HierachicalMap(new Vector2(100, 100), new List<ComponentViewModel>() { Rectangle.Create(new Vector2(50, 50), new Vector2(10, 10)).ToViewModel() }, 1);
            var path = map.FindPath(new Point(new Vector2(33, 33)), new Point(new Vector2(77, 77)), 1);

            Assert.True(path.Count > 0);
        }
    }
}