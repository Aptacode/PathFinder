using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aptacode.AppFramework.Components;
using Aptacode.AppFramework.Scene;
using Aptacode.Geometry.Collision;
using Aptacode.Geometry.Primitives.Polygons;
using Aptacode.PathFinder.Utilities;
using Constants = Aptacode.Geometry.Constants;

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

            _clusters = new Cluster[maxLevel + 1][][];
            _clusterSize = new Vector2[maxLevel + 1];
            _clusterColumnCount = new int[maxLevel + 1];
            _clusterRowCount = new int[maxLevel + 1];
            
            for (var i = 0; i <= maxLevel; i++)
            {
                var clusterSize = new Vector2((int) Math.Pow(10, i)); //Using 10^x where x is the level, could be changed, might not be necessary.
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

        public void UpdateDoorPointsInCluster(Cluster cluster)
        {
            cluster.DoorPoints.Clear();

            var clusterWidth = (int) (cluster.Region.Size.X + 1);
            var clusterHeight = (int) (cluster.Region.Size.Y + 1);

            var clusters = GetAdjacentClusters(cluster);
            for (var index = 0; index < 4; index++)
            {
                var adjacentCluster = clusters[index];
                var direction = (Direction)index;
                
                if (adjacentCluster == Cluster.Empty)
                {
                    continue;
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
                EdgePoint lastEdgePoint = null;
                
                for (var i = 0; i < edgeCount; i++)
                {
                    var edgePoint = cluster.EdgePoints[offset + i];
                    if(edgePoint.AdjacencyDirection != direction)
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
                    continue;
                }

                cluster.DoorPoints.Add(cluster.EdgePoints[offset + edgeCount - 1]);
                if (j > 1)
                {
                    cluster.DoorPoints.Add(cluster.EdgePoints[offset + edgeCount - j]);
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

            var clusters = GetAdjacentClusters(currentCluster);
            for (var index = 0; index < 4; index++)
            {
                var adjacentCluster = clusters[index];
                var direction = (Direction)index;
                
                var intraEdge = currentCluster.GetIntraEdge(currentNode.DoorPoint, direction);
                if (intraEdge == IntraEdge.Empty)
                {
                    continue;
                }

                var neighbourDoorPoint = GetAdjacentPoint(intraEdge.Path.Last(), intraEdge.AdjacencyDirection);
                var neighbourCost = currentNode.Cost + intraEdge.Path.Length; //This is 1 more than the actual path length but works for our purposes so we don't need to add inter-edge costs (1)

                yield return new Node(currentNode, targetNode, adjacentCluster, neighbourDoorPoint, intraEdge, neighbourCost);
            }
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

        public void Update(ComponentViewModel component)
        {
            var invalidatedClusters = new HashSet<Cluster>();

            if (_componentClusterDictionary.TryGetValue(component.Id, out var currentClusters))
            {
                foreach (var currentCluster in currentClusters)
                {
                    currentCluster.Components.Remove(component);
                    invalidatedClusters.Add(currentCluster);
                }
                currentClusters.Clear();
            }
            else
            {
                currentClusters = new List<Cluster>();
                _componentClusterDictionary.Add(component.Id, currentClusters);
            }


            foreach (var clusterArray in _clusters)
            {
                foreach (var clusterColumn in clusterArray)
                {
                    foreach (var cluster in clusterColumn)
                    {
                        if (!component.CollidesWith(cluster.Region))
                        {
                            continue;
                        }

                        currentClusters.Add(cluster);
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

        public Cluster[] GetAdjacentClusters(Cluster cluster)
        {
            var i = cluster.Column;
            var j = cluster.Row;
            var level = cluster.Level;
            
            return new []
            {
                GetCluster(level, i, j - 1),//The cluster above this cluster
                GetCluster(level, i + 1, j),//The cluster to the right of this cluster
                GetCluster(level, i, j + 1),//The cluster below this cluster
                GetCluster(level, i - 1, j)//The cluster to the left of this cluster
            };
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
            cluster.IntraEdges.Clear();
            
            AddIntraEdge(point, cluster, Direction.Up);
            AddIntraEdge(point, cluster, Direction.Right);
            AddIntraEdge(point, cluster, Direction.Down);
            AddIntraEdge(point, cluster, Direction.Left);
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

            foreach (var doorPoint in cluster.DoorPoints)
            {
                if (doorPoint.AdjacencyDirection != adjacencyDirection)
                {
                    continue;
                }

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

    public class Cluster
    {
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

        #region IntraEdges

        public IntraEdge GetIntraEdge(Vector2 startPoint, Direction direction)
        {
            for (var i = 0; i < IntraEdges.Count; i++)
            {
                var edge = IntraEdges[i];
                if (edge.AdjacencyDirection == direction && edge.Path[0] == startPoint) //Need point equality or to move away from using points, probably this one.
                {
                    return edge;
                }
            }

            return IntraEdge.Empty;
        }

        public IntraEdge GetIntraEdge(Vector2 startPoint, Vector2 endPoint)
        {
            for (var i = 0; i < IntraEdges.Count; i++)
            {
                var edge = IntraEdges[i];
                if (edge.Path.Last() == endPoint && edge.Path[0] == startPoint)
                {
                    return edge;
                }
            }

            return IntraEdge.Empty;
        }

        public bool HasIntraEdge(Vector2 startPoint, Vector2 endPoint, Direction direction)
        {
            for (var i = 0; i < IntraEdges.Count; i++)
            {
                var edge = IntraEdges[i];
                if (edge.AdjacencyDirection == direction && edge.Path.Last() == endPoint && edge.Path[0] == startPoint)
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
                var edge = IntraEdges[i];
                if (edge.Path[0] == endPoint && edge.Path.Last() == startPoint)
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
                var edge = IntraEdges[i];
                if (edge.Path[0] == startPoint && edge.AdjacencyDirection == direction)
                {
                    return true;
                }
            }

            return false;
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

        #endregion
    }

    public enum Direction
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,
        None = 4
    }

    public class IntraEdge
    {
        public static IntraEdge Empty = new();

        public IntraEdge(Direction adjacencyDirection, Vector2[] points)
        {
            Path = points;
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
    
    public record EdgePoint
    {
        public EdgePoint(Direction adjacencyDirection, Vector2 position)
        {
            Position = position;
            AdjacencyDirection = adjacencyDirection;
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