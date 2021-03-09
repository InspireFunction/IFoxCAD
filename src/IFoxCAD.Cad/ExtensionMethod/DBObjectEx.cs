using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.MacroRecorder;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// 实体对象扩展类
    /// </summary>
    public static class DBObjectEx
    {
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

    }
}
