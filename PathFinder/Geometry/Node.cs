using System;
using System.Numerics;
using Aptacode.PathFinder.Utilities;

namespace Aptacode.PathFinder.Geometry
{
    public record Node
    {
        public static readonly Node Empty = new();
        public readonly float Cost;
        public readonly float CostDistance;
        public readonly float Distance;
        public readonly Node Parent;

        public readonly Vector2 Position;

        public Node(Node parent, Vector2 position, Vector2 target, float cost)
        {
            Parent = parent;
            Position = position;
            Cost = cost;

            var delta = Vector2.Abs(Position - target);

            Distance = delta.X + delta.Y;
            CostDistance = Cost + Distance;
        }

        internal Node(Vector2 position)
        {
            Position = position;
            Cost = float.MaxValue;
            CostDistance = float.MaxValue;
            Distance = 0;
            Parent = Empty;
        }

        internal Node()
        {
            Position = Vector2.Zero;
            Cost = 0;
            CostDistance = 0;
            Distance = 0;
            Parent = Empty;
        }

        internal Node(Vector2 position, Vector2 target)
        {
            Position = position;
            Cost = 0;
            Parent = Empty;

            var delta = Vector2.Abs(Position - target);
            Distance = delta.X + delta.Y;

            CostDistance = Cost + Distance;
        }

        public bool IsInline(Vector2 position)
        {
            return Math.Abs(position.X - Position.X) < Constants.Tolerance ||
                   Math.Abs(position.Y - Position.Y) < Constants.Tolerance;
        }

        public static Node StartNode(Vector2 position, Vector2 target)
        {
            return new(position, target);
        }

        public static Node EndNode(Vector2 position)
        {
            return new(position);
        }

        #region IEquatable

        public override int GetHashCode()
        {
            return (Position, Cost, Distance).GetHashCode();
        }

        public virtual bool Equals(Node other)
        {
            return Position == other?.Position &&
                   Math.Abs(Distance - other.Distance) < Constants.Tolerance &&
                   Math.Abs(Cost - other.Cost) < Constants.Tolerance;
        }

        #endregion
    }
}