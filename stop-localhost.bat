@echo off
setlocal

taskkill /F /IM iisexpress.exe >nul 2>&1
if errorlevel 1 (
    echo No IIS Express process was running.
    exit /b 0
)

echo IIS Express stopped.
exit /b 0
