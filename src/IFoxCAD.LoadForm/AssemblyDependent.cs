#define HarmonyPatch
#define HarmonyPatch_1

namespace IFoxCAD.LoadEx;

#if HarmonyPatch_1
[HarmonyPatch("Autodesk.AutoCAD.ApplicationServices.ExtensionLoader", "OnAssemblyLoad")]
#endif
public class AssemblyDependent : IDisposable
{
#if HarmonyPatch
    //这个是不能删除的,否则就不执行了
    //HarmonyPatch hook method 返回 false 表示拦截原函数
    public static bool Prefix() { return false; }
#endif

    #region 字段和事件
    /// <summary>
    /// 当前域加载事件,运行时出错的话,就靠这个事件来解决
    /// </summary>
    public event ResolveEventHandler CurrentDomainAssemblyResolveEvent
    {
        add { AppDomain.CurrentDomain.AssemblyResolve += value; }
        remove { AppDomain.CurrentDomain.AssemblyResolve -= value; }
    }

    /// <summary>
    /// 拦截cad的Loader异常:默认是<paramref name="false"/>
    /// </summary>
    public bool PatchExtensionLoader = false;
    #endregion

    #region 构造
    /// <summary>
    /// 链式加载dll依赖
    /// </summary>
    public AssemblyDependent()
    {
        //初始化一次,反复load
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
    /// <returns> 参数 <paramref name="dllFullName"/> 是否加载成功
    /// <code>    链条后面的不再理会,因为相同的dll引用辨识无意义 </code>
    /// </returns>
    public bool Load(string? dllFullName, List<LoadState> loadStates, bool byteLoad = true)
    {
        if (dllFullName == null)
            throw new ArgumentNullException(nameof(dllFullName));

        dllFullName = Path.GetFullPath(dllFullName);//相对路径要先转换
        if (!File.Exists(dllFullName))
            throw new ArgumentException("路径不存在");

        //程序集数组要动态获取(每次Load的时候),
        //否则会变成一个固定数组,造成加载了之后也不会出现成员
        var cadAssembly = AppDomain.CurrentDomain.GetAssemblies();
        var cadAssemblyRef = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies();

        List<string> allRefs = new();
        GetAllRefPaths(cadAssembly, cadAssemblyRef, dllFullName, allRefs);

        bool dllFullNameLoadOk = false;

        //查询加载链逆向加载,确保前面不丢失
        //这里有问题,从尾巴开始的,就一定是没有任何引用吗?
        for (int i = allRefs.Count - 1; i >= 0; i--)
        {
            var allRef = allRefs[i];

            //路径转程序集名
            var an = AssemblyName.GetAssemblyName(allRef).FullName;
            var assembly = cadAssembly.FirstOrDefault(a => a.FullName == an);
            if (assembly != null)
            {
                loadStates.Add(new LoadState(allRef, false));//版本号没变不加载
                continue;
            }

            //有一次true,就是true 
            if (allRef == dllFullName)
                dllFullNameLoadOk = true;

            try
            {
                var ass = GetPdbAssembly(allRef);

#if Debug_WriteLine_null
                if (ass == null)
                    System.Diagnostics.Debug
                        .WriteLine($"****{nameof(Load)}:此文件无加载了pdb对象:" + allRef); 
#endif

#if Debug_WriteLine_notnull
                if (ass != null)
                    System.Diagnostics.Debug
                        .WriteLine($"****{nameof(Load)}:此文件加载了pdb对象:" + allRef);
#endif
                if (ass == null)
                    if (byteLoad)
                        ass = Assembly.Load(File.ReadAllBytes(allRef));
                    else
                        ass = Assembly.LoadFile(allRef);
                loadStates.Add(new LoadState(allRef, true, ass));/*加载成功*/
            }
            catch { loadStates.Add(new LoadState(allRef, false));/*错误造成*/ }
        }
        return dllFullNameLoadOk;
    }

    /// <summary>
    /// 在debug模式的时候才获取PBD调试信息
    /// </summary>
    /// <param name="path"></param>
    /// <param name="byteLoad"></param>
    /// <returns></returns>
    Assembly? GetPdbAssembly(string? path)
    {
#if DEBUG
        //为了实现Debug时候出现断点,见链接,加依赖
        // https://www.cnblogs.com/DasonKwok/p/10510218.html
        // https://www.cnblogs.com/DasonKwok/p/10523279.html

        var dir = Path.GetDirectoryName(path);
        var pdbName = Path.GetFileNameWithoutExtension(path) + ".pdb";
        var pdbFullName = Path.Combine(dir, pdbName);
        if (File.Exists(pdbFullName))
            return Assembly.Load(File.ReadAllBytes(path), File.ReadAllBytes(pdbFullName));
#endif
        return null;
    }

    /// <summary>
    /// 递归获取加载链
    /// </summary>
    /// <param name="cadAssembly">程序集_内存区</param>
    /// <param name="cadAssemblyRef">程序集_映射区</param>
    /// <param name="dllFullName">dll文件位置</param>
    /// <param name="dllFullNamesOut">返回的集合</param>
    /// <returns></returns>
    void GetAllRefPaths(Assembly[] cadAssembly,
                        Assembly[] cadAssemblyRef,
                        string? dllFullName,
                        List<string> dllFullNamesOut)
    {
        if (dllFullName == null)
            throw new ArgumentNullException(nameof(dllFullName));

        if (dllFullNamesOut.Contains(dllFullName) || !File.Exists(dllFullName))
            return;
        dllFullNamesOut.Add(dllFullName);

        var assemblyAsRef = GetAssembly(cadAssembly, cadAssemblyRef, dllFullName);
        if (assemblyAsRef == null)
            return;

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
                GetAllRefPaths(cadAssembly, cadAssemblyRef, paths[j], dllFullNamesOut);
        }
    }

