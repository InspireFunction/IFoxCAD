#!/usr/bin/env pwsh
# 发布脚本 - PowerShell 版本 (重构版)
# 支持参数: -Yes, -Target <project|global>, -Mode <aot|single>, -LogFile <path>
# ai测试的时候可以
# cd "g:\Project\acad_IFoxCAD_35\tests\GitEncodingChecker"; echo "1" | pwsh -File 发布.ps1

# ============================================
# 参数解析 - 必须是脚本中第一个可执行语句
# ============================================
param(
    [switch]$Yes,           # 自动确认所有提示
    [string]$Target,        # 安装目标: project|global
    [string]$Mode,          # 编译模式: aot|single
    [string]$LogFile,       # 日志文件路径
    [switch]$Help           # 显示帮助
)

$ErrorActionPreference = "Stop"

# 设置 UTF-8 编码
$OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# 显示帮助
if ($Help) {
    Write-Host @"
发布脚本 - 用法:
  .\发布.ps1 [选项]

选项:
  -Yes              自动确认所有提示（非交互模式）
  -Target <target>  安装目标: project 或 global
  -Mode <mode>      编译模式: aot 或 single
  -LogFile <path>   日志文件路径
  -Help             显示此帮助信息

示例:
  .\发布.ps1                                    # 交互式安装
  .\发布.ps1 -Yes                               # 自动确认所有提示
  .\发布.ps1 -Target project -Mode aot          # 指定目标和模式
  .\发布.ps1 -Yes -LogFile "install.log"        # 自动模式并记录日志
"@ -ForegroundColor Cyan
    exit 0
}

# ============================================
# 配置常量
# ============================================
$script:Config = @{
    SourceExe = "bin\Release\win-x64\publish\EncodingChecker.exe"
    SourceHook = "pre-commit"
    GlobalDir = "$env:USERPROFILE\.git-encoding-checker"
    GlobalHooksDir = "$env:USERPROFILE\.git-hooks"
    BackupDir = "$env:USERPROFILE\.git-encoding-checker-backup"
}

# 检测是否通过管道运行或是自动模式
# 使用更可靠的方法检测管道输入
$script:IsPiped = [bool]($MyInvocation.PipelinePosition -gt 1) -or -not [Environment]::UserInteractive
# 如果没有 -Yes 参数且不是管道模式，则为交互模式
$script:IsAutoMode = $Yes

# 日志文件句柄
$script:LogHandle = $null

# ============================================
# 日志函数
# ============================================

function Initialize-Log {
    param([string]$LogPath)
    if ($LogPath) {
        $logDir = Split-Path -Parent $LogPath
        if ($logDir -and -not (Test-Path $logDir)) {
            New-Item -ItemType Directory -Path $logDir -Force | Out-Null
        }
        $script:LogHandle = $LogPath
        Write-Log "========================================"
        Write-Log "发布脚本启动 - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
        Write-Log "========================================"
    }
}

function Write-Log {
    param(
        [string]$Message,
        [string]$Level = "INFO"
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$Level] $Message"

    # 写入控制台
    switch ($Level) {
        "ERROR" { Write-Host $Message -ForegroundColor Red }
        "WARN"  { Write-Host $Message -ForegroundColor Yellow }
        "SUCCESS" { Write-Host $Message -ForegroundColor Green }
        default { Write-Host $Message }
    }

    # 写入日志文件
    if ($script:LogHandle) {
        Add-Content -Path $script:LogHandle -Value $logEntry -ErrorAction SilentlyContinue
    }
}

# ============================================
# 工具函数
# ============================================

function Show-Menu {
    param(
        [string]$Title,
        [hashtable]$Options,
        [string]$Default = "1"
    )

    # 自动模式下直接返回默认值
    if ($script:IsAutoMode) {
        Write-Log "自动模式: 选择默认值 $Default" "INFO"
        return $Default
    }

    Write-Host "`n$Title" -ForegroundColor Cyan
    $validOptions = @($Options.Keys | Sort-Object)
    foreach ($key in $validOptions) {
        Write-Host "  $key. $($Options[$key])"
    }
    $choice = Read-Host "`n请输入选项 ($($validOptions -join ', ')，默认 $Default)"
    if ($validOptions -contains $choice) {
        return $choice
    }
    return $Default
}

