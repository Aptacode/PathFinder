﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aptacode.AppFramework.Components;
using Aptacode.AppFramework.Scene;
using Aptacode.Geometry.Collision;
using Aptacode.Geometry.Primitives.Polygons;
using Aptacode.PathFinder.Utilities;

namespace Aptacode.PathFinder.Maps.Hpa
{
    public class HierachicalMap : IDisposable
    {
        #region Ctor

        public HierachicalMap(Scene scene, int maxLevel)
        {
            _scene = scene;
            _scene.OnComponentAdded += SceneOnOnComponentAdded;
            _scene.OnComponentRemoved += SceneOnOnComponentRemoved;

            for (var i = 0; i <= maxLevel; i++)
            {
                var clusterSize = new Vector2((int) Math.Pow(10, i)); //Using 10^x where x is the level, could be changed, might not be necessary.
                var clusterColumnCount = (int) (scene.Size.X / clusterSize.X); //Map dimensions must be divisible by chosen cluster size
                var clusterRowCount = (int) (scene.Size.Y / clusterSize.Y);
                var clusters = new Cluster[clusterColumnCount][];
                _clusterSize.Add(i, clusterSize);
                _clusterColumnCount.Add(i, clusterColumnCount);
                _clusterRowCount.Add(i, clusterRowCount);
                for (var x = 0; x < clusterColumnCount; x++)
                {
                    clusters[x] = new Cluster[clusterRowCount];
                    for (var y = 0; y < clusterRowCount; y++)
                    {
                        clusters[x][y] = new Cluster(i, x, y, clusterSize);
                    }
                }

                _clusters.Add(i, clusters);
                foreach (var component in scene.Components)
                {
                    Add(component);
                }

                UpdateClusters(i);
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _scene.OnComponentAdded -= SceneOnOnComponentAdded;
            _scene.OnComponentRemoved -= SceneOnOnComponentRemoved;
        }

        #endregion

        #region DoorPoints

        public void UpdateDoorPointsInCluster(Cluster cluster)
        {
            foreach (var (key, adjacentCluster) in GetAdjacentClusters(cluster))
            {
                if (adjacentCluster == Cluster.Empty)
                {
                    continue;
                }

                var baseClusterEdgePoints = cluster.GetEdgePoints(key);
                var adjacentClusterEdgePoints = adjacentCluster.GetEdgePoints(key.Inverse());
                var j = 0;
                for (var i = 0; i < baseClusterEdgePoints.Count; i++)
                {
                    var baseClusterHasCollision = cluster.EdgePointHasCollision(baseClusterEdgePoints[i]);
                    var adjacentClusterHasCollision = adjacentCluster.EdgePointHasCollision(adjacentClusterEdgePoints[i]);


                    if (!baseClusterHasCollision && !adjacentClusterHasCollision)
                    {
                        j++;
                    }
                    else if (j != 0)
                    {
                        cluster.DoorPoints.Add(baseClusterEdgePoints[i - 1]);
                        if (j > 1)
                        {
                            cluster.DoorPoints.Add(baseClusterEdgePoints[i - j]);
                        }

                        j = 0;
                    }
                }

                if (j == 0)
                {
                    continue;
                }

                cluster.DoorPoints.Add(baseClusterEdgePoints.Last());
                if (j > 1)
                {
                    cluster.DoorPoints.Add(baseClusterEdgePoints[^j]);
                }
            }
        }


        #endregion

        #region Events

        private void SceneOnOnComponentAdded(object? sender, ComponentViewModel component)
        {
            Add(component);
        }

        private void SceneOnOnComponentRemoved(object? sender, ComponentViewModel component)
        {
            Remove(component);
        }

        #endregion

        #region Path Finding

        public Cluster GetClusterContainingPoint(Vector2 point, int level)
        {
            var clusterSize = _clusterSize[level];

            var clusterColumn = (int) Math.Floor(point.X / clusterSize.X);
            var clusterRow = (int) Math.Floor(point.Y / clusterSize.Y);
            return _clusters[level][clusterColumn][clusterRow];
        }

        public Vector2[] RefineAbstractPath(Node[] abstractPath, Vector2 endPoint, int level)
        {
            if (abstractPath.Length == 0)
            {
                return Array.Empty<Vector2>();
            }

            if (level == 0) //This path is the refined path, just need to convert and stitch it together
            {
                var refinedPath = new Vector2[abstractPath.Length];
                for (var i = 0; i < abstractPath.Length; i++)
                {
                    refinedPath[i] = abstractPath[i].DoorPoint;
                }

                return refinedPath;
            }
            else
            {
                var refinedPath = new List<Vector2>();
                for (var i = 1; i < abstractPath.Length; i++) //The first node does not have a parentIntraEdge of relevance
                {
                    refinedPath.AddRange(abstractPath[i].ParentIntraEdge.Path);
                }

                //Paths are backwards so some of this isn't right

                var endNode = abstractPath.Last();
                var endNodeDoorPoint = endNode.DoorPoint;
                var endNodePath = FindPath(endNodeDoorPoint, endPoint, abstractPath.Last().Cluster.Level - 1);
                refinedPath.AddRange(endNodePath);
                return refinedPath.ToArray();
            }
        }

        public Node[] FindAbstractPath(Vector2 startPoint, Vector2 endPoint, int level) //This is A*
        {
            var endNode = SetEndNode(endPoint, level);
            var startNode = SetStartNode(startPoint, endNode, level);

            var sortedOpenNodes = new PriorityQueue<float, Node>();

            sortedOpenNodes.Enqueue(startNode, startNode.CostDistance);

            var closedNodes = new Dictionary<Vector2, Node>();
            var openNodes = new Dictionary<Vector2, Node>
            {
                {startNode.DoorPoint, startNode}
            };

            while (!sortedOpenNodes.IsEmpty())
            {
                var currentNode = sortedOpenNodes.Dequeue();
                if (currentNode.Cluster == endNode.Cluster) //if we've reached the end node a path has been found.
                {
                    var node = currentNode;
                    var abstractPath = new List<Node>();
                    while (true)
                    {
                        abstractPath.Add(node);
                        node = node.Parent;
                        if (node == Node.Empty)
                        {
                            return abstractPath.ToArray().Backwards(); //Flip it here and then we don't need to worry using it.
                        }
                    }
                }

                closedNodes[currentNode.DoorPoint] = currentNode;
                openNodes.Remove(currentNode.DoorPoint);

                foreach (var node in GetNeighbours(currentNode, endNode))
                {
                    if (closedNodes.ContainsKey(node.DoorPoint)
                    ) //Don't need to recheck node if it's already be looked at
                    {
                        continue;
                    }

                    if (openNodes.TryGetValue(node.DoorPoint, out var existingOpenNode))
                    {
                        if (!(existingOpenNode.CostDistance > node.CostDistance))
                        {
                            continue;
                        }

                        sortedOpenNodes.Remove(existingOpenNode, existingOpenNode.CostDistance);
                    }

                    sortedOpenNodes.Enqueue(node, node.CostDistance);
                    openNodes[node.DoorPoint] = node;
                }
            }

            return Array.Empty<Node>(); //need to be wary of this.
        }

        public Vector2[] FindPath(Vector2 startPoint, Vector2 endPoint, int level)
        {
            var abstractPath = FindAbstractPath(startPoint, endPoint, level);
            return RefineAbstractPath(abstractPath, endPoint, level);
        }

        public Node SetStartNode(Vector2 point, Node target, int level)
        {
            var cluster = GetClusterContainingPoint(point, level);
            AddIntraEdgeInEachValidDirection(new EdgePoint(Direction.None, point), cluster);
            return new Node(Node.Empty, target, cluster, point, IntraEdge.Empty, 0);
        }

        public Node SetEndNode(Vector2 point, int level)
        {
            var cluster = GetClusterContainingPoint(point, level);
            return new Node(cluster, point);
        }

        public Vector2 GetAdjacentPoint(Vector2 point, Direction adjacencyDirection)
        {
            return adjacencyDirection switch
            {
                Direction.Up => point + new Vector2(0, -1),
                Direction.Down => point + new Vector2(0, 1),
                Direction.Left => point + new Vector2(-1, 0),
                Direction.Right => point + new Vector2(1, 0),
                _ => point
            };
        }

        public IEnumerable<Node> GetNeighbours(Node currentNode, Node targetNode)
        {
            var currentCluster = currentNode.Cluster;
            foreach (var adjacentCluster in GetAdjacentClusters(currentCluster))
            {
                var intraEdge = currentCluster.GetIntraEdge(currentNode.DoorPoint, adjacentCluster.Key);
                if (intraEdge == IntraEdge.Empty)
                {
                    continue;
                }

                var neighbourDoorPoint = GetAdjacentPoint(intraEdge.Path.Last(), intraEdge.AdjacencyDirection);
                var neighbourCost = currentNode.Cost + intraEdge.Path.Length; //This is 1 more than the actual path length but works for our purposes so we don't need to add inter-edge costs (1)

                yield return new Node(currentNode, targetNode, adjacentCluster.Value, neighbourDoorPoint, intraEdge, neighbourCost);
            }
        }

        #endregion

        #region Props

        private readonly Scene _scene;
        private readonly Dictionary<int, Vector2> _clusterSize = new();
        private readonly Dictionary<int, int> _clusterColumnCount = new(); //Key is the level
        private readonly Dictionary<int, int> _clusterRowCount = new(); //Key is the level
        private readonly Dictionary<int, Cluster[][]> _clusters = new();
        private readonly Dictionary<Guid, List<Cluster>> _componentClusterDictionary = new();
        private readonly List<ComponentViewModel> _components = new();
        private readonly CollisionDetector _collisionDetector = new HybridCollisionDetector();

        #endregion

        #region ComponentViewModels

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
                currentCluster.Components.Remove(component);
                invalidatedClusters.Add(currentCluster);
            }

