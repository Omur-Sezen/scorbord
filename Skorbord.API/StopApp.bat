@echo off
title SKORBORD - Stop Application
color 0C

echo Stopping SKORBORD application...
echo.

netstat -ano | findstr :5208 >nul
if %errorlevel% equ 0 (
    echo [INFO] Found processes using port 5208:
    netstat -ano | findstr :5208
    echo.
    
    for /f "tokens=5" %%a in ('netstat -ano ^| findstr :5208') do (
        echo [INFO] Stopping process PID: %%a
        taskkill /F /PID %%a >nul 2>&1
        if %errorlevel% equ 0 (
            echo [OK] Process %%a stopped successfully
        ) else (
            echo [ERROR] Could not stop process %%a
        )
    )
    
    echo.
    echo [INFO] All processes stopped. Port 5208 is now free.
) else (
    echo [INFO] No processes found using port 5208.
)

echo.
echo Press any key to exit...
pause >nul
