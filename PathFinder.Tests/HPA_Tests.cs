using System.Numerics;
using Aptacode.AppFramework.Components.Primitives;
using Aptacode.AppFramework.Scene;
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
            var map = new HierachicalMap(new Scene(new Vector2(100, 100)), 1);
            mapIsConstructed = true;
            Assert.True(mapIsConstructed);
        }

        [Fact]
        public void HierachicalMap_PathFinding_Test()
        {
            var map = new HierachicalMap(new Scene(new Vector2(100, 100)), 1);
            var path = map.FindPath(new Vector2(33, 33), new Vector2(77, 77), 1);

            Assert.True(path.Length > 0);
        }

        [Fact]
        public void HierachicalMap_PathFinding_WithObstacles_Test()
        {
            var scene = new Scene(new Vector2(100, 100));
            scene.Add(Rectangle.Create(new Vector2(50, 50), new Vector2(10, 10)).ToViewModel());

            var map = new HierachicalMap(scene, 1);
            var path = map.FindPath(new Vector2(33, 33), new Vector2(77, 77), 1);
            Assert.True(path.Length > 0);
        }

        [Fact]
        public void ConcretePathfinding_Test()
        {
            var cluster = new Cluster(new Vector2(10, 10));
            var path = cluster.FindConcretePath(new Vector2(1, 1), new Vector2(8, 8));

            Assert.True(path.Length > 0);
        }
    }
}