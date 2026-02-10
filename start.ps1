#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Start the Robotic Control System (Simulator + Backend + Frontend)

.DESCRIPTION
    This script starts all three components of the Robotic Control System:
    - Robot Simulator (TCP server on port 5000)
    - Backend API (.NET Web API on port 5001)
    - Frontend (Vite dev server on port 5173)

.PARAMETER Clean
    Perform cleanup before starting (removes bin, obj, node_modules, dist folders)

.PARAMETER SkipInstall
    Skip npm install step (use if dependencies are already installed)

.EXAMPLE
    .\start.ps1
    Start all components

.EXAMPLE
    .\start.ps1 -Clean
    Clean and start all components

.EXAMPLE
    .\start.ps1 -SkipInstall
    Start without running npm install
#>

param(
    [switch]$Clean,
    [switch]$SkipInstall
)

# Color output functions
function Write-Info { param($msg) Write-Host $msg -ForegroundColor Cyan }
function Write-Success { param($msg) Write-Host $msg -ForegroundColor Green }
function Write-Warning { param($msg) Write-Host $msg -ForegroundColor Yellow }
function Write-Error { param($msg) Write-Host $msg -ForegroundColor Red }

# Wait for a port to be listening
function Wait-ForPort {
    param(
        [int]$Port,
        [int]$TimeoutSeconds = 30,
        [string]$ServiceName = "Service"
    )

    $elapsed = 0
    $checkInterval = 500 # milliseconds

    Write-Info "   Waiting for $ServiceName to be ready on port $Port..."

    while ($elapsed -lt ($TimeoutSeconds * 1000)) {
        $connection = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
        if ($connection) {
            Write-Success "   [OK] $ServiceName is listening on port $Port"
            return $true
        }

        Start-Sleep -Milliseconds $checkInterval
        $elapsed += $checkInterval

        # Show progress every 2 seconds
        if ($elapsed % 2000 -eq 0) {
            Write-Host "   Still waiting... ($([int]($elapsed/1000))s)" -ForegroundColor Gray
        }
    }

    Write-Error "   [TIMEOUT] $ServiceName did not start within $TimeoutSeconds seconds"
    return $false
}

# Store process objects
$script:simulatorProcess = $null
$script:backendProcess = $null
$script:frontendProcess = $null
$script:rootDir = $PSScriptRoot
if ([string]::IsNullOrEmpty($script:rootDir)) {
    $script:rootDir = Get-Location
}

# Cleanup function - calls clean.ps1
function Invoke-Cleanup {
    $cleanScript = Join-Path $script:rootDir "clean.ps1"
    if (Test-Path $cleanScript) {
        & $cleanScript -ProcessesOnly
    }
}

# Register Ctrl+C handler
Register-EngineEvent -SourceIdentifier PowerShell.Exiting -Action {
    $cleanScript = Join-Path $PSScriptRoot "clean.ps1"
    if (Test-Path $cleanScript) {
        & $cleanScript -ProcessesOnly
    }
} | Out-Null

# Trap handler for script interruption
trap {
    $cleanScript = Join-Path $PSScriptRoot "clean.ps1"
    if (Test-Path $cleanScript) {
        & $cleanScript -ProcessesOnly
    }
    break
}

