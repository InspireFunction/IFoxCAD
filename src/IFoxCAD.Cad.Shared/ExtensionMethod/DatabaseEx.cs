namespace IFoxCAD.Cad;

public static class DatabaseEx
{
    /// <summary>
    /// 后台开图文字偏移处理
    /// <para>
    /// 0x01 此方案利用前台数据库进行处理<br/>
    /// 0x02 当关闭所有前台文档时,会出现无<see cref="HostApplicationServices.WorkingDatabase"/>时,不能使用(惊惊没有测试过此状态)<br/>
    /// 0x03 当关闭所有前台文档时,如何发送命令呢?那就是利用跨进程通讯
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