            currentClusters.Clear();

            foreach (var clusterArray in _clusters.Values)
            {
                foreach (var clusterColumn in clusterArray)
                {
                    foreach (var cluster in clusterColumn)
                    {
                        if (!component.CollidesWith(cluster.Region))
                        {
                            continue;
                        }

                        _componentClusterDictionary[component.Id].Add(cluster);
                        cluster.Components.Add(component);
                        invalidatedClusters.Add(cluster);
                    }
                }
            }

            foreach (var invalidatedCluster in invalidatedClusters)
            {
                UpdateCluster(invalidatedCluster);
            }
        }

        #endregion

        #region Cluster

        public void UpdateClusters(int level)
        {
            for (var i = 0; i < _clusterColumnCount[level]; i++)
            {
                for (var j = 0; j < _clusterRowCount[level]; j++)
                {
                    UpdateCluster(_clusters[level][i][j]);
                }
            }
        }

        public void UpdateCluster(Cluster cluster)
        {
            UpdateDoorPointsInCluster(cluster);
            UpdateIntraEdges(cluster);
        }

        public Cluster GetCluster(int level, int x, int y)
        {
            if (x >= 0 && x < _clusterColumnCount[level] && y >= 0 && y < _clusterRowCount[level])
            {
                return _clusters[level][x][y];
            }

            return Cluster.Empty;
        }

