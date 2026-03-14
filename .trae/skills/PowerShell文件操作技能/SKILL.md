---
name: "PowerShell文件操作技能"
description: "指导如何正确使用PowerShell进行文件内容替换而不破坏文件格式。Invoke when user needs to replace text in files using PowerShell, especially when dealing with encoding and line ending issues."
---

# PowerShell文件操作技能

## 问题根源

之前使用PowerShell的 `Set-Content` 进行字符串替换时导致文件破坏，原因如下：

1. **`-NoNewline` 参数问题**：
   - `Set-Content -NoNewline` 会将整个文件内容合并为一行
   - 所有换行符被移除，导致文件格式完全破坏

2. **编码问题**：
   - `Set-Content` 默认使用系统默认编码
   - 如果原文件是UTF-8无BOM，可能被改为有BOM或其他编码

3. **字符串转义问题**：
   - PowerShell命令行中特殊字符（如 `"` 和 `'`）需要正确转义
   - 复杂的正则表达式容易出错

## 正确做法

### 方法1：使用 `Replace.ps1` 脚本（推荐）

项目中已有 `.trae\rules\replace.ps1` 脚本，专门用于处理文件替换：

```powershell
# 查看脚本用法
Get-Help .trae\rules\replace.ps1

# 使用示例
.trae\rules\replace.ps1 -Path "src\file.cs" -OldString "text" -NewString "replacement"
```

### 方法2：使用 `SearchReplace` 工具

对于代码文件，优先使用IDE的 `SearchReplace` 工具：

```xml
<tool>
  <name>SearchReplace</name>
  <arguments>
    <file_path>绝对路径</file_path>
    <old_str>要替换的内容</old_str>
    <new_str>新内容</new_str>
  </arguments>
</tool>
```

### 方法3：正确的PowerShell单行命令

如果必须用PowerShell，使用 `-Raw` 读取和正确编码：

```powershell
# 错误做法（会破坏格式）
(Get-Content 'file.cs') -replace 'old', 'new' | Set-Content 'file.cs' -NoNewline

# 正确做法1：保留换行符
$content = Get-Content 'file.cs' -Raw
$content = $content -replace 'old', 'new'
$content | Set-Content 'file.cs' -NoNewline

# 正确做法2：逐行处理
(Get-Content 'file.cs') | ForEach-Object { $_ -replace 'old', 'new' } | Set-Content 'file.cs'

# 正确做法3：指定编码
$content = Get-Content 'file.cs' -Raw
$content = $content -replace 'old', 'new'
[System.IO.File]::WriteAllText('file.cs', $content, [System.Text.UTF8Encoding]::new($false))
```

## 检查清单

使用PowerShell操作文件前，确认：

- [ ] 是否可以使用 `SearchReplace` 工具替代？
- [ ] 是否可以使用 `replace.ps1` 脚本？
- [ ] 是否需要保留原文件编码（UTF-8无BOM）？
- [ ] 是否需要处理多行内容？
- [ ] 操作后是否验证了文件格式正确？

## 教训总结

1. **永远不要**在管道中直接使用 `Set-Content -NoNewline` 处理多行文件
2. **优先使用** IDE提供的 `SearchReplace` 工具，它更安全
3. **使用脚本** `.trae\rules\replace.ps1` 处理复杂的替换需求
4. **操作后验证** 文件格式是否正确（换行、编码等）
