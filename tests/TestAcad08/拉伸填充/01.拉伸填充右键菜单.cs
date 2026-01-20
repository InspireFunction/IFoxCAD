using IFoxCAD.Cad;
using System.Drawing.Drawing2D;
using static IFoxCAD.Cad.PostCmd;

namespace JoinBoxAcad;

public class StretchFill​
{

    [IFoxInitializeAttribute]
    [CommandMethod(nameof(HatchPickInit))]
    public void HatchPickInit(Document doc)
    {
        Env.Printl($"※拉伸填充控制※\n{nameof(HatchPickSwitch)} - 切换开关\n");

        if (Debugger.IsAttached)
            Env.SetVar("hpscale", 22);

        // 设定高版本双击填充启动修改面板
        // JoinBoxAcad.Menu.Cui.CuiInit();

        LoadHelper(true);
    }

    // 只能命令卸载哦,因为关闭cad是不需要卸载的
    [CommandMethod(nameof(UnLoadHatchPick))]
    public void UnLoadHatchPick()
    {
        LoadHelper(false);
    }

    [CommandMethod(nameof(HatchPickSwitch))]
    public void HatchPickSwitch()
    {
        if (HatchPick.State.IsStop)
        {
            Env.Printl("已经 卸载 拉伸填充控制+ 用: " + nameof(HatchPickInit) + " 加载");
            return;
        }

        if (HatchPick.State.IsRun)
            HatchPick.State.Break();
        else
            HatchPick.State.Start();
        Env.Printl("已经 " + (HatchPick.State.IsRun ? "开启" : "禁用") + " 拉伸填充控制+");
    }


    internal static Dictionary<Document, HatchPick> HatchPickMap = [];

    void LoadHelper(bool isLoad)
    {
        var dm = Acap.DocumentManager;
        if (dm is null || dm.Count == 0)
            return;

        if (isLoad)
        {
            dm.DocumentCreated += Dm_DocumentCreated;
            Dm_DocumentCreated(); // 自执行一次
            AddRightClickMenu();
            HatchPick.Start();
            dm.DocumentLockModeChanged += Dm_VetoCommand;
        }
        else
        {
            HatchPick.Stop();
            dm.DocumentCreated -= Dm_DocumentCreated;
            UnDocumentCreated();
            StretchFill​.RemoveRightClickMenu();
            dm.DocumentLockModeChanged -= Dm_VetoCommand;
        }
    }

    /// <summary>
    /// 文档创建反应器
    /// </summary>
    void Dm_DocumentCreated(object? sender = null, DocumentCollectionEventArgs? e = null)
    {
        var dm = Acap.DocumentManager;
        if (dm is null || dm.Count == 0)
            return;
        var doc = dm.MdiActiveDocument;
        if (doc is null)
            return;
        if (!HatchPickMap.ContainsKey(doc))
            HatchPickMap.Add(doc, new HatchPick(doc));
    }

    /// <summary>
    /// 卸载文档创建反应器
    /// </summary>
    static void UnDocumentCreated()
    {
        var dm = Acap.DocumentManager;
        if (dm is null || dm.Count == 0)
            return;
        var doc = dm.MdiActiveDocument;
        if (doc is null)
            return;
        if (HatchPickMap.TryGetValue(doc, out var xx))
        {
            xx.Dispose();
            HatchPickMap.Remove(doc);
        }
    }



    #region 否决特性面板
    // 文档锁事件: 期间是不允许再次锁文档.
    // 最好就是直接异步发送命令,
    // 清理当前文档拉伸边界,则内部需要保证不锁文档.

    /// <summary>
    /// 否决特性面板Ctrl+1
    /// </summary>
    bool _vetoProperties = false;


    /// <summary>
    /// 反应器->命令否决触发命令前(不可锁文档)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Dm_VetoCommand(object sender, DocumentLockModeChangedEventArgs e)
    {
        if (!HatchPick.State.IsRun)
            return;
        if (string.IsNullOrEmpty(e.GlobalCommandName) || e.GlobalCommandName == "#")
            return;
        switch (e.GlobalCommandName.ToUpper())
        {
            case "PROPERTIES": // 特性面板
            {
                // 事件顺序问题:
                // 开cad之后第一次双击必弹出特性面板
                // 所以这里直接删除填充边界
                SetPropertiesInfoTask();
                if (_vetoProperties)
                {
                    DebugEx.Printl("Dm_VetoCommand 否决了");
                    e.Veto();
                    _vetoProperties = false;
                    SendCommand("_hatchedit ", RunCmdFlag.AcedPostCommand);
                    return;
                }
                DebugEx.Printl("Dm_VetoCommand 没否决");
            }
            break;
            case "#PROPERTIES":
            {
                DebugEx.Printl("#PROPERTIES");
            }
            break;
        }
    }

