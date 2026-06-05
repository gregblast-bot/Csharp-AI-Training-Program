
namespace Classical_Pathfinding;

/// <summary>
/// Represents a cell in a two-dimensional grid for pathfinding, storing coordinates, traversal state, a parent link,
/// and A* cost values.
/// </summary>
/// <remarks>GCost defaults to int.MaxValue to indicate an unset cost.</remarks>
/// <param name="x">Initial x-coordinate (horizontal) of the node.</param>
/// <param name="y">Initial y-coordinate (vertical) of the node.</param>
/// <param name="isWalkable">State to track if the node can be traversed; defaults to true.</param>
internal class Node(int x, int y, bool isWalkable = true)
{
    /// <summary>
    /// The horizontal position of the node, read-only.
    /// </summary>
    public int X { get; } = x;
    /// <summary>
    /// The vertical position of the node, read-only.
    /// </summary>
    public int Y { get; } = y;
    /// <summary>
    /// Is the path not blocked by an obstacle?
    /// </summary>
    public bool IsWalkable { get; set; } = isWalkable;
    /// <summary>
    /// The parent node to track traversal history.
    /// </summary>
    public Node? Parent { get; set; }
    /// <summary>
    /// The actual cost of the path.
    /// </summary>
    public int GCost { get; set; } = int.MaxValue;
    /// <summary>
    /// A heuristic to help estimate the cost to reach the goal, calculated as the Manhattan distance.
    /// </summary>
    public int HCost { get; set; }
    /// <summary>
    /// The total cost of the path.
    /// </summary>
    public int FCost => GCost == int.MaxValue ? int.MaxValue : GCost + HCost;
}
