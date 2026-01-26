using IFoxCAD.Cad;

namespace JoinBoxAcad;

/// <summary>
/// 在位编辑操作处理器
/// </summary>
public class InPlaceEditHandler : IDisposable
{
    private readonly Document _document;
    private readonly EnhancedActionLogger _logger;
    private bool _refeditRun;
    private HashSet<ObjectId> _currentIds;
    private bool _IsDisposed;

    public InPlaceEditHandler(Document document, EnhancedActionLogger logger)
    {
        _document = document;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentIds = [];

        // 注册命令结束事件
        _document.CommandEnded += OnCommandEnded;
    }

    /// <summary>
    /// 命令结束事件处理
    /// </summary>
    private void OnCommandEnded(object sender, CommandEventArgs e)
    {
        DebugEx.Printl($"OnCommandEnded - {DateTime.Now}");

        // #260126a 使用了异步命令这里需要清理
        if (_logger.AsyncCmdsPop(e.GlobalCommandName))
        {
            //全部移除就恢复
            if (_logger.AsyncCmdsCount == 0 && _logger.IsExecutingUndoRedo)
                _logger.IsExecutingUndoRedo = false;
            return;
        }
        if (_logger.IsExecutingUndoRedo)
            return;

        var 撤销重做触发 = false;

        var cmdup = e.GlobalCommandName.ToUpper();
        switch (cmdup)
        {
            case "REFEDIT":
            {
                _refeditRun = true;
                // 命令后,扫描全图获取,多出来的就是在位编辑块内的
                var prompt = _document.Editor.SelectAll(FilterForHatch);
                if (prompt.Status == PromptStatus.OK)
                {
                    Clear();
                    var newIds = prompt.Value.GetObjectIds()
                        .AsParallel()
                        .Where(a => !_currentIds.Contains(a))
                        .ToList();

                    if (newIds.Count > 0)
                    {
                        // 创建在位编辑添加动作
                        var action = new InPlaceAddAction(newIds);
                        _logger.LogAction(action);

                        // 更新当前ID集合
                        WorkSetAdd(newIds);
                    }
                }
            }
            break;
            case "REFSET": // 加减在位编辑图元
            {
                // 命令历史的最后一行是:添加/删除
                // 才发现这样获取最后一行,可能会被事件打印影响

                var last = Env.GetVar("lastprompt").ToString();
                if (last is null)
                    return;
                // 完成后必然有上次选择集
                var prompt = _document.Editor.SelectPrevious();
                if (prompt.Status != PromptStatus.OK)
                    return;

                var selectedIds = prompt.Value.GetObjectIds();

                // 就是因为无法遍历到在位编辑的块内图元,只能进行布尔运算
                if (last.Contains("添加") || last.Contains("Added"))// 中英文cad
                {
                    if (!撤销重做触发)
                    {
                        // 创建在位编辑添加动作
                        var action = new InPlaceAddAction(selectedIds);
                        _logger.LogAction(action);
                    }
                    WorkSetAdd(selectedIds);
                    _currentIds.ExceptWith(selectedIds);
                    return;
                }
                if (last.Contains("删除") || last.Contains("Removed"))// 中英文cad
                {
                    if (!撤销重做触发)
                    {
                        // 创建在位编辑移除动作
                        var action = new InPlaceRemoveAction(selectedIds);
                        _logger.LogAction(action);
                    }
                    WorkSetRemove(selectedIds);
                    _currentIds.UnionWith(selectedIds);
                    return;
                }
            }
            break;
            case "REFCLOSE": // 保存块,清空集合
            {
                _refeditRun = false;
                Clear();
            }
            break;
        }
    }

    /// <summary>
    /// 过滤器
    /// </summary>
    private static SelectionFilter FilterForHatch => new([]);

    /// <summary>
    /// 清空集合
    /// </summary>
    private void Clear()
    {
        _currentIds.Clear();
    }

    /// <summary>
    /// 添加到工作集
    /// </summary>
    private void WorkSetAdd(IEnumerable<ObjectId> objectIds)
    {
        foreach (var id in objectIds)
        {
            _currentIds.Add(id);
        }
    }

    /// <summary>
    /// 从工作集移除
    /// </summary>
    private void WorkSetRemove(IEnumerable<ObjectId> objectIds)
    {
        foreach (var id in objectIds)
        {
            _currentIds.Remove(id);
        }
    }

    // 公共 Dispose 方法
    public void Dispose()
    {
        Dispose(true);
        // 阻止垃圾回收器调用析构函数
        GC.SuppressFinalize(this);
    }

    // 受保护的虚拟 Dispose 方法
    protected virtual void Dispose(bool disposing)
    {
        if (_IsDisposed)
            return;
        _IsDisposed = true;

        if (disposing)
        {
            // 释放托管资源
            _document.CommandEnded -= OnCommandEnded;
            Clear();
        }
    }
}
