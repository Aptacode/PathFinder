using Aptacode.FlowDesigner.Core.ViewModels;
using Aptacode.PathFinder.Geometry;
using Aptacode.PathFinder.Geometry.Neighbours;
using Aptacode.PathFinder.Maps;
using Microsoft.AspNetCore.Components;
using System;
using System.Diagnostics;
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
            var map = new Map(new Vector2(100, 100), new Vector2(5, 5), new Vector2(90, 90),
                new Obstacle(Guid.NewGuid(), new Vector2(5, 20), new Vector2(40, 50)),
                new Obstacle(Guid.NewGuid(), new Vector2(50, 10), new Vector2(20, 60)),
                new Obstacle(Guid.NewGuid(), new Vector2(75, 75), new Vector2(10, 10)));

            var timer = new Stopwatch();
            timer.Start();
            var path = new Aptacode.PathFinder.Algorithm.PathFinder(map,
                DefaultNeighbourFinder.Straight(0.5f)).FindPath().ToList();
            timer.Stop();

            var totalLength = path.Zip(path.Skip(1), (a, b) => a - b).Select(s => s.Length()).Sum();

            Designer = new DesignerViewModel((int)map.Dimensions.X, (int)map.Dimensions.Y);
            foreach (var obstacle in map.Obstacles)
            {
                var item1 = Designer.AddItem("State 1", obstacle.Position, obstacle.Dimensions);
                item1.CollisionsAllowed = false;
            }

            foreach (var point in path)
            {
                Designer.AddPoint(point);
            }

            await base.OnInitializedAsync();
        }
    }
}
