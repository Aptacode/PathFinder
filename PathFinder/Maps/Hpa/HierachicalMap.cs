using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Aptacode.Geometry.Blazor.Components.ViewModels.Components;
using Aptacode.Geometry.Collision;
using Aptacode.Geometry.Primitives;
using Aptacode.Geometry.Primitives.Polygons;
using Aptacode.PathFinder.Utilities;

namespace Aptacode.PathFinder.Maps.Hpa
{
    public class HierachicalMap
    {

        #region Props

        private readonly Vector2 _dimensions;
        private readonly int _maxLevel;
        private Dictionary<int, Vector2> _clusterSize = new();
        private Dictionary<int, int> _clusterColumnCount = new(); //Key is the level
        private Dictionary<int, int> _clusterRowCount = new(); //Key is the level
        private Dictionary<int, Cluster[][]> _clusters = new();
        private readonly Dictionary<Guid, List<Cluster>> _componentClusterDictionary = new();
        private readonly List<ComponentViewModel> _components = new();
        private readonly CollisionDetector _collisionDetector = new HybridCollisionDetector();

        #endregion

        #region Ctor

        public HierachicalMap(Vector2 dimensions, ComponentViewModel[] components, int maxLevel)
        {
            _dimensions = dimensions;
            _maxLevel = maxLevel;
            for (var i = 0; i <= _maxLevel; i++)
            {
                var clusterSize = new Vector2((int)Math.Pow(10, i)); //Using 10^x where x is the level, could be changed, might not be necessary.
                var clusterColumnCount = (int)(_dimensions.X / clusterSize.X); //Map dimensions must be divisible by chosen cluster size
                var clusterRowCount = (int)(_dimensions.Y / clusterSize.Y);
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
                UpdateClusters(i);
            }

            _components = components.ToList();


        }

        #endregion
        #region ComponentViewModel

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

