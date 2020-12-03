using System;
using System.Collections.Generic;
using System.Text;

namespace Aptacode.PathFinder.Map
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
            new MapValidationResult(message, false);

        public static MapValidationResult Ok(string message) =>
            new MapValidationResult(message, true);
    }
}

