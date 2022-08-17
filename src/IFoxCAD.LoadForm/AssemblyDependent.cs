namespace IFoxCAD.LoadEx;

/// <summary>
/// 加载程序集和加载状态
/// </summary>
public struct LoadState
{
    public Assembly? Assembly;
    public string DllFullName;
    public bool State;
    public LoadState(string dllFullName, bool state, Assembly? assembly = null)
    {
        DllFullName = dllFullName;
        State = state;
        Assembly = assembly;
    }
}

[HarmonyPatch("Autodesk.AutoCAD.ApplicationServices.ExtensionLoader", "OnAssemblyLoad")]
public class AssemblyDependent : IDisposable
{
    #region 公共
    /// <summary>
    /// 当前域加载事件,运行时出错的话,就靠这个事件来解决
    /// </summary>
    public event ResolveEventHandler CurrentDomainAssemblyResolveEvent
    {
        add { AppDomain.CurrentDomain.AssemblyResolve += value; }
        remove { AppDomain.CurrentDomain.AssemblyResolve -= value; }
    }


    #endregion

    #region 字段


    /// <summary>
    /// cad程序域依赖_内存区(不可以卸载)
    /// </summary>
    readonly Assembly[] _cadAssembly;
    /// <summary>
    /// cad程序域依赖_映射区(不可以卸载)
    /// </summary>
    readonly Assembly[] _cadAssemblyRef;
    #endregion

    #region 构造
    /// <summary>
    /// 链式加载dll依赖
    /// </summary>
    public AssemblyDependent()
    {
        //初始化一次,反复load,所以这里只放公共元素
        _cadAssembly = AppDomain.CurrentDomain.GetAssemblies();
        _cadAssemblyRef = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies();
        CurrentDomainAssemblyResolveEvent += AssemblyHelper.DefaultAssemblyResolve;
    }
    #endregion

    #region 获取加载链

    /// <summary>
    /// 加载程序集
    /// </summary>
    /// <param name="dllFullName">dll的文件位置</param>
    /// <param name="loadStates">返回加载链</param>
    /// <param name="byteLoad">true字节加载,false文件加载</param>
    /// <returns> 参数 <paramref name="dllFullName"/> 加载成功标识
    /// <code> 链条后面的不再理会,因为相同的dll引用辨识无意义 </code>
    /// </returns>
    public bool Load(string dllFullName, List<LoadState> loadStates, bool byteLoad = true)
    {
        //var accAsb = typeof(Document).Assembly;
        //CSharpUtils.ReplaceMethod(
        //            accAsb.GetType("Autodesk.AutoCAD.ApplicationServices.ExtensionLoader"),
        //            "OnAssemblyLoad", typeof(AssemblyDependent), "OnAssemblyLoad",
        //            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

        if (dllFullName == null)
            throw new ArgumentNullException(nameof(dllFullName));

        dllFullName = Path.GetFullPath(dllFullName);//相对路径要先转换
        if (!File.Exists(dllFullName))
            throw new ArgumentException("路径不存在");

        List<string> allRefs = new();
        GetAllRefPaths(dllFullName, allRefs);

        bool dllFullNameLoad = false;

        //查询加载链逆向加载,确保前面不丢失
        //这里有问题,从尾巴开始的,就一定是没有任何引用吗?
        for (int i = allRefs.Count - 1; i >= 0; i--)
        {
            var path = allRefs[i];

            //路径转程序集名
            var an = AssemblyName.GetAssemblyName(path).FullName;
            var assembly = _cadAssembly.FirstOrDefault(a => a.FullName == an);
            if (assembly != null)
            {
                loadStates.Add(new LoadState(path, false));//版本号没变不加载
                continue;
            }

            //有一次true,就是true 
            if (path == dllFullName)
                dllFullNameLoad = true;

            try
            {
                var ass = GetPdbAssembly(path, byteLoad);
                if (ass == null)
                {
                    if (byteLoad)
                        ass = Assembly.Load(File.ReadAllBytes(path));
                    else
                        ass = Assembly.LoadFile(path);
                }
                loadStates.Add(new LoadState(path, true, ass));/*加载成功*/
            }
            catch { loadStates.Add(new LoadState(path, false));/*错误造成*/ }
        }
        return dllFullNameLoad;
    }

    /// <summary>
    /// 在debug模式的时候才获取PBD调试信息
    /// </summary>
    /// <param name="path"></param>
    /// <param name="byteLoad"></param>
    /// <returns></returns>
    Assembly? GetPdbAssembly(string path, bool byteLoad)
    {
#if DEBUG
        //为了实现Debug时候出现断点,见链接,加依赖
        // https://www.cnblogs.com/DasonKwok/p/10510218.html
        // https://www.cnblogs.com/DasonKwok/p/10523279.html

        var dir = Path.GetDirectoryName(path);
        var pdbName = Path.GetFileNameWithoutExtension(path) + ".pdb";
        var pdbFullName = Path.Combine(dir, pdbName);
        if (File.Exists(pdbFullName) && byteLoad)
            return Assembly.Load(File.ReadAllBytes(path), File.ReadAllBytes(pdbFullName));
        return null;
#endif
    }

