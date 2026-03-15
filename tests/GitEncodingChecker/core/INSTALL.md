# GitEncodingChecker 安装说明

## 安装方式

GitEncodingChecker 支持两种安装方式：

### 1. 项目级别安装（推荐团队协作）

仅在当前 Git 仓库生效，不影响其他仓库。

```powershell
# 运行发布脚本
pwsh -File 发布.ps1

# 选择 [1] 项目级别
```

**安装位置**：
- 可执行文件：`.git/hooks/EncodingChecker.exe`
- Hook 脚本：`.git/hooks/pre-commit`

**生效方式**：
- 在当前仓库执行 `git commit` 时自动触发编码检查
- 检查失败会阻止提交

---

### 2. 全局级别安装（推荐个人使用）

在本机所有 Git 仓库生效。

```powershell
# 运行发布脚本
pwsh -File 发布.ps1

# 选择 [2] 全局级别
```

**安装位置**：
- 可执行文件：`~/.git-hooks/EncodingChecker.exe`
- Hook 脚本：`~/.git-hooks/pre-commit`

**生效方式**：
- 通过设置 `core.hooksPath` 指向 `~/.git-hooks/`
- 本机所有 Git 仓库在 `git commit` 时自动触发检查

---

## 核心机制

### 不再使用 Git 别名

旧版本通过修改 `.gitconfig` 添加 Git 别名（如 `git ec`）来调用工具。

**新版本改用 Git Hook 机制**：
- 安装时创建 `pre-commit` hook 脚本
- Git 在 `commit` 前自动执行 hook
- 无需用户手动输入命令

### 全局安装的 Hook 链式调用

全局安装时，hook 脚本会：

1. **先执行 EncodingChecker 检查**
2. **再调用仓库自己的 pre-commit**（如果存在）

这样不会破坏仓库已有的 hook。

**全局 hook 内容示例**：

```sh
#!/bin/sh
# EncodingChecker Global Hook

SCRIPT_DIR="/C/Users/用户名/.git-hooks"
EXE_PATH="$SCRIPT_DIR/EncodingChecker.exe"

# 1. 运行 EncodingChecker 检查
if [ -f "$EXE_PATH" ]; then
    MSYS_NO_PATHCONV=1 cmd /c ""$EXE_PATH""
    RESULT=$?
    if [ $RESULT -ne 0 ]; then
        echo "编码检查失败，提交已取消"
        exit $RESULT
    fi
fi

# 2. 链式调用仓库自己的 pre-commit
REPO_HOOK="$(git rev-parse --git-dir)/hooks/pre-commit"
if [ -f "$REPO_HOOK" ] && [ "$REPO_HOOK" != "$0" ]; then
    "$REPO_HOOK"
    exit $?
fi

exit 0
```

---

## 系统要求

- **Git 版本**: 2.9+（支持 `core.hooksPath`）
- **操作系统**: Windows 10/11
- **PowerShell**: 7.0+（用于运行发布脚本）

---

## 卸载

```powershell
# 运行发布脚本
pwsh -File 发布.ps1

# 选择 [2] 清理卸载
# 然后选择清理目标：项目/全局/全部
```

---

## 手动安装（高级）

如果不想使用发布脚本，可以手动安装：

### 项目级别

```bash
# 1. 复制文件到 hooks 目录
cp EncodingChecker.exe .git/hooks/

# 2. 创建 pre-commit hook
cat > .git/hooks/pre-commit << 'EOF'
#!/bin/sh
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
EXE_PATH="$SCRIPT_DIR/EncodingChecker.exe"
if [ -f "$EXE_PATH" ]; then
    MSYS_NO_PATHCONV=1 cmd /c "$EXE_PATH"
    exit $?
else
    echo "错误: 找不到 EncodingChecker.exe"
    exit 1
fi
EOF

# 3. 设置编码配置
git config core.autocrlf false
git config i18n.commitencoding utf-8
```

### 全局级别

```bash
# 1. 创建全局 hooks 目录
mkdir -p ~/.git-hooks

# 2. 复制文件
cp EncodingChecker.exe ~/.git-hooks/

# 3. 创建 pre-commit hook（支持链式调用）
# （参考上面的全局 hook 内容）

# 4. 设置全局 hooks 路径
git config --global core.hooksPath "~/.git-hooks"

# 5. 设置全局编码配置
git config --global core.autocrlf false
git config --global i18n.commitencoding utf-8
```

---

## 跳过检查

如果某次提交需要跳过编码检查：

```bash
git commit --no-verify -m "你的提交信息"
```

---

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
