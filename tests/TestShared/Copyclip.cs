#define test
#define COPYCLIP
#define PASTECLIP

namespace Test;
using Autodesk.AutoCAD.DatabaseServices;
using System.Threading;

/*
 * 0x01 (已完成)
 * 跨cad复制,由于高版本会保存为当前dwg格式,所以我们将所有都保存为07格式(有动态块),
 * 就可以多个版本cad相互复制粘贴了
 *
 * 0x02
 * 设置一个粘贴板栈,用tmp.config储存(路径和粘贴基点),
 * ctrl+shfit+v v v 就是三次前的剪贴板内容;也可以制作一个剪贴板窗口更好给用户交互
 *
 * 0x03
 * 天正图元的复制粘贴出错原因
 *
 * 引用技术贴:
 * https://forums.autodesk.com/t5/net/paste-list-of-objects-from-clipboard-on-dwg-file-using-c-net/td-p/6797606
 */
public class Copyclip
{
    #region 命令
#if test
    [IFoxInitialize]
    public void Init()
    {
        Acap.DocumentManager.DocumentLockModeChanged
            += DocumentManager_DocumentLockModeChanged;
    }

    /// <summary>
    /// 反应器->命令否决触发命令前(不可锁文档)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void DocumentManager_DocumentLockModeChanged(object sender, DocumentLockModeChangedEventArgs e)
    {
        var up = e.GlobalCommandName.ToUpper();
        string? cmd = null;
#if COPYCLIP
        if (up == "COPYCLIP")// 复制
        {
            e.Veto();
            cmd = nameof(IFoxCopyClip);
        }
        else if (up == "COPYBASE") //ctrl+shift+c 带基点复制
        {
            e.Veto();
            cmd = nameof(IFoxCopyBase);
        }
        else if (up == "CUTCLIP") // 剪切
        {
            e.Veto();
            cmd = nameof(IFoxCutclip);
        }
#endif
#if PASTECLIP
        if (up == "PASTECLIP")// 粘贴
        {
            //TODO === 完成之后此处将会移除
            var getClip = ClipTool.GetClipboard(ClipboardEnv.CadVer, out TagClipboardInfo tag);
            if (!getClip)
                return;
            //=== 完成之后此处将会移除

            e.Veto();
            cmd = nameof(IFoxPasteClip);
        }
        else if (up == "PASTEBLOCK") //ctrl+shift+v 粘贴为块
        {
            //TODO === 完成之后此处将会移除
            var getClip = ClipTool.GetClipboard(ClipboardEnv.CadVer, out TagClipboardInfo tag);
            if (!getClip)
                return;
            //=== 完成之后此处将会移除

            e.Veto();
            cmd = nameof(IFoxPasteBlock);
        }
#endif
        if (cmd != null)
        {
            var dm = Acap.DocumentManager;
            if (dm.Count == 0)
                return;
            var doc = dm.MdiActiveDocument;
            // 发送命令是因为导出WMF函数的com需要命令形式,否则将报错
            // 但是发送命令会导致选择集被取消了,那么就需要设置 CommandFlags.Redraw
            doc.SendStringToExecute(cmd + "\n", true, false, false);
        }
    }

    /// <summary>
    /// 复制
    /// </summary>
    [CommandMethod(nameof(IFoxCopyClip), CommandFlags.UsePickSet | CommandFlags.Redraw)]
    public void IFoxCopyClip()
    {
        Copy(false);
    }
    /// <summary>
    /// 带基点复制
    /// </summary>
    [CommandMethod(nameof(IFoxCopyBase), CommandFlags.UsePickSet | CommandFlags.Redraw)]
    public void IFoxCopyBase()
    {
        Copy(true);
    }
    /// <summary>
    /// 剪切
    /// </summary>
    [CommandMethod(nameof(IFoxCutclip), CommandFlags.UsePickSet | CommandFlags.Redraw)]
    public void IFoxCutclip()
    {
        Copy(false, true);
    }


    /// <summary>
    /// 粘贴
    /// </summary>
    [CommandMethod(nameof(IFoxPasteClip))]
    public void IFoxPasteClip()
    {
        Paste(false);
    }
    /// <summary>
    /// 粘贴为块
    /// </summary>
    [CommandMethod(nameof(IFoxPasteBlock))]
    public void IFoxPasteBlock()
    {
        Paste(true);
    }
#endif
    #endregion

