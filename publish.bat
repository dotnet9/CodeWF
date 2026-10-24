@echo off
setlocal
set "ROOT=%~dp0"
set "PUBLISH_DIR=%ROOT%publish"
set "PROJECT=%ROOT%src\CodeWF.Web\CodeWF.Web.csproj"
set "OUT=%PUBLISH_DIR%\web"

echo [CodeWF] Publishing Blazor Web App (single process)...
dotnet publish "%PROJECT%" --configuration Release --output "%OUT%" || goto :error

echo.
echo [CodeWF] Publish completed.
echo   Site: %OUT%
echo   Run : dotnet CodeWF.Web.dll  (or run.bat)
echo   Remember to point Site:LocalAssetsDir (appsettings.Production.json) at the Assets.Dotnet9 checkout.
exit /b 0

:error
echo [CodeWF] Publish failed.
exit /b 1
