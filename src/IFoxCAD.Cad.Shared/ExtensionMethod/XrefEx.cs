//#define error_demo
//#define test_demo

namespace IFoxCAD.Cad;

#if test_demo
public class TestCmd_BindXrefs
{
    //后台绑定
    [CommandMethod("MyXBindXrefs")]
    public static void MyXBindXrefs()
    {
        string newfile = @"D:\桌面\xxx\xxx.dwg";
        using var tr = new DBTrans(newfile,
            openMode: FileOpenMode.OpenForReadAndAllShare/*后台绑定特别注意*/);
        tr.XrefFactory(XrefModes.Bind);
        tr.SaveDwgFile();
    }

    //前台绑定
    [CommandMethod("MyXBindXrefs1")]
    public static void MyXBindXrefs1()
    {
        using var tr = new DBTrans();
        tr.XrefFactory(XrefModes.Bind);
        tr.SaveDwgFile();
    }
}
#endif

#region 参照绑定类
public static class XrefEx
{
    /// <summary>
    /// 外部参照
    /// </summary>
    /// <param name="tr"></param>
    /// <param name="xrefModes">处理参照的枚举</param>
    /// <param name="xrefNames">要处理的参照名称,<see langword="null"/>就处理所有</param>
    /// <param name="xrefModesBind">
    /// <para>
    /// 参数<paramref name="xrefModes"/>==<see cref="XrefModes.Bind"/>才触发<br/>
    /// 需要绑定的符号表:请保持默认<br/>
    /// 目前仅推荐用于<see cref="SymModes.LayerTable"/>项<br/>
    /// 其他项有异常: eWasOpenForNotify<br/>
    /// </para>
    /// </param>
    public static void XrefFactory(this DBTrans tr,
                                   XrefModes xrefModes,
                                   HashSet<string>? xrefNames = null,
                                   SymModes xrefModesBind = SymModes.LayerTable)
    {
        DatabaseEx.DBTextDeviation(tr.Database, () => {

            switch (xrefModes)
            {
                case XrefModes.Unload:
                {
                    var xrefIds = GetXrefNode(tr.Database, xrefNames);
                    if (xrefIds.Count > 0)
                        tr.Database.UnloadXrefs(xrefIds);
                }
                break;
                case XrefModes.Reload:
                {
                    var xrefIds = GetXrefNode(tr.Database, xrefNames);
                    if (xrefIds.Count > 0)
                        tr.Database.ReloadXrefs(xrefIds);
                }
                break;
                case XrefModes.Detach:
                {
                    var xrefIds = GetXrefNode(tr.Database, xrefNames);
                    foreach (ObjectId id in xrefIds)
                        tr.Database.DetachXref(id);
                }
                break;
                case XrefModes.Bind:
                {
                    //此功能有绑定出错的问题
                    //db.BindXrefs(xrefIds, true);

                    //绑定后会自动拆离
                    //此功能修补了上面缺失
                    BindXrefsEx(tr, xrefNames, xrefModesBind);
                }
                break;
                default:
                break;
            }
        });
    }

    /// <summary>
    /// 获取参照
    /// </summary>
    /// <param name="db"></param>
    /// <param name="names">过滤名称</param>
    /// <returns></returns>
    static ObjectIdCollection GetXrefNode(Database db, HashSet<string>? names)
    {
        //储存要处理的参照id
        var xrefIds = new ObjectIdCollection();
        XrefNodeForEach(db, (xNodeName, xNodeId, xNodeStatus, xNodeIsNested) => {
            //为空的时候全部加入 || 有内容时候含有目标
            if (names is null || names.Contains(xNodeName))
                xrefIds.Add(xNodeId);
        });
        return xrefIds;
    }

