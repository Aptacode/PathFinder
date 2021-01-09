using Aptacode.Geometry.Primitives;
using Aptacode.PathFinder.Resources;

namespace Aptacode.PathFinder.Maps.Validation
{
    public static class MapValidator
    {
        public static MapValidationResult IsValid(this Map map)
        {
            if (map.Dimensions.X <= 0 || map.Dimensions.Y <= 0)
            {
                return MapValidationResult.Fail(ExceptionMessages.InvalidMapDimensions);
            }

            if (map.IsOutOfBounds(new Point(map.Start.Position)))
            {
                return MapValidationResult.Fail(ExceptionMessages.StartPointOutOfBounds);
            }

            if (map.IsOutOfBounds(new Point(map.End.Position)))
            {
                return MapValidationResult.Fail(ExceptionMessages.EndPointOutOfBounds);
            }

            if (map.HasCollision(new Point(map.Start.Position)))
            {
                return MapValidationResult.Fail(ExceptionMessages.StartPointHasCollisionWithObstacle);
            }

            if (map.HasCollision(new Point(map.End.Position)))
            {
                return MapValidationResult.Fail(ExceptionMessages.EndPointHasCollisionWithObstacle);
            }

            return MapValidationResult.Ok(GeneralMessages.Success);
        }
    }
}