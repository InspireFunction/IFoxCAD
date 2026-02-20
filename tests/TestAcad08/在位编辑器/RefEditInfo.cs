namespace Test;

/// <summary>
/// 在位编辑器信息管理类 - 管理文档的在位编辑器状态、历史记录和UI显示
/// </summary>
public class RefEditInfo : IDisposable
{
    /// <summary>
    /// 临时图层常量名称
    /// </summary>
    public const string RefEdit0 = "RefEdit-0";

    /// <summary>
    /// 全局储存每个文档的在位编辑器状态
    /// </summary>
    public static readonly Dictionary<Document, RefEditInfo> RefeditMap = [];

    /// <summary>
    /// 关联的文档
    /// </summary>
    public Document Document { get; }

    /// <summary>
    /// 保存的原始窗口标题，用于在位编辑器结束后恢复
    /// </summary>
    private string? _originalWindowTitle;

    /// <summary>
    /// 窗口边框绘制器
    /// </summary>
    private WindowBorderDrawer? _borderDrawer;

    // 命令事件前锁定图层用,不需要加入历史
    // map[图元id,原有图层id]
    public Dictionary<ObjectId, ObjectId> EntityLayerBak = [];

    /// <summary>
    /// 工作区 - 当前可编辑状态
    /// </summary>
    public WorkingArea WorkingArea { get; private set; } = new();

    // 历史记录链表 - 不可变历史表（线性 reflog 风格）
    private readonly LinkedList<HistoryNode> _historyList = new();

    // 当前历史节点指针 - 指向当前历史位置
    private LinkedListNode<HistoryNode>? _currentNode = null;

    // 全局索引计数器 - 用于线性历史，持续自增不重置
    private int _globalIndex = 0;

    /// <summary>
    /// 当前历史索引（用于调试）
    /// </summary>
    public int CurrentIndex
    {
        get
        {
            if (_currentNode == null)
                return -1;
            int index = 0;
            var node = _historyList.First;
            while (node != null && node != _currentNode)
            {
                index++;
                node = node.Next;
            }
            return node == null ? -1 : index;
        }
    }

    /// <summary>
    /// 历史记录总数（用于调试）
    /// </summary>
    public int HistoryCount => _historyList.Count;

    /// <summary>
    /// 打印当前历史链信息（用于调试）- reflog 风格显示
    /// </summary>
    public void PrintHistoryChain()
    {
        Env.Printl("");
        Env.Print("========== RefEdit 历史链调试信息 (reflog 风格) ==========");
        Env.Print("");
        Env.Print("【工作区 - 当前可编辑状态】");
        Env.Print($"  CtrlState: {WorkingArea.CtrlState}");
        Env.Print($"  BlockReferenceId: {WorkingArea.BlockReferenceId}");
        Env.Print($"  CurrentSpaceId: {WorkingArea.CurrentSpaceId}");
        Env.Print($"  Workset: {WorkingArea.Workset.Count} 个对象");
        Env.Print($"  LockedLayers: {WorkingArea.LockedLayers.Count} 个对象");
        Env.Print($"  RefsetAddIds: {WorkingArea.RefsetAddIds.Count} 个对象");
        Env.Print($"  RefsetRemoveIds: {WorkingArea.RefsetRemoveIds.Count} 个对象");
        Env.Print("");
        Env.Print("【历史表 - 线性 reflog（索引持续自增，不删除）】");
        Env.Print($"  当前指针位置: {CurrentIndex}");
        Env.Print($"  历史记录总数: {HistoryCount}");
        Env.Print($"  全局索引计数器: {_globalIndex}");
        Env.Print("");

        if (_historyList.Count == 0)
        {
            Env.Print("  (历史表为空)");
        }
        else
        {
            foreach (var node in _historyList)
            {
                string marker = (_currentNode != null && _currentNode.Value == node) ? " <-- 当前指针" : "";
                string cmdName = string.IsNullOrEmpty(node.CommandName) ? "" : $" [{node.CommandName}]";
                Env.Print($"  索引[{node.Index}]{cmdName}{marker}");
                Env.Print($"    CtrlState: {node.CtrlState}, Workset: {node.Workset.Count}, RefsetAdd: {node.RefsetAddIds.Count}, RefsetRemove: {node.RefsetRemoveIds.Count}");
            }
        }

        Env.Print("");
        Env.Print("============================================");
    }

