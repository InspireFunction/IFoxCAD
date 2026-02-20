namespace IFoxCAD.Cad;

/// <summary>
/// 实体对象扩展类
/// </summary>
public static class DBObjectEx
{
    #region Linq

    /// <summary>
    /// 删除数据库对象
    /// </summary>
    /// <param name="dBObjects">数据库对象列表</param>
    public static void Erase(this IEnumerable<DBObject> dBObjects)
    {
        foreach (var dbo in dBObjects)
        {
            if (dbo.IsNewObject || dbo.IsErased)
                continue;
            using (dbo.ForWrite())
            {
                dbo.Erase();
            }
        }
    }

    #endregion

    #region Xdata扩展

    /// <summary>
    /// 删除扩展数据
    /// </summary>
    /// <param name="obj">对象实例</param>
    /// <param name="appName">应用程序名称</param>
    /// <param name="dxfCode">要删除数据的组码</param>
    public static void RemoveXData(this DBObject obj, string appName, DxfCode dxfCode)
    {
        if (obj.XData is null)
            return;
        XDataList data = obj.XData;

        // 移除指定App的扩展
        var indexes = data.GetXdataAppIndex(appName, [dxfCode]);
        if (indexes.Count == 0)
            return;

        for (var i = indexes.Count - 1; i >= 0; i--)
            data.RemoveAt(indexes[i]);

        using (obj.ForWrite())
            obj.XData = data;
    }

    /// <summary>
    /// 删除扩展数据
    /// </summary>
    /// <param name="obj">对象实例</param>
    /// <param name="appName">应用程序名称</param>
    public static void RemoveXData(this DBObject obj, string appName)
    {
        if (obj.GetXDataForApplication(appName) is null)
            return;

        // 直接赋值进去等于清空名称
        using (obj.ForWrite())
            obj.XData = new XDataList { { 1001, appName } };
    }

    /// <summary>
    /// 克隆对象
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="obj">对象</param>
    /// <returns>克隆后的对象</returns>
    /// <exception cref="ArgumentException"></exception>
    public static T CloneEx<T>(this T obj) where T : ICloneable
    {
        return obj.Clone() is T tObj ? tObj : throw new ArgumentException(nameof(CloneEx) + "克隆出错");
    }

    /// <summary>
    /// 修改扩展数据
    /// </summary>
    /// <param name="obj">对象实例</param>
    /// <param name="appName">应用程序名称</param>
    /// <param name="dxfCode">要修改数据的组码</param>
    /// <param name="newValue">新的数据</param>
    public static void ChangeXData(this DBObject obj, string appName, DxfCode dxfCode,
        object newValue)
    {
        if (obj.XData is null)
            return;
        XDataList data = obj.XData;

        var indexes = data.GetXdataAppIndex(appName, [dxfCode]);
        if (indexes.Count == 0)
            return;

        for (var i = indexes.Count - 1; i >= 0; i--)
            data[indexes[i]] = new TypedValue((short)dxfCode, newValue);

        using (obj.ForWrite())
            obj.XData = data;
    }

    #endregion

    #region 读写模式切换

    /// <summary>
    /// 实体自动管理读写函数，此函数性能比using模式低一倍
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="obj">实体对象</param>
    /// <param name="action">操作委托</param>
    public static void ForWrite<T>(this T obj, Action<T> action) where T : DBObject
    {
        var isNotifyEnabled = obj.IsNotifyEnabled;
        var isWriteEnabled = obj.IsWriteEnabled;
        if (isNotifyEnabled)
            obj.UpgradeFromNotify();
        else if (!isWriteEnabled)
            obj.UpgradeOpen();

        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        action?.Invoke(obj);

        if (isNotifyEnabled)
            obj.DowngradeToNotify(isWriteEnabled);
        else if (!isWriteEnabled)
            obj.DowngradeOpen();
    }

    /// <summary>
    /// 打开模式提权
    /// </summary>
    /// <param name="obj">实体对象</param>
    /// <returns>提权类对象</returns>
    public static UpgradeOpenManager ForWrite(this DBObject obj)
    {
        return new UpgradeOpenManager(obj);
    }

    /// <summary>
    /// 提权类
    /// </summary>
    public class UpgradeOpenManager : IDisposable
    {
        private readonly DBObject _obj;
        private readonly bool _isNotifyEnabled;
        private readonly bool _isWriteEnabled;

        internal UpgradeOpenManager(DBObject obj)
        {
            _obj = obj;
            _isNotifyEnabled = _obj.IsNotifyEnabled;
            _isWriteEnabled = _obj.IsWriteEnabled;
            if (_isNotifyEnabled)
                _obj.UpgradeFromNotify();
            else if (!_isWriteEnabled)
                _obj.UpgradeOpen();
        }

        #region IDisposable 成员

        /// <summary>
        /// 注销函数
        /// </summary>
        public void Dispose()
        {
            if (_isNotifyEnabled)
                _obj.DowngradeToNotify(_isWriteEnabled);
            else if (!_isWriteEnabled)
                _obj.DowngradeOpen();
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable 成员
    }

    #endregion
}