    // 想要重启cad之后还可以继续用剪贴板,那么就不要这个
    // 会出现永远存在临时文件夹的情况:
    // 0x01 复制的时候,无法删除占用中的,
    // 0x02 调试期间直接退出acad.exe
    // [IFoxInitialize(isInitialize: false)]
    public void Terminate()
    {
        // 此处要先去删除tmp文件夹的上次剪贴板产生的dwg文件
        for (int i = 0; i < _delFile.Count; i++)
        {
            if (!File.Exists(_delFile[i]))
            {
                _delFile.RemoveAt(i);
                continue;
            }

            try
            {
                File.Delete(_delFile[i]);
                _delFile.RemoveAt(i);
            }
            catch { Env.Printl("无法删除(是否占用):" + _delFile[i]); }
        }
    }


    /// <summary>
    /// 储存准备删除的文件
    /// 也可以用txt代替
    /// 如果删除出错(占用),将一直在这个集合中,直到cad关闭
    /// </summary>
    readonly List<string> _delFile = new();

    /// <summary>
    /// 复制
    /// </summary>
    /// <param name="getPoint"></param>
    void Copy(bool getPoint, bool isEraseSsget = false)
    {
        var dm = Acap.DocumentManager;
        if (dm.Count == 0)
            return;
        var doc = dm.MdiActiveDocument;

        if (doc.Editor == null)
            return;
        var psr = doc.Editor.SelectImplied();// 预选
        if (psr.Status != PromptStatus.OK)
            psr = doc.Editor.GetSelection();// 手选
        if (psr.Status != PromptStatus.OK)
            return;

        // 设置基点
        Point3d pt = Point3d.Origin;
        var idArray = psr.Value.GetObjectIds();

        var tempFile = CreateTempFileName();
        while (File.Exists(tempFile) ||
               File.Exists(Path.ChangeExtension(tempFile, "wmf")))
        {
            tempFile = CreateTempFileName();
            Thread.Sleep(1);
        }

        using var tr = new DBTrans();

        #region 写入 AutoCAD.R17 数据
        if (getPoint)
        {
            var pr = doc.Editor.GetPoint("\n选择基点");
            if (pr.Status != PromptStatus.OK)
                return;
            pt = pr.Value;
        }
        else
        {
            // 遍历块内
            // 获取左下角点作为基点
            double minx = double.MaxValue;
            double miny = double.MaxValue;
            double minz = double.MaxValue;
            foreach (var id in idArray)
            {
                var ent = tr.GetObject<Entity>(id);
                if (ent == null)
                    continue;
                var info = ent.GetBoundingBoxEx();
                if (ent is BlockReference brf)
                    info.Move(brf.Position, Point3d.Origin);
                minx = minx > info.MinX ? info.MinX : minx;
                miny = miny > info.MinY ? info.MinY : miny;
                minz = minz > info.MinZ ? info.MinZ : minz;
            }
            pt = new(minx, miny, minz);
        }

        var cadClipType = new TagClipboardInfo(tempFile, pt);

        // 克隆到目标块表内
        using (var fileTr = new DBTrans(cadClipType.File))
        {
            fileTr.Task(() => {
                var map = new IdMapping();
                using var ids = new ObjectIdCollection(idArray);
                tr.Database.WblockCloneObjects(
                    ids,
                    fileTr.ModelSpace.ObjectId,
                    map,
                    DuplicateRecordCloning.Replace,
                    false);
            });

            // 大于dwg07格式的,保存为07,以实现高低版本通用剪贴板
            // 小于dwg07格式的,本工程没有支持cad06dll,所以这里仅为展示
            if ((int)DwgVersion.Current >= 27)
            {
                fileTr.SaveFile((DwgVersion)27, false);
            }
            else
            {
#pragma warning disable CS0162 // 检测到无法访问的代码
                fileTr.SaveFile();
#pragma warning restore CS0162 // 检测到无法访问的代码
            }
        }

        // 剪切时候删除
        if (isEraseSsget)
        {
            idArray.ForEach(id => {
                id.Erase();
            });
        }
        #endregion

        #region 写入 WMF 数据
        var wmf = Path.ChangeExtension(cadClipType.File, "wmf");
        Env.Editor.ExportWMF(wmf, idArray);

        //using var mf = new Metafile(wmf);
        //IntPtr emfHandle = mf.GetHenhmetafile();
        #endregion

        /*
         * 剪贴板说明
         * https://blog.csdn.net/chinabinlang/article/details/9815495
         *
         * 看了这ole剪贴板,感觉如果桌子真的是这样做,那么粘贴链接可能还真没法做成.
         * 1,不知道桌子如何发送wmf文件,是结构体传送,还是文件路径传送.
         * 2,不知道桌子如何接收剪贴板数据,是延迟接收还是一次性写入全局变量或者文件.
         * https://blog.csdn.net/chinabinlang/article/details/9815495
         */
        /// 必须一次性写入剪贴板,详见<see cref="ClipTool.OpenClipboardTask"/>说明
        bool getFlag = false;
        ClipTool.OpenClipboardTask((freeDatas) => {
            getFlag = ClipTool.GetClipboardFormat(
                ClipboardEnv.CadVer, cadClipType,
                out uint cadClipFormat, out IntPtr cadClipData);
            freeDatas.Add(cadClipData);

            // 写入cad图元
            ClipTool.SetClipboardData(cadClipFormat, cadClipData);

            // 写入BMP位图...这是截图,不是WMF转BMP,不对
            BitmapTool.CaptureWndImage(doc.Window.Handle, bitmapHandle => {
                ClipTool.SetClipboardData((uint)ClipboardFormat.CF_BITMAP, bitmapHandle);
            });


            // 写入WMF
            // MFC类 CMetaFileDC 不知道怎么做...
            // https://blog.csdn.net/glt3953/article/details/8808262
            // ClipTool.SetClipboardData((uint)ClipboardFormat.CF_ENHMETAFILE, wmfHandle);

            // c# WMF转换文件
            // https://blog.csdn.net/u013419838/article/details/100154891
            //using MemoryStream ms = new(File.ReadAllBytes(wmf));
            //using GZipStream gzipStream = new(ms, CompressionMode.Decompress);
            //using MemoryStream outStream = new();
            //int readCount;
            //byte[] data = new byte[2048];
            //do
            //{
            //    readCount = gzipStream.Read(data, 0, data.Length);
            //    outStream.Write(data, 0, readCount);
            //} while (readCount == data.Length);
            // outStream.GetBuffer()

            //ClipTool.GetClipboardFormat(
            //  System.Windows.Forms.DataFormats.EnhancedMetafile, emfClipType,
            //     out uint emfClipFormat, out IntPtr emfClipData);
            //freeDatas.Add(emfClipData);

            // 没效果
            // using var mf = new Metafile(wmf);
            // IntPtr emfHandle = mf.GetHenhmetafile();
            //if (emfHandle != IntPtr.Zero)
            //{
            //    ClipTool.SetClipboardData(
            //       ClipTool.RegisterClipboardFormat(System.Windows.Forms.DataFormats.EnhancedMetafile),
            //       emfHandle);
            //}

            /*
             * 文件结构
             * https://www.vuln.cn/6358#:~:text=%20%E4%B8%8B%E9%9D%A2%E7%AE%80%E8%A6%81%E4%BB%8B%E7%BB%8D%E4%B8%80%E4%B8%8BEMF%E6%96%87%E4%BB%B6%E7%9A%84%E7%BB%93%E6%9E%84%EF%BC%8CEMF%E6%96%87%E4%BB%B6%E7%94%B1%E5%8F%AF%E5%8F%98%E5%A4%A7%E5%B0%8F%E7%9A%84%E5%85%83%E6%96%87%E4%BB%B6%E5%9D%97%E7%BB%84%E6%88%90%E3%80%82,%E6%AF%8F%E4%B8%AA%E5%85%83%E6%96%87%E4%BB%B6%E5%9D%97%E9%83%BD%E6%98%AF%E4%B8%80%E4%B8%AA%E5%8F%AF%E5%8F%98%E9%95%BF%E5%BA%A6%E7%9A%84ENHMETARECORD%E7%BB%93%E6%9E%84%EF%BC%8C%E7%BB%93%E6%9E%84%E5%A6%82%E4%B8%8B%E3%80%82%20SDK%E4%B8%AD%E5%AE%9A%E4%B9%89%E4%BA%86%E4%B8%8D%E5%90%8C%E7%9A%84iType%E7%B1%BB%E5%9E%8B%EF%BC%8C%E5%A6%82%E4%B8%8B%E6%89%80%E7%A4%BA%E3%80%82%20%E6%A0%B9%E6%8D%AEiType%E7%B1%BB%E5%9E%8B%E7%9A%84%E4%B8%8D%E5%90%8C%EF%BC%8CdParm%E6%98%AF%E4%B8%8D%E5%90%8C%E7%9A%84%E7%BB%93%E6%9E%84%EF%BC%8CEMR_SETDIBITSTODEVICE%E5%AF%B9%E5%BA%94%E7%9A%84%E7%BB%93%E6%9E%84%E6%98%AFEMRSETDIBITSTODEVICE%E3%80%82
             * https://blog.csdn.net/juma/article/details/2200023 有代码抄
             * https://www.cnblogs.com/5iedu/p/4706327.html
             * https://vimsky.com/examples/detail/csharp-ex-System.Runtime.InteropServices.ComTypes-STGMEDIUM---class.html
             * https://blog.csdn.net/qq_45533841/article/details/106011204
             */
        }, true);

        // 成功拷贝就删除上一次的临时文件
        if (getFlag)
            Terminate();

        // 加入删除队列,下次删除
        if (!_delFile.Contains(cadClipType.File))
            _delFile.Add(cadClipType.File);
        if (!_delFile.Contains(wmf))
            _delFile.Add(wmf);
    }