    /// <summary>
    /// 遍历参照
    /// </summary>
    /// <param name="db"></param>
    /// <param name="action">(参照名,参照块表记录id,参照状态,是否嵌入)</param>
    static void XrefNodeForEach(Database db, Action<string, ObjectId, XrefStatus, bool> action)
    {
        // btRec.IsFromOverlayReference 是覆盖
        // btRec.GetXrefDatabase(true) 外部参照数据库

        //解析外部参照:此功能不能锁定文档
        //useThreadEngine==true,线性引擎,会在cad命令历史打印一些AEC信息,并导致绑定慢一点...具体作用不详
        //doNewOnly==true,仅处理 Unresolved_未融入(未解析)的参照
        db.ResolveXrefs(useThreadEngine: false, doNewOnly: false);

        var xg = db.GetHostDwgXrefGraph(true);//参数:包含僵尸参照
        for (int i = 0; i < xg.NumNodes; i++)
        {
            var xNode = xg.GetXrefNode(i);
            if (!xNode.BlockTableRecordId.IsOk())
                continue;

            action.Invoke(xNode.Name,
                          xNode.BlockTableRecordId,
                          xNode.XrefStatus,
                          xNode.IsNested);
        }
    }


    /// <summary>
    /// 双重绑定参照
    /// <see href="https://www.cnblogs.com/SHUN-ONCET/p/16593360.html">参考链接</a>
    /// </summary>
    /// <param name="tr"></param>
    /// <param name="xrefNames">要处理的参照名称,<see langword="null"/>就处理所有</param>
    /// <param name="xrefModesBind">
    /// <para>
    /// 参数<paramref name="xrefModes"/>==<see cref="XrefModes.Bind"/>才触发<br/>
    /// 需要绑定的符号表:请保持默认<br/>
    /// 目前仅推荐用于<see cref="SymModes.LayerTable"/>项<br/>
    /// 其他项有异常: eWasOpenForNotify<br/>
    /// </para>
    /// </param>
    /// <param name="detachXref">是否拆离参照,默认true:学官方的绑定后自动拆离</param>
    /// <param name="eraseNested">是否删除被卸载的嵌套参照,默认true</param>
    static void BindXrefsEx(DBTrans tr,
                            HashSet<string>? xrefNames = null,
                            SymModes xrefModesBind = SymModes.LayerTable,
                            bool detachXref = true,
                            bool eraseNested = true)
    {
        //嵌套参照(块表记录id,名称)
        Dictionary<ObjectId, string> nested = new();
        DoubleBind(tr, nested, (xbindXrefsIds) => {

            // 起初测试是将九大符号表记录均加入的,但经实测不行...(为什么?存疑)
            #region Option1
            if ((xrefModesBind & SymModes.LayerTable) == SymModes.LayerTable)
                AddedxbindIds(xbindXrefsIds, tr.LayerTable);

            if ((xrefModesBind & SymModes.TextStyleTable) == SymModes.TextStyleTable)
                AddedxbindIds(xbindXrefsIds, tr.TextStyleTable);

            if ((xrefModesBind & SymModes.RegAppTable) == SymModes.RegAppTable)
                AddedxbindIds(xbindXrefsIds, tr.RegAppTable);

            if ((xrefModesBind & SymModes.DimStyleTable) == SymModes.DimStyleTable)
                AddedxbindIds(xbindXrefsIds, tr.DimStyleTable);

            if ((xrefModesBind & SymModes.LinetypeTable) == SymModes.LinetypeTable)
                AddedxbindIds(xbindXrefsIds, tr.LinetypeTable);
            #endregion

            #region Option2
            if ((xrefModesBind & SymModes.UcsTable) == SymModes.UcsTable)
                AddedxbindIds(xbindXrefsIds, tr.UcsTable);

            if ((xrefModesBind & SymModes.ViewTable) == SymModes.ViewTable)
                AddedxbindIds(xbindXrefsIds, tr.ViewTable);

            if ((xrefModesBind & SymModes.ViewportTable) == SymModes.ViewportTable)
                AddedxbindIds(xbindXrefsIds, tr.ViewportTable);
            #endregion

        }, xrefNames, detachXref);


        // 内部删除嵌套参照的块操作
        if (eraseNested)
        {
#if ac2008
            //因为Acad08索引器存在会暴露isErase的(桌子底层的原因),
            //也就是可能获取两个名称一样的,只能用遍历的方式进行
            HashSet<string> namess = new();
            foreach (var item in nested)
                namess.Add(item.Value);

            //遍历全图,找到参照名称一样的删除
            tr.BlockTable.ForEach(btr => {
                if (btr.IsLayout)
                    return;
                if (namess.Contains(btr.Name))
                {
                    btr.UpgradeOpen();
                    btr.Erase();
                    btr.DowngradeOpen();
                    btr.Dispose();
                }
            }, checkIdOk: true);
#else
            foreach (var item in nested)
            {
                var name = item.Value;
                if (tr.BlockTable.Has(name))
                    tr.GetObject<BlockTableRecord>(tr.BlockTable[name], OpenMode.ForWrite)?
                      .Erase();
            }
#endif
        }
    }

