namespace IFoxCAD.Cad;

/// <summary>
/// 数据库扩展函数
/// </summary>
public static class DatabaseEx
{
    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="db">数据库</param>
    /// <param name="version">文件版本</param>
    public static void SaveDwgFile(this Database db, DwgVersion version = DwgVersion.AC1800)
    {
        db.SaveFile(version);
    }
    
    /// <summary>
    /// 保存文件<br/>
    /// </summary>
    /// <param name="db">数据库</param>
    /// <param name="version">默认2004dwg;若保存dxf则需要在路径输入扩展名</param>
    /// <param name="automatic">为true时候<paramref name="version"/>无效,将变为自动识别环境变量</param>
    /// <param name="saveAsFile">另存为文件,前台将调用时它将无效,将变为弹出面板</param>
    /// <param name="echoes">保存路径失败的提示</param>
    public static void SaveFile(this Database db, DwgVersion version = DwgVersion.AC1800,
        bool automatic = true,
        string? saveAsFile = null,
        bool echoes = true)
    {
        // 遍历当前所有文档,文档必然是前台的
        Document? doc = null;
        foreach (Document docItem in Acaop.DocumentManager)
        {
            if (docItem.Database.Filename == db.Filename)
            {
                doc = docItem;
                break;
            }
        }
        // 前台开图,使用命令保存;不需要切换文档
        if (doc != null)
        {
            // 无法把 <paramref name="saveAsFile"/>给这个面板
            doc.SendStringToExecute(saveAsFile == null ? "_qsave\n" : $"_Saveas\n", false, true, true);
            return;
        }

        // 后台开图,用数据库保存
        string? fileMsg;
        bool creatFlag = false;
        if (string.IsNullOrWhiteSpace(saveAsFile))
        {
            fileMsg = db.Filename;
            saveAsFile = fileMsg;
            //creatFlag = true;
        }
        else
        {
            fileMsg = saveAsFile;

            // 路径失败也保存到桌面
            var path = Path.GetDirectoryName(saveAsFile);
            if (string.IsNullOrWhiteSpace(path))
            {
                creatFlag = true;
            }
            else if (!Directory.Exists(path))
            {
                try { Directory.CreateDirectory(path); }
                catch { creatFlag = true; }
            }

            // 文件名缺失时
            if (!creatFlag &&
                string.IsNullOrWhiteSpace(Path.GetFileName(saveAsFile)))
                creatFlag = true;
        }
        if (saveAsFile != null)
        {
            var fileNameWith = Path.GetFileNameWithoutExtension(saveAsFile);
            if (string.IsNullOrWhiteSpace(fileNameWith))
                creatFlag = true;
        }
        else
        {
            creatFlag = true;
        }

        if (creatFlag)
        {
            var (error, file) = db.GetOrCreateSaveAsFile();
            if (echoes && error)
                System.Windows.Forms.MessageBox.Show($"错误参数:\n{fileMsg}\n\n它将保存:\n{file}", "错误的文件路径");
            saveAsFile = file;
        }

        if (Path.GetExtension(saveAsFile)!.ToLower().Contains("dxf"))
        {
            // dxf用任何版本号都会报错
#if acad || gcad
            db.DxfOut(saveAsFile, 7, true);
#endif

#if zcad  // 中望这里没有测试
            db.DxfOut(saveAsFile, 7, version, true);
#endif
            return;
        }

        if (automatic)
            version = Env.GetDefaultDwgVersion();

        // dwg需要版本号,而dxf不用,dwg用dxf版本号会报错
        // 若扩展名和版本号冲突,按照扩展名为准
        if (version.IsDxfVersion())
            version = DwgVersion.Current;

        db.SaveAs(saveAsFile, version);
    }
    
    /// <summary>
    /// 获取文件名,无效的话就制造
    /// </summary>
    /// <returns></returns>
    private static (bool error, string path) GetOrCreateSaveAsFile(this Database db)
    {
        var file = db.Filename;
        if (!string.IsNullOrWhiteSpace(file))
            return (false, file);

        // 为了防止用户输入了错误的路径造成无法保存,
        // 所以此处将进行保存到桌面,
        // 而不是弹出警告就结束
        // 防止前台关闭了所有文档导致没有Editor,所以使用 MessageBox 发送警告
        var fileName = Path.GetFileNameWithoutExtension(file);
        var fileExt = Path.GetExtension(file);

        if (string.IsNullOrWhiteSpace(fileName))
            fileName = DateTime.Now.ToString("--yyMMdd--hhmmssffff");
        if (string.IsNullOrWhiteSpace(fileExt))
            fileExt = ".dwg";

        // 构造函数(fileName)用了不存在的路径进行后台打开,就会出现此问题
        // 测试命令 FileNotExist
        var dir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                  + "\\后台保存出错的文件\\";

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        file = dir + fileName + fileExt;
        while (File.Exists(file))
        {
            var time = DateTime.Now.ToString("--yyMMdd--hhmmssffff");
            file = dir + fileName + time + fileExt;
                System.Threading.Thread.Sleep(100);
        }
        return (true, file);
    }

}