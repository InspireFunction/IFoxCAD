# Register AutoCAD plugin to all installed versions
param(
    [string]$AppName = "",
    [string]$DllPath = ""
)

if ([string]::IsNullOrEmpty($AppName)) {
    Write-Error "[RegisterAcadPlugin]AppName parameter is required"
    exit 1
}

if ([string]::IsNullOrEmpty($DllPath)) {
    Write-Error "[RegisterAcadPlugin]DllPath parameter is required"
    exit 1
}

$basePath = "Registry::HKEY_CURRENT_USER\SOFTWARE\Autodesk\AutoCAD"
$registered = @()

if (Test-Path $basePath) {
    Get-ChildItem $basePath | ForEach-Object {
        $versionName = $_.PSChildName
        Get-ChildItem $_.PSPath | ForEach-Object {
            $localeName = $_.PSChildName
            $appPath = Join-Path $_.PSPath "Applications"
            if (Test-Path $appPath) {
                $regPath = "HKCU\SOFTWARE\Autodesk\AutoCAD\$versionName\$localeName\Applications\$AppName"
                reg add "$regPath" /v NAME /t REG_SZ /d "$AppName" /f | Out-Null
                reg add "$regPath" /v LOADCTRLS /t REG_DWORD /d 2 /f | Out-Null
                reg add "$regPath" /v MANAGED /t REG_DWORD /d 1 /f | Out-Null
                reg add "$regPath" /v LOADER /t REG_SZ /d "$DllPath" /f | Out-Null
                $registered += "$versionName ($localeName)"
                Write-Host "[RegisterAcadPlugin]Registered: $versionName ($localeName)"
            }
        }
    }
}

if ($registered.Count -eq 0) {
    Write-Warning "[RegisterAcadPlugin]No AutoCAD versions found"
} else {
    Write-Host "[RegisterAcadPlugin]Total registered: $($registered.Count) versions"
}
