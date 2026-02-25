/// <summary>
/// Service to track and persist the currently selected model across requests
/// </summary>
public class ModelSelectionService
{
    private string _selectedModelType = "cloud"; // "local" or "cloud"

    public string GetSelectedModelType() => _selectedModelType;

    public void SetSelectedModelType(string modelType)
    {
        if (modelType == "local" || modelType == "cloud")
        {
            _selectedModelType = modelType;
        }
    }
}
