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


//被注入的函数将不能使用断点,
//因此用户要充分了解才能使用
#if false
/*
 * 类库用户想侵入的命名空间是用户的,
 * 所以需要用户手动进行AOP.Run(),
 * 默认情况不侵入用户的命令,必须用户手动启用此功能;
 * 启动执行策略之后,侵入命名空间下的命令,
 * 此时有拒绝特性的策略保证括免,因为用户肯定是想少写一个事务注入的特性;
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
        public void IFoxRefuseInjectionTransaction()
        {
        }

        //不拒绝
        [CommandMethod("InjectionTransaction")]
        public void InjectionTransaction()
        {
            //怎么用事务呢?
            //直接用 DBTrans.Top
            var dBTrans = new DBTrans();
            dBTrans.Commit();
        }
    }

    //拒绝注入事务,写类上,则方法全都拒绝
    [IFoxRefuseInjectionTransaction]
    public class AopTestClassRefuseInjection
    {
        //此时这个也是拒绝的..这里加特性只是无所谓
        [IFoxRefuseInjectionTransaction]
        [CommandMethod("IFoxRefuseInjectionTransaction2")]
        public void IFoxRefuseInjectionTransaction2()
        {
            //拒绝注入就要自己开事务,通常用在循环提交事务上面.
            //另见 报错0x02 https://www.cnblogs.com/JJBox/p/10798940.html
            using var tr = new DBTrans();
        }

        [CommandMethod("InjectionTransaction2")]
        public void InjectionTransaction2()
        {
        }
    }
} 
#endif