    RefEditInfo(Document document)
    {
        Document = document;
    }

    public static RefEditInfo Create(Document document)
    {
        // 不要在打开文件就修改数据库,要在第一次运行refedit命令时才修改数据库
        //using var xdocLock = document.LockDocument();
        var info = new RefEditInfo(document);
        //info.HistoryInit();
        RefeditMap.Add(document, info);
        return info;
    }

    public static void Destroy(Document document)
    {
        if (RefeditMap.TryGetValue(document, out var info))
        {
            info.ClearHistory();
            RefeditMap.Remove(document);
        }

        // 此处不清理回滚标记,因为保存存在意外,添加一个refclear命令
    }

    /// <summary>
    /// 清理历史记录
    /// </summary>
    private void ClearHistory()
    {
        _historyList.Clear();
        _currentNode = null;
        _globalIndex = 0;
        WorkingArea.Reset();
    }

    /// <summary>
    /// 写入历史 - 将当前工作区状态保存为不可变快照（线性 reflog 风格）
    /// </summary>
    /// <param name="commandName">触发历史记录的命令名称</param>
    public void HistoryWrite(string commandName = "", Transaction? tr = null)
    {
        // 线性历史记录：不删除分支历史，索引持续自增
        // 使用全局索引计数器，确保每次写入都有唯一的递增索引
        int newIndex = _globalIndex++;

        // 从工作区创建不可变快照（包含索引）
        var snapshot = WorkingArea.CreateSnapshot(newIndex, commandName);

        // 添加到历史表
        _currentNode = _historyList.AddLast(snapshot);

        UndoMarker.UndoMarkNodWrite(newIndex, tr);
    }


    /// <summary>
    /// 根据全局索引获取历史节点（reflog 风格）
    /// </summary>
    /// <param name="globalIndex">目标全局索引（HistoryNode.Index）</param>
    /// <returns>对应索引的链表节点，如果未找到则返回null</returns>
    public LinkedListNode<HistoryNode>? GetNodeByIndex(int globalIndex)
    {
        if (globalIndex < 0 || _historyList.Count == 0)
            return null;

        // 根据 HistoryNode.Index 属性查找（全局自增索引）
        var node = _historyList.First;
        while (node != null)
        {
            if (node.Value.Index == globalIndex)
                return node;
            node = node.Next;
        }
        return null;
    }

    /// <summary>
    /// 设置当前历史节点（用于Undo/Redo事件恢复）
    /// </summary>
    /// <param name="node">目标节点</param>
    public void SetCurrentNode(LinkedListNode<HistoryNode> node)
    {
        // 记录恢复前的状态，用于判断是否需要切换标题和边框
        bool wasRunning = CtrlState.IsRun;

        _currentNode = node;
        // 将历史节点恢复到工作区
        WorkingArea.RestoreFrom(node.Value);

        // 根据恢复后的状态更新窗口标题和边框
        bool isRunning = CtrlState.IsRun;
        if (wasRunning != isRunning)
        {
            if (isRunning)
            {
                // 恢复到运行状态，设置标题和边框
                SetRefEditWindowTitle(Document);
            }
            else
            {
                // 恢复到停止状态，恢复标题和清除边框
                RestoreWindowTitle();
            }
        }
    }


    /// <summary>
    /// 初始化历史记录系统
    /// </summary>
    public void HistoryInit()
    {
        // 此处初始化数据对象需要锁定文档
        UndoMarker.Init();

        ClearHistory();
        // 将初始工作区状态保存为第一个历史节点
        HistoryWrite("Init");
    }

    /// <summary>
    /// 清理工作区（用于RefClose等场景）
    /// </summary>
    public void Reset()
    {
        WorkingArea.Reset();
        EntityLayerBak.Clear();
    }



