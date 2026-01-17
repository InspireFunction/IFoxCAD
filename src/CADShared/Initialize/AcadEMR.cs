#if true 
#if acad
namespace IFoxCAD.Cad;

// 作者: [VB.net]福萝卜  莱昂纳多·胖子
// Email:oneeshine@163.com
// QQ: 461884072
// 测试 2006-2019+

/// <summary>
/// 去教育版
/// </summary>
/// <returns></returns>
internal class AcadEMR
{
    // /// <summary>
    // /// 释放库
    // /// </summary>
    // /// <param name="loadLibraryIntPtr">句柄</param>
    // /// <returns></returns>
    // [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    // static extern IntPtr FreeLibrary(IntPtr loadLibraryIntPtr);

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
    // ReSharper disable once IdentifierTypo
    static extern bool VirtualProtect(IntPtr lpAddress, IntPtr dwSize, uint flNewProtect, ref uint lpflOldProtect);


    /// <summary>
    /// 移除教育版
    /// </summary>
    /// <param name="echoes">打印出错信息</param>
    public static void Remove(bool echoes = false)
    {
        var dllName = Env.GetAcapVersionDll();
        var moduleHandle = GetModuleHandle(dllName);
        if (moduleHandle == IntPtr.Zero)
        {
            if (echoes)
                Env.Printl(typeof(AcadEMR).FullName + "." + nameof(Remove) + "找不到模块：" + dllName);
            return;
        }

        var funcName = Encoding.Unicode.GetString([63]);
        if (IntPtr.Size == 4)
            funcName += "isEMR@AcDbDatabase@@QBE_NXZ";
        else
            funcName += "isEMR@AcDbDatabase@@QEBA_NXZ";

        var funcAddress = GetProcAddress(moduleHandle, funcName);
        if (funcAddress == IntPtr.Zero)
        {
            if (echoes)
                Env.Printl("无法找指定函数：" + funcName);
            return;
        }

        var ptr = IntPtr.Size == 4
            ? new IntPtr(funcAddress.ToInt32() + 3)
            : new IntPtr(funcAddress.ToInt64() + 4);

        if (!CheckFunc(ref ptr, 51, 2)) // 08 通过此处
            if (echoes)
                Env.Printl("无法验证函数体：0x33");
        var destPtr = ptr;

        if (!CheckFunc(ref ptr, 57, 6)) // 08 无法通过此处,所以只是打印提示
            if (echoes)
                Env.Printl("无法验证函数体：0x39");
        if (!CheckFunc(ref ptr, 15, 2)) // 08 无法通过此处,所以只是打印提示
            if (echoes)
                Env.Printl("无法验证函数体：0x0F");

        uint flag = default;
        // ReSharper disable once IdentifierTypo
        uint tccc = default;

        IntPtr ip100 = new(100);
        if (!VirtualProtect(destPtr, ip100, 64, ref flag)) // 修改内存权限
        {
            if (echoes)
                Env.Printl("内存模式修改失败!");
            return;
        }

        Marshal.WriteByte(destPtr, 137);
        VirtualProtect(destPtr, ip100, flag, ref tccc); // 恢复内存权限
    }

    /// <summary>
    /// 验证函数体
    /// </summary>
    /// <param name="address"></param>
    /// <param name="val"></param>
    /// <param name="len"></param>
    /// <returns></returns>
    static bool CheckFunc(ref IntPtr address, byte val, int len)
    {
        if (address.ToInt64() > 0)
        {
            if (Marshal.ReadByte(address) == 233)
            {
                if (IntPtr.Size == 4)
                {
                    var pass = Marshal.ReadInt32(new IntPtr(address.ToInt32() + 1));
                    address = new IntPtr(address.ToInt32() + pass + 5);
                }
                else
                {
                    var pass = Marshal.ReadInt64(new IntPtr(address.ToInt64() + 1));
                    address = new IntPtr(address.ToInt64() + pass + 5);
                }
            }

            if (address.ToInt64() > 0 && Marshal.ReadByte(address) == val)
            {
                address = IntPtr.Size == 4
                    ? new IntPtr(address.ToInt32() + len)
                    : new IntPtr(address.ToInt64() + len);
                return true;
            }
        }

        return false;
    }
}
#endif
#endif