    /// <summary>
    /// 符号表记录加入容器
    /// </summary>
    static void AddedxbindIds<TTable, TRecord>(ObjectIdCollection xbindXrefsIds,
                              SymbolTable<TTable, TRecord> symbolTable)
                              where TTable : SymbolTable
                              where TRecord : SymbolTableRecord, new()
    {
        symbolTable.ForEach(tabRec => {
            if (tabRec.IsResolved)
                xbindXrefsIds.Add(tabRec.ObjectId);
        }, checkIdOk: true);
    }

    /// <summary>
    /// 双重绑定
    /// </summary>
    static void DoubleBind(DBTrans tr,
                           Dictionary<ObjectId, string> nested,
                           Action<ObjectIdCollection> xbindAction,
                           HashSet<string>? xrefNames = null,
                           bool detachXref = true)
    {
        //xbind
        //0x01 它是用来绑其他符号表,绑块表会有异常
        //0x02 集合若有问题,就会出现eWrongObjectType
        var xbindXrefsIds = new ObjectIdCollection();
        //bind 只绑块表
        var bindXrefsIds = new ObjectIdCollection();

        tr.BlockTable.ForEach(btr => {
            //外部参照 && 已融入
            if (btr.IsFromExternalReference && btr.IsResolved)
                bindXrefsIds.Add(btr.ObjectId);
        }, checkIdOk: true);

        // 直接拆离的id
        List<ObjectId> detachXrefIds = new();

        //收集要处理的id
        XrefNodeForEach(tr.Database, (xNodeName, xNodeId, xNodeStatus, xNodeIsNested) => {
            switch (xNodeStatus)
            {
                case XrefStatus.Unresolved://未融入_ResolveXrefs参数2
                break;
                case XrefStatus.FileNotFound://未融入(未解析)_未找到文件
                break;
                case XrefStatus.Unreferenced://未参照
                {
                    //为空的时候全部加入 || 有内容时候含有目标
                    if (xrefNames == null || xrefNames.Contains(xNodeName))
                        detachXrefIds.Add(xNodeId);
                }
                break;
                case XrefStatus.Unloaded://已卸载
                {
                    //为空的时候全部加入 || 有内容时候含有目标
                    if (xrefNames == null || xrefNames.Contains(xNodeName))
                    {
                        var btr = tr.GetObject<BlockTableRecord>(xNodeId);
                        if (btr != null && btr.IsFromExternalReference)
                        {
                            if (!xNodeIsNested)
                                detachXrefIds.Add(xNodeId);
                            else if (!nested.ContainsKey(xNodeId))
                                nested.Add(xNodeId, xNodeName);//嵌套参照
                        }
                    }
                }
                break;
                case XrefStatus.Resolved://已融入_就是可以绑定的
                break;
                case XrefStatus.NotAnXref://不是外部参照
                break;
                default:
                break;
            }
        });

        //拆离未参照的文件
        if (detachXref)
        {
            for (int i = 0; i < detachXrefIds.Count; i++)
                tr.Database.DetachXref(detachXrefIds[i]);
        }

        xbindAction?.Invoke(xbindXrefsIds);

        //嵌套参照被卸载则进行重载,才能进行绑定
        var keys = nested.Keys;
        if (keys.Count > 0)
            tr.Database.ReloadXrefs(new ObjectIdCollection(keys.ToArray()));

        //切勿交换,若交换秩序,则会绑定无效
        if (xbindXrefsIds.Count > 0)
            tr.Database.XBindXrefs(xbindXrefsIds, true);
        if (bindXrefsIds.Count > 0)
            tr.Database.BindXrefs(bindXrefsIds, true);
    }
}

