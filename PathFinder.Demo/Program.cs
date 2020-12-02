using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PathFinder.ConsoleDemo
{
    public static class ImageGenerator
    {
        public static Image Generate(this PathFinderResult result)
        {
            Image image =
                new Image<Rgba32>((int) result.Map.Dimensions.X * 10, (int) result.Map.Dimensions.Y * 10);
            var options = new ShapeGraphicsOptions();


            IPen gridPen = Pens.Solid(Color.DarkGray, 0.5f);

            var gridLines = new List<(PointF, PointF)>();
            for (var y = 1; y < result.Map.Dimensions.Y - 1; y++)
            {
                gridLines.Add((new PointF(1, y * 10), new PointF(result.Map.Dimensions.X * 10 - 1, y * 10)));
            }

            for (var x = 1; x < result.Map.Dimensions.X - 1; x++)
            {
                gridLines.Add((new PointF(x * 10, 1), new PointF(x * 10, result.Map.Dimensions.Y * 10 - 1)));
            }

            foreach (var (start, end) in gridLines)
            {
                image.Mutate(x => x.DrawLines(gridPen, start, end));
            }

            IBrush brush = Brushes.Solid(Color.Gray);
            IPen pen = Pens.Solid(Color.Black, 2);
            foreach (var mapObstacle in result.Map.Obstacles)
            {
                IPath yourPolygon = new RectangularPolygon(mapObstacle.Position.X * 10, mapObstacle.Position.Y * 10,
                    mapObstacle.Dimensions.X * 10, mapObstacle.Dimensions.Y * 10);
                image.Mutate(x => x.Fill(options, brush, yourPolygon)
                    .Draw(options, pen, yourPolygon));
            }

            var path = result.Path.Select(p => new PointF(p.X * 10, p.Y * 10)).ToArray();
            image.Mutate(x => x.DrawLines(pen, path));
            var font = SystemFonts.CreateFont("Arial", 20, FontStyle.Bold);
            image.Mutate(x => x.DrawText(result.ToString(), font, Color.Black, new PointF(10, 10)));

            return image;
        }
    }

    public class PathImageOutput
    {
        public void Output(IEnumerable<PathFinderResult> results)
        {
            foreach (var result in results)
            {
                Output(result);
            }
        }

        public void Output(PathFinderResult result)
        {
            Directory.CreateDirectory("output");

            var image = result.Generate();
            image.Save($"output/{result.Name}.png");
            image.Dispose();
        }
    }

    internal static class Program
    {
        private static void Main()
        {
            var running = true;
            while (running)
            {
                var pathFinderDemo = new PathFinderDemo();
                var results = pathFinderDemo.RunAll(2);
                Console.WriteLine("Enter (Y) to run again, (O) to output or anything else to exit.");
                var answer = Console.ReadLine();

                if (string.Equals(answer?.ToUpper(), "O", StringComparison.Ordinal))
                {
                    var imageOutput = new PathImageOutput();
                    imageOutput.Output(results.First());
                    running = false;
                }

                if (!string.Equals(answer?.ToUpper(), "Y", StringComparison.Ordinal))
                {
                    running = false;
                }
            }
        }
    }
}