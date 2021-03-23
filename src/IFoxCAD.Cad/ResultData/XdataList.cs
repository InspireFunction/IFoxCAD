using Autodesk.AutoCAD.DatabaseServices;

using System.Collections.Generic;
using System.Linq;

namespace IFoxCAD.Cad
{

    /// <summary>
    /// 扩展数据封装类
    /// </summary>
    public class XDataList : TypedValueList
    {
        public XDataList(IEnumerable<TypedValue> values) : base(values) { }
        
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
                throw new System.Exception("传入的组码值不是XData有效范围！");
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

            Add((int)code, obj);
        }

        #endregion

        #region 转换器
        /// <summary>
        /// ResultBuffer 隐式转换到 XDataList
        /// </summary>
        /// <param name="buffer">ResultBuffer 实例</param>
        public static implicit operator XDataList(ResultBuffer buffer) => new(buffer.AsArray());
        /// <summary>
        /// XDataList 隐式转换到 TypedValue 数组
        /// </summary>
        /// <param name="values">TypedValueList 实例</param>
        public static implicit operator TypedValue[](XDataList values) => values.ToArray();
        /// <summary>
        /// XDataList 隐式转换到 ResultBuffer
        /// </summary>
        /// <param name="values">TypedValueList 实例</param>
        public static implicit operator ResultBuffer(XDataList values) => new(values);
        /// <summary>
        /// TypedValue 数组隐式转换到 XDataList
        /// </summary>
        /// <param name="values">TypedValue 数组</param>
        public static implicit operator XDataList(TypedValue[] values) => new(values);
        #endregion
    }
}