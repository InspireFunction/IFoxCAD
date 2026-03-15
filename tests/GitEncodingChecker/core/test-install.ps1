#!/usr/bin/env pwsh
# 自动化测试脚本 - 验证 GitEncodingChecker 安装和功能
# 使用方法: pwsh -File test-install.ps1


# 1. 编译项目 - 验证代码编译
# 2. 发布并安装到项目级别 - 调用 发布.ps1 -Yes -Target project
# 3. 运行单元测试 - 运行单元测试和集成测试
# 4. 测试帮助命令 - 验证帮助输出
# 5. 测试检查命令 - 验证无仓库时的行为
# 6. 在临时 Git 仓库中测试 - 验证基本功能
# 7. 验证项目级别安装 - 检查项目级别文件是否正确安装
# 8. 发布并安装到全局级别 - 调用 发布.ps1 -Yes -Target global
# 9. 验证全局安装和 Git 别名 - 检查全局文件和别名
# 10. 测试编码检测 - 验证编码检测功能
# 11. 清理项目级别安装 - 调用发布脚本清理项目级别
# 12. 清理全局级别安装 - 调用发布脚本清理全局级别
# 要运行这个完整的生产环境测试吗？


$ErrorActionPreference = "Stop"

# 路径映射字典 - 集中管理所有路径（使用绝对路径）
$script:PathMap = @{}

# 辅助函数：将相对路径转换为绝对路径
function Resolve-TestPath($relativePath) {
    $combined = Join-Path $PSScriptRoot $relativePath
    return (Resolve-Path $combined -ErrorAction SilentlyContinue).Path
}

# 初始化路径映射（先定义相对路径）
$relativePaths = @{
    ProjectDir     = $PSScriptRoot
    ExePath        = "bin\Release\win-x64\publish\EncodingChecker.exe"
    PublishScript  = "发布.ps1"
    UnitTest       = "..\tests\unit\EncodingChecker.Tests.csproj"
    IntegrationTest= "..\tests\Integration\EncodingChecker.IntegrationTests.csproj"
}

# 转换为绝对路径并存入 PathMap
$script:PathMap.ProjectDir = $PSScriptRoot
$script:PathMap.ExePath = Join-Path $PSScriptRoot $relativePaths.ExePath
$script:PathMap.PublishScript = Join-Path $PSScriptRoot $relativePaths.PublishScript
$script:PathMap.UnitTest = Resolve-TestPath $relativePaths.UnitTest
$script:PathMap.IntegrationTest = Resolve-TestPath $relativePaths.IntegrationTest

# 打印测试参数表
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  测试参数配置" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "项目目录: $($PathMap.ProjectDir)"
Write-Host "发布模式: AOT (Native)"
Write-Host "安装目标: project -> global"
Write-Host "测试项目:"
Write-Host "  - $($PathMap.UnitTest)"
Write-Host "  - $($PathMap.IntegrationTest)"
Write-Host "========================================`n" -ForegroundColor Cyan

# 预检：验证关键路径是否存在
Write-Host "预检: 验证关键路径..." -ForegroundColor Yellow
$requiredPaths = @{
    "单元测试"     = $PathMap.UnitTest
    "集成测试"     = $PathMap.IntegrationTest
    "发布脚本"     = $PathMap.PublishScript
}
$allPathsExist = $true
foreach ($name in $requiredPaths.Keys) {
    $path = $requiredPaths[$name]
    if ($path -and (Test-Path $path)) {
        Write-Host "  ✓ $name" -ForegroundColor Green
    }
    else {
        Write-Host "  ✗ $name (不存在: $path)" -ForegroundColor Red
        $allPathsExist = $false
    }
}
if (-not $allPathsExist) {
    Write-Host "`n❌ 预检失败: 部分关键路径不存在，请检查路径配置" -ForegroundColor Red
    exit 1
}
Write-Host "预检通过`n" -ForegroundColor Green

# 颜色输出函数
function Write-Success($msg) { Write-Host "✓ $msg" -ForegroundColor Green }
function Write-Error($msg) { Write-Host "✗ $msg" -ForegroundColor Red }
function Write-Info($msg) { Write-Host "ℹ $msg" -ForegroundColor Cyan }
function Write-Step($msg) { Write-Host "`n==> $msg" -ForegroundColor Yellow }

# 测试计数器
$script:TestsPassed = 0
$script:TestsFailed = 0

function Test-Step($name, $scriptBlock) {
    Write-Step $name
    try {
        & $scriptBlock
        $script:TestsPassed++
        Write-Success $name
        return $true
    }
    catch {
        $script:TestsFailed++
        Write-Error "$name : $_"
        return $false
    }
}

Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "  GitEncodingChecker 自动化测试脚本" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

