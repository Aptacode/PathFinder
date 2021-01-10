using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Aptacode.Geometry.Blazor.Components.ViewModels;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components.Primitives;
using Aptacode.Geometry.Blazor.Extensions;
using Aptacode.Geometry.Primitives;
using Rectangle = Aptacode.Geometry.Primitives.Polygons.Rectangle;

namespace PathFinder.BlazorDemo.Pages
{
    public class PathFinderSceneController : SceneControllerViewModel
    {
        private DateTime lastTick = DateTime.Now;
        private ConnectionPointViewModel StartPoint;
        private ConnectionPointViewModel EndPoint;
        private ConnectionViewModel Connection;
        public PathFinderSceneController(Vector2 size) : base(
            new SceneViewModel(
                size
            ))
        {
            UserInteractionController.OnMouseDown += UserInteractionControllerOnOnMouseDown;
            UserInteractionController.OnMouseUp += UserInteractionControllerOnOnMouseUp;
            UserInteractionController.OnMouseMoved += UserInteractionControllerOnOnMouseMoved;

            var obstacle1 = Rectangle.Create(new Vector2(20, 20),
                new Vector2(20, 20)).ToViewModel();
            obstacle1.FillColor = Color.Gray;
            Scene.Components.Add(obstacle1);

            var obstacle2 = Rectangle.Create(new Vector2(60, 20),
                new Vector2(20, 20)).ToViewModel();
            obstacle2.FillColor = Color.Gray;
            Scene.Components.Add(obstacle2);

            var obstacle3 = Rectangle.Create(new Vector2(20, 60),
                new Vector2(20, 20)).ToViewModel();
            obstacle3.FillColor = Color.Gray;
            Scene.Components.Add(obstacle3);

            StartPoint = new ConnectionPointViewModel(Ellipse.Create(15, 15, 2, 2, 0));
            StartPoint.FillColor = Color.Green;
            Scene.Components.Add(StartPoint);

            EndPoint = new ConnectionPointViewModel(Ellipse.Create(70, 70, 2, 2, 0));
            EndPoint.FillColor = Color.Red;
            Scene.Components.Add(EndPoint);

            Connection = ConnectionViewModel.Connect(Scene, StartPoint, EndPoint);
            Scene.Components.Add(Connection);
        }

        public bool Running { get; set; }

        public ComponentViewModel SelectedComponent { get; set; }

        public override async Task Tick()
        {
            var currentTime = DateTime.Now;
            var delta = currentTime - lastTick;
            lastTick = currentTime;
            await base.Tick();
        }

        private void UserInteractionControllerOnOnMouseMoved(object? sender, Vector2 e)
        {
            if (SelectedComponent == null)
            {
                return;
            }

            var delta = e - UserInteractionController.LastMousePosition;

            Translate(SelectedComponent, delta, new List<ComponentViewModel> {SelectedComponent},
                new CancellationTokenSource());
            
            Connection.RecalculatePath();
        }

        private void UserInteractionControllerOnOnMouseUp(object? sender, Vector2 e)
        {
            foreach (var componentViewModel in Scene.Components)
            {
                componentViewModel.BorderColor = Color.Black;
            }

            SelectedComponent = null;
        }

        private void UserInteractionControllerOnOnMouseDown(object? sender, Vector2 e)
        {
            SelectedComponent = null;

            foreach (var componentViewModel in Scene.Components.CollidingWith(e, CollisionDetector))
            {
                SelectedComponent = componentViewModel;
                componentViewModel.BorderColor = Color.Green;
            }

            Scene.BringToFront(SelectedComponent);
        }
    }
}