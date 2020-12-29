using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components;
using Aptacode.Geometry.Collision;
using Aptacode.Geometry.Primitives;
using Aptacode.Geometry.Primitives.Polygons;

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
            var i = cluster.Column;
            var j = cluster.Row;
            var topCluster = j >= 1 ? _clusters[i][j - 1] : Cluster.Empty;
            var rightCluster = i < _clusterColumnCount ? _clusters[i + 1][j] : Cluster.Empty;
            var bottomCluster = j < _clusterRowCount ? _clusters[i][j + 1] : Cluster.Empty;
            var leftCluster = i >= 1 ? _clusters[i - 1][j] : Cluster.Empty;
            var middleCluster = _clusters[i][j];

            middleCluster.ClearEntrances();
            //ToDo Interpolate over the edges to get a list of potential points
            middleCluster.Entrances[topCluster.Id] = GetEntrances(middleCluster, topCluster, new List<Point>());
            middleCluster.Entrances[rightCluster.Id] = GetEntrances(middleCluster, rightCluster, new List<Point>());
            middleCluster.Entrances[bottomCluster.Id] = GetEntrances(middleCluster, bottomCluster, new List<Point>());
            middleCluster.Entrances[leftCluster.Id] = GetEntrances(middleCluster, leftCluster, new List<Point>());
        }

        public void UpdateIntraPaths(Cluster cluster)
        {
            //Find paths between each entrance
        }

        public List<Entrance> GetEntrances(Cluster a, Cluster b, List<Point> edgePoints)
        {
            var entrances = new List<Entrance>();

            var components = a.Children.Intersect(b.Children);

            Point lastPoint = edgePoints[0];
            Point lastPointAdded = null;
            var wasColliding = true;

            foreach (var edgePoint in edgePoints)
            {
                var isColliding = components.All(c => !c.CollidesWith(edgePoint, _collisionDetector));

                if (isColliding && !wasColliding)
                {
                    //Add Last Point
                    if (lastPointAdded != lastPoint)
                    {
                        lastPointAdded = lastPoint;
                        entrances.Add(new Entrance(lastPoint));
                    }
                }
                else if (!isColliding && wasColliding)
                {
                    //Add current Point
                    lastPointAdded = edgePoint;
                    entrances.Add(new Entrance(edgePoint));
                }

                if (!isColliding)
                {
                    lastPoint = edgePoint;
                }

                wasColliding = isColliding;
            }

            return entrances;
        }

        public List<Point> FindPath(Point a, Point b)
        {
            var path = new List<Point>();


            return path;
        }

        #region Props

        private readonly Vector2 _dimensions;
        private readonly Vector2 _clusterSize;
        private readonly int _clusterColumnCount;
        private readonly int _clusterRowCount;
        private readonly Cluster[][] _clusters;

        private readonly Dictionary<Guid, List<Cluster>> _componentClusterDictionary = new();
        private readonly List<ComponentViewModel> _components = new();
        private readonly CollisionDetector _collisionDetector = new HybridCollisionDetector();

        #endregion
    }

    public class Cluster : IEquatable<Cluster>
    {
        public void SetEntrances(Guid clusterId, List<Entrance> entrances)
        {
            Entrances[clusterId] = entrances;
        }

        public void ClearEntrances()
        {
            Entrances.Clear();
        }

        #region Props

        public Guid Id { get; set; }
        public Rectangle Region { get; private set; }
        public List<ComponentViewModel> Children { get; }
        public Dictionary<Guid, List<Entrance>> Entrances { get; }
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
            Entrances = new Dictionary<Guid, List<Entrance>>();
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

    public class Entrance
    {
        public Entrance(Point point)
        {
            Point = point;
            Paths = new List<Path>();
        }

        public Point Point { get; set; }
        public List<Path> Paths { get; set; }
    }

    public class Path
    {
        public List<Entrance> Entrances { get; set; }
        public PolyLine PolyLine { get; set; }
    }
}