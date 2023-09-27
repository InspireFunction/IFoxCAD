namespace IFoxCAD.Cad;

/// <summary>
/// 实体对象扩展类
/// </summary>
public static class DBObjectEx
{
    /// <summary>
    /// 获取块的有效名字
    /// </summary>
    /// <param name="blk">块参照</param>
    /// <returns>名字</returns>
    public static string GetBlockName(this BlockReference blk)
    {
        ArgumentNullEx.ThrowIfNull(blk);
        if (blk.IsDynamicBlock)
        {
            var btrid = blk.DynamicBlockTableRecord;
            var tr = btrid.Database.TransactionManager.TopTransaction;
            ArgumentNullEx.ThrowIfNull(tr);
            var btr = (BlockTableRecord)tr.GetObject(btrid);
            return btr.Name;
        }
        return blk.Name;
    }

    #region Xdata扩展
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
        var indexs = data.GetXdataAppIndex(appName, new DxfCode[] { dxfCode });
        if (indexs.Count == 0)
            return;

        for (int i = indexs.Count - 1; i >= 0; i--)
            data.RemoveAt(indexs[i]);

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
        if (obj.XData == null)
            return;
        foreach (var data in obj.XData)
        {
            // 直接赋值进去等于清空名称
            using var rb = new ResultBuffer();
            rb.Add(new((int)DxfCode.ExtendedDataRegAppName, appName));
            using (obj.ForWrite())
                obj.XData = rb;
        }
    }
    /// <summary>
    /// 克隆对象
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="ent">对象</param>
    /// <returns>克隆后的对象</returns>
    /// <exception cref="ArgumentException"></exception>
    public static T CloneEx<T>(this T ent) where T : RXObject
    {
        return ent.Clone() is T tEnt ? tEnt : throw new ArgumentException(nameof(CloneEx) + "克隆出错");
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

        var indexs = data.GetXdataAppIndex(appName, new DxfCode[] { dxfCode });
        if (indexs.Count == 0)
            return;

        for (int i = indexs.Count - 1; i >= 0; i--)
            data[indexs[i]] = new TypedValue((short)dxfCode, newvalue);

        using (obj.ForWrite())
            obj.XData = data;
    }
    #endregion

    #region 读写模式切换

#line hidden // 调试的时候跳过它
    /// <summary>
    /// 实体自动管理读写函数，此函数性能比using模式低一倍
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
#line default
    #endregion
}