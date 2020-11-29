using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aptacode.PathFinder.Geometry;

namespace PathFinder.Demo
{
    public static class ConsoleOutputter
    {
        public static void DrawToConsole(this Map map, IEnumerable<Vector2> path)
        {
            for (var i = 0; i < (int) map.Dimensions.Y; i++)
            {
                for (var j = 0; j < (int) map.Dimensions.X; j++)
                {
                    var point = new Vector2(j, i);
                    Console.ResetColor();
                    if (point == map.Start.Position)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(" A");
                    }
                    else if (point == map.End.Position)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(" B");
                    }
                    else if (map.HasCollision(point))
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write(" +");
                    }
                    else if (path.Contains(point))
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