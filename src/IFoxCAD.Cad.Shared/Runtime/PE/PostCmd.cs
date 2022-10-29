namespace IFoxCAD.Cad;

public class PostCmd
{
    /*
#if NET35 || NET40
[DllImport("acad.exe", EntryPoint = "acedCmd", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
#else
// cad2015的AcedCmd改为AcedCmdS.参数也改了,不过不影响pe读取,accore.dll是2013版本后才开始改
[DllImport("accore.dll", EntryPoint = "acedCmdS", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
static extern int AcedCmd(IntPtr rbp, bool forFutureUse = false, IntPtr pForFutureUse = IntPtr.Zero);
#endif
static extern int AcedCmd(IntPtr rbp);
public static int AcedCmd(ResultBuffer args)
{
    if (Acap.DocumentManager.IsApplicationContext)
        return 0;
    else
        return AcedCmd(args.UnmanagedObject);
}
 */
    delegate int DelegateAcedCmd(IntPtr parameter);
    static DelegateAcedCmd? acedCmd;//nameof 别改名称
    /// <summary>
    /// 发送命令(同步)如果2015.+这里报错,那么表示vs需要提权测试
    /// </summary>
    public static int AcedCmd(ResultBuffer args)
    {
        if (Acap.DocumentManager.IsApplicationContext)
            return 0;
        if (acedCmd is null)
        {
            string str = nameof(acedCmd);
            if (Acap.Version.Major >= 20)// 2015.+
                str += "S";

            acedCmd = AcadPeInfo.GetDelegate<DelegateAcedCmd>(
                            str, AcadPeEnum.ExeAndCore);
        }
        if (acedCmd is null)
            return 0;

        var result = acedCmd.Invoke(args.UnmanagedObject);
        if (result != (int)PromptStatus.OK)
            throw new ArgumentException("发送命令出错,是否vs权限不足?");
        return result;
    }

    /*
     *[DllImport("accore.dll", EntryPoint = "acedCommand")]
     *static extern int AcedCommand(IntPtr vlist);
    */
    delegate int DelegateAcedCommand(IntPtr parameter);
    static DelegateAcedCommand? acedCommand;//nameof 别改名称
    /// <summary>
    /// 发送命令(同步)
    /// </summary>
    public static int AcedCommand(IntPtr args)
    {
        acedCommand ??= AcadPeInfo.GetDelegate<DelegateAcedCommand>(
                            nameof(acedCommand), AcadPeEnum.ExeAndCore);
        if (acedCommand is null)
            return 0;
        return acedCommand.Invoke(args);// 调用方法
    }

    /*
     *[DllImport("acad.exe", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl,
     *    EntryPoint = "?acedPostCommand@@YAHPB_W@Z")]
     *public static extern int AcedPostCommand(string strExpr);
    */
    delegate int DelegateAcedPostCommand(byte[] parameter);
    static DelegateAcedPostCommand? acedPostCommand;//nameof 别改名称
    /// <summary>
    /// 发送命令(同步)
    /// 这个可以在多线程发送
    /// </summary>
    public static int AcedPostCommand(string args)
    {
        acedPostCommand ??= AcadPeInfo.GetDelegate<DelegateAcedPostCommand>(
                                nameof(acedPostCommand), AcadPeEnum.ExeAndCore);

        // 不然到CAD之后会乱码
        byte[] bytes = Encoding.Unicode.GetBytes(args);
        if (acedPostCommand is null)
            return 0;
        return acedPostCommand.Invoke(bytes);// 调用方法
    }

    delegate int DelegateAcedInvoke(byte[] parameter);
    static DelegateAcedInvoke? acedInvoke;//nameof 别改名称
    /// <summary>
    /// 发送命令(同步)
    /// </summary>
    public static int AcedInvoke(string args)
    {
        acedInvoke ??= AcadPeInfo.GetDelegate<DelegateAcedInvoke>(
                            nameof(acedInvoke), AcadPeEnum.ExeAndCore);

        // 不然到CAD之后会乱码
        byte[] bytes = Encoding.Unicode.GetBytes(args);

        if (acedInvoke is null)
            return 0;
        return acedInvoke.Invoke(bytes);// 调用方法
    }

    /// <summary>
    /// 发送命令(异步)+CommandFlags.Session可以同步发送
    /// </summary>
    public static object? AsyncCommand(string args)
    {
        object[] commandArray = { args + "\n" };
#if zcad
        var App = Acap.ZcadApplication;
#else
        var App = Acap.AcadApplication;
#endif
        // activeDocument 加载lisp第二个文档有问题,似乎要切换了才能
        var doc = App.GetType().InvokeMember("ActiveDocument", BindingFlags.GetProperty, null, App, null);
        return doc?.GetType().InvokeMember("SendCommand", BindingFlags.InvokeMethod, null, doc, commandArray);
    }

    /*
     * 发送命令会记录在命令历史
     * 发送lisp的(command "xx")就不会
     */
    public static bool SendCommand(string args)
    {
        // cmd 需要加\n吗?
        if (!Acap.DocumentManager.IsApplicationContext)
        {
            int ret;
            // ret = AcedCmd(ResultBuffer);
            // ret = AcedCommand(IntPtr)
            // ret = AcedPostCommand(args);
            ret = AcedInvoke(args);

            //不太确定所有的发送 返回值int 是不是 PromptStatus
            // if (ret == (int)PromptStatus.OK)
            //     return true;
            if (ret != 0)
                return true;
        }
        else
        {
            var dm = Acap.DocumentManager;
            var doc = dm.MdiActiveDocument;
            if (doc != null)
            {
                // 此处+CommandFlags.Session可以同步发送,bo命令可以,其他是否可以?
                // AsyncCommand()异步com的
                doc.SendStringToExecute(args, true, false, false);
                return true;
            }
        }
        return false;
    }
}