    /// <summary>
    /// 在内存区和映射区找已经加载的程序集
    /// </summary>
    /// <param name="cadAssembly">程序集_内存区</param>
    /// <param name="cadAssemblyRef">程序集_映射区</param>
    /// <param name="dllFullName">dll文件位置</param>
    /// <returns></returns>
    Assembly? GetAssembly(Assembly[] cadAssembly,
                          Assembly[] cadAssemblyRef,
                          string? dllFullName)
    {
        //路径转程序集名
        var assName = AssemblyName.GetAssemblyName(dllFullName).FullName;
        //在当前程序域的 assemblyAs内存区 和 assemblyAsRef映射区 找这个程序集名
        var assemblyAs = cadAssembly.FirstOrDefault(ass => ass.FullName == assName);

        //内存区有表示加载过
        //映射区有表示查找过,但没有加载(一般来说不存在.只是debug会注释掉 Assembly.Load 的时候用来测试)
        if (assemblyAs != null)
            return assemblyAs;

        //映射区
        var assemblyAsRef = cadAssemblyRef.FirstOrDefault(ass => ass.FullName == assName);

        //内存区和映射区都没有的话就把dll加载到映射区,用来找依赖表
        if (assemblyAsRef != null)
            return assemblyAsRef;

        var byteRef = File.ReadAllBytes(dllFullName);
        if (PatchExtensionLoader)
        {
#if HarmonyPatch_1
            /* QQ1548253108:这里会报错,他提供了解决方案.
             * 方案一:
             * 在类上面加 [HarmonyPatch("Autodesk.AutoCAD.ApplicationServices.ExtensionLoader", "OnAssemblyLoad")]
             */
            const string ext = "Autodesk.AutoCAD.ApplicationServices.ExtensionLoader";
            Harmony hm = new(ext);
            hm.PatchAll();
            assemblyAsRef = Assembly.ReflectionOnlyLoad(byteRef);
            hm.UnpatchAll(ext);
#endif
#if HarmonyPatch_2
            //方案二:跟cad耦合了
            const string ext = "Autodesk.AutoCAD.ApplicationServices.ExtensionLoader";
            var docAss = typeof(Autodesk.AutoCAD.ApplicationServices.Document).Assembly;
            var a = docAss.GetType(ext);
            var b = a.GetMethod("OnAssemblyLoad");
            Harmony hm = new(ext);
            hm.Patch(b, new HarmonyMethod(GetType(), "Dummy"));
            assemblyAsRef = Assembly.ReflectionOnlyLoad(byteRef);
            hm.UnpatchAll(ext);
#endif
        }
        else
        {
            /*
             * 0x01 此句没有依赖会直接报错 
             *      assemblyAsRef = Assembly.ReflectionOnlyLoad(dllFullName);
             * 0x02 重复加载无修改的同一个dll,会出现如下异常:
             *      System.IO.FileLoadException:
             *      “API 限制: 程序集“”已从其他位置加载。无法从同一个 Appdomain 中的另一新位置加载该程序集。”
             *      catch 兜不住的,仍然会在cad上面打印,原因是程序集数组要动态获取(已改)
             */
            try
            {
                assemblyAsRef = Assembly.ReflectionOnlyLoad(byteRef);
            }
            catch (System.IO.FileLoadException)
            { }
        }
        return assemblyAsRef;
    }


