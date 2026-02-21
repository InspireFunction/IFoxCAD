Get-ChildItem -Path "g:\IFox工程\acad_IFoxCAD_35\.trae\agents","g:\IFox工程\acad_IFoxCAD_35\.trae\rules" -Filter "*.md" -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw -Encoding UTF8
    if ($content -match "小小") {
        $newContent = $content -replace "小小", "妈妈"
        [System.IO.File]::WriteAllText($_.FullName, $newContent, [System.Text.UTF8Encoding]::new($false))
        Write-Host "已替换: $($_.FullName)"
    }
}
