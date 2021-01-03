using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components;
using Aptacode.Geometry.Collision;
using Aptacode.Geometry.Primitives;
using Aptacode.Geometry.Primitives.Polygons;
using Aptacode.PathFinder.Geometry.Neighbours;

namespace Aptacode.PathFinder.Hpa
{
    public class HierachicalMap
    {
        #region Ctor

        public HierachicalMap(Vector2 dimensions)
        {
            _dimensions = dimensions;

            _clusterSize = new Vector2(10);

            _clusterColumnCount = (int) (_dimensions.X / _clusterSize.X);
            _clusterRowCount = (int) (_dimensions.Y / _clusterSize.Y);
            _clusters = new Cluster[_clusterColumnCount][];
            for (var i = 0; i < _clusterColumnCount; i++)
            {
                _clusters[i] = new Cluster[_clusterRowCount];
                for (var j = 0; j < _clusterRowCount; j++)
                {
                    _clusters[i][j] = new Cluster(i, j, _clusterSize);
                }
            }
        }

        #endregion


        public void Add(ComponentViewModel component)
        {
            if (_componentClusterDictionary.TryAdd(component.Id, new List<Cluster>()))
            {
                _components.Add(component);
            }

            Update(component);
        }

        public void Remove(ComponentViewModel component)
        {
            if (_componentClusterDictionary.Remove(component.Id))
            {
                _components.Remove(component);
            }
        }

        public void Update(ComponentViewModel component)
        {
            var invalidatedClusters = new HashSet<Cluster>();

            var currentClusters = _componentClusterDictionary[component.Id];
            foreach (var currentCluster in currentClusters)
            {
                currentCluster.Children.Remove(component);
                invalidatedClusters.Add(currentCluster);
            }

            currentClusters.Clear();

            foreach (var clusterArray in _clusters)
            {
                foreach (var cluster in clusterArray)
                {
                    if (component.CollidesWith(cluster.Region, _collisionDetector))
                    {
                        _componentClusterDictionary[component.Id].Add(cluster);
                        cluster.Children.Add(component);
                        invalidatedClusters.Add(cluster);
                    }
                }
            }

            foreach (var invalidatedCluster in invalidatedClusters)
            {
                UpdateCluster(invalidatedCluster);
            }
        }

        public void UpdateClusters()
        {
            for (var i = 0; i < _clusterColumnCount; i++)
            {
                for (var j = 0; j < _clusterRowCount; j++)
                {
                    UpdateCluster(_clusters[i][j]);
                }
            }
        }

        public void UpdateCluster(Cluster cluster)
        {
            UpdateIntraEdges(cluster);
        }

        public void UpdateIntraEdges(Cluster cluster)
        {
            var doorPoints = GetDoorPointsInCluster(cluster);
            foreach(var entranceId1 in doorPoints.Keys)
            {
                foreach(var entranceId2 in doorPoints.Keys)
                {
                    if(entranceId1 != entranceId2)
                    {
                        var entranceIntraEdges = new Dictionary<(Guid, Guid), List<IntraEdge>>()
                        { 
                            { 
                                (entranceId1, entranceId2), new List<IntraEdge>()
                            } 
                        };
                        var intraEdgeList = new List<IntraEdge>();
                        var entrance1DoorPoints = doorPoints[entranceId1];
                        var entrance2DoorPoints = doorPoints[entranceId2];
                        for(var i = 0; i < entrance1DoorPoints.Count; i++)
                        {
                            for(var j = 0; j < entrance2DoorPoints.Count; j++)
                            {
                                var intraEdge = new IntraEdge(FindPath(entrance1DoorPoints[i], entrance2DoorPoints[j], cluster));
                                intraEdgeList.Add(intraEdge);
                            }
                        }
                        entranceIntraEdges[(entranceId1, entranceId2)] = intraEdgeList;
                        ClusterIntraEdges.Add(cluster.Id, entranceIntraEdges);
                    }
                }
            }

        }

        public void UpdateEntrances()
        {
            foreach(var clusterColumn in _clusters)
            {
                foreach(var cluster in clusterColumn)
                {
                    var adjacentClusters = GetAdjacentClusters(cluster);
                    foreach(var adjacentCluster in adjacentClusters)
                    {
                        UpdateEntrance(cluster, adjacentCluster.Value);
                    }
                }
            }

        }

        public void UpdateEntrance(Cluster baseCluster, Cluster adjacentCluster)
        {
            var entrance = new Entrance(baseCluster, adjacentCluster);
            Entrances.Add(entrance);
            ClusterEntranceDictionary[baseCluster.Id].Add(entrance);
        }

