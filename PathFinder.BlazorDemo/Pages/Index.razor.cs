using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Aptacode.Geometry.Blazor.Components.ViewModels;
using Aptacode.Geometry.Blazor.Utilities;
using Aptacode.Geometry.Primitives;
using Aptacode.Geometry.Vertices;
using Aptacode.PathFinder.Geometry.Neighbours;
using Aptacode.PathFinder.Maps;
using Microsoft.AspNetCore.Components;
using Rectangle = Aptacode.Geometry.Primitives.Polygons.Rectangle;

namespace PathFinder.BlazorDemo.Pages
{
    public class IndexBase : ComponentBase
    {

        protected override async Task OnInitializedAsync()
        {
            var componentBuilder = new ComponentBuilder();
            var components = new List<ComponentViewModel>();
            var map = GenerateVerticalBars();
            var scale = new Vector2(10, 10);

            var timer = new Stopwatch();
            timer.Start();
            var path = new Aptacode.PathFinder.Algorithm.PathFinder(map,
                DefaultNeighbourFinder.Straight(0.5f)).FindPath().ToList();
            timer.Stop();

            Console.WriteLine(timer.ElapsedMilliseconds);
            foreach (var obstacle in map.Obstacles)
            {
                obstacle.Scale(scale);
                components.Add(componentBuilder.SetPrimitive(obstacle).SetFillColor(Color.Red).Build());
            }

            var polyLinePath = new PolyLine(VertexArray.Create(path.ToArray()));
            polyLinePath.Scale(scale);

            components.Add(componentBuilder.SetPrimitive(polyLinePath).Build());
            SceneController = new PathFinderSceneController(map.Dimensions * scale, components);

            await base.OnInitializedAsync();
        }


        public Map GenerateVerticalBars()
        {
            var mapBuilder = new MapBuilder();
            const int width = 100;
            const int height = 100;
            const int thickness = 5;
            mapBuilder.SetDimensions(width, height);
            mapBuilder.SetStart(0, 0);
            mapBuilder.SetEnd(width, height);

            var count = 0;
            for (var i = thickness; i < width - thickness; i += 2 * thickness)
            {
                var offset = count++ % 2 == 0 ? thickness : -thickness;
                mapBuilder.AddObstacle(Rectangle.Create(new Vector2(i, offset),
                    new Vector2(thickness, height - thickness)));
            }

            var mapResult = mapBuilder.Build();
            return mapResult.Map;
        }

        #region Properties

        public PathFinderSceneController SceneController { get; set; }
        private readonly Random _rand = new();

        #endregion
    }
}