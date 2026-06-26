
namespace Beam_Search;

public class DirectedGraph
{
    // Uses C# collection expressions ([]) to initialize an empty dictionary
    public Dictionary<string, Dictionary<string, int>> Graph { get; } = [];
    public string? CurrentHead { get; private set; }
    public string? CurrentTail { get; private set; }

    /// <summary>
    /// Add a directed edge from head to tail and increment transition frequency count.
    /// </summary>
    public void AddEdge(string head, string tail)
    {
        CurrentHead = head;
        CurrentTail = tail;

        if (!Graph.ContainsKey(head))
        {
            Graph[head] = [];
        }

        // Safely increments or initializes the path frequency counter
        Graph[head][tail] = Graph[head].GetValueOrDefault(tail, 0) + 1;
    }
}