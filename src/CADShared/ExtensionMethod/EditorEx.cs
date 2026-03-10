using IFoxCAD.Com;

namespace IFoxCAD.Cad;

/// <summary>
/// 命令行扩展类
/// </summary>
public static class EditorEx
{
    #region 选择集
    /// <summary>
    /// 选择穿过一个点的对象
    /// </summary>
    /// <param name="editor">命令行对象</param>
    /// <param name="point">点</param>
    /// <param name="filter">过滤器</param>
    /// <returns>选择集结果类</returns>
    public static PromptSelectionResult SelectAtPoint(this Editor editor, Point3d point,
        SelectionFilter? filter = null)
    {
        return editor.SelectCrossingWindow(point, point, filter ?? new SelectionFilter([]));
    }

    /// <summary>
    /// 根据线宽创建图层选择集
    /// </summary>
    /// <param name="editor">命令行对象</param>
    /// <param name="lineWeight">线宽</param>
    /// <returns>图层选择集</returns>
    public static SelectionSet SelectByLineWeight(this Editor editor, LineWeight lineWeight)
    {
        OpFilter filter = new OpEqual(370, lineWeight);

        var lays = DBTrans.Top.LayerTable.GetRecords()
            .Where(ltr => ltr.LineWeight == lineWeight)
            .Select(ltr => ltr.Name)
            .ToArray();

        if (lays.Length > 0)
        {
            filter = new OpOr
            {
                filter, new OpAnd { { 8, string.Join(",", lays) }, { 370, LineWeight.ByLayer } }
            };
        }

        var res = editor.SelectAll(filter);
        return res.Value;
    }

    /// <summary>
    /// 选择集
    /// </summary>
    /// <param name="editor">命令行对象</param>
    /// <param name="mode">模式</param>
    /// <param name="filter">过滤器</param>
    /// <param name="messages">消息</param>
    /// <param name="keywords">
    /// 关键字和回调函数
    /// <para>不用使用下列关键字 "Window/Last/Crossing/BOX/ALL/Fence/WPolygon/CPolygon/Group/Add/Remove/Multiple/Previous/Undo/AUto/Single" </para>
    /// </param>
    /// <returns></returns>
    public static PromptSelectionResult SSGet(this Editor editor, string? mode = null,
        SelectionFilter? filter = null, (string add, string remove)? messages = null,
        Dictionary<string, (string, Action)>? keywords = null)
    {
        PromptSelectionOptions pso = new();
        if (mode is not null)
        {
            mode = mode.ToUpper();
            pso.SinglePickInSpace = mode.Contains(":A");
            pso.RejectObjectsFromNonCurrentSpace = mode.Contains(":C");
            pso.AllowDuplicates = mode.Contains(":D");
            pso.SelectEverythingInAperture = mode.Contains(":E");
            pso.RejectObjectsOnLockedLayers = mode.Contains(":L");
            pso.PrepareOptionalDetails = mode.Contains(":N");
            pso.SingleOnly = mode.Contains(":S");
            pso.RejectPaperspaceViewport = mode.Contains(":V");
            pso.AllowSubSelections = mode.Contains("-A");
            pso.ForceSubSelections = mode.Contains("-F");
        }

        if (messages is not null)
        {
            pso.MessageForAdding = messages.Value.add;
            pso.MessageForRemoval = messages.Value.remove;
        }

        if (keywords is not null)
        {
            foreach (var keyword in keywords.Keys)
                pso.Keywords.Add(keyword);
            if (pso.MessageForRemoval is null)
                pso.MessageForAdding = "选择对象";

            var str = keywords.Keys.Select(key => {
                keywords.TryGetValue(key, out var value);
                return $"{value.Item1}({key})";
            }).ToArray();

            pso.MessageForAdding += $" [{string.Join("/", str)}]";
            pso.KeywordInput += (_, e) => {
                if (keywords.TryGetValue(e.Input, out var value))
                    value.Item2.Invoke();
            };
        }

        var ss = filter is not null ? editor.GetSelection(pso, filter) : editor.GetSelection(pso);
        return ss;
    }


    /*
     *  // 定义选择集选项
     *  var pso = new PromptSelectionOptions
     *  {
     *      AllowDuplicates = false,  // 重复选择
     *  };
     *
     *  // getai遍历全图选择块有用到
     *  var dic = new Dictionary<string, Action>() {
     *          { "Z,全部同名", ()=> {
     *              getai = BlockHelper.EnumAttIdentical.AllBlockName;
     *              SendEsc.Esc();
     *          }},
     *          { "X,动态块显示",  ()=> {
     *              getai = BlockHelper.EnumAttIdentical.Display;
     *          }},
     *          { "V,属性值-默认", ()=> {
     *              getai = BlockHelper.EnumAttIdentical.DisplayAndTagText;
     *          }},
     *          // 允许以下操作,相同的会加入前面的
     *          // { "V,属性值-默认|X,啊啊啊啊", ()=> {
     *
     *          // }},
     *  };
     *  pso.SsgetAddKeys(dic);
     *
     *  // 创建选择集过滤器,只选择块对象
     *  var filList = new TypedValue[] { new TypedValue((int)DxfCode.Start, "INSERT") };
     *  var filter = new SelectionFilter(filList);
     *  ssPsr = ed.GetSelection(pso, filter);
     */


