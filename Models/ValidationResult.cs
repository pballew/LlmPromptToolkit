using System;
using System.Collections.Generic;

/// <summary>
/// Result of validating a JSON response
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, bool> RequiredFieldsStatus { get; set; } = new();
    public string ParseError { get; set; } = string.Empty;
    public int TotalRequiredFields { get; set; }
    public int PopulatedRequiredFields { get; set; }

    public bool AllRequiredFieldsPopulated => PopulatedRequiredFields == TotalRequiredFields;
}
