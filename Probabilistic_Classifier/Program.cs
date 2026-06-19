using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Probabilistic_Classifier;

public static class Program
{
    private static readonly Dictionary<string, string> ClassMap = new()
    {
        { "NORMAL", "data/artificialNoAnomaly/" },
        { "CPU", "data/realAWSCloudwatch/ec2_cpu" },
        { "NETWORK", "data/realAWSCloudwatch/ec2_network" },
        { "DISK", "data/realAWSCloudwatch/rds_cpu" }
    };

    public static void Main()
    {
        Console.WriteLine("=== C# Naive Bayes NAB Incident Pipeline ===");

        // Get the directory where the application executable is running
        string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Walk backwards to find the actual project root where your 'data' folder sits
        // This moves up past bin/Debug/net8.0 to find the root folder
        string projectRoot = Path.GetFullPath(Path.Combine(exeDirectory, "..", "..", ".."));

        // If you are running a published build or a different structure, 
        // you can fallback to checking if 'data' exists locally:
        if (!Directory.Exists(Path.Combine(projectRoot, "data")))
        {
            projectRoot = Directory.GetCurrentDirectory(); // fallback to current working directory
        }

        Console.WriteLine($"Looking for data directory at: {Path.GetFullPath(Path.Combine(projectRoot, "data"))}");

        // Ingest Data using the fully resolved absolute path
        var trainingData = LoadDatasetPipeline(projectRoot, windowSize: 12);

        if (!trainingData.Any())
        {
            Console.WriteLine("[Error] No training data found. Verify that the 'data' folder is in your root project directory.");
            return;
        }

        // Model training
        var model = new NaiveBayesClassifier();
        model.Train(trainingData);

        // Live Production Simulation Test Case
        double[] liveIncidentWindow = [94.2, 95.1, 93.8, 96.4, 98.2, 99.1, 99.8, 100.0, 99.4, 102.1, 105.3, 108.5];
        List<string> liveTokens = FeatureExtractor.ExtractTokens(liveIncidentWindow);

        // Run prediction (which now generates an audit)
        var result = model.Predict(liveTokens);

        // --- RENDER AUDIT REPORT ---
        Console.WriteLine("\n==================================================");
        Console.WriteLine("                INCIDENT AUDIT REPORT             ");
        Console.WriteLine("==================================================");
        Console.WriteLine($"Final Diagnosis : **{result.Category}**");
        Console.WriteLine($"Model Confidence: {result.Probability * 100:F1}%");
        Console.WriteLine($"Category Prior Log Probability: {result.Audit.PriorLogProbability:F4}");
        Console.WriteLine("--------------------------------------------------");
        Console.WriteLine("Token Influence Breakdown (Higher/Less Negative = Strongest Drivers):");

        foreach (var influence in result.Audit.TopInfluences)
        {
            // Highlight tokens that provided the strongest statistical weight
            string driverTag = influence.LogProbabilityContribution > -3.0 ? "[Strong Driver]" : "[Weak/Common]";
            Console.WriteLine($"  -> Token: {influence.Token,-30} | Weight Score: {influence.LogProbabilityContribution:F4} {driverTag}");
        }
        Console.WriteLine("==================================================");
    }
    

    private static List<LogDocument> LoadDatasetPipeline(string basePath, int windowSize)
    {
        List<LogDocument> dataset = [];

        foreach (var mapping in ClassMap)
        {
            // 1. Separate the hardcoded map path into its directory and prefix components
            string fullMapPath = mapping.Value; // e.g., "data/realAWSCloudwatch/ec2_cpu"

            string folderPath = Path.Combine(basePath, Path.GetDirectoryName(fullMapPath));
            string filePrefix = Path.GetFileName(fullMapPath); // e.g., "ec2_cpu"

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine($"[Warning] Base directory not found: {folderPath}");
                continue;
            }

            // 2. Search using the prefix as a search pattern (e.g., "ec2_cpu*.csv")
            var files = Directory.GetFiles(folderPath, $"{filePrefix}*.csv");

            foreach (var file in files)
            {
                var metrics = File.ReadLines(file)
                                  .Skip(1)
                                  .Select(line => line.Split(','))
                                  .Where(parts => parts.Length > 1 && double.TryParse(parts[1], out _))
                                  .Select(parts => double.Parse(parts[1]))
                                  .ToArray();

                for (int i = 0; i <= metrics.Length - windowSize; i++)
                {
                    double[] window = metrics.Skip(i).Take(windowSize).ToArray();
                    string label = mapping.Key;

                    if (label == "CPU" && window[^1] < 50.0)
                    {
                        label = "NORMAL";
                    }

                    List<string> tokens = FeatureExtractor.ExtractTokens(window);
                    if (tokens.Count > 0)
                    {
                        dataset.Add(new LogDocument(tokens, label));
                    }
                }
            }
        }
        return dataset;
    }
}