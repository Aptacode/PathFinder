using System.Collections.Generic;

namespace Aptacode.PathFinder.Geometry.Neighbours
{
    public interface INeighbourFinder
    {
        IEnumerable<Node> GetNeighbours(Map map, Node currentNode, Node targetNode);
    }
}