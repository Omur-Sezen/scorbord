@echo off
title SKORBORD - Football Data Application
color 0A

echo.
echo ========================================
echo    SKORBORD - Football Data App
echo ========================================
echo.
echo Starting application...
echo.

REM Check if .NET is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] .NET is not installed or not in PATH
    echo Please install .NET SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

REM Check if we're in the right directory
if not exist "Skorbord.API.csproj" (
    echo [ERROR] Skorbord.API.csproj not found
    echo Please run this script from the project directory
    pause
    exit /b 1
)

echo [INFO] .NET detected
echo [INFO] Project file found
echo [INFO] Building and running application...
echo.

REM Run the application
dotnet run

REM If the application stops, show a message
echo.
echo ========================================
echo Application stopped
echo Press any key to restart or close this window to exit
echo ========================================
pause >nul

REM Restart the application
start "" /d "%~dp0" "%~f0"
exit
