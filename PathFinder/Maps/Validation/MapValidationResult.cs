namespace Aptacode.PathFinder.Maps.Validation
{
    public class MapValidationResult
    {
        private MapValidationResult(string message, bool success)
        {
            Message = message;
            Success = success;
        }

        public string Message { get; }
        public bool Success { get; }

        public static MapValidationResult Fail(string message) =>
            new(message, false);

        public static MapValidationResult Ok(string message) =>
            new(message, true);
    }
}