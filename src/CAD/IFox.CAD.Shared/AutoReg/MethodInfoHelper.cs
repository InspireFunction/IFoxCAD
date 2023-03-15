

namespace IFoxCAD.Cad;

internal static class MethodInfoHelper
{
#if cache
    private static readonly Dictionary<MethodInfo, object> methodDic = new();
#endif

    /// <summary>
    /// 执行函数
    /// </summary>
    /// <param name="methodInfo">函数</param>
    /// <param name="instance">已经外部创建的对象,为空则此处创建</param>
    public static object? Invoke(this MethodInfo methodInfo, ref object? instance)
    {
        methodInfo.NotNull(nameof(methodInfo));
        object? result = null;
        if (methodInfo.IsStatic)
        {
            // 新函数指针进入此处
            // 参数数量一定要匹配,为null则参数个数不同导致报错,
            // 参数为stirng[],则可以传入object[]代替,其他参数是否还可以实现默认构造?
            var args = new List<object> { };
            var paramInfos = methodInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
                args.Add(null!);
            result = methodInfo.Invoke(null, args.ToArray());// 静态调用
        }
        else
        {
#if cache
            // 原命令的函数指针进入此处
            // object instance;
            if (methodDic.ContainsKey(methodInfo))
                instance = methodDic[methodInfo];
#endif
            if (instance == null)
            {
                var reftype = methodInfo.ReflectedType;
                if (reftype == null)
                    return null;

                var fullName = reftype.FullName; // 命名空间+类
                if (fullName == null)
                    return null;

                var type = reftype.Assembly.GetType(fullName);
                if (type == null)
                    return null;

                instance = Activator.CreateInstance(type);// 构造类
#if cache
                if (!type.IsAbstract)// 无法创建抽象类成员
                    methodDic.Add(methodInfo, instance);
#endif
            }
            if (instance != null)
                result = methodInfo.Invoke(instance, null); // 非静态,调用实例化方法
        }
        return result;
    }
}