#if true
namespace IFoxCAD.Cad;

using System.Diagnostics;

// 作者: [VB.net]福萝卜  莱昂纳多·胖子
// Email:oneeshine@163.com
// QQ: 461884072
// 测试 2006-2019+

/// <summary>
/// 去教育版
/// </summary>
/// <returns></returns>
internal class EMR
{
    /// <summary>
    /// 释放库
    /// </summary>
    /// <param name="loadLibraryIntPtr">句柄</param>
    /// <returns></returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    static extern IntPtr FreeLibrary(IntPtr loadLibraryIntPtr);

    /// <summary>
    /// 获取一个应用程序或dll的模块句柄,要求已经载入
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern IntPtr GetModuleHandle(string name);

    /// <summary>
    /// 获取要引入的函数,将符号名或标识号转换为DLL内部地址
    /// </summary>
    /// <param name="hModule">exe/dll句柄</param>
    /// <param name="procName">接口名</param>
    /// <returns></returns>
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
    static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    /// <summary>
    /// 虚拟保护
    /// </summary>
    /// <param name="lpAddress"></param>
    /// <param name="dwSize"></param>
    /// <param name="flNewProtect"></param>
    /// <param name="lpflOldProtect"></param>
    /// <returns></returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern bool VirtualProtect(IntPtr lpAddress, IntPtr dwSize, uint flNewProtect, ref uint lpflOldProtect);

    /// <summary>
    /// 去教育版
    /// </summary>
    /// <returns></returns>
    public static string Remove(bool echoe = false)
    {
        var dllName = Env.GetAcapVersionDll();
        IntPtr moduleHandle = GetModuleHandle(dllName);
        if (moduleHandle == IntPtr.Zero)
            return typeof(EMR).FullName + "." + nameof(Remove) + "找不到模块：" + dllName;

        string funcname = System.Text.Encoding.Unicode.GetString(new byte[] { 63 });
        if (IntPtr.Size == 4)
            funcname += "isEMR@AcDbDatabase@@QBE_NXZ";
        else
            funcname += "isEMR@AcDbDatabase@@QEBA_NXZ";

        IntPtr funcAdress = GetProcAddress(moduleHandle, funcname);
        if (funcAdress == IntPtr.Zero)
            return "无法找指定函数：" + funcname;

        IntPtr ptr;
        if (IntPtr.Size == 4)
            ptr = new IntPtr(funcAdress.ToInt32() + 3);
        else
            ptr = new IntPtr(funcAdress.ToInt64() + 4);

        if (!CheckFunc(ref ptr, 51, 2) && echoe)//08 通过此处
            Debug.WriteLine("无法验证函数体：0x33");
        IntPtr destPtr = ptr;

        if (!CheckFunc(ref ptr, 57, 6) && echoe)//08 无法通过此处,所以只是打印提示
            Debug.WriteLine("无法验证函数体：0x39");
        if (!CheckFunc(ref ptr, 15, 2) && echoe)//08 无法通过此处,所以只是打印提示
            Debug.WriteLine("无法验证函数体：0x0F");

        uint flag = default;
        uint tccc = default;

        IntPtr ip100 = new(100);
        if (!VirtualProtect(destPtr, ip100, 64, ref flag))// 修改内存权限
            return "内存模式修改失败!";

        Marshal.WriteByte(destPtr, 137);
        VirtualProtect(destPtr, ip100, flag, ref tccc);// 恢复内存权限

        return string.Empty;
    }

    /// <summary>
    /// 验证函数体
    /// </summary>
    /// <param name="adress"></param>
    /// <param name="val"></param>
    /// <param name="len"></param>
    /// <returns></returns>
    static bool CheckFunc(ref IntPtr adress, byte val, int len)
    {
        if (Marshal.ReadByte(adress) == 233)
        {
            if (IntPtr.Size == 4)
            {
                var pass = Marshal.ReadInt32(new IntPtr(adress.ToInt32() + 1));
                adress = new IntPtr(adress.ToInt32() + pass + 5);
            }
            else
            {
                var pass = Marshal.ReadInt64(new IntPtr(adress.ToInt64() + 1));
                adress = new IntPtr(adress.ToInt64() + pass + 5);
            }
        }
        if (Marshal.ReadByte(adress) == val)
        {
            if (IntPtr.Size == 4)
                adress = new IntPtr(adress.ToInt32() + len);
            else
                adress = new IntPtr(adress.ToInt64() + len);
            return true;
        }
        return false;
    }
}
#endif