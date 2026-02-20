using IFoxCAD.Cad;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PEInfoTest;

/// <summary>
/// 测试 AcadPeInfo 功能的程序
/// 来源 https://blog.csdn.net/zgke/article/details/2955560 我在他基础上面增加了X64的处理
/// </summary>
class Program
{
    /// <summary>
    /// 主函数
    /// </summary>
    static void Main(string[] args)
    {
        var path = @"C:\Program Files (x86)\AutoCAD 2008\acad.exe";

        var pe = new PeInfo(path);

        // 输出所有的函数名
        var sets = pe.ExportDirectory.FunctionNames();
        foreach (var item in sets)
        {
            if (item.Contains("acedRegenLayers"))
            {
                Console.WriteLine(item);
            }
        }

        /*
?acedRegenLayers@@YGXABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@H@Z
?acedRegenLayers@@YGXABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@HH@Z
?acedRegenLayersForOneVP@@YGXABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@ABVAcDbObjectId@@H_N@Z
         */


        // 原作者的封装
        var ss = pe.GetPETable();
        foreach (var item in ss.Tables)
        {
        }
    }
}
