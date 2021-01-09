using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Aptacode.CSharp.Common.Utilities.Extensions;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components.Primitives;
using Aptacode.Geometry.Blazor.Utilities;
using Aptacode.Geometry.Primitives;
using Aptacode.Geometry.Vertices;
using Aptacode.PathFinder.Geometry.Neighbours;
using Aptacode.PathFinder.Maps;
using Microsoft.AspNetCore.Components;
using Point = Aptacode.Geometry.Primitives.Point;
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

            var timer = new Stopwatch();
            timer.Start();
            var path = new Aptacode.PathFinder.Algorithm.PathFinder(map,
                DefaultNeighbourFinder.Straight(0.5f)).FindPath().ToList();
            timer.Stop();

            Console.WriteLine(timer.ElapsedMilliseconds);
            components.AddRange(map.Obstacles);

            var polyLinePath = new PolyLine(VertexArray.Create(path.ToArray()));

            components.Add(polyLinePath.ToViewModel());
            SceneController = new PathFinderSceneController(map.Dimensions);
            SceneController.Scene.Components.AddRange(components);


            var startPoint = Point.Create(15, 15).ToViewModel();
            startPoint.FillColor = Color.Green;
            startPoint.CollisionDetectionEnabled = false;
            SceneController.Scene.Components.Add(startPoint);

            var endPoint = Point.Create(70, 70).ToViewModel();
            endPoint.FillColor = Color.Red;
            endPoint.CollisionDetectionEnabled = false;
            SceneController.Scene.Components.Add(endPoint);

            await base.OnInitializedAsync();
        }


        public Map GenerateVerticalBars()
        {
            var mapBuilder = new MapBuilder();
            const int width = 100;
            const int height = 100;
            mapBuilder.SetDimensions(width, height);
            mapBuilder.SetStart(15,15);
            mapBuilder.SetEnd(70,70);
            

            var obstacle1 = Rectangle.Create(new Vector2(20,20 ),
                new Vector2(20,20)).ToViewModel();
            obstacle1.FillColor = Color.Gray;


            var obstacle2 = Rectangle.Create(new Vector2(60, 20),
                new Vector2(20, 20)).ToViewModel();
            obstacle2.FillColor = Color.Gray;


            var obstacle3 = Rectangle.Create(new Vector2(20, 60),
                new Vector2(20, 20)).ToViewModel();
            obstacle3.FillColor = Color.Gray;
            
            mapBuilder.AddObstacle(obstacle1);
            mapBuilder.AddObstacle(obstacle2);
            mapBuilder.AddObstacle(obstacle3);

            var mapResult = mapBuilder.Build();
            return mapResult.Map;
        }

        #region Properties

        public PathFinderSceneController SceneController { get; set; }
        private readonly Random _rand = new();

        #endregion
    }
}