---
name: 编译问题助手
description: 针对 .NET 项目编译失败场景的快速排查工具，通过指定参数的 dotnet build 命令定位代码级别的编译错误。
---

# 编译问题助手

## 技能描述
编译问题助手是针对 .NET 项目编译失败场景的快速排查工具，核心通过指定参数的 `dotnet build` 命令，跳过依赖还原和依赖检查，聚焦目标测试项目本身的编译问题，快速定位代码级别的编译错误。

## 功能
- 快速排查 .NET 项目编译失败问题
- 跳过依赖还原和依赖检查，专注于代码本身的编译错误
- 适用于测试项目和主项目的编译问题诊断

## 用法
当遇到项目无法编译（尤其是 tests/TestAcad08 测试项目）时，执行以下命令：

```bash
dotnet build --no-restore --no-dependencies tests/TestAcad08
```

## 参数说明
- `--no-restore`: 跳过依赖包还原步骤
- `--no-dependencies`: 跳过依赖项目的检查和构建
- `tests/TestAcad08`: 目标测试项目路径（可根据实际情况修改）

## 适用场景
- 当完整构建失败时，快速定位具体项目的编译错误
- 当依赖包问题导致构建失败，需要排除依赖干扰时
- 当需要快速验证代码修改是否会导致编译错误时

## 注意事项
- 此命令仅适用于 .NET 项目
- 执行前确保已安装 .NET SDK
- 如果项目确实存在依赖问题，此命令可能无法完全解决问题，仅用于定位代码本身的编译错误

## 示例
### 输入
```bash
dotnet build --no-restore --no-dependencies tests/TestAcad08
```

### 输出
```
Microsoft (R) Build Engine version 17.0.0-preview-21460-01+8f208e609 for .NET
Copyright (C) Microsoft Corporation. All rights reserved.

  Determining projects to restore...
  Skipping project "G:\IFox工程\acad_IFoxCAD_35\tests\TestAcad08\TestAcad08.csproj" because it was not found.
  Skipping project "G:\IFox工程\acad_IFoxCAD_35\tests\TestAcad08\TestAcad08.csproj" because it was not found.
  Generating MSBuild file G:\IFox工程\acad_IFoxCAD_35\tests\TestAcad08\obj\TestAcad08.csproj.nuget.g.props.
  Generating MSBuild file G:\IFox工程\acad_IFoxCAD_35\tests\TestAcad08\obj\TestAcad08.csproj.nuget.g.targets.
  Restore completed in 100.00 ms for G:\IFox工程\acad_IFoxCAD_35\tests\TestAcad08\TestAcad08.csproj.
  TestAcad08 -> G:\IFox工程\acad_IFoxCAD_35\tests\TestAcad08\bin\Debug\net35\TestAcad08.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.23
```