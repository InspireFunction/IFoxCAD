#define test
#define COPYCLIP
#define PASTECLIP

namespace Test;
using Autodesk.AutoCAD.DatabaseServices;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows;
using static HarmonyLib.Code;

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

    // 想要重启cad之后还可以继续用剪贴板,那么就不要这个:
    // [IFoxInitialize(isInitialize: false)]
    // 会出现永远存在临时文件夹的情况:
    // 0x01 复制的时候,无法删除占用中的,
    // 0x02 调试期间直接退出 acad.exe
    public void Terminate()
    {
        // 此处要先去删除tmp文件夹的上次剪贴板产生的dwg文件
        for (int i = _delFile.Count - 1; i >= 0; i--)
        {
            try
            {
                if (File.Exists(_delFile[i]))
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
                using IdMapping map = new();
                using ObjectIdCollection ids = new(idArray);
                tr.Database.WblockCloneObjects(
                    ids,
                    fileTr.ModelSpace.ObjectId,
                    map,
                    DuplicateRecordCloning.Replace,
                    false);
            });

            // 大于dwg07格式的,保存为07,以实现高低版本通用剪贴板
            // 小于dwg07格式的,本工程没有支持cad06dll
            if ((int)DwgVersion.Current >= 27)
                fileTr.SaveFile((DwgVersion)27, false);
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

        //wmf指针
        IntPtr wmfMeta = IntPtr.Zero;

        int a1 = 1;
        if (a1 == 1)
        {
            Env.Editor.ExportWMF(wmf, idArray);

            var hWnd = doc.Window.Handle;
            WindowsAPI.GetClientRect(hWnd, out IntRect rcClient);
            int width = rcClient.Right - rcClient.Left;
            int height = rcClient.Bottom - rcClient.Top;
            wmfMeta = EmfTool.CadGetMetafile(wmf, hWnd, rcClient);
        }
        if (a1 == 2)
        {
            // 这是c#写入wmf流程
            // c#画的wmf格式是可以的...用这样方式生成的就是可以写剪贴板
            WindowsAPI.GetClientRect(doc.Window.Handle, out IntRect rcClient);
            int width = rcClient.Right - rcClient.Left;
            int height = rcClient.Bottom - rcClient.Top;
            EmfTool.Export(wmf, width, height);//cad的命令wmfin:不能导入c#自绘的

            //c#方法,但是它读取不了cad的wmf
            wmfMeta = EmfTool.GetMetafile(wmf);
        }
        #endregion

        /// 必须一次性写入剪贴板,详见 OpenClipboardTask
        bool getFlag = false;
        ClipTool.OpenClipboardTask(free => {
            getFlag = ClipTool.GetClipboardFormat(
                ClipboardEnv.CadVer, cadClipType,
                out uint cadClipFormat, out IntPtr cadClipData);

            // 写入cad图元
            ClipTool.SetClipboardData(cadClipFormat, cadClipData);
            free.Add(cadClipData);

            // c# cad截图 https://www.cnblogs.com/shangdishijiao/p/15166499.html
            // 写入BMP位图...这是截图,不是WMF转BMP,不对
            // BitmapTool.CaptureWndImage(doc.Window.Handle, bitmapHandle => {
            //     ClipTool.SetClipboardData((uint)ClipboardFormat.CF_BITMAP, bitmapHandle);
            // });

            if (wmfMeta != IntPtr.Zero)
            {
                ClipTool.SetClipboardData((uint)ClipboardFormat.CF_ENHMETAFILE, wmfMeta);
                EmfTool.DeleteEnhMetaFile(wmfMeta);
            }
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
        tr.Editor?.SetImpliedSelection(new ObjectId[0]); // 清空选择集

        // 新建块表记录
        var btr = CreateBlockTableRecord(tr, clipboardInfo.File);
        if (btr == null)
            return;

        // 加入新建的块表记录
        // 动态块粘贴,然后用:ctrl+z会导致动态块特性无法恢复,
        /// 是因为它 <see cref=" DuplicateRecordCloning.Replace"/>
        using IdMapping map = new();
        using ObjectIdCollection idc = new(fileEntityIds.ToArray());
        tr.Task(() => {
            tr.Database.WblockCloneObjects(
                idc,
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

        #region 读取剪贴板WMF
#if true2
        // win32api 不成功
        ClipTool.OpenClipboardTask((freeDatas) => {
            // 剪贴板数据保存目标数据列表
            List<byte[]> _bytes = new();
            var hMem = ClipTool.GetClipboardData((uint)ClipboardFormat.CF_ENHMETAFILE);
            WindowsAPI.GlobalLockTask(hMem, prt => {
                uint size = WindowsAPI.GlobalSize(hMem);
                if (size > 0)
                {
                    var buffer = new byte[size];
                    Marshal.Copy(prt, buffer, 0, buffer.Length);// 将剪贴板数据保存到自定义字节数组
                    _bytes.Add(buffer);
                }
            });

            //{
            //    var size = Marshal.SizeOf(typeof(PlaceableMetaHeader));
            //    if (size > 0)
            //    {
            //        var buffer = new byte[size];
            //        Marshal.Copy(hMem, buffer, 0, buffer.Length);// 将剪贴板数据保存到自定义字节数组
            //        _bytes.Add(buffer);
            //    }
            //}
        }, false);
#else
        try
        {
            // c# 读取成功
            if (Clipboard.ContainsData(DataFormats.EnhancedMetafile))
            {
                var iData = Clipboard.GetDataObject();//从剪切板获取数据
                if (!iData.GetDataPresent(DataFormats.EnhancedMetafile))
                    return;

                //定义图形-图元文件
                var metafile = (Metafile)iData.GetData(DataFormats.EnhancedMetafile);
                Env.Printl("Metafile:" + metafile.Size.ToString());
            }
        }
        catch (Exception e)
        {
            throw;
        }
#endif
        #endregion
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
        using IdMapping map = new();
        tr.CurrentSpace.DeepCloneEx(ids, map);

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


#if true2

public class TestImageFormat
{
    public ImageFormat GetFormat(string filename)
    {
        string ext = Path.GetExtension(filename).ToLower();
        var imf = ext switch
        {
            ".bmp" => ImageFormat.Bmp,
            ".gif" => ImageFormat.Gif,
            ".jpg" => ImageFormat.Jpeg,
            ".tif" => ImageFormat.Tiff,
            ".wmf" => ImageFormat.Wmf,
            ".png" => ImageFormat.Png,
            _ => throw new NotImplementedException(),
        };
        return imf;
    }

    [CommandMethod("CPI")]
    public void CreatePreviewImage()
    {
        // Get the size of the document and capture the preview at that size
        var size = Document.Window.DeviceIndependentSize;
        using (var bmp = Document.CapturePreviewImage(Convert.ToUInt32(size.Width), Convert.ToUInt32(size.Height)))
        {
            // Save the file with the format derived from the filename
            bmp.Save(outFile, GetFormat(outFile));
        }
    }
}
#endif