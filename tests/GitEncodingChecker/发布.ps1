#!/usr/bin/env pwsh
# 发布脚本 - PowerShell 版本

$ErrorActionPreference = "Stop"

# 设置 UTF-8 编码
$OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "开始发布 EncodingChecker..." -ForegroundColor Cyan

# 让用户选择编译模式
Write-Host "`n请选择编译模式：" -ForegroundColor Cyan
Write-Host "  1. AOT 原生编译 (推荐) - 文件小 (~2.6MB)，启动快，无运行时依赖" -ForegroundColor Green
Write-Host "  2. 单文件模式 - 包含运行时 (~60MB)，兼容性更好" -ForegroundColor Yellow

$choice = Read-Host "`n请输入选项 (1 或 2，默认 1)"

# 根据选择执行不同的发布命令
switch ($choice) {
    "2" {
        Write-Host "`n正在编译发布（单文件模式）..." -ForegroundColor Yellow
        dotnet publish EncodingChecker.csproj -c Release -r win-x64 --self-contained true `
            /p:PublishSingleFile=true `
            /p:IncludeNativeLibrariesForSelfExtract=true `
            /p:EnableCompressionInSingleFile=true `
            /p:PublishAot=false
        $mode = "单文件模式"
    }
    default {
        Write-Host "`n正在编译发布（AOT原生编译模式）..." -ForegroundColor Yellow
        dotnet publish EncodingChecker.csproj -c Release
        $mode = "AOT原生编译模式"
    }
}

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
    $fileSize = (Get-Item $destExe).Length / 1MB
    Write-Host "已复制: $sourceExe -> $destExe ($([math]::Round($fileSize, 2)) MB)" -ForegroundColor Green
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

Write-Host "`n✅ 发布完成！模式: $mode，已安装到: $hooksDir" -ForegroundColor Green
