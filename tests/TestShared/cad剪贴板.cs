using Clipboard = System.Windows.Forms.Clipboard;

namespace JoinBoxAcad;
// https://forums.autodesk.com/t5/net/paste-list-of-objects-from-clipboard-on-dwg-file-using-c-net/td-p/6797606
public class cad剪贴板
{
    //[IFoxInitialize]
    public void Init()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        doc.CommandWillStart += Doc_CommandWillStart;
    }

    void Doc_CommandWillStart(object sender, CommandEventArgs e)
    {
        // 粘贴命令
        if (e.GlobalCommandName != "PASTECLIP")
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

        using var tr = new DBTrans(fileName, true, FileOpenMode.OpenForReadAndAllShare);
        foreach (var id in tr.ModelSpace)
        {
            if (!id.IsOk())
                continue;
            var ent = tr.GetObject<Entity>(id, OpenMode.ForWrite);
            ent!.ColorIndex = 30;
        }
        tr.SaveFile((DwgVersion)27);
    }

    [CommandMethod(nameof(Ccopy), CommandFlags.UsePickSet)]
    public void Ccopy()
    {
        var dm = Acap.DocumentManager;
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
        // var path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\"
        var path = Path.GetTempPath()
                   + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".DWG";

        // 写入剪贴板
        var data = new MemoryStream();
        var bytes = Encoding.Unicode.GetBytes(path);
        data.Write(bytes, 0, bytes.Length);
        Clipboard.SetData($"AutoCAD.r{Acap.Version.Major}", data);

        // 克隆到目标块表内
        using var tr2 = new DBTrans(path);
        tr2.Task(() => {
            var map = new IdMapping();
            sdb.WblockCloneObjects(
                new ObjectIdCollection(result.ToArray()),
                tr2.ModelSpace.ObjectId,
                map,
                DuplicateRecordCloning.Replace,
                false);
        });
        tr2.SaveFile();
    }

    [CommandMethod(nameof(NewPaste), CommandFlags.Transparent)]
    public void NewPaste()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        var obj = (System.Windows.Forms.DataObject)Clipboard.GetDataObject();
        if (obj == null)
            return;

        // Find out whether the clipboard contains AutoCAD data
        var formats = obj.GetFormats();
        string formatFound = "";
        bool foundDwg = false;
        foreach (var name in formats)
        {
            if (name.Contains("AutoCAD.r"))
            {
                foundDwg = true;
                formatFound = name;
                break;
            }
        }
        if (foundDwg)
        {
            // If Found, discover where is the Database
            var MStr = obj.GetData(formatFound) as MemoryStream;
            if (MStr == null)
                return;

            MStr.Position = 0;
            var sr = new StreamReader(MStr, Encoding.UTF8);
            var myStr = sr.ReadToEnd();

            //删除不必要的字符
            var sda = myStr.Replace("\0", "");
            int index = sda.IndexOf(".DWG");
            if (index > 0)
                sda = sda.Substring(0, index + 4);

            doc.Editor.WriteMessage(sda);
        }
        else
        {
            // If not, pasteclip normally
            doc.SendStringToExecute("\x1B\x1B_.PASTECLIP ", true, false, true);
        }
    }
}