@echo off
setlocal
set "ROOT=%~dp0"
set "OUT=%ROOT%publish\web"

if not defined WEB_PORT set "WEB_PORT=5000"

if not exist "%OUT%\CodeWF.Web.dll" (
  echo [CodeWF] Publish output is missing. Running publish.bat...
  call "%ROOT%publish.bat" || exit /b 1
)

echo [CodeWF] Starting http://localhost:%WEB_PORT% ...
pushd "%OUT%"
set "ASPNETCORE_URLS=http://localhost:%WEB_PORT%"
set "ASPNETCORE_ENVIRONMENT=Production"
dotnet CodeWF.Web.dll
popd
exit /b 0