        public Dictionary<Direction, Cluster> GetAdjacentClusters(Cluster cluster)
        {
            var i = cluster.Column;
            var j = cluster.Row;
            var level = cluster.Level;
            return new Dictionary<Direction, Cluster>
            {
                {Direction.Up, GetCluster(level, i, j - 1)}, //The cluster above this cluster
                {Direction.Down, GetCluster(level, i, j + 1)}, //The cluster below this cluster
                {Direction.Left, GetCluster(level, i - 1, j)}, //The cluster to the left of this cluster
                {Direction.Right, GetCluster(level, i + 1, j)} //The cluster to the right of this cluster
            };
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

        #endregion

        #region IntraEdges

        public void UpdateIntraEdges(Cluster cluster)
        {
            foreach (var doorPoint in cluster.DoorPoints)
            {
                AddIntraEdgeInEachValidDirection(doorPoint, cluster);
            }
        }

        public void AddIntraEdgeInEachValidDirection(EdgePoint point, Cluster cluster)
        {
            if (cluster.DoorPoints.Count == 0)
            {
                return;
            }

            for (var i = 0; i <= 3; i++)
            {
                var direction = (Direction) i;
                AddIntraEdge(point, cluster, direction);
            }
        }

        public void AddIntraEdge(EdgePoint point, Cluster cluster, Direction adjacencyDirection)
        {
            if (cluster.HasIntraEdgeWithStartPoint(point.Position, adjacencyDirection)) //No need to find the same intra edges again.
            {
                return;
            }

            if (point.AdjacencyDirection == adjacencyDirection) //Don't waste time finding the shortest path, this has to be it.
            {
                cluster.IntraEdges.Add(new IntraEdge(adjacencyDirection, point.Position));
                return;
            }

            var shortestPathInDirection = Array.Empty<Vector2>();
            var shortestIntraEdgeLength = int.MaxValue;

            foreach (var doorPoint in cluster.GetDoorPoints(adjacencyDirection))
            {
                if (cluster.HasIntraEdge(doorPoint.Position, point.Position) //Check to see if we've already calculated an IntraEdge between these two points.
                    && doorPoint.AdjacencyDirection != adjacencyDirection) //But make sure they aren't different points with the same adjacency direction
                {
                    var intraEdge = cluster.GetIntraEdge(doorPoint.Position, point.Position); //This is necessarily the correct intraEdge because only the shortest intraEdge in this direction for the door point is saved.
                    var path = intraEdge.Path.Backwards();
                    cluster.IntraEdges.Add(new IntraEdge(adjacencyDirection, path));
                    return; //Now we don't need to calculate the path again.
                }

                switch (cluster.Level)
                {
                    case 0:
                        cluster.IntraEdges.Add(new IntraEdge(adjacencyDirection, doorPoint.Position));
                        return;
                    case > 0:
                    {
                        var path = FindPath(point.Position, doorPoint.Position, cluster.Level - 1);

                        if (path.Length < shortestIntraEdgeLength) //We only want the shortest intraedge in this direction.
                        {
                            shortestIntraEdgeLength = path.Length;
                            shortestPathInDirection = path;
                        }

                        break;
                    }
                }
            }

            if (shortestPathInDirection.Length > 0)
            {
                cluster.IntraEdges.Add(new IntraEdge(adjacencyDirection, shortestPathInDirection));
            }
        }

        #endregion
    }

