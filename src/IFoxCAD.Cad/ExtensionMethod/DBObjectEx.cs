using System.Linq;

namespace IFoxCAD.Cad;

/// <summary>
/// 实体对象扩展类
/// </summary>
public static class DBObjectEx
{
    #region Xdata扩展

    /// <summary>
    /// 获取appName的索引区间
    /// </summary>
    /// <param name="data">xdata</param>
    /// <param name="appName">注册名称</param>
    /// <param name="dxfCodes">任务组码对象</param>
    /// <returns>返回任务组码的索引</returns>
    static List<int> GetXdataAppIndex(XDataList data, string appName, DxfCode[] dxfCodes)
    {
        List<int> acIndex = new();
        int appNameIndex = -1;
        // int appNameIndexNext = -1;

        // 先找到属于它的名字索引,然后再找到下一个不属于它名字的索引,移除中间部分
        for (int i = 0; i < data.Count; i++)
        {
            if (data[i].TypeCode == (short)DxfCode.ExtendedDataRegAppName)
            {
                if (data[i].Value.ToString() == appName)
                {
                    appNameIndex = i;
                    continue;
                }
                if (appNameIndex != -1)
                {
                    // 找到了后面的appName
                    // appNameIndexNext = i;
                    break;
                }
            }
            if (appNameIndex != -1 && // 找next的时候,获取任务(移除)的对象
                dxfCodes.Contains((DxfCode)data[i].TypeCode))
                acIndex.Add(i);
        }

        // 当前app索引
        // if (appNameIndex == -1)
        // return;

        // 下一个app索引,如果是空,就为末尾
        // if (appNameIndexNext == -1)
        //    appNameIndexNext = data.Count;

        return acIndex;
    }

    /// <summary>
    /// 删除扩展数据
    /// </summary>
    /// <param name="obj">对象实例</param>
    /// <param name="appName">应用程序名称</param>
    /// <param name="dxfCode">要删除数据的组码</param>
    public static void RemoveXData(this DBObject obj, string appName, DxfCode dxfCode)
    {
        if (obj.XData == null)
            return;
        XDataList data = obj.XData;

        // 测试命令 addxdata removexdata
        // 移除指定App的扩展
        var indexs = GetXdataAppIndex(data, appName, new DxfCode[] { dxfCode });
        if (indexs.Count == 0)
            return;

        for (int i = indexs.Count - 1; i >= 0; i--)
            data.RemoveAt(indexs[i]);

        using (obj.ForWrite())
            obj.XData = data;
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
        if (obj.XData == null)
            return;
        XDataList data = obj.XData;

        var indexs = GetXdataAppIndex(data, appName, new DxfCode[] { dxfCode });
        if (indexs.Count == 0)
            return;

        for (int i = indexs.Count - 1; i >= 0; i--)
            data[i] = new TypedValue((short)dxfCode, newvalue);

        using (obj.ForWrite())
            obj.XData = data;
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
            obj.UpgradeFromNotify();
        else if (!_isWriteEnabled)
            obj.UpgradeOpen();

        action?.Invoke(obj);

        if (_isNotifyEnabled)
            obj.DowngradeToNotify(_isWriteEnabled);
        else if (!_isWriteEnabled)
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
