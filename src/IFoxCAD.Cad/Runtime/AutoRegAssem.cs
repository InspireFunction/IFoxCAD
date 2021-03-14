using Autodesk.AutoCAD.DatabaseServices;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CadRuntime = Autodesk.AutoCAD.Runtime;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// 程序集加载类型
    /// </summary>
    public enum AssemLoadType
    {
        /// <summary>
        /// 启动
        /// </summary>
        Startting = 2,

        /// <summary>
        /// 随命令
        /// </summary>
        ByCommand = 12,

        /// <summary>
        /// 无效
        /// </summary>
        Disabled = 20
    }

    /// <summary>
    /// 自动加载程序集的抽象类，继承自 IExtensionApplication 接口
    /// </summary>
    public abstract class AutoRegAssem : CadRuntime.IExtensionApplication
    {
        private AssemInfo _info = new AssemInfo();

        /// <summary>
        /// 程序集的路径
        /// </summary>
        public static FileInfo Location => new FileInfo(Assembly.GetCallingAssembly().Location);

        /// <summary>
        /// 程序集的目录
        /// </summary>
        public static DirectoryInfo CurrDirectory => Location.Directory;

        /// <summary>
        /// 获取程序集的目录
        /// </summary>
        /// <param name="assem">程序集</param>
        /// <returns>路径对象</returns>
        public static DirectoryInfo GetDirectory(Assembly assem)
        {
            if (assem == null)
            {
                throw new ArgumentNullException(nameof(assem));
            }
            return new FileInfo(assem.Location).Directory;
        }

        /// <summary>
        /// 初始化程序集信息
        /// </summary>
        public AutoRegAssem()
        {
            Assembly assem = Assembly.GetCallingAssembly();
            _info.Loader = assem.Location;
            _info.Fullname = assem.FullName;
            _info.Name = assem.GetName().Name;
            _info.LoadType = AssemLoadType.Startting;

            if (!SearchForReg())
            {
                RegApp();
            }
        }

        #region RegApp

        private static RegistryKey GetAcAppKey()
        {
            string key = HostApplicationServices.Current.MachineRegistryProductRootKey;
            RegistryKey ackey =
                Registry.CurrentUser.OpenSubKey(key, true);
            return ackey.CreateSubKey("Applications");
        }

        private bool SearchForReg()
        {
            RegistryKey appkey = GetAcAppKey();
            var regApps = appkey.GetSubKeyNames();
            return regApps.Contains(_info.Name);
        }

        /// <summary>
        /// 在注册表写入自动加载的程序集信息
        /// </summary>
        public void RegApp()
        {
            RegistryKey appkey = GetAcAppKey();
            RegistryKey rk = appkey.CreateSubKey(_info.Name);
            rk.SetValue("DESCRIPTION", _info.Fullname, RegistryValueKind.String);
            rk.SetValue("LOADCTRLS", _info.LoadType, RegistryValueKind.DWord);
            rk.SetValue("LOADER", _info.Loader, RegistryValueKind.String);
            rk.SetValue("MANAGED", 1, RegistryValueKind.DWord);
            appkey.Close();
        }

        #endregion RegApp

        #region IExtensionApplication 成员

        /// <summary>
        /// 初始化函数
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// 结束函数
        /// </summary>
        public abstract void Terminate();

        #endregion IExtensionApplication 成员
    }
}