    /// <summary>
    /// 添加选择集关键字和回调
    /// </summary>
    /// <param name="pso">选择集配置</param>
    /// <param name="acMap">关键字,回调委托</param>
    /// <param name="defaultKey">关键字,默认key</param>
    /// <returns></returns>
    public static void SsgetAddKeys(this PromptSelectionOptions pso,
        Dictionary<string, Action<object, SelectionTextInputEventArgs>> acMap,
        string? defaultKey = null)
    {
        ArgumentNullEx.ThrowIfNull(pso);
        ArgumentNullEx.ThrowIfNull(acMap);
        if (acMap.Count == 0) return;

        // 创建处理后的新字典，避免在遍历时修改
        var processedMap = new Dictionary<string, Action<object, SelectionTextInputEventArgs>>();

        // 第一阶段：处理 | 分隔的关键词
        foreach (var pair in acMap)
        {
            var keys = pair.Key.Split('|');

            // 如果没有 | 分隔符，直接添加到处理字典
            if (keys.Length < 2)
            {
                AddOrCombineAction(processedMap, pair.Key, pair.Value);
                continue;
            }

            // 处理每个 | 分隔的关键词
            foreach (var singleKey in keys)
            {
                AddOrCombineAction(processedMap, singleKey, pair.Value);
            }
        }

        // 第二阶段：处理 , 分隔的关键词并合并相同前缀
        var finalMap = new Dictionary<string, Action<object, SelectionTextInputEventArgs>>();
        foreach (var pair in processedMap)
        {
            var prefix = pair.Key.Split(',')[0];
            if (finalMap.TryGetValue(prefix, out var existingAction))
            {
                // 合并委托
                finalMap[prefix] = existingAction + pair.Value;
            }
            else
            {
                finalMap[prefix] = pair.Value;
            }
        }

        // key,描述
        Dictionary<string, string> description = [];

        // 第三阶段：添加到 pso.Keywords
        foreach (var pair in finalMap)
        {
            // 由于我们只保留了前缀，这里需要从原始数据中查找完整格式
            // 或者修改逻辑以保留完整的关键词格式
            var fullKey = processedMap.Keys
                .FirstOrDefault(k => k.StartsWith(pair.Key + ","));

            if (!string.IsNullOrEmpty(fullKey))
            {
                var keyParts = fullKey.Split([",", "|"], StringSplitOptions.RemoveEmptyEntries);
                if (keyParts.Length >= 2)
                {
                    pso.Keywords.Add(keyParts[0], keyParts[0], keyParts[1] + "(" + keyParts[0] + ")");
                    description[keyParts[0]] = keyParts[1];
                }
            }
        }

        // 第四阶段：更新回调字典
        acMap.Clear();
        foreach (var pair in finalMap)
        {
            acMap[pair.Key] = pair.Value;
        }

        // 设置默认,获取默认的描述
        // 输入选项 [添加(A)/删除(R)] <添加>:
        var defaultStr = "";
        if (defaultKey is not null)
        {
            pso.Keywords.Default = defaultKey;
            defaultStr = "<" + description[defaultKey] + ">";
        }

        // 显示关键字,加上用户预设的
        var ss = pso.MessageForAdding;
        pso.MessageForAdding = ss
            + pso.Keywords.GetDisplayString(true)
            + " "
            + defaultStr;

        // 设置回调事件
        pso.KeywordInput += (s, e) => {
            if (acMap.TryGetValue(e.Input, out var action))
            {
                // 替换描述为什么没有生效?可能是因为已经写入缓冲区,就不再接受.
                var defaultStr = "";
                defaultKey = e.Input;
                if (defaultKey is not null)
                {
                    pso.Keywords.Default = defaultKey;
                    defaultStr = "<" + description[defaultKey] + ">";
                }

                pso.MessageForAdding = ss
                 + pso.Keywords.GetDisplayString(true)
                 + " "
                 + defaultStr;

                action.Invoke(s, e);
            }
        };
    }

    // 辅助方法：添加或合并 Action
    private static void AddOrCombineAction(
        Dictionary<string, Action<object, SelectionTextInputEventArgs>> dictionary,
        string key,
         Action<object, SelectionTextInputEventArgs> action)
    {
        if (dictionary.TryGetValue(key, out var existingAction))
        {
            dictionary[key] = existingAction + action;
        }
        else
        {
            dictionary[key] = action;
        }
    }

    // #region 即时选择样板
    // /// <summary>
    // ///  即时选择,框选更新关键字
    // /// </summary>
    // public static void SelectTest()
    // {
    //     Env.Editor.WriteMessage("\n[白嫖工具]--测试");
    //     // 激活选中事件
    //     Env.Editor.SelectionAdded += SelectTest_SelectionAdded;
    //     // 初始化坐标系
    //     Env.Editor.CurrentUserCoordinateSystem = Matrix3d.Identity;
    //     // 创建过滤器
    //     var sf = new OpEqual(0, "arc");
    //     var pso = new PromptSelectionOptions
    //     {
    //         MessageForAdding = "\n请选择对象:"
    //     };

    //     pso.Keywords.Add("Z");
    //     pso.Keywords.Add("X");
    //     pso.Keywords.Add("Q");
    //     // 注册关键字
    //     pso.KeywordInput += SelectTest_KeywordInput;
    //     try
    //     {
    //         // 用户选择
    //         var psr = Env.Editor.GetSelection(pso, sf);
    //         // 处理代码
    //     }
    //     catch (Exception ex)// 捕获关键字
    //     {
    //         if (ex.Message == "XuError")
    //         {
    //             // 关闭关键字事件
    //             pso.KeywordInput -= SelectTest_KeywordInput;
    //             // 关闭选中事件
    //             Env.Editor.SelectionAdded -= SelectTest_SelectionAdded;
    //             // 重新调用自身
    //             ZengLiangYuanJiao();
    //         }
    //     }
    //     // 关闭关键字事件
    //     pso.KeywordInput -= SelectTest_KeywordInput;
    //     // 关闭选中事件
    //     Env.Editor.SelectionAdded -= SelectTest_SelectionAdded;
    // }

