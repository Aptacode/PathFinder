using Aptacode.Geometry.Primitives;
using Aptacode.PathFinder.Resources;

namespace Aptacode.PathFinder.Maps.Validation
{
    public static class MapValidator
    {
        public static MapValidationResult IsValid(this Map map)
        {
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