        public Dictionary<Guid, List<Point>> GetDoorPointsInCluster(Cluster cluster)
        {
            var clusterEntrances = GetEntrances(cluster);
            var doorPointDictionary = new Dictionary<Guid, List<Point>>();
            foreach(var entrance in clusterEntrances)
            {
                var doorPointsInCluster = entrance.DoorPoints.Select(d => d.Item1).ToList();
                doorPointDictionary.Add(entrance.Id, doorPointsInCluster);
            }
            return doorPointDictionary;
        }
        public List<Entrance> GetEntrances(Cluster cluster)
        {
            return ClusterEntranceDictionary[cluster.Id];
        }

        public List<Point> FindPath(Point a, Point b, Cluster cluster)
        {
            var path = new List<Point>();
            var map = new Maps.Map(cluster.Region.Size, a.Position, b.Position, cluster.Children.ToArray());
            var vectorPath = new Algorithm.PathFinder(map, DefaultNeighbourFinder.Straight(1.0f)).FindPath();
            foreach(var pathPoint in vectorPath)
            {
                path.Add(new Point(pathPoint));
            }
            return path;
        }

        public Cluster GetCluster(int i, int j)
        {
            if (0 <= i && i <= _clusterColumnCount && 0 <= j && j <= _clusterRowCount)
            {
                return _clusters[i][j];
            }
            return Cluster.Empty;
        }

        public Dictionary<Direction, Cluster> GetAdjacentClusters(Cluster cluster)
        {

            var i = cluster.Column;
            var j = cluster.Row;
            var adjacentClusters = new Dictionary<Direction, Cluster>()
            {   {Direction.Up, GetCluster(i, j - 1) },  //The cluster above this cluster
                {Direction.Down, GetCluster(i, j - 1) },  //The cluster below this cluster
                {Direction.Left, GetCluster(i - 1, j) },  //The cluster to the left of this cluster
                {Direction.Right, GetCluster(i + 1, j) }  //The cluster to the right of this cluster
            };
            return adjacentClusters;
        }

        public void ClearEntrances()
        {
            Entrances.Clear();
        }


        #region Props

        private readonly Vector2 _dimensions;
        private readonly Vector2 _clusterSize;
        private readonly int _clusterColumnCount;
        private readonly int _clusterRowCount;
        private readonly Cluster[][] _clusters;
        public List<Entrance> Entrances { get; set; }
        public Dictionary<Guid, List<Entrance>> ClusterEntranceDictionary { get; set; }
        public Dictionary<Guid, Dictionary<(Guid, Guid), List<IntraEdge>>> ClusterIntraEdges = new();
        private readonly Dictionary<Guid, List<Cluster>> _componentClusterDictionary = new();
        private readonly List<ComponentViewModel> _components = new();
        private readonly CollisionDetector _collisionDetector = new HybridCollisionDetector();

        #endregion
    }

    public class Cluster : IEquatable<Cluster>
    {
        #region Props

        public Guid Id { get; set; }
        public Rectangle Region { get; private set; }
        public List<ComponentViewModel> Children { get; }
        
        public int Row { get; }
        public int Column { get; }

        #endregion

        #region Ctor

        public Cluster(int column, int row, Vector2 clusterSize)
        {
            Column = column;
            Row = row;
            Id = Guid.NewGuid();
            Region = Rectangle.Create(new Vector2(column * clusterSize.X, row * clusterSize.Y), clusterSize);
            Children = new List<ComponentViewModel>();
        }

        protected Cluster()
        {
            Id = Guid.Empty;
        }

        public static readonly Cluster Empty = new()
        {
            Region = Rectangle.Zero
        };

        #endregion

        #region IEquatable

        public override int GetHashCode() => Region.GetHashCode();

        public virtual bool Equals(Cluster other) =>
            Region == other?.Region;

        #endregion
    }

    public enum Direction
    {
        Up,
        Right,
        Down,
        Left,
        None
    }

    public class Entrance
    {
        public Entrance(Cluster a, Cluster b)
        {
            baseCluster = a;
            adjacentCluster = b;
            DoorPoints = SetDoorPoints(a, b);
            Id = Guid.NewGuid();
        }