# Main script
try {
    Write-Host @"
================================================================

         Robotic Control System - Launcher

================================================================
"@ -ForegroundColor Cyan

    # Initial cleanup - call clean.ps1 to stop processes and optionally clean artifacts
    $cleanScript = Join-Path $script:rootDir "clean.ps1"
    if ($Clean) {
        Write-Info "`n[INIT] Running full cleanup (processes + artifacts)..."
        & $cleanScript
    } else {
        Write-Info "`n[INIT] Stopping any running processes..."
        & $cleanScript -ProcessesOnly
    }

    # Install frontend dependencies if needed
    if (!$SkipInstall) {
        Write-Info "`n[NPM] Installing frontend dependencies..."
        Push-Location (Join-Path $rootDir "client")
        try {
            $npmOutput = npm install 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Success "[OK] Frontend dependencies installed"
            } else {
                Write-Error "[ERROR] Failed to install frontend dependencies"
                Write-Host $npmOutput
                throw "npm install failed"
            }
        } finally {
            Pop-Location
        }
    } else {
        Write-Info "`n[SKIP] Skipping npm install (as requested)"
    }

    Write-Info "`n[START] Starting all services...`n"

    # Start Robot Simulator
    Write-Info "[1/3] Starting Robot Simulator (port 5000)..."
    $simulatorDir = Join-Path $rootDir "tools\RobotSimulator"
    $script:simulatorProcess = Start-Process -FilePath "cmd.exe" `
        -ArgumentList "/c", "dotnet", "run", "--project", $simulatorDir `
        -PassThru `
        -WindowStyle Normal

    if ($script:simulatorProcess) {
        Write-Success "   [OK] Simulator process started (PID: $($script:simulatorProcess.Id))"
    } else {
        throw "Failed to start simulator"
    }

    # Wait for simulator to be ready
    if (!(Wait-ForPort -Port 5000 -ServiceName "Robot Simulator" -TimeoutSeconds 15)) {
        throw "Simulator failed to start listening on port 5000"
    }

    # Start Backend API
    Write-Info "`n[2/3] Starting Backend API (port 5001)..."
    $apiDir = Join-Path $rootDir "src\RoboticControl.API"
    $script:backendProcess = Start-Process -FilePath "cmd.exe" `
        -ArgumentList "/c", "dotnet", "run", "--project", $apiDir, "--launch-profile", "https" `
        -PassThru `
        -WindowStyle Normal

    if ($script:backendProcess) {
        Write-Success "   [OK] Backend API process started (PID: $($script:backendProcess.Id))"
    } else {
        throw "Failed to start backend API"
    }

    # Wait for backend to be ready
    if (!(Wait-ForPort -Port 5001 -ServiceName "Backend API" -TimeoutSeconds 20)) {
        throw "Backend API failed to start listening on port 5001"
    }

    # Start Frontend
    Write-Info "`n[3/3] Starting Frontend Dev Server (port 5173)..."
    $clientDir = Join-Path $rootDir "client"
    $script:frontendProcess = Start-Process -FilePath "cmd.exe" `
        -ArgumentList "/c", "npm", "run", "dev" `
        -WorkingDirectory $clientDir `
        -PassThru `
        -WindowStyle Normal

    if ($script:frontendProcess) {
        Write-Success "   [OK] Frontend process started (PID: $($script:frontendProcess.Id))"
    } else {
        throw "Failed to start frontend"
    }

    # Wait for frontend to be ready
    if (!(Wait-ForPort -Port 5173 -ServiceName "Frontend" -TimeoutSeconds 15)) {
        throw "Frontend failed to start listening on port 5173"
    }

    Write-Host @"

================================================================

  All services are starting!

  Robot Simulator:  Running in separate window
  Backend API:      https://localhost:5001
  Frontend:         http://localhost:5173
  Swagger API Docs: https://localhost:5001/swagger

  Open your browser to: http://localhost:5173

  Press Ctrl+C to stop all services

================================================================

"@ -ForegroundColor Green

    Write-Info "[MONITOR] Monitoring processes... (Press Ctrl+C to stop)"
    Write-Host ""

    # Monitor processes and keep script running
    while ($true) {
        # Check if any process has exited
        if ($script:simulatorProcess.HasExited) {
            Write-Error "[ERROR] Simulator process exited unexpectedly!"
            break
        }
        if ($script:backendProcess.HasExited) {
            Write-Error "[ERROR] Backend API process exited unexpectedly!"
            break
        }
        if ($script:frontendProcess.HasExited) {
            Write-Error "[ERROR] Frontend process exited unexpectedly!"
            break
        }

        Start-Sleep -Seconds 2
    }

} catch {
    Write-Error "`n[ERROR] Error: $_"
    Write-Error $_.ScriptStackTrace
} finally {
    # Final cleanup when script exits
    $cleanScript = Join-Path $script:rootDir "clean.ps1"
    if (Test-Path $cleanScript) {
        & $cleanScript -ProcessesOnly
    }

    # Cleanup event registration
    Get-EventSubscriber -SourceIdentifier PowerShell.Exiting -ErrorAction SilentlyContinue |
        Unregister-Event -ErrorAction SilentlyContinue

    Write-Host "`nPress any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}
