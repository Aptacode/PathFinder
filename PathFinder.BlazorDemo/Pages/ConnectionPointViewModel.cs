using System.Numerics;
using Aptacode.AppFramework.Components.Primitives;
using Aptacode.Geometry.Primitives;

namespace PathFinder.BlazorDemo.Pages;

public class ConnectionPointViewModel : EllipseViewModel
{
    #region Ctor

    public ConnectionPointViewModel(Ellipse ellipse) : base(ellipse)
    {
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