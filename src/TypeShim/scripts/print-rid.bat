@echo off
setlocal enabledelayedexpansion

set "rid="

for /f "tokens=1,* delims=:" %%A in ('dotnet --info ^| findstr /c:" RID:"') do (
  set "rid=%%B"
)

for %%T in (!rid!) do set "rid=%%T"

if not defined rid (
  >&2 echo Failed to determine RID from "dotnet --info"
  exit /b 1
)

<nul set /p="!rid!"
exit /b 0