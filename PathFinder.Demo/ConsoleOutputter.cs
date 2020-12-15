using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aptacode.Geometry.Primitives;
using Aptacode.PathFinder.Maps;

namespace PathFinder.ConsoleDemo
{
    public static class ConsoleOutputter
    {
        public static void DrawToConsole(this Map map, IEnumerable<Vector2> path)
        {
            for (var i = 0; i < (int) map.Dimensions.Y; i++)
            {
                for (var j = 0; j < (int) map.Dimensions.X; j++)
                {
                    var point = new Point(new Vector2(j, i));
                    Console.ResetColor();
                    if (point == new Point(map.Start.Position))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(" A");
                    }
                    else if (point == new Point(map.End.Position))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(" B");
                    }
                    else if (map.HasCollision(point))
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write(" +");
                    }
                    else if (path.Contains(point.Position))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(" +");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(" +");
                    }
                }

                Console.Write(Environment.NewLine);
            }
        }
    }
}