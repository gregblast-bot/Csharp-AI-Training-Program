using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AIModule2.Probability;

// Clean domain structures using modern C# records
public record LogDocument(string Text, string Category);
public record PredictionResult(string Category, double Probability);

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
            var tokens = Tokenize(doc.Text);
            if (tokens.Length == 0) continue;

            _totalDocumentCount++;

            // Modern C# dictionary pattern matching and collection expressions
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

    public PredictionResult Predict(string rawText)
    {
        var tokens = Tokenize(rawText);
        if (_totalDocumentCount == 0 || tokens.Length == 0)
        {
            return new PredictionResult("Unknown", 0.0);
        }

        string bestCategory = "Unknown";
        double maxLogLikelihood = double.NegativeInfinity;
        var scores = new Dictionary<string, double>();

        // Calculate probability for each known category
        foreach (var category in _tokenizedDocsByCategory.Keys)
        {
            // Prior Probability: P(Category)
            double prior = (double)_docCountByCategory[category] / _totalDocumentCount;

            // Using Log Probabilities to completely prevent underflow errors from multiplying small decimals
            double logLikelihood = Math.Log(prior);

            // Likelihood: P(Word | Category) using Laplace Smoothing (+1) to handle unknown words
            int totalWordsInCat = _totalWordsByCategory[category];
            int vocabSize = _vocabulary.Count;

            foreach (var token in tokens)
            {
                int wordCountInCat = _tokenizedDocsByCategory[category]
                    .Sum(docTokens => docTokens.Count(t => t == token));

                // Formula: P(w|c) = (count(w,c) + 1) / (total_words_in_c + vocabulary_size)
                double wordProbability = (double)(wordCountInCat + 1) / (totalWordsInCat + vocabSize);
                logLikelihood += Math.Log(wordProbability);
            }

            scores[category] = logLikelihood;

            if (logLikelihood > maxLogLikelihood)
            {
                maxLogLikelihood = logLikelihood;
                bestCategory = category;
            }
        }

        // Convert the winning log-likelihood score back to a scannable percentage absolute value
        double totalExp = scores.Values.Sum(v => Math.Exp(v - maxLogLikelihood));
        double confidence = 1.0 / totalExp;

        return new PredictionResult(bestCategory, confidence);
    }

    private static string[] Tokenize(string text) =>
        Regex.Replace(text.ToLower(), @"[^\w\s]", "")
             .Split(' ', StringSplitOptions.RemoveEmptyEntries);
}

// Verification runner
public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Module 2: Probabilistic Log Triage Classifier ===");

        // 1. Generate historical training data 
        List<LogDocument> trainingData = [
            new("Out of memory error encountered on instance microservice thread pool", "Hardware"),
            new("High CPU utilization spike detected on physical host machine cluster", "Hardware"),
            new("Disk space full critical warning partition dev sda1 exhausted", "Hardware"),
            new("Null reference exception thrown in user auth service controller index", "Software"),
            new("Index out of bounds array validation failed during payload parsing", "Software"),
            new("Database unique constraint violation duplicate key on email field", "Software"),
            new("Connection timeout dropped packet failure communicating with gateway API", "Network"),
            new("DNS resolution failed unable to resolve external auth endpoints", "Network"),
            new("TCP handshake dropped packet connection reset by peer socket error", "Network")
        ];

        // 2. Train our model
        var model = new NaiveBayesClassifier();
        model.Train(trainingData);
        Console.WriteLine("Model training successfully completed over historical datasets.\n");

        // 3. Test with ambiguous, unseen log statements
        string[] testLogs = [
            "Critical exception! Memory pool exhausted on index allocation worker thread.",
            "Failed connection handshake dropped packets on index database node API.",
            "Null pointer payload string parsing error."
        ];

        foreach (var log in testLogs)
        {
            var prediction = model.Predict(log);
            Console.WriteLine($"Log: \"{log}\"");
            Console.WriteLine($"--> Predicted Category: **{prediction.Category}** ({prediction.Probability * 100:F1}% confidence)\n");
        }
    }
}