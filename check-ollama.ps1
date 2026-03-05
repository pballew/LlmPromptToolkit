# Check connectivity to the online Ollama API
$apiUrl = "https://ollama.com/api"
$modelName = "llama3.1:8b"

Write-Host "Checking connectivity to Ollama API at $apiUrl..." -ForegroundColor Cyan

try {
    $response = Invoke-WebRequest -Uri "$apiUrl/tags" -Method Get -TimeoutSec 10 -ErrorAction Stop
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Successfully connected to Ollama API" -ForegroundColor Green
        Write-Host "✓ Using model: $modelName" -ForegroundColor Green
    }
}
catch {
    Write-Host "✗ Failed to connect to Ollama API at $apiUrl" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