function Wait-EscOrEnter {
    # 自动模式或管道模式下直接返回
    if ($script:IsAutoMode) {
        return $false  # false = 不重新开始
    }
    Write-Host "`n按 ESC 退出 或 按回车重新开始..." -ForegroundColor Yellow
    while ($true) {
        $key = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
        if ($key.VirtualKeyCode -eq 27) {  # ESC
            return $false  # 退出
        }
        if ($key.VirtualKeyCode -eq 13) {  # Enter
            return $true   # 重新开始
        }
    }
}

function Test-GitRepository {
    $gitDir = git rev-parse --git-dir 2>$null
    if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrEmpty($gitDir)) {
        return $gitDir
    }
    return $null
}

function Get-HooksDirectory {
    param([string]$GitDir)
    return Join-Path $GitDir "hooks"
}

function Copy-Executable {
    param(
        [string]$Source,
        [string]$Destination
    )
    if (-not (Test-Path $Source)) {
        Write-Log "源文件不存在: $Source" "ERROR"
        return $false
    }

    # 检查目标文件是否已存在且被占用
    if (Test-Path $Destination) {
        try {
            $stream = [System.IO.File]::Open($Destination, 'Open', 'Read', 'None')
            $stream.Close()
            $stream.Dispose()
        }
        catch {
            Write-Log "目标文件被占用，无法覆盖: $Destination`n请关闭占用该文件的程序后重试。" "ERROR"
            return $false
        }
    }

    try {
        Copy-Item -Path $Source -Destination $Destination -Force -ErrorAction Stop
        $fileSize = (Get-Item $Destination).Length / 1MB
        Write-Log "已复制: $Source -> $Destination ($([math]::Round($fileSize, 2)) MB)" "SUCCESS"
        return $true
    }
    catch {
        Write-Log "复制文件失败: $_" "ERROR"
        return $false
    }
}

function Copy-HookFile {
    param(
        [string]$Source,
        [string]$Destination
    )
    if (Test-Path $Source) {
        Copy-Item -Path $Source -Destination $Destination -Force
        Write-Log "已复制: $Source -> $Destination" "SUCCESS"
    } else {
        Write-Log "pre-commit 文件不存在，跳过复制" "WARN"
    }
}

function Remove-FileSafely {
    param([string]$Path)
    if (Test-Path $Path) {
        Remove-Item -Path $Path -Force
        Write-Log "  已删除: $Path" "SUCCESS"
    }
}

# ============================================
# 版本检查和备份功能
# ============================================

function Get-InstalledVersion {
    param([string]$ExePath)
    if (Test-Path $ExePath) {
        try {
            $versionInfo = (Get-Item $ExePath).VersionInfo
            return $versionInfo.FileVersion
        }
        catch {
            return $null
        }
    }
    return $null
}

function Backup-ExistingInstallation {
    param(
        [string]$InstallDir,
        [string]$Level
    )
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupName = "backup_${Level}_${timestamp}"
    $backupPath = Join-Path $script:Config.BackupDir $backupName

    if (Test-Path $InstallDir) {
        try {
            if (-not (Test-Path $script:Config.BackupDir)) {
                New-Item -ItemType Directory -Path $script:Config.BackupDir -Force | Out-Null
            }
            Copy-Item -Path $InstallDir -Destination $backupPath -Recurse -Force
            Write-Log "已备份现有安装到: $backupPath" "SUCCESS"
            return $backupPath
        }
        catch {
            Write-Log "备份失败: $_" "WARN"
            return $null
        }
    }
    return $null
}

