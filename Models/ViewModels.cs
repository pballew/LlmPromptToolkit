using System;
using System.Collections.Generic;

namespace OllamaClient.Models;

public class HomeIndexViewModel
{
    public string Prompt { get; set; } = string.Empty;
    public List<Prompt> AvailablePrompts { get; set; } = new();
    public string? SelectedPromptId { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

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
