#if !NET8_0_OR_GREATER
using ArgumentNullException = IFoxCAD.Basal.ArgumentNullEx;
#endif

namespace IFoxCAD.Cad;

/// <summary>
/// MethodInfo扩展方法
/// </summary>
internal static class MethodInfoHelper
{
    /// <summary>
    /// 执行函数
    /// </summary>
    /// <param name="methodInfo">函数</param>
    /// <param name="instance">已经外部创建的对象,为空则此处创建</param>
    /// <returns>方法执行结果</returns>
    public static object? Invoke(this MethodInfo methodInfo, ref object? instance)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);

        object? result = null;
        if (methodInfo.IsStatic)
        {
            // 构造参数数组
            var args = CreateDefaultArgs(methodInfo);
            result = methodInfo.Invoke(null, args);
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
            {
                // 构造参数数组
                var args = CreateDefaultArgs(methodInfo);
                result = methodInfo.Invoke(instance, args);
            }
        }
        return result;
    }

    /// <summary>
    /// 为方法构造默认参数数组
    /// </summary>
    /// <param name="methodInfo">方法信息</param>
    /// <returns>参数数组</returns>
    private static object?[] CreateDefaultArgs(MethodInfo methodInfo)
    {
        var paramInfos = methodInfo.GetParameters();
        if (paramInfos.Length == 0)
            return [];

        var args = new object?[paramInfos.Length];
        for (var i = 0; i < paramInfos.Length; i++)
        {
            args[i] = GetDefaultParameterValue(paramInfos[i]);
        }
        return args;
    }

    /// <summary>
    /// 获取参数的默认值
    /// </summary>
    /// <param name="paramInfo">参数信息</param>
    /// <returns>默认值</returns>
    private static object? GetDefaultParameterValue(ParameterInfo paramInfo)
    {
        var paramType = paramInfo.ParameterType;

        // 处理可空类型
        var underlyingType = Nullable.GetUnderlyingType(paramType);
        if (underlyingType != null)
            return null;

        // 值类型,使用默认值
        if (paramType.IsValueType)
            return Activator.CreateInstance(paramType);

        // 引用类型,返回null
        return null;
    }
}
