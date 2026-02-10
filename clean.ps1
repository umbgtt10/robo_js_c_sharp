#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Clean build artifacts, dependencies, and stop running processes

.DESCRIPTION
    Removes all build artifacts (bin, obj) and dependencies (node_modules, dist)
    from the Robotic Control System project. Also stops all running processes.

.PARAMETER ProcessesOnly
    Only stop running processes without cleaning artifacts

.EXAMPLE
    .\clean.ps1
    Clean all build artifacts and stop processes

.EXAMPLE
    .\clean.ps1 -ProcessesOnly
    Only stop running processes
#>

param(
    [switch]$ProcessesOnly
)

# Color output functions
function Write-Info { param($msg) Write-Host $msg -ForegroundColor Cyan }
function Write-Success { param($msg) Write-Host $msg -ForegroundColor Green }

$rootDir = $PSScriptRoot
if ([string]::IsNullOrEmpty($rootDir)) {
    $rootDir = Get-Location
}

if (!$ProcessesOnly) {
    Write-Host @"
================================================================

         Robotic Control System - Cleanup

================================================================
"@ -ForegroundColor Cyan
}

Write-Info "`n[CLEANUP] Stopping running processes..."

# Kill processes on port 5000 (Simulator)
$proc5000 = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
if ($proc5000) {
    foreach ($processId in $proc5000) {
        Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
        Write-Host "  Killed process $processId on port 5000" -ForegroundColor Gray
    }
}

# Kill processes on port 5001 (Backend API)
$proc5001 = Get-NetTCPConnection -LocalPort 5001 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
if ($proc5001) {
    foreach ($processId in $proc5001) {
        Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
        Write-Host "  Killed process $processId on port 5001" -ForegroundColor Gray
    }
}

# Kill processes on port 5173 (Frontend)
$proc5173 = Get-NetTCPConnection -LocalPort 5173 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
if ($proc5173) {
    foreach ($processId in $proc5173) {
        Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
        Write-Host "  Killed process $processId on port 5173" -ForegroundColor Gray
    }
}

# Kill any RoboticControl processes
Get-Process | Where-Object {
    $_.MainWindowTitle -like "*RoboticControl*" -or
    $_.ProcessName -like "*RobotSimulator*" -or
    $_.ProcessName -eq "RoboticControl.API"
} | ForEach-Object {
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
}

Start-Sleep -Milliseconds 500
Write-Success "  [OK] Processes stopped"

if (!$ProcessesOnly) {
    Write-Info "`n[CLEANUP] Cleaning .NET build artifacts (bin, obj)..."
    $cleaned = 0
    Get-ChildItem -Path $rootDir -Include bin,obj -Recurse -Directory -ErrorAction SilentlyContinue | ForEach-Object {
        Write-Host "  Removing: $($_.FullName)" -ForegroundColor Gray
        Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
        $cleaned++
    }
    Write-Success "  [OK] Removed $cleaned .NET build folders"

    Write-Info "`n[CLEANUP] Cleaning frontend artifacts..."
    $clientDir = Join-Path $rootDir "client"

    if (Test-Path (Join-Path $clientDir "node_modules")) {
        Write-Host "  Removing: node_modules" -ForegroundColor Gray
        Remove-Item (Join-Path $clientDir "node_modules") -Recurse -Force -ErrorAction SilentlyContinue
        Write-Success "  [OK] Removed node_modules"
    } else {
        Write-Host "  No node_modules to remove" -ForegroundColor Gray
    }

    if (Test-Path (Join-Path $clientDir "dist")) {
        Write-Host "  Removing: dist" -ForegroundColor Gray
        Remove-Item (Join-Path $clientDir "dist") -Recurse -Force -ErrorAction SilentlyContinue
        Write-Success "  [OK] Removed dist"
    } else {
        Write-Host "  No dist to remove" -ForegroundColor Gray
    }

    Write-Info "`n[CLEANUP] Cleaning logs..."
    if (Test-Path (Join-Path $rootDir "src\RoboticControl.API\logs")) {
        Remove-Item (Join-Path $rootDir "src\RoboticControl.API\logs") -Recurse -Force -ErrorAction SilentlyContinue
        Write-Success "  [OK] Removed logs"
    }
}

Write-Success "`n[OK] Cleanup complete!`n"
