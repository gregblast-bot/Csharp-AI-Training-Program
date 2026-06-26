
namespace Beam_Search;

public class ExtractGraph
{
    private readonly DirectedGraph _dg = new();
    public Dictionary<string, Dictionary<string, int>> Graph => _dg.Graph;
    private const string SentencesPath = "data/assign1_sentences.txt";

    public ExtractGraph()
    {
        Extract();
        Console.WriteLine(new string('*', 100));
        Console.WriteLine("Finished!");
    }

    private void Extract()
    {
        if (!File.Exists(SentencesPath))
        {
            Console.WriteLine($"Warning: Sentence file not found at {SentencesPath}");
            return;
        }

        // Memory efficient line-by-line reading
        foreach (var line in File.ReadLines(SentencesPath))
        {
            var nodes = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            BuildGraph(nodes);
        }
    }

    private void BuildGraph(string[] nodes)
    {
        for (int i = 0; i < nodes.Length - 1; i++)
        {
            _dg.AddEdge(nodes[i], nodes[i + 1]);
        }
    }

    /// <summary>
    /// Gets the transition probability from a head word to a tail word.
    /// </summary>
    public double GetProb(string headWord, string tailWord)
    {
        if (!Graph.TryGetValue(headWord, out var tails))
        {
            return 0.0;
        }

        double totalOccurrences = tails.Values.Sum();
        if (totalOccurrences == 0)
        {
            return 0.0;
        }

        int occurrenceOfTailWord = tails.GetValueOrDefault(tailWord, 0);
        return occurrenceOfTailWord / totalOccurrences;
    }

    public Dictionary<string, int> GetTails(string headWord)
    {
        return Graph[headWord];
    }
}