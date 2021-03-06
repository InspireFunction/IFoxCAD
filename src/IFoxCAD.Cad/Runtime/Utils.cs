using System;

namespace IFoxCAD.Cad;

public class Helper
{
    /*
     * id = db.GetObjectId(false, handle, 0);
     * 参数意义: db.GetObjectId(如果没有找到就创建,句柄号,标记..将来备用)
     * 在vs的输出会一直抛出:
     * 引发的异常:“Autodesk.AutoCAD.Runtime.Exception”(位于 AcdbMgd.dll 中)
     * "eUnknownHandle"
     * 这就是为什么慢的原因,所以直接运行就好了!而Debug还是需要用arx的API替代.
     */

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("acdb17.dll", CallingConvention = CallingConvention.ThisCall/*08的调用约定 高版本是__cdecl*/,
       EntryPoint = "?getAcDbObjectId@AcDbDatabase@@QAE?AW4ErrorStatus@Acad@@AAVAcDbObjectId@@_NABVAcDbHandle@@K@Z")]
    extern static int getAcDbObjectId17x32(IntPtr db, out ObjectId id, [MarshalAs(UnmanagedType.U1)] bool createnew, ref Handle h, uint reserved);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("acdb17.dll", CallingConvention = CallingConvention.ThisCall/*08的调用约定 高版本是__cdecl*/,
      EntryPoint = "?getAcDbObjectId@AcDbDatabase@@QEAA?AW4ErrorStatus@Acad@@AEAVAcDbObjectId@@_NAEBVAcDbHandle@@K@Z")]
    extern static int getAcDbObjectId17x64(IntPtr db, out ObjectId id, [MarshalAs(UnmanagedType.U1)] bool createnew, ref Handle h, uint reserved);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("acdb18.dll", CallingConvention = CallingConvention.ThisCall/*08的调用约定 高版本是__cdecl*/,
       EntryPoint = "?getAcDbObjectId@AcDbDatabase@@QAE?AW4ErrorStatus@Acad@@AAVAcDbObjectId@@_NABVAcDbHandle@@K@Z")]
    extern static int getAcDbObjectId18x32(IntPtr db, out ObjectId id, [MarshalAs(UnmanagedType.U1)] bool createnew, ref Handle h, uint reserved);

    [System.Security.SuppressUnmanagedCodeSecurity]
    [DllImport("acdb18.dll", CallingConvention = CallingConvention.ThisCall/*08的调用约定 高版本是__cdecl*/,
      EntryPoint = "?getAcDbObjectId@AcDbDatabase@@QEAA?AW4ErrorStatus@Acad@@AEAVAcDbObjectId@@_NAEBVAcDbHandle@@K@Z")]
    extern static int getAcDbObjectId18x64(IntPtr db, out ObjectId id, [MarshalAs(UnmanagedType.U1)] bool createnew, ref Handle h, uint reserved);

    /// <summary>
    /// 句柄转id,NET35(08~12)专用的
    /// </summary>
    /// <param name="db">数据库</param>
    /// <param name="handle">句柄</param>
    /// <param name="id">返回的id</param>
    /// <param name="createIfNotFound">不存在则创建</param>
    /// <param name="reserved">保留,用于未来</param>
    /// <returns>成功0,其他值都是错误.可以强转ErrorStatus</returns>
    static int GetAcDbObjectId(IntPtr db, Handle handle, out ObjectId id, bool createIfNotFound = false, uint reserved = 0)
    {
        id = ObjectId.Null;
        switch (Application.Version.Major)
        {
            case 17:
                {
                    if (IntPtr.Size == 4)
                        return getAcDbObjectId17x32(db, out id, createIfNotFound, ref handle, reserved);
                    else
                        return getAcDbObjectId17x64(db, out id, createIfNotFound, ref handle, reserved);
                }
            case 18:
                {
                    if (IntPtr.Size == 4)
                        return getAcDbObjectId18x32(db, out id, createIfNotFound, ref handle, reserved);
                    else
                        return getAcDbObjectId18x64(db, out id, createIfNotFound, ref handle, reserved);
                }
        }
        return -1;
    }

    /// <summary>
    /// 句柄转id
    /// </summary>
    /// <param name="db">数据库</param>
    /// <param name="handle">句柄</param>
    /// <returns>id</returns>
    public static ObjectId TryGetObjectId(Database db, Handle handle)
    {
#if !NET35
        //高版本直接利用
        var es = db.TryGetObjectId(handle, out ObjectId id);
        //if (!es)
#else
        var es = GetAcDbObjectId(db.UnmanagedObject, handle, out ObjectId id);
        //if (ErrorStatus.OK != (ErrorStatus)es)
#endif
        return id;
    }

    //public static int GetCadFileVersion(string filename)
    //{
    //    var bytes = File.ReadAllBytes(filename);
    //    var headstr = Encoding.Default.GetString(bytes)[0..6];
    //    if (!headstr.StartsWith("AC")) return 0;
    //    var vernum = int.Parse(headstr.Replace("AC", ""));
    //    var a = Enum.Parse(typeof(DwgVersion), "AC1800");
    //    Enum.TryParse()
    //    return vernum + 986;
        
    //}
}

internal static class MethodInfoHelper
{
    private static readonly Dictionary<MethodInfo, object> methodDic = new();

    /// <summary>
    /// 执行函数
    /// </summary>
    /// <param name="methodInfo">函数</param>
    /// <param name="instance">已经外部创建的对象,为空则此处创建</param>
    public static object? Invoke(this MethodInfo methodInfo, object? instance = null)
    {
        if (methodInfo == null)
            throw new ArgumentNullException(nameof(methodInfo));

        object? result = null;
        if (methodInfo.IsStatic)
        {
            //新函数指针进入此处
            //参数数量一定要匹配,为null则参数个数不同导致报错,
            //参数为stirng[],则可以传入object[]代替,其他参数是否还可以实现默认构造?
            var paramInfos = methodInfo.GetParameters();
            var args = new List<object> { };
            for (int i = 0; i < paramInfos.Length; i++)
                args.Add(null!);
            result = methodInfo.Invoke(null, args.ToArray());//静态调用
        }
        else
        {
            //原命令的函数指针进入此处
            //object instance;
            if (methodDic.ContainsKey(methodInfo))
                instance = methodDic[methodInfo];

            if (instance == null)
            {
                var reftype = methodInfo.ReflectedType;
                if (reftype == null) return null;
                var fullName = reftype.FullName; //命名空间+类
                if (fullName == null) return null;
                var type = reftype.Assembly.GetType(fullName);//通过程序集反射创建类+
                if (type == null) return null;
                instance = Activator.CreateInstance(type);
                if (!type.IsAbstract)//无法创建抽象类成员
                    methodDic.Add(methodInfo, instance);
            }
            if (instance != null)
                result = methodInfo.Invoke(instance, null); //非静态,调用实例化方法
        }
        return result;
    }
}