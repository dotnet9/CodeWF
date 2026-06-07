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

echo.
echo [CodeWF] Cleaning publish folders...
if exist "%WEB_OUT%" rmdir /s /q "%WEB_OUT%"
if exist "%ADMIN_OUT%" rmdir /s /q "%ADMIN_OUT%"
if exist "%API_OUT%" rmdir /s /q "%API_OUT%"
mkdir "%WEB_OUT%" "%ADMIN_OUT%" "%API_OUT%" || goto :error

echo.
echo [CodeWF] Building Next.js frontend...
pushd "%ROOT%" || goto :error
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

echo.
echo [CodeWF] Publishing ASP.NET Web API to publish\api...
dotnet publish "%API_PROJECT%" --configuration Release --output "%API_OUT%" || goto :error

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

:error
echo.
echo [CodeWF] Publish failed.
exit /b 1
