using System;
using System.Collections.Generic;
using System.Linq;

namespace Probabilistic_Classifier;

public record LogDocument(List<string> Tokens, string Category);
public record TokenInfluence(string Token, double LogProbabilityContribution);

public record AuditReport(
    string ChosenCategory,
    double PriorLogProbability,
    List<TokenInfluence> TopInfluences
);

public record PredictionResult(string Category, double Probability, AuditReport Audit);

public class NaiveBayesClassifier
{
    private readonly HashSet<string> _vocabulary = [];
    private readonly Dictionary<string, List<string[]>> _tokenizedDocsByCategory = [];
    private readonly Dictionary<string, int> _totalWordsByCategory = [];
    private readonly Dictionary<string, int> _docCountByCategory = [];
    private int _totalDocumentCount;

    public void Train(IEnumerable<LogDocument> trainingSet)
    {
        foreach (var doc in trainingSet)
        {
            var tokens = doc.Tokens.ToArray();
            if (tokens.Length == 0) continue;

            _totalDocumentCount++;

            if (!_tokenizedDocsByCategory.TryGetValue(doc.Category, out var docList))
            {
                docList = [];
                _tokenizedDocsByCategory[doc.Category] = docList;
                _totalWordsByCategory[doc.Category] = 0;
                _docCountByCategory[doc.Category] = 0;
            }

            docList.Add(tokens);
            _docCountByCategory[doc.Category]++;
            _totalWordsByCategory[doc.Category] += tokens.Length;

            foreach (var token in tokens)
            {
                _vocabulary.Add(token);
            }
        }
    }

    public PredictionResult Predict(List<string> tokens)
    {
        if (_totalDocumentCount == 0 || tokens.Count == 0)
        {
            return new PredictionResult("UNKNOWN", 0.0, new AuditReport("UNKNOWN", 0, []));
        }

        string bestCategory = "UNKNOWN";
        double maxLogLikelihood = double.NegativeInfinity;

        var scores = new Dictionary<string, double>();
        // Temporary storage to track token details for every category during evaluation
        var categoryAuditDetails = new Dictionary<string, (double PriorLog, List<TokenInfluence> Influences)>();

        foreach (var category in _tokenizedDocsByCategory.Keys)
        {
            double prior = (double)_docCountByCategory[category] / _totalDocumentCount;
            double priorLog = Math.Log(prior);
            double logLikelihood = priorLog;

            int totalWordsInCat = _totalWordsByCategory[category];
            int vocabSize = _vocabulary.Count;
            var influences = new List<TokenInfluence>();

            foreach (var token in tokens)
            {
                int wordCountInCat = _tokenizedDocsByCategory[category]
                    .Sum(docTokens => docTokens.Count(t => t == token));

                // Laplace Smoothing formula calculation
                double wordProbability = (double)(wordCountInCat + 1) / (totalWordsInCat + vocabSize);
                double logContrib = Math.Log(wordProbability);

                logLikelihood += logContrib;

                // Track how much this specific token impacted this specific category
                influences.Add(new TokenInfluence(token, logContrib));
            }

            scores[category] = logLikelihood;
            categoryAuditDetails[category] = (priorLog, influences);

            if (logLikelihood > maxLogLikelihood)
            {
                maxLogLikelihood = logLikelihood;
                bestCategory = category;
            }
        }

        double totalExp = scores.Values.Sum(v => Math.Exp(v - maxLogLikelihood));
        double confidence = 1.0 / totalExp;

        // Pull the audit details specifically for the winning category
        var winningDetails = categoryAuditDetails[bestCategory];

        // Sort influences so the most mathematically "positive" or least negative tokens are at the top
        var sortedInfluences = winningDetails.Influences
            .OrderByDescending(i => i.LogProbabilityContribution)
            .ToList();

        var auditReport = new AuditReport(bestCategory, winningDetails.PriorLog, sortedInfluences);

        return new PredictionResult(bestCategory, confidence, auditReport);
    }
}