    // /// <summary>
    // /// 即时选择
    // /// </summary>
    // /// <param name="sender"></param>
    // /// <param name="e"></param>
    // private static void SelectTest_SelectionAdded(object sender, SelectionAddedEventArgs e)
    // {
    //     // 关闭选中事件
    //     Env.Editor.SelectionAdded -= SelectTest_SelectionAdded;
    //     using (var tr = new DBTrans())
    //     {
    //         // 处理代码
    //         for (int i = 0; i < e.AddedObjects.Count; i++)
    //         {
    //             // 处理完移除已处理的对象
    //             e.Remove(i);
    //         }
    //     }
    //     // 激活选中事件
    //     Env.Editor.SelectionAdded += SelectTest_SelectionAdded;
    // }

    // /// <summary>
    // /// 关键字响应
    // /// </summary>
    // /// <param name="sender"></param>
    // /// <param name="e"></param>
    // private static void SelectTest_KeywordInput(object sender, SelectionTextInputEventArgs e)
    // {
    //     // 获取关键字
    //     switch (e.Input)
    //     {
    //         case "Z":
    //                 break;
    //         case "X":
    //                 break;

    //         case "Q":
    //                 break;
    //     }
    //     // 抛出异常,用于更新提示信息
    //     throw new ArgumentException("XuError");
    // }
    // #endregion
    #endregion

    #region Info

    /// <summary>
    /// 带错误提示对话框的打印信息函数
    /// </summary>
    /// <param name="format">带格式项的字符串</param>
    /// <param name="args">指定格式化的对象数组</param>
    public static void StreamMessage(string format, params object[] args)
    {
        StreamMessage(string.Format(format, args));
    }

    /// <summary>
    /// 带错误提示对话框的打印信息函数
    /// </summary>
    /// <param name="message">打印信息</param>
    public static void StreamMessage(string message)
    {
        try
        {
            if (HasEditor())
                WriteMessage(message);
            else
                InfoMessageBox(message);
        }
        catch (Exception ex)
        {
            Message(ex);
        }
    }

    /// <summary>
    /// 异常信息对话框
    /// </summary>
    /// <param name="ex">异常</param>
    public static void Message(Exception ex)
    {
        try
        {
            System.Windows.Forms.MessageBox.Show(ex.ToString(), "Error",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error);
        }
        catch
        {
        }
    }

