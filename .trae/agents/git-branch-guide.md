---
trigger: Git分支,分支规范,代码合并
---

# Git分支规范与工作流

## 分支结构
```
main (线上稳定版)
  ├── dev (集成测试版)
  │    ├── task/20260222-001-功能A (任务分支)
  │    │    └── 开发完成 -> 合并到dev
  │    ├── task/20260222-002-功能B (任务分支)
  │    │    └── 开发完成 -> 合并到dev
  │    └── (其他任务分支...)
  └── agent/assistant-N (xx君专属分支,可选)
```

---

## 分支说明

### 1. main 分支
- 用途: 线上稳定版本
- 保护级别: 🔴 最高
- 谁可以操作: 仅妈妈可以合并
- 合并条件: 
  - dev分支测试通过
  - 代码审查完成
  - 无已知严重BUG

### 2. dev 分支
- 用途: 集成测试版本
- 保护级别: 🟡 中
- 谁可以操作: 妈妈
- 合并条件:
  - 任务分支完成
  - 单元测试通过
  - 无编译错误

### 3. task/ 分支 (任务分支)
- 命名格式: `task/YYYYMMDD-NNN-简短描述`
- 示例: 
  - `task/20260222-001-添加CAD命令`
  - `task/20260222-002-修复内存泄漏`
- 创建者: 妈妈
- 生命周期: 
  - 创建 -> 开发 -> 测试 -> 合并 -> 删除
- 谁可以操作: 被分配的xx君

### 4. agent/ 分支 (君专属分支,可选)
- 命名格式: `agent/assistant-N`
- 示例: 
  - `agent/assistant-2` (测试君)
  - `agent/assistant-3` (开发君)
- 用途: 
  - 君个人工作区
  - 实验性代码
  - 临时保存
- 注意: 不强制要求,xx君可直接在task分支工作

---

## 标准工作流程

### 场景1: 正常任务开发

```bash
# 1. 妈妈创建任务分支
git checkout dev
git pull origin dev
git checkout -b task/20260222-001-添加CAD命令

# 2. 君开发代码
# ... 编写代码 ...
git add .
git commit -m "feat: 实现CAD命令功能"

# 3. 君提交到远程
git push origin task/20260222-001-添加CAD命令

# 4. 妈妈合并到dev
git checkout dev
git merge task/20260222-001-添加CAD命令
git push origin dev

# 5. 妈妈删除任务分支
git branch -d task/20260222-001-添加CAD命令
git push origin --delete task/20260222-001-添加CAD命令
```

### 场景2: 回滚操作

当任务失败需要回滚时:

```bash
# 1. 妈妈执行回滚
git checkout task/20260222-001-添加CAD命令
git reset --hard HEAD^  # 回滚到上一个提交

# 2. 君重新开发
# ... 修复问题 ...
git add .
git commit -m "fix: 修复CAD命令问题"
git push origin task/20260222-001-添加CAD命令 --force

# 3. 妈妈重新合并
git checkout dev
git merge task/20260222-001-添加CAD命令
```

### 场景3: 发布到main

```bash
# 1. 妈妈合并dev到main
git checkout main
git merge dev
git tag v1.2.0
git push origin main
git push origin v1.2.0
```

---

## 分支保护规则

| 分支 | 保护级别 | 谁可以推送 | 谁可以合并 |
|------|----------|------------|------------|
| main | 🔴 最高 | 无人 | 妈妈 |
| dev | 🟡 中 | 妈妈 | 妈妈 |
| task/* | 🟢 低 | 被分配的君 | 妈妈 |
| agent/* | ⚪ 无 | 对应君 | 对应君 |

---

## 提交信息规范

### 格式
```
<type>: <subject>

<body>

<footer>
```

### Type说明
- feat: 新功能
- fix: 修复BUG
- docs: 文档更新
- style: 代码格式(不影响功能)
- refactor: 重构
- test: 测试相关
- chore: 构建/工具相关

### 示例
```
feat: 添加CAD图形选择功能

- 实现矩形选择框
- 支持多选模式
- 添加选择事件回调

Closes #123
```

---

## 冲突解决

### 君解决冲突
```bash
# 1. 获取最新dev分支
git checkout dev
git pull origin dev

# 2. 切换到任务分支并合并dev
git checkout task/20260222-001-xxx
git merge dev

# 3. 解决冲突
# ... 手动编辑冲突文件 ...
git add .
git commit -m "merge: 解决与dev分支的冲突"
git push origin task/20260222-001-xxx
```

### 妈妈合并时冲突
```bash
# 如果妈妈合并时遇到冲突,退回给君
git merge --abort
git checkout dev

# 通知君解决冲突
```

---

## 注意事项

1. 禁止直接向main提交: 所有代码必须通过dev分支
2. 及时删除已完成的分支: 避免分支过多
3. 定期同步dev分支: 开发前先pull最新dev
4. 提交前自测: 确保代码能编译通过
5. 写清楚提交信息: 方便后续追溯
