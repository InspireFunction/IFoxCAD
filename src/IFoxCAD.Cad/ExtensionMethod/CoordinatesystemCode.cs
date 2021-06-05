using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// 坐标系类型枚举
    /// </summary>
    public enum CoordinateSystemCode
    {
        /// <summary>
        /// 世界坐标系
        /// </summary>
        Wcs = 0,

        /// <summary>
        /// 用户坐标系
        /// </summary>
        Ucs,

        /// <summary>
        /// 模型空间坐标系
        /// </summary>
        MDcs,

        /// <summary>
        /// 图纸空间坐标系
        /// </summary>
        PDcs
    }

}
