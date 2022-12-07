using Autodesk.AutoCAD.Windows;
using System.Diagnostics;
using static IFoxCAD.Cad.PostCmd;
using MenuItem = Autodesk.AutoCAD.Windows.MenuItem;

namespace JoinBoxAcad;
public class HatchPick
{
    [IFoxInitialize]
    [CommandMethod(nameof(HatchPickInit))]
    public void HatchPickInit()
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
        if (HatchPickEvent.State.IsStop)
        {
            Env.Printl("已经 卸载 拉伸填充控制+ 用: " + nameof(HatchPickInit) + " 加载");
            return;
        }

        if (HatchPickEvent.State.IsRun)
            HatchPickEvent.State.Break();
        else
            HatchPickEvent.State.Start();
        Env.Printl("已经 " + (HatchPickEvent.State.IsRun ? "开启" : "禁用") + " 拉伸填充控制+");
    }


    internal static Dictionary<Document, HatchPickEvent> MapDocHatchPickEvent = new();
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
            HatchPickEvent.AddInit();
        }
        else
        {
            HatchPickEvent.RemoveInit();
            dm.DocumentCreated -= Dm_DocumentCreated;
            UnDocumentCreated();
            HatchPick.RemoveRightClickMenu();
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
        if (!MapDocHatchPickEvent.ContainsKey(doc))
            MapDocHatchPickEvent.Add(doc, new HatchPickEvent(doc));
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
        if (MapDocHatchPickEvent.ContainsKey(doc))
        {
            MapDocHatchPickEvent[doc].Dispose();
            MapDocHatchPickEvent.Remove(doc);
        }
    }



    private const string V0 = "拉伸填充-开";
    private const string V1 = "拉伸填充-关";// (面板的独立填充必须关,否则致命错误)
    private const string V2 = "独立填充";//(快捷,不需要关...目前还是会崩溃)
    static readonly HashSet<string> _menuItems = new() { V0, V1, V2 };
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
            HatchPickEvent.State.Start();
            break;
            case V1:
            HatchPickEvent.State.Break();
            break;
            case V2:
            {
                HatchPickEvent.State.Break();
                PromptSelectionOptions pso = new()
                {
                    AllowDuplicates = true, // 不允许重复选择
                    SingleOnly = true,      // 隐含窗口选择(不需要空格确认)
                };
                var ssPsr = Env.Editor.GetSelection(pso, HatchPickEvent.FilterForHatch);
                if (ssPsr.Status != PromptStatus.OK)
                    return;

                Env.Editor.SetImpliedSelection(ssPsr.Value.GetObjectIds());
                SendCommand("-hatchedit H ", RunCmdFlag.AcedPostCommand);
                HatchPickEvent.State.Start();
            }
            break;
        }
    }
}