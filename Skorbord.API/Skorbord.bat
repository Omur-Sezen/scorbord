@echo off
title SKORBORD - Football Data Management System
color 0B

:menu
cls
echo.
echo ========================================
echo    SKORBORD - Football Data System
echo ========================================
echo.
echo  1. Start Web Application
echo  2. Start with Database Reset
echo  3. Build Only
echo  4. Clean and Build
echo  5. View Application Status
echo  6. Open Swagger (API Documentation)
echo  7. Open Web Application
echo  8. Exit
echo.
echo ========================================
echo.

set /p choice="Enter your choice (1-8): "

if "%choice%"=="1" goto start_app
if "%choice%"=="2" goto start_with_reset
if "%choice%"=="3" goto build_only
if "%choice%"=="4" goto clean_build
if "%choice%"=="5" goto status
if "%choice%"=="6" goto swagger
if "%choice%"=="7" goto webapp
if "%choice%"=="8" goto exit
goto menu

:start_app
echo.
echo [INFO] Checking for existing instances...
netstat -ano | findstr :5208 >nul
if %errorlevel% equ 0 (
    echo [WARNING] Port 5208 is already in use!
    echo [INFO] Stopping existing processes...
    
    for /f "tokens=5" %%a in ('netstat -ano ^| findstr :5208') do (
        echo [INFO] Stopping process PID: %%a
        taskkill /F /PID %%a >nul 2>&1
    )
    
    echo [INFO] Waiting 2 seconds...
    timeout /t 2 >nul
)

echo [INFO] Starting SKORBORD application...
echo.
dotnet run
goto menu

:start_with_reset
echo.
echo [WARNING] This will reset the database!
set /p confirm="Are you sure? (y/N): "
if /i not "%confirm%"=="y" goto menu

echo [INFO] Cleaning database...
if exist "skorbord.db" del "skorbord.db"
if exist "*.db" del "*.db"

echo [INFO] Starting application...
dotnet run
goto menu

:build_only
echo.
echo [INFO] Building application...
dotnet build
echo.
echo Build completed. Press any key to continue...
pause >nul
goto menu

:clean_build
echo.
echo [INFO] Cleaning and rebuilding...
dotnet clean
dotnet build
echo.
echo Clean build completed. Press any key to continue...
pause >nul
goto menu

:status
echo.
echo [INFO] Checking application status...
echo.

REM Check if .NET is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] .NET is not installed
) else (
    echo [OK] .NET is installed
    for /f "tokens=*" %%i in ('dotnet --version') do echo [INFO] .NET Version: %%i
)

REM Check project files
if exist "Skorbord.API.csproj" (
    echo [OK] Project file found
) else (
    echo [ERROR] Project file not found
)

REM Check database
if exist "skorbord.db" (
    echo [OK] SQLite database found
) else (
    echo [INFO] No local database found
)

echo.
echo Press any key to continue...
pause >nul
goto menu

:swagger
echo.
echo [INFO] Opening Swagger documentation...
start http://localhost:5208/swagger
goto menu

:webapp
echo.
echo [INFO] Opening web application...
start http://localhost:5208
goto menu

:exit
echo.
echo [INFO] Goodbye!
timeout /t 2 >nul
exit
