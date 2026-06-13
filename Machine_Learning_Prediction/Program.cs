using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Data;

namespace AIModule3.MachineLearning;

// 1. Define distinct data structures for input and output schemas
public class ServerMetrics
{
    [LoadColumn(0)] public float CpuUtilization { get; set; }
    [LoadColumn(1)] public float MemoryUsedGB { get; set; }
    [LoadColumn(2)] public float NetworkTrafficMbS { get; set; }
    [LoadColumn(3)] public float ActiveThreadCount { get; set; }
    [LoadColumn(4)] public bool IsBottlenecked { get; set; } // The target label we want to predict
}

public class BottleneckPrediction
{
    [ColumnName("PredictedLabel")] public bool IsBottlenecked { get; set; }
    [ColumnName("Probability")] public float Probability { get; set; }
    [ColumnName("Score")] public float Score { get; set; }
}

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Module 3: Predictive Infrastructure Bottleneck Engine (ML.NET) ===");

        // Initialize the ML Context (the gateway to the ML.NET framework)
        var mlContext = new MLContext(seed: 42);

        // 2. Synthesize or load historical training data
        List<ServerMetrics> historicalData = [
            new() { CpuUtilization = 12.5f, MemoryUsedGB = 4.2f,  NetworkTrafficMbS = 15.0f,  ActiveThreadCount = 22f,  IsBottlenecked = false },
            new() { CpuUtilization = 95.0f, MemoryUsedGB = 15.8f, NetworkTrafficMbS = 450.5f, ActiveThreadCount = 450f, IsBottlenecked = true },
            new() { CpuUtilization = 45.2f, MemoryUsedGB = 8.0f,  NetworkTrafficMbS = 85.1f,  ActiveThreadCount = 64f,  IsBottlenecked = false },
            new() { CpuUtilization = 88.7f, MemoryUsedGB = 14.1f, NetworkTrafficMbS = 310.0f, ActiveThreadCount = 380f, IsBottlenecked = true },
            new() { CpuUtilization = 22.1f, MemoryUsedGB = 5.5f,  NetworkTrafficMbS = 40.2f,  ActiveThreadCount = 30f,  IsBottlenecked = false },
            new() { CpuUtilization = 91.3f, MemoryUsedGB = 15.1f, NetworkTrafficMbS = 120.0f, ActiveThreadCount = 410f, IsBottlenecked = true }
        ];

        // Load data into an IDataView (lazy-loaded data pipeline)
        IDataView trainingDataView = mlContext.Data.LoadFromEnumerable(historicalData);

        // 3. Construct the Data Processing and Model Training Pipeline
        // Step A: Combine all individual numerical features into a single array column named "Features"
        var dataProcessPipeline = mlContext.Transforms.Concatenate(
            "Features",
            nameof(ServerMetrics.CpuUtilization),
            nameof(ServerMetrics.MemoryUsedGB),
            nameof(ServerMetrics.NetworkTrafficMbS),
            nameof(ServerMetrics.ActiveThreadCount));

        // Step B: Append a Binary Classification Trainer (using FastTree / Stochastic Dual Coordinate Ascent)
        var trainer = mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
            labelColumnName: nameof(ServerMetrics.IsBottlenecked),
            featureColumnName: "Features");

        var trainingPipeline = dataProcessPipeline.Append(trainer);

        // 4. Train the Model
        Console.WriteLine("Training statistical logistic regression model via ML.NET pipeline...");
        var trainedModel = trainingPipeline.Fit(trainingDataView);
        Console.WriteLine("Model pipeline training completed successfully.\n");

        // 5. Create a Prediction Engine for optimized single-instance inference
        var predictionEngine = mlContext.Model.CreatePredictionEngine<ServerMetrics, BottleneckPrediction>(trainedModel);

        // 6. Predict live, unseen system telemetry data
        List<ServerMetrics> liveTelemetry = [
            new() { CpuUtilization = 15.0f, MemoryUsedGB = 3.9f,  NetworkTrafficMbS = 20.0f,  ActiveThreadCount = 18f },
            new() { CpuUtilization = 89.5f, MemoryUsedGB = 14.9f, NetworkTrafficMbS = 280.0f, ActiveThreadCount = 395f }
        ];

        Console.WriteLine("=== Live Inference Evaluation ===");
        foreach (var metrics in liveTelemetry)
        {
            var prediction = predictionEngine.Predict(metrics);

            Console.WriteLine($"Telemetry -> CPU: {metrics.CpuUtilization}%, RAM: {metrics.MemoryUsedGB}GB, Threads: {metrics.ActiveThreadCount}");
            Console.WriteLine($"--> Alert Level: {(prediction.IsBottlenecked ? "**CRITICAL BOTTLENECK DETECTED**" : "Healthy System Cluster")}");
            Console.WriteLine($"--> Computed Crash Probability: {prediction.Probability * 100:F2}%\n");
        }
    }
}