        private List<(Point, Point)> SetDoorPoints(Cluster a, Cluster b)
        {
            var collisionDetector = new HybridCollisionDetector();
            var adjacentEdgePoints = GetAdjacentEdgePoints(a, b);

            var doorPoints = new List<(Point, Point)>();

            var aComponents = a.Children;
            var bComponents = b.Children;

            var lastPointPair = adjacentEdgePoints[0];
            (Point, Point) lastPointPairAdded = (Point.Zero, Point.Zero);

            var aWasColliding = true;
            var bWasColliding = true;

            foreach(var adjacentEdgePointPair in adjacentEdgePoints)
            {
                var aIsColliding = aComponents.All(c => !c.CollidesWith(adjacentEdgePointPair.Item1, collisionDetector));
                var bIsColliding = bComponents.All(c => !c.CollidesWith(adjacentEdgePointPair.Item2, collisionDetector));

                if((aIsColliding && !aWasColliding) || (bIsColliding && !bWasColliding))
                {
                    if (lastPointPairAdded != lastPointPair)
                    {
                        lastPointPairAdded = lastPointPair;
                        doorPoints.Add(lastPointPair);
                    }
                }
                else if((!aIsColliding && aWasColliding && !bIsColliding) || (!bIsColliding && bWasColliding && !aIsColliding))
                {
                    lastPointPairAdded = adjacentEdgePointPair;
                    doorPoints.Add(adjacentEdgePointPair);
                }
                if(!aIsColliding && !bIsColliding)
                {
                    lastPointPair = adjacentEdgePointPair;
                }

                aIsColliding = aWasColliding;
                bIsColliding = bWasColliding;

            }
            return doorPoints;
        }

        public List<Point> GetEdgePoints(Cluster cluster, Direction direction)
        {
            var edgePointList = new List<Point>();
            var clusterWidth = cluster.Region.Width;
            var clusterHeight = cluster.Region.Height;
            var clusterTopLeft = cluster.Region.TopLeft;
            var clusterTopRight = cluster.Region.TopRight;
            var clusterBottomLeft = cluster.Region.BottomLeft;
            switch (direction)
            {
                case Direction.Up:
                    for (var i = 0; i < clusterWidth; i++)
                    {
                        edgePointList.Add(new Point(clusterTopLeft + new Vector2(i, 0)));
                    }
                    break;
                case Direction.Down:
                    for (var i = 0; i < clusterWidth; i++)
                    {
                        edgePointList.Add(new Point(clusterBottomLeft + new Vector2(i, 0)));
                    }
                    break;
                case Direction.Left:
                    for (var i = 0; i < clusterHeight; i++)
                    {
                        edgePointList.Add(new Point(clusterTopLeft + new Vector2(0, i)));
                    }
                    break;
                case Direction.Right:
                    for (var i = 0; i < clusterHeight; i++)
                    {
                        edgePointList.Add(new Point(clusterTopRight + new Vector2(0, i)));
                    }
                    break;

            }
            return edgePointList;

        }
        public List<(Point, Point)> GetAdjacentEdgePoints(Cluster a, Cluster b)
        {
            var adjacencyDirectionAB = GetAdjacencyDirection(a, b);
            var adjacencyDirectionBA = GetAdjacencyDirection(a, b);

            var aEdgePoints = GetEdgePoints(a, adjacencyDirectionAB);
            var bEdgePoints = GetEdgePoints(b, adjacencyDirectionBA);

            var adjacentEdgePoints = new List<(Point, Point)>();

            for (var i = 0; i < a.Region.Width; i++)
            {
                adjacentEdgePoints.Add((aEdgePoints[i], bEdgePoints[i]));
            }

            return adjacentEdgePoints;


        }

        public Direction GetAdjacencyDirection(Cluster a, Cluster b)
        {
            var i1 = a.Column;
            var i2 = b.Column;
            var j1 = a.Row;
            var j2 = b.Row;
            if (j1 > j2)
            {
                return Direction.Up;
            }
            if (i1 < i2)
            {
                return Direction.Right;
            }
            if (j1 < j2)
            {
                return Direction.Down;
            }
            if (i1 > i2)
            {
                return Direction.Left;
            }
            return Direction.None;
        }

        public Cluster baseCluster { get; }
        public Cluster adjacentCluster { get; }

        public Guid Id { get; set; }
        public List<(Point, Point)> DoorPoints { get; set; }
    }

    public class IntraEdge
    {
        public List<Point> Path { get; set; }
        public (Point, Point) EndPoints { get; set; }

        public IntraEdge(List<Point> path)
        {
            Path = path;
            EndPoints = (path.First(), path.Last());
        }

    }
}