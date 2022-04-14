/*
 * 这里必须要实现一次这个接口,才能使用特性
 */
public class AutoGoExtensionApplication : IExtensionApplication
{
    public void Initialize()
    {
        new AutoClass().Initialize();
    }

    public void Terminate()
    {
    }
}

/*
 * 在所有的命令末尾注入清空事务栈函数
 */
public class AutoAOP
{
    [IFoxInitialize]
    public void Initialize()
    {
        AOP.Run(nameof(Test));
    }
}