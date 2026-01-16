#define test
#define COPYCLIP
#define PASTECLIP
#if false
namespace Test;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows;

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
    static bool _IsRunIFoxCopyClip = false;
    [IFoxInitialize] // 惊惊: 遇到了高版本无法导出WMF,放弃此功能,等待有缘人
    public void Init()
    {
        Acap.DocumentManager.DocumentLockModeChanged += Dm_VetoCommand;
        Env.Printl($"※剪贴板控制※\n{nameof(Copyclip_Switch)} - 切换开关\n");
    }

    [CommandMethod(nameof(Copyclip_Switch))]
    public void Copyclip_Switch()
    {
        _IsRunIFoxCopyClip = !_IsRunIFoxCopyClip;
        Env.Printl("已经 " + (_IsRunIFoxCopyClip ? "开启" : "禁用") + " 剪贴板+");
    }

    /// <summary>
    /// 反应器->命令否决触发命令前(不可锁文档)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Dm_VetoCommand(object sender, DocumentLockModeChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.GlobalCommandName) || e.GlobalCommandName == "#")
            return;
        if (!_IsRunIFoxCopyClip)
            return;

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
            // === 完成之后此处将会移除
            // 粘贴文本的生成单行文字/多行文字,这些还需要自己去实现
            var getClip = ClipTool.GetClipboard(ClipboardEnv.CadVer, out TagClipboardInfo tag);
            if (!getClip)
                return;
            //=== 完成之后此处将会移除

            e.Veto();
            cmd = nameof(IFoxPasteClip);
        }
        else if (up == "PASTEBLOCK") //ctrl+shift+v 粘贴为块
        {
            // === 完成之后此处将会移除
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
            // 发送命令是因为com导出WMF需要命令形式,否则将报错
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
    /// 读写锁,当资源处于写入模式时,<br/>
    /// 其他线程写入需要等待本次写入结束之后才能继续写入
    /// <a href=" https://www.cnblogs.com/Tench/p/CSharpSimpleFileWriteLock.html ">参考链接</a>
    /// </summary>
    static readonly ReaderWriterLockSlim _rwLock = new();

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
        try
        {
            if (!_rwLock.IsWriteLockHeld)
                _rwLock.EnterWriteLock(); // 进入写入锁

            var dm = Acap.DocumentManager;
            if (dm.Count == 0)
                return;
            var doc = dm.MdiActiveDocument;
            if (doc == null)
                return;

            ObjectId[] idArray;
#if true
            var psr = doc.Editor.SelectImplied();// 预选
            if (psr.Status != PromptStatus.OK)
                psr = doc.Editor.GetSelection();// 手选
            if (psr.Status != PromptStatus.OK)
                return;
            idArray = psr.Value.GetObjectIds();
#endif

            // 设置基点
            Point3d pt = Point3d.Origin;
            var tempFile = CreateTempFileName();
            while (File.Exists(tempFile) ||
                   File.Exists(Path.ChangeExtension(tempFile, "wmf")))
            {
                tempFile = CreateTempFileName();
                Thread.Sleep(1);
            }

            using DBTrans tr = new();


            #region 写入 WMF 数据
            /*
             * 通过cadCom导出wmf(免得自己转换每种图元),
             * 再将wmf转为emf,然后才能写入剪贴板
             * wmf生成(win32api): https://www.cnblogs.com/5iedu/p/4706324.html
             */
            IntPtr hMetaFile = IntPtr.Zero;
            {
                var wmf = Path.ChangeExtension(tempFile, "wmf");
                Env.Editor.ComExportWMF(wmf, idArray);
                if (File.Exists(wmf))
                {
                    hMetaFile = PlaceableMetaHeader.Wmf2Emf(wmf);//emf文件句柄
                    try { File.Delete(wmf); }
                    catch (Exception e) { Env.Printl(e); }
                }
                else
                {
                    Env.Printl("没有创建wmf,失败");
                }
            }
            if (hMetaFile != IntPtr.Zero)
            {
                // 克隆一个,并写入描述...此处尚未完成
                // EmfTool.SetEnhMetaFileDescriptionEx(ref hMetaFile, "这是阿惊的emf");
            }

            // 保存文件
            //var emfsave = Path.ChangeExtension(cadClipType.File, ".emf");
            //EmfTool.Save(emf, emfsave);
            #endregion

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
                    if (info != null)
                    {
                        if (ent is BlockReference brf)
                            info.Value.Move(brf.Position, Point3d.Origin);
                        minx = minx > info.Value.MinX ? info.Value.MinX : minx;
                        miny = miny > info.Value.MinY ? info.Value.MinY : miny;
                        minz = minz > info.Value.MinZ ? info.Value.MinZ : minz;
                    }
                }
                pt = new(minx, miny, minz);
            }

            var cadClipType = new TagClipboardInfo(tempFile, pt);

            // 克隆到目标块表内
            using (DBTrans fileTr = new(cadClipType.File))
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
                    fileTr.Database.SaveFile((DwgVersion)27, false);
                else
                    throw new ArgumentException($"版本过低,无法保存,版本号:{DwgVersion.Current}");
            }
            #endregion

            // 必须一次性写入剪贴板,详见 OpenClipboardTask
            var cadClipFormat = ClipTool.RegisterClipboardFormat(ClipboardEnv.CadVer);

            // 剪贴板需要的指针: 克隆一个新的,不释放内存,不锁定内存(否则高频触发时候卡死)
            var cadClipData = cadClipType.CloneToPtr();

            bool getFlag = ClipTool.OpenClipboardTask(true, () => {
                // 写入剪贴板: cad图元
                ClipTool.SetClipboardData(cadClipFormat, cadClipData);

                // 写入剪贴板: wmf,使得在粘贴链接的时候可以用
                if (hMetaFile != IntPtr.Zero)
                    ClipTool.SetClipboardData((uint)ClipboardFormat.CF_ENHMETAFILE, hMetaFile);
            });
            if (hMetaFile != IntPtr.Zero)
                EmfTool.DeleteEnhMetaFile(hMetaFile);

            // 成功拷贝就删除上一次的临时文件
            if (getFlag)
                Terminate();

            // 加入删除队列,下次删除
            if (!_delFile.Contains(cadClipType.File))
                _delFile.Add(cadClipType.File);

            // 剪切时候删除
            if (isEraseSsget)
            {
                idArray.ForEach(id => {
                    id.Erase();
                });
            }
        }
        catch (Exception e)
        {
            Debugger.Break();
            throw e;
        }
        finally
        {
            if (_rwLock.IsWriteLockHeld)
                _rwLock.ExitWriteLock(); // 退出写入锁
        }
    }


    /// <summary>
    /// 粘贴
    /// </summary>
    /// <param name="isBlock"></param>
    void Paste(bool isBlock)
    {
        try
        {
            if (!_rwLock.IsWriteLockHeld)
                _rwLock.EnterWriteLock(); // 进入写入锁

            var dm = Acap.DocumentManager;
            if (dm.Count == 0)
                return;

            var getClip = ClipTool.GetClipboard(ClipboardEnv.CadVer, out TagClipboardInfo tag);
            if (!getClip)
            {
                // 在没有安装插件的高版本cad中复制,此时剪贴板是当前版本的,
                // 那么在安装了插件的cad中需要识别这个同版本的剪贴板内容
                // 例如天正只在某个启动的cad中加载插件,而不是全部
                getClip = ClipTool.GetClipboard(ClipboardEnv.CadCurrentVer, out tag);
                if (!getClip)
                    return;
            }

            var cadClipType = tag;
            Env.Print("粘贴来源: " + cadClipType.File);

            if (!File.Exists(cadClipType.File))
            {
                Env.Print("文件不存在");
                return;
            }

            // 获取临时文件的图元id
            List<ObjectId> fileEntityIds = [];
            using (DBTrans fileTr = new(cadClipType.File, commit: false,
                                        fileOpenMode: FileOpenMode.OpenForReadAndAllShare))
            {
                fileTr.ModelSpace.ForEach(id => {
                    if (id.IsOk())
                        fileEntityIds.Add(id);
                });
            }
            if (fileEntityIds.Count == 0)
                return;

            using DBTrans tr = new();
            tr.Editor?.SetImpliedSelection(new ObjectId[0]); // 清空选择集

            // 新建块表记录
            var btr = CreateBlockTableRecord(tr, cadClipType.File);
            if (btr == null)
                return;

            /// 克隆进块表记录
            /// 动态块粘贴之后,用ctrl+z导致动态块特性无法恢复,
            /// 是因为它: <see cref="DuplicateRecordCloning.Replace"/>
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
                    Env.Printl("jig预览块内有克隆失败的脏东西,是否天正克隆期间导致?");
                    continue;
                }
                var ent = tr.GetObject<Entity>(id);
                if (ent == null)
                    continue;
                using (ent.ForWrite())
                    ent.Move(cadClipType.Point, Point3d.Origin);
            }

            // 预览并获取交互点
            // 天正此处可能存在失败:天正图元不给你jig接口调用之类的
            using var moveJig = new JigEx((mousePoint, drawEntitys) => {
                var brf = new BlockReference(Point3d.Origin, btr.ObjectId);
                brf.Move(Point3d.Origin, mousePoint);
                drawEntitys.Enqueue(brf);
            });
            var jppo = moveJig.SetOptions(cadClipType.Point);
            jppo.Keywords.Add(" ", " ", "<空格取消>");
            jppo.Keywords.Add("A", "A", "引线点粘贴(A)");

            var dr = moveJig.Drag();
            Point3d moveTo = Point3d.Origin;
            if (dr.Status == PromptStatus.Keyword)
                moveTo = cadClipType.Point;
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

            try
            {
                #region 读取剪贴板WMF
                var msg = new StringBuilder();

                int a3 = 0;
                a3 = 2 | 4;
                if ((a3 & 1) == 1)
                {
                    // win32api 不成功
                    ClipTool.OpenClipboardTask(false, () => {
                        // 剪贴板数据保存目标数据列表
                        List<byte[]> _bytes = new();
                        var cf = (uint)ClipboardFormat.CF_ENHMETAFILE;
                        var clipTypeData = ClipTool.GetClipboardData(cf);
                        if (clipTypeData == IntPtr.Zero)
                        {
                            Env.Printl("失败:粘贴剪贴板emf1");
                            return;
                        }

                        // 无法锁定剪贴板emf内存,也无法获取GlobalSize
                        bool locked = WindowsAPI.GlobalLockTask(clipTypeData, prt => {
                            uint size = WindowsAPI.GlobalSize(prt);
                            if (size > 0)
                            {
                                var buffer = new byte[size];
                                Marshal.Copy(prt, buffer, 0, buffer.Length);
                                _bytes.Add(buffer);
                            }
                        });
                        if (!locked)
                            Env.Printl("锁定内存失败");
                    });
                }
                if ((a3 & 2) == 2)
                {
                    ClipTool.OpenClipboardTask(false, () => {
                        // 无法锁定剪贴板emf内存,也无法获取GlobalSize
                        // 需要直接通过指针跳转到指定emf结构位置
                        var cf = (uint)ClipboardFormat.CF_ENHMETAFILE;
                        var clipTypeData = ClipTool.GetClipboardData(cf);
                        if (clipTypeData == IntPtr.Zero)
                        {
                            Env.Printl("失败:粘贴剪贴板emf2");
                            return;
                        }

                        int a4 = 1;
                        //int a4 = 1 | 2 | 4;
                        if ((a4 & 1) == 1)
                        {
                            // 获取描述
                            var desc = EmfTool.GetEnhMetaFileDescriptionEx(clipTypeData);
                            if (!string.IsNullOrEmpty(desc))
                                msg.AppendLine("DescriptionEx::" + desc);
                        }
                        if ((a4 & 2) == 2)
                        {
                            // 获取文件信息
                            var obj = EnhMetaHeader.Create(clipTypeData);
                            msg.AppendLine("EnhMetaHeader::" + obj.ToString());
                        }
                        if ((a4 & 4) == 4)
                        {
                            // 保存文件
                            //var emfsave = Path.ChangeExtension(cadClipType.File, ".emf");
                            //EmfTool.Save(clipTypeData, emfsave);
                        }
                    });
                }
                if ((a3 & 4) == 4)
                {
                    // c# 读取成功,win32直接读取剪贴板的话是不成功的
                    if (Clipboard.ContainsData(DataFormats.EnhancedMetafile))
                    {
                        var iData = Clipboard.GetDataObject();//从剪切板获取数据
                        if (!iData.GetDataPresent(DataFormats.EnhancedMetafile))
                            return;
                        var metafile = (Metafile)iData.GetData(DataFormats.EnhancedMetafile);
                        /*
                            // 为什么序列化失败了呢
                            var formatter = new BinaryFormatter();
                            //using MemoryStream stream = new();
                            Stream stream = new FileStream("MyFile.bin", FileMode.Create, FileAccess.Write, FileShare.None);
                            formatter.Serialize(stream, metafile);
                            stream.Close();
                            //Metafile obj = (Metafile)formatter.Deserialize(stream);
                        */
                        msg.AppendLine($"c# Metafile::{metafile.Size}");
                    }
                }

                if (msg.Length != 0)
                    Env.Printl(msg);
                #endregion
            }
            catch (Exception e)
            {
                Debugger.Break();
                DebugEx.Printl(e);
            }
        }
        catch (Exception e)//{"剪贴板上的数据无效 (异常来自 HRESULT:0x800401D3 (CLIPBRD_E_BAD_DATA))"}
        {
            Debugger.Break();
            DebugEx.Printl(e);
        }
        finally
        {
            if (_rwLock.IsWriteLockHeld)
                _rwLock.ExitWriteLock(); // 退出写入锁
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

    // 此处相当于截图,后台没有doc不可用
    // https://www.cnblogs.com/shangdishijiao/p/15166499.html
    [CommandMethod(nameof(CreatePreviewImage))]
    public void CreatePreviewImage()
    {
        using DBTrans tr = new();
        if (tr.Document == null)
            return;

        var doc = tr.Document;

        var size = doc.Window.DeviceIndependentSize;
        using var bmp = doc.CapturePreviewImage(
            Convert.ToUInt32(size.Width),
            Convert.ToUInt32(size.Height));

        //保存wmf会变png,看二进制签名
        var outFile = Path.ChangeExtension(tr.Database.Filename, ".bmp");
        bmp.Save(outFile, GetFormat(outFile));
        Env.Printl($"保存文件:{outFile}");
        Env.Printl($"保存后缀:{GetFormat(outFile)}");

        // 利用winAPI截图
        bool getFlag = ClipTool.OpenClipboardTask(true, () => {
            BitmapTool.CaptureWndImage(doc.Window.Handle, bitmapHandle => {
                // 写入剪贴板: BMP位图,这是截图,不是WMF转BMP,不对
                ClipTool.SetClipboardData((uint)ClipboardFormat.CF_BITMAP, bitmapHandle);
            });
        });
    }
}


