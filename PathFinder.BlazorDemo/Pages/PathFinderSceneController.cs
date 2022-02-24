using System.Drawing;
using System.Numerics;
using Aptacode.AppFramework.Components;
using Aptacode.AppFramework.Components.Controls;
using Aptacode.AppFramework.Components.Primitives;
using Aptacode.AppFramework.Scene;
using Aptacode.AppFramework.Scene.Events;
using Aptacode.Geometry.Primitives;
using Aptacode.PathFinder.Maps.Hpa;

namespace PathFinder.BlazorDemo.Pages;

public class PathFinderSceneController : SceneController
{
    public PathFinderSceneController(Vector2 size) : base(size)
    {
        ShowGrid = true;

        Scene = new Scene(size);
        PathFinderScene = new Scene(size);

        DragBox = new DragBox(Polygon.Rectangle.FromTwoPoints(Vector2.Zero, size));
        DragBox.FillColor = Color.Transparent;

        Scene.Add(DragBox);

        var obstacle1 = Polygon.Rectangle.FromPositionAndSize(new Vector2(20, 20), new Vector2(10, 10)).ToViewModel();
        obstacle1.FillColor = Color.Gray;
        DragBox.Add(obstacle1);
        PathFinderScene.Add(obstacle1);
        obstacle1.OnTranslated += obstacleTranslated;

        var obstacle2 = Polygon.Rectangle.FromPositionAndSize(new Vector2(20, 60), new Vector2(10, 10)).ToViewModel();
        obstacle2.Margin = 0.0f;
        obstacle2.FillColor = Color.Gray;
        DragBox.Add(obstacle2);
        PathFinderScene.Add(obstacle2);
        obstacle2.OnTranslated += obstacleTranslated;

        var obstacle3 = Polygon.Rectangle.FromPositionAndSize(new Vector2(60, 20),
            new Vector2(10, 10)).ToViewModel();
        obstacle3.FillColor = Color.Gray;
        DragBox.Add(obstacle3);
        PathFinderScene.Add(obstacle3);
        obstacle3.OnTranslated += obstacleTranslated;

        _startPoint = new ConnectionPointViewModel(Ellipse.Create(10f, 10f, 1, 1, 0))
        {
            FillColor = Color.Green
        };
        DragBox.Add(_startPoint);

        _endPoint = new ConnectionPointViewModel(Ellipse.Create(80, 80, 1f, 1f, 0))
        {
            FillColor = Color.Red
        };
        DragBox.Add(_endPoint);

        Map = new HierachicalMap(PathFinderScene);

        _connection = new ConnectionViewModel(Map, _startPoint, _endPoint);
        _startPoint.Connection = _connection;
        _endPoint.Connection = _connection;

        PathFinderScene.Add(_connection);
        Scene.Add(_connection);

        Scenes.Add(Scene);
    }

    public ComponentViewModel SelectedComponent { get; set; }
    public Scene Scene { get; set; }
    public Scene PathFinderScene { get; set; }
    public DragBox DragBox { get; set; }

    private void obstacleTranslated(object? sender, TranslateEvent e)
    {
        _connection.RecalculatePath();
    }

    #region Props

    public readonly HierachicalMap Map;
    private readonly ConnectionPointViewModel _startPoint;
    private readonly ConnectionPointViewModel _endPoint;
    private readonly ConnectionViewModel _connection;

    #endregion
}