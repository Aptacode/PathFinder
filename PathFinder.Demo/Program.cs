using System;

namespace PathFinder.Demo
{
    internal static class Program
    {
        private static void Main()
        {
            var running = true;
            while (running)
            {
                var pathFinderDemo = new PathFinderDemo();
                pathFinderDemo.RunAll(10);
                Console.WriteLine("Enter (Y) to run again or anything else to exit.");
                var answer = Console.ReadLine();
                if (!string.Equals(answer?.ToUpper(), "Y", StringComparison.Ordinal))
                {
                    running = false;
                }
            }
        }
    }
}