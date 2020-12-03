namespace Aptacode.PathFinder.Maps
{
    public class MapResult
    {
        public readonly Map? Map;
        public readonly string Message;
        public readonly bool Success;

        private MapResult(string message, bool success, Map? map)
        {
            Message = message;
            Success = success;
            Map = map;
        }

        public static MapResult Fail(string message) => new MapResult(message, false, null);

        public static MapResult Ok(Map map, string message) =>
            new MapResult(message, true, map);
    }
}