function Restore-FromBackup {
    param([string]$BackupPath)
    if ($BackupPath -and (Test-Path $BackupPath)) {
        try {
            $originalDir = $BackupPath -replace "backup_.*?_\d{8}_\d{6}$", "" -replace "\\$", ""
            if (Test-Path $originalDir) {
                Remove-Item -Path $originalDir -Recurse -Force
            }
            Copy-Item -Path $BackupPath -Destination $originalDir -Recurse -Force
            Write-Log "已从备份恢复: $BackupPath" "SUCCESS"
            return $true
        }
        catch {
            Write-Log "恢复失败: $_" "ERROR"
            return $false
        }
    }
    return $false
}

# ============================================
# 编译函数
# ============================================

function Invoke-Build {
    param([string]$BuildMode)

    switch ($BuildMode) {
        "single" {
            Write-Log "正在编译发布（单文件模式）..." "INFO"
            dotnet publish EncodingChecker.csproj -c Release -r win-x64 --self-contained true `
                /p:PublishSingleFile=true `
                /p:IncludeNativeLibrariesForSelfExtract=true `
                /p:EnableCompressionInSingleFile=true `
                /p:PublishAot=false
            return "单文件模式"
        }
        default {
            Write-Log "正在编译发布（AOT原生编译模式）..." "INFO"
            dotnet publish EncodingChecker.csproj -c Release
            return "AOT原生编译模式"
        }
    }
}

# ============================================
# 清理函数
# ============================================

function Clear-ProjectLocal {
    param([string]$SourceExe)

    Write-Log "正在清理当前项目..." "INFO"
    $gitDir = Test-GitRepository
    if (-not $gitDir) {
        Write-Log "  未找到 Git 仓库，跳过项目清理" "WARN"
        return
    }

    $hooksDir = Get-HooksDirectory -GitDir $gitDir
    $destExe = Join-Path $hooksDir "EncodingChecker.exe"
    $destHook = Join-Path $hooksDir "pre-commit"

    # 先卸载配置，再删除文件
    if (Test-Path $destExe) {
        Write-Log "  正在卸载项目配置，请稍候..." "INFO"
        try {
            $psi = New-Object System.Diagnostics.ProcessStartInfo
            $psi.FileName = $destExe
            $psi.Arguments = "--uninstall --target project"
            $psi.UseShellExecute = $false
            $psi.CreateNoWindow = $true
            $psi.RedirectStandardOutput = $true
            $psi.RedirectStandardError = $true
            $process = [System.Diagnostics.Process]::Start($psi)
            if ($process.WaitForExit(10000)) {  # 等待10秒
                if ($process.ExitCode -eq 0) {
                    Write-Log "  已卸载项目级别 Git 配置" "SUCCESS"
                }
            } else {
                $process.Kill()
                Write-Log "  卸载项目配置超时，将继续清理文件..." "WARN"
            }
        }
        catch {
            Write-Log "  卸载项目配置时出错: $_，将继续清理文件..." "WARN"
        }
    }

    # 逐个删除文件，每个删除后立即输出提示
    if (Test-Path $destExe) {
        Remove-Item -Path $destExe -Force
        Write-Log "  已删除: $destExe" "SUCCESS"
    }
    if (Test-Path $destHook) {
        Remove-Item -Path $destHook -Force
        Write-Log "  已删除: $destHook" "SUCCESS"
    }
}

