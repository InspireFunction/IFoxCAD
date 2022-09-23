#define test
#define PASTECLIP
#define COPYCLIP

namespace Test;

using IFoxCAD.Cad;
using System;
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
#if COPYCLIP
        if (up == "COPYCLIP")// 复制
        {
            e.Veto();
            IFoxCopyClip();
        }
        else if (up == "COPYBASE") //ctrl+shift+c 带基点复制
        {
            e.Veto();
            IFoxCopyBase();
        }
        else if (up == "CUTCLIP") // 剪切
        {
            e.Veto();
            Copy(false, true);
        }
#endif
#if PASTECLIP
        // TODO 此处直接粘贴到命令栏也会触发,所以需要控制焦点
        if (up == "PASTECLIP")// 粘贴
        {
            e.Veto();
            IFoxPasteClip();
        }
        else if (up == "PASTEBLOCK") //ctrl+shift+v 粘贴为块
        {
            e.Veto();
            IFoxPasteBlock();
        }
#endif
    }

    /// <summary>
    /// 复制
    /// </summary>
    [CommandMethod(nameof(IFoxCopyClip), CommandFlags.UsePickSet)]
    public void IFoxCopyClip()
    {
        Copy(false);
    }
    /// <summary>
    /// 带基点复制
    /// </summary>
    [CommandMethod(nameof(IFoxCopyBase), CommandFlags.UsePickSet)]
    public void IFoxCopyBase()
    {
        Copy(true);
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


#if false
    // 想要重启cad之后还可以继续用剪贴板,那么就不要这个
    // 会出现永远存在临时文件夹的情况:
    // 0x01 复制的时候,无法删除占用中的,
    // 0x02 调试期间直接退出acad.exe
    [IFoxInitialize(isInitialize: false)]
#endif
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
            catch
            {
                Env.Printl("无法删除(是否占用):" + _delFile[i]);
            }
        }
    }


    /// <summary>
    /// 储存准备删除的文件
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

        using var tr = new DBTrans();
        if (tr.Editor == null)
            return;
        var psr = tr.Editor.SelectImplied();// 预选
        if (psr.Status != PromptStatus.OK)
            psr = tr.Editor.GetSelection();// 手选
        if (psr.Status != PromptStatus.OK)
            return;

        // 设置基点
        Point3d pt = Point3d.Origin;
        var idArray = psr.Value.GetObjectIds();
        if (getPoint)
        {
            var pr = tr.Editor.GetPoint("\n选择基点");
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

        var tempFile = CreateTempFileName();
        while (File.Exists(tempFile))
        {
            tempFile = CreateTempFileName();
            Thread.Sleep(1);
        }

        // 写入剪贴板
        // 如果成功拷贝,删除上一次的临时文件
        var clipboardInfo = new TagClipboardInfo(tempFile, pt);
        if (TagClipboardInfo.SetClipboard(clipboardInfo))
            Terminate();

        // 克隆到目标块表内
        using (var fileTr = new DBTrans(clipboardInfo.File))
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

        if (!_delFile.Contains(clipboardInfo.File))
            _delFile.Add(clipboardInfo.File);
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

        var tag = TagClipboardInfo.GetClipboard();
        if (tag == null)
            return;
        var clipboardInfo = tag.Value;
        Env.Print("粘贴来源: " + clipboardInfo.File);

        if (!File.Exists(clipboardInfo.File))
        {
            Env.Print("文件不存在");
            return;
        }

        // 获取临时文件的图元id
        var fileEntityIds = new List<ObjectId>();
        using (var fileTr = new DBTrans(clipboardInfo.File, false, FileOpenMode.OpenForReadAndAllShare))
        {
            foreach (var id in fileTr.ModelSpace)
                if (id.IsOk())
                    fileEntityIds.Add(id);
        }
        if (fileEntityIds.Count == 0)
            return;

        using var tr = new DBTrans();
#if test
        // 给辰的测试
        //var pr = tr.Editor?.GetPoint("aaa");
        //if (pr != null && pr.Status == PromptStatus.OK)
        //{
        //    tr.Editor?.WriteMessage("获取了点:" + pr.Value);
        //}
#endif
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