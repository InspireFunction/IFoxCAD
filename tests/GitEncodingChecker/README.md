# EncodingChecker - Git 编码检查工具

## 功能

检查暂存区文件的编码是否为 UTF-8 无 BOM，行尾是否符合项目配置。

**特殊规则**：
- `.ps1` / `.psm1` 文件跳过编码检查（PowerShell 脚本必须是UTF8BOM才能允许，但是提交文件时候不强制要求）
- 其他文本文件使用 UTF-8 无 BOM

## 安装

```powershell
pwsh -File 发布.ps1
```

## 使用

```bash
git commit -m "xxx"          # 自动检查编码
git ec                       # 手动检查编码
git ec-fix                   # 修复编码问题
git ecc -m "msg"             # 修复编码并提交
git ec-commit -m "msg"       # 跳过检查强制提交
```

## 已知问题

### 2026-03-13: git commit 卡死/报错找不到 dll

**问题现象**：

```
The application to execute does not exist: 'G:\...\EncodingChecker.dll'.
```

**原因**：
发布脚本未使用单文件模式，生成的 exe 需要配套 dll，但脚本只复制了 exe。

**解决方案**：
修改 `发布.ps1`，使用单文件发布模式：

```powershell
dotnet publish EncodingChecker.csproj -c Release -r win-x64 --self-contained true `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:EnableCompressionInSingleFile=true
```

### AOT 编译问题

项目配置了 `EnablePublishAot=true`，但需要安装 native-aot workload。

如果 AOT 不可用，使用单文件发布是替代方案。