function Clear-GlobalInstallation {
    param([string]$SourceExe)

    Write-Log "正在清理系统全局..." "INFO"
    # 使用 .git-hooks 作为全局安装目录
    $globalHooksDir = $script:Config.GlobalHooksDir
    $destExe = Join-Path $globalHooksDir "EncodingChecker.exe"
    $destHook = Join-Path $globalHooksDir "pre-commit"

    if (Test-Path $destExe) {
        Write-Log "  正在卸载全局配置，请稍候..." "INFO"
        try {
            $psi = New-Object System.Diagnostics.ProcessStartInfo
            $psi.FileName = $destExe
            $psi.Arguments = "--uninstall --target global"
            $psi.UseShellExecute = $false
            $psi.CreateNoWindow = $true
            $psi.RedirectStandardOutput = $true
            $psi.RedirectStandardError = $true
            $process = [System.Diagnostics.Process]::Start($psi)
            if ($process.WaitForExit(10000)) {  # 等待10秒
                if ($process.ExitCode -eq 0) {
                    Write-Log "  已卸载全局 Git 配置" "SUCCESS"
                }
            } else {
                $process.Kill()
                Write-Log "  卸载全局配置超时，将继续清理文件..." "WARN"
            }
        }
        catch {
            Write-Log "  卸载全局配置时出错: $_，将继续清理文件..." "WARN"
        }
    }

    # 逐个删除文件，每个删除后立即输出提示
    if (Test-Path $destExe) {
        Remove-Item -Path $destExe -Force
        Write-Log "  已删除: $destExe" "SUCCESS"
    }
    if (Test-Path $destHook) {
        Remove-Item -Path $destHook -Force
        Write-Log "  已删除: $destHook" "SUCCESS"
    }

    # 清理旧的 .git-encoding-checker 目录（如果存在）
    $oldGlobalDir = $script:Config.GlobalDir
    if (Test-Path $oldGlobalDir) {
        Remove-Item -Path $oldGlobalDir -Recurse -Force
        Write-Log "  已删除旧目录: $oldGlobalDir" "SUCCESS"
    }
}

# ============================================
# 安装函数
# ============================================

function Install-ToDirectory {
    param(
        [string]$SourceExe,
        [string]$SourceHook,
        [string]$DestDir
    )

    if (-not (Test-Path $DestDir)) {
        Write-Log "创建目录: $DestDir" "INFO"
        New-Item -ItemType Directory -Path $DestDir -Force | Out-Null
    }

    $destExe = Join-Path $DestDir "EncodingChecker.exe"
    $destHook = Join-Path $DestDir "pre-commit"

    if (-not (Copy-Executable -Source $SourceExe -Destination $destExe)) {
        return $null
    }
    Copy-HookFile -Source $SourceHook -Destination $destHook

    return $destExe
}

function Install-Global {
    param(
        [string]$SourceExe,
        [string]$SourceHook,
        [string]$Mode
    )

    Write-Log "正在安装到系统全局..." "INFO"
    # 使用 .git-hooks 作为全局安装目录（Git 全局配置标准目录）
    $globalHooksDir = $script:Config.GlobalHooksDir

    # 备份现有安装
    $backupPath = Backup-ExistingInstallation -InstallDir $globalHooksDir -Level "global"

    $destExe = Install-ToDirectory -SourceExe $SourceExe -SourceHook $SourceHook -DestDir $globalHooksDir
    if (-not $destExe) {
        # 安装失败，尝试恢复
        if ($backupPath) {
            Write-Log "安装失败，正在恢复备份..." "WARN"
            Restore-FromBackup -BackupPath $backupPath
        }
        Write-Log "全局安装失败！" "ERROR"
        exit 1
    }

    # 使用 --force --target global 直接安装，不再交互询问
    $installArgs = @("--install", "--force", "--target", "global")
    Write-Log "使用非交互模式安装全局配置" "INFO"

    # 执行安装命令，将输出直接显示到控制台
    & $destExe $installArgs 2>&1 | ForEach-Object { Write-Host $_ }
    $installExitCode = $LASTEXITCODE

    if ($installExitCode -eq 0) {
        Write-Log "发布完成！模式: $Mode，已全局安装到: $globalHooksDir" "SUCCESS"
        Write-Log "   所有 Git 仓库都会自动生效" "INFO"
    } else {
        # 安装失败，尝试恢复
        if ($backupPath) {
            Write-Log "安装失败，正在恢复备份..." "WARN"
            Restore-FromBackup -BackupPath $backupPath
        }
        Write-Log "全局安装失败！" "ERROR"
        exit 1
    }

    return $destExe
}

