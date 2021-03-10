using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// 符号表类扩展函数
    /// </summary>
    public static class SymbolTableEx
    {
        #region 图层表
        /// <summary>
        /// 添加图层
        /// </summary>
        /// <param name="table">图层符号表</param>
        /// <param name="name">图层名</param>
        /// <param name="color">图层颜色</param>
        /// <returns>图层id</returns>
        public static ObjectId Add(this SymbolTable<LayerTable, LayerTableRecord> table, string name, Color color)
        {
            return table.Add(name, lt => lt.Color = color);
        }
        /// <summary>
        /// 添加图层
        /// </summary>
        /// <param name="table">图层符号表</param>
        /// <param name="name">图层名</param>
        /// <param name="colorIndex">图层颜色索引值</param>
        /// <returns>图层id</returns>
        public static ObjectId Add(this SymbolTable<LayerTable, LayerTableRecord> table, string name, int colorIndex)
        {
            if (colorIndex < 1)
            {
                colorIndex = 1;
            }
            else if (colorIndex >= 256)
            {
                colorIndex = 256;
            }
            return table.Add(name, lt => lt.Color = Color.FromColorIndex(ColorMethod.ByColor, (short)colorIndex));
        }
        /// <summary>
        /// 更改图层名
        /// </summary>
        /// <param name="table">图层符号表</param>
        /// <param name="Oldname">旧图层名</param>
        /// <param name="NewName">新图层名</param>
        public static ObjectId Rename(this SymbolTable<LayerTable, LayerTableRecord> table, string Oldname, string NewName)
        {
            if (table.Has(Oldname))
            {
                table.Change(Oldname, ly =>
                {
                    ly.Name = NewName;
                }
                );
                return table[NewName];
            }
            else
            {
                return ObjectId.Null;
            }
        }
        #endregion

        #region 块表
        /// <summary>
        /// 添加块定义
        /// </summary>
        /// <param name="table">块表</param>
        /// <param name="name">块名</param>
        /// <param name="ents">图元</param>
        /// <param name="attdef">属性定义</param>
        /// <returns>块定义id</returns>
        public static ObjectId Add(this SymbolTable<BlockTable,BlockTableRecord> table, string name, Func<IEnumerable<Entity>> ents, IEnumerable<AttributeDefinition> attdef = null)
        {
            return table.Add(name, btr =>
            {
                table.DTrans.AddEntity(ents?.Invoke(), btr);
                if (attdef is not null)
                {
                    table.DTrans.AddEntity(attdef, btr);
                }
                
            });
        }



        /// <summary>
        /// 从文件中获取块定义
        /// </summary>
        /// <param name="table">块表</param>
        /// <param name="fileName">文件名</param>
        /// <param name="over">是否覆盖</param>
        /// <returns>块定义Id</returns>
        public static ObjectId GetBlockFrom(this SymbolTable<BlockTable, BlockTableRecord> table, string fileName, bool over)
        {
            FileInfo fi = new FileInfo(fileName);
            string blkdefname = fi.Name;
            if (blkdefname.Contains("."))
            {
                blkdefname = blkdefname.Substring(0, blkdefname.LastIndexOf('.'));
            }

            ObjectId id = table[blkdefname];
            bool has = id != ObjectId.Null;
            if ((has && over) || !has)
            {
                Database db = new Database();
                db.ReadDwgFile(fileName, FileShare.Read, true, null);
                id = table.Database.Insert(BlockTableRecord.ModelSpace, blkdefname, db, false);
            }

            return id;
        }
        #endregion

        #region 线型表
        /// <summary>
        /// 添加线型
        /// </summary>
        /// <param name="table">线型表</param>
        /// <param name="name">线型名</param>
        /// <param name="description">线型说明</param>
        /// <param name="length">线型长度</param>
        /// <param name="dash">笔画长度数组</param>
        /// <returns>线型id</returns>
        public static ObjectId Add(this SymbolTable<LinetypeTable, LinetypeTableRecord> table, string name, string description, double length, double[] dash)
        {
            return table.Add(
                name,
                ltt =>
                {
                    ltt.AsciiDescription = description;
                    ltt.PatternLength = length; //线型的总长度
                    ltt.NumDashes = dash.Length; //组成线型的笔画数目
                    for (int i = 0; i < dash.Length; i++)
                    {
                        ltt.SetDashLengthAt(i, dash[i]);
                    }
                    //ltt.SetDashLengthAt(0, 0.5); //0.5个单位的划线
                    //ltt.SetDashLengthAt(1, -0.25); //0.25个单位的空格
                    //ltt.SetDashLengthAt(2, 0); // 一个点
                    //ltt.SetDashLengthAt(3, -0.25); //0.25个单位的空格
                }
            );
        }
        #endregion

        #region 文字样式表
        /// <summary>
        /// 添加文字样式记录
        /// </summary>
        /// <param name="table">文字样式表</param>
        /// <param name="textStyleName">文字样式名</param>
        /// <param name="font">字体名</param>
        /// <param name="xscale">宽度比例</param>
        /// <returns>文字样式Id</returns>
        public static ObjectId Add(this SymbolTable<TextStyleTable, TextStyleTableRecord> table, string textStyleName, string font, double xscale)
        {
            return
                table.Add(
                    textStyleName,
                    tstr =>
                    {
                        tstr.Name = textStyleName;
                        tstr.FileName = font;
                        tstr.XScale = xscale;
                    });
        }
        #endregion

        #region 注册应用程序表

        #endregion

        #region 标注样式表

        #endregion

        #region 用户坐标系表

        #endregion

        #region 视图表

        #endregion

        #region 视口表

        #endregion
    }
}
