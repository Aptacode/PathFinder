using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Aptacode.AppFramework.Components;
using Aptacode.Geometry.Primitives.Polygons;
using Priority_Queue;

namespace Aptacode.PathFinder.Maps.Hpa
{
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        private readonly FastPriorityQueue<ConcreteNode> _sortedOpenConcreteNodes = new(100);

        public Vector2[] FindConcretePath(Vector2 startPoint, Vector2 endPoint) //This is A* as normal within the confines of a cluster.
        {
            _closedConcreteNodes.Clear();
            _openConcreteNodes.Clear();
            _sortedOpenConcreteNodes.Clear();

            var endNode = new ConcreteNode(this, endPoint);
            var startNode = new ConcreteNode(this, startPoint, endPoint);
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

        public readonly int Row;
        public readonly int Column;

        #endregion

        #region Ctor

        public Cluster(int column, int row, Vector2 clusterSize)
        {
            Column = column;
            Row = row;
            Id = Guid.NewGuid();
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
            var rectSize = clusterSize - new Vector2(1, 1);
            Region = Rectangle.Create(new Vector2(1, 1), rectSize);
            Components = new List<ComponentViewModel>();
            EdgePoints = SetEdgePoints();
            DoorPoints = new List<EdgePoint> {EdgePoints[0], EdgePoints[9], EdgePoints[10], EdgePoints[19], EdgePoints[20], EdgePoints[29], EdgePoints[30], EdgePoints[39]};
            IntraEdges = new List<IntraEdge>();
        }

        #endregion
    }
}