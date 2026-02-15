#!/usr/bin/env pwsh
# Kill all running dotnet processes

Write-Host "Stopping all dotnet processes..." -ForegroundColor Yellow

$processes = Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -match "dotnet" }

if ($processes) {
    $processes | Stop-Process -Force -ErrorAction SilentlyContinue
    Write-Host "✓ Stopped $($processes.Count) dotnet process(es)" -ForegroundColor Green
    Start-Sleep -Seconds 1
} else {
    Write-Host "✓ No dotnet processes running" -ForegroundColor Green
}

Write-Host "Done." -ForegroundColor Green