#endregion

#region 参照路径工具类
/// <summary>
/// 获取外部参照的路径
/// </summary>
public class XrefPath
{
    #region 属性
    /// <summary>
    /// 基础路径
    /// </summary>
    public string CurrentDatabasePath;
    /// <summary>
    /// 是否外部参照
    /// </summary>
    public bool IsFromExternalReference { get; private set; }
    /// <summary>
    /// 外部参照保存的路径
    /// <para>
    /// 它们会是以下任一路径:<br/>
    /// 0x01 相对路径<br/>
    /// 0x02 绝对路径<br/>
    /// 0x03 共目录优先找到的路径(文件夹整体移动会发生此类情况)
    /// </para>
    /// </summary>
    public string? PathSave { get; private set; }
    /// <summary>
    /// 找到的路径(参照面板的名称)
    /// <para><see cref="PathSave"/>路径不存在时,返回是外部参照dwg文件路径</para>
    /// </summary>
    public string? PathDescribe { get; private set; }

    string? _PathComplete;
    /// <summary>
    /// 绝对路径
    /// </summary>
    public string? PathComplete => _PathComplete ??=
           PathConverter(CurrentDatabasePath, PathDescribe, PathConverterModes.Complete);

    string? _PathRelative;
    /// <summary>
    /// 相对路径
    /// </summary>
    public string? PathRelative => _PathRelative ??=
           PathConverter(CurrentDatabasePath, PathComplete, PathConverterModes.Relative);
    #endregion

    #region 构造
    /// <summary>
    /// 获取外部参照的路径
    /// </summary>
    /// <param name="brf">外部参照图元</param>
    /// <param name="tr">事务</param>
    /// <returns>是否外部参照</returns>
    public XrefPath(BlockReference brf, DBTrans tr)
    {
        if (brf == null)
            throw new ArgumentNullException(nameof(brf));

        CurrentDatabasePath = Path.GetDirectoryName(tr.Database.Filename);

        var btRec = tr.GetObject<BlockTableRecord>(brf.BlockTableRecord);//块表记录
        if (btRec == null)
            return;

        IsFromExternalReference = btRec.IsFromExternalReference;
        if (!IsFromExternalReference)
            return;

        //相对路径==".\\AA.dwg"
        //无路径=="AA.dwg"
        PathSave = btRec.PathName;

        if ((!string.IsNullOrEmpty(PathSave) && PathSave[0] == '.') || File.Exists(PathSave))
        {
            //相对路径||绝对路径
            PathDescribe = PathSave;
        }
        else
        {
            //无路径
            var db = btRec.GetXrefDatabase(true);
            PathDescribe = db.Filename;
        }
    }
    #endregion

