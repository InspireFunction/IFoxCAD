#define test

namespace Test;

using IFoxCAD.Cad;
using System.Threading;
using Clipboard = System.Windows.Forms.Clipboard;

// https://forums.autodesk.com/t5/net/paste-list-of-objects-from-clipboard-on-dwg-file-using-c-net/td-p/6797606
public class Copyclip
{
#if test
    [IFoxInitialize]
#endif
    public void Init()
    {
        Acap.DocumentManager.DocumentLockModeChanged += DocumentManager_DocumentLockModeChanged;
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
            // 获取剪贴板上面的保存路径
            var format = Clipboard.GetDataObject()
                                  .GetFormats()
                                  .FirstOrDefault(f => f.StartsWith("AutoCAD"));

            if (string.IsNullOrEmpty(format))
                return;

            string fileName;
            var data = (MemoryStream)Clipboard.GetData(format);
            using (var reader = new StreamReader(data))
            {
                fileName = reader.ReadToEnd();
                fileName = fileName.Replace("\0", string.Empty);
                fileName = fileName.Substring(0, fileName.IndexOf(".DWG") + 4);
            }
            if (string.IsNullOrEmpty(Path.GetFileName(fileName)))
                return;

            // 如果剪贴板存在这个路径,才去否决,粘贴文字之类的由原本的命令执行
            _fileName = fileName;
            e.Veto();
            IFoxPasteclip();
        }
        else if (up == "PASTEBLOCK") //ctrl+shift+v 粘贴为块也要自己造
        {
        }
    }

    /// <summary>
    /// 粘贴命令
    /// </summary>
#if test
    [CommandMethod(nameof(IFoxPasteclip))]
#endif
    public void IFoxPasteclip()
    {
        var dm = Acap.DocumentManager;
        if (dm.Count == 0)
            return;

        if (_fileName == null)
            return;

        Env.Printl("粘贴来源: " + _fileName);

        // 获取临时文件的图元id
        var result = new List<ObjectId>();
        using var fileTr = new DBTrans(_fileName, false, FileOpenMode.OpenForReadAndAllShare);
        foreach (var id in fileTr.ModelSpace)
            result.Add(id);

        // 加入当前图纸的块表
        using var tr = new DBTrans();
        var map = new IdMapping();
        tr.Task(() => {
            tr.Database.WblockCloneObjects(
                new ObjectIdCollection(result.ToArray()),
                tr.Database.BlockTableId, // 粘贴目标
                map,
                DuplicateRecordCloning.Replace,
                false);
        });

        // 在jig预览
        List<Entity> ents = new();
        map.GetValues().ForEach(a => {
            var ent = tr.GetObject<Entity>(a);
            if (ent != null)
                ents.Add(ent);
        });

        // 求全部图元的左下角作为基点
        double minx = double.MaxValue;
        double miny = double.MaxValue;
        ents.ForEach(ent => {
            var info = ent.GetBoundingBoxEx();
            minx = minx > info.MinX ? info.MinX : minx;
            miny = miny > info.MinY ? info.MinY : miny;
        });
        var bs = new Point3d(minx, miny, 0);

        var moveJig = new JigEx((mousePoint, drawEntitys) => {
            ents.ForEach(ent => {
                var entClone = (Entity)ent.Clone();
                entClone.Move(bs, mousePoint);
                drawEntitys.Enqueue(entClone);
            });
        });
        moveJig.SetOptions(bs, orthomode: false);
        moveJig.Drag();

        // 加入当前空间
        tr.CurrentSpace.AddEntity(moveJig.Entitys);
    }

    // 有了这个的话,还需要读取剪贴板吗??
    static string? _fileName;

    /// <summary>
    /// 复制命令
    /// </summary>
#if test
    [CommandMethod(nameof(IFoxCopyclip), CommandFlags.UsePickSet)]
#endif
    public void IFoxCopyclip()
    {
        // 此处要先去删除tmp文件夹的上次剪贴板产生的dwg文件
        if (_fileName != null)
            File.Delete(_fileName);

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
            psr = ed.GetSelection();
            if (psr.Status != PromptStatus.OK)
                return;
        }
        result.AddRange(psr.Value.GetObjectIds());

        // 获取临时路径
        string file;
        do
        {
            var t1 = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            t1 = Convert.ToInt32(t1.GetHashCode()).ToString("X");
            var t2 = Convert.ToInt32(t1.GetHashCode()).ToString("X");// 这里是为了满足长度而做的
            file = Path.GetTempPath() + "A$" + t1 + t2[0] + ".DWG";
            Thread.Sleep(1);
        } while (File.Exists(file));

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
        sb.Append(file);
#endif
        var data = new MemoryStream();
        var bytes = Encoding.Unicode.GetBytes(sb.ToString());
        data.Write(bytes, 0, bytes.Length);

        Clipboard.SetData($"AutoCAD.r{Acap.Version.Major}", data);

        // 克隆到目标块表内
        using var fileTr = new DBTrans(file);
        fileTr.Task(() => {
            var map = new IdMapping();
            sdb.WblockCloneObjects(
                new ObjectIdCollection(result.ToArray()),
                fileTr.ModelSpace.ObjectId,
                map,
                DuplicateRecordCloning.Replace,
                false);
        });
        fileTr.SaveFile();
        _fileName = file;
    }
}