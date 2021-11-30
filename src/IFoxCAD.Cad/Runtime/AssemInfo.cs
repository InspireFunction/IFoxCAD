using System;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// 程序集信息
    /// </summary>
    [Serializable]
    public struct AssemInfo
    {
        /// <summary>
        /// 注册名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 程序集全名
        /// </summary>
        public string Fullname { get; set; }

        /// <summary>
        /// 程序集路径
        /// </summary>
        public string Loader { get; set; }

        /// <summary>
        /// 加载方式
        /// </summary>
        public AssemLoadType LoadType { get; set; }

        /// <summary>
        /// 程序集说明
        /// </summary>
        public string Description { get; set; }
    }
}
