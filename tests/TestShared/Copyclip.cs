#define test

namespace Test;

using IFoxCAD.Cad;
using System;
using System.Linq;
using System.Threading;
using Clipboard = System.Windows.Forms.Clipboard;

// https://forums.autodesk.com/t5/net/paste-list-of-objects-from-clipboard-on-dwg-file-using-c-net/td-p/6797606
public class InterceptCopyclip
{
#if test
    [IFoxInitialize]
#endif
    public void Init()
    {
        Acap.DocumentManager.DocumentLockModeChanged += DocumentManager_DocumentLockModeChanged;
    }

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
            catch // 占用的时候无法删除
            { }
        }
    }


    /// <summary>
    /// 反应器->命令否决触发命令前(不可锁文档)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void DocumentManager_DocumentLockModeChanged(object sender, DocumentLockModeChangedEventArgs e)
    {
        var up = e.GlobalCommandName.ToUpper();

        if (up == "PASTEBLOCK" || up == "PASTECLIP")
        {
            // 桌子是如何做到 粘贴为块的时候读取到基点的? 而直接粘贴则是左下角
            // 是通过粘贴的字符串结构(乱码)提供的
            var ok = GetCadPasteclip("AutoCAD");
            if (!ok)
                return;

            // 如果剪贴板存在这个路径,才去否决,粘贴文字之类的由原本的命令执行
            Env.Print("粘贴来源: " + _fileName);
        }


        if (up == "COPYCLIP")// 复制
        {
            e.Veto();
            IFoxCopyclip();
        }
        else if (up == "COPYBASE") //ctrl+shift+c 带基点复制
        {
            e.Veto();
            IFoxCopyBase();
        }
        else if (up == "PASTECLIP")// 粘贴
        {
            e.Veto();
            IFoxPasteclip();
        }
        else if (up == "PASTEBLOCK") //ctrl+shift+v 粘贴为块
        {
            e.Veto();
            IFoxPasteBlock();
        }
        else if (up == "CUTCLIP") // 剪切
        {
            e.Veto();
            List<ObjectId> ids = new();
            Copy(false, ids);

            using var tr = new DBTrans();
            ids.ForEach(id => {
                id.Erase();
            });
        }
    }

    /// <summary>
    /// 复制命令
    /// </summary>
    [CommandMethod(nameof(IFoxCopyclip), CommandFlags.UsePickSet)]
    public void IFoxCopyclip()
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
    /// 粘贴命令
    /// </summary>
    [CommandMethod(nameof(IFoxPasteclip))]
    public void IFoxPasteclip()
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





    // 有了这个的话,还需要读取剪贴板吗?
    // 需要的,当前剪贴板中,有可能不是dwg路径,而是文字内容等
    string? _fileName;
    // 基点
    Point3d? _basePoint;
    // 基点字符串
    const string _BasePointStr = "BasePoint:";
    // 删除的文件,如果删除出错(占用),将一直在这个集合中,直到cad关闭
    readonly List<string> _delFile = new();

    /// <summary>
    /// 获取剪贴板路径
    /// </summary>
    /// <param name="pasteclipStr">控制不同的cad</param>
    /// <returns>获取是否成功</returns>
    bool GetCadPasteclip(string pasteclipStr)
    {
        // 获取剪贴板上面的保存路径
        var format = Clipboard.GetDataObject()
                              .GetFormats()
                              .FirstOrDefault(f => f.StartsWith(pasteclipStr));

        if (string.IsNullOrEmpty(format))
            return false;

        var data = (MemoryStream)Clipboard.GetData(format);
        using (var reader = new StreamReader(data))
        {
            var str = reader.ReadToEnd();
            str = str.Replace("\0", string.Empty);
            _fileName = str.Substring(0, str.IndexOf(".DWG") + 4);

            var bpstr = str.IndexOf(_BasePointStr);
            if (bpstr > -1)
            {
                int start = bpstr + _BasePointStr.Length + 1;//1是(括号
                int end = str.IndexOf(")") - start;
                var ptstr = str.Substring(start, end);
                var ptArr = ptstr.Split(',');
                if (ptArr != null)
                {
                    _basePoint = new Point3d(
                        double.Parse(ptArr[0]),
                        double.Parse(ptArr[1]),
                        double.Parse(ptArr[2]));
                }
            }
        }
        if (string.IsNullOrEmpty(Path.GetFileName(_fileName)))
            return false;
        return true;
    }

    /// <summary>
    /// 复制
    /// </summary>
    /// <param name="getPoint"></param>
    public void Copy(bool getPoint, List<ObjectId>? objectIds = null)
    {
        Terminate();

        var dm = Acap.DocumentManager;
        if (dm.Count == 0)
            return;

        var doc = dm.MdiActiveDocument;
        var sdb = doc.Database;
        var ed = doc.Editor;

        objectIds ??= new();
        var psr = ed.SelectImplied();// 预选
        if (psr.Status != PromptStatus.OK)
        {
            psr = ed.GetSelection();// 手选
            if (psr.Status != PromptStatus.OK)
                return;
        }
        objectIds.AddRange(psr.Value.GetObjectIds());

        Point3d basePoint = Point3d.Origin;
        if (getPoint)
        {
            var pr = ed.GetPoint("\n选择基点");
            if (pr.Status != PromptStatus.OK)
                return;
            basePoint = pr.Value;
        }
        else
        {
            // 遍历块内
            // 获取左下角点作为基点
            double minx = double.MaxValue;
            double miny = double.MaxValue;
            double minz = double.MaxValue;
            using var tr = new DBTrans();
            foreach (var id in objectIds)
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
            basePoint = new Point3d(minx, miny, minz);
        }

        // 获取临时路径
        do
        {
            _fileName = TimeName();
            Thread.Sleep(1);
        } while (File.Exists(_fileName));

        // 写入剪贴板,这里arx有个结构体
        var sb = new StringBuilder();
        sb.Append(_fileName);
        sb.Append(_BasePointStr + basePoint.ToString());

        var data = new MemoryStream();
        var bytes = Encoding.Unicode.GetBytes(sb.ToString());
        data.Write(bytes, 0, bytes.Length);
        Clipboard.SetData($"AutoCAD.r{Acap.Version.Major}", data);

        // 克隆到目标块表内
        using var fileTr = new DBTrans(_fileName);
        fileTr.Task(() => {
            var map = new IdMapping();
            sdb.WblockCloneObjects(
                new ObjectIdCollection(objectIds.ToArray()),
                fileTr.ModelSpace.ObjectId,
                map,
                DuplicateRecordCloning.Replace,
                false);
        });

        // 大于dwg07格式,保存为07,以实现高低版本通用剪贴板
        if ((int)DwgVersion.Current >= 27)
        {
            fileTr.SaveFile((DwgVersion)27, false);
        }
        else
        {
#pragma warning disable CS0162 // 检测到无法访问的代码
            fileTr.SaveFile(); // 低于dwg07格式的,本工程没有支持cad06dll,所以这里只是展示
#pragma warning restore CS0162 // 检测到无法访问的代码
        }

        if (!_delFile.Contains(_fileName))
            _delFile.Add(_fileName);
    }



    /// <summary>
    /// 获取时间名
    /// </summary>
    /// <returns></returns>
    string TimeName()
    {
        var t1 = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        t1 = Convert.ToInt32(t1.GetHashCode()).ToString("X");
        var t2 = Convert.ToInt32(t1.GetHashCode()).ToString("X");// 这里是为了满足长度而做的
        return Path.GetTempPath() + "A$" + t1 + t2[0] + ".DWG";
    }

    /// <summary>
    /// 粘贴
    /// </summary>
    /// <param name="isBlock"></param>
    public void Paste(bool isBlock)
    {
        var dm = Acap.DocumentManager;
        if (dm.Count == 0)
            return;

        if (_fileName == null)
            return;

        // 获取临时文件的图元id
        var fileEntityIds = new List<ObjectId>();
        using (var fileTr = new DBTrans(_fileName, false, FileOpenMode.OpenForReadAndAllShare))
        {
            foreach (var id in fileTr.ModelSpace)
                fileEntityIds.Add(id);
        }

        using var tr = new DBTrans();
        tr.Editor?.SetImpliedSelection(new ObjectId[0]); // 清空选择集

        // 获取块名
        var blockNameNew = Path.GetFileNameWithoutExtension(_fileName);
        while (tr.BlockTable.Has(blockNameNew))
        {
            blockNameNew = Path.GetFileNameWithoutExtension(TimeName());
            Thread.Sleep(1);
        }
        // 新建块表记录
        var btrIdNew = tr.BlockTable.Add(blockNameNew);

        // 加入新建的块表记录
        // 动态块粘贴,然后用:ctrl+z会导致动态块特性无法恢复,
        /// 是因为它 <see cref=" DuplicateRecordCloning.Replace"/>
        var map = new IdMapping();
        tr.Task(() => {
            tr.Database.WblockCloneObjects(
                new ObjectIdCollection(fileEntityIds.ToArray()),
                btrIdNew, // tr.Database.BlockTableId, // 粘贴目标
                map,
                DuplicateRecordCloning.Ignore,
                false);
        });

        Point3d basePoint = _basePoint!.Value;

        // 移动块内的带基点
        var btr = tr.GetObject<BlockTableRecord>(btrIdNew);
        if (btr == null)
            return;
        foreach (var id in btr)
        {
            var ent = tr.GetObject<Entity>(id);
            if (ent == null)
                return;
            using (ent.ForWrite())
                ent.Move(basePoint, Point3d.Origin);
        }

        // 预览并获取交互点
        using var moveJig = new JigEx((mousePoint, drawEntitys) => {
            var blockref = new BlockReference(Point3d.Origin, btrIdNew);
            blockref.Move(Point3d.Origin, mousePoint);
            drawEntitys.Enqueue(blockref);
        });

        moveJig.SetOptions(basePoint);
        var dr = moveJig.Drag();
        if (dr.Status == PromptStatus.None) // 空格为原点拷贝?
            return;
        if (dr.Status != PromptStatus.OK)
            return;

        if (isBlock)
        {
            // 粘贴为块,创建图元
            tr.CurrentSpace.AddEntity(moveJig.Entitys);
        }
        else
        {
            // 直接粘贴
            using ObjectIdCollection ids = new();
            var brf = (BlockReference)moveJig.Entitys[0];
            brf.ForEach(id => ids.Add(id));

            map = tr.CurrentSpace.DeepCloneEx(ids);
            map.GetValues().ForEach(id => {
                if (!id.IsOk())
                    return;
                var ent = tr.GetObject<Entity>(id);
                if (ent == null)
                    return;
                using (ent.ForWrite())
                    ent.Move(Point3d.Origin, moveJig.MousePointWcsLast);
            });
        }
    }
}

public static class BlockReferenceHelper
{
    /// <summary>
    /// 遍历块内
    /// </summary>
    /// <param name="brf"></param>
    /// <param name="action"></param>
    /// <param name="tr"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ForEach(this BlockReference brf, Action<ObjectId> action, DBTrans? tr = null)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        tr ??= DBTrans.Top;

        var btr = tr.GetObject<BlockTableRecord>(brf.BlockTableRecord);
        if (btr == null)
            return;
        foreach (var id in btr)
            action.Invoke(id);
    }
}