public class OleTestClass
{
    // https://adndevblog.typepad.com/autocad/2012/04/update-linked-ole-object-from-net.html

    /// <summary>
    /// 更新ole链接
    /// 如果 OLE 对象在另一个应用程序中打开,则上面的代码似乎不会更新该对象
    /// 例如,位图在 Microsoft 画图中打开
    /// </summary>
    /// <param name="thisClientItem"></param>
    /// <returns></returns>
    [DllImport("mfc90u.dll", CallingConvention = CallingConvention.ThisCall,
        EntryPoint = "#6766")]
    public static extern int COleClientItem_UpdateLink(IntPtr thisClientItem);

    [DllImport("acdb18.dll", CallingConvention = CallingConvention.ThisCall,
        EntryPoint = "?getOleClientItem@AcDbOle2Frame@@QBEPAVCOleClientItem@@XZ")]
    public static extern IntPtr AcDbOle2Frame_getOleClientItem(IntPtr thisOle2Frame);

    [CommandMethod(nameof(UpdateOleClient))]
    public void UpdateOleClient()
    {
        var per = Env.Editor.GetEntity("\n选择OLE更新链接");
        if (per.Status != PromptStatus.OK)
            return;

        using DBTrans tr = new();
        var ole2frame = tr.GetObject<Ole2Frame>(per.ObjectId);
        if (ole2frame == null)
            return;
        using (ole2frame.ForWrite())
        {
            IntPtr ptrClientItem = AcDbOle2Frame_getOleClientItem(ole2frame.UnmanagedObject);
            COleClientItem_UpdateLink(ptrClientItem);
        }
    }


