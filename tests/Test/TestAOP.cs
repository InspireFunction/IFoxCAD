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


namespace Test
{
    //拒绝注入事务,写类上,则方法全都拒绝
    [IFoxRefuseInjectionTransaction]
    public class MyClass2
    {
        //此时这个也是拒绝的..这里加特性只是无所谓
        [IFoxRefuseInjectionTransaction]
        [CommandMethod("IFoxRefuseInjectionTransaction2")]
        public void TestIFoxRefuseInjectionTransaction()
        {
        }

        [CommandMethod("InjectionTransaction2")]
        public void InjectionTransaction()
        {
        }
    }

    public class MyClass
    {
        //类不拒绝,这里拒绝
        [IFoxRefuseInjectionTransaction]
        [CommandMethod("IFoxRefuseInjectionTransaction")]
        public void TestIFoxRefuseInjectionTransaction()
        {
        }

        //不拒绝
        [CommandMethod("InjectionTransaction")]
        public void InjectionTransaction()
        {
        }
    }
}