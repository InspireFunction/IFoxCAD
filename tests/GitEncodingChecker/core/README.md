# EncodingChecker - Git 编码检查工具

## 功能

检查暂存区文件的编码是否为 UTF-8 无 BOM，行尾是否符合项目配置。

**特殊规则**：
- `.ps1` / `.psm1` 文件跳过编码检查（PowerShell 脚本必须是UTF8BOM才能允许，但是提交文件时候不强制要求）
- 其他文本文件使用 UTF-8 无 BOM

## 安装

### 项目级别安装（默认）

仅在当前 Git 仓库生效：

```powershell
pwsh -File 发布.ps1
# 选择 [1] 项目级别
```

### 全局安装（本机所有仓库生效）

```powershell
pwsh -File 发布.ps1
# 选择 [2] 全局级别
```

全局安装会：
- 将 exe 和 hook 复制到 `%USERPROFILE%\.git-hooks\`
- 设置 `git config --global core.hooksPath` 指向该目录
- 所有 Git 仓库自动触发编码检查

## 使用

```bash
git commit -m "xxx"          # 自动检查编码（通过 pre-commit hook）
git ec                       # 手动检查编码（如果安装了别名）
git ec-fix                   # 修复编码问题
```

**跳过检查**：
```bash
git commit --no-verify -m "msg"  # 跳过 pre-commit hook
```

## 技术方案（重要）

### 不修改 `.gitconfig` 的别名配置

**旧方案**：修改 `.gitconfig` 添加 Git 别名（如 `git ec`）
- 缺点：污染用户 Git 配置，需要手动输入命令

**新方案**：使用 **Git Hook** 机制
- 优点：自动触发，无需修改 `.gitconfig` 别名，零学习成本

### 核心技术：Git Hook + core.hooksPath

#### 项目级别
- 将 `pre-commit` hook 放入 `.git/hooks/` 目录
- **不修改任何 Git 配置**
- 仅当前仓库生效

#### 全局级别
- 将 `pre-commit` hook 放入 `%USERPROFILE%/.git-hooks/` 目录
- 仅修改 **`core.hooksPath`** 配置项
- **不添加任何 Git 别名**
- 本机所有仓库生效

### 关键技术点

| 技术 | 说明 | 版本要求 |
|------|------|----------|
| `core.hooksPath` | Git 2.9+ 支持的全局 hooks 路径配置 | Git **2.9+** |
| `pre-commit` hook | Git 原生支持的提交前钩子 | 所有版本 |
| Hook 链式调用 | 全局 hook 调用仓库本地 hook | Git 2.9+ |

### 全局 Hook 链式调用机制

全局安装时，hook 脚本会：

1. **先执行 EncodingChecker 检查**
2. **再调用仓库自己的 pre-commit**（如果存在）

这样不会破坏仓库已有的 hook。

**全局 hook 内容示例**：

```sh
#!/bin/sh
# 1. 运行 EncodingChecker 检查
# 2. 链式调用仓库自己的 pre-commit
REPO_HOOK="$(git rev-parse --git-dir)/hooks/pre-commit"
if [ -f "$REPO_HOOK" ] && [ "$REPO_HOOK" != "$0" ]; then
    "$REPO_HOOK"
    exit $?
fi
```

## 配置层级说明

| 配置项 | 项目级别 | 全局级别 |
|--------|----------|----------|
| 触发方式 | `git commit` 自动触发 | `git commit` 自动触发 |
| 作用范围 | 仅当前仓库 | 本机所有仓库 |
| exe 位置 | `.git/hooks/` | `%USERPROFILE%/.git-hooks/` |
| hook 位置 | `.git/hooks/pre-commit` | `%USERPROFILE%/.git-hooks/pre-commit` |
| Git 配置 | 无（直接放 hook） | `core.hooksPath` |

**注意**：如果同时安装项目级别和全局级别，commit 时会执行两次检查。
建议只选择一种安装方式。

## 系统要求

### 必需

| 组件 | 最低版本 | 说明 |
|------|----------|------|
| **Git** | **2.9+** | 必需 `core.hooksPath` 支持 |
| Windows | 10/11 | 64位系统 |
| PowerShell | 7.0+ | 用于运行发布脚本 |

### 检查 Git 版本

```bash
git --version
# 需要 2.9.0 或更高版本
```

### Git 2.9+ 新特性说明

`core.hooksPath` 是 Git 2.9 引入的配置项，允许：
- 指定一个**全局 hooks 目录**
- 该目录下的 hooks 会应用到**所有仓库**
- 配合 `pre-commit` hook 实现全局代码检查

## 已知问题与解决方案

### 问题 1: 同时安装项目级别和全局级别导致重复检查

**现象**：`git commit` 时编码检查执行两次

**原因**：项目 hook 和全局 hook 都触发了

**解决方案**：
- 个人使用：只安装全局级别
- 团队协作：每个仓库单独安装项目级别

### 问题 2: git commit 卡死/报错找不到 dll

**现象**：
```
The application to execute does not exist: 'G:\...\EncodingChecker.dll'.
```

**原因**：发布脚本未使用单文件模式，生成的 exe 需要配套 dll

**解决方案**：使用单文件发布模式（已修复）

### 问题 3: AOT 编译失败

**现象**：发布时编译错误

**解决方案**：发布脚本会自动处理，或使用单文件模式

## 故障排查

### 检查 core.hooksPath 是否设置

```bash
git config --global core.hooksPath
```

### 查看当前生效的 hook

```bash
# 项目级别
ls -la .git/hooks/pre-commit

# 全局级别
ls -la ~/.git-hooks/pre-commit
```

### 手动测试 hook

```bash
# 项目级别
.git/hooks/pre-commit

# 全局级别
~/.git-hooks/pre-commit
```

### 检查 Git 版本

```bash
git --version  # 需要 2.9+
```

## 卸载

```powershell
pwsh -File 发布.ps1
# 选择 [2] 清理卸载
# 然后选择清理目标：项目/全局/全部
```

## 手动安装（高级）

如果不想使用发布脚本，可以参考 [INSTALL.md](INSTALL.md) 进行手动安装。
