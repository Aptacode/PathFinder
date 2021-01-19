﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aptacode.AppFramework.Components;
using Aptacode.AppFramework.Scene;
using Aptacode.Geometry.Primitives.Polygons;
using Aptacode.PathFinder.Utilities;
using Priority_Queue;

namespace Aptacode.PathFinder.Maps.Hpa
{
    public class HierachicalMap : IDisposable
    {
        #region Ctor

        public HierachicalMap(Scene scene, int maxLevel)
        {
            _scene = scene;
            //_scene.OnComponentAdded += SceneOnOnComponentAdded;
            //_scene.OnComponentRemoved += SceneOnOnComponentRemoved;

            _clusters = new Cluster[maxLevel][][];
            _clusterSize = new Vector2[maxLevel];
            _clusterColumnCount = new int[maxLevel];
            _clusterRowCount = new int[maxLevel];

            for (var i = 0; i < maxLevel; i++)
            {
                var clusterSize = new Vector2((int) Math.Pow(10, i + 1)); //Using 10^x where x is the level, could be changed, might not be necessary.
                var clusterColumnCount = (int) (scene.Size.X / clusterSize.X); //Map dimensions must be divisible by chosen cluster size
                var clusterRowCount = (int) (scene.Size.Y / clusterSize.Y);
                var clusters = new Cluster[clusterColumnCount][];
                _clusterSize[i] = clusterSize;
                _clusterColumnCount[i] = clusterColumnCount;
                _clusterRowCount[i] = clusterRowCount;

                for (var x = 0; x < clusterColumnCount; x++)
                {
                    clusters[x] = new Cluster[clusterRowCount];
                    for (var y = 0; y < clusterRowCount; y++)
                    {
                        clusters[x][y] = new Cluster(i, x, y, clusterSize);
                    }
                }

                _clusters[i] = clusters;
                UpdateClusters(i);
            }

            foreach (var componentViewModel in scene.Components)
            {
                Update(componentViewModel);
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

        public void UpdateDoorPoints(Cluster cluster, Cluster adjacentCluster, Direction direction, int clusterWidth, int clusterHeight)
        {
            if (adjacentCluster == Cluster.Empty)
            {
                return;
            }

            var offset = 0;
            var adjacencyOffset = 0;
            var edgeCount = 0;
            switch (direction)
            {
                case Direction.Up:
                    offset = 0;
                    adjacencyOffset = clusterWidth + clusterHeight;
                    edgeCount = clusterWidth;
                    break;
                case Direction.Right:
                    offset = clusterWidth;
                    adjacencyOffset = clusterWidth + clusterHeight + clusterWidth;
                    edgeCount = clusterHeight;
                    break;
                case Direction.Down:
                    offset = clusterWidth + clusterHeight;
                    adjacencyOffset = 0;
                    edgeCount = clusterWidth;
                    break;
                case Direction.Left:
                    offset = clusterWidth + clusterHeight + clusterWidth;
                    adjacencyOffset = clusterWidth;
                    edgeCount = clusterHeight;
                    break;
            }

            var j = 0;
            var lastEdgePoint = EdgePoint.Empty;

            for (var i = 0; i < edgeCount; i++)
            {
                var edgePoint = cluster.EdgePoints[offset + i];
                if (edgePoint.AdjacencyDirection != direction)
                {
                    continue;
                }

                var adjacentEdgePoint = adjacentCluster.EdgePoints[adjacencyOffset + i];

                var baseClusterHasCollision = cluster.EdgePointHasCollision(edgePoint);
                var adjacentClusterHasCollision = adjacentCluster.EdgePointHasCollision(adjacentEdgePoint);

                if (!baseClusterHasCollision && !adjacentClusterHasCollision)
                {
                    j++;
                }
                else if (j != 0)
                {
                    cluster.DoorPoints.Add(lastEdgePoint);
                    if (j > 1)
                    {
                        cluster.DoorPoints.Add(cluster.EdgePoints[offset + i - j]);
                    }

                    j = 0;
                }

                lastEdgePoint = edgePoint;
            }

            if (j == 0)
            {
                return;
            }

            cluster.DoorPoints.Add(cluster.EdgePoints[offset + edgeCount - 1]);
            if (j > 1)
            {
                cluster.DoorPoints.Add(cluster.EdgePoints[offset + edgeCount - j]);
            }
        }

        public void UpdateDoorPointsInCluster(Cluster cluster)
        {
            cluster.DoorPoints.Clear();

            var clusterWidth = (int) (cluster.Region.Size.X + 1);
            var clusterHeight = (int) (cluster.Region.Size.Y + 1);

            var i = cluster.Column;
            var j = cluster.Row;
            var level = cluster.Level;

            UpdateDoorPoints(cluster, GetCluster(level, i, j - 1), Direction.Up, clusterWidth, clusterHeight);
            UpdateDoorPoints(cluster, GetCluster(level, i + 1, j), Direction.Right, clusterWidth, clusterHeight);
            UpdateDoorPoints(cluster, GetCluster(level, i, j + 1), Direction.Down, clusterWidth, clusterHeight);
            UpdateDoorPoints(cluster, GetCluster(level, i - 1, j), Direction.Left, clusterWidth, clusterHeight);
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
            var clusterSize = _clusterSize[level - 1];

            var clusterColumn = (int) Math.Floor(point.X / clusterSize.X);
            var clusterRow = (int) Math.Floor(point.Y / clusterSize.Y);
            return _clusters[level - 1][clusterColumn][clusterRow];
        }

        public static Vector2[] RefineAbstractPath(AbstractNode[] abstractPath, Vector2 endPoint, int level)
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

            var edgeCount = 0;
            for (var i = 1; i < abstractPath.Length; i++) //The first node does not have a parentIntraEdge of relevance
            {
                edgeCount += abstractPath[i].ParentIntraEdge.Path.Length;
            }
            //Paths are backwards so some of this isn't right

            var endNode = abstractPath.Last();
            var endNodeDoorPoint = endNode.DoorPoint;
            var endNodePath = endNode.Cluster.FindConcretePath(endNodeDoorPoint, endPoint);
            edgeCount += endNodePath.Length;


            var output = new Vector2[edgeCount];
            var index = 0;
            for (var i = 1; i < abstractPath.Length; i++) //The first node does not have a parentIntraEdge of relevance
            {
                var path = abstractPath[i].ParentIntraEdge.Path;
                for (var j = 0; j < path.Length; j++)
                {
                    output[index++] = path[j];
                }
            }

            for (var i = 1; i < endNodePath.Length; i++) //The first node does not have a parentIntraEdge of relevance
            {
                output[index++] = endNodePath[i];
            }

            return output;
        }

        public void CheckCluster(Cluster currentCluster, Cluster adjacentCluster, AbstractNode currentNode, Direction direction, AbstractNode endNode)
        {
            var intraEdge = currentCluster.GetIntraEdge(currentNode.DoorPoint, direction);
            if (intraEdge.Equals(IntraEdge.Empty))
            {
                return;
            }

            var neighbourDoorPoint = intraEdge.Path.Last().GetAdjacentPoint(intraEdge.AdjacencyDirection);
            var neighbourCost = currentNode.Cost + intraEdge.Path.Length; //This is 1 more than the actual path length but works for our purposes so we don't need to add inter-edge costs (1)

            var node = new AbstractNode(currentNode, endNode, adjacentCluster, neighbourDoorPoint, intraEdge, neighbourCost);

            if (_closedAbstractNodes.Contains(node)
            ) //Don't need to recheck node if it's already be looked at
            {
                return;
            }

            if (_openAbstractNodes.TryGetValue(node.DoorPoint, out var existingNode))
            {
                if (!(existingNode.CostDistance > node.CostDistance))
                {
                    return;
                }

                _sortedOpenAbstractNodes.Remove(node);
            }

            _sortedOpenAbstractNodes.Enqueue(node, node.CostDistance);
            _openAbstractNodes[node.DoorPoint] = node;
        }

        private readonly HashSet<AbstractNode> _closedAbstractNodes = new();
        private readonly Dictionary<Vector2, AbstractNode> _openAbstractNodes = new();
        private readonly FastPriorityQueue<AbstractNode> _sortedOpenAbstractNodes = new(200);

        public AbstractNode[] FindAbstractPath(Vector2 startPoint, Vector2 endPoint, int level) //This is A* on Hierachical map clusters as nodes
        {
            _closedAbstractNodes.Clear();
            _openAbstractNodes.Clear();
            _sortedOpenAbstractNodes.Clear();

            var endNode = SetEndNode(endPoint, level);
            var startNode = SetStartNode(startPoint, endNode, level);
            _sortedOpenAbstractNodes.Enqueue(startNode, startNode.CostDistance);
            _openAbstractNodes.Add(startNode.DoorPoint, startNode);

            while (_sortedOpenAbstractNodes.Count > 0)
            {
                var currentNode = _sortedOpenAbstractNodes.Dequeue();
                if (currentNode.Cluster == endNode.Cluster) //if we've reached the end node a path has been found.
                {
                    var node = currentNode;
                    var abstractPath = new List<AbstractNode>();
                    while (true)
                    {
                        abstractPath.Add(node);
                        node = node.Parent;
                        if (node == AbstractNode.Empty)
                        {
                            return abstractPath.Backwards(); //Flip it here and then we don't need to worry using it.
                        }
                    }
                }

                _closedAbstractNodes.Add(currentNode);
                _openAbstractNodes.Remove(currentNode.DoorPoint);

                var currentCluster = currentNode.Cluster;

                var i = currentCluster.Column;
                var j = currentCluster.Row;
                var level2 = currentCluster.Level;

                CheckCluster(currentCluster, GetCluster(level2, i, j - 1), currentNode, Direction.Up, endNode);
                CheckCluster(currentCluster, GetCluster(level2, i + 1, j), currentNode, Direction.Right, endNode);
                CheckCluster(currentCluster, GetCluster(level2, i, j + 1), currentNode, Direction.Down, endNode);
                CheckCluster(currentCluster, GetCluster(level2, i - 1, j), currentNode, Direction.Left, endNode);
            }

            return Array.Empty<AbstractNode>(); //need to be wary of this.
        }

        public Vector2[] FindPath(Vector2 startPoint, Vector2 endPoint, int level)
        {
            var abstractPath = FindAbstractPath(startPoint, endPoint, level);
            return RefineAbstractPath(abstractPath, endPoint, level);
        }

        public AbstractNode SetStartNode(Vector2 point, AbstractNode target, int level)
        {
            var cluster = GetClusterContainingPoint(point, level);
            var edgePoint = new EdgePoint(Direction.None, point);
            AddIntraEdge(edgePoint, cluster, Direction.Up);
            AddIntraEdge(edgePoint, cluster, Direction.Right);
            AddIntraEdge(edgePoint, cluster, Direction.Down);
            AddIntraEdge(edgePoint, cluster, Direction.Left);
            return new AbstractNode(AbstractNode.Empty, target, cluster, point, IntraEdge.Empty, 0);
        }

        public AbstractNode SetEndNode(Vector2 point, int level)
        {
            var cluster = GetClusterContainingPoint(point, level);
            return new AbstractNode(cluster, point);
        }

        #endregion

        #region Props

        private readonly Scene _scene;

        //Index is the level
        private readonly Vector2[] _clusterSize;
        private readonly int[] _clusterColumnCount;
        private readonly int[] _clusterRowCount;
        private readonly Cluster[][][] _clusters;
        private readonly Dictionary<Guid, List<Cluster>> _componentClusterDictionary = new();

        #endregion

        #region ComponentViewModels

        public void Add(ComponentViewModel component)
        {
            Update(component);
        }

        public void Remove(ComponentViewModel component)
        {
            Update(component);
        }

        private readonly HashSet<Cluster> _invalidatedClusters = new();

        public void Update(ComponentViewModel component)
        {
            _invalidatedClusters.Clear();

            if (_componentClusterDictionary.TryGetValue(component.Id, out var currentClusters))
            {
                foreach (var currentCluster in currentClusters)
                {
                    currentCluster.Components.Remove(component);
                    _invalidatedClusters.Add(currentCluster);
                }

                currentClusters.Clear();
            }
            else
            {
                currentClusters = new List<Cluster>();
                _componentClusterDictionary.Add(component.Id, currentClusters);
            }

            for (var i = 0; i < _clusters.Length; i++)
            {
                for (var j = 0; j < _clusters[i].Length; j++)
                {
                    for (var k = 0; k < _clusters[i][j].Length; k++)
                    {
                        var cluster = _clusters[i][j][k];
                        if (!component.CollidesWith(cluster.Region))
                        {
                            continue;
                        }

                        currentClusters.Add(cluster);
                        cluster.Components.Add(component);
                        _invalidatedClusters.Add(cluster);
                    }
                }
            }

            foreach (var invalidatedCluster in _invalidatedClusters)
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

        #endregion

        #region IntraEdges

        public void UpdateIntraEdges(Cluster cluster)
        {
            cluster.IntraEdges.Clear();
            if (cluster.DoorPoints.Count == 0)
            {
                return;
            }

            foreach (var doorPoint in cluster.DoorPoints)
            {
                AddIntraEdge(doorPoint, cluster, Direction.Up);
                AddIntraEdge(doorPoint, cluster, Direction.Right);
                AddIntraEdge(doorPoint, cluster, Direction.Down);
                AddIntraEdge(doorPoint, cluster, Direction.Left);
            }
        }

        public void AddIntraEdge(EdgePoint point, Cluster cluster, Direction adjacencyDirection)
        {
            for (var i = 0; i < cluster.IntraEdges.Count; i++)
            {
                var edge = cluster.IntraEdges[i];
                if (edge.Path[0] == point.Position && edge.AdjacencyDirection == adjacencyDirection) //We may have already found the intraedge with one of the other shortcuts.
                {
                    return;
                }
            }

            if (point.AdjacencyDirection == adjacencyDirection) //Don't waste time finding the shortest path, this has to be it.
            {
                cluster.IntraEdges.Add(new IntraEdge(adjacencyDirection, new[] {point.Position}));
                return;
            }

            Vector2[] shortestPathInDirection = null;
            var shortestIntraEdgeLength = int.MaxValue;

            foreach (var doorPoint in cluster.DoorPoints)
            {
                if (doorPoint.AdjacencyDirection != adjacencyDirection) //We only want IntraEdges in the direction that we're currently trying to add them in the direction of
                {
                    continue;
                }

                for (var i = 0; i < cluster.IntraEdges.Count; i++)
                {
                    var edge = cluster.IntraEdges[i];
                    if (edge.Path[0] != doorPoint.Position || edge.Path.Last() != point.Position) //We might have already found this path in the other direction and it is necessarily the shortest path by definition of how we found it.
                    {
                        continue;
                    }

                    var reversePath = edge.Path.Backwards();
                    cluster.IntraEdges.Add(new IntraEdge(adjacencyDirection, reversePath));
                    return; //Now we don't need to calculate the path again.
                }

                var path = cluster.FindConcretePath(point.Position, doorPoint.Position);

                if (path.Length >= shortestIntraEdgeLength)
                {
                    continue;
                }

                shortestIntraEdgeLength = path.Length;
                shortestPathInDirection = path;

                //var shortestPossiblePath = Vector2.Abs(point.Position - doorPoint.Position);
                //if (shortestPossiblePath.X + shortestPossiblePath.Y + 1 >= path.Length) //Might be better served a little earlier?
                //{
                //    break;
                //}
            }

            if (shortestPathInDirection?.Length > 0)
            {
                cluster.IntraEdges.Add(new IntraEdge(adjacencyDirection, shortestPathInDirection));
            }
        }

        #endregion
    }

    public record Cluster
    {
        #region IntraEdges

        public IntraEdge GetIntraEdge(Vector2 startPoint, Direction direction)
        {
            for (var i = 0; i < IntraEdges.Count; i++)
            {
                var edge = IntraEdges[i];
                if (edge.Path[0] == startPoint && edge.AdjacencyDirection == direction) //Need point equality or to move away from using points, probably this one.
                {
                    return edge;
                }
            }

            return IntraEdge.Empty;
        }

        #endregion

        #region EdgePoints

        public EdgePoint[] SetEdgePoints()
        {
            var clusterWidth = Region.Width;
            var clusterHeight = Region.Height;
            var edgePoints = new EdgePoint[(int) ((clusterWidth + 1) * 4)];

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
                    case Direction.Right:
                        for (var j = 0; j <= clusterHeight; j++)
                        {
                            edgePoints[edgeIndex++] = new EdgePoint(direction, Region.TopRight + new Vector2(0, j));
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
                }
            }

            return edgePoints;
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

        #region Concrete Pathfinding

        public bool HasCollision(Vector2 point)
        {
            for (var i = 0; i < Components.Count; i++)
            {
                if (Components[i].CollidesWith(point))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsInCluster(Vector2 point)
        {
            return Region.TopLeft.X <= point.X &&
                   Region.TopLeft.Y <= point.Y &&
                   Region.BottomRight.X >= point.X &&
                   Region.BottomRight.Y >= point.Y;
        }

        public bool IsInValidPosition(Vector2 point)
        {
            return IsInCluster(point) && !HasCollision(point);
        }

        private readonly HashSet<Vector2> _closedConcreteNodes = new();
        private readonly Dictionary<Vector2, ConcreteNode> _openConcreteNodes = new();
        private readonly FastPriorityQueue<ConcreteNode> _sortedOpenConcreteNodes = new(200);

        public Vector2[] FindConcretePath(Vector2 startPoint, Vector2 endPoint) //This is A* as normal within the confines of a cluster.
        {
            _closedConcreteNodes.Clear();
            _openConcreteNodes.Clear();
            _sortedOpenConcreteNodes.Clear();

            var endNode = this.SetEndConcreteNode(endPoint);
            var startNode = this.SetStartConcreteNode(startPoint, endPoint);
            _sortedOpenConcreteNodes.Enqueue(startNode, startNode.CostDistance);
            _openConcreteNodes.Add(startNode.Position, startNode);

            while (_sortedOpenConcreteNodes.Count > 0)
            {
                var currentNode = _sortedOpenConcreteNodes.Dequeue();
                if (currentNode.Position == endNode.Position) //if we've reached the end node a path has been found.
                {
                    var node = currentNode;
                    var path = new List<Vector2>();
                    while (true)
                    {
                        path.Add(node.Position);
                        node = node.Parent;
                        if (node == ConcreteNode.Empty)
                        {
                            return path.Backwards(); //Flip it here and then we don't need to worry using it.
                        }
                    }
                }

                _closedConcreteNodes.Add(currentNode.Position);
                _openConcreteNodes.Remove(currentNode.Position);


                for (var index = 0; index < 4; index++)
                {
                    var direction = (Direction) index;

                    var neighbour = currentNode.Position.GetAdjacentPoint(direction);

                    if (!currentNode.Cluster.IsInValidPosition(neighbour))
                    {
                        continue;
                    }

                    var node = new ConcreteNode(currentNode.Cluster, currentNode, endNode, neighbour, currentNode.Cost + 1); //Back to hardcoding this for now because the og neighbour finder stuff is ugly.

                    if (_closedConcreteNodes.Contains(node.Position)) //Don't need to recheck node if it's already be looked at
                    {
                        continue;
                    }

                    if (_openConcreteNodes.TryGetValue(node.Position, out var existingOpenNode))
                    {
                        if (!(existingOpenNode.CostDistance > node.CostDistance))
                        {
                            continue;
                        }

                        _sortedOpenConcreteNodes.Remove(existingOpenNode);
                    }

                    _sortedOpenConcreteNodes.Enqueue(node, node.CostDistance);
                    _openConcreteNodes[node.Position] = node;
                }
            }

            return Array.Empty<Vector2>(); //need to be wary of this.
        }

        #endregion

        #region Props

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
            EdgePoints = SetEdgePoints();
            DoorPoints = new List<EdgePoint>();
            IntraEdges = new List<IntraEdge>();
        }

        public static readonly Cluster Empty = new();

        public Cluster(Vector2 clusterSize)
        {
            Column = 1;
            Row = 1;
            Id = Guid.NewGuid();
            Level = 1;
            var rectSize = clusterSize - new Vector2(1, 1);
            Region = Rectangle.Create(new Vector2(1, 1), rectSize);
            Components = new List<ComponentViewModel>();
            EdgePoints = SetEdgePoints();
            DoorPoints = new List<EdgePoint> {EdgePoints[0], EdgePoints[9], EdgePoints[10], EdgePoints[19], EdgePoints[20], EdgePoints[29], EdgePoints[30], EdgePoints[39]};
            IntraEdges = new List<IntraEdge>();
        }

        #endregion
    }

    public static class ClusterExtensions
    {
        public static ConcreteNode SetStartConcreteNode(this Cluster cluster, Vector2 point, Vector2 target)
        {
            return new(cluster, point, target);
        }

        public static ConcreteNode SetEndConcreteNode(this Cluster cluster, Vector2 point)
        {
            return new(cluster, point);
        }
    }

    public enum Direction
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,
        None = 4
    }

    public record IntraEdge(Direction AdjacencyDirection, Vector2[] Path)
    {
        public static IntraEdge Empty = new(Direction.None, Array.Empty<Vector2>());
    }

    public readonly struct EdgePoint
    {
        public readonly Direction AdjacencyDirection;

        public readonly Vector2 Position;

        public EdgePoint(Direction adjacencyDirection, Vector2 position)
        {
            Position = position;
            AdjacencyDirection = adjacencyDirection;
        }

        public static readonly EdgePoint Empty = new(Direction.None, Vector2.Zero);
    }

    public class AbstractNode : FastPriorityQueueNode
    {
        public static readonly AbstractNode Empty = new();
        public readonly Cluster Cluster;

        public readonly float Cost;
        public readonly float CostDistance;
        public readonly float Distance;
        public readonly Vector2 DoorPoint;
        public readonly AbstractNode Parent;
        public readonly IntraEdge ParentIntraEdge;

        public AbstractNode(AbstractNode parent, AbstractNode target, Cluster cluster, Vector2 doorPoint, IntraEdge parentIntraEdge, float cost)
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

        public AbstractNode(Cluster cluster, Vector2 doorPoint)
        {
            Parent = Empty;
            Cluster = cluster;
            DoorPoint = doorPoint;
            Cost = float.MaxValue;
            Distance = 0;
            CostDistance = float.MaxValue;
            ParentIntraEdge = IntraEdge.Empty;
        }

        protected AbstractNode()
        {
            Parent = Empty;
            Cluster = Cluster.Empty;
            DoorPoint = Vector2.Zero;
            ParentIntraEdge = IntraEdge.Empty;
        }
    }

    public class ConcreteNode : FastPriorityQueueNode
    {
        public static readonly ConcreteNode Empty = new();
        public readonly Cluster Cluster;

        public readonly float Cost;
        public readonly float CostDistance;
        public readonly float Distance;
        public readonly ConcreteNode Parent;
        public readonly Vector2 Position;

        public ConcreteNode(Cluster cluster, ConcreteNode parent, ConcreteNode target, Vector2 position, float cost)
        {
            Parent = parent;
            Position = position;
            Cost = cost;
            var distanceVector = Vector2.Abs(target.Position - Position);
            Distance = distanceVector.X + distanceVector.Y;
            CostDistance = Cost + Distance;
            Cluster = cluster;
        }

        protected ConcreteNode()
        {
            Parent = Empty;
            Position = Vector2.Zero;
            Cluster = Cluster.Empty;
        }

        public ConcreteNode(Cluster cluster, Vector2 position)
        {
            Parent = Empty;
            Position = position;
            Cost = float.MaxValue;
            Distance = 0;
            CostDistance = float.MaxValue;
            Cluster = cluster;
        }

        internal ConcreteNode(Cluster cluster, Vector2 position, Vector2 target)
        {
            Position = position;
            Cost = 0;
            Parent = Empty;
            Cluster = cluster;
            var delta = Vector2.Abs(Position - target);
            Distance = delta.X + delta.Y;

            CostDistance = Cost + Distance;
        }

        #region IEquatable

        public virtual bool Equals(ConcreteNode other)
        {
            return other is not null && Position == other.Position;
        }

        #endregion

        public static ConcreteNode EndNode(Cluster cluster, Vector2 position)
        {
            return new(cluster, position);
        }

        public static ConcreteNode StartNode(Cluster cluster, Vector2 position, Vector2 target)
        {
            return new(cluster, position, target);
        }
    }

    public static class ListExtensions
    {
        public static T[] Backwards<T>(this List<T> list)
        {
            var n = list.Count;
            T[] reverseList = new T[n];

            for (var i = 0; i < n; i++)
            {
                reverseList[n - 1 - i] = list[i];
            }

            return reverseList;
        }

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

    public static class Vector2Extensions
    {
        private static readonly Vector2 _up = new(0, -1);
        private static readonly Vector2 _down = new(0, 1);
        private static readonly Vector2 _left = new(-1, 0);
        private static readonly Vector2 _right = new(1, 0);

        public static Vector2 GetAdjacentPoint(this Vector2 point, Direction adjacencyDirection)
        {
            return adjacencyDirection switch
            {
                Direction.Up => point + _up,
                Direction.Right => point + _right,
                Direction.Down => point + _down,
                Direction.Left => point + _left,
                _ => point
            };
        }
    }
}