using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading;
using Aptacode.AppFramework.Components;
using Aptacode.AppFramework.Components.Primitives;
using Aptacode.AppFramework.Extensions;
using Aptacode.AppFramework.Scene;
using Aptacode.AppFramework.Scene.Events;
using Aptacode.Geometry.Primitives;
using Aptacode.PathFinder.Maps.Hpa;
using Rectangle = Aptacode.Geometry.Primitives.Polygons.Rectangle;

namespace PathFinder.BlazorDemo.Pages
{
    public class PathFinderSceneController : SceneController
    {
        public PathFinderSceneController(Vector2 size)
        {
            UserInteractionController.OnMouseEvent += UserInteractionControllerOnOnMouseEvent;

            PathFinderScene = new Scene(size);

            var obstacle1 = Rectangle.Create(new Vector2(20, 20),
                new Vector2(10, 10)).ToViewModel();
            obstacle1.FillColor = Color.Gray;
            PathFinderScene.Add(obstacle1);

            var obstacle2 = Rectangle.Create(new Vector2(20, 60),
                new Vector2(10, 10)).ToViewModel();
            obstacle2.Margin = 0.0f;
            obstacle2.FillColor = Color.Gray;
            PathFinderScene.Add(obstacle2);

            var obstacle3 = Rectangle.Create(new Vector2(60, 20),
                new Vector2(10, 10)).ToViewModel();
            obstacle3.FillColor = Color.Gray;
            PathFinderScene.Add(obstacle3);

            _startPoint = new ConnectionPointViewModel(Ellipse.Create(10f, 10f, 0.2f, 0.2f, 0))
            {
                FillColor = Color.Green,
                CollisionDetectionEnabled = false
            };
            // Scene.Add(_startPoint);

            _endPoint = new ConnectionPointViewModel(Ellipse.Create(80, 80, 0.2f, 0.2f, 0))
            {
                FillColor = Color.Red,
                CollisionDetectionEnabled = false
            };
            // Scene.Add(_endPoint);

            Map = new HierachicalMap(PathFinderScene);

            _connection = new ConnectionViewModel(Map, _startPoint, _endPoint);
            _startPoint.Connection = _connection;
            _endPoint.Connection = _connection;

            PathFinderScene.Add(_connection);

            Scenes.Add(PathFinderScene);
        }

        public ComponentViewModel SelectedComponent { get; set; }
        public Scene PathFinderScene { get; set; }

        private void UserInteractionControllerOnOnMouseEvent(object? sender, MouseEvent e)
        {
            switch (e)
            {
                case MouseMoveEvent mouseMove:
                    UserInteractionControllerOnOnMouseMoved(this, mouseMove.Position);
                    break;
                case MouseUpEvent mouseUp:
                    UserInteractionControllerOnOnMouseUp(this, mouseUp.Position);
                    break;
                case MouseDownEvent mouseDown:
                    UserInteractionControllerOnOnMouseDown(this, mouseDown.Position);
                    break;
            }
        }

        private void UserInteractionControllerOnOnMouseMoved(object? sender, Vector2 e)
        {
            if (SelectedComponent == null)
            {
                return;
            }

            var delta = e - UserInteractionController.LastMousePosition;

            var movedItems = new List<ComponentViewModel> {SelectedComponent};
            PathFinderScene.Translate(SelectedComponent, delta, movedItems,
                new CancellationTokenSource());

            Map.Update(SelectedComponent);

            _connection.RecalculatePath();
        }

        private void UserInteractionControllerOnOnMouseUp(object? sender, Vector2 e)
        {
            foreach (var componentViewModel in PathFinderScene.Components)
            {
                componentViewModel.BorderColor = Color.Black;
            }

            SelectedComponent = null;
        }

        private void UserInteractionControllerOnOnMouseDown(object? sender, Vector2 e)
        {
            SelectedComponent = null;

            foreach (var componentViewModel in PathFinderScene.Components.CollidingWith(e))
            {
                SelectedComponent = componentViewModel;
                componentViewModel.BorderColor = Color.Green;
            }

            PathFinderScene.BringToFront(SelectedComponent);
        }

        #region Props

        public readonly HierachicalMap Map;
        private readonly ConnectionPointViewModel _startPoint;
        private readonly ConnectionPointViewModel _endPoint;
        private readonly ConnectionViewModel _connection;

        #endregion
    }
}