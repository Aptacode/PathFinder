﻿using System.Linq;
using Aptacode.AppFramework.Components.Primitives;
using Aptacode.Geometry.Primitives;
using Aptacode.Geometry.Vertices;
using Aptacode.PathFinder.Maps.Hpa;

namespace PathFinder.BlazorDemo.Pages
{
    public class ConnectionViewModel : PolylineViewModel
    {
        #region Ctor

        public ConnectionViewModel(HierachicalMap map, ConnectionPointViewModel startPoint, ConnectionPointViewModel endPoint) : base(new PolyLine(VertexArray.Create(new[]
        {
            startPoint.Ellipse.BoundingCircle.Center,
            endPoint.Ellipse.BoundingCircle.Center
        })))
        {
            Map = map;
            StartPoint = startPoint;
            EndPoint = endPoint;
            CollisionDetectionEnabled = false;
            RecalculatePath();
        }

        #endregion

        public void RecalculatePath()
        {
            var points = Map.FindPath(StartPoint.Ellipse.Position, EndPoint.Ellipse.Position);

            var path = points.ToList();

            path.Insert(0, StartPoint.Ellipse.Position);
            path.Add(EndPoint.Ellipse.Position);

            PolyLine = new PolyLine(VertexArray.Create(path.ToArray()));
            UpdateBoundingRectangle();
            Invalidated = true;
        }

        #region Prop

        public HierachicalMap Map { get; set; }
        public ConnectionPointViewModel StartPoint { get; set; }
        public ConnectionPointViewModel EndPoint { get; set; }

        public PolyLine Connection => PolyLine;

        #endregion
    }
}