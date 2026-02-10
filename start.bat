@echo off
REM Batch file wrapper for start.ps1 PowerShell script
REM This allows running the script by double-clicking or from cmd.exe

echo Starting Robotic Control System...
echo.

REM Check if PowerShell is available
where pwsh >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    pwsh -ExecutionPolicy Bypass -File "%~dp0start.ps1" %*
) else (
    powershell -ExecutionPolicy Bypass -File "%~dp0start.ps1" %*
)

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Error: Failed to start the application
    pause
)
