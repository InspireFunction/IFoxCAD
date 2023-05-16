#if Debug
namespace IFoxCAD.Cad;
public static class CheckFactory
{
    /// <summary>
    /// 检查Command命令重复
    /// 将此函数添加在IExtensionApplication.Initialize()开头
    /// 2023-05-16 by DYH
    /// </summary>
    public static void CheckDuplicateCommand(Assembly? assembly = null)
    {
        var dic = new Dictionary<string, List<string>>();
        assembly ??= Assembly.GetCallingAssembly();
        var typeArray = assembly.GetTypes();
        foreach (var type in typeArray)
        {
            foreach (var method in type.GetMethods())
            {
                foreach (Attribute add in method.GetCustomAttributes(typeof(CommandMethodAttribute), false))
                {
                    if (add is CommandMethodAttribute cma)
                    {
                        if (!dic.ContainsKey(cma.GlobalName))
                        {
                            dic.Add(cma.GlobalName, new());
                        }
                        dic[cma.GlobalName].Add(type.Name + "." + method.Name);
                    }
                }
            }
        }
        var strings = dic.Where(o => o.Value.Count() > 1).Select(o => o.Key + "命令重复，在类" + string.Join("和", o.Value) + "中");
        string str = string.Join(Environment.NewLine, strings);
        if (!string.IsNullOrEmpty(str))
            MessageBox.Show(str, "错误：重复命令！");
    }
}
#endif