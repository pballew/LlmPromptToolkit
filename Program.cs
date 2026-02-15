using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Default Ollama server URL
        string ollamaUrl = "http://localhost:11434";
        string modelName = "llama3.1:8b";
        string prompt;
        PromptService _promptService = new PromptService();

        var firstFile = args.Length > 0 ? args[0] : null;
        var secondFile = args.Length > 1 ? args[1] : null;

        if (firstFile == null)
        {
            Console.WriteLine("No prompt file specified.");
            return;
        }

        prompt = await _promptService.LoadPromptAsync(firstFile);

        var client = new OllamaService(ollamaUrl, modelName);
        LlmResponse responseContext = await client.GetLlmResponseAsync(prompt);

        if (!string.IsNullOrWhiteSpace(secondFile))
        {
            prompt = await _promptService.LoadPromptAsync(secondFile);
            responseContext = await client.GetLlmResponseAsync(prompt, responseContext.Context);
            responseContext.Print();
        }
    }
}