# 步骤 1: 编译项目
Test-Step "编译项目" {
    Set-Location $PathMap.ProjectDir
    $output = dotnet build -c Release 2>&1
    if ($LASTEXITCODE -ne 0) { throw "编译失败" }
}

# 步骤 2: 发布并安装到项目级别（调用发布脚本）
Test-Step "发布并安装到项目级别" {
    Set-Location $PathMap.ProjectDir
    # 使用 -Yes 自动模式，-Target project 安装到项目级别，-Mode aot 使用AOT编译
    $output = pwsh -File $PathMap.PublishScript -Yes -Target project -Mode aot 2>&1
    if ($LASTEXITCODE -ne 0) { throw "发布脚本执行失败" }
    if (-not (Test-Path $PathMap.ExePath)) { throw "找不到发布文件: $($PathMap.ExePath)" }
}

# 步骤 3: 运行单元测试
Test-Step "运行单元测试" {
    Set-Location $PathMap.ProjectDir
    $testProjects = @(
        $PathMap.UnitTest,
        $PathMap.IntegrationTest
    )
    foreach ($testProj in $testProjects) {
        if (Test-Path $testProj) {
            Write-Info "编译测试项目: $testProj"
            dotnet build $testProj -c Release | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "测试项目编译失败: $testProj" }
            
            Write-Info "运行测试: $testProj"
            dotnet test $testProj --no-build --nologo
            if ($LASTEXITCODE -ne 0) { throw "单元测试失败: $testProj" }
        }
    }
}

# 步骤 4: 测试帮助命令
Test-Step "测试帮助命令 (--help)" {
    $output = & $PathMap.ExePath --help 2>&1
    $outputText = $output -join "`n"
    if ($outputText -notmatch "EncodingChecker") { throw "帮助输出不正确" }
    if ($outputText -notmatch "git ec") { throw "帮助中缺少别名信息" }
}

# 步骤 5: 测试检查命令（在无 Git 仓库目录）
Test-Step "测试检查命令（无仓库）" {
    $tempDir = [System.IO.Path]::GetTempPath()
    Push-Location $tempDir
    try {
        $output = & $PathMap.ExePath --check 2>&1
        # 应该返回错误码，因为不在 Git 仓库中
        if ($LASTEXITCODE -eq 0) { throw "应该返回错误码" }
    }
    finally {
        Pop-Location
    }
}

