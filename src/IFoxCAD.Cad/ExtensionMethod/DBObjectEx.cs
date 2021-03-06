namespace IFoxCAD.Cad;

/// <summary>
/// 实体对象扩展类
/// </summary>
public static class DBObjectEx
{
    #region Xdata扩展
    /// <summary>
    /// 删除扩展数据
    /// </summary>
    /// <param name="obj">对象实例</param>
    /// <param name="appName">应用程序名称</param>
    /// <param name="dxfCode">要删除数据的组码</param>
    public static void RemoveXData(this DBObject obj, string appName, DxfCode dxfCode)
    {
        XDataList data = obj.XData;
        var indexlst = new List<int>();
        bool flag = false;
        for (int i = 0; i < data.Count; i++)
        {
            if (data[i].TypeCode == (int)DxfCode.ExtendedDataRegAppName && data[i].Value.ToString() == appName)
            {
                flag = true;
            }
            if (flag)
            {
                if (data[i].TypeCode == (int)DxfCode.ExtendedDataRegAppName && data[i].Value.ToString() != appName)
                    break;
                if (data[i].TypeCode == (int)dxfCode)
                {
                    indexlst.Add(i);
                }
            }
        }
        foreach (var item in indexlst)
        {
            data.RemoveAt(item);
        }

        using (obj.ForWrite())
        {
            obj.XData = data;
        }
    }
    /// <summary>
    /// 修改扩展数据
    /// </summary>
    /// <param name="obj">对象实例</param>
    /// <param name="appName">应用程序名称</param>
    /// <param name="dxfCode">要修改数据的组码</param>
    /// <param name="newvalue">新的数据</param>
    public static void ChangeXData(this DBObject obj, string appName, DxfCode dxfCode, object newvalue)
    {
        XDataList data = obj.XData;
        bool flag = false;
        for (int i = 0; i < data.Count; i++)
        {
            if (data[i].TypeCode == (int)DxfCode.ExtendedDataRegAppName && data[i].Value.ToString() == appName)
            {
                flag = true;
            }
            if (flag)
            {
                if (data[i].TypeCode == (int)DxfCode.ExtendedDataRegAppName && data[i].Value.ToString() != appName)
                    break;
                if (data[i].TypeCode == (int)dxfCode)
                {
                    data[i] = new TypedValue((int)dxfCode, newvalue);
                }
            }
        }

        using (obj.ForWrite())
        {
            obj.XData = data;
        }
    }
    #endregion

    #region 读写模式切换

    /// <summary>
    /// 实体自动管理读写函数
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="obj">实体对象</param>
    /// <param name="action">操作委托</param>
    public static void ForWrite<T>(this T obj, Action<T> action) where T : DBObject
    {
        var _isNotifyEnabled = obj.IsNotifyEnabled;
        var _isWriteEnabled = obj.IsWriteEnabled;
        if (_isNotifyEnabled)
        {
            obj.UpgradeFromNotify();
        }
        else if (!_isWriteEnabled)
        {
            obj.UpgradeOpen();
        }
        action?.Invoke(obj);
        if (_isNotifyEnabled)
        {
            obj.DowngradeToNotify(_isWriteEnabled);
        }
        else if (!_isWriteEnabled)
        {
            obj.DowngradeOpen();
        }
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