    /// <summary>
    /// 修改窗口标题为在位编辑器运行中状态，并在文档视图周围绘制红色边框
    /// </summary>
    /// <param name="doc">当前文档</param>
    public void SetRefEditWindowTitle(Document doc)
    {
        try
        {
            var mainWindow = Acap.MainWindow;
            if (mainWindow == null)
                return;

            // 保存原始标题
            _originalWindowTitle ??= mainWindow.Text.Split('-')[0];
            mainWindow.Text = $"在位编辑器 - 运行中";

            // 获取文档窗口句柄（MDI子窗口）
            IntPtr hWnd = GetDocumentWindowHandle(doc);
            if (hWnd == IntPtr.Zero)
            {
                Debug.WriteLine("[SetRefEditWindowTitle] 无法获取文档窗口句柄，使用主窗口");
                hWnd = mainWindow.Handle;
            }

            // 创建或复用边框绘制器，并启动持续绘制
            _borderDrawer ??= new WindowBorderDrawer(borderWidth: 5, r: 255, g: 0, b: 0);
            _borderDrawer.Start(hWnd, mainWindow.Handle);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SetRefEditWindowTitle] 修改标题失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取文档视图的窗口句柄（MDI子窗口）
    /// </summary>
    private IntPtr GetDocumentWindowHandle(Document doc)
    {
        try
        {
            // 使用反射获取文档窗口句柄
            var prop = doc.GetType().GetProperty("WindowHandle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (prop != null)
            {
                var handle = prop.GetValue(doc, null);
                if (handle is IntPtr ptr && ptr != IntPtr.Zero)
                {
                    Debug.WriteLine($"[GetDocumentWindowHandle] 获取到文档窗口句柄: {ptr}");
                    return ptr;
                }
            }

            // 备选方案：通过主窗口查找MDI子窗口
            IntPtr mainHwnd = Acap.MainWindow.Handle;
            IntPtr mdiClient = FindWindowEx(mainHwnd, IntPtr.Zero, "MDIClient", null);
            if (mdiClient != IntPtr.Zero)
            {
                // 获取第一个子窗口（当前活动文档）
                IntPtr childWnd = GetWindow(mdiClient, GW_CHILD);
                if (childWnd != IntPtr.Zero)
                {
                    Debug.WriteLine($"[GetDocumentWindowHandle] 通过MDIClient获取到子窗口: {childWnd}");
                    return childWnd;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[GetDocumentWindowHandle] 获取失败: {ex.Message}");
        }
        return IntPtr.Zero;
    }

    #region Win32 API

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string? lpszClass, string? lpszWindow);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    private const uint GW_CHILD = 5;

    #endregion

    /// <summary>
    /// 恢复窗口标题为原始状态，并清除红色边框
    /// </summary>
    public void RestoreWindowTitle()
    {
        try
        {
            var mainWindow = Acap.MainWindow;
            if (mainWindow == null)
                return;

            // 恢复标题
            if (_originalWindowTitle != null)
            {
                mainWindow.Text = _originalWindowTitle;
                _originalWindowTitle = null;
            }

            // 停止边框绘制
            _borderDrawer?.Stop();

            Debug.WriteLine("[RestoreWindowTitle] 窗口标题和边框已恢复");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RestoreWindowTitle] 恢复标题失败: {ex.Message}");
        }
    }

    #region 便捷属性 - 直接访问工作区

    public CtrlState CtrlState
    {
        get => WorkingArea.CtrlState;
    }

    public ObjectId BlockReferenceId
    {
        get => WorkingArea.BlockReferenceId;
    }

    public void SetBlockReferenceId(ObjectId id)
    {
        WorkingArea.BlockReferenceId = id;
    }

    public ObjectId CurrentSpaceId
    {
        get => WorkingArea.CurrentSpaceId;
    }

    public void SetCurrentSpaceId(ObjectId id)
    {
        WorkingArea.CurrentSpaceId = id;
    }

    public HashSet<ObjectId> Workset => WorkingArea.Workset;
    public HashSet<ObjectId> LockedLayers => WorkingArea.LockedLayers;
    public HashSet<ObjectId> RefsetAddIds => WorkingArea.RefsetAddIds;
    public HashSet<ObjectId> RefsetRemoveIds => WorkingArea.RefsetRemoveIds;
    public string CommandName
    {
        get => WorkingArea.CommandName;
    }

    #endregion


#if true2
    /// <summary>
    /// 读取历史并刷新显示
    /// </summary>
    /// <param name="isUndo">true为撤销，false为重做</param>
    public void HistoryRead(bool isUndo)
    {
        bool success = isUndo ? Undo() : Redo();
        if (!success)
            return;

        // 刷新显示
        RefreshDisplay();
        Env.Printl($"已{(isUndo ? "撤销" : "重做")}到历史记录索引: {CurrentIndex}");
    }

    /// <summary>
    /// 撤销操作 - 从历史表恢复到工作区
    /// </summary>
    /// <returns>是否成功撤销</returns>
    public bool Undo()
    {
        if (_currentNode == null || _currentNode.Previous == null)
            return false;

        _currentNode = _currentNode.Previous;
        // 将历史节点恢复到工作区
        WorkingArea.RestoreFrom(_currentNode.Value);
        return true;
    }

    /// <summary>
    /// 重做操作 - 从历史表恢复到工作区
    /// </summary>
    /// <returns>是否成功重做</returns>
    public bool Redo()
    {
        if (_currentNode == null || _currentNode.Next == null)
            return false;

        _currentNode = _currentNode.Next;
        // 将历史节点恢复到工作区
        WorkingArea.RestoreFrom(_currentNode.Value);
        return true;
    } 
#endif

    /// <summary>
    /// 刷新显示
    /// </summary>
    public void RefreshDisplay(HashSet<ObjectId>? lockedLayers = null)
    {
        using (var tr = DBTrans.Create(openCloseTrans: true))
        {
            lockedLayers ??= [];
            Fade(tr, lockedLayers);
        }
        using (var tr = DBTrans.Create(openCloseTrans: true))
        {
            RegenLayers(tr);
        }
    }

    /// <summary>
    /// 淡显全部图元
    /// </summary>
    /// <param name="tr"></param>
    /// <param name="lockedLayers"></param>
    void Fade(DBTrans tr, HashSet<ObjectId> lockedLayers)
    {
        // 1,锁定图层
        // 2,刷新图层状态,让锁定的图层的图元是暗显.
        // 3,解锁全部图层,不刷新
        tr.LayerTable.ForEach((layer, state) => {
            if (!layer.IsLocked)
            {
                layer.IsLocked = true;
                lockedLayers.Add(layer.ObjectId);
            }
        }, OpenMode.ForWrite);

        // 刷新画面的图层暗显
        IFoxUtils.RegenLayers(lockedLayers);

        // 解锁图层
        tr.LayerTable.ForEach((layer, state) => {
            if (lockedLayers.Contains(layer.ObjectId))
                layer.IsLocked = false;
        }, OpenMode.ForWrite);
    }

    /// <summary>
    /// 局部刷新图元
    /// </summary>
    /// <param name="tr"></param>
    void RegenLayers(DBTrans tr)
    {
        // 此时已经解锁全部图层,但是没有使用 IFoxUtils.RegenLayers 刷新,
        // 然后我们使用平移图元就会亮显这一部分的图元.
        foreach (var id in Workset)
        {
            using var ent = (Entity)tr.GetObject(id, OpenMode.ForWrite, true, true);
            ent.Move(Point3d.Origin, Point3d.Origin);
        }

        // 刷新这个图层,
        // 即使这个图层没有任何图元,也会触发刷新修改过的图元而不是整个图层
        var refLayerId = tr.LayerTable.Add(RefEdit0);
        var lays = new List<ObjectId> { refLayerId };
        IFoxUtils.RegenLayers(lays);
    }

    #region IDisposable

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _borderDrawer?.Dispose();
        _borderDrawer = null;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    ~RefEditInfo()
    {
        _borderDrawer?.Dispose();
    }

    #endregion
}
