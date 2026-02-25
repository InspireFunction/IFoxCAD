@echo off
chcp 65001 >nul
dotnet publish EncodingChecker.csproj -c Release -p:PublishAOT=true

for /f "delims=" %%i in ('git rev-parse --git-dir') do set "GIT_DIR=%%i"
if not exist "%GIT_DIR%\hooks" mkdir "%GIT_DIR%\hooks"
copy /y bin\Release\win-x64\publish\EncodingChecker.exe "%GIT_DIR%\hooks\EncodingChecker.exe"
copy /y pre-commit "%GIT_DIR%\hooks\pre-commit"
echo 已安装到: %GIT_DIR%\hooks