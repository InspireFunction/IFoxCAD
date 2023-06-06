#if Debug
using System.Windows.Forms;

namespace IFoxCAD.Cad;
public static class CheckFactory
{
    /* 
     * 平时command命令的globalName如果重复，加载时会报错
     * 但是并不会告诉你是哪里错了，通常需要花大量时间来查找
     * 将此函数添加在IExtensionApplication.Initialize()函数开头
     * 虽然还会报错，但是至少能知道哪个类下哪个方法导致的报错
     * 聊胜于无吧
     * 2023-05-16 by DYH
     */

    /// <summary>
    /// 检查Command命令重复
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