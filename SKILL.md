# 编译问题助手

## 1. 技能概述
编译问题助手是针对 .NET 项目编译失败场景的快速排查工具，核心通过指定参数的 `dotnet build` 命令，跳过依赖还原和依赖检查，聚焦目标测试项目本身的编译问题，快速定位代码级别的编译错误。

## 2. 核心命令
当遇到项目无法编译（尤其是 tests/TestAcad08 测试项目）时，执行以下命令：
```bash
dotnet build --no-restore --no-dependencies tests/TestAcad08
```