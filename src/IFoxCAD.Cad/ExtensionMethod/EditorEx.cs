using System.Runtime.InteropServices;

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
    public static PromptSelectionResult SelectAtPoint(this Editor editor, Point3d point, SelectionFilter? filter = default)
    {
        return editor.SelectCrossingWindow(point, point, filter);
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

        var lays =
            DBTrans.Top.LayerTable
            .GetRecords()
            .Where(ltr => ltr.LineWeight == lineWeight)
            .Select(ltr => ltr.Name)
            .ToArray();

        if (lays.Length > 0)
        {
            filter =
                new OpOr
                {
                        filter,
                        new OpAnd
                        {
                            { 8, string.Join(",", lays) },
                            { 370, LineWeight.ByLayer }
                        }
                };
        }

        PromptSelectionResult res = editor.SelectAll(filter);
        return res.Value;
    }


    public static PromptSelectionResult? SSGet(this Editor editor, string? mode = null, SelectionFilter? filter = null, string[]? messages = null, Dictionary<string, Action>? keywords = null)
    {
        var pso = new PromptSelectionOptions();
        PromptSelectionResult? ss = null;
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
            pso.MessageForAdding = messages[0];
            pso.MessageForRemoval = messages[1];
        }

        if (keywords is not null)
        {
            foreach (var keyword in keywords.Keys)
            {
                pso.Keywords.Add(keyword);
            }
            if (pso.MessageForRemoval is null)
            {
                pso.MessageForAdding = "选择对象";
            }
            pso.MessageForAdding += $"[{string.Join(" / ", keywords.Keys.ToArray())}]";
            pso.KeywordInput += (s, e) => {
                if (keywords.ContainsKey(e.Input))
                {
                    keywords[e.Input].Invoke();
                }
            };

        }
        try
        {
            if (filter is not null)
            {
                ss = editor.GetSelection(pso, filter);
            }
            else
            {
                ss = editor.GetSelection(pso);
            }
        }
        catch (Autodesk.AutoCAD.Runtime.Exception e)
        {

            editor.WriteMessage($"\nKey is {e.Message}");
        }
        return ss;
    }

    //#region 即时选择样板

    ///// <summary>
    /////  即时选择,框选更新关键字
    ///// </summary>
    //public static void SelectTest()
    //{
    //    Env.Editor.WriteMessage("\n[白嫖工具]--测试");
    //    //激活选中事件
    //    Env.Editor.SelectionAdded += SelectTest_SelectionAdded;
    //    //初始化坐标系
    //    Env.Editor.CurrentUserCoordinateSystem = Matrix3d.Identity;

    //    //创建过滤器
    //    var sf = new OpEqual(0, "arc");
    //    var pso = new PromptSelectionOptions
    //    {
    //        MessageForAdding = "\n请选择对象:"
    //    };

    //    pso.Keywords.Add("Z");
    //    pso.Keywords.Add("X");
    //    pso.Keywords.Add("Q");
    //    //注册关键字
    //    pso.KeywordInput += SelectTest_KeywordInput;
    //    try
    //    {
    //        //用户选择
    //        var psr = Env.Editor.GetSelection(pso, sf);
    //        //处理代码


    //    }
    //    catch (Exception ex)//捕获关键字
    //    {
    //        if (ex.Message == "XuError")
    //        {
    //            //关闭关键字事件
    //            pso.KeywordInput -= SelectTest_KeywordInput;
    //            //关闭选中事件
    //            Env.Editor.SelectionAdded -= SelectTest_SelectionAdded;
    //            //重新调用自身
    //            ZengLiangYuanJiao();
    //        }
    //    }
    //    //关闭关键字事件
    //    pso.KeywordInput -= SelectTest_KeywordInput;
    //    //关闭选中事件
    //    Env.Editor.SelectionAdded -= SelectTest_SelectionAdded;
    //}

    ///// <summary>
    ///// 即时选择
    ///// </summary>
    ///// <param name="sender"></param>
    ///// <param name="e"></param>
    //private static void SelectTest_SelectionAdded(object sender, SelectionAddedEventArgs e)
    //{
    //    //关闭选中事件
    //    Env.Editor.SelectionAdded -= SelectTest_SelectionAdded;
    //    using (var tr = new DBTrans())
    //    {
    //        //处理代码
    //        for (int i = 0; i < e.AddedObjects.Count; i++)
    //        {


    //            //处理完移除已处理的对象
    //            e.Remove(i);
    //        }
    //    }
    //    //激活选中事件
    //    Env.Editor.SelectionAdded += SelectTest_SelectionAdded;
    //}

    ///// <summary>
    ///// 关键字响应
    ///// </summary>
    ///// <param name="sender"></param>
    ///// <param name="e"></param>
    //private static void SelectTest_KeywordInput(object sender, SelectionTextInputEventArgs e)
    //{
    //    //获取关键字
    //    switch (e.Input)
    //    {
    //        case "Z":
    //            {
    //                break;
    //            }
    //        case "X":
    //            {
    //                break;
    //            }

    //        case "Q":
    //            {
    //                break;
    //            }
    //    }
    //    //抛出异常,用于更新提示信息
    //    throw new ArgumentException("XuError");
    //}


    //#endregion
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
    }//

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
        catch (System.Exception ex)
        {
            Message(ex);
        }
    }//

    /// <summary>
    /// 异常信息对话框
    /// </summary>
    /// <param name="ex">异常</param>
    public static void Message(System.Exception ex)
    {
        try
        {
            System.Windows.Forms.MessageBox.Show(
                ex.ToString(),
                "Error",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Error);
        }
        catch
        {
        }
    }//

    /// <summary>
    /// 提示信息对话框
    /// </summary>
    /// <param name="caption">对话框的标题</param>
    /// <param name="message">对话框文本</param>
    public static void InfoMessageBox(string caption, string message)
    {
        try
        {
            System.Windows.Forms.MessageBox.Show(
                message,
                caption,
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Information);
        }
        catch (System.Exception ex)
        {
            Message(ex);
        }
    }//

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
    /// 提示信息对话框，默认标题为NFox.Cad
    /// </summary>
    /// <param name="message">对话框文本</param>
    public static void InfoMessageBox(string message)
    {
        InfoMessageBox("NFox.Cad", message);
    }//

    /// <summary>
    /// 提示信息对话框
    /// </summary>
    /// <param name="format">带格式化项的对话框文本</param>
    /// <param name="args">指定格式化的对象数组</param>
    public static void InfoMessageBox(string format, params object[] args)
    {
        InfoMessageBox(string.Format(format, args));
    }//

    /// <summary>
    /// 命令行打印字符串
    /// </summary>
    /// <param name="message">字符串</param>
    public static void WriteMessage(string message)
    {
        try
        {
            if (Acceptable())
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n" + message);
            else
                return;
        }
        catch (System.Exception ex)
        {
            Message(ex);
        }
    }//

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
    /// <returns><see langword="true"/>有，<see langword="false"/>没有</returns>
    public static bool HasEditor()
    {
        return Application.DocumentManager.MdiActiveDocument is not null
            && Application.DocumentManager.Count != 0
            && Application.DocumentManager.MdiActiveDocument.Editor is not null;
    }//

    /// <summary>
    /// 判断是否可以打印字符串
    /// </summary>
    /// <returns><see langword="true"/>可以打印，<see langword="false"/>不可以打印</returns>
    public static bool Acceptable()
    {
        return HasEditor()
            && !Application.DocumentManager.MdiActiveDocument.Editor.IsDragging;
    }//

    #endregion Info

    #region 画矢量线

    /// <summary>
    /// 根据点表返回矢量线的列表
    /// </summary>
    /// <param name="pnts">点表</param>
    /// <param name="isClosed">是否闭合，<see langword="true"/> 为闭合，<see langword="false"/> 为不闭合</param>
    /// <returns></returns>
    public static List<TypedValue> GetLines(IEnumerable<Point2d> pnts, bool isClosed)
    {

        var itor = pnts.GetEnumerator();
        if (!itor.MoveNext())
            return new List<TypedValue>();

        List<TypedValue> values = new();

        TypedValue tvFirst = new((int)LispDataType.Point2d, itor.Current);
        TypedValue tv1;
        TypedValue tv2 = tvFirst;

        while (itor.MoveNext())
        {
            tv1 = tv2;
            tv2 = new TypedValue((int)LispDataType.Point2d, itor.Current);
            values.Add(tv1);
            values.Add(tv2);
        }

        if (isClosed)
        {
            values.Add(tv2);
            values.Add(tvFirst);
        }

        return values;
    }

    /// <summary>
    /// 画矢量线
    /// </summary>
    /// <param name="editor">编辑器对象</param>
    /// <param name="pnts">点表</param>
    /// <param name="colorIndex">颜色码</param>
    /// <param name="isClosed">是否闭合，<see langword="true"/> 为闭合，<see langword="false"/> 为不闭合</param>
    public static void DrawVectors(this Editor editor, IEnumerable<Point2d> pnts, short colorIndex, bool isClosed)
    {
        var rlst =
            new LispList { { LispDataType.Int16, colorIndex } };
        rlst.AddRange(GetLines(pnts, isClosed));
        editor.DrawVectors(rlst, editor.CurrentUserCoordinateSystem);
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
    public static void DrawCircles(this Editor editor, IEnumerable<Point2d> pnts, short colorIndex, double radius, int numEdges)
    {
        var rlst =
            new LispList { { LispDataType.Int16, colorIndex } };

        foreach (Point2d pnt in pnts)
        {
            Vector2d vec = Vector2d.XAxis * radius;
            double angle = Math.PI * 2 / numEdges;

            List<Point2d> tpnts = new()
            {
                pnt + vec
            };
            for (int i = 1; i < numEdges; i++)
            {
                tpnts.Add(pnt + vec.RotateBy(angle * i));
            }

            rlst.AddRange(GetLines(tpnts, true));
        }
        editor.DrawVectors(rlst, editor.CurrentUserCoordinateSystem);
    }

    /// <summary>
    /// 用矢量线画近似圆（正多边形）
    /// </summary>
    /// <param name="editor">编辑器对象</param>
    /// <param name="pnt">点</param>
    /// <param name="colorIndex">颜色码</param>
    /// <param name="radius">半径</param>
    /// <param name="numEdges">多边形边的个数</param>
    public static void DrawCircle(this Editor editor, Point2d pnt, short colorIndex, double radius, int numEdges)
    {
        Vector2d vec = Vector2d.XAxis * radius;
        double angle = Math.PI * 2 / numEdges;

        List<Point2d> pnts = new()
        {
            pnt + vec
        };
        for (int i = 1; i < numEdges; i++)
        {
            pnts.Add(pnt + vec.RotateBy(angle * i));
        }

        editor.DrawVectors(pnts, colorIndex, true);
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
        Matrix3d mat;
        using ViewTableRecord vtr = editor.GetCurrentView();
        mat = Matrix3d.PlaneToWorld(vtr.ViewDirection);
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
            throw new Autodesk.AutoCAD.Runtime.Exception(ErrorStatus.InvalidInput, "Espace papier uniquement");

        Database db = editor.Document.Database;
        Matrix3d mat;
        using (Transaction tr = db.TransactionManager.StartTransaction())
        {
            Viewport? vp = tr.GetObject(editor.CurrentViewportObjectId, OpenMode.ForRead) as Viewport;
            if (vp?.Number == 1)
            {
                try
                {
                    editor.SwitchToModelSpace();
                    vp = tr.GetObject(editor.CurrentViewportObjectId, OpenMode.ForRead) as Viewport;
                    editor.SwitchToPaperSpace();
                }
                catch
                {
                    throw new Autodesk.AutoCAD.Runtime.Exception(ErrorStatus.InvalidInput, "Aucun fenêtre active");
                }
            }
            Point3d vCtr = new(vp!.ViewCenter.X, vp.ViewCenter.Y, 0.0);
            mat = Matrix3d.Displacement(vCtr.GetAsVector().Negate());
            mat = Matrix3d.Displacement(vp.CenterPoint.GetAsVector()) * mat;
            mat = Matrix3d.Scaling(vp.CustomScale, vp.CenterPoint) * mat;
            tr.Commit();
        }
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
    public static Matrix3d GetMatrix(this Editor editor, CoordinateSystemCode from, CoordinateSystemCode to)
    {
#if ac2009
        switch (from)
        {
            case CoordinateSystemCode.Wcs:
                switch (to)
                {
                    case CoordinateSystemCode.Ucs:
                        return editor.GetMatrixFromWcsToUcs();

                    case CoordinateSystemCode.MDcs:
                        return editor.GetMatrixFromMDcsToWcs();

                    case CoordinateSystemCode.PDcs:
                        throw new Autodesk.AutoCAD.Runtime.Exception(
                            ErrorStatus.InvalidInput,
                            "To be used only with DCS");
                }
                break;
            case CoordinateSystemCode.Ucs:
                switch (to)
                {
                    case CoordinateSystemCode.Wcs:
                        return editor.GetMatrixFromUcsToWcs();

                    case CoordinateSystemCode.MDcs:
                        return editor.GetMatrixFromUcsToWcs() * editor.GetMatrixFromWcsToMDcs();

                    case CoordinateSystemCode.PDcs:
                        throw new Autodesk.AutoCAD.Runtime.Exception(
                            ErrorStatus.InvalidInput,
                            "To be used only with DCS");
                }
                break;
            case CoordinateSystemCode.MDcs:
                switch (to)
                {
                    case CoordinateSystemCode.Wcs:
                        return editor.GetMatrixFromMDcsToWcs();

                    case CoordinateSystemCode.Ucs:
                        return editor.GetMatrixFromMDcsToWcs() * editor.GetMatrixFromWcsToUcs();

                    case CoordinateSystemCode.PDcs:
                        return editor.GetMatrixFromMDcsToPDcs();
                }
                break;
            case CoordinateSystemCode.PDcs:
                switch (to)
                {
                    case CoordinateSystemCode.Wcs:
                        throw new Autodesk.AutoCAD.Runtime.Exception(
                            ErrorStatus.InvalidInput,
                            "To be used only with DCS");
                    case CoordinateSystemCode.Ucs:
                        throw new Autodesk.AutoCAD.Runtime.Exception(
                            ErrorStatus.InvalidInput,
                            "To be used only with DCS");
                    case CoordinateSystemCode.MDcs:
                        return editor.GetMatrixFromPDcsToMDcs();
                }
                break;
        }
        return Matrix3d.Identity;
#else
        return (from, to) switch
        {
            (CoordinateSystemCode.Wcs, CoordinateSystemCode.Ucs) => editor.GetMatrixFromWcsToUcs(),
            (CoordinateSystemCode.Wcs, CoordinateSystemCode.MDcs) => editor.GetMatrixFromMDcsToWcs(),
            (CoordinateSystemCode.Ucs, CoordinateSystemCode.Wcs) => editor.GetMatrixFromUcsToWcs(),
            (CoordinateSystemCode.Ucs, CoordinateSystemCode.MDcs) => editor.GetMatrixFromUcsToWcs() * editor.GetMatrixFromWcsToMDcs(),
            (CoordinateSystemCode.MDcs, CoordinateSystemCode.Wcs) => editor.GetMatrixFromMDcsToWcs(),
            (CoordinateSystemCode.MDcs, CoordinateSystemCode.Ucs) => editor.GetMatrixFromMDcsToWcs() * editor.GetMatrixFromWcsToUcs(),
            (CoordinateSystemCode.MDcs, CoordinateSystemCode.PDcs) => editor.GetMatrixFromMDcsToPDcs(),
            (CoordinateSystemCode.PDcs, CoordinateSystemCode.MDcs) => editor.GetMatrixFromPDcsToMDcs(),
            (CoordinateSystemCode.PDcs, CoordinateSystemCode.Wcs or CoordinateSystemCode.Ucs)
            or (CoordinateSystemCode.Wcs or CoordinateSystemCode.Ucs, CoordinateSystemCode.PDcs) => throw new Autodesk.AutoCAD.Runtime.Exception(ErrorStatus.InvalidInput, "To be used only with DCS"),
            (_, _) => Matrix3d.Identity
        };
#endif
    }

#endregion

#region 缩放

    /// <summary>
    /// 缩放窗口范围
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="minPoint">窗口左下点</param>
    /// <param name="maxPoint">窗口右上点</param>
    public static void ZoomWindow(this Editor ed, Point3d minPoint, Point3d maxPoint)
    {
        ViewTableRecord cvtr = ed.GetCurrentView();
        ViewTableRecord vtr = new();
        vtr.CopyFrom(cvtr);

        Point3d[] oldpnts = new Point3d[] { minPoint, maxPoint };
        Point3d[] pnts = new Point3d[8];
        Point3d[] dpnts = new Point3d[8];
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    int n = i * 4 + j * 2 + k;
                    pnts[n] = new Point3d(oldpnts[i][0], oldpnts[j][1], oldpnts[k][2]);
                    dpnts[n] = pnts[n].TransformBy(ed.GetMatrixFromWcsToMDcs());
                }
            }
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
        vtr.CenterPoint = (dpnts[0] + (dpnts[7] - dpnts[0]) / 2).Convert2d(new Plane());

        ed.SetCurrentView(vtr);
        ed.Regen();
    }

    /// <summary>
    /// 缩放窗口范围
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="ext">窗口范围点</param>
    public static void ZoomWindow(this Editor ed, Extents3d ext)
    {
        ZoomWindow(ed, ext.MinPoint, ext.MaxPoint);
    }

    /// <summary>
    /// 缩放比例
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="CenPt">中心点</param>
    /// <param name="width">窗口宽</param>
    /// <param name="height">窗口高</param>
    public static void Zoom(this Editor ed, Point3d CenPt, double width, double height)
    {
        using ViewTableRecord view = ed.GetCurrentView();
        view.Width = width;
        view.Height = height;
        view.CenterPoint = new Point2d(CenPt.X, CenPt.Y);
        ed.SetCurrentView(view);//更新当前视图
    }

    /// <summary>
    ///缩放窗口范围
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="lpt">第一点</param>
    /// <param name="rpt">对角点</param>
    /// <param name="offsetDist">偏移距离</param>
    public static void ZoomWindow(this Editor ed, Point3d lpt, Point3d rpt, double offsetDist = 0.00)
    {
        Extents3d extents = new();
        extents.AddPoint(lpt);
        extents.AddPoint(rpt);
        rpt = extents.MaxPoint + new Vector3d(offsetDist, offsetDist, 0);
        lpt = extents.MinPoint - new Vector3d(offsetDist, offsetDist, 0);
        Vector3d ver = rpt - lpt;
        ed.Zoom(lpt + ver / 2, ver.X, ver.Y);
    }


    /// <summary>
    /// 获取有效的数据库范围
    /// </summary>
    /// <param name="db">数据库</param>
    /// <param name="dbExtent">范围</param>
    /// <returns>数据库没有图元返回 true, 反之返回 false</returns>
    public static bool GetValidExtents3d(this Database db, out Extents3d dbExtent)
    {
        dbExtent = new Extents3d(Point3d.Origin, new Point3d(1, 1, 0));
        //数据库没有图元的时候,min是大,max是小,导致新建出错
        var a = db.Extmin;
        var b = db.Extmax;
        var ve = new Vector3d(1, 1, 0);
        if (!(a.X == 1E20 && a.Y == 1E20 && a.Z == 1E20 &&
              b.X == -1E20 && b.Y == -1E20 && b.Z == -1E20))
        {
            dbExtent = new Extents3d(db.Extmin - ve, db.Extmax + ve);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 动态缩放
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="offsetDist">偏移距离</param>
    public static void ZoomExtents(this Editor ed, double offsetDist = 0.00)
    {
        Database db = ed.Document.Database;
        db.UpdateExt(true);

        if (db.GetValidExtents3d(out Extents3d extents3D))
        {
            ed.ZoomWindow(extents3D.MinPoint, extents3D.MaxPoint, offsetDist);
            return;
        }
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
        Extents3d ext = ent.GeometricExtents;
        ed.ZoomWindow(ext.MinPoint, ext.MaxPoint, offsetDist);
    }

#endregion

#region Get交互类

    /// <summary>
    /// 获取Point
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="Message">提示信息</param>
    /// <param name="BasePoint">提示使用的基点</param>
    /// <returns></returns>
    public static PromptPointResult GetPoint(this Editor ed, string Message, Point3d BasePoint)
    {
        PromptPointOptions ptOp = new(Message)
        {
            BasePoint = BasePoint,
            UseBasePoint = true
        };
        return ed.GetPoint(ptOp);
    }

    /// <summary>
    /// 获取double值
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="Message">提示信息</param>
    /// <param name="DefaultValue">double默认值</param>
    /// <returns></returns>
    public static PromptDoubleResult GetDouble(this Editor ed, string Message, double DefaultValue = 1.0)
    {
        PromptDoubleOptions douOp = new(Message)
        {
            DefaultValue = DefaultValue
        };
        return ed.GetDouble(douOp);
    }

    /// <summary>
    /// 获取int值
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="Message">提示信息</param>
    /// <param name="DefaultValue">double默认值</param>
    /// <returns></returns>
    public static PromptIntegerResult GetInteger(this Editor ed, string Message, int DefaultValue = 1)
    {
        PromptIntegerOptions douOp = new(Message)
        {
            DefaultValue = DefaultValue
        };
        return ed.GetInteger(douOp);
    }

    /// <summary>
    /// 获取string值
    /// </summary>
    /// <param name="ed">命令行对象</param>
    /// <param name="Message">提示信息</param>
    /// <param name="DefaultValue">string默认值</param>
    /// <returns></returns>
    public static PromptResult GetString(this Editor ed, string Message, string DefaultValue = "")
    {
        PromptStringOptions strOp = new(Message)
        {
            DefaultValue = DefaultValue
        };
        return ed.GetString(strOp);
    }

#endregion Get交互类

#region 执行lisp

#if ac2009
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("acad.exe", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acedEvaluateLisp@@YAHPB_WAAPAUresbuf@@@Z")]
        private static extern int AcedEvaluateLisp(string lispLine, out IntPtr result);

        [DllImport("acad.exe", CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedInvoke")]
        private static extern int AcedInvoke(IntPtr args, out IntPtr result);
#else
    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("accore.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?acedEvaluateLisp@@YAHPEB_WAEAPEAUresbuf@@@Z")]
    private static extern int AcedEvaluateLisp(string lispLine, out IntPtr result);

    [DllImport("accore.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedInvoke")]
    private static extern int AcedInvoke(IntPtr args, out IntPtr result);
#endif
    /// <summary>
    /// 发送lisp语句字符串到cad执行
    /// </summary>
    /// <param name="ed">编辑器对象</param>
    /// <param name="arg">lisp语句</param>
    /// <returns>缓冲结果,返回值</returns>
#pragma warning disable IDE0060 // 删除未使用的参数
    public static ResultBuffer? RunLisp(this Editor ed, string arg)
#pragma warning restore IDE0060 // 删除未使用的参数
    {
        _ = AcedEvaluateLisp(arg, out IntPtr rb);
        if (rb != IntPtr.Zero)
        {
            try
            {
                var rbb = DisposableWrapper.Create(typeof(ResultBuffer), rb, true) as ResultBuffer;
                return rbb;
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

#endregion 执行lisp
}
