﻿using System.Numerics;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components.Primitives;
using Aptacode.Geometry.Primitives;

namespace PathFinder.BlazorDemo.Pages
{
    public class ConnectionPointViewModel : EllipseViewModel
    {
        #region Ctor

        public ConnectionPointViewModel(Ellipse ellipse) : base(ellipse)
        {
            Margin = 1;
        }

        #endregion

        #region Prop

        public ConnectionViewModel Connection { get; set; }

        #endregion

        public override void Translate(Vector2 delta)
        {
            base.Translate(delta);
            RecalculatePaths();
        }

        public void RecalculatePaths()
        {
            Connection?.RecalculatePath();
        }
    }
}