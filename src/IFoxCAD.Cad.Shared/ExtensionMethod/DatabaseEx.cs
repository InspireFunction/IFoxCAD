namespace IFoxCAD.Cad;

public static class DatabaseEx
{
    /// <summary>
    /// 后台开图文字偏移处理
    /// <para>
    /// 0x01 此方案利用前台数据库进行处理<br/>
    /// 0x02 当关闭所有前台文档时,会出现无<see cref="HostApplicationServices.WorkingDatabase"/>时,应该不能使用(惊惊没有测试过此状态)<br/>
    /// 测试条件是:当关闭所有前台文档时,那么如何发送命令呢?那就是利用跨进程通讯<br/>
    /// 0x03 此问题主要出现是<see cref="Database.ResolveXrefs"/>这个线性引擎上面,在参照/深度克隆的底层共用此技术,导致单行文字偏移<br/>
    /// 0x04 异常: 前台绑定的时候不能用它,否则出现: <see langword="eWasErased"/><br/>
    /// </para>
    /// </summary>
    /// <param name="backstageOpenDwg">后台打开的数据库</param>
    /// <param name="action">处理后台的任务</param>
    public static void DBTextDeviation(this Database backstageOpenDwg, Action action)
    {
        var wdb = HostApplicationServices.WorkingDatabase;
        if (wdb != null)
        {
            HostApplicationServices.WorkingDatabase = backstageOpenDwg;
            action?.Invoke();
            HostApplicationServices.WorkingDatabase = wdb;
        }
    }
}