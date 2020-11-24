using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace PathFinder.Demo
{
    internal static class Program
    {
        private static void Main()
        {
            var timer = new Stopwatch();
            timer.Start();

            var map = new Map(100, 100, new Vector2(5, 5), new Vector2(90, 90),
                new Obstacle(Guid.NewGuid(), new Vector2(5, 20), new Vector2(40, 50)),
                new Obstacle(Guid.NewGuid(), new Vector2(50, 10), new Vector2(20, 60)),
                new Obstacle(Guid.NewGuid(), new Vector2(75, 75), new Vector2(10, 10))
            );

            var pathFinder = new PathFinder();
            var path = pathFinder.FindPath(map);
            timer.Stop();

            Console.WriteLine($"Total Elapsed: {timer.ElapsedMilliseconds}ms");
            Draw(map, path);

            Console.ReadLine();
        }

        public static void Draw(Map map, List<Vector2> path)
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