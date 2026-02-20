# Convert Chinese punctuation to English punctuation
param([string]$Directory = ".")

# Chinese to English punctuation mapping using Unicode escape sequences
$map = @{}
$map[[char]0xFF0C] = ","   # 
$map[[char]0x3002] = "."   # 
$map[[char]0xFF1B] = ";"   # 
$map[[char]0xFF1A] = ":"   # 
$map[[char]0x3001] = ","   # 
$map[[char]0xFF1F] = "?"   # 
$map[[char]0xFF01] = "!"   # 
$map[[char]0x201C] = '"'   # 
$map[[char]0x201D] = '"'   # 
$map[[char]0x2018] = "'"   # 
$map[[char]0x2019] = "'"   # 
$map[[char]0xFF08] = "("   # 
$map[[char]0xFF09] = ")"   # 
$map[[char]0x3010] = "["   # 
$map[[char]0x3011] = "]"   # 
$map[[char]0x300A] = "<"   # 
$map[[char]0x300B] = ">"   # 
$map[[char]0x2014] = "-"   # 

$files = Get-ChildItem -Path $Directory -Filter "*.md" -Recurse
$total = 0
$modified = 0

foreach ($file in $files) {
    $total++
    $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
    $original = $content
    
    foreach ($key in $map.Keys) {
        $content = $content.Replace($key, $map[$key])
    }
    
    if ($content -ne $original) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        $modified++
        Write-Host "Modified: $($file.FullName)" -ForegroundColor Green
    }
}

Write-Host "Done!" -ForegroundColor Cyan
Write-Host "Total: $total, Modified: $modified" -ForegroundColor White
