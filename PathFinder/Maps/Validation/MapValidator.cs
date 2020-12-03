using Aptacode.PathFinder.Resources;

namespace Aptacode.PathFinder.Maps.Validation
{
    public static class MapValidator
    {
        public static MapValidationResult IsValid(this Map map)
        {
            if (map.IsOutOfBounds(map.Start.Position))
            {
                return MapValidationResult.Fail(ExceptionMessages.StartPointOutOfBounds);
            }

            if (map.IsOutOfBounds(map.End.Position))
            {
                return MapValidationResult.Fail(ExceptionMessages.EndPointOutOfBounds);
            }

            if (map.HasCollision(map.Start.Position))
            {
                return MapValidationResult.Fail(ExceptionMessages.StartPointHasCollisionWithObstacle);
            }

            if (map.HasCollision(map.End.Position))
            {
                return MapValidationResult.Fail(ExceptionMessages.EndPointHasCollisionWithObstacle);
            }

            foreach (var obstacle in map.Obstacles)
            {
                if (map.IsOutOfBounds(obstacle.Position))
                {
                    return MapValidationResult.Fail(ExceptionMessages.ObstacleOutOfBounds);
                }
            }

            return MapValidationResult.Ok(GeneralMessages.Success);
        }
    }
}