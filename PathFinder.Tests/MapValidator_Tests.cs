using Aptacode.PathFinder.Maps;
using Aptacode.PathFinder.Maps.Validation;
using Xunit;

namespace PathFinder.Tests
{
    public class MapValidator_Tests
    {
        [Theory]
        [ClassData(typeof(MapValidator_TestData))]
        public void MapValidator_IsValidTests(string message, Map map)
        {
            //Arrange
            //Act
            var mapValidationResult = map.IsValid();
            //Assert
            Assert.Equal(message, mapValidationResult.Message);
        }
    }
}