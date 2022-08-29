namespace IFoxCAD.LoadEx;

public class AssemblyHelper
{
    /// <summary>
    /// <code>
    /// 程序域运行事件
    /// 这相当于是dll注入的意思,只是动态加载的这个"dll"不存在实体,只是一段内存.
    /// 它总是被 <seealso cref="AppDomain.CurrentDomain.AssemblyResolve"/>事件使用
    /// 0x01 动态加载要注意所有的引用外的dll的加载顺序
    /// 0x02 指定版本: Acad2008若没有这个事件,会使动态命令执行时候无法引用当前的程序集函数
    /// 0x03 目录构成: 动态加载时,dll的地址会在系统的动态目录里,而它所处的程序集(运行域)是在动态目录里.
    /// 0x04 命令构成: cad自带的netload会把所处的运行域给改到cad自己的,而动态加载不通过netload,所以要自己去改.
    /// </code>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    /// <returns>程序集如果为空就不会调用</returns>
    public static Assembly? DefaultAssemblyResolve(object sender, ResolveEventArgs args)
    {
        Assembly? result = null;
        var cadAss = AppDomain.CurrentDomain.GetAssemblies();

        // 名称和版本号都一致的,调用它
        result = cadAss.FirstOrDefault(ass => ass.GetName().FullName == args.Name);
        if (result != null)
            return result;

        // 获取名称一致,但是版本号不同的,调用最后的可用版本
        var ag = GetAssemblyName(args.Name);

        // 获取最后一个符合条件的,
        // 否则a.dll引用b.dll函数的时候,b.dll修改重生成之后,
        // 加载进去会调用第一个版本的b.dll,
        // vs会迭代程序版本号的*,所以最后的可用就是循环到最后的.
        for (int i = 0; i < cadAss.Length; i++)
        {
            if (GetAssemblyName(cadAss[i].GetName().FullName) != ag)
                continue;
            result = cadAss[i];
        }
        return result;
    }

    static string GetAssemblyName(string argString)
    {
        return argString.Substring(argString.IndexOf(','));
    }
}