@echo off
setlocal

set "ROOT=%~dp0"
set "SITE=%ROOT%FAME LMS Production Source Code\FAME.Web"
set "IIS_EXPRESS=C:\Program Files\IIS Express\iisexpress.exe"
set "SQL_SERVICE=MSSQL$SQLEXPRESS01"
set "URL=http://localhost:8080/"

if not exist "%SITE%\Web.config" (
    echo Site folder not found: "%SITE%"
    exit /b 1
)

if not exist "%IIS_EXPRESS%" (
    echo IIS Express is not installed at "%IIS_EXPRESS%"
    exit /b 1
)

powershell.exe -NoProfile -Command "try { $r = Invoke-WebRequest '%URL%' -UseBasicParsing -TimeoutSec 5; if ($r.StatusCode -eq 200) { exit 0 } else { exit 1 } } catch { exit 1 }"
if not errorlevel 1 (
    echo Site is already running at %URL%
    start "" "%URL%"
    exit /b 0
)

sc query "%SQL_SERVICE%" >nul 2>&1
if errorlevel 1 (
    echo SQL Server service "%SQL_SERVICE%" was not found.
    exit /b 1
)

powershell.exe -NoProfile -Command "$svc = Get-Service -Name '%SQL_SERVICE%' -ErrorAction SilentlyContinue; if (-not $svc) { exit 2 } elseif ($svc.Status -eq 'Running') { exit 0 } else { exit 1 }"
if errorlevel 2 (
    echo SQL Server service "%SQL_SERVICE%" was not found.
    exit /b 1
)
if errorlevel 1 (
    echo Starting SQL Server service "%SQL_SERVICE%"...
    net start "%SQL_SERVICE%" >nul
    if errorlevel 1 (
        echo Failed to start SQL Server service "%SQL_SERVICE%".
        exit /b 1
    )
)

start "" "%IIS_EXPRESS%" /path:"%SITE%" /port:8080
powershell.exe -NoProfile -Command "Start-Sleep -Seconds 5"

powershell.exe -NoProfile -Command "try { $r = Invoke-WebRequest '%URL%' -UseBasicParsing -TimeoutSec 15; if ($r.StatusCode -eq 200) { exit 0 } else { exit 1 } } catch { exit 1 }"
if errorlevel 1 (
    echo Localhost did not start correctly. Try running this file as Administrator.
    exit /b 1
)

echo Site is running at %URL%
start "" "%URL%"
exit /b 0
