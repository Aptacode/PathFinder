using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Point = Aptacode.Geometry.Primitives.Point;
using Polygon = Aptacode.Geometry.Primitives.Polygon;

namespace PathFinder.ConsoleDemo
{
    public static class ImageGenerator
    {
        public static Image Generate(this PathFinderResult result)
        {
            var width = (int) result.Map.Dimensions.X * 10;
            var height = (int) result.Map.Dimensions.Y * 10;
            Image image =
                new Image<Rgba32>(width, height);
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
                if (mapObstacle is Polygon polygon)
                {
                    try
                    {
                        var lineSegments = new List<LinearLineSegment>();
                        foreach (var edge in polygon.Edges)
                        {
                            var p1 = edge.p1 * 10;
                            var p2 = edge.p2 * 10;
                            p1 = new Vector2(Math.Clamp(p1.X, 0, width), Math.Clamp(p1.Y, 0, height));
                            p2 = new Vector2(Math.Clamp(p2.X, 0, width), Math.Clamp(p2.Y, 0, height));
                            lineSegments.Add(new LinearLineSegment(p1, p2));
                        }

                        IPath yourPolygon = new SixLabors.ImageSharp.Drawing.Polygon(lineSegments);
                        image.Mutate(x => x?.Fill(options, brush, yourPolygon)
                            .Draw(options, pen, yourPolygon));
                    }
                    catch { }
                }

                if (mapObstacle is Point point) { }
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