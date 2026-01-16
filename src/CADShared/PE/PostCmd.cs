namespace IFoxCAD.Cad;
/// <summary>
/// 发送命令
/// </summary>
public class PostCmd
{
    /*
     * #if NET35 || NET40
     * [DllImport("acad.exe", EntryPoint = "acedCmd", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
     * #else
     * // cad2015的AcedCmd改为AcedCmdS.参数也改了,不过不影响pe读取,accore.dll是2013版本后才开始改
     * [DllImport("accore.dll", EntryPoint = "acedCmdS", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
     * static extern int AcedCmd(IntPtr rbp, bool forFutureUse = false, IntPtr pForFutureUse = IntPtr.Zero);
     * #endif
     * static extern int AcedCmd(IntPtr rbp);
     * public static int AcedCmd(ResultBuffer args)
     * {
     *     if (Acap.DocumentManager.IsApplicationContext)
     *         return 0;
     *     else
     *         return AcedCmd(args.UnmanagedObject);
     * }
     */
    delegate int DelegateAcedCmd(IntPtr parameter);
    static DelegateAcedCmd? acedCmd;//nameof 别改名称
    /// <summary>
    /// 发送命令(同步)如果2015.+这里报错,那么表示vs需要提权测试
    /// </summary>
    static PromptStatus AcedCmd(ResultBuffer args)
    {
        if (Acaop.DocumentManager.IsApplicationContext)
            return 0;
        if (acedCmd is null)
        {
            var str = nameof(acedCmd);
            if (Acaop.Version.Major >= 20)// 2015.+
                str += "S";

            acedCmd = AcadPeInfo.GetDelegate<DelegateAcedCmd>(
                            str, AcadPeEnum.ExeAndCore);
        }
        if (acedCmd is null)
            return 0;

        var result = (PromptStatus)acedCmd.Invoke(args.UnmanagedObject);
        if (result != PromptStatus.OK)
            throw new ArgumentException("发送命令出错,是否vs权限不足?");
        return result;
    }

    /*
     * [DllImport("accore.dll", EntryPoint = "acedCommand")]
     * static extern int AcedCommand(IntPtr vlist);
     */
    delegate int DelegateAcedCommand(IntPtr parameter);
    static DelegateAcedCommand? acedCommand;//nameof 别改名称
    /// <summary>
    /// 发送命令(同步)
    /// </summary>
    static PromptStatus AcedCommand(IntPtr args)
    {
        acedCommand ??= AcadPeInfo.GetDelegate<DelegateAcedCommand>(
                            nameof(acedCommand), AcadPeEnum.ExeAndCore);
        if (acedCommand is null)
            return PromptStatus.Error;
        return (PromptStatus)acedCommand.Invoke(args);// 调用方法
    }

    /*
     * [DllImport("acad.exe", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl,
     *     EntryPoint = "?acedPostCommand@@YAHPB_W@Z")]
     * public static extern int AcedPostCommand(string strExpr);
     */
    delegate int DelegateAcedPostCommand(byte[] parameter);
    static DelegateAcedPostCommand? acedPostCommand;//nameof 别改名称
    /// <summary>
    /// 发送命令(同步)
    /// 这个可以在多线程发送
    /// </summary>
    static PromptStatus AcedPostCommand(string args)
    {
        acedPostCommand ??= AcadPeInfo.GetDelegate<DelegateAcedPostCommand>(
                                nameof(acedPostCommand), AcadPeEnum.ExeAndCore);

        // 不然到CAD之后会乱码
        var bytes = Encoding.Unicode.GetBytes(args);
        if (acedPostCommand is null)
            return PromptStatus.Error;
        return (PromptStatus)acedPostCommand.Invoke(bytes);// 调用方法
    }

    delegate int DelegateAcedInvoke(byte[] parameter);
    static DelegateAcedInvoke? acedInvoke;//nameof 别改名称
    /// <summary>
    /// 发送命令(同步)
    /// </summary>
    static PromptStatus AcedInvoke(string args)
    {
        acedInvoke ??= AcadPeInfo.GetDelegate<DelegateAcedInvoke>(
                            nameof(acedInvoke), AcadPeEnum.ExeAndCore);

        // 不然到CAD之后会乱码
        var bytes = Encoding.Unicode.GetBytes(args);

        if (acedInvoke is null)
            return PromptStatus.Error;
        return (PromptStatus)acedInvoke.Invoke(bytes);// 调用方法
    }

