@echo off
title SKORBORD - Quick Start
color 0A

echo Starting SKORBORD...
echo Web App: http://localhost:5208
echo Swagger: http://localhost:5208/swagger
echo.

REM Check if port 5208 is already in use
netstat -ano | findstr :5208 >nul
if %errorlevel% equ 0 (
    echo [WARNING] Port 5208 is already in use!
    echo [INFO] Stopping existing processes...
    
    REM Kill processes using port 5208
    for /f "tokens=5" %%a in ('netstat -ano ^| findstr :5208') do (
        echo [INFO] Stopping process PID: %%a
        taskkill /F /PID %%a >nul 2>&1
    )
    
    echo [INFO] Waiting 2 seconds...
    timeout /t 2 >nul
)

REM Start the application
dotnet run
