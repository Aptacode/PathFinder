using Aptacode.Geometry.Blazor.Components.ViewModels;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components.Primitives;
using Aptacode.Geometry.Primitives;
using Aptacode.Geometry.Vertices;

namespace PathFinder.BlazorDemo.Pages
{
    public class ConnectionViewModel : PolylineViewModel
    {
        #region Ctor

        protected ConnectionViewModel(Scene scene, ConnectionPointViewModel startPoint, ConnectionPointViewModel endPoint) : base(new PolyLine(VertexArray.Create(new[]
        {
            startPoint.Ellipse.BoundingCircle.Center,
            endPoint.Ellipse.BoundingCircle.Center
        })))
        {
            Scene = scene;
            StartPoint = startPoint;
            EndPoint = endPoint;
            CollisionDetectionEnabled = false;
            RecalculatePath();
        }

        #endregion

        public static ConnectionViewModel Connect(Scene scene, ConnectionPointViewModel connectionPoint1, ConnectionPointViewModel connectionPoint2)
        {
            var connection = new ConnectionViewModel(scene, connectionPoint1, connectionPoint2);
            connectionPoint1.Connection = connection;
            connectionPoint2.Connection = connection;
            return connection;
        }

        public void RecalculatePath()
        {
            var path = Scene.GetPath(StartPoint.Ellipse.Position, EndPoint.Ellipse.Position);

            path.Insert(0, EndPoint.Ellipse.Position);
            path.Add(StartPoint.Ellipse.Position);

            PolyLine = new PolyLine(VertexArray.Create(path.ToArray()));
            UpdateBoundingRectangle();
            Invalidated = true;
        }

        #region Prop

        public Scene Scene { get; set; }
        public ConnectionPointViewModel StartPoint { get; set; }
        public ConnectionPointViewModel EndPoint { get; set; }

        public PolyLine Connection => PolyLine;

        #endregion
    }
}