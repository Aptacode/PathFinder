using Aptacode.Geometry.Blazor.Components.ViewModels.Components;
using Aptacode.PathFinder.Maps;
using Aptacode.PathFinder.Maps.Hpa;
using Aptacode.PathFinder.Maps.Validation;
using System.Numerics;
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
    }
}