function Install-Project {
    param(
        [string]$SourceExe,
        [string]$SourceHook,
        [string]$Mode
    )

    Write-Log "正在安装到当前项目..." "INFO"
    $gitDir = Test-GitRepository
    if (-not $gitDir) {
        Write-Log "无法找到 Git 仓库目录！" "ERROR"
        exit 1
    }

    $hooksDir = Get-HooksDirectory -GitDir $gitDir

    # 备份现有安装
    $backupPath = Backup-ExistingInstallation -InstallDir $hooksDir -Level "project"

    $destExe = Install-ToDirectory -SourceExe $SourceExe -SourceHook $SourceHook -DestDir $hooksDir
    if (-not $destExe) {
        # 安装失败，尝试恢复
        if ($backupPath) {
            Write-Log "安装失败，正在恢复备份..." "WARN"
            Restore-FromBackup -BackupPath $backupPath
        }
        exit 1
    }

    Write-Log "正在安装项目级别配置..." "INFO"
    # 使用 --force --target project 直接安装，不再交互询问
    $installArgs = @("--install", "--force", "--target", "project")
    Write-Log "使用非交互模式安装项目级别配置" "INFO"
    # 执行安装命令，将输出直接显示到控制台
    & $destExe $installArgs 2>&1 | ForEach-Object { Write-Host $_ }
    $installExitCode = $LASTEXITCODE
    
    if ($installExitCode -eq 0) {
        Write-Log "项目级别配置安装完成！" "SUCCESS"
    } else {
        # 安装失败，尝试恢复
        if ($backupPath) {
            Write-Log "安装失败，正在恢复备份..." "WARN"
            Restore-FromBackup -BackupPath $backupPath
        }
        Write-Log "项目级别配置安装可能出现问题" "WARN"
    }

    Write-Log "发布完成！模式: $Mode，已安装到: $hooksDir" "SUCCESS"

    # 自动模式下跳过全局配置询问
    if ($script:IsAutoMode) {
        Write-Log "自动模式：跳过全局配置询问" "INFO"
        return $destExe
    }

    # 询问是否同时安装全局配置
    $globalChoice = Show-Menu `
        -Title "是否同时安装全局配置（本机所有 Git 仓库生效）？" `
        -Options @{
            "1" = "是 - 安装全局别名和编码配置"
            "2" = "否 - 仅保持项目级别安装（默认）"
        } `
        -Default "2"

    if ($globalChoice -eq "1") {
        Write-Log "正在安装全局配置..." "INFO"
        & $destExe --install-global
        if ($LASTEXITCODE -eq 0) {
            Write-Log "全局配置安装完成！" "SUCCESS"
        } else {
            Write-Log "全局配置安装可能出现问题" "WARN"
        }
    } else {
        Write-Log "跳过全局配置安装。如需稍后安装，请运行:" "INFO"
        Write-Log "  git ec-global  或  .git/hooks/EncodingChecker.exe --install-global" "INFO"
    }

    return $destExe
}

# ============================================
# 主程序
# ============================================

function Main {
    # 初始化日志
    Initialize-Log -LogPath $LogFile

    Write-Log "开始发布 EncodingChecker..." "INFO"

    # 处理命令行参数 -Target
    if ($Target) {
        $Target = $Target.ToLower()
        if ($Target -notin @("project", "global")) {
            Write-Log "无效的目标: $Target，使用 project" "WARN"
            $Target = "project"
        }
    }

    # 处理命令行参数 -Mode
    if ($Mode) {
        $Mode = $Mode.ToLower()
        if ($Mode -notin @("aot", "single")) {
            Write-Log "无效的模式: $Mode，使用 aot" "WARN"
            $Mode = "aot"
        }
    }

    # 选择操作模式
    $operationOptions = @{
        "1" = "发布安装"
        "2" = "清理卸载"
    }

    # 自动模式下默认选择发布安装
    if ($script:IsAutoMode) {
        $operation = "1"
        Write-Log "自动模式: 选择发布安装" "INFO"
    } else {
        $operation = Show-Menu `
            -Title "请选择操作：" `
            -Options $operationOptions `
            -Default "1"
    }

    # 清理模式
    if ($operation -eq "2") {
        $cleanOptions = @{
            "1" = "清理当前项目"
            "2" = "清理系统全局"
            "3" = "全部清理"
        }

        if ($script:IsAutoMode) {
            $cleanTarget = "1"
            Write-Log "自动模式: 选择清理当前项目" "INFO"
        } else {
            $cleanTarget = Show-Menu `
                -Title "请选择清理目标：" `
                -Options $cleanOptions `
                -Default "1"
        }

        $sourceExe = $script:Config.SourceExe

        if ($cleanTarget -eq "1" -or $cleanTarget -eq "3") {
            Clear-ProjectLocal -SourceExe $sourceExe
        }
        if ($cleanTarget -eq "2" -or $cleanTarget -eq "3") {
            Clear-GlobalInstallation -SourceExe $sourceExe
        }

        Write-Log "清理完成！" "SUCCESS"
        return  # 返回主循环
    }

    # 发布安装模式
    $installOptions = @{
        "1" = "当前项目 (.git/hooks) - 仅当前 Git 仓库生效"
        "2" = "系统全局 - 安装到用户目录，所有 Git 仓库生效"
    }

    # 使用命令行参数或显示菜单
    if ($Target -eq "global") {
        $installTarget = "2"
        Write-Log "命令行指定: 安装到系统全局" "INFO"
    } elseif ($Target -eq "project") {
        $installTarget = "1"
        Write-Log "命令行指定: 安装到当前项目" "INFO"
    } else {
        $installTarget = Show-Menu `
            -Title "请选择安装目标：" `
            -Options $installOptions `
            -Default "1"
    }

    $buildOptions = @{
        "1" = "AOT 原生编译 (推荐) - 文件小 (~2.6MB)，启动快，无运行时依赖"
        "2" = "单文件模式 - 包含运行时 (~60MB)，兼容性更好"
    }

    # 使用命令行参数或显示菜单
    if ($Mode -eq "single") {
        $buildMode = "2"
        Write-Log "命令行指定: 使用单文件模式" "INFO"
    } elseif ($Mode -eq "aot") {
        $buildMode = "1"
        Write-Log "命令行指定: 使用AOT原生编译模式" "INFO"
    } else {
        $buildMode = Show-Menu `
            -Title "请选择编译模式：" `
            -Options $buildOptions `
            -Default "1"
    }

    # 编译
    $mode = Invoke-Build -BuildMode $(if ($buildMode -eq "2") { "single" } else { "aot" })

    if ($LASTEXITCODE -ne 0) {
        Write-Log "发布失败！" "ERROR"
        return  # 返回主循环
    }

    $sourceExe = $script:Config.SourceExe
    $sourceHook = $script:Config.SourceHook

    if (-not (Test-Path $sourceExe)) {
        Write-Log "源文件不存在: $sourceExe" "ERROR"
        return  # 返回主循环
    }

    # 安装
    if ($installTarget -eq "2") {
        $destExe = Install-Global -SourceExe $sourceExe -SourceHook $sourceHook -Mode $mode
    } else {
        $destExe = Install-Project -SourceExe $sourceExe -SourceHook $sourceHook -Mode $mode
    }

    # 打印安装路径
    Write-Log "========================================" "INFO"
    Write-Log "实际安装路径:" "INFO"
    Write-Log "  $destExe" "INFO"
    Write-Log "========================================" "INFO"

    if ($script:LogHandle) {
        Write-Log "日志已保存到: $script:LogHandle" "INFO"
    }
}

# ============================================
# 主循环
# ============================================

# 自动模式下只执行一次
if ($script:IsAutoMode) {
    Main
} else {
    # 交互模式下循环执行，直到用户选择退出
    do {
        Main
        $restart = Wait-EscOrEnter
        if ($restart) {
            Write-Host "`n========================================" -ForegroundColor Cyan
            Write-Host "重新开始..." -ForegroundColor Cyan
            Write-Host "========================================`n" -ForegroundColor Cyan
        }
    } while ($restart)
}
