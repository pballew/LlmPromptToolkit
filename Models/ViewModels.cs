using System;

namespace OllamaClient.Models;

public class RequestResponseViewModel
{
    public string Prompt { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int TimingMs { get; set; }
    public int TokenCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
