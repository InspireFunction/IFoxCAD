using Autodesk.AutoCAD.DatabaseServices;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// 扩展字典数据封装类
    /// </summary>
    public class XRecordDataList : TypedValueList
    {
        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="code">组码</param>
        /// <param name="obj">组码值</param>
        public override void Add(int code, object obj)
        {
            if (code >= 1000)
            {
                throw new System.Exception("传入的组码值不是 XRecordData 有效范围！");
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

            if ((int)code >= 1000)
            {
                throw new System.Exception("传入的组码值不是 XRecordData 有效范围！");
            }
            Add(new TypedValue((int)code, obj));
        }
        #endregion
    }
}