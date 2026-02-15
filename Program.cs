using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Default Ollama server URL
        string ollamaUrl = "http://localhost:11434";
        string modelName = "llama3.1:8b";
        string prompt;
        string? secondPrompt = null;
        
        // Get first prompt from file or command line
        if (args.Length > 0 && File.Exists(args[0]))
        {
            try
            {
                prompt = File.ReadAllText(args[0]);
                Console.WriteLine($"Sending prompt from file: {args[0]}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                Environment.Exit(1);
                return;
            }
        }
        else if (args.Length > 0)
        {
            prompt = string.Join(" ", args);
            Console.WriteLine("Sending prompt from command line arguments");
        }
        else
        {
            prompt = "Hello! What is C#?";
            Console.WriteLine("Using default prompt");
        }

        // Check if a second filename is specified
        if (args.Length > 1 && File.Exists(args[1]))
        {
            try
            {
                secondPrompt = File.ReadAllText(args[1]);
                Console.WriteLine($"Second prompt file loaded: {args[1]}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading second file: {ex.Message}");
            }
        }

        Console.WriteLine($"Model: {modelName}");
        Console.WriteLine($"Prompt: {prompt}");
        Console.WriteLine("---");

        try
        {
            var client = new OllamaClient(ollamaUrl);
            var startTime = DateTime.Now;
            var responseContext = await client.GenerateAsync(modelName, prompt, []);
            var endTime = DateTime.Now;
            TimeSpan elapsed = endTime - startTime;
            
            Console.WriteLine("Response Context:");
            Console.WriteLine($"Model: {responseContext.Model}");
            Console.WriteLine($"Created At: {responseContext.CreatedAt}");
            Console.WriteLine($"Done: {responseContext.Done}");
            Console.WriteLine($"Response:\n{responseContext.Response}");
            Console.WriteLine($"---");
            Console.WriteLine($"Request/Response Time: {elapsed.TotalSeconds:F2} seconds");

            // Log first response context
            var logJson = JsonSerializer.Serialize(responseContext, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("response1.json", logJson);
            Console.WriteLine("Response logged to response1.json");

            // If a second prompt exists, send it with context from the first response
            if (!string.IsNullOrEmpty(secondPrompt))
            {
                Console.WriteLine("\n=== SECOND REQUEST ===");
                string secondPromptWithContext = $"Previous context:\n{responseContext.Response}\n\n---\n\n{secondPrompt}";
                Console.WriteLine($"Second Prompt: {secondPrompt}");
                Console.WriteLine("---");

                var startTime2 = DateTime.Now;
                var responseContext2 = await client.GenerateAsync(modelName, secondPromptWithContext, responseContext.Context);
                var endTime2 = DateTime.Now;
                TimeSpan elapsed2 = endTime2 - startTime2;

                Console.WriteLine("Second Response Context:");
                Console.WriteLine($"Model: {responseContext2.Model}");
                Console.WriteLine($"Created At: {responseContext2.CreatedAt}");
                Console.WriteLine($"Done: {responseContext2.Done}");
                Console.WriteLine($"Response:\n{responseContext2.Response}");
                Console.WriteLine($"---");
                Console.WriteLine($"Request/Response Time: {elapsed2.TotalSeconds:F2} seconds");

                // Log second response context
                var logJson2 = JsonSerializer.Serialize(responseContext2, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("response2.json", logJson2);
                Console.WriteLine("Response logged to response2.json");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
