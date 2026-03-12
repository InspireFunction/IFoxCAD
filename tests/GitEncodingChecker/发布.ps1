#!/usr/bin/env pwsh
# 发布脚本 - PowerShell 版本

$ErrorActionPreference = "Stop"

# 设置 UTF-8 编码
$OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "开始发布 EncodingChecker..." -ForegroundColor Cyan

# 发布项目（单文件模式，嵌入dll到exe中）
Write-Host "正在编译发布（单文件模式）..." -ForegroundColor Yellow
dotnet publish EncodingChecker.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true
if ($LASTEXITCODE -ne 0) {
    Write-Error "发布失败！"
    exit 1
}

# 获取 Git 目录
$gitDir = git rev-parse --git-dir 2>$null
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrEmpty($gitDir)) {
    Write-Error "无法找到 Git 仓库目录！"
    exit 1
}

$hooksDir = Join-Path $gitDir "hooks"

# 创建 hooks 目录（如果不存在）
if (-not (Test-Path $hooksDir)) {
    Write-Host "创建 hooks 目录: $hooksDir" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $hooksDir -Force | Out-Null
}

# 复制文件
$sourceExe = "bin\Release\win-x64\publish\EncodingChecker.exe"
$destExe = Join-Path $hooksDir "EncodingChecker.exe"
$sourceHook = "pre-commit"
$destHook = Join-Path $hooksDir "pre-commit"

if (Test-Path $sourceExe) {
    Copy-Item -Path $sourceExe -Destination $destExe -Force
    Write-Host "已复制: $sourceExe -> $destExe" -ForegroundColor Green
} else {
    Write-Error "源文件不存在: $sourceExe"
    exit 1
}

if (Test-Path $sourceHook) {
    Copy-Item -Path $sourceHook -Destination $destHook -Force
    Write-Host "已复制: $sourceHook -> $destHook" -ForegroundColor Green
} else {
    Write-Warning "pre-commit 文件不存在，跳过复制"
}

Write-Host "`n✅ 发布完成！已安装到: $hooksDir" -ForegroundColor Green
