using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace IFoxCAD.Cad.ExtensionMethod
{
    /// <summary>
    /// 符号表记录扩展类
    /// </summary>
    public static class SymbolTableRecordEx
    {
        #region 图层表
        /// <summary>
        /// 添加图层
        /// </summary>
        /// <param name="table">图层符号表</param>
        /// <param name="name">图层名</param>
        /// <param name="color">图层颜色</param>
        /// <returns>图层id</returns>
        public static ObjectId Add(this SymbolTable<LayerTable,LayerTableRecord> table, string name, Color color)
        {
            return table.Add(name, lt => lt.Color = color);
        }

        #endregion
    }
}
