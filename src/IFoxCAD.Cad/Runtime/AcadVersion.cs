using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// cad版本号类
    /// </summary>
    public class AcadVersion
    {
        /// <summary>
        /// 主版本
        /// </summary>
        public int Major
        { private set; get; }

        /// <summary>
        /// 次版本
        /// </summary>
        public int Minor
        { private set; get; }

        /// <summary>
        /// 版本号
        /// </summary>
        public double ProgId => double.Parse($"{Major}.{Minor}");

        /// <summary>
        /// 注册表名称
        /// </summary>
        public string ProductName
        { private set; get; }

        /// <summary>
        /// 注册表位置
        /// </summary>
        public string ProductRootKey
        { private set; get; }

        private static readonly string _pattern = @"Autodesk\\AutoCAD\\R(\d+)\.(\d+)\\.*?";

        private static List<AcadVersion> _versions;

        /// <summary>
        /// 所有安装的cad的版本号
        /// </summary>
        public static List<AcadVersion> Versions
        {
            get
            {
                if (_versions == null)
                {
                    string[] copys =
                       Registry.LocalMachine
                       .OpenSubKey(@"SOFTWARE\Autodesk\Hardcopy")
                       .GetValueNames();
                    _versions = new List<AcadVersion>();
                    foreach (var rootkey in copys)
                    {
                        if (Regex.IsMatch(rootkey, _pattern))
                        {
                            var gs = Regex.Match(rootkey, _pattern).Groups;
                            var ver =
                                new AcadVersion
                                {
                                    ProductRootKey = rootkey,
                                    ProductName =
                                        Registry.LocalMachine
                                        .OpenSubKey("SOFTWARE")
                                        .OpenSubKey(rootkey)
                                        .GetValue("ProductName")
                                        .ToString(),

                                    Major = int.Parse(gs[1].Value),
                                    Minor = int.Parse(gs[2].Value),
                                };

                            _versions.Add(ver);
                        }
                    }
                }
                return _versions;
            }
        }

        /// <summary>已打开的cad的版本号</summary>
        /// <param name="app">已打开cad的application对象</param>
        /// <returns>cad版本号对象</returns>
        public static AcadVersion FromApp(object app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            string acver =
                app.GetType()
                    .InvokeMember(
                    "Version",
                    BindingFlags.GetProperty,
                    null,
                    app,
                    new object[0]).ToString();

            var gs = Regex.Match(acver, @"(\d+)\.(\d+).*?").Groups;
            int major = int.Parse(gs[1].Value);
            int minor = int.Parse(gs[2].Value);
            foreach (var ver in Versions)
            {
                if (ver.Major == major && ver.Minor == minor)
                    return ver;
            }

            return null;
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        /// <returns>表示版本号的字符串</returns>
        public override string ToString()
        {
            return
                $"名称:{ProductName}\n版本号:{ProgId}\n注册表位置:{ProductRootKey}";
        }
    }
}