    /// <summary>
    /// 加载信息
    /// </summary>
    public static string? PrintMessage(List<LoadState> loadStates,
                                       PrintModes modes = PrintModes.All)
    {
        if (loadStates == null)
            return null;

        var sb = new StringBuilder();
        var ok = loadStates.FindAll(a => a.State);
        var no = loadStates.FindAll(a => !a.State);

        if ((modes & PrintModes.Yes) == PrintModes.Yes)
        {
            if (ok.Count != 0)
            {
                sb.Append("** 这些文件加载成功!");
                foreach (var item in ok)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append("++ ");
                    sb.Append(item.DllFullName);
                }
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
            }
        }

        if ((modes & PrintModes.No) == PrintModes.No)
        {
            if (no.Count != 0)
            {
                sb.Append("** 这些文件已被加载过,同时重复名称和版本号,跳过!");
                foreach (var item in no)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append("-- ");
                    sb.Append(item.DllFullName);
                }
            }
        }
        return sb.ToString();
    }
    #endregion

    #region 删除文件 
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
            FileEx.DeleteFolder(projobj);
        }
    }
    #endregion

    #region 移动文件
    /// <summary>
    /// Debug的时候移动obj目录,防止占用
    /// </summary>
    public void DebugMoveObjFiles(string? dllFullName, Action action)
    {
        // 临时文件夹_pdb的,无论是否创建这里都应该进行删除
        const string Temp = "Temp";

        string? temp_Pdb_dest = null;
        string? temp_Pdb_source = null;
        string? temp_Obj_dest = null; ;
        string? temp_Obj_source = null;
        try
        {
            var filename = Path.GetFileNameWithoutExtension(dllFullName);
            var path = Path.GetDirectoryName(dllFullName);

            //新建文件夹_临时目录
            temp_Pdb_dest = path + $"\\{Temp}\\";
            //移动文件进去
            temp_Pdb_source = path + "\\" + filename + ".pdb";
            FileEx.MoveFolder(temp_Pdb_source, temp_Pdb_dest);

            //检查是否存在obj文件夹,有就递归移动
            var list = path.Split('\\');
            if (list[list.Length - 1] == "Debug" && list[list.Length - 2] == "bin")
            {
                var proj = path.Substring(0, path.LastIndexOf("bin"));
                temp_Obj_source = proj + "obj";
                temp_Obj_dest = proj + $"{Temp}";
                FileEx.MoveFolder(temp_Obj_source, temp_Obj_dest);
            }
            action.Invoke();
        }
        finally
        {
            // 还原移动
            FileEx.MoveFolder(temp_Pdb_dest, temp_Pdb_source);
            FileEx.DeleteFolder(temp_Pdb_dest);

            FileEx.MoveFolder(temp_Obj_dest, temp_Obj_source);
            FileEx.DeleteFolder(temp_Obj_dest);
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
    }
    #endregion
}

