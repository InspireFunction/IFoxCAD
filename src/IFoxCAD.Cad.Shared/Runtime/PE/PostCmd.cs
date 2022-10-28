namespace IFoxCAD.Cad;

public class PostCmd
{
    /*
     *[DllImport("accore.dll", EntryPoint = "acedCommand")]
     *static extern int AcedCommand(IntPtr vlist);
    */
    delegate int DelegateAcedCommand(IntPtr strExpr);
    static DelegateAcedCommand? _AcedCommand;
    /// <summary>
    /// 发送命令(同步)
    /// </summary>
    public static int AcedCommand(IntPtr strExpr)
    {
        if (_AcedCommand is null)
        {
            string str = "acedCommand";
            _AcedCommand =
                AcadPeInfo.GetDelegate<DelegateAcedCommand>(str, AcadPeEnum.ExeAndCore);
        }
        if (_AcedCommand is null)
            return -1;
        return _AcedCommand.Invoke(strExpr);// 调用方法
    }


    /*
     *[DllImport("acad.exe", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl,
     *    EntryPoint = "?acedPostCommand@@YAHPB_W@Z")]
     *public static extern int AcedPostCommand(string strExpr);
    */
    delegate int DelegateAcedPostCommand(byte[] strExpr);
    static DelegateAcedPostCommand? _AcedPostCommand;
    /// <summary>
    /// 发送命令(同步)
    /// 这个可以在多线程发送
    /// </summary>
    public static int AcedPostCommand(string strExpr)
    {
        if (_AcedPostCommand is null)
        {
            string str = "acedPostCommand";
            _AcedPostCommand =
                AcadPeInfo.GetDelegate<DelegateAcedPostCommand>(str, AcadPeEnum.ExeAndCore);
        }
        // 不然到CAD之后会乱码
        byte[] bytes = Encoding.Unicode.GetBytes(strExpr);

        if (_AcedPostCommand is null)
            return -1;
        return _AcedPostCommand.Invoke(bytes);// 调用方法
    }

    delegate int DelegateAcedInvoke(byte[] strExpr);
    static DelegateAcedInvoke? _AcedInvoke;
    /// <summary>
    /// 发送命令(同步)
    /// </summary>
    public static int AcedInvoke(string strExpr)
    {
        if (_AcedInvoke is null)
        {
            string str = "acedInvoke";
            _AcedInvoke =
                AcadPeInfo.GetDelegate<DelegateAcedInvoke>(str, AcadPeEnum.ExeAndCore);
        }
        // 不然到CAD之后会乱码
        byte[] bytes = Encoding.Unicode.GetBytes(strExpr);

        if (_AcedInvoke is null)
            return -1;
        return _AcedInvoke.Invoke(bytes);// 调用方法
    }


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
    delegate int DelegateAcedCmd(IntPtr rb);
    static DelegateAcedCmd? _AcedCmd;
    /// <summary>
    /// 发送命令(同步)如果2015.+这里报错,那么表示vs需要提权测试
    /// </summary>
    public static int AcedCmd(ResultBuffer args)
    {
        if (Acap.DocumentManager.IsApplicationContext)
            return 0;
        if (_AcedCmd is null)
        {
            string str = "acedCmd";
            if (Acap.Version.Major >= 20)// 2015.+
                str += "S";
            _AcedCmd =
                AcadPeInfo.GetDelegate<DelegateAcedCmd>(str, AcadPeEnum.ExeAndCore);
        }
        if (_AcedCmd is null)
            return -1;
        var reNum = _AcedCmd.Invoke(args.UnmanagedObject);
        if (reNum != 5100)// 5100正确
            throw new ArgumentException("发送命令出错,是否vs权限不足?");
        return reNum;
    }

    /// <summary>
    /// 发送命令(异步)
    /// </summary>
    public static object? ActiveCmd(string str)
    {
        object[] commandArray = { str + "\n" };
#if zcad
        var App = Acap.ZcadApplication;
#else
        var App = Acap.AcadApplication;
#endif
        // activeDocument 加载lisp第二个文档有问题,似乎要切换了才能
        var activeDocument = App.GetType().InvokeMember("ActiveDocument", BindingFlags.GetProperty, null, App, null);
        return activeDocument?.GetType().InvokeMember("SendCommand", BindingFlags.InvokeMethod, null, activeDocument, commandArray);
    }
}