    /// <summary>
    /// 递归获取加载链
    /// </summary>
    /// <param name="dllFullName"></param>
    /// <param name="dllFullNamesOut">返回的集合</param>
    /// <returns></returns>
    void GetAllRefPaths(string dllFullName, List<string> dllFullNamesOut)
    {
        if (dllFullNamesOut.Contains(dllFullName) || !File.Exists(dllFullName))
            return;
        dllFullNamesOut.Add(dllFullName);

        //路径转程序集名
        var assName = AssemblyName.GetAssemblyName(dllFullName).FullName;

        //在当前程序域的 assemblyAs内存区 和 assemblyAsRef映射区 找这个程序集名
        var assemblyAs = _cadAssembly.FirstOrDefault(ass => ass.FullName == assName);
        Assembly assemblyAsRef;

        //内存区有表示加载过
        //映射区有表示查找过,但没有加载(一般来说不存在.只是debug会注释掉 Assembly.Load 的时候用来测试)
        if (assemblyAs != null)
        {
            assemblyAsRef = assemblyAs;
        }
        else
        {
            assemblyAsRef = _cadAssemblyRef.FirstOrDefault(ass => ass.FullName == assName);

            //内存区和映射区都没有的话就把dll加载到映射区,用来找依赖表
            if (assemblyAsRef == null)
            {
                var byteRef = File.ReadAllBytes(dllFullName);
#if true2
                //1548253108:这里会报错,但是我测试不出来它报错.....他提供了解决方案
                //var accAsb = typeof(Document).Assembly;
                const string ext = "Autodesk.AutoCAD.ApplicationServices.ExtensionLoader";
                Harmony hm = new(ext);
                hm.PatchAll();
                assemblyAsRef = Assembly.ReflectionOnlyLoad(byteRef);
                hm.UnpatchAll(ext);
                //CSharpUtils.RestoreMethod();
#else
                // assemblyAsRef = Assembly.ReflectionOnlyLoad(dllFullName); //没有依赖会直接报错
                assemblyAsRef = Assembly.ReflectionOnlyLoad(byteRef);
#endif
            }
        }

        var sb = new StringBuilder();
        //dll拖拉加载路径-搜索路径(可以增加到这个dll下面的所有文件夹?)
        sb.Append(Path.GetDirectoryName(dllFullName));
        sb.Append("\\");

        //遍历依赖,如果存在dll拖拉加载目录就加入dlls集合
        var asse = assemblyAsRef.GetReferencedAssemblies();
        for (int i = 0; i < asse.Length; i++)
        {
            var path = sb.ToString() + asse[i].Name;
            var paths = new string[]
            {
                path + ".dll",
                path + ".exe"
            };
            for (int j = 0; j < paths.Length; j++)
                GetAllRefPaths(paths[j], dllFullNamesOut);
        }
    }

    /// <summary>
    /// 加载信息
    /// </summary>
    public static string? PrintMessage(List<LoadState> loadStates)
    {
        if (loadStates == null)
            return null;

        var sb = new StringBuilder();
        bool flag = true;
        foreach (var item in loadStates)
        {
            if (!item.State)
            {
                sb.Append(Environment.NewLine);
                sb.Append("** ");
                sb.Append(item.DllFullName);
                sb.Append(Environment.NewLine);
                sb.Append("** 此文件已加载,同时重复名称和版本号,跳过!");
                sb.Append(Environment.NewLine);
                flag = false;
            }
        }
        if (flag)
        {
            sb.Append(Environment.NewLine);
            sb.Append("** 链式加载成功!");
            sb.Append(Environment.NewLine);
        }
        return sb.ToString();
    }
    #endregion

    #region 删除文件
    /// <summary>
    /// 递归删除文件夹目录及文件
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    static void DeleteFolder(string dir)
    {
        if (!Directory.Exists(dir)) //如果存在这个文件夹删除之
            return;
        foreach (string d in Directory.GetFileSystemEntries(dir))
        {
            if (File.Exists(d))
                File.Delete(d); //直接删除其中的文件
            else
                DeleteFolder(d); //递归删除子文件夹
        }
        Directory.Delete(dir, true); //删除已空文件夹
    }

    /// <summary>
    /// Debug的时候删除obj目录,防止占用
    /// </summary>
    /// <param name="dllFullName">dll文件位置</param>
    public void DebugDelObjFiles(string dllFullName)
    {
        var filename = Path.GetFileNameWithoutExtension(dllFullName);
        var path = Path.GetDirectoryName(dllFullName);

        var pdb = path + "\\" + filename + ".pdb";
        if (File.Exists(pdb))
            File.Delete(pdb);

        var list = path.Split('\\');
        if (list[list.Length - 1] == "Debug" && list[list.Length - 2] == "bin")
        {
            var projobj = path.Substring(0, path.LastIndexOf("bin")) + "obj";
            DeleteFolder(projobj);
        }
    }
    #endregion

    #region Dispose
    public bool Disposed = false;

    /// <summary>
    /// 显式调用Dispose方法,继承IDisposable
    /// </summary>
    public void Dispose()
    {
        //由手动释放
        Dispose(true);
        //通知垃圾回收机制不再调用终结器(析构器)_跑了这里就不会跑析构函数了
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 析构函数,以备忘记了显式调用Dispose方法
    /// </summary>
    ~AssemblyDependent()
    {
        Dispose(false); //由系统释放
    }

    /// <summary>
    /// 释放
    /// </summary>
    /// <param name="ing"></param>
    protected virtual void Dispose(bool ing)
    {
        if (Disposed) return; //不重复释放
        Disposed = true;//让类型知道自己已经被释放

        CurrentDomainAssemblyResolveEvent -= AssemblyHelper.DefaultAssemblyResolve;
        GC.Collect();
    }
    #endregion
}