    public class Cluster : IEquatable<Cluster>
    {
        #region DoorPoints

        public List<EdgePoint> GetDoorPoints(Direction direction)
        {
            var doorPointsInGivenDirection = new List<EdgePoint>();
            for (var i = 0; i < DoorPoints.Count; i++)
            {
                if (DoorPoints[i].AdjacencyDirection == direction)
                {
                    doorPointsInGivenDirection.Add(DoorPoints[i]);
                }
            }

            return doorPointsInGivenDirection;
        }

        #endregion

        #region EdgePoints

        public EdgePoint[] SetEdgePoints()
        {
            var clusterWidth = Region.Width;
            var clusterHeight = Region.Height;
            var edgePoints = new EdgePoint[(int)((clusterWidth + 1) * 4)];

            var edgeIndex = 0;
            for (var i = 0; i <= 3; i++)
            {
                var direction = (Direction) i;
                switch (direction)
                {
                    case Direction.Up:
                        for (var j = 0; j <= clusterWidth; j++)
                        {
                            edgePoints[edgeIndex++] = new EdgePoint(direction, Region.TopLeft + new Vector2(j, 0));
                        }
                        continue;
                    case Direction.Down:
                        for (var j = 0; j <= clusterWidth; j++)
                        {
                            edgePoints[edgeIndex++] = new EdgePoint(direction, Region.BottomLeft + new Vector2(j, 0));
                        }
                        continue;
                    case Direction.Left:
                        for (var j = 0; j <= clusterHeight; j++)
                        {
                            edgePoints[edgeIndex++] = new EdgePoint(direction, Region.TopLeft + new Vector2(0, j));
                        }
                        continue;
                    case Direction.Right:
                        for (var j = 0; j <= clusterHeight; j++)
                        {
                            edgePoints[edgeIndex++] = new EdgePoint(direction, Region.TopRight + new Vector2(0, j));
                        }
                        continue;
                }
            }

            return edgePoints;
        }

