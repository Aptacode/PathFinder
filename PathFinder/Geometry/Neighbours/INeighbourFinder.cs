using System.Collections.Generic;
using Aptacode.PathFinder.Maps;

namespace Aptacode.PathFinder.Geometry.Neighbours
{
    public interface INeighbourFinder
    {
        IEnumerable<Node> GetNeighbours(Map map, Node currentNode, Node targetNode);
    }
}