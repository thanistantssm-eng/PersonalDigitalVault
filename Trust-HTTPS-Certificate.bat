@echo off
setlocal
echo Trusting the ASP.NET Core local HTTPS development certificate...
dotnet dev-certs https --trust
if errorlevel 1 (
  echo Failed. Install the .NET SDK and run this file again.
) else (
  echo HTTPS development certificate is ready.
)
pause
endlocal
