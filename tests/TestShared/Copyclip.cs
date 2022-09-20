#define test

namespace Test;

using IFoxCAD.Cad;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;
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

#if test
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
        if (up == "COPYCLIP")
        {
            e.Veto();
            IFoxCopyclip();
        }
        else if (up == "PASTECLIP")
        {
            // 由于不知道校验码是怎么构造的,所以只能否决命令,并且自己实现粘贴
            var fileName = GetCadPasteclip("AutoCAD");
            if (fileName == null)
                return;

            // 如果剪贴板存在这个路径,才去否决,粘贴文字之类的由原本的命令执行
            _fileName = fileName;
            e.Veto();
            IFoxPasteclip();
        }
        else if (up == "COPYBASE") //ctrl+shift+c 带基点复制
        {
            e.Veto();
            IFoxCopyBase();
        }
        else if (up == "PASTEBLOCK") //ctrl+shift+v 粘贴为块
        {
            e.Veto();
            IFoxPasteBlock();
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
    [CommandMethod(nameof(IFoxCopyBase))]
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
    [CommandMethod(nameof(IFoxPasteBlock), CommandFlags.UsePickSet)]
    public void IFoxPasteBlock()
    {
        Paste(true);
    }





    // 有了这个的话,还需要读取剪贴板吗?
    // 需要的,剪贴板可能存在不是dwg路径,而是文字内容等
    static string? _fileName;
    List<string> _delFile = new();

    /// <summary>
    /// 获取剪贴板路径
    /// </summary>
    /// <param name="pasteclipStr">控制不同的cad</param>
    /// <returns>获取是否成功</returns>
    string? GetCadPasteclip(string pasteclipStr = "AutoCAD")
    {
        // 获取剪贴板上面的保存路径
        var format = Clipboard.GetDataObject()
                              .GetFormats()
                              .FirstOrDefault(f => f.StartsWith(pasteclipStr));

        if (string.IsNullOrEmpty(format))
            return null;

        string fileName;
        var data = (MemoryStream)Clipboard.GetData(format);
        using (var reader = new StreamReader(data))
        {
            fileName = reader.ReadToEnd();
            fileName = fileName.Replace("\0", string.Empty);
            fileName = fileName.Substring(0, fileName.IndexOf(".DWG") + 4);
        }
        if (string.IsNullOrEmpty(Path.GetFileName(fileName)))
            return null;
        return fileName;
    }

    /// <summary>
    /// 复制
    /// </summary>
    /// <param name="getPoint"></param>
    public void Copy(bool getPoint)
    {
        Terminate();

        var dm = Acap.DocumentManager;
        if (dm.Count == 0)
            return;

        var doc = dm.MdiActiveDocument;
        var sdb = doc.Database;
        var ed = doc.Editor;

        List<ObjectId> result = new();
        var psr = ed.SelectImplied();// 预选
        if (psr.Status != PromptStatus.OK)
        {
            psr = ed.GetSelection();// 手选
            if (psr.Status != PromptStatus.OK)
                return;
        }
        result.AddRange(psr.Value.GetObjectIds());

        Point3d basePoint = Point3d.Origin;
        if (getPoint)
        {
            var pr = ed.GetPoint("\n选择基点");
            if (pr.Status != PromptStatus.OK)
                return;
            basePoint = pr.Value;
        }

        // 获取临时路径
        do
        {
            _fileName = TimeName();
            Thread.Sleep(1);
        } while (File.Exists(_fileName));

        // 写入剪贴板
        var sb = new StringBuilder();
#if true2
        //每个字符后面插入\0
        for (int i = 0; i < file.Length; i++)
        {
            sb.Append(file[i]);
            sb.Append("\0");
        }
        sb.Append("\0");
        var r15 = "R15";
        for (int i = 0; i < r15.Length; i++)
        {
            sb.Append(r15[i]);
            sb.Append("\0");
        }
        // 后面一段是还是啥?? 校验码??
        // 因为不知道,所以只能自己粘贴的时候处理了
#else
        sb.Append(_fileName);
#endif
        var data = new MemoryStream();
        var bytes = Encoding.Unicode.GetBytes(sb.ToString());
        data.Write(bytes, 0, bytes.Length);

        Clipboard.SetData($"AutoCAD.r{Acap.Version.Major}", data);

        // 克隆到目标块表内
        using var fileTr = new DBTrans(_fileName);
        fileTr.Task(() => {
            var map = new IdMapping();
            sdb.WblockCloneObjects(
                new ObjectIdCollection(result.ToArray()),
                fileTr.ModelSpace.ObjectId,
                map,
                DuplicateRecordCloning.Replace,
                false);
        });

        // 移动到原点
        if (!basePoint.IsEqualTo(Point3d.Origin))
        {
            var ents = fileTr.ModelSpace.GetEntities<Entity>();
            foreach (var ent in ents)
                using (ent.ForWrite())
                    ent.Move(basePoint, Point3d.Origin);
        }

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

        Env.Printl("粘贴来源: " + _fileName);

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
        /// 动态块粘贴,然后用:ctrl+z会导致动态块特性无法恢复,
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

        Point3d basePoint = Point3d.Origin;
        if (!isBlock)
        {
            // 遍历块内
            // 获取左下角点作为基点
            double minx = double.MaxValue;
            double miny = double.MaxValue;
            var btrNew = tr.GetObject<BlockTableRecord>(btrIdNew);
            foreach (var id in btrNew!)
            {
                var ent = tr.GetObject<Entity>(id);
                if (ent == null)
                    continue;
                var info = ent.GetBoundingBoxEx();
                if (ent is BlockReference brf)
                    info.Move(brf.Position, Point3d.Origin);
                minx = minx > info.MinX ? info.MinX : minx;
                miny = miny > info.MinY ? info.MinY : miny;
            }
            basePoint = new Point3d(minx, miny, 0);
        }

        // 预览并获取交互点
        using var moveJig = new JigEx((mousePoint, drawEntitys) => {
            var blockref = new BlockReference(Point3d.Origin, btrIdNew);
            blockref.Move(basePoint, mousePoint);
            drawEntitys.Enqueue(blockref);
        });

        moveJig.SetOptions(basePoint);
        var dr = moveJig.Drag();
        if (dr.Status == PromptStatus.None) // 空格为原点拷贝?
            return;
        if (dr.Status != PromptStatus.OK)
            return;

        // 粘贴为块,创建图元
        if (isBlock)
        {
            tr.CurrentSpace.AddEntity(moveJig.Entitys);
            return;
        }

        // 直接粘贴
        using ObjectIdCollection ids = new();
        ((BlockReference)moveJig.Entitys[0]).ForEach(a => ids.Add(a));

        map = tr.CurrentSpace.DeepCloneEx(ids);
        map.GetValues().ForEach(id => {
            if (!id.IsOk())
                return;
            var ent = tr.GetObject<Entity>(id);
            if (ent == null)
                return;
            using (ent.ForWrite())
                ent.Move(basePoint, moveJig.MousePointWcsLast);
        });
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