    /// <summary>
    /// 这是文档事件期间,不能再次锁文档!!
    /// </summary>
    void SetPropertiesInfoTask()
    {
        // 原有选择集
        var prompt = Env.Editor.SelectImplied();
        if (prompt.Status != PromptStatus.OK)
            return;

        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc == null) return;
        if (!HatchPickMap.TryGetValue(doc, out var hpe))
            return;

        using DBTrans tr = new();

        // 获取当前文档记录的填充边界
        HashSet<ObjectId> boAll = [];
        foreach (var boid in hpe.HatchConvMap.Values.SelectMany(hc => hc.BoundaryNewlyIds))
        {
            boAll.Add(boid);
        }

        // 获取选择集上面所有的填充,如果没有填充就结束(不屏蔽特性面板)
        bool hasHatch = false;

        HashSet<ObjectId> idsOfSsget = [];
        foreach (var id in prompt.Value.GetObjectIds())
        {
            // 含有填充
            if (hpe.HatchConvMap.ContainsKey(id))
                hasHatch = true;

            // 排除边界的加入
            if (!boAll.Contains(id))
                idsOfSsget.Add(id);
        }
        if (!hasHatch)
            return;

        // 删除填充边界,并清理关联反应器
        hpe.EraseAllHatchBorders(false);

        // 重设选择集 提供给后续命令判断
        hpe.SetImpliedSelection(idsOfSsget);

        // 如果有填充才否决
        _vetoProperties = idsOfSsget.Count != 0;
    }
    #endregion



    private const string V0 = "拉伸填充-开";
    private const string V1 = "拉伸填充-关";// (面板的独立填充必须关,否则致命错误)
    private const string V2 = "独立填充";//(快捷,不需要关...目前还是会崩溃)
    static readonly HashSet<string> _menuItems = [V0, V1, V2];
    static readonly ContextMenuExtension _contextMenu = new() { Title = "惊惊盒子" };
    /// <summary>
    /// 添加右键菜单
    /// </summary>
    void AddRightClickMenu()
    {
        // 右键菜单
        foreach (var item in _menuItems)
        {
            MenuItem mi = new(item);        // 添加菜单项
            mi.Click += MenuItemClick;      // 添加单击事件

            //mi.MenuItems.Add(new MenuItem("改颜色1")); // 二级菜单
            _contextMenu.MenuItems.Add(mi); // 提交
        }
        Acap.AddDefaultContextMenuExtension(_contextMenu);// 添加默认上下文菜单扩展,带标题的

        //加入到某一种对象的右键菜单中
        //RXClass rxClass = Entity.GetClass(typeof(BlockReference));
        //Acap.AddObjectContextMenuExtension(rxClass, contextMenu);

        //// 选择实体右键菜单才有用. 获得实体所属的RXClass类型
        // RXClass rx = RXObject.GetClass(typeof(Entity));
        // Acap.AddObjectContextMenuExtension(rx, contextMenu); // 这里为什么又可以不带标题
    }

    /// <summary>
    /// 卸载右键菜单
    /// </summary>
    static void RemoveRightClickMenu()
    {
        if (_contextMenu is null)
            return;
        Acap.RemoveDefaultContextMenuExtension(_contextMenu);
    }

    /// <summary>
    /// 右键点击触发
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void MenuItemClick(object sender, EventArgs e)
    {
        // 获取发出命令的快捷菜单项
        if (sender is not MenuItem mi)
            return;

        // 根据快捷菜单项的名字,分别调用对应的命令
        if (!_menuItems.Contains(mi.Text))
            return;

        switch (mi.Text)
        {
            case V0:
            HatchPick.State.Start();
            break;
            case V1:
            HatchPick.State.Break();
            break;
            case V2:
            {
                PromptSelectionOptions pso = new()
                {
                    AllowDuplicates = true, // 不允许重复选择
                    SingleOnly = true,      // 隐含窗口选择(不需要空格确认)
                };
                //
                //var ssPsr = Env.Editor.GetSelection(pso, HatchPick.FilterForHatch);
                var ssPsr = Env.Editor.GetSelection(pso);
                if (ssPsr.Status != PromptStatus.OK)
                    return;

                Env.Editor.SetImpliedSelection(ssPsr.Value.GetObjectIds());
                SendCommand("-hatchedit H ", RunCmdFlag.AcedPostCommand);
            }
            break;
        }
    }
}