    #region 静态函数  
    /// <summary>
    /// 获取相对路径或者绝对路径
    /// <see href="https://www.cnblogs.com/hont/p/5412340.html">参考链接</see>
    /// </summary>
    /// <param name="directory">基础目录(末尾无斜杠)</param>
    /// <param name="fileRelations">相对路径或者绝对路径</param>
    /// <param name="converterModes">依照枚举返回对应的字符串</param>
    /// <returns></returns>
    public static string? PathConverter(string? directory,
                                        string? fileRelations,
                                        PathConverterModes converterModes)
    {
        if (directory == null)
            throw new ArgumentNullException(nameof(directory));
        if (fileRelations == null)
            throw new ArgumentNullException(nameof(fileRelations));

        string? result = null;
        switch (converterModes)
        {
            case PathConverterModes.Relative:
            result = GetRelativePath(directory, fileRelations);
            break;
            case PathConverterModes.Complete:
            result = GetCompletePath(directory, fileRelations);
            break;
            default:
            break;
        }
        return result;
    }

#if error_demo
    /// <summary>
    /// 绝对路径->相对路径
    /// </summary>
    /// <param name="strDbPath">绝对路径</param>
    /// <param name="strXrefPath">相对关系</param>
    /// <returns></returns>
    /// StringHelper.GetRelativePath("G:\\A1.项目\\20190920金山谷黄宅\\01.饰施图\\03.平面图", 
    /// "G:\\A1.项目\\20190920金山谷黄宅\\01.饰施图\\01.辅助文件\\图框\\A3图框.dwg");
    public static string GetRelativePath(string strDbPath, string strXrefPath)
    {
        Uri uri1 = new(strXrefPath);
        Uri uri2 = new(strDbPath);
        Uri relativeUri = uri2.MakeRelativeUri(uri1);
        //测试例子变成 01.%E8%BE%85%E5%8A%A9%E6%96%87%E4%BB%B6/%E5%9B%BE%E6%A1%86/A3%E5%9B%BE%E6%A1%86.dwg
        string str = relativeUri.ToString();

        //因为这里不会实现".\A.dwg"而是"A.dwg",所以加入这个操作,满足同目录文件
        var strs = str.Split('\\');
        if (strs.Length == 1)
            str = ".\\" + str;
        return str;
    }
#else
    /// <summary>
    /// 绝对路径->相对路径
    /// </summary>
    /// <param name="directory">相对关系:文件夹路径</param>
    /// <param name="file">完整路径:文件路径</param>
    /// <returns>相对路径</returns>
    /// <![CDATA[
    /// GetRelativePath("G:\\A1.项目\\20190920金山谷黄宅\\01.饰施图\\03.平面图",
    /// "G:\\A1.项目\\20190920金山谷黄宅\\01.饰施图\\01.辅助文件\\图框\\A3图框.dwg")
    /// =>  "..\\01.辅助文件\\图框\\A3图框.dwg"
    /// ]]>
    static string GetRelativePath(string directory, string file)
    {
        string[] directorys = directory.Split('\\');
        string[] files = file.Split('\\');
        //获取两条路径中的最短路径
        int getMinLength = directorys.Length < files.Length ? directorys.Length : files.Length;

        //用于确定我们退出的循环中的位置。
        int lastCommonRoot = -1;
        int index;
        //找到共根
        for (index = 0; index < getMinLength; index++)
        {
            if (directorys[index] != files[index])
                break;
            lastCommonRoot = index;
        }
        //如果我们没有找到一个共同的前缀,那么抛出
        if (lastCommonRoot == -1)
            throw new ArgumentException("路径没有公共相同路径部分");

        //建立相对路径
        var result = new StringBuilder();
        for (index = lastCommonRoot + 1; index < directorys.Length; index++)
            if (directorys[index].Length > 0)
                result.Append("..\\");//上级目录加入

        //添加文件夹
        for (index = lastCommonRoot + 1; index < files.Length - 1; index++)
            result.Append(files[index] + "\\");

        //本级目录
        if (result.Length == 0)
            result.Append(".\\");
        //result.Append(strXrefPaths[^1]);//下级目录加入
        result.Append(files[files.Length - 1]);//下级目录加入
        return result.ToString();
    }
#endif

