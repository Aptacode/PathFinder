using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading;
using Aptacode.Geometry.Blazor.Components.ViewModels;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components.Primitives;
using Aptacode.Geometry.Blazor.Extensions;
using Aptacode.Geometry.Primitives;
using Aptacode.PathFinder.Maps.Hpa;
using Rectangle = Aptacode.Geometry.Primitives.Polygons.Rectangle;

namespace PathFinder.BlazorDemo.Pages
{
    public class PathFinderSceneController : SceneController
    {
        public PathFinderSceneController(Vector2 size) : base(
            new Scene(
                size
            ))
        {
            UserInteractionController.OnMouseDown += UserInteractionControllerOnOnMouseDown;
            UserInteractionController.OnMouseUp += UserInteractionControllerOnOnMouseUp;
            UserInteractionController.OnMouseMoved += UserInteractionControllerOnOnMouseMoved;

            var obstacle1 = Rectangle.Create(new Vector2(20, 20),
                new Vector2(20, 20)).ToViewModel();
            obstacle1.FillColor = Color.Gray;
            Scene.Add(obstacle1);

            var obstacle2 = Rectangle.Create(new Vector2(60, 20),
                new Vector2(20, 20)).ToViewModel();
            obstacle2.FillColor = Color.Gray;
            Scene.Add(obstacle2);

            var obstacle3 = Rectangle.Create(new Vector2(20, 60),
                new Vector2(20, 20)).ToViewModel();
            obstacle3.FillColor = Color.Gray;
            Scene.Add(obstacle3);

            _startPoint = new ConnectionPointViewModel(Ellipse.Create(15, 15, 2, 2, 0))
            {
                FillColor = Color.Green,
                CollisionDetectionEnabled = false
            };
            // Scene.Add(_startPoint);

            _endPoint = new ConnectionPointViewModel(Ellipse.Create(70, 70, 2, 2, 0))
            {
                FillColor = Color.Red,
                CollisionDetectionEnabled = false
            };
            // Scene.Add(_endPoint);

            Map = new HierachicalMap(Scene, 1);

            _connection = new ConnectionViewModel(Map, _startPoint, _endPoint);
            _startPoint.Connection = _connection;
            _endPoint.Connection = _connection;
            Scene.Add(_connection);
        }

        public ComponentViewModel SelectedComponent { get; set; }

        private void UserInteractionControllerOnOnMouseMoved(object? sender, Vector2 e)
        {
            if (SelectedComponent == null)
            {
                return;
            }

            var delta = e - UserInteractionController.LastMousePosition;

            Translate(SelectedComponent, delta, new List<ComponentViewModel> {SelectedComponent},
                new CancellationTokenSource());

            _connection.RecalculatePath();
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

        #region Props

        public readonly HierachicalMap Map;
        private readonly ConnectionPointViewModel _startPoint;
        private readonly ConnectionPointViewModel _endPoint;
        private readonly ConnectionViewModel _connection;

        #endregion
    }
}