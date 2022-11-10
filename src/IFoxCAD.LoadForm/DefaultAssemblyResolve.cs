namespace IFoxCAD.LoadEx;

using System.Diagnostics;

public class AssemblyHelper
{
    public static Assembly? ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs e)
    {
        var cadAss = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies();
        return Resolve(cadAss, sender, e);
    }

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
    /// <param name="e"></param>
    /// <returns>程序集如果为空就不会调用</returns>
    public static Assembly? DefaultAssemblyResolve(object sender, ResolveEventArgs e)
    {
        var cadAss = AppDomain.CurrentDomain.GetAssemblies();
        return Resolve(cadAss, sender, e);
    }

    public static Assembly? Resolve(Assembly[] cadAss, object sender, ResolveEventArgs e)
    {
        // 名称和版本号都一致的,调用它
        var result = cadAss.FirstOrDefault(ass => ass.GetName().FullName == e.Name);
        if (result != null)
            return result;

        // 获取名称一致,但是版本号不同的,调用最后的可用版本
        var ag = GetAssemblyName(e.Name);
        // 获取最后一个符合条件的,
        // 否则a.dll引用b.dll函数的时候,b.dll修改重生成之后,加载进去会调用第一个版本的b.dll,
        // vs会迭代程序版本号的*,所以最后的可用就是循环到最后的.
        for (int i = 0; i < cadAss.Length; i++)
            if (GetAssemblyName(cadAss[i].GetName().FullName) == ag)
                result = cadAss[i];

        if (result == null)
        {
            // 惊惊: acad21+vs22 容易触发这个资源的问题
            // https://stackoverflow.com/questions/4368201/
            string[] fields = e.Name.Split(',');
            string name = fields[0];
            string culture = fields[2];
            if (name.EndsWith(".resources") && !culture.EndsWith("neutral"))
                return null;
        }

        if (result == null)
        {
            // acad08debug的时候查看一些变量时候,会弹出找不到它,并且闪退
            if (ag == "Microsoft.CSharp")
                return null;
        }

        if (result == null)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(LoadEx)}------------------------------------------------------------");
            sb.AppendLine(nameof(DefaultAssemblyResolve) + "出错,程序集无法找到它");
            sb.AppendLine("++参数名:: " + GetAssemblyName(e.Name));
            sb.AppendLine("++参数完整信息:: " + e.Name);
            for (int i = 0; i < cadAss.Length; i++)
                sb.AppendLine("-------匹配对象:: " + GetAssemblyName(cadAss[i].GetName().FullName));

            sb.AppendLine($"程序集找不到,遇到无法处理的错误,杀死当前进程!");
            sb.AppendLine($"{nameof(LoadEx)}------------------------------------------------------------");
            Debug.WriteLine(sb.ToString());

            Process.GetCurrentProcess().Kill();
            //Debugger.Break();
        }

        return result;
    }

    static string GetAssemblyName(string argString)
    {
        return argString.Substring(0, argString.IndexOf(','));
    }
}