    /// <summary>
    /// 相对路径->绝对路径
    /// </summary>
    /// <param name="directory">文件夹路径</param>
    /// <param name="relativePath">相对关系:有..的</param> 
    /// <returns>完整路径</returns>
    /// <![CDATA[
    /// GetCompletePath("G:\\A1.项目\\20190920金山谷黄宅\\01.饰施图\\03.平面图" ,
    /// "..\\01.辅助文件\\图框\\A3图框.dwg")
    /// =>   "G:\\A1.项目\\20190920金山谷黄宅\\01.饰施图\\01.辅助文件\\图框\\A3图框.dwg"
    /// ]]>
    static string? GetCompletePath(string directory, string relativePath)
    {
        if (relativePath is null || relativePath.Trim() == string.Empty)
            return null;

        var relativeName = Path.GetDirectoryName(relativePath);
        if (relativeName is null)
            return null;

        if (relativePath[0] != '.')
            return relativePath;

        const char slash = '\\';

        //判断向上删除几个
        var path_xiangduis = relativeName.Split(slash);
        int index = 0;
        for (int i = 0; i < path_xiangduis.Length; i++)
        {
            if (path_xiangduis[i] != "..")
                break;
            index++;
        }

        var result = new StringBuilder();
        //前段
        var path_dwgs = directory.Split(slash);
        path_dwgs = path_dwgs.Where(s => !string.IsNullOrEmpty(s)).ToArray();//清理空数组
        for (int i = 0; i < path_dwgs.Length - index; i++)
        {
            result.Append(path_dwgs[i]);
            result.Append(slash);
        }
        //后段
        for (int i = 0; i < path_xiangduis.Length; i++)
        {
            var item = path_xiangduis[i];
            if (item != "." && item != "..")
            {
                result.Append(item);
                result.Append(slash);
            }
        }
        result.Append(Path.GetFileName(relativePath));
        return result.ToString();
    }
    #endregion
}
#endregion


public static class DBTransEx
{
    /// <summary>
    /// 清理符号表
    /// </summary>
    /// <param name="tr"></param>
    /// <param name="sym"></param>
    public static void Purge(this DBTrans tr, SymModes sym = SymModes.Purge)
    {
        var db = tr.Database;
        if ((sym & SymModes.BlockTable) == SymModes.BlockTable)
            DatabasePurge(db, tr.BlockTable);
        if ((sym & SymModes.DimStyleTable) == SymModes.DimStyleTable)
            DatabasePurge(db, tr.DimStyleTable);
        if ((sym & SymModes.LayerTable) == SymModes.LayerTable)
            DatabasePurge(db, tr.LayerTable);
        if ((sym & SymModes.LinetypeTable) == SymModes.LinetypeTable)
            DatabasePurge(db, tr.LinetypeTable);
        if ((sym & SymModes.TextStyleTable) == SymModes.TextStyleTable)
            DatabasePurge(db, tr.TextStyleTable);
        if ((sym & SymModes.ViewportTable) == SymModes.ViewportTable)
            DatabasePurge(db, tr.ViewportTable);
        if ((sym & SymModes.RegAppTable) == SymModes.RegAppTable)
            DatabasePurge(db, tr.RegAppTable);

        //以下不能这样清理,不然有异常
        if ((sym & SymModes.ViewTable) == SymModes.ViewTable)
            DatabasePurge(db, tr.ViewTable);
        if ((sym & SymModes.UcsTable) == SymModes.UcsTable)
            DatabasePurge(db, tr.UcsTable);
    }

    static void DatabasePurge<TTable, TRecord>(Database db,
                             SymbolTable<TTable, TRecord> symbolTable)
                             where TTable : SymbolTable
                             where TRecord : SymbolTableRecord, new()
    {
        var idArray = symbolTable.Select(id => id);
        if (!idArray.Any())
            return;
        var ids = new ObjectIdCollection(idArray.ToArray());
        while (ids.Count > 0)
            db.Purge(ids);
    }
}