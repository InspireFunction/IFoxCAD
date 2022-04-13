namespace IFoxCAD.Cad;

public static class MethodInfoHelper
{
    /// <summary>
    /// 执行函数
    /// </summary>
    /// <param name="methodInfo">函数</param>
    /// <param name="instance">已经外部创建的对象,为空则此处创建</param>
    public static object? Invoke(this MethodInfo methodInfo, object? instance = null)
    {
        if (methodInfo == null)
            throw new ArgumentNullException(nameof(methodInfo));

        object? result;
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
            if (instance == null)
            {
                var reftype = methodInfo.ReflectedType;
                if (reftype == null) return null;
                var fullName = reftype.FullName; //命名空间+类
                if (fullName == null) return null;
                var type = reftype.Assembly.GetType(fullName);//通过程序集反射创建类+
                if (type == null) return null;
                instance = Activator.CreateInstance(type);
            }
            result = methodInfo.Invoke(instance, null); //非静态,调用实例化方法
        }
        return result;
    }
}
