#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Manage the ultimate dual-purpose Ollama model (dialog + quest generation)
    
.DESCRIPTION
    Single model for both NPC dialog and quest generation.
    Task detection is automatic based on prompt content.
    Saves 4.9GB VRAM by combining both models into one.
    
.PARAMETER Action
    The action to perform: reload (default), stop, start, test, clean
    
.EXAMPLE
    .\reload_ultimate_model.ps1
    .\reload_ultimate_model.ps1 -Action test
    .\reload_ultimate_model.ps1 -Action clean
#>

param(
    [ValidateSet('reload', 'stop', 'start', 'test', 'clean')]
    [string]$Action = 'reload'
)

# Find ollama executable
$ollamaPath = $null
if (Get-Command ollama -ErrorAction SilentlyContinue) {
    $ollamaPath = "ollama"
} elseif (Test-Path "C:\Program Files\ollama\ollama.exe") {
    $ollamaPath = "C:\Program Files\ollama\ollama.exe"
} else {
    Write-Error "Ollama not found in PATH or Program Files"
    exit 1
}

$modelFile = Join-Path $PSScriptRoot "Modelfile"
$modelName = "ultimate"

if (-not (Test-Path $modelFile)) {
    Write-Error "Modelfile not found at: $modelFile"
    exit 1
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Ultimate Dual-Purpose Model Manager" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Clean old models (before reload)
if ($Action -eq 'clean') {
    Write-Host "Cleaning old models to free VRAM..." -ForegroundColor Yellow
    
    @('dialog-engine', 'quest-generator') | ForEach-Object {
        $oldModel = $_
        Write-Host "  Stopping $oldModel..." -ForegroundColor DarkYellow
        try {
            & $ollamaPath stop "${oldModel}:latest" *>$null
            Start-Sleep -Milliseconds 300
        } catch {
            # Ignore
        }
        
        Write-Host "  Removing $oldModel..." -ForegroundColor DarkYellow
        try {
            & $ollamaPath rm "${oldModel}:latest" *>$null
        } catch {
            # Already removed
        }
    }
    Write-Host "✓ Cleanup complete" -ForegroundColor Green
    Write-Host ""
}

# Stop the model
if ($Action -in 'reload', 'stop') {
    Write-Host "Stopping $modelName..." -ForegroundColor Yellow
    try {
        & $ollamaPath stop "${modelName}:latest" *>$null
        Start-Sleep -Milliseconds 500
        Write-Host "✓ Stopped" -ForegroundColor Green
    } catch {
        Write-Host "✓ (Not running)" -ForegroundColor Gray
    }
}

# Remove the old model
if ($Action -eq 'reload') {
    Write-Host "Removing old $modelName..." -ForegroundColor Yellow
    try {
        & $ollamaPath rm "${modelName}:latest" *>$null
        Write-Host "✓ Removed" -ForegroundColor Green
    } catch {
        Write-Host "✓ (Did not exist)" -ForegroundColor Gray
    }
}

# Create the new model
if ($Action -in 'reload', 'start') {
    Write-Host "Creating $modelName from Modelfile..." -ForegroundColor Yellow
    try {
        $createOutput = & $ollamaPath create $modelName --file $modelFile 2>&1
        Write-Host "✓ Created" -ForegroundColor Green
        Write-Host ""
    } catch {
        Write-Error "Failed to create model: $_"
        exit 1
    }

    # Warm up the model
    if ($Action -eq 'reload') {
        Write-Host "Warming up $modelName..." -ForegroundColor Yellow
        try {
            "test" | & $ollamaPath run $modelName *>$null | Out-Null
            Write-Host "✓ Model is ready" -ForegroundColor Green
        } catch {
            Write-Host "✓ (Model created; warmup is optional)" -ForegroundColor Gray
        }
    }
}

# Test both capabilities
if ($Action -eq 'test') {
    Write-Host "Testing $modelName..." -ForegroundColor Yellow
    Write-Host ""
    
    Write-Host "1. Testing DIALOG generation:" -ForegroundColor Cyan
    $dialogTest = @"
You are Ser Ulric, a town guard. The player just asked about a missing person.
"@
    $dialogResult = $dialogTest | & $ollamaPath run $modelName 2>&1
    Write-Host $dialogResult -ForegroundColor Green
    Write-Host ""
    
    Write-Host "2. Testing QUEST generation:" -ForegroundColor Cyan
    $questTest = @"
Generate a quest for a merchant NPC named "Aldrin" in town. Challenge rating 2, theme: acquisition.
"@
    $questResult = $questTest | & $ollamaPath run $modelName 2>&1
    Write-Host $questResult -ForegroundColor Green
    Write-Host ""
    
    Write-Host "✓ Tests completed" -ForegroundColor Green
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  $modelName ready - Single model for" -ForegroundColor Cyan
Write-Host "  both Dialog and Quest generation" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Dialog Example:" -ForegroundColor DarkGray
Write-Host "    'NPC speaks about quests' | ollama run $modelName" -ForegroundColor DarkGray
Write-Host ""
Write-Host "  Quest Example:" -ForegroundColor DarkGray
Write-Host "    'Generate CR2 quest for merchant' | ollama run $modelName" -ForegroundColor DarkGray
Write-Host ""
