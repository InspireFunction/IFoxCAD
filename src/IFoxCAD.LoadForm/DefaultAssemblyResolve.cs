namespace IFoxCAD.LoadEx;

public class AssemblyHelper
{
    #region  程序域运行事件 
    /// <summary>
    /// 程序域运行事件
    /// <code>
    /// 动态加载要注意所有的引用外的dll的加载顺序
    /// Acad2008若没有这个事件,会使动态命令执行时候无法引用当前的程序集函数
    /// 跨程序集反射
    /// 动态加载时,dll的地址会在系统的动态目录里,而它所处的程序集(运行域)是在动态目录里.
    /// netload会把所处的运行域给改到cad自己的,而动态加载不通过netload,所以要自己去改.
    /// 这相当于是dll注入的意思,只是动态加载的这个"dll"不存在实体,只是一段内存.
    /// </code>
    /// </summary>
    public static Assembly? DefaultAssemblyResolve(object sender, ResolveEventArgs args)
    {
        var cadAss = AppDomain.CurrentDomain.GetAssemblies();

        //名称和版本号都一致的,调用它
        var load = cadAss.FirstOrDefault(ass => ass.GetName().FullName == args.Name);
        if (load != null)
            return load;

        //获取名称一致,但是版本号不同的,调用最后的可用版本
        var ag = args.Name.Split(',')[0];

        //获取最后一个符合条件的,
        //否则a.dll引用b.dll函数的时候,b.dll修改重生成之后,加载进去会调用第一个版本的b.dll
        foreach (var item in cadAss)
        {
            if (item.GetName().FullName.Split(',')[0] != ag)
                continue;

            //为什么加载的程序版本号最后要是*
            //因为vs会帮你迭代这个版本号,所以最后的可用就是循环到最后的.
            load = item;
        }
        return load;
    }
    #endregion
}