    /// <summary>
    /// 粘贴
    /// </summary>
    /// <param name="isBlock"></param>
    void Paste(bool isBlock)
    {
        var dm = Acap.DocumentManager;
        if (dm.Count == 0)
            return;

        var getClip = ClipTool.GetClipboard(ClipboardEnv.CadVer, out TagClipboardInfo tag);
        if (!getClip)
            return;
        var clipboardInfo = tag;
        Env.Print("粘贴来源: " + clipboardInfo.File);

        if (!File.Exists(clipboardInfo.File))
        {
            Env.Print("文件不存在");
            return;
        }

        // 获取临时文件的图元id
        var fileEntityIds = new List<ObjectId>();
        using (var fileTr = new DBTrans(clipboardInfo.File,
                                        commit: false,
                                        openMode: FileOpenMode.OpenForReadAndAllShare))
        {
            foreach (var id in fileTr.ModelSpace)
                if (id.IsOk())
                    fileEntityIds.Add(id);
        }
        if (fileEntityIds.Count == 0)
            return;

        using var tr = new DBTrans();

        // 给辰的测试
        //var pr = tr.Editor?.GetPoint("aaa");
        //if (pr != null && pr.Status == PromptStatus.OK)
        //    tr.Editor?.WriteMessage("获取了点:" + pr.Value);


        tr.Editor?.SetImpliedSelection(new ObjectId[0]); // 清空选择集

        // 新建块表记录
        var btr = CreateBlockTableRecord(tr, clipboardInfo.File);
        if (btr == null)
            return;

        // 加入新建的块表记录
        // 动态块粘贴,然后用:ctrl+z会导致动态块特性无法恢复,
        /// 是因为它 <see cref=" DuplicateRecordCloning.Replace"/>
        var map = new IdMapping();
        tr.Task(() => {
            tr.Database.WblockCloneObjects(
                new ObjectIdCollection(fileEntityIds.ToArray()),
                btr.ObjectId, // tr.Database.BlockTableId, // 粘贴目标
                map,
                DuplicateRecordCloning.Ignore,
                false);
        });


        // 移动块内,从基点到原点
        foreach (var id in btr)
        {
            if (!id.IsOk())
            {
                Env.Printl("jig预览块内有克隆失败的东西,是否天正克隆期间导致?");
                continue;
            }
            var ent = tr.GetObject<Entity>(id);
            if (ent == null)
                continue;
            using (ent.ForWrite())
                ent.Move(clipboardInfo.Point, Point3d.Origin);
        }

        // 预览并获取交互点
        // 天正此处可能存在失败:天正图元不给你jig接口调用之类的
        using var moveJig = new JigEx((mousePoint, drawEntitys) => {
            var blockref = new BlockReference(Point3d.Origin, btr.ObjectId);
            blockref.Move(Point3d.Origin, mousePoint);
            drawEntitys.Enqueue(blockref);
        });
        moveJig.SetOptions(clipboardInfo.Point)
               .Keywords.Add("A", "A", "引线点粘贴(A)");

        var dr = moveJig.Drag();
        Point3d moveTo = Point3d.Origin;
        if (dr.Status == PromptStatus.Keyword)
            moveTo = clipboardInfo.Point;
        else if (dr.Status == PromptStatus.OK)
            moveTo = moveJig.MousePointWcsLast;
        else
        {
            // 删除jig预览的块表记录
            using (btr.ForWrite())
                btr.Erase();
            return;
        }

        if (isBlock)
        {
            PasteIsBlock(tr, moveJig.Entitys, moveJig.MousePointWcsLast, moveTo);
        }
        else
        {
            PasteNotBlock(tr, btr, Point3d.Origin, moveTo);
            // 删除jig预览的块表记录
            using (btr.ForWrite())
                btr.Erase();
        }
    }