    /// <summary>
    /// 发送命令(异步)+CommandFlags.Session可以同步发送
    /// </summary>
    static void AsyncCommand(string args)
    {
        object[] commandArray = [args + "\n"];
#if zcad
        var com = Acap.ZcadApplication;
#else
        var com = Acap.AcadApplication;
#endif
        // activeDocument 加载lisp第二个文档有问题,似乎要切换了才能
        var doc = com.GetType()
            .InvokeMember("ActiveDocument", BindingFlags.GetProperty, null, com, null);
        doc?.GetType()
            .InvokeMember("SendCommand", BindingFlags.InvokeMethod, null, doc, commandArray);// 返回值是null
    }
    /// <summary>
    /// 命令模式
    /// </summary>
    public enum RunCmdFlag : byte
    {
        /// <summary>
        /// 发送命令(同步)如果2015.+这里报错,那么表示vs需要提权测试
        /// </summary>
        AcedCmd = 1,
        /// <summary>
        /// 发送命令(同步)
        /// </summary>
        AcedCommand = 2,
        /// <summary>
        /// 发送命令(同步)，可以多线程
        /// </summary>
        AcedPostCommand = 4,
        /// <summary>
        /// 发送命令(同步)
        /// </summary>
        AcedInvoke = 8,
        /// <summary>
        /// 默认的发送命令
        /// </summary>
        SendStringToExecute = 16,
        /// <summary>
        /// 异步命令
        /// </summary>
        AsyncCommand = 32,
    }

    /*
     * 发送命令会记录在命令历史
     * 发送lisp的(command "xx")就不会
     */
     /// <summary>
     /// 发送命令
     /// </summary>
     /// <param name="args"></param>
     /// <returns></returns>
    public static PromptStatus SendCommand(ResultBuffer args)
    {
        return AcedCmd(args);
    }
    /// <summary>
    /// 发送命令
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static PromptStatus SendCommand(IntPtr args)
    {
        return AcedCommand(args);
    }
    /// <summary>
    /// 发送命令
    /// </summary>
    /// <param name="args"></param>
    /// <param name="flag"></param>
    /// <returns></returns>
    public static PromptStatus SendCommand(string args, RunCmdFlag flag)
    {
        var ret = PromptStatus.OK;
        if (!Acaop.DocumentManager.IsApplicationContext)
        {
            if ((flag & RunCmdFlag.AcedCmd) == RunCmdFlag.AcedCmd)
            {
                using ResultBuffer rb = new()
                {
                    new((int)LispDataType.Text, args),
                };
                ret = SendCommand(rb);
            }
            if ((flag & RunCmdFlag.AcedCommand) == RunCmdFlag.AcedCommand)
            {
                // 此处是这样转换吗?
                using ResultBuffer rb = new()
                {
                    new((int)LispDataType.Text, args),
                };
                ret = SendCommand(rb.UnmanagedObject);
            }
            if ((flag & RunCmdFlag.AcedPostCommand) == RunCmdFlag.AcedPostCommand)
            {
                ret = AcedPostCommand(args);
            }
            if ((flag & RunCmdFlag.AcedInvoke) == RunCmdFlag.AcedInvoke)
            {
                ret = AcedInvoke(args);
            }
        }
        else
        {
            var dm = Acaop.DocumentManager;
            var doc = dm.MdiActiveDocument;
            if (doc == null)
                return PromptStatus.Error;

            if ((flag & RunCmdFlag.SendStringToExecute) == RunCmdFlag.SendStringToExecute)
            {
                doc.SendStringToExecute(args, true, false, false);
            }
            if ((flag & RunCmdFlag.AsyncCommand) == RunCmdFlag.AsyncCommand)
            {
                // 此处+CommandFlags.Session可以同步发送,bo命令可以,其他是否可以?
                // 仿人工输入,像lisp一样可以直接发送关键字
                AsyncCommand(args);
            }
        }
        return ret;
    }
}