public enum PrintModes
{
    Yes = 1,
    No = 2,
    All = Yes | No,
}

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

public class FileEx
{
    /// <summary>
    /// 判断含有文件名和后缀
    /// </summary>
    /// <param name="pathOrFile">路径或者完整文件路径</param>
    static bool ContainFileName(string? pathOrFile)
    {
        // 判断输入的是单文件,它可能不存在
        var a = Path.GetDirectoryName(pathOrFile);
        var b = Path.GetFileName(pathOrFile);
        var c = Path.GetExtension(pathOrFile);
        // 是文件
        return a.Length > 0 && b.Length > 0 && c.Length > 0;
    }

    /// <summary>
    /// 移动文件夹中的所有文件夹与文件到另一个文件夹
    /// </summary>
    /// <param name="sourcePathOrFile">源文件夹</param>
    /// <param name="destPath">目标文件夹</param>
    public static void MoveFolder(string? sourcePathOrFile, string? destPath)
    {
        if (sourcePathOrFile is null)
            throw new ArgumentException(nameof(sourcePathOrFile));
        if (destPath is null)
            throw new ArgumentException(nameof(destPath));

        if (ContainFileName(destPath))
            destPath = Path.GetDirectoryName(destPath);

        //目标目录不存在则创建
        if (!Directory.Exists(destPath))
            Directory.CreateDirectory(destPath);

        if (ContainFileName(sourcePathOrFile))
        {
            // 如果是单个文件,就移动到目录就好了
            if (File.Exists(sourcePathOrFile))
            {
                destPath += "\\" + Path.GetFileName(sourcePathOrFile);
                File.Move(sourcePathOrFile, destPath);
                return;
            }
            return;
        }

        // 如果是文件就改为路径
        if (!Directory.Exists(sourcePathOrFile))
        {
            sourcePathOrFile = Path.GetDirectoryName(sourcePathOrFile);
            if (!Directory.Exists(sourcePathOrFile))
                throw new DirectoryNotFoundException("源目录不存在！");
        }
        MoveFolder2(sourcePathOrFile, destPath);
    }

    /// <summary>
    /// 移动文件夹中的所有文件夹与文件到另一个文件夹
    /// </summary>
    /// <param name="sourcePath">源文件夹</param>
    /// <param name="destPath">目标文件夹</param>
    static void MoveFolder2(string sourcePath, string destPath)
    {
        //目标目录不存在则创建
        if (!Directory.Exists(destPath))
            Directory.CreateDirectory(destPath);

        //获得源文件下所有文件
        var files = new List<string>(Directory.GetFiles(sourcePath));
        files.ForEach(c => {
            string destFile = Path.Combine(destPath, Path.GetFileName(c));
            //覆盖模式
            if (File.Exists(destFile))
                File.Delete(destFile);
            File.Move(c, destFile);
        });
        //获得源文件下所有目录文件
        List<string> folders = new(Directory.GetDirectories(sourcePath));

        folders.ForEach(c => {
            string destDir = Path.Combine(destPath, Path.GetFileName(c));
            //Directory.Move必须要在同一个根目录下移动才有效，不能在不同卷中移动。
            //Directory.Move(c, destDir);

            //采用递归的方法实现
            MoveFolder2(c, destDir);
        });
    }

    /// <summary>
    /// 递归删除文件夹目录及文件
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static void DeleteFolder(string? dir)
    {
        if (dir is null)
            throw new ArgumentException(nameof(dir));
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
}