            foreach (var clusterArray in _clusters.Values)
            {
                foreach (var clusterColumn in clusterArray)
                {
                    foreach (var cluster in clusterColumn)
                    {
                        if (component.CollidesWith(cluster.Region, _collisionDetector))
                        {
                            _componentClusterDictionary[component.Id].Add(cluster);
                            cluster.Children.Add(component);
                            invalidatedClusters.Add(cluster);
                        }
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
            var adjacentClusters = new Dictionary<Direction, Cluster>()
            {   {Direction.Up, GetCluster(level, i, j - 1) },  //The cluster above this cluster
                {Direction.Down, GetCluster(level, i, j + 1) },  //The cluster below this cluster
                {Direction.Left, GetCluster(level, i - 1, j) },  //The cluster to the left of this cluster
                {Direction.Right, GetCluster(level, i + 1, j) }  //The cluster to the right of this cluster
            };
            return adjacentClusters;
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

        public void AddIntraEdgeInEachValidDirection(Point point, Cluster cluster)
        {
            if(cluster.DoorPoints.Count > 0) //Don't add intraEdges to clusters that can't be escaped.
            {
                for(var i = 0; i <= 3; i++)
                {
                    var direction = (Direction)i;
                    AddIntraEdge(point, cluster, direction);
                }
            }
            
        }

        public void AddIntraEdge(Point point, Cluster cluster, Direction adjacencyDirection)
        {
            var clusterDoorPointsInDirection = cluster.GetDoorPoints(adjacencyDirection);
            if (clusterDoorPointsInDirection.Count > 0)
            {
                if(cluster.Level == 0)
                {
                    cluster.IntraEdges.Add(new IntraEdge(adjacencyDirection, clusterDoorPointsInDirection[0]));
                }

                else if(cluster.Level > 0)
                {
                    var shortestPathInDirection = new List<Point>();
                    var shortestIntraEdgeLength = int.MaxValue;
                    foreach (var clusterDoorPoint in clusterDoorPointsInDirection)
                    {
                        var path = FindPath(point, clusterDoorPoint, cluster.Level - 1);
                        
                        if (path.Count < shortestIntraEdgeLength)
                        {
                            shortestIntraEdgeLength = path.Count;
                            shortestPathInDirection = path;
                        }
                        //get the shortest path and then add that as an intraedge only.
                    }
                    cluster.IntraEdges.Add(new IntraEdge(adjacencyDirection, shortestPathInDirection.ToArray()));
                }
            }
        }


        #endregion


        #region DoorPoints

        public void UpdateDoorPointsInCluster(Cluster cluster)
        {
            var adjacentClusters = GetAdjacentClusters(cluster);
            foreach (var adjacentCluster in adjacentClusters)
            {
                if (adjacentCluster.Value != Cluster.Empty)
                {
                    var baseClusterEdgePoints = cluster.GetEdgePoints(adjacentCluster.Key);
                    var adjacentClusterEdgePoints = adjacentCluster.Value.GetEdgePoints(adjacentCluster.Key.Inverse());
                    int j = 0;
                    for (var i = 0; i < baseClusterEdgePoints.Count; i++)
                    {
                        var baseClusterHasCollision = cluster.EdgePointHasCollision(baseClusterEdgePoints[i]);
                        var adjacentClusterHasCollision = adjacentCluster.Value.EdgePointHasCollision(adjacentClusterEdgePoints[i]);
                        

                        if (!baseClusterHasCollision && !adjacentClusterHasCollision)
                        {
                            j++;
                            continue;
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
                    if(j != 0) //No more collisions before we reach the end of the edge
                    {
                        cluster.DoorPoints.Add(baseClusterEdgePoints.Last());
                        if(j > 1)
                        {
                            cluster.DoorPoints.Add(baseClusterEdgePoints[baseClusterEdgePoints.Count - j]);
                        }
                    } //This logic looks a bit weird.
                }
            }
        }

        #endregion


        public Cluster GetClusterContainingPoint(Point point, int level)
        {
            var clusterSize = _clusterSize[level];
            var pointPosition = point.Position;

            var clusterColumn = (int) Math.Floor(pointPosition.X / clusterSize.X);
            var clusterRow = (int) Math.Floor(pointPosition.Y / clusterSize.Y);
            return _clusters[level][clusterColumn][clusterRow];
        }

        public List<Point> RefineAbstractPath(List<Node> abstractPath, Point endPoint, int level)
        {
            if(abstractPath.Count == 0)
            {
                return new List<Point>();
            }
            if(level == 0) //This path is the refined path, just need to convert and stitch it together
            {
                var refinedPath = new List<Point>();
                for (var i = 0; i < abstractPath.Count; i++)
                {
                    refinedPath.Add(abstractPath[i].DoorPoint);
                }
                return refinedPath;
            }
            else
            {
                var refinedPath = new List<Point>();
                for(var i = abstractPath.Count - 2; i >= 0; i--) //Start node has no parent and hence no parent intraEdge
                {
                    refinedPath = refinedPath.Concat(abstractPath[i].ParentIntraEdge.Path).ToList(); //This is ugly.
                } //Construct path up to final node adjacent DoorPoint;

                //Paths are backwards so some of this isn't right

                var endNode = abstractPath[0];
                var endNodeDoorPoint = endNode.DoorPoint;
                var endNodePath = FindPath(endPoint, endNodeDoorPoint, abstractPath[0].Cluster.Level - 1); //do it this way so you don't have to flip the list befor concat
                refinedPath = refinedPath.Concat(endNodePath).ToList();

                return refinedPath;
            }
        }

        public List<Node> FindAbstractPath(Point startPoint, Point endPoint, int level) //This is A*
        {
            var endNode = SetEndNode(endPoint, level);
            var startNode = SetStartNode(startPoint, endNode, level);

            var sortedOpenNodes = new PriorityQueue<float, Node>();

            sortedOpenNodes.Enqueue(startNode, startNode.CostDistance);

            var closedNodes = new Dictionary<Vector2, Node>();
            var openNodes = new Dictionary<Vector2, Node>
            {
                {startNode.DoorPoint.Position, startNode}
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
                            return abstractPath;
                        }
                    }
                }

                closedNodes[currentNode.DoorPoint.Position] = currentNode;
                openNodes.Remove(currentNode.DoorPoint.Position);

                foreach (var node in GetNeighbours(currentNode, endNode))
                {
                    if (closedNodes.ContainsKey(node.DoorPoint.Position)
                    ) //Don't need to recheck node if it's already be looked at
                    {
                        continue;
                    }

                    if (openNodes.TryGetValue(node.DoorPoint.Position, out var existingOpenNode))
                    {
                        if (!(existingOpenNode?.CostDistance > node.CostDistance))
                        {
                            continue;
                        }

                        sortedOpenNodes.Remove(existingOpenNode, existingOpenNode.CostDistance);
                    }

                    sortedOpenNodes.Enqueue(node, node.CostDistance);
                    openNodes[node.DoorPoint.Position] = node;
                }
            }

            return new List<Node>(); //need to be wary of this.
        }

        public List<Point> FindPath(Point startPoint, Point endPoint, int level)
        {
            var abstractPath = FindAbstractPath(startPoint, endPoint, level);
            var path = RefineAbstractPath(abstractPath, endPoint, level);
            return path;
        }

        public Node SetStartNode(Point point, Node target, int level)
        {
            var cluster = GetClusterContainingPoint(point, level);
            AddIntraEdgeInEachValidDirection(point, cluster);
            return new Node(Node.Empty, target, cluster, point, IntraEdge.Empty, 0);
        }

        public Node SetEndNode(Point point, int level) 
        {
            var cluster = GetClusterContainingPoint(point, level);
            return new Node(cluster, point);
        }

        public Point GetAdjacentPoint(Point point, Direction adjacencyDirection)
        {
            switch(adjacencyDirection)
            {
                case Direction.Up:
                    return new Point(point.Position + new Vector2(0, -1));
                case Direction.Down:
                    return new Point(point.Position + new Vector2(0, 1));
                case Direction.Left:
                    return new Point(point.Position + new Vector2(-1, 0));
                case Direction.Right:
                    return new Point(point.Position + new Vector2(1, 0));
                default:
                    return point;
            }
        }

        public IEnumerable<Node> GetNeighbours(Node currentNode, Node targetNode)
        {
            var currentCluster = currentNode.Cluster;
            var currentClusterLevel = currentCluster.Level;
            var adjacentClusters = GetAdjacentClusters(currentCluster);
            foreach (var adjacentCluster in adjacentClusters)
            {

                var intraEdge = currentCluster.GetIntraEdge(currentNode.DoorPoint, adjacentCluster.Key);
                if (intraEdge != IntraEdge.Empty)
                {
                    var neighbourDoorPoint = GetAdjacentPoint(intraEdge.Path[0], intraEdge.AdjacencyDirection);
                    var neighbourCost = currentNode.Cost + intraEdge.Path.Count; //This is 1 more than the actual path length but works for our purposes so we don't need to add inter-edge costs (1)

                    yield return new Node(currentNode, targetNode, adjacentCluster.Value, neighbourDoorPoint, intraEdge, neighbourCost);
                }

                
            }
        }
    }

    public class Cluster : IEquatable<Cluster>
    {
        #region Props

        public Guid Id { get; set; }
        public Rectangle Region { get; private set; }
        public List<ComponentViewModel> Children { get; }

        public List<IntraEdge> IntraEdges { get; set; }
        public List<EdgePoint> DoorPoints { get; set; }
        public List<EdgePoint> EdgePoints { get; set; }
        public int Level { get; }
        public int Row { get; }
        public int Column { get; }

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
            Children = new List<ComponentViewModel>();
            EdgePoints = SetEdgePoints();
            DoorPoints = new();
            IntraEdges = new();
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

        public List<EdgePoint> SetEdgePoints()
        {
            var clusterWidth = Region.Width;
            var clusterHeight = Region.Height;
            var edgePoints = new List<EdgePoint>();
            for (var i = 0; i <= 3; i++)
            {
                var direction = (Direction)i;
                switch (direction)
                {
                    case Direction.Up:
                        for (var j = 0; j <= clusterWidth; j++)
                        {
                            edgePoints.Add(new EdgePoint(direction, Region.TopLeft + new Vector2(j, 0)));
                        }
                        continue;
                    case Direction.Down:
                        for (var j = 0; j <= clusterWidth; j++)
                        {
                            edgePoints.Add(new EdgePoint(direction, Region.BottomLeft + new Vector2(j, 0)));
                        }
                        continue;
                    case Direction.Left:
                        for (var j = 0; j <= clusterHeight; j++)
                        {
                            edgePoints.Add(new EdgePoint(direction, Region.TopLeft + new Vector2(0, j)));
                        }
                        continue;
                    case Direction.Right:
                        for (var j = 0; j <= clusterHeight; j++)
                        {
                            edgePoints.Add(new EdgePoint(direction, Region.TopRight + new Vector2(0, j)));
                        }
                        continue;

                }
            }
            return edgePoints;
        }

        public List<EdgePoint> GetEdgePoints(Direction direction) //Should test to see if linq is faster??
        {
            var edgePointsInGivenDirection = new List<EdgePoint>();
            for(var i = 0; i < EdgePoints.Count; i++)
            {
                if(EdgePoints[i].AdjacencyDirection == direction)
                {
                    edgePointsInGivenDirection.Add(EdgePoints[i]);
                }
            }
            return edgePointsInGivenDirection;
        }

        public bool EdgePointHasCollision(EdgePoint edgePoint) //I don't like that this news up a collision detector every time.
        {
            var collisionDetector = new FineCollisionDetector();

            return Children.Any(c => c.CollidesWith(edgePoint, collisionDetector));
        }

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
        
        public IntraEdge GetIntraEdge(Point startPoint, Direction direction)
        {
            for (var i = 0; i < IntraEdges.Count; i++)
            {
                if (IntraEdges[i].AdjacencyDirection == direction && IntraEdges[i].Path.Last().Position == startPoint.Position) //Need point equality or to move away from using points, probably this one.
                {
                    return IntraEdges[i];
                }
            }
            return IntraEdge.Empty;
        }

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

    public static class DirectionExtensions
    {
        public static Direction Inverse(this Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return Direction.Down;
                case Direction.Down:
                    return Direction.Up;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Left;
                case Direction.None:
                    return Direction.None;
                default:
                    return Direction.None;
            }
        }
    }



    public class IntraEdge
    {
        public List<Point> Path { get; }
        public Direction AdjacencyDirection { get; }

        public IntraEdge(Direction adjacencyDirection, params Point[] points)
        {
            Path = points.ToList(); //Paths are constructed backwards
            AdjacencyDirection = adjacencyDirection;
        }

        public IntraEdge()
        {
            Path = new List<Point>();
            AdjacencyDirection = Direction.None;
        }

        public static IntraEdge Empty = new();

    }

    public record EdgePoint : Point
    {
        public Direction AdjacencyDirection { get; }
        public EdgePoint(Direction adjacencyDirection, Vector2 position) : base(position)
        {
            AdjacencyDirection = adjacencyDirection;
        }
    }

    public record Node
    {
        
        public readonly float Cost;
        public readonly float Distance;
        public readonly float CostDistance;
        public readonly Node Parent;
        public readonly Cluster Cluster;
        public readonly Point DoorPoint;
        public readonly IntraEdge ParentIntraEdge;

        public Node(Node parent, Node target, Cluster cluster, Point doorPoint, IntraEdge parentIntraEdge, float cost)
        {
            Parent = parent;
            Cluster = cluster;
            DoorPoint = doorPoint;
            Cost = cost;
            ParentIntraEdge = parentIntraEdge;
            var distanceVector = Vector2.Abs(target.DoorPoint.Position - DoorPoint.Position);
            Distance = distanceVector.X + distanceVector.Y;
            CostDistance = Cost + Distance;
        }

        public Node(Cluster cluster, Point doorPoint)
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
            DoorPoint = Point.Zero;
            ParentIntraEdge = IntraEdge.Empty;
        }

        public static readonly Node Empty = new();
    }
}