using System;
using System.Collections.Generic;
using System.Linq;

namespace Probabilistic_Classifier;

public static class FeatureExtractor
{
    public static List<string> ExtractTokens(double[] window)
    {
        List<string> tokens = [];
        if (window.Length < 2) return tokens;

        // 1. Basic Stats
        double mean = window.Average();
        double variance = window.Select(v => Math.Pow(v - mean, 2)).Average();
        double stdDev = Math.Sqrt(variance);
        double max = window.Max();

        // 2. Dynamics (Burstiness)
        double maxDelta = 0;
        for (int i = 1; i < window.Length; i++)
        {
            double delta = Math.Abs(window[i] - window[i - 1]);
            if (delta > maxDelta) maxDelta = delta;
        }

        // 3. Shape (Trend via simple linear regression slope)
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
        int n = window.Length;
        for (int i = 0; i < n; i++)
        {
            sumX += i;
            sumY += window[i];
            sumXY += i * window[i];
            sumX2 += i * i;
        }
        double slope = ((n * sumXY) - (sumX * sumY)) / ((n * sumX2) - (sumX * sumX));

        // 4. Instability (Z-score of the final point)
        double zScoreLast = (window[^1] - mean) / (stdDev + 1e-6);

        // --- QUANTIZATION: Convert metrics into text tokens ---

        // Volatility Token
        if (stdDev > mean * 0.5) tokens.Add("Volatility_High");
        else if (stdDev > mean * 0.2) tokens.Add("Volatility_Medium");
        else tokens.Add("Volatility_Low");

        // Trend Token
        if (slope > 0.5) tokens.Add("Trend_Up_Sharp");
        else if (slope > 0.1) tokens.Add("Trend_Up_Steady");
        else if (slope < -0.1) tokens.Add("Trend_Down");
        else tokens.Add("Trend_Flat");

        // Burstiness Token
        if (maxDelta > stdDev * 2) tokens.Add("Burstiness_Spike");

        // Instability Token
        if (zScoreLast > 2.0) tokens.Add("LatestPoint_AnomalousHigh");
        else if (zScoreLast < -2.0) tokens.Add("LatestPoint_AnomalousLow");

        return tokens;
    }
}