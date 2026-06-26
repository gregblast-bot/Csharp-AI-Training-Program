
namespace Beam_Search;

// This is for INFSCI 2440 in Spring 2026.
// Extract graph from assign1_sentences.txt.
// Make sure do not change this code.

internal class Program
{
    public static void Main(string[] args)
    {
        var graph = new ExtractGraph();

        // Test extraction correctness. 
        string headWord = "<s>";
        string tailWord = "Water";
        Console.WriteLine($"The probability of \"{tailWord}\" appearing after \"{headWord}\" is {graph.GetProb(headWord, tailWord)}");

        headWord = "Water";
        tailWord = "<s>";
        Console.WriteLine($"The probability of \"{tailWord}\" appearing after \"{headWord}\" is {graph.GetProb(headWord, tailWord)}");

        headWord = "planned";
        tailWord = "economy";
        Console.WriteLine($"The probability of \"{tailWord}\" appearing after \"{headWord}\" is {graph.GetProb(headWord, tailWord)}");

        headWord = ".";
        tailWord = "</s>";
        Console.WriteLine($"The probability of \"{tailWord}\" appearing after \"{headWord}\" is {graph.GetProb(headWord, tailWord)}");

        Console.WriteLine(new string('*', 50));

        // Find the sentence with highest probability using basic beam search.
        var beamSearch = new BeamSearch(graph);
        var sentenceProb = beamSearch.BeamSearchV1("<s>", 10, 20);
        Console.WriteLine($"{sentenceProb.Score}\t{sentenceProb.String}");

        sentenceProb = beamSearch.BeamSearchV1("<s> Israel and Jordan signed the peace", 10, 40);
        Console.WriteLine($"{sentenceProb.Score}\t{sentenceProb.String}");

        sentenceProb = beamSearch.BeamSearchV1("<s> It is", 10, 15);
        Console.WriteLine($"{sentenceProb.Score}\t{sentenceProb.String}");

        Console.WriteLine(new string('*', 50));

        // Find the sentence with highest probability using beam search with sentence length-normalization.
        double paramLambda = 0.7;
        sentenceProb = beamSearch.BeamSearchV2("<s>", 10, paramLambda, 20);
        Console.WriteLine($"{sentenceProb.Score}\t{sentenceProb.String}");

        sentenceProb = beamSearch.BeamSearchV2("<s> Israel and Jordan signed the peace", 10, paramLambda, 40);
        Console.WriteLine($"{sentenceProb.Score}\t{sentenceProb.String}");

        sentenceProb = beamSearch.BeamSearchV2("<s> It is", 10, paramLambda, 15);
        Console.WriteLine($"{sentenceProb.Score}\t{sentenceProb.String}");
    }
}