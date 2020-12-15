using Aptacode.FlowDesigner.Core.ViewModels;
using Aptacode.PathFinder.Geometry;
using Aptacode.PathFinder.Geometry.Neighbours;
using Aptacode.PathFinder.Maps;
using Microsoft.AspNetCore.Components;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace PathFinder.BlazorDemo.Pages
{
    public class IndexBase : ComponentBase
    {

        public DesignerViewModel Designer { get; set; }

        protected async override Task OnInitializedAsync()
        {
            var rng = new Random();

            var mapBuilder = new MapBuilder();
            var dimensions = new Vector2(100, 100);
            var start = new Vector2(99, 1);
            var end = new Vector2(1, 99);
            
            mapBuilder.SetDimensions(dimensions).SetStart(start).SetEnd(end);
            
            foreach(int i in Enumerable.Range(0, (int) dimensions.X - 1))
            {
                foreach(int j in Enumerable.Range(0, (int) dimensions.Y - 1))
                {
                    var randomNum = rng.Next(0, 100);
                    if (randomNum < 30 && ((i != start.X && j != start.Y) || (i != end.X && j != end.Y)))
                    {
                        mapBuilder.AddObstacle(new Vector2(i, j), new Vector2(0, 0));
                    }
                }
            }
            var map = mapBuilder.Build().Map;

            var timer = new Stopwatch();
            timer.Start();
            var path = new Aptacode.PathFinder.Algorithm.PathFinder(map,
                DefaultNeighbourFinder.Straight(1f)).FindPath().ToList();
            timer.Stop();

            var totalLength = path.Zip(path.Skip(1), (a, b) => a - b).Select(s => s.Length()).Sum();

            Designer = new DesignerViewModel((int)map.Dimensions.X, (int)map.Dimensions.Y);

            //foreach (var obstacle in map.Obstacles)
            //{
            //    var newPoint = Designer.AddPoint(obstacle.Position);
            //    newPoint.FillColor = Color.Red;
            //}

            //Designer.AddPath(new Aptacode.FlowDesigner.Core.ViewModels.Components.PathViewModel(Guid.NewGuid(), path));

            await base.OnInitializedAsync();
        }
    }
}
