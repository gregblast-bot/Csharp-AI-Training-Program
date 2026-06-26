
namespace Beam_Search;

// Primary Constructor notation introduced in modern C#
public class BeamSearch(ExtractGraph inputGraph)
{
    private readonly ExtractGraph _graph = inputGraph;

    public StringDouble BeamSearchV1(string preWords, int beamK, int maxToken)
    {
        return BeamSearchV2(preWords, beamK, 0, maxToken);
    }

    public StringDouble BeamSearchV2(string preWords, int beamK, double paramLambda, int maxToken)
    {
        var tokens = preWords.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        double initialProbability = 0.0;

        // Tuple mapping mimicking the Python nested array approach
        var sentences = new List<(List<string> Tokens, double Probability)> { (tokens, initialProbability) };

        while (true)
        {
            var possibleSentences = new List<(List<string> Tokens, double Probability)>();
            bool canExpand = false;

            foreach (var (currentTokens, probability) in sentences)
            {
                // C# Index-from-end operator (^1) targets the final element
                string headWord = currentTokens[^1];

                if (currentTokens.Count >= maxToken || headWord == "</s>")
                {
                    possibleSentences.Add((currentTokens, probability));
                    continue;
                }

                if (_graph.Graph.ContainsKey(headWord))
                {
                    var tails = _graph.GetTails(headWord);
                    if (tails is { Count: > 0 })
                    {
                        canExpand = true;
                        foreach (var tailWord in tails.Keys)
                        {
                            double prob = _graph.GetProb(headWord, tailWord);
                            if (prob > 0)
                            {
                                double newProbability = probability + Math.Log(prob);
                                var newTokens = new List<string>(currentTokens) { tailWord };
                                possibleSentences.Add((newTokens, newProbability));
                            }
                        }
                    }
                    else
                    {
                        possibleSentences.Add((currentTokens, probability));
                    }
                }
                else
                {
                    possibleSentences.Add((currentTokens, probability));
                }
            }

            if (!canExpand)
            {
                break;
            }

            // Slice out the highest probability paths using LINQ evaluation
            sentences = possibleSentences
                .OrderByDescending(x => LengthNormalization(x.Tokens, x.Probability, paramLambda))
                .Take(beamK)
                .ToList();
        }

        var bestPath = sentences[0];
        string sentence = string.Join(" ", bestPath.Tokens);
        double score = LengthNormalization(bestPath.Tokens, bestPath.Probability, paramLambda);

        return new StringDouble(sentence, score);
    }

    private static double LengthNormalization(List<string> tokens, double probability, double paramLambda)
    {
        double lengthPenalty = Math.Pow(tokens.Count, paramLambda);
        return probability / lengthPenalty;
    }
}