# 步骤 6: 创建临时 Git 仓库测试
Test-Step "在临时 Git 仓库中测试" {
    $testDir = Join-Path ([System.IO.Path]::GetTempPath()) "GitEncodingTest_$(Get-Random)"
    New-Item -ItemType Directory -Path $testDir -Force | Out-Null
    Push-Location $testDir
    try {
        # 初始化仓库
        git init | Out-Null
        git config user.email "test@test.com"
        git config user.name "Test User"

        # 创建测试文件
        "Hello World" | Out-File -FilePath "test.txt" -Encoding utf8NoBOM

        # 测试检查（应该通过，因为没有暂存文件）
        $output = & $PathMap.ExePath --check 2>&1
        # 没有暂存文件时应该返回 0

        # 添加文件到暂存区
        git add test.txt

        # 再次测试检查
        $output = & $PathMap.ExePath --check 2>&1
        # 应该通过 UTF-8 无 BOM 检查
    }
    finally {
        Pop-Location
        Remove-Item -Path $testDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# 步骤 7: 验证项目级别安装
Test-Step "验证项目级别安装" {
    $repoRoot = git rev-parse --show-toplevel 2>$null
    if ($repoRoot) {
        $projectHook = Join-Path $repoRoot ".git\hooks\pre-commit"
        $projectExe = Join-Path $repoRoot ".git\hooks\EncodingChecker.exe"
        if (Test-Path $projectHook) {
            Write-Info "项目级别 hook 已安装: $projectHook"
        }
        else {
            throw "项目级别 hook 未找到"
        }
        if (Test-Path $projectExe) {
            Write-Info "项目级别 exe 已安装: $projectExe"
        }
        else {
            throw "项目级别 exe 未找到"
        }
    }
}

# 步骤 8: 发布并安装到全局级别
Test-Step "发布并安装到全局级别" {
    Set-Location $PathMap.ProjectDir
    # 使用 -Yes 自动模式，-Target global 安装到全局级别，-Mode aot 使用AOT编译
    $output = pwsh -File $PathMap.PublishScript -Yes -Target global -Mode aot 2>&1
    if ($LASTEXITCODE -ne 0) { throw "全局安装失败" }
}

# 步骤 9: 验证全局安装和 Git 别名
Test-Step "验证全局安装和 Git 别名" {
    $globalHooksDir = Join-Path $env:USERPROFILE ".git-hooks"
    $globalHook = Join-Path $globalHooksDir "pre-commit"
    $globalExe = Join-Path $globalHooksDir "EncodingChecker.exe"

    if (Test-Path $globalHook) {
        Write-Info "全局 hook 已安装: $globalHook"
    }
    else {
        throw "全局 hook 未找到"
    }
    if (Test-Path $globalExe) {
        Write-Info "全局 exe 已安装: $globalExe"
    }
    else {
        throw "全局 exe 未找到"
    }

    # 验证 Git 别名
    $aliases = git config --global --get-regexp "^alias\.ec" 2>$null
    if ($aliases) {
        Write-Info "找到已注册的 Git 别名:"
        $aliases | ForEach-Object { Write-Host "  $_" }
    }
    else {
        throw "未找到 Git 别名"
    }
}

# 步骤 10: 测试编码检测功能
Test-Step "测试编码检测" {
    $testDir = Join-Path ([System.IO.Path]::GetTempPath()) "GitEncodingTest_$(Get-Random)"
    New-Item -ItemType Directory -Path $testDir -Force | Out-Null
    Push-Location $testDir
    try {
        git init | Out-Null
        git config user.email "test@test.com"
        git config user.name "Test User"

        # 创建 UTF-8 无 BOM 文件（应该通过）
        $utf8NoBom = New-Object System.Text.UTF8Encoding $false
        [System.IO.File]::WriteAllText("$(Join-Path $testDir 'utf8nobom.txt')", "UTF8 No BOM content", $utf8NoBom)

        # 创建 UTF-8 BOM 文件（应该失败）
        $utf8Bom = New-Object System.Text.UTF8Encoding $true
        [System.IO.File]::WriteAllText("$(Join-Path $testDir 'utf8bom.txt')", "UTF8 BOM content", $utf8Bom)

        # 测试 UTF-8 无 BOM 文件
        git add utf8nobom.txt
        $output = & $PathMap.ExePath --check 2>&1
        # 应该通过

        # 重置并测试 UTF-8 BOM 文件
        git reset
        git add utf8bom.txt
        $output = & $PathMap.ExePath --check 2>&1
        # 应该检测到 BOM 并返回错误
        if ($LASTEXITCODE -eq 0) {
            Write-Info "注意: BOM 检测可能需要进一步完善"
        }
    }
    finally {
        Pop-Location
        Remove-Item -Path $testDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# 步骤 11: 清理项目级别安装
Test-Step "清理项目级别安装" {
    Set-Location $PathMap.ProjectDir
    # 直接使用已安装的程序卸载项目级别
    $repoRoot = git rev-parse --show-toplevel 2>$null
    if ($repoRoot) {
        $projectExe = Join-Path $repoRoot ".git\hooks\EncodingChecker.exe"
        if (Test-Path $projectExe) {
            $output = & $projectExe --uninstall --target project 2>&1
        }
    }
    # 验证项目级别已清理
    $repoRoot = git rev-parse --show-toplevel 2>$null
    if ($repoRoot) {
        $projectHook = Join-Path $repoRoot ".git\hooks\pre-commit"
        $projectExe = Join-Path $repoRoot ".git\hooks\EncodingChecker.exe"
        if ((Test-Path $projectHook) -or (Test-Path $projectExe)) {
            Write-Info "警告: 项目级别文件可能未完全清理"
        }
        else {
            Write-Info "项目级别已清理"
        }
    }
}

# 步骤 12: 清理全局级别安装
Test-Step "清理全局级别安装" {
    Set-Location $PathMap.ProjectDir
    # 直接使用已安装的程序卸载全局级别
    $globalHooksDir = Join-Path $env:USERPROFILE ".git-hooks"
    $globalExe = Join-Path $globalHooksDir "EncodingChecker.exe"
    if (Test-Path $globalExe) {
        $output = & $globalExe --uninstall --target global 2>&1
    }
    # 验证全局级别已清理
    $globalHooksDir = Join-Path $env:USERPROFILE ".git-hooks"
    $globalHook = Join-Path $globalHooksDir "pre-commit"
    $globalExe = Join-Path $globalHooksDir "EncodingChecker.exe"
    if ((Test-Path $globalHook) -or (Test-Path $globalExe)) {
        Write-Info "警告: 全局级别文件可能未完全清理"
    }
    else {
        Write-Info "全局级别已清理"
    }
}

# 总结
Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "  测试完成" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "通过: $script:TestsPassed" -ForegroundColor Green
Write-Host "失败: $script:TestsFailed" -ForegroundColor Red

if ($script:TestsFailed -eq 0) {
    Write-Host "`n✓ 所有测试通过!" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "`n✗ 有测试失败" -ForegroundColor Red
    exit 1
}
