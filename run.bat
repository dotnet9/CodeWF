@echo off
setlocal

set "ROOT=%~dp0"
set "PUBLISH_DIR=%ROOT%publish"
set "WEB_PROJECT=%ROOT%src\CodeWF.Web"
set "ADMIN_PROJECT=%ROOT%src\CodeWF.Admin"
set "WEB_OUT=%PUBLISH_DIR%\web"
set "ADMIN_OUT=%PUBLISH_DIR%\admin"
set "API_OUT=%PUBLISH_DIR%\api"

if not defined WEB_PORT set "WEB_PORT=5000"
if not defined ADMIN_PORT set "ADMIN_PORT=5001"
if not defined API_PORT set "API_PORT=5002"
if not defined API_URL set "API_URL=http://localhost:%API_PORT%"
if not defined API_BASE_URL set "API_BASE_URL=%API_URL%/api"
if not defined ADMIN_URL set "ADMIN_URL=/admin/"

if /i "%~1"=="--api" goto :run_api
if /i "%~1"=="--web" goto :run_web
if /i "%~1"=="--admin" goto :run_admin
if /i "%~1"=="--help" goto :help
if /i "%~1"=="/?" goto :help

echo.
echo [CodeWF] One-click run
echo   Web service:   http://localhost:%WEB_PORT%
echo   Admin service: http://localhost:%ADMIN_PORT%/admin/
echo   API service:   %API_URL%
echo   Nginx paths:   /, /admin/, /api/
echo.

where npm >nul 2>nul || (
  echo [CodeWF] npm was not found. Install Node.js 22+ first.
  exit /b 1
)

where dotnet >nul 2>nul || (
  echo [CodeWF] dotnet was not found. Install .NET 10 SDK or Runtime first.
  exit /b 1
)

call :ensure_node_modules || goto :error

if not exist "%WEB_OUT%\package.json" (
  echo [CodeWF] Missing published frontend: %WEB_OUT%
  echo [CodeWF] Run publish.bat manually first. run.bat does not publish automatically.
  exit /b 1
)

if not exist "%API_OUT%\" (
  echo [CodeWF] Missing published API: %API_OUT%
  echo [CodeWF] Run publish.bat manually first. run.bat does not publish automatically.
  exit /b 1
)

if not exist "%ADMIN_OUT%\index.html" (
  echo [CodeWF] Missing published admin frontend: %ADMIN_OUT%
  echo [CodeWF] Run publish.bat manually first. run.bat does not publish automatically.
  exit /b 1
)

if not exist "%WEB_OUT%\node_modules\next\" (
  echo [CodeWF] Installing production dependencies for published frontend...
  pushd "%WEB_OUT%" || goto :error
  call npm install --omit=dev --package-lock=false || (popd & goto :error)
  popd
)

echo.
echo [CodeWF] Starting services in separate windows...
start "CodeWF API" cmd /k ""%~f0" --api"
start "CodeWF Web" cmd /k ""%~f0" --web"
start "CodeWF Admin" cmd /k ""%~f0" --admin"

echo.
echo [CodeWF] Started.
echo   Web service:   http://localhost:%WEB_PORT%
echo   Admin service: http://localhost:%ADMIN_PORT%/admin/
echo   API service:   %API_URL%
echo   Nginx paths:   /, /admin/, /api/
echo.
exit /b 0

:run_api
echo [CodeWF] Starting API on %API_URL%...
pushd "%API_OUT%" || exit /b 1
set "ASPNETCORE_URLS=%API_URL%"
set "ASPNETCORE_ENVIRONMENT=Production"
if exist "CodeWF.Api.exe" (
  CodeWF.Api.exe
) else (
  dotnet CodeWF.Api.dll
)
set "EXIT_CODE=%ERRORLEVEL%"
popd
exit /b %EXIT_CODE%

:run_web
echo [CodeWF] Starting Web on http://localhost:%WEB_PORT%...
pushd "%WEB_OUT%" || exit /b 1
set "API_BASE_URL=%API_BASE_URL%"
set "NEXT_PUBLIC_ADMIN_URL=%ADMIN_URL%"
call npm run start -- --port %WEB_PORT%
set "EXIT_CODE=%ERRORLEVEL%"
popd
exit /b %EXIT_CODE%

:run_admin
echo [CodeWF] Starting Admin on http://localhost:%ADMIN_PORT%/admin/...
call :ensure_node_modules || exit /b 1
pushd "%ADMIN_PROJECT%" || exit /b 1
call npm run preview -- --port %ADMIN_PORT% --outDir "%ADMIN_OUT%"
set "EXIT_CODE=%ERRORLEVEL%"
popd
exit /b %EXIT_CODE%

:ensure_node_modules
where npm >nul 2>nul || (
  echo [CodeWF] npm was not found. Install Node.js 22+ first.
  exit /b 1
)
if not exist "%ROOT%node_modules\" (
  echo [CodeWF] Installing workspace dependencies...
  pushd "%ROOT%" || exit /b 1
  call npm install || (popd & exit /b 1)
  popd
)
exit /b 0

:help
echo Usage:
echo   run.bat
echo.
echo Run publish.bat manually before run.bat. If publish folders already exist,
echo run.bat starts them directly and never republishes.
echo.
echo Environment overrides:
echo   WEB_PORT=5000
echo   ADMIN_PORT=5001
echo   API_PORT=5002
echo   API_URL=http://localhost:5002
echo   API_BASE_URL=http://localhost:5002/api
echo   ADMIN_URL=/admin/
exit /b 0

:error
echo.
echo [CodeWF] Run failed.
exit /b 1