    /// <summary>
    /// 粘贴为块
    /// </summary>
    /// <param name="tr"></param>
    /// <param name="entitys"></param>
    /// <param name="move"></param>
    /// <param name="moveTo"></param>
    static void PasteIsBlock(DBTrans tr, Entity[] entitys, Point3d move, Point3d moveTo)
    {
        if (!move.IsEqualTo(moveTo, new Tolerance(1e-6, 1e-6)))
        {
            entitys.ForEach(ent => {
                ent.Move(move, moveTo);
            });
        }
        tr.CurrentSpace.AddEntity(entitys);
    }

    /// <summary>
    /// 直接粘贴(不为块参照)
    /// </summary>
    /// <param name="tr"></param>
    /// <param name="btr"></param>
    /// <param name="move">它总是为<see cref="Point3d.Origin"/></param>
    /// <param name="moveTo">目标点</param>
    static void PasteNotBlock(DBTrans tr, BlockTableRecord btr, Point3d move, Point3d moveTo)
    {
        using ObjectIdCollection ids = new();
        foreach (var id in btr)
        {
            if (!id.IsOk())
                continue;
            ids.Add(id);
        }

        // 深度克隆,然后平移到当前目标点位置
        var map = tr.CurrentSpace.DeepCloneEx(ids);
        map.GetValues().ForEach(id => {
            if (!id.IsOk())
                return;
            var ent = tr.GetObject<Entity>(id);
            if (ent == null)
                return;
            using (ent.ForWrite())
                ent.Move(move, moveTo);
        });
    }

