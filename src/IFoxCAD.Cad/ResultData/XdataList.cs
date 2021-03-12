using Autodesk.AutoCAD.DatabaseServices;

using System.Linq;

namespace IFoxCAD.Cad
{

    /// <summary>
    /// 扩展数据封装类
    /// </summary>
    public class XDataList : TypedValueList
    {
        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="code">组码</param>
        /// <param name="obj">组码值</param>
        public override void Add(int code, object obj)
        {
            if (code < 1000 || code > 1071)
            {
                throw new System.Exception("传入的组码值不是xdata有效范围！");
            }
            Add(new TypedValue(code, obj));
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="code">dxfcode枚举值</param>
        /// <param name="obj">组码值</param>
        public void Add(DxfCode code, object obj)
        {

            if ((int)code < 1000 || (int)code > 1071)
            {
                throw new System.Exception("传入的组码值不是xdata有效范围！");
            }
            Add(new TypedValue((int)code, obj));
        }

        #endregion
    }
}