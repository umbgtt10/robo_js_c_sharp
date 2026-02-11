#!/usr/bin/env pwsh
# Test runner for Robotic Control System
# Runs both frontend and backend tests

param(
    [switch]$Frontend,
    [switch]$Backend,
    [switch]$Coverage,
    [switch]$Watch
)

$ErrorActionPreference = "Stop"
$rootDir = $PSScriptRoot

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "  Robotic Control System - Tests" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# If no specific flags, run all tests
$runAll = -not ($Frontend -or $Backend)

# Frontend Tests
if ($Frontend -or $runAll) {
    Write-Host "Frontend: Running React tests..." -ForegroundColor Yellow
    Push-Location "$rootDir\client"

    try {
        # Check if node_modules exists, install if missing
        if (-not (Test-Path "node_modules")) {
            Write-Host "  Installing dependencies..." -ForegroundColor Gray
            npm install
            if ($LASTEXITCODE -ne 0) {
                Write-Host "Frontend: npm install FAILED!" -ForegroundColor Red
                Pop-Location
                exit 1
            }
        }

        if ($Watch) {
            npm run test:watch
        }
        elseif ($Coverage) {
            npm run test:coverage
        }
        else {
            npm test -- --run
        }

        if ($LASTEXITCODE -ne 0) {
            Write-Host "Frontend: Tests FAILED!" -ForegroundColor Red
            Pop-Location
            exit 1
        }

        Write-Host "Frontend: Tests PASSED" -ForegroundColor Green
        Write-Host ""
    }
    finally {
        Pop-Location
    }
}

# Backend Tests
if ($Backend -or $runAll) {
    Write-Host "Backend: Running .NET tests..." -ForegroundColor Yellow

    # Check for running API processes and warn user
    $apiProcess = Get-Process -Name "RoboticControl.API" -ErrorAction SilentlyContinue
    if ($apiProcess) {
        Write-Host "  Warning: RoboticControl.API is running (PID: $($apiProcess.Id))" -ForegroundColor Yellow
        Write-Host "  This may cause DLL lock issues. Stop the API first with: .\clean.ps1" -ForegroundColor Yellow
        Write-Host ""
    }

    # Unit Tests
    if (Test-Path "$rootDir\tests\RoboticControl.UnitTests") {
        Write-Host "  -> Unit Tests" -ForegroundColor Gray
        dotnet test "$rootDir\tests\RoboticControl.UnitTests\RoboticControl.UnitTests.csproj" --nologo --logger "console;verbosity=normal"

        if ($LASTEXITCODE -ne 0) {
            Write-Host "Backend: Unit tests FAILED!" -ForegroundColor Red
            exit 1
        }
    }

    # Integration Tests
    if (Test-Path "$rootDir\tests\RoboticControl.IntegrationTests") {
        Write-Host "  -> Integration Tests" -ForegroundColor Gray
        dotnet test "$rootDir\tests\RoboticControl.IntegrationTests\RoboticControl.IntegrationTests.csproj" --nologo --logger "console;verbosity=normal"

        if ($LASTEXITCODE -ne 0) {
            Write-Host "Backend: Integration tests FAILED!" -ForegroundColor Red
            exit 1
        }
    }

    Write-Host "Backend: Tests PASSED" -ForegroundColor Green
    Write-Host ""
}

Write-Host "=====================================" -ForegroundColor Green
Write-Host "  All Tests Completed Successfully!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