        public List<EdgePoint> GetEdgePoints(Direction direction) //Should test to see if linq is faster??
        {
            var edgePointsInGivenDirection = new List<EdgePoint>();
            for (var i = 0; i < EdgePoints.Length; i++)
            {
                if (EdgePoints[i].AdjacencyDirection == direction)
                {
                    edgePointsInGivenDirection.Add(EdgePoints[i]);
                }
            }

            return edgePointsInGivenDirection;
        }

        public bool EdgePointHasCollision(EdgePoint edgePoint)
        {
            for (var i = 0; i < Components.Count; i++)
            {
                if (Components[i].CollidesWith(edgePoint.Position))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region IntraEdges

        public IntraEdge GetIntraEdge(Vector2 startPoint, Direction direction)
        {
            for (var i = 0; i < IntraEdges.Count; i++)
            {
                if (IntraEdges[i].AdjacencyDirection == direction && IntraEdges[i].Path[0] == startPoint) //Need point equality or to move away from using points, probably this one.
                {
                    return IntraEdges[i];
                }
            }

            return IntraEdge.Empty;
        }

        public IntraEdge GetIntraEdge(Vector2 startPoint, Vector2 endPoint)
        {
            for (var i = 0; i < IntraEdges.Count; i++)
            {
                if (IntraEdges[i].Path.Last() == endPoint && IntraEdges[i].Path[0] == startPoint)
                {
                    return IntraEdges[i];
                }
            }

            return IntraEdge.Empty;
        }

        public bool HasIntraEdge(Vector2 startPoint, Vector2 endPoint, Direction direction)
        {
            for (var i = 0; i < IntraEdges.Count; i++)
            {
                if (IntraEdges[i].AdjacencyDirection == direction && IntraEdges[i].Path.Last() == endPoint && IntraEdges[i].Path[0] == startPoint)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasIntraEdge(Vector2 startPoint, Vector2 endPoint)
        {
            for (var i = 0; i < IntraEdges.Count; i++)
            {
                if (IntraEdges[i].Path[0] == endPoint && IntraEdges[i].Path.Last() == startPoint)
                {
                    return true;
                }
            }

            return false;
        }


        public bool HasIntraEdgeWithStartPoint(Vector2 startPoint, Direction direction)
        {
            for (var i = 0; i < IntraEdges.Count; i++)
            {
                if (IntraEdges[i].Path[0] == startPoint && IntraEdges[i].AdjacencyDirection == direction)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Props
        public static readonly FineCollisionDetector CollisionDetector = new();

        public readonly Guid Id;
        public readonly Rectangle Region;
        public readonly List<ComponentViewModel> Components;

        public readonly List<IntraEdge> IntraEdges;
        public readonly List<EdgePoint> DoorPoints;
        public readonly EdgePoint[] EdgePoints;
        public readonly int Level;
        public readonly int Row;
        public readonly int Column;

        #endregion

        #region Ctor

        public Cluster(int level, int column, int row, Vector2 clusterSize)
        {
            Column = column;
            Row = row;
            Id = Guid.NewGuid();
            Level = level;
            var rectSize = clusterSize - new Vector2(1, 1); //We want the rectangle to contain as many points as the cluster size along its edge hence -1
            Region = Rectangle.Create(new Vector2(column * clusterSize.X, row * clusterSize.Y), rectSize);
            Components = new List<ComponentViewModel>();
            EdgePoints = SetEdgePoints();
            DoorPoints = new List<EdgePoint>();
            IntraEdges = new List<IntraEdge>();
        }

        private Cluster()
        {
            Id = Guid.Empty;
            Column = 0;
            Row = 0;
            Id = Guid.NewGuid();
            Level = 0;
            Region = Rectangle.Zero;
            Components = new List<ComponentViewModel>();
            EdgePoints = SetEdgePoints();
            DoorPoints = new List<EdgePoint>();
            IntraEdges = new List<IntraEdge>();
        }

        public static readonly Cluster Empty = new();

        #endregion

        #region IEquatable

        public override int GetHashCode()
        {
            return Region.GetHashCode();
        }

        public virtual bool Equals(Cluster other)
        {
            return Region == other.Region;
        }

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

    public static class DirectionExtensions
    {
        public static Direction Inverse(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                Direction.None => Direction.None,
                _ => Direction.None
            };
        }
    }


    public class IntraEdge
    {
        public static IntraEdge Empty = new();

        public IntraEdge(Direction adjacencyDirection, Vector2[] points)
        {
            Path = points; //Paths are constructed backwards
            AdjacencyDirection = adjacencyDirection;
        }

        public IntraEdge(Direction adjacencyDirection, Vector2 point)
        {
            Path = new[] {point};
            AdjacencyDirection = adjacencyDirection;
        }

        public IntraEdge()
        {
            Path = Array.Empty<Vector2>();
            AdjacencyDirection = Direction.None;
        }

        public readonly Vector2[] Path;
        public readonly Direction AdjacencyDirection;
    }

    public static class IntraEdgeExtensions
    {
        public static IntraEdge Reverse(this IntraEdge intraEdge)
        {
            var reversePath = intraEdge.Path.Backwards();
            var reverseDirection = intraEdge.AdjacencyDirection.Inverse();
            return new IntraEdge(reverseDirection, reversePath);
        }
    }

    public record EdgePoint
    {
        public EdgePoint(Direction adjacencyDirection, Vector2 position)
        {
            Position = position;
            AdjacencyDirection = adjacencyDirection;
        }

        public EdgePoint(Vector2 position)
        {
            Position = position;
            AdjacencyDirection = Direction.None;
        }

        public readonly Vector2 Position;
        public readonly Direction AdjacencyDirection;
    }

    public record Node
    {
        public static readonly Node Empty = new();
        public readonly Cluster Cluster;

        public readonly float Cost;
        public readonly float CostDistance;
        public readonly float Distance;
        public readonly Vector2 DoorPoint;
        public readonly Node Parent;
        public readonly IntraEdge ParentIntraEdge;

        public Node(Node parent, Node target, Cluster cluster, Vector2 doorPoint, IntraEdge parentIntraEdge, float cost)
        {
            Parent = parent;
            Cluster = cluster;
            DoorPoint = doorPoint;
            Cost = cost;
            ParentIntraEdge = parentIntraEdge;
            var distanceVector = Vector2.Abs(target.DoorPoint - DoorPoint);
            Distance = distanceVector.X + distanceVector.Y;
            CostDistance = Cost + Distance;
        }

        public Node(Cluster cluster, Vector2 doorPoint)
        {
            Parent = Empty;
            Cluster = cluster;
            DoorPoint = doorPoint;
            Cost = float.MaxValue;
            Distance = 0;
            CostDistance = float.MaxValue;
            ParentIntraEdge = IntraEdge.Empty;
        }

        protected Node()
        {
            Parent = Empty;
            Cluster = Cluster.Empty;
            DoorPoint = Vector2.Zero;
            ParentIntraEdge = IntraEdge.Empty;
        }
    }

    public static class ListExtensions
    {
        public static T[] Backwards<T>(this T[] list)
        {
            var n = list.Length;
            T[] reverseList = new T[n];

            for (var i = 0; i < n; i++)
            {
                reverseList[n - 1 - i] = list[i];
            }

            return reverseList;
        }
    }
}