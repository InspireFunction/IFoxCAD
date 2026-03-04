# Unregister AutoCAD plugin from all versions
param(
    [string]$AppName = ""
)

if ([string]::IsNullOrEmpty($AppName)) {
    Write-Error "AppName parameter is required"
    exit 1
}

$basePath = "Registry::HKEY_CURRENT_USER\SOFTWARE\Autodesk\AutoCAD"
$removed = @()

if (Test-Path $basePath) {
    Get-ChildItem $basePath | ForEach-Object {
        $versionName = $_.PSChildName
        Get-ChildItem $_.PSPath | ForEach-Object {
            $localeName = $_.PSChildName
            $appRegPath = "HKCU\SOFTWARE\Autodesk\AutoCAD\$versionName\$localeName\Applications\$AppName"
            reg delete "$appRegPath" /f >$null 2>$null
            if ($LASTEXITCODE -eq 0) {
                $removed += "$versionName ($localeName)"
            }
        }
    }
}

if ($removed.Count -gt 0) {
    Write-Host "Cleaned up $($removed.Count) versions"
}