    /// <summary>
    /// 提示信息对话框
    /// </summary>
    /// <param name="caption">对话框的标题</param>
    /// <param name="message">对话框文本</param>
    public static void InfoMessageBox(string caption, string message)
    {
        try
        {
            System.Windows.Forms.MessageBox.Show(message, caption,
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Message(ex);
        }
    }

    /// <summary>
    /// 提示信息对话框
    /// </summary>
    /// <param name="caption">对话框的标题</param>
    /// <param name="format">带格式化项的对话框文本</param>
    /// <param name="args">指定格式化的对象数组</param>
    public static void InfoMessageBox(string caption, string format, params object[] args)
    {
        InfoMessageBox(caption, string.Format(format, args));
    }

    /// <summary>
    /// 提示信息对话框,默认标题为NFox.Cad
    /// </summary>
    /// <param name="message">对话框文本</param>
    public static void InfoMessageBox(string message)
    {
        InfoMessageBox("NFox.Cad", message);
    }

    /// <summary>
    /// 提示信息对话框
    /// </summary>
    /// <param name="format">带格式化项的对话框文本</param>
    /// <param name="args">指定格式化的对象数组</param>
    public static void InfoMessageBox(string format, params object[] args)
    {
        InfoMessageBox(string.Format(format, args));
    }

    /// <summary>
    /// 命令行打印字符串
    /// </summary>
    /// <param name="message">字符串</param>
    public static void WriteMessage(string message)
    {
        try
        {
            if (Acceptable())
                Acaop.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n" + message);
        }
        catch (Exception ex)
        {
            Message(ex);
        }
    }

    /// <summary>
    /// 命令行打印字符串
    /// </summary>
    /// <param name="format">带格式化项的文本</param>
    /// <param name="args">指定格式化的对象数组</param>
    public static void WriteMessage(string format, params object[] args)
    {
        WriteMessage(string.Format(format, args));
    }

    /// <summary>
    /// 判断是否有活动的编辑器对象
    /// </summary>
    /// <returns><see langword="true"/>有,<see langword="false"/>没有</returns>
    public static bool HasEditor()
    {
        return Acaop.DocumentManager.MdiActiveDocument is not null &&
               Acaop.DocumentManager.Count != 0 &&
               Acaop.DocumentManager.MdiActiveDocument.Editor is not null;
    }

    /// <summary>
    /// 判断是否可以打印字符串
    /// </summary>
    /// <returns><see langword="true"/>可以打印,<see langword="false"/>不可以打印</returns>
    public static bool Acceptable()
    {
        return HasEditor() && !Acaop.DocumentManager.MdiActiveDocument.Editor.IsDragging;
    }

    #endregion Info

    #region 画矢量线

    /// <summary>
    /// 根据点表返回矢量线的列表
    /// </summary>
    /// <param name="pnts">点表</param>
    /// <param name="isClosed">是否闭合,<see langword="true"/> 为闭合,<see langword="false"/> 为不闭合</param>
    /// <returns></returns>
    public static List<TypedValue> GetLines(IEnumerable<Point2d> pnts, bool isClosed)
    {
        using var enumerator = pnts.GetEnumerator();
        if (!enumerator.MoveNext())
            return [];

        List<TypedValue> values = [];

        TypedValue first = new((int)LispDataType.Point2d, enumerator.Current);
        var last = first;

        while (enumerator.MoveNext())
        {
            values.Add(last);
            last = new TypedValue((int)LispDataType.Point2d, enumerator.Current);
            values.Add(last);
        }

        if (isClosed)
        {
            values.Add(last);
            values.Add(first);
        }

        return values;
    }

    /// <summary>
    /// 画矢量线
    /// </summary>
    /// <param name="editor">编辑器对象</param>
    /// <param name="pnts">点表</param>
    /// <param name="colorIndex">颜色码</param>
    /// <param name="isClosed">是否闭合,<see langword="true"/> 为闭合,<see langword="false"/> 为不闭合</param>
    public static void DrawVectors(this Editor editor, IEnumerable<Point2d> pnts, short colorIndex,
        bool isClosed)
    {
        var rlst = new LispList { { LispDataType.Int16, colorIndex } };
        rlst.AddRange(GetLines(pnts, isClosed));
        editor.DrawVectors(new(rlst.ToArray()), Matrix3d.Identity);
    }

    /// <summary>
    /// 画矢量线
    /// </summary>
    /// <param name="editor">编辑器对象</param>
    /// <param name="pnts">点表</param>
    /// <param name="colorIndex">颜色码</param>
    public static void DrawVectors(this Editor editor, IEnumerable<Point2d> pnts, short colorIndex)
    {
        editor.DrawVectors(pnts, colorIndex, false);
    }

    /// <summary>
    /// 用矢量线画近似圆（正多边形）
    /// </summary>
    /// <param name="editor">编辑器对象</param>
    /// <param name="pnts">点表</param>
    /// <param name="colorIndex">颜色码</param>
    /// <param name="radius">半径</param>
    /// <param name="numEdges">多边形边的个数</param>
    public static void DrawCircles(this Editor editor, IEnumerable<Point2d> pnts, short colorIndex,
        double radius, int numEdges)
    {
        var rlst = new LispList { { LispDataType.Int16, colorIndex } };

        foreach (var pnt in pnts)
        {
            var vec = Vector2d.XAxis * radius;
            var angle = Math.PI * 2 / numEdges;

            List<Point2d> tpnts = [pnt + vec];
            for (var i = 1; i < numEdges; i++)
            {
                tpnts.Add(pnt + vec.RotateBy(angle * i));
            }

            rlst.AddRange(GetLines(tpnts, true));
        }

        editor.DrawVectors(new(rlst.ToArray()), editor.CurrentUserCoordinateSystem);
    }

    /// <summary>
    /// 用矢量线画近似圆（正多边形）
    /// </summary>
    /// <param name="editor">编辑器对象</param>
    /// <param name="pnt">点</param>
    /// <param name="colorIndex">颜色码</param>
    /// <param name="radius">半径</param>
    /// <param name="numEdges">多边形边的个数</param>
    public static void DrawCircle(this Editor editor, Point2d pnt, short colorIndex, double radius,
        int numEdges)
    {
        var vec = Vector2d.XAxis * radius;
        var angle = Math.PI * 2 / numEdges;

        List<Point2d> pnts = [pnt + vec];
        for (var i = 1; i < numEdges; i++)
            pnts.Add(pnt + vec.RotateBy(angle * i));

        editor.DrawVectors(pnts, colorIndex, true);
    }

    /// <summary>
    /// 根据点表绘制矢量线段(每两点为一条线段的起始点和终止点)
    /// </summary>
    /// <param name="editor">用户交互对象</param>
    /// <param name="points">点表</param>
    /// <param name="colorIndex">CAD颜色索引;默认：1为红色</param>
    /// <param name="drawHighlighted">是否高亮显示;<see langword="true"/>为高亮显示,默认：<see langword="false"/>为不高亮显示</param>
    public static void DrawLineVectors(this Editor editor, IEnumerable<Point3d> points,
        int colorIndex = 1, bool drawHighlighted = false)
    {
        using var itor = points.GetEnumerator();
        while (itor.MoveNext())
        {
            var endPoint1 = itor.Current;
            if (!itor.MoveNext())
                return;
            var endPoint2 = itor.Current;
            editor.DrawVector(endPoint1, endPoint2, colorIndex, drawHighlighted);
        }
    }

    /// <summary>
    /// 根据点表绘制首尾相连的矢量
    /// </summary>
    /// <param name="editor">用户交互对象</param>
    /// <param name="points">点表</param>
    /// <param name="colorIndex">CAD颜色索引;默认：1为红色</param>
    /// <param name="isClose">是否闭合;<see langword="true"/> 为闭合,默认：<see langword="false"/> 为不闭合</param>
    /// <param name="drawHighlighted">是否高亮显示;<see langword="true"/>为高亮显示,默认：<see langword="false"/>为不高亮显示</param>
    public static void DrawEndToEndVectors(this Editor editor, IEnumerable<Point3d> points,
        int colorIndex = 1, bool isClose = false, bool drawHighlighted = false)
    {
        using var itor = points.GetEnumerator();
        if (!itor.MoveNext())
            return;
        Point3d endPoint1 = itor.Current, endPoint2 = new(), firstEndPoint = endPoint1;
        while (itor.MoveNext())
        {
            endPoint2 = itor.Current;
            editor.DrawVector(endPoint1, endPoint2, colorIndex, drawHighlighted);
            endPoint1 = endPoint2;
        }

        if (isClose)
            editor.DrawVector(endPoint2, firstEndPoint, colorIndex, drawHighlighted);
    }

    #endregion

    #region 矩阵

    /// <summary>
    /// 获取UCS到WCS的矩阵
    /// </summary>
    /// <param name="editor">命令行对象</param>
    /// <returns>变换矩阵</returns>
    public static Matrix3d GetMatrixFromUcsToWcs(this Editor editor)
    {
        return editor.CurrentUserCoordinateSystem;
    }

    /// <summary>
    /// 获取WCS到UCS的矩阵
    /// </summary>
    /// <param name="editor">命令行对象</param>
    /// <returns>变换矩阵</returns>
    public static Matrix3d GetMatrixFromWcsToUcs(this Editor editor)
    {
        return editor.CurrentUserCoordinateSystem.Inverse();
    }

    /// <summary>
    /// 获取MDCS(模型空间)到WCS的矩阵
    /// </summary>
    /// <param name="editor">命令行对象</param>
    /// <returns>变换矩阵</returns>
    public static Matrix3d GetMatrixFromMDcsToWcs(this Editor editor)
    {
        using var vtr = editor.GetCurrentView();
        var mat = Matrix3d.PlaneToWorld(vtr.ViewDirection);
        mat = Matrix3d.Displacement(vtr.Target - Point3d.Origin) * mat;
        return Matrix3d.Rotation(-vtr.ViewTwist, vtr.ViewDirection, vtr.Target) * mat;
    }

    /// <summary>
    /// 获取WCS到MDCS(模型空间)的矩阵
    /// </summary>
    /// <param name="editor">命令行对象</param>
    /// <returns>变换矩阵</returns>
    public static Matrix3d GetMatrixFromWcsToMDcs(this Editor editor)
    {
        return editor.GetMatrixFromMDcsToWcs().Inverse();
    }

    /// <summary>
    /// 获取MDCS(模型空间)到PDCS(图纸空间)的矩阵
    /// </summary>
    /// <param name="editor">命令行对象</param>
    /// <returns>变换矩阵</returns>
    public static Matrix3d GetMatrixFromMDcsToPDcs(this Editor editor)
    {
        if ((short)Env.GetVar("TILEMODE") == 1)
            throw new ArgumentException("TILEMODE == 1..Espace papier uniquement");

        var mat = Matrix3d.Identity;
        //using var tr = DBTrans.Create();
        var tr = DBTrans.GetTopTransaction(editor.Document.Database);
        if (tr is null)
        {
            throw new Exception("事务不存在");
        }

        var vp = tr.GetObject<Viewport>(editor.CurrentViewportObjectId);
        if (vp == null)
            return mat;

        if (vp.Number == 1)
        {
            try
            {
                editor.SwitchToModelSpace();
                vp = tr.GetObject<Viewport>(editor.CurrentViewportObjectId);
                editor.SwitchToPaperSpace();
            }
            catch
            {
                throw new Exception("Aucun fenêtre active...ErrorStatus.InvalidInput");
            }
        }

        if (vp == null)
            return mat;

        Point3d vCtr = new(vp.ViewCenter.X, vp.ViewCenter.Y, 0.0);
        mat = Matrix3d.Displacement(vCtr.GetAsVector().Negate());
        mat = Matrix3d.Displacement(vp.CenterPoint.GetAsVector()) * mat;
        mat = Matrix3d.Scaling(vp.CustomScale, vp.CenterPoint) * mat;
        return mat;
    }

    /// <summary>
    /// 获取PDCS(图纸空间)到MDCS(模型空间)的矩阵
    /// </summary>
    /// <param name="editor">命令行对象</param>
    /// <returns>变换矩阵</returns>
    public static Matrix3d GetMatrixFromPDcsToMDcs(this Editor editor)
    {
        return editor.GetMatrixFromMDcsToPDcs().Inverse();
    }

    /// <summary>
    /// 获取变换矩阵
    /// </summary>
    /// <param name="editor">命令行对象</param>
    /// <param name="from">源坐标系</param>
    /// <param name="to">目标坐标系</param>
    /// <returns>变换矩阵</returns>
    public static Matrix3d GetMatrix(this Editor editor, CoordinateSystemCode from,
        CoordinateSystemCode to)
    {
        return (from, to) switch
        {
            (CoordinateSystemCode.Wcs, CoordinateSystemCode.Ucs) => editor.GetMatrixFromWcsToUcs(),
            (CoordinateSystemCode.Wcs, CoordinateSystemCode.MDcs) =>
                editor.GetMatrixFromWcsToMDcs(),
            (CoordinateSystemCode.Ucs, CoordinateSystemCode.Wcs) => editor.GetMatrixFromUcsToWcs(),
            (CoordinateSystemCode.Ucs, CoordinateSystemCode.MDcs) =>
                editor.GetMatrixFromUcsToWcs() * editor.GetMatrixFromWcsToMDcs(),
            (CoordinateSystemCode.MDcs, CoordinateSystemCode.Wcs) =>
                editor.GetMatrixFromMDcsToWcs(),
            (CoordinateSystemCode.MDcs, CoordinateSystemCode.Ucs) =>
                editor.GetMatrixFromMDcsToWcs() * editor.GetMatrixFromWcsToUcs(),
            (CoordinateSystemCode.MDcs, CoordinateSystemCode.PDcs) =>
                editor.GetMatrixFromMDcsToPDcs(),
            (CoordinateSystemCode.PDcs, CoordinateSystemCode.MDcs) =>
                editor.GetMatrixFromPDcsToMDcs(),
            (CoordinateSystemCode.PDcs, CoordinateSystemCode.Wcs or CoordinateSystemCode.Ucs)
                or (CoordinateSystemCode.Wcs or CoordinateSystemCode.Ucs, CoordinateSystemCode.PDcs)
                => throw new Exception("To be used only with DCS...ErrorStatus.InvalidInput"),
            (_, _) => Matrix3d.Identity
        };

    }

    #endregion

    #region 缩放

    // TODO 暂时先屏蔽这个又臭又长的代码，待搞明白为什么都这么写之后再说
#if false
    /// <summary>
    /// 缩放窗口范围
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="minPoint">窗口左下点</param>
    /// <param name="maxPoint">窗口右上点</param>
    public static void ZoomWindow(this Editor ed, Point3d minPoint, Point3d maxPoint)
    {
        ViewTableRecord vtr = new();
        vtr.CopyFrom(ed.GetCurrentView());

        var oldpnts = new Point3d[] { minPoint, maxPoint };
        var pnts = new Point3d[8];
        var dpnts = new Point3d[8];

        var mat = ed.GetMatrixFromWcsToMDcs();
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
                for (int k = 0; k < 2; k++)
                {
                    int n = i * 4 + j * 2 + k;
                    pnts[n] = new Point3d(oldpnts[i][0], oldpnts[j][1], oldpnts[k][2]);
                    dpnts[n] = pnts[n].TransformBy(mat);
                }

        double xmin, xmax, ymin, ymax;
        xmin = xmax = dpnts[0][0];
        ymin = ymax = dpnts[0][1];
        for (int i = 1; i < 8; i++)
        {
            xmin = Math.Min(xmin, dpnts[i][0]);
            xmax = Math.Max(xmax, dpnts[i][0]);
            ymin = Math.Min(ymin, dpnts[i][1]);
            ymax = Math.Max(ymax, dpnts[i][1]);
        }

        vtr.Width = xmax - xmin;
        vtr.Height = ymax - ymin;
        vtr.CenterPoint = (dpnts[0] + (dpnts[7] - dpnts[0]) / 2)
                          .Convert2d(Curve2dEx._planeCache);

        ed.SetCurrentView(vtr);
        //ed.Regen();
    }
#endif

    /// <summary>
    /// 缩放窗口范围
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="ext">窗口范围点</param>
    /// <param name="offsetDist">偏移距离</param>
    public static void ZoomWindow(this Editor ed, Extents3d ext, double offsetDist = 0)
    {
        using var view = ed.GetCurrentView().CloneEx();
        var mt = Matrix3d.WorldToPlane(view.ViewDirection) *
                 Matrix3d.Displacement(Point3d.Origin - view.Target) *
                 Matrix3d.Rotation(view.ViewTwist, view.ViewDirection, view.Target);
        ext.TransformBy(mt);
        var width = ext.MaxPoint.X - ext.MinPoint.X + offsetDist * 2;
        var height = ext.MaxPoint.Y - ext.MinPoint.Y + offsetDist * 2;
        var ratio = view.Width / view.Height;
        if (width / height < ratio)
        {
            view.Height = height;
            view.Width = height * ratio;
        }
        else
        {
            view.Height = width / ratio;
            view.Width = width;
        }

        view.CenterPoint = ext.MinPoint.GetMidPointTo(ext.MaxPoint).Point2d();
        ed.SetCurrentView(view);
    }

    /// <summary>
    /// 按范围缩放
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="cenPt">中心点</param>
    /// <param name="width">窗口宽</param>
    /// <param name="height">窗口高</param>
    public static void Zoom(this Editor ed, Point3d cenPt, double width, double height)
    {
        using var vtr = ed.GetCurrentView();
        vtr.Width = width;
        vtr.Height = height;
        vtr.CenterPoint = cenPt.TransformBy(ed.GetMatrixFromWcsToMDcs()).Point2d();
        ed.SetCurrentView(vtr); // 更新当前视图
    }

    /// <summary>
    /// 缩放窗口范围
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="lpt">第一点</param>
    /// <param name="rpt">对角点</param>
    /// <param name="offsetDist">偏移距离</param>
    public static void ZoomWindow(this Editor ed, Point3d lpt, Point3d rpt,
        double offsetDist = 0.00)
    {
        Extents3d ext = new();
        ext.AddPoint(lpt);
        ext.AddPoint(rpt);
        ed.ZoomWindow(ext, offsetDist);
    }


    /// <summary>
    /// 获取有效的数据库范围
    /// </summary>
    /// <param name="db">数据库</param>
    /// <param name="extention">容差值:图元包围盒会超过数据库边界,用此参数扩大边界</param>
    /// <returns></returns>
    public static Extents3d? GetValidExtents3d(this Database db, double extention = 1e-6)
    {
        db.UpdateExt(true); // 更新当前模型空间的范围
        var ve = new Vector3d(extention, extention, extention);
        // 数据库没有图元的时候,min是大,max是小,导致新建出错
        // 数据如下:
        // min.X == 1E20 && min.Y == 1E20 && min.Z == 1E20 &&
        // max.X == -1E20 && max.Y == -1E20 && max.Z == -1E20)
        var a = db.Extmin;
        var b = db.Extmax;
        if (a.X < b.X && a.Y < b.Y)
            return new Extents3d(db.Extmin - ve, db.Extmax + ve);

        return null;
    }

    /// <summary>
    /// 动态缩放
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="offsetDist">偏移距离</param>
    public static void ZoomExtents(this Editor ed, double offsetDist = 0.00)
    {
        var db = ed.Document.Database;
        // db.UpdateExt(true); // GetValidExtents3d内提供了
        var dbExtent = db.GetValidExtents3d();
        if (dbExtent == null)
            ed.ZoomWindow(Point3d.Origin, new Point3d(1, 1, 0), offsetDist);
        else
            ed.ZoomWindow(db.Extmin, db.Extmax, offsetDist);
    }

    /// <summary>
    /// 根据实体对象的范围显示视图
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="ent">Entity对象</param>
    /// <param name="offsetDist">偏移距离</param>
    public static void ZoomObject(this Editor ed, Entity ent, double offsetDist = 0.00)
    {
        var ext = ent.GeometricExtents;
        ed.ZoomWindow(ext.MinPoint, ext.MaxPoint, offsetDist);
    }

    #endregion

    #region Get交互类

    /// <summary>
    /// 获取Point
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="message">提示信息</param>
    /// <param name="basePoint">提示使用的基点</param>
    /// <returns>交互结果</returns>
    public static PromptPointResult GetPoint(this Editor ed, string message, Point3d basePoint)
    {
        PromptPointOptions ptOp = new(message) { BasePoint = basePoint, UseBasePoint = true, AllowNone = true };
        return ed.GetPoint(ptOp);
    }

    /// <summary>
    /// 获取double值
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="message">提示信息</param>
    /// <param name="defaultValue">double默认值</param>
    /// <returns>交互结果</returns>
    public static PromptDoubleResult GetDouble(this Editor ed, string message,
        double defaultValue = 1.0)
    {
        PromptDoubleOptions douOp = new(message) { DefaultValue = defaultValue, AllowNone = true };
        return ed.GetDouble(douOp);
    }

    /// <summary>
    /// 获取int值
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="message">提示信息</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>交互结果</returns>
    public static PromptIntegerResult GetInteger(this Editor ed, string message,
        int defaultValue = 1)
    {
        PromptIntegerOptions douOp = new(message) { DefaultValue = defaultValue, AllowNone = true };
        return ed.GetInteger(douOp);
    }

    /// <summary>
    /// 获取string值
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="message">提示信息</param>
    /// <param name="defaultValue">string默认值</param>
    /// <returns></returns>
    public static PromptResult GetString(this Editor ed, string message, string defaultValue = "")
    {
        PromptStringOptions strOp = new(message)
        { DefaultValue = defaultValue, UseDefaultValue = !StringHelper.IsNullOrWhiteSpace(defaultValue) };
        return ed.GetString(strOp);
    }

    #endregion

    #region 执行lisp

    [DllImport("accore.dll", CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "acedInvoke")]
    private static extern int AcedInvoke(IntPtr args, out IntPtr result);

#if NET35
    [DllImport("acad.exe", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "?acedEvaluateLisp@@YAHPB_WAAPAUresbuf@@@Z")]
#else
    // 高版本此接口不能使用lisp(command "xx"),但是可以直接在自动运行接口上
    [DllImport("accore.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl,
        EntryPoint = "?acedEvaluateLisp@@YAHPEB_WAEAPEAUresbuf@@@Z")]
#endif
    [System.Security.SuppressUnmanagedCodeSecurity]// 初始化默认值
    static extern int AcedEvaluateLisp(string lispLine, out IntPtr result);

#if NET35
    [DllImport("acad.exe",
#else
    [DllImport("accore.dll",
#endif
    CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ads_queueexpr")]
    static extern int Ads_queueexpr(string strExpr);

    /// <summary>
    /// 执行lisp的方式枚举
    /// </summary>
    [Flags]
    public enum RunLispFlag : byte
    {
        /// <summary>
        /// 同步执行
        /// </summary>
        AdsQueueexpr = 1,
        /// <summary>
        /// 计算LISP表达式
        /// </summary>
        AcedEvaluateLisp = 2,
        /// <summary>
        /// 发送命令栏
        /// </summary>
        SendStringToExecute = 4,
    }

    /*
     * 测试命令:
     *   [CommandMethod(nameof(CmdTest_RunLisp))]
     *   public static void CmdTest_RunLisp()
     *   {
     *       var res = RunLisp("(setq abc 10)");
     *   }
     * 调用方式:
     *    (command "CmdTest_RunLisp1")
     * bug说明:
     *    AcedEvaluateLisp 接口
     *    在高版本调用时候没有运行成功,使得 !abc 没有值
     *    在cad08成功,此bug与CommandFlags无关
     * 解决方案:
     *   0x01 用异步接口,但是这样是显式调用了:
     *        (setq thisdrawing (vla-get-activedocument (vlax-get-acad-object)))(vla-SendCommand thisdrawing "CmdTest_RunLisp1 ")
     *   0x02 使用 Ads_queueexpr 接口
     */
    /// <summary>
    /// 发送lisp语句字符串到cad执行
    /// </summary>
    /// <param name="ed">编辑器对象</param>
    /// <param name="lispCode">lisp语句</param>
    /// <param name="flag">运行方式</param>
    /// <returns>缓冲结果,返回值</returns>
    public static ResultBuffer? RunLisp(this Editor ed, string lispCode,
        RunLispFlag flag = RunLispFlag.AdsQueueexpr)
    {
        if ((flag & RunLispFlag.AdsQueueexpr) == RunLispFlag.AdsQueueexpr)
        {
            // 这个在08/12发送lisp不会出错,但是发送bo命令出错了.
            // 0x01 设置 CommandFlags.Session 可以同步,
            // 0x02 自执行发送lisp都是异步,用来发送 含有(command)的lisp的
            _ = Ads_queueexpr(lispCode + "\n");
        }
        if ((flag & RunLispFlag.AcedEvaluateLisp) == RunLispFlag.AcedEvaluateLisp)
        {
            _ = AcedEvaluateLisp(lispCode, out var rb);
            if (rb != IntPtr.Zero)
                return (ResultBuffer)DisposableWrapper.Create(typeof(ResultBuffer), rb, true);
        }

        if ((flag & RunLispFlag.SendStringToExecute) == RunLispFlag.SendStringToExecute)
        {
            var dm = Acaop.DocumentManager;
            var doc = dm.MdiActiveDocument;
            doc?.SendStringToExecute(lispCode + "\n", false, false, false);
        }
        return null;
    }
    #endregion

    #region Export
    /// <summary>
    /// 输出WMF<br/>
    /// 此函数不适用于后台
    /// </summary>
    /// <param name="editor">命令行对象</param>
    /// <param name="saveFile">保存文件</param>
    /// <param name="ids">选择集的对象,为null时候手选</param>
    /// <param name="wmfSetDel">是否清空选择集</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    public static void ComExportWMF(this Editor editor, string saveFile, ObjectId[]? ids = null,
        bool wmfSetDel = false)
    {
        if (StringHelper.IsNullOrWhiteSpace(saveFile))
            throw new System.ArgumentNullException(nameof(saveFile));
        if (File.Exists(saveFile))
            throw new FileFormatException("文件重复:" + saveFile);

        var dm = Acap.DocumentManager;
        if (dm.Count == 0)
            return;

        // 剔除后缀
        saveFile = Path.Combine(Path.GetDirectoryName(saveFile) ?? string.Empty,
            Path.GetFileNameWithoutExtension(saveFile));
        // ActiveSelectionSet:
        // 第一次执行会触发选择,再次重复命令执行的时候,它会无法再选择(即使清空选择集).
        // 因此此处netAPI进行选择,它就能读取当前选择集缓冲区的对象
        if (ids == null || ids.Length == 0)
        {
            var psr = editor.SelectImplied();// 预选
            if (psr.Status != PromptStatus.OK)
                psr = editor.GetSelection();// 手选
            if (psr.Status != PromptStatus.OK)
                return;
            ids = psr.Value.GetObjectIds();
        }
        editor.SetImpliedSelection(ids);

#if zcad
        var com = Acap.ZcadApplication;
#else
        var com = Acap.AcadApplication;
#endif
        var doc = com.GetProperty("ActiveDocument");
        var wmfSet = doc.GetProperty("ActiveSelectionSet");

        // TODO #221007 导出wmf的bug
        // cad21 先net选择,再进行,此处再选择一次?
        // cad21 调试期间无法选择性粘贴?
        var exp = doc.Invoke("Export", saveFile, "wmf", wmfSet); // JPGOUT,PNGOUT
        if (wmfSetDel)
            wmfSet.Invoke("Delete");
    }
    #endregion

    #region JigEx

    /// <summary>
    /// jig前的准备工作，使图元暗显
    /// </summary>
    /// <param name="ed">命令栏</param>
    /// <param name="ents">实体（已存在数据库中）</param>
    public static void PrepareForJig(this Editor ed, params Entity[] ents)
    {
        ed.PrepareForJig(ents.ToList());
    }

    /// <summary>
    /// jig前的准备工作，使图元暗显
    /// </summary>
    /// <param name="ed">命令栏</param>
    /// <param name="ents">实体（已存在数据库中）</param>
    public static void PrepareForJig(this Editor ed, IEnumerable<Entity> ents)
    {
        var dic = new Dictionary<Entity, Color>();
        foreach (var ent in ents)
        {
            if (ent.IsNewObject)
                continue;
            dic.Add(ent, ent.Color);
            using (ent.ForWrite())
            {
                ent.ColorIndex = 250;
                ent.Draw();
            }
        }

        ed.Redraw();
        foreach (var kvp in dic)
        {
            var ent = kvp.Key;
            using (ent.ForWrite())
            {
                kvp.Key.Color = kvp.Value;
            }
        }
    }

    #endregion

    #region Extension

    /// <summary>
    /// 获取CAD鼠标当前位置坐标
    /// </summary>
    /// <param name="ed">命令栏</param>
    /// <returns>坐标(可能为null)</returns>
    public static Point3d? GetCurrentMouthPoint(this Editor ed)
    {
        return ed.RunLisp("(grread T)", RunLispFlag.AcedEvaluateLisp)
            ?.AsArray()
            .FirstOrDefault(tv => tv.TypeCode == 5009)
            .Value as Point3d?;
    }

    #endregion
}