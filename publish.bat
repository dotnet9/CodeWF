@echo off
setlocal enabledelayedexpansion

set "ROOT=%~dp0"
set "PUBLISH_DIR=%ROOT%publish"
set "WEB_PROJECT=%ROOT%src\CodeWF.Web"
set "ADMIN_PROJECT=%ROOT%src\CodeWF.Admin"
set "API_PROJECT=%ROOT%src\CodeWF.Api\CodeWF.Api.csproj"
set "WEB_OUT=%PUBLISH_DIR%\web"
set "ADMIN_OUT=%PUBLISH_DIR%\admin"
set "API_OUT=%PUBLISH_DIR%\api"
if not defined API_URL set "API_URL=http://localhost:5002"
set "API_BASE_URL=%API_URL%/api"
set "TEMP_API_PID_FILE=%PUBLISH_DIR%\publish-api.pid"

echo.
echo [CodeWF] Cleaning publish folders...
if exist "%WEB_OUT%" rmdir /s /q "%WEB_OUT%"
if exist "%ADMIN_OUT%" rmdir /s /q "%ADMIN_OUT%"
if exist "%API_OUT%" rmdir /s /q "%API_OUT%"
mkdir "%WEB_OUT%" "%ADMIN_OUT%" "%API_OUT%" || goto :error

call :sync_logo || goto :error

echo.
echo [CodeWF] Publishing ASP.NET Web API to publish\api...
dotnet publish "%API_PROJECT%" --configuration Release --output "%API_OUT%" || goto :error

echo.
echo [CodeWF] Ensuring API is available for frontend prerendering...
powershell -NoProfile -ExecutionPolicy Bypass -Command "try { $r = Invoke-WebRequest -Uri '%API_BASE_URL%/health' -UseBasicParsing -TimeoutSec 3; if ($r.StatusCode -lt 400) { exit 0 } else { exit 1 } } catch { exit 1 }"
if errorlevel 1 (
  echo [CodeWF] Starting temporary API on %API_URL%...
  powershell -NoProfile -ExecutionPolicy Bypass -Command "$env:ASPNETCORE_URLS='%API_URL%'; $env:ASPNETCORE_ENVIRONMENT='Production'; $p = Start-Process -FilePath '%API_OUT%\CodeWF.Api.exe' -WorkingDirectory '%API_OUT%' -PassThru -WindowStyle Hidden; Set-Content -Path '%TEMP_API_PID_FILE%' -Value $p.Id" || goto :error
  call :wait_for_api || goto :error
) else (
  echo [CodeWF] Existing API is available.
)

echo.
echo [CodeWF] Building Next.js frontend...
pushd "%ROOT%" || goto :error
set "API_BASE_URL=%API_BASE_URL%"
call npm run build:frontend || goto :error
popd

echo.
echo [CodeWF] Publishing frontend to publish\web...
robocopy "%WEB_PROJECT%\.next" "%WEB_OUT%\.next" /E /NFL /NDL /NJH /NJS /NP
if errorlevel 8 goto :error
if exist "%WEB_PROJECT%\public" (
  robocopy "%WEB_PROJECT%\public" "%WEB_OUT%\public" /E /NFL /NDL /NJH /NJS /NP
  if errorlevel 8 goto :error
)
call :sync_published_logo || goto :error
copy /Y "%WEB_PROJECT%\package.json" "%WEB_OUT%\" >nul || goto :error
copy /Y "%WEB_PROJECT%\next.config.ts" "%WEB_OUT%\" >nul || goto :error
copy /Y "%WEB_PROJECT%\tsconfig.json" "%WEB_OUT%\" >nul || goto :error
copy /Y "%WEB_PROJECT%\next-env.d.ts" "%WEB_OUT%\" >nul || goto :error

echo.
echo [CodeWF] Building admin frontend...
pushd "%ROOT%" || goto :error
call npm run build:admin || goto :error
popd

echo.
echo [CodeWF] Publishing admin frontend to publish\admin...
robocopy "%ADMIN_PROJECT%\dist" "%ADMIN_OUT%" /E /NFL /NDL /NJH /NJS /NP
if errorlevel 8 goto :error
call :sync_published_logo || goto :error

call :stop_temp_api

echo.
echo [CodeWF] Publish completed.
echo   Web:   %WEB_OUT%
echo   Admin: %ADMIN_OUT%
echo   API:   %API_OUT%
echo.
echo To run the published Next.js frontend, install production dependencies in publish\web and run:
echo   npm install --omit=dev
echo   npm run start -- --port 5000
echo.
exit /b 0

:wait_for_api
for /L %%i in (1,1,30) do (
  powershell -NoProfile -ExecutionPolicy Bypass -Command "try { $r = Invoke-WebRequest -Uri '%API_BASE_URL%/health' -UseBasicParsing -TimeoutSec 2; if ($r.StatusCode -lt 400) { exit 0 } else { exit 1 } } catch { exit 1 }"
  if not errorlevel 1 exit /b 0
  timeout /t 1 /nobreak >nul
)
echo [CodeWF] Temporary API did not become ready.
exit /b 1

:stop_temp_api
if exist "%TEMP_API_PID_FILE%" (
  for /f "usebackq delims=" %%p in ("%TEMP_API_PID_FILE%") do (
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Stop-Process -Id %%p -Force -ErrorAction SilentlyContinue"
  )
  del "%TEMP_API_PID_FILE%" >nul 2>nul
)
exit /b 0

:sync_logo
echo [CodeWF] Syncing root logo files to frontend public folders...
if not exist "%WEB_PROJECT%\public" mkdir "%WEB_PROJECT%\public" || exit /b 1
if not exist "%ADMIN_PROJECT%\public" mkdir "%ADMIN_PROJECT%\public" || exit /b 1
for %%f in (logo.svg logo.png logo.ico) do (
  if not exist "%ROOT%%%f" (
    echo [CodeWF] Missing root logo file: %ROOT%%%f
    exit /b 1
  )
  copy /Y "%ROOT%%%f" "%WEB_PROJECT%\public\%%f" >nul || exit /b 1
  copy /Y "%ROOT%%%f" "%ADMIN_PROJECT%\public\%%f" >nul || exit /b 1
)
copy /Y "%ROOT%logo.ico" "%WEB_PROJECT%\public\favicon.ico" >nul || exit /b 1
copy /Y "%ROOT%logo.ico" "%ADMIN_PROJECT%\public\favicon.ico" >nul || exit /b 1
exit /b 0

:sync_published_logo
echo [CodeWF] Syncing root logo files to published output...
if exist "%WEB_OUT%\" (
  if not exist "%WEB_OUT%\public" mkdir "%WEB_OUT%\public" || exit /b 1
  for %%f in (logo.svg logo.png logo.ico) do (
    copy /Y "%ROOT%%%f" "%WEB_OUT%\public\%%f" >nul || exit /b 1
  )
  copy /Y "%ROOT%logo.ico" "%WEB_OUT%\public\favicon.ico" >nul || exit /b 1
)
if exist "%ADMIN_OUT%\" (
  for %%f in (logo.svg logo.png logo.ico) do (
    copy /Y "%ROOT%%%f" "%ADMIN_OUT%\%%f" >nul || exit /b 1
  )
  copy /Y "%ROOT%logo.ico" "%ADMIN_OUT%\favicon.ico" >nul || exit /b 1
)
exit /b 0

:error
call :stop_temp_api
echo.
echo [CodeWF] Publish failed.
exit /b 1