    /// <summary>
    /// 创建块表记录
    /// </summary>
    /// <param name="tr"></param>
    /// <param name="tempFile">此名称若已在块表存在,就会自动用时间名称代替</param>
    /// <returns></returns>
    BlockTableRecord? CreateBlockTableRecord(DBTrans tr, string tempFile)
    {
        var blockNameNew = Path.GetFileNameWithoutExtension(tempFile);
        while (tr.BlockTable.Has(blockNameNew))
        {
            tempFile = CreateTempFileName();
            blockNameNew = Path.GetFileNameWithoutExtension(tempFile);
            Thread.Sleep(1);
        }
        var btrIdNew = tr.BlockTable.Add(blockNameNew);
        return tr.GetObject<BlockTableRecord>(btrIdNew);
    }

    /// <summary>
    /// 创建临时路径的时间文件名
    /// </summary>
    /// <param name="format">格式,X是16进制</param>
    /// <returns></returns>
    static string CreateTempFileName(string format = "X")
    {
        var t1 = DateTime.Now.ToString("yyyyMMddHHmmssfffffff");
        t1 = Convert.ToInt32(t1.GetHashCode()).ToString(format);
        var t2 = Convert.ToInt32(t1.GetHashCode()).ToString(format);// 这里是为了满足长度而做的
        return Path.GetTempPath() + "A$" + t1 + t2[0] + ".DWG";
    }
}

//public static class BlockReferenceHelper
//{
//    /// <summary>
//    /// 遍历块内
//    /// </summary>
//    /// <param name="brf"></param>
//    /// <param name="action"></param>
//    /// <param name="tr"></param>
//    /// <exception cref="ArgumentNullException"></exception>
//    public static void ForEach(this BlockReference brf, Action<ObjectId> action, DBTrans? tr = null)
//    {
//        if (action == null)
//            throw new ArgumentNullException(nameof(action));

//        tr ??= DBTrans.Top;

//        var btr = tr.GetObject<BlockTableRecord>(brf.BlockTableRecord);
//        if (btr == null)
//            return;
//        foreach (var id in btr)
//            action.Invoke(id);
//    }
//}