using System;
using System.Numerics;
using Aptacode.PathFinder.Geometry.Neighbours;
using Aptacode.PathFinder.Utilities;

namespace Aptacode.PathFinder.Geometry
{
    public class Node : IEquatable<Node>
    {
        public static readonly Node Empty = new Node();
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

        public static Node StartNode(Vector2 position, Vector2 target) => new Node(position, target);
        public static Node EndNode(Vector2 position) => new Node(position);

        #region IEquatable

        public override int GetHashCode() => (Position, Cost, Distance).GetHashCode();

        public override bool Equals(object obj) => obj is Node pattern && Equals(pattern);

        public bool Equals(Node other) => this == other;

        public static bool operator ==(Node lhs, Node rhs) =>
            lhs?.Position == rhs?.Position && Math.Abs(lhs.Distance - rhs.Distance) < Constants.Tolerance && Math.Abs(lhs.Cost - rhs.Cost) < Constants.Tolerance;

        public static bool operator !=(Node lhs, Node rhs) => !(lhs == rhs);

        #endregion
    }
}