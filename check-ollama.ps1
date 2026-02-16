# Check if Ollama is running the model and start it if needed
$modelName = "llama3.1:8b"

Write-Host "Checking if Ollama is running $modelName..." -ForegroundColor Cyan

# Check if ollama is running the model
$ollamaPs = ollama ps 2>$null

if ($ollamaPs -like "*$modelName*") {
    Write-Host "✓ Model $modelName is already running" -ForegroundColor Green
}
else {
    Write-Host "Model $modelName is not running. Starting it..." -ForegroundColor Yellow
    
    # Start the model in background
    # We use echo to send /bye to exit the interactive prompt and keep the server running
    echo "/bye" | ollama run $modelName --keepalive 8h 2>$null | Out-Null
    
    # Wait a moment for the model to start
    Start-Sleep -Seconds 2
    
    # Verify it's running
    $ollamaPs = ollama ps 2>$null
    if ($ollamaPs -like "*$modelName*") {
        Write-Host "✓ Model $modelName started successfully" -ForegroundColor Green
    }
    else {
        Write-Host "✗ Failed to start model $modelName" -ForegroundColor Red
        exit 1
    }
}
