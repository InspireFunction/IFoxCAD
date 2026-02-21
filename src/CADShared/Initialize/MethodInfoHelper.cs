#if !NET8_0_OR_GREATER
using ArgumentNullException = IFoxCAD.Basal.ArgumentNullEx;
#endif

namespace IFoxCAD.Cad;

internal static class MethodInfoHelper
{
    /// <summary>
    /// 执行函数
    /// </summary>
    /// <param name="methodInfo">函数</param>
    /// <param name="instance">已经外部创建的对象,为空则此处创建</param>
    public static object? Invoke(this MethodInfo methodInfo, ref object? instance)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);

        object? result = null;
        if (methodInfo.IsStatic)
        {
            var args = new List<object>();
            var paramInfos = methodInfo.GetParameters();
            for (var i = 0; i < paramInfos.Length; i++)
                args.Add(null!);
            result = methodInfo.Invoke(null, args.ToArray());
        }
        else
        {
            if (instance == null)
            {
                var refType = methodInfo.ReflectedType;
                if (refType == null)
                    return null;

                var fullName = refType.FullName;
                if (fullName == null)
                    return null;

                var type = refType.Assembly.GetType(fullName);
                if (type == null)
                    return null;

                instance = Activator.CreateInstance(type);
            }
            if (instance != null)
                result = methodInfo.Invoke(instance, null);
        }
        return result;
    }
}
