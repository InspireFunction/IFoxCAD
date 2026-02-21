---
trigger: Git操作,分支管理,版本控制
---

# Git操作规范
> 详细的Git操作流程和规范要求

## 1. Git分支管理策略

### 1.1 分支命名规范
- **功能分支**: `feature/YYYYMMDD-功能描述`
- **修复分支**: `fix/YYYYMMDD-问题描述`
- **任务分支**: `task/YYYYMMDD-NNN-任务描述`
- **热修复分支**: `hotfix/YYYYMMDD-紧急描述`

### 1.2 分支创建流程
```bash
# 1. 确保在main分支
$ git checkout main

# 2. 拉取最新代码
$ git pull origin main

# 3. 创建新分支
$ git checkout -b task/20240222-001-CAD图形选择

# 4. 推送分支到远程
$ git push -u origin task/20240222-001-CAD图形选择
```

### 1.3 分支合并策略
```bash
# 1. 确保功能开发完成并测试通过
$ git add .
$ git commit -m "feat: 实现CAD图形选择功能"

# 2. 切换到目标分支
$ git checkout dev

# 3. 拉取最新代码
$ git pull origin dev

# 4. 合并功能分支
$ git merge --no-ff task/20240222-001-CAD图形选择

# 5. 推送到远程
$ git push origin dev

# 6. 删除已合并的分支
$ git branch -d task/20240222-001-CAD图形选择
$ git push origin --delete task/20240222-001-CAD图形选择
```

## 2. 提交信息规范

### 2.1 提交信息格式
```
<type>(<scope>): <subject>

<body>

<footer>
```

### 2.2 提交类型(type)
- **feat**: 新功能(feature)
- **fix**: 修复BUG
- **docs**: 文档更新
- **style**: 代码格式调整
- **refactor**: 代码重构
- **test**: 测试相关
- **chore**: 构建过程或辅助工具的变动

### 2.3 提交示例
```
feat(CAD): 添加图形选择功能

- 实现矩形选择框
- 支持多选和单选模式
- 添加选择事件回调

Closes #123
```

## 3. 回滚操作规范

### 3.1 回滚触发条件
- 连续2次编译失败
- 功能逻辑严重错误
- 用户明确要求的回滚
- 妈妈判定需要回滚

### 3.2 回滚操作流程
```bash
# 1. 记录当前状态
$ git status

# 2. 查看提交历史
$ git log --oneline -10

# 3. 执行回滚到上一个提交
$ git reset --hard HEAD^  

# 4. 强制推送到远程(谨慎使用)
$ git push -f origin <branch-name>

# 5. 更新任务状态文件
echo "状态: rollback" >> task-state.md
```

### 3.3 回滚后处理
- 更新 `task-state.md` 状态为 `rollback`
- 分析回滚原因并记录
- 通知相关的xx君重新评估任务
- 制定新的执行计划

## 4. 代码审查要求

### 4.1 审查前自检
- [ ] 代码能够通过编译
- [ ] 所有测试用例通过
- [ ] 代码符合项目规范
- [ ] 添加了必要的注释
- [ ] 更新了相关文档

### 4.2 审查要点
- **功能完整性**: 是否实现了需求要求的所有功能
- **代码质量**: 逻辑是否清晰,边界条件是否处理
- **性能优化**: 是否存在性能瓶颈或可优化点
- **安全性**: 是否存在潜在的安全风险
- **可维护性**: 代码是否易于理解和维护

### 4.3 合并前确认
- [ ] 通过了所有自动化测试
- [ ] 代码审查意见已处理
- [ ] 文档已同步更新
- [ ] 任务状态已更新为完成

## 5. 版本标签管理

### 5.1 标签命名规范
- **正式发布**: `v1.0.0` (主版本.次版本.修订版本)
- **预发布**: `v1.0.0-beta.1`
- **内部测试**: `v1.0.0-alpha.1`

### 5.2 标签创建流程
```bash
# 1. 确保代码稳定
$ git checkout main
$ git pull origin main

# 2. 创建标签
$ git tag -a v1.0.0 -m "正式发布版本1.0.0"

# 3. 推送标签
$ git push origin v1.0.0
```

## 6. 协作冲突解决

### 6.1 冲突预防
- 经常拉取最新代码
- 小步快跑,频繁提交
- 及时沟通,避免多人同时修改同一文件

### 6.2 冲突处理流程
```bash
# 1. 拉取远程代码
$ git pull origin <branch-name>

# 2. 解决冲突后标记
$ git add <resolved-files>

# 3. 提交解决
$ git commit -m "resolve: 解决合并冲突"

# 4. 推送到远程
$ git push origin <branch-name>
```

## 7. 安全操作要求

### 7.1 禁止操作
- ❌ 禁止在main分支直接开发
- ❌ 禁止强制推送重要分支
- ❌ 禁止提交敏感信息(密码、密钥等)
- ❌ 禁止删除重要历史提交

### 7.2 必须操作
- ✅ 提交前必须编译检查
- ✅ 合并前必须代码审查
- ✅ 重要操作必须备份
- ✅ 异常情况必须记录