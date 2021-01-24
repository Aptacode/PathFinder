using System.Linq;
using Aptacode.AppFramework.Components.Primitives;
using Aptacode.Geometry.Primitives;
using Aptacode.PathFinder.Maps.Hpa;

namespace PathFinder.BlazorDemo.Pages
{
    public class ConnectionViewModel : PolylineViewModel
    {
        #region Ctor

        public ConnectionViewModel(HierachicalMap map, ConnectionPointViewModel startPoint, ConnectionPointViewModel endPoint) : base(PolyLine.Create(new[]
        {
            startPoint.Primitive.BoundingRectangle.Center,
            endPoint.Primitive.BoundingRectangle.Center
        }))
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
            var points = Map.FindPath(StartPoint.Primitive.Position, EndPoint.Primitive.Position);

            var path = points.ToList();

            path.Insert(0, StartPoint.Primitive.Position);
            path.Add(EndPoint.Primitive.Position);

            Primitive = PolyLine.Create(path.ToHashSet().ToArray());
            Invalidated = true;
        }

        #region Prop

        public HierachicalMap Map { get; set; }
        public ConnectionPointViewModel StartPoint { get; set; }
        public ConnectionPointViewModel EndPoint { get; set; }
        #endregion
    }
}