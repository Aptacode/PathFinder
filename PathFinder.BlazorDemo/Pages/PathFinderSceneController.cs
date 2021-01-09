using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Aptacode.Geometry.Blazor.Components.ViewModels;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components;
using Aptacode.Geometry.Blazor.Extensions;

namespace PathFinder.BlazorDemo.Pages
{
    public class PathFinderSceneController : SceneControllerViewModel
    {
        private DateTime lastTick = DateTime.Now;

        public PathFinderSceneController(Vector2 size) : base(
            new SceneViewModel(
                size
            ))
        {
            UserInteractionController.OnMouseDown += UserInteractionControllerOnOnMouseDown;
            UserInteractionController.OnMouseUp += UserInteractionControllerOnOnMouseUp;
            UserInteractionController.OnMouseMoved += UserInteractionControllerOnOnMouseMoved;
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