    // https://adndevblog.typepad.com/autocad/2012/06/iterating-ole-linked-entities.html
    // 获取OLE路径
    [CommandMethod(nameof(GetOlePath))]
    public void GetOlePath()
    {
        using DBTrans tr = new();
        foreach (ObjectId id in tr.CurrentSpace)
        {
            if (!id.IsOk())
                continue;

            var ole2frame = tr.GetObject<Ole2Frame>(id);
            if (ole2frame == null)
                continue;
            switch (ole2frame.Type)
            {
                case Ole2Frame.ItemType.Static:
                Env.Editor.WriteMessage("\n" + "Static");
                break;
                case Ole2Frame.ItemType.Embedded:
                Env.Editor.WriteMessage("\n" + "Embedded");
                break;
                case Ole2Frame.ItemType.Link:
                Env.Editor.WriteMessage("\n" + "Link");
                Env.Editor.WriteMessage("\n" + ole2frame.LinkPath);
                break;
            }
        }
    }

    [CommandMethod(nameof(SetOle))]
    public void SetOle()
    {
        //var pr = Env.Editor.GetPoint("\n选择基点");
        //if (pr.Status != PromptStatus.OK)
        //    return;
        //var pt = pr.Value;

        using DBTrans tr = new();

        // https://forums.autodesk.com/t5/net/how-to-create-an-ole-object-with-ole2frame/td-p/3203222
        var oo = new Ole2Frame();
        oo.SetDatabaseDefaults();
        //oo.CopyFrom()
        oo.Position2d = new(0, 0, 100, 100);

        // 打印质量? https://learn.microsoft.com/zh-cn/dotnet/api/system.printing.outputquality?view=windowsdesktop-6.0
        //oo.OutputQuality =
        //oo.AutoOutputQuality =
        oo.Rotation = 0.0;
        //oo.WcsWidth = ;
        //oo.WcsHeight = ;
        //oo.ScaleWidth = ;
        //oo.ScaleHeight = ;

        //宽高比锁定
        oo.LockAspect = true;
    }

#if true2
    // https://forums.autodesk.com/t5/net/how-can-i-read-data-from-ole2frame-object-excel-through-api/td-p/7944241
    public void GetOleForOffice()
    {
        using DBTrans tr = new();
        foreach (ObjectId id in tr.CurrentSpace)
        {
            if (!id.IsOk())
                continue;

            var ole2frame = tr.GetObject<Ole2Frame>(id);
            if (ole2frame == null)
                continue;

            var wb = (Microsoft.Office.Interop.Excel.Workbook)ole2frame.OleObject;
            var ws = (Microsoft.Office.Interop.Excel.Worksheet)wb.ActiveSheet;
            var range = (Microsoft.Office.Interop.Excel.Range)ws.UsedRange;

            tb.SetSize(range.Rows.Count, range.Columns.Count);
            tb.SetRowHeight(range.RowHeight);
            tb.SetColumnWidth(range.ColumnWidth);
            tb.Position = new Point3d(ole2frame.Location.X, ole2frame.Location.Y, ole2frame.Location.Z);
            for (int row = 0; row < range.Rows.Count; row++)
            {
                for (int col = 0; col < range.Columns.Count; col++)
                {
                    tb.Cells[row, col].TextHeight = 1;
                    var aa = (Microsoft.Office.Interop.Excel.Range)range.Cells[row + 1, col + 1];
                    var bb = Convert.ToString(aa.Value2);
                    tb.SetTextString(row, col, bb ?? "");
                    tb.Cells[row, col].Alignment = CellAlignment.MiddleCenter;
                }
            }
            tb.GenerateLayout();
        }
    }
#endif
}

#endif