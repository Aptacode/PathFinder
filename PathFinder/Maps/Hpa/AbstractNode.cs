using System.Numerics;
using Priority_Queue;

namespace Aptacode.PathFinder.Maps.Hpa;

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

    public AbstractNode(AbstractNode parent, AbstractNode target, Cluster cluster, Vector2 doorPoint,
        IntraEdge parentIntraEdge, float cost)
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