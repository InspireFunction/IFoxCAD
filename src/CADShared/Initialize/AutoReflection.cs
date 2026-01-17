#pragma warning disable CS1591 // 缺少XML注释
#pragma warning disable CS1572 // XML注释中有不存在的参数
#pragma warning disable CS1573 // 参数在XML注释中没有匹配的参数标记

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace IFoxCAD.Cad;

public class AutoReflection
{
    private readonly string _dllName;
    private readonly AutoRegConfig _config;
    AutoClass? autoClass = null;

    public AutoReflection(string dllName, AutoRegConfig config)
    {
        _dllName = dllName;
        _config = config;
    }

    public void Initialize()
    {
        autoClass = new AutoClass();
        autoClass.Initialize();
    }

    public void Terminate()
    {
        autoClass?.Terminate();
    }



    // CheckFactory.

    /// <summary>
    /// 检查当前程序域重复出现命令,
    /// 当出现重复时候将引起断点
    /// </summary>
    public static void DebugCheckCmdRecurrence()
    {
        HashSet<string> keys = new();

        // 本dll中存在冲突命令,此时cad自动接口可以运行,但是加载命令之后会报错,因此利用断点告诉程序员
        var types = AutoClass.AppDomainGetTypes(Assembly.GetCallingAssembly().GetName().Name);
        foreach (var type in types)
        {
            var mets = type.GetMethods();
            for (int ii = 0; ii < mets.Length; ii++)
            {
                var method = mets[ii];
                var attr = method.GetCustomAttributes(true);
                for (int jj = 0; jj < attr.Length; jj++)
                    if (attr[jj] is CommandMethodAttribute att)
                    {
                        if (keys.Contains(att.GlobalName))
                            Debugger.Break();
                        keys.Add(att.GlobalName);
                    }
            }
        }

        // 其他dll中存在冲突命令,此时会覆盖命令,友好的提示程序员
        keys.Clear();
        HashSet<string> msgMod = new();
        foreach (var type in types)
        {
            var mets = type.GetMethods();
            for (int ii = 0; ii < mets.Length; ii++)
            {
                var method = mets[ii];
                var attr = method.GetCustomAttributes(true);
                for (int jj = 0; jj < attr.Length; jj++)
                    if (attr[jj] is CommandMethodAttribute att)
                    {
                        if (keys.Contains(att.GlobalName))
                            msgMod.Add(att.GlobalName);
                        keys.Add(att.GlobalName);
                    }
            }
        }

        var sb = new StringBuilder();
        foreach (string key in msgMod)
            sb.AppendLine(key);
        if (sb.Length != 0)
        {
            Env.Printl("当前cad环境加载的多个DLL中存在重复命令将被覆盖:");
            Env.Printl("{");
            Env.Printl(sb.ToString());
            Env.Printl("}");
        }
    }
}