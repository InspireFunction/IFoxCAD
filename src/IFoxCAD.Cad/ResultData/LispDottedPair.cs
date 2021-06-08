using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

using System.Collections.Generic;

namespace IFoxCAD.Cad
{
    /// <summary>
    /// lisp 点对表的数据封装类
    /// </summary>
    public class LispDottedPair : LispList
    {
        #region 构造函数
        /// <summary>
        /// 默认无参构造函数
        /// </summary>
        public LispDottedPair()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="values">TypedValue 迭代器</param>
        public LispDottedPair(IEnumerable<TypedValue> values) : base(values)
        {
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="left">点对表左数</param>
        /// <param name="right">点对表右数</param>
        public LispDottedPair(TypedValue left, TypedValue right)
        {
            Add(left);
            Add(right);
        }
        #endregion

        /// <summary>
        /// 点对表的值
        /// </summary>
        public override List<TypedValue> Value
        {
            get
            {
                var value = new List<TypedValue>
                {
                    new TypedValue((int)LispDataType.ListBegin,-1),
                    new TypedValue((int)LispDataType.DottedPair,-1)
                };
                value.InsertRange(1, this);
                return value;
            }
        }

        #region 转换器

        /// <summary>
        /// LispDottedPair 隐式转换到 TypedValue 数组
        /// </summary>
        /// <param name="values">TypedValueList 实例</param>
        public static implicit operator TypedValue[](LispDottedPair values) => values.Value.ToArray();
        /// <summary>
        /// LispDottedPair 隐式转换到 ResultBuffer
        /// </summary>
        /// <param name="values">TypedValueList 实例</param>
        public static implicit operator ResultBuffer(LispDottedPair values) => new(values.Value.ToArray());

        #endregion
    }
}