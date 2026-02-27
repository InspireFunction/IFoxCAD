---
trigger: always_on
---

# Git操作规范
> 详细的Git操作流程和规范要求

## 1. Git分支管理策略

### 1.1 分支命名规范

统一使用以下格式：
```
{来源分支}_task_{任务名}_{YYYYMMDD}_{NNN}
```

| 组成部分 | 说明 | 示例 |
|----------|------|------|
| 来源分支名 | 基于哪个分支创建 | dev, main |
| task | 固定标识 | task |
| 任务名称 | 简短描述 | fix-memory-leak, add-layer-command |
| YYYYMMDD | 创建日期 | 20260224 |
| NNN | 当日序号 | 001, 002 |

**完整示例**：`dev_task_fix-memory-leak_20260224_001`

### 1.2 分支创建流程
```bash
# 1. 确保在dev分支（或main分支）
$ git checkout dev

# 2. 拉取最新代码
$ git pull origin dev

# 3. 创建新分支
$ git checkout -b dev_task_fix-memory-leak_20260224_001

# 4. 推送分支到远程（由爸爸执行）
# git push -u origin dev_task_fix-memory-leak_20260224_001
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
$ git merge dev_task_fix-memory-leak_20260224_001

# 5. 推送到远程（由爸爸执行）
# git push origin dev

# 6. 删除已合并的分支
$ git branch -d dev_task_fix-memory-leak_20260224_001
# git push origin --delete dev_task_fix-memory-leak_20260224_001（由爸爸执行）
```

## 2. 提交信息规范

### 2.1 提交信息格式
```
<type>(<scope>): <subject>

<body>

<footer>
```

### 2.2 提交类型(type)
| 前缀 | 用途 | 示例 |
|------|------|------|
| `feat:` | 新功能(feature) | `feat: 添加图层管理功能` |
| `fix:` | 修复BUG | `fix: 修复内存泄漏问题` |
| `docs:` | 文档更新 | `docs: 更新API说明` |
| `style:` | 代码格式调整 | `style: 统一代码缩进` |
| `refactor:` | 代码重构 | `refactor: 优化数据库查询` |
| `test:` | 测试相关 | `test: 添加单元测试` |
| `perf:` | 性能优化 | `perf: 提升渲染速度` |
| `chore:` | 构建过程或辅助工具的变动 | `chore: 更新依赖包` |

### 2.3 提交示例
```
feat(CAD): 添加图形选择功能

- 实现矩形选择框
- 支持多选和单选模式
- 添加选择事件回调

Closes #123
```

### 2.4 其他要求
1. 不用带对话框的命令，用简短中文提交
2. 不用 `git diff`（太慢）
3. 不用 `git push`（爸爸用）
4. 提交前编译检查，有异常则停止
5. 示例：

```cmd
git status
git add .
git commit -m "feat: 添加图层管理功能"
```

## 3. Git操作流程（妈妈专用）

### 3.1 妈妈创建任务分支
```bash
# 1. 切换到dev分支
$ git checkout dev

# 2. 拉取最新代码
$ git pull origin dev

# 3. 创建新分支
$ git checkout -b dev_task_xxx_20260224_001
```

### 3.2 码农君开发提交
```bash
$ git add .
$ git commit -m "feat: xxx"
# git push 由爸爸执行
```

### 3.3 妈妈合并分支（测试通过后）
```bash
# 1. 切换到dev分支
$ git checkout dev

# 2. 合并功能分支
$ git merge dev_task_xxx_20260224_001

# 3. git push 由爸爸执行
```

### 3.4 妈妈清理分支
```bash
$ git branch -d dev_task_xxx_20260224_001
# git push --delete 由爸爸执行
```

## 4. 回滚操作规范

### 4.1 回滚触发条件
- 连续2次编译失败
- 功能逻辑严重错误
- 用户明确要求的回滚
- 妈妈判定需要回滚

### 4.2 回滚操作流程
```bash
# 1. 记录当前状态
$ git status

# 2. 查看提交历史
$ git log --oneline -10

# 3. 执行回滚到上一个提交
$ git reset --hard HEAD^  

# 4. 强制推送到远程（由爸爸执行）
# git push -f origin <branch-name>

# 5. 更新任务状态文件
echo "状态: rollback" >> task-state.md
```

### 4.3 回滚后处理
- 更新 `task-state.md` 状态为 `rollback`
- 分析回滚原因并记录
- 通知相关的xx君重新评估任务
- 制定新的执行计划

## 5. 代码审查要求

### 5.1 审查前自检
- [ ] 代码能够通过编译
- [ ] 所有测试用例通过
- [ ] 代码符合项目规范
- [ ] 添加了必要的注释
- [ ] 更新了相关文档

### 5.2 审查要点
- **功能完整性**: 是否实现了需求要求的所有功能
- **代码质量**: 逻辑是否清晰,边界条件是否处理
- **性能优化**: 是否存在性能瓶颈或可优化点
- **安全性**: 是否存在潜在的安全风险
- **可维护性**: 代码是否易于理解和维护

### 5.3 合并前确认
- [ ] 通过了所有自动化测试
- [ ] 代码审查意见已处理
- [ ] 文档已同步更新
- [ ] 任务状态已更新为完成

## 6. 版本标签管理

### 6.1 标签命名规范
- **正式发布**: `v1.0.0` (主版本.次版本.修订版本)
- **预发布**: `v1.0.0-beta.1`
- **内部测试**: `v1.0.0-alpha.1`

### 6.2 标签创建流程
```bash
# 1. 确保代码稳定
$ git checkout main
$ git pull origin main

# 2. 创建标签
$ git tag -a v1.0.0 -m "正式发布版本1.0.0"

# 3. 推送标签（由爸爸执行）
# git push origin v1.0.0
```

## 7. 协作冲突解决

### 7.1 冲突预防
- 经常拉取最新代码
- 小步快跑,频繁提交
- 及时沟通,避免多人同时修改同一文件

### 7.2 冲突处理流程
```bash
# 1. 拉取远程代码
$ git pull origin <branch-name>

# 2. 解决冲突后标记
$ git add <resolved-files>

# 3. 提交解决
$ git commit -m "resolve: 解决合并冲突"

# 4. 禁止消息推送到远程,由爸爸负责
```

## 8. 安全操作要求

### 8.1 禁止操作
- ❌ 禁止在main分支直接开发
- ❌ 禁止强制推送重要分支
- ❌ 禁止提交敏感信息(密码、密钥等)
- ❌ 禁止删除重要历史提交

### 8.2 必须操作
- ✅ 提交前必须编译检查
- ✅ 合并前必须代码审查
- ✅ 重要操作必须备份
- ✅ 异常情况必须记录
