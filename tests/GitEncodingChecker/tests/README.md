# GitEncodingChecker 测试解决方案

此解决方案包含分离的测试项目：

## 项目结构

### 🔴 集成测试项目 (`tests/Integration/`)
- **项目文件**: `EncodingChecker.IntegrationTests.csproj`
- **测试内容**: 完整的用户工作流测试
  - 场景1: 用户工作流 - 检查通过 → 提交
  - 场景2: 用户工作流 - 检查失败 → 修复 → 重新检查 → 提交
  - 场景3: 用户工作流 - 混合换行符检测
  - 场景4: 用户工作流 - 空白行检测
  - 场景5: 用户工作流 - 多个文件批量处理
  - 场景6: 用户工作流 - 空暂存区
  - 场景7: 用户工作流 - 非文本文件跳过
  - 场景8: 用户工作流 - ecc 完整流程
  - 场景9: 用户工作流 - 安装/卸载钩子
  - 场景10: 用户工作流 - 帮助命令

### 🟢 单元测试项目 (`tests/`)
- **项目文件**: `EncodingChecker.Tests.csproj`
- **测试内容**: 纯单元测试
  - `EncodingCheckerTests.cs` - 编码检测功能的单元测试
  - `CommandHandlersTests.cs` - 命令处理器的单元测试
  - `GitPathResolverTests.cs` - Git路径解析的单元测试
  - `InstallManagerTests.cs` - 安装管理器的单元测试

## 运行测试

### 运行所有测试
```bash
# 从项目根目录执行
cd tests
dotnet test
```

### 单独运行集成测试
```bash
cd tests/Integration
dotnet test
```

### 单独运行单元测试
```bash
cd tests
dotnet test --filter Category!=Integration
```

## 测试分离的好处

1. **执行速度**: 单元测试快速执行，集成测试可以单独运行
2. **依赖隔离**: 集成测试需要Git环境和文件系统操作
3. **CI/CD优化**: 可以并行执行不同类型的测试
4. **调试便利**: 更容易定位问题类型