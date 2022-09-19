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
            e.Veto();
            IFoxPasteclip();
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

        Env.Printl("粘贴来源: " + fileName);

        // 获取临时文件的图元id
        var result = new List<ObjectId>();
        using var fileTr = new DBTrans(fileName, false, FileOpenMode.OpenForReadAndAllShare);
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

        var moveJig = new JigEx((mousePoint, drawEntitys) => {
            ents.ForEach(ent => {
                var entClone = (Entity)ent.Clone();
                entClone.Move(Point3d.Origin, mousePoint);
                drawEntitys.Enqueue(entClone);
            });
        });
        moveJig.SetOptions(Point3d.Origin, orthomode: false);
        moveJig.Drag();

        // 加入当前空间
        tr.CurrentSpace.AddEntity(moveJig.Entitys);
    }


    /// <summary>
    /// 复制命令
    /// </summary>
#if test
    [CommandMethod(nameof(IFoxCopyclip), CommandFlags.UsePickSet)]
#endif
    public void IFoxCopyclip()
    {
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
            file = Path.GetTempPath()
                   + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".DWG";
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
    }
}