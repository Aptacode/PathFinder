using System;
using System.Numerics;
using Aptacode.Geometry;
using Priority_Queue;

namespace Aptacode.PathFinder.Maps.Hpa
{
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
            var isInline = parent.IsInline(position);
            Cost = cost - (isInline ? 0.1f : 0);
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

        public ConcreteNode(Cluster cluster, Vector2 position, Vector2 target)
        {
            Position = position;
            Cost = 0;
            Parent = Empty;
            Cluster = cluster;
            var delta = Vector2.Abs(Position - target);
            Distance = delta.X + delta.Y;

            CostDistance = Cost + Distance;
        }

        public bool IsInline(Vector2 position)
        {
            return Math.Abs(position.X - Position.X) < Constants.Tolerance ||
                   Math.Abs(position.Y - Position.Y) < Constants.Tolerance;
        }

        #region IEquatable

        public virtual bool Equals(ConcreteNode other)
        {
            return other is not null && Position == other.Position;
        }

        #endregion
    }
}