# Ollama C# Console Client

A simple C# console application that sends prompts to an Ollama model via REST API.

## Prerequisites

- .NET 8.0 SDK or later
- [Ollama](https://ollama.ai/) installed and running locally
- Model `llama3.1:8b` pulled in Ollama (`ollama pull llama3.1:8b`)

## Building

```bash
dotnet build
```

## Running

Basic usage with default prompt:
```bash
dotnet run
```

Send a custom prompt:
```bash
dotnet run "What is .NET?"
```

Multi-word prompts:
```bash
dotnet run "Explain the concept of async/await in C#"
```

## Configuration

The application connects to Ollama at `http://localhost:11434` by default. To use a different server, modify the `ollamaUrl` variable in `Program.cs`.

## Model

The application uses the `llama3.1:8b` model. To change the model, modify the `modelName` variable in `Program.cs` or ensure that the desired model is available in your Ollama installation.

## How It Works

1. Takes a prompt as input (from command line or uses default)
2. Sends the prompt to the Ollama API at `/api/generate`
3. Receives and displays the model's response

## Troubleshooting

- **Connection Error**: Make sure Ollama is running (`ollama serve`)
- **Model Not Found**: Pull the required model with `ollama pull llama3.1:8b`
- **Port Mismatch**: Verify Ollama is listening on port 11434
