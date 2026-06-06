
namespace Classical_Pathfinding;

/// <summary>
/// 
/// </summary>
/// <param name="environment"></param>
internal class Pathfinder(Node[,] environment)
{
    private readonly int _width = environment.GetLength(0);
    private readonly int _height = environment.GetLength(1);
    private static readonly (int dx, int dy)[] Directions = [(0, 1), (1, 0), (0, -1), (-1, 0)];

    public List<Node> FindPath(Node start, Node goal)
    {
        // Low-allocation data structures.
        PriorityQueue<Node, int> openSet = new();
        HashSet<Node> closedSet = [];

        // Reset all nodes in the local environment to ensure a fresh path calculation.
        foreach (Node node in environment)
        {
            node.Parent = null;
            node.GCost = int.MaxValue;
        }

        start.GCost = 0;
        start.HCost = getManhattanDistance(start, goal);
        openSet.Enqueue(start, start.FCost);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.Dequeue();
            closedSet.Add(currentNode);

            if (currentNode == goal)
            {
                return retracePath(start, goal);
            }

            foreach (Node neighbor in getNeighbors(currentNode))
            {
                if (!neighbor.IsWalkable || closedSet.Contains(neighbor))
                    continue;

                int tentativeGCost = currentNode.GCost + 1;

                if (tentativeGCost < neighbor.GCost)
                {
                    neighbor.Parent = currentNode;
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = getManhattanDistance(neighbor, goal);

                    openSet.Enqueue(neighbor, neighbor.FCost);
                }
            }
        }

        return [];
    }

    private int getManhattanDistance(Node a, Node b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

    private IEnumerable<Node> getNeighbors(Node node)
    {
        foreach ((int dx, int dy) in Directions)
        {
            int checkX = node.X + dx;
            int checkY = node.Y + dy;

            if (checkX >= 0 && checkX < _width && checkY >= 0 && checkY < _height)
            {
                yield return environment[checkX, checkY];
            }
        }
    }

    private List<Node> retracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node current = endNode;

        while (current != startNode && current is not null)
        {
            path.Add(current);
            current = current.Parent;
        }

        path.Reverse();
        return path;
    }
}
