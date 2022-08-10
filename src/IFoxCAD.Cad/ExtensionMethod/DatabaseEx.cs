public static class DatabaseEx
{
    /// <summary>
    /// 后台开图文字偏移处理
    /// <para>0x01 此方案利用前台数据库进行处理</para>
    /// <para>0x02 无<see cref="HostApplicationServices.WorkingDatabase"/>时,不能使用</para>
    /// <para>0x03 例如写了一个exe发送后台处理代码</para>
    /// </summary>
    /// <param name="backstageOpenDwg">后台打开的数据库</param>
    /// <param name="action">处理后台的任务</param>
    public static void DBTextDeviation(this Database backstageOpenDwg, Action action)
    {
        var wdb = HostApplicationServices.WorkingDatabase;
        HostApplicationServices.WorkingDatabase = backstageOpenDwg;
        action?.Invoke();
        HostApplicationServices.WorkingDatabase = wdb;
    }
}