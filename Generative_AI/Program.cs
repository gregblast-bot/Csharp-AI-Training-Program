using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AIModule5.LocalGenerativeAI;

public class DevOpsAutomationPlugin
{
    [KernelFunction, Description("Looks up which downstream microservices or databases depend on a given service name.")]
    public string CheckServiceDependencies(
        [Description("The exact name of the microservice (e.g., AuthService, PaymentAPI).")] string serviceName)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n[LOCAL WORKFLOW] C# Triggered: Fetching dependencies for {serviceName}...");
        Console.ResetColor();

        return serviceName.ToLower() switch
        {
            "authservice" => "Dependencies: PostgreSQL-UserDB, RedisCache-SessionCluster.",
            "paymentapi" => "Dependencies: RabbitMQ-PaymentQueue, ExternalStripeGateway.",
            _ => $"No known recorded architecture dependencies for {serviceName}."
        };
    }

    [KernelFunction, Description("Executes a safe, isolated soft reboot command on a target service or cluster partition.")]
    public bool ExecuteClusterSoftReboot(
        [Description("The cluster node name or target service name to reboot.")] string target)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n[LOCAL WORKFLOW] C# Triggered: CRITICAL ACTION executed -> Soft-rebooting {target}...");
        Console.ResetColor();
        return true;
    }
}

public static class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== Module 5: 100% Offline Local AI DevOps Agent ===");

        var builder = Kernel.CreateBuilder();

        // Fix: Use Microsoft's official OpenAI connector but target your local Ollama port.
        // Ollama automatically acts as an OpenAI endpoint on /v1 when you pass an absolute URI.
        // No API key is required, so pass an empty or arbitrary string value.
        builder.AddOpenAIChatCompletion(
            modelId: "llama3.2",
            apiKey: "local-airgapped-mode",
            endpoint: new Uri("http://localhost:11434/v1") // Notice the trailing /v1 for compliance
        );

        // Register our DevOps tools plugin
        builder.Plugins.AddFromType<DevOpsAutomationPlugin>("DevOpsTools");

        Kernel kernel = builder.Build();

        // This will now successfully resolve without a NullReferenceException
        var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

        // Enable automatic tool calling behavior supported natively by Llama 3
        var executionSettings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        ChatHistory chatHistory = [];
        chatHistory.AddSystemMessage("You are an advanced DevOps Site Reliability Engineering assistant. You have live access to infrastructure diagnostic and restart tools. Be concise, safe, and state exactly which actions you take.");

        string engineerIncidentReport = "We just got an alert: 'AuthService connection pool exhausted.' Is there anything dependent on it? If so, perform a soft reboot on its target dependencies to clear the stale threads.";

        chatHistory.AddUserMessage(engineerIncidentReport);
        Console.WriteLine($"\nEngineer: {engineerIncidentReport}");

        Console.WriteLine("\n[Local model processing text patterns entirely via localhost loopback...]");

        // Execute the pipeline completely isolated from the internet
        var agentResponse = await chatCompletion.GetChatMessageContentAsync(chatHistory, executionSettings, kernel);

        Console.WriteLine($"\nAgent: {agentResponse.Content}");
    }
}