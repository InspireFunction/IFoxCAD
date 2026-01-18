namespace IFoxCAD.Cad;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

/// <summary>
/// 
/// </summary>
public static class RefeditManager
{
    private static readonly Dictionary<Document, HashSet<ObjectId>> _workSets = [];

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_"></param>
    [IFoxInitialize]
    public static void Initialize(Document _)
    {
        var dm = Application.DocumentManager;

        // 初始化已打开的文档
        foreach (Document doc in dm)
        {
            InitializeDocument(doc);
        }

        dm.DocumentCreated += (sender, e) => InitializeDocument(e.Document);
        dm.DocumentToBeDestroyed += (sender, e) => _workSets.Remove(e.Document);
    }

    private static void InitializeDocument(Document doc)
    {
        if (!_workSets.ContainsKey(doc))
        {
            _workSets[doc] = [];
            doc.CommandWillStart += (sender, e) => Md_CommandWillStart(doc, e);
            doc.CommandEnded += (sender, e) => Md_CommandEnded(doc, e);
        }
    }

    /// <summary>
    /// 获取文档的工作集（只读）
    /// </summary>
    /// <param name="doc"></param>
    /// <returns></returns>
    public static ReadOnlyCollection<ObjectId> GetWorkSet(Document doc)
    {
        if (_workSets.TryGetValue(doc, out var set))
        {
            return set.ToList().AsReadOnly();
        }
        return new([]);
    }

    /// <summary>
    /// 命令开启时候
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="e"></param>
    private static void Md_CommandWillStart(Document doc, CommandEventArgs e)
    {
        if (string.IsNullOrEmpty(e.GlobalCommandName) || e.GlobalCommandName == "#")
            return;

        if (e.GlobalCommandName.ToUpper() == "REFEDIT")
        {
            // 开始REFEDIT时记录当前所有图元
            var result = doc.Editor.SelectAll();
            if (result.Status == PromptStatus.OK)
            {
                _workSets[doc] = [.. result.Value.GetObjectIds()];
            }
        }
    }

    /// <summary>
    /// 命令结束时
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="e"></param>
    private static void Md_CommandEnded(Document doc, CommandEventArgs e)
    {
        if (string.IsNullOrEmpty(e.GlobalCommandName) || e.GlobalCommandName == "#")
            return;

        var command = e.GlobalCommandName.ToUpper();

        switch (command)
        {
            case "REFEDIT":
            // 更新工作集
            //var result = doc.Editor.SelectAll();
            //if (result.Status == PromptStatus.OK)
            //{
            //    _workSets[doc] = [.. result.Value.GetObjectIds()];
            //}
            break;

            case "REFCLOSE":
            _workSets[doc].Clear();
            break;

            case "REFSET":
            // 处理添加/删除图元
            HandleRefset(doc);
            break;
        }
    }

    private static void HandleRefset(Document doc)
    {
        var ed = doc.Editor;

        // 获取用户上一次选择集（REFSET命令选择的对象）
        var psr = ed.SelectPrevious();
        if (psr.Status != PromptStatus.OK)
            return;

        var selectedIds = psr.Value.GetObjectIds().ToHashSet();

        // 获取最后一行命令提示判断是添加还是删除
        var lastPrompt = Acap.GetSystemVariable("LASTPROMPT").ToString();

        if (lastPrompt.Contains("添加") || lastPrompt.Contains("Added"))
        {
            // 添加模式：将选择的图元加入工作集
            _workSets[doc].UnionWith(selectedIds);
        }
        else if (lastPrompt.Contains("删除") || lastPrompt.Contains("Removed"))
        {
            // 删除模式：从工作集中移除选择的图元
            _workSets[doc].ExceptWith(selectedIds);
        }
    }
}
