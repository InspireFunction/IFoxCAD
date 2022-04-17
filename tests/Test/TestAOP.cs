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
    /* 
     * 天秀的事务注入,让你告别事务处理
     * https://www.cnblogs.com/JJBox/p/16157578.html
     */
    public class AopTestClass
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
            //怎么用事务呢?
            //直接用 DBTrans.Top
        }
    }

    //拒绝注入事务,写类上,则方法全都拒绝
    [IFoxRefuseInjectionTransaction]
    public class AopTestClassRefuseInjection
    {
        //此时这个也是拒绝的..这里加特性只是无所谓
        [IFoxRefuseInjectionTransaction]
        [CommandMethod("IFoxRefuseInjectionTransaction2")]
        public void TestIFoxRefuseInjectionTransaction()
        {
            //拒绝注入就要自己开事务,通常用在循环提交事务上面.
            //另见 报错0x02 https://www.cnblogs.com/JJBox/p/10798940.html
            using var tr = new DBTrans();
        }

        [CommandMethod("InjectionTransaction2")]
        public void InjectionTransaction()
        {
        }
    }
}