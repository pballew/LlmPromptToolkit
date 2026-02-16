#!/usr/bin/env pwsh
# Kill all running dotnet processes, VS Code debugger, and processes using ports 5123/5124

Write-Host "Stopping all dotnet and debugger processes..." -ForegroundColor Yellow

# Kill by process name
$processes = Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -match "dotnet|vsdbg" }

if ($processes) {
    $processes | Stop-Process -Force -ErrorAction SilentlyContinue
    Write-Host "✓ Stopped $($processes.Count) process(es)" -ForegroundColor Green
}

# Kill by port (handles edge cases where processes are still holding ports)
$ports = @(5123, 5124)
foreach ($port in $ports) {
    $tcpConnection = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
    if ($tcpConnection) {
        $process = Get-Process -Id $tcpConnection.OwningProcess -ErrorAction SilentlyContinue
        if ($process) {
            Write-Host "Killing process on port $port (PID: $($process.Id))" -ForegroundColor Yellow
            $process | Stop-Process -Force -ErrorAction SilentlyContinue
            Write-Host "✓ Killed process on port $port" -ForegroundColor Green
        }
    }
}

# Wait longer to ensure ports are released
Start-Sleep -Seconds 2

Write-Host "Done." -ForegroundColor Green
