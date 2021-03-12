using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace IFoxCAD.Cad
{
    /*  老代码,为防万一，不能删除
    /// <summary>
    /// lisp 数据格式类
    /// </summary>
    public class LispData
    {
        /// <summary>
        /// lisp 的 t
        /// </summary>
        public static readonly LispData T =
            new LispData(new TypedValue((int)LispDataType.T_atom));

        /// <summary>
        /// lisp的 nil
        /// </summary>
        public readonly static LispData Nil =
            new LispData(new TypedValue((int)LispDataType.Nil));

        /// <summary>
        /// LispData的值(TypedValue)
        /// </summary>
        public TypedValue Value { get; protected set; }

        /// <summary>
        /// 判断 lisp 数据是否为列表（listp）
        /// </summary>
        public virtual bool IsList { get { return false; } }

        #region Constructor

        /// <summary>
        /// 默认初始化
        /// </summary>
        public LispData() { }

        /// <summary>
        /// 采用lisp组码初始化
        /// </summary>
        /// <param name="code">lisp组码</param>
        public LispData(LispDataType code)
        {
            Value = new TypedValue((int)code);
        }

        /// <summary>
        /// 采用lisp组码和组码值初始化
        /// </summary>
        /// <param name="code">组码</param>
        /// <param name="value">组码值</param>
        public LispData(LispDataType code, object value)
        {
            Value = new TypedValue((int)code, value);
        }

        /// <summary>
        /// 采用TypedValue初始化
        /// </summary>
        /// <param name="value">TypedValue对象</param>
        internal LispData(TypedValue value)
        {
            Value = value;
        }

        /// <summary>
        /// 采用短整型数据初始化
        /// </summary>
        /// <param name="value">短整型数据</param>
        public LispData(short value)
            : this(LispDataType.Int16, value)
        { }

        /// <summary>
        /// 采用整型数据初始化
        /// </summary>
        /// <param name="value">整型数据</param>
        public LispData(int value)
            : this(LispDataType.Int32, value)
        { }

        /// <summary>
        /// 采用浮点数据初始化
        /// </summary>
        /// <param name="value">浮点数据</param>
        public LispData(double value)
            : this(LispDataType.Double, value)
        { }

        /// <summary>
        /// 采用二维点初始化
        /// </summary>
        /// <param name="value">二维点</param>
        public LispData(Point2d value)
            : this(LispDataType.Point2d, value)
        { }

        /// <summary>
        /// 采用三维点初始化
        /// </summary>
        /// <param name="value">三维点</param>
        public LispData(Point3d value)
            : this(LispDataType.Point3d, value)
        { }

        /// <summary>
        /// 采用对象id初始化
        /// </summary>
        /// <param name="value">对象id</param>
        public LispData(ObjectId value)
            : this(LispDataType.ObjectId, value)
        { }

        /// <summary>
        /// 采用字符串初始化
        /// </summary>
        /// <param name="value">字符串</param>
        public LispData(string value)
            : this(LispDataType.Text, value)
        { }

        /// <summary>
        /// 采用选择集初始化
        /// </summary>
        /// <param name="value">选择集</param>
        public LispData(SelectionSet value)
            : this(LispDataType.SelectionSet, value)
        { }

        /// <summary>
        /// lisp 角度的静态函数
        /// </summary>
        /// <param name="value">弧度值</param>
        /// <returns>lisp数据</returns>
        public static LispData Angle(double value)
        {
            return new LispData(LispDataType.Angle, value);
        }

        /// <summary>
        /// lisp 方向的静态函数
        /// </summary>
        /// <param name="value">弧度值</param>
        /// <returns>lisp数据</returns>
        public static LispData Orientation(double value)
        {
            return new LispData(LispDataType.Orientation, value);
        }

        #endregion Constructor

        /// <summary>
        /// 很奇怪的函数，并没有返回值
        /// </summary>
        /// <param name="rb">ResultBuffer对象</param>
        internal virtual void GetValues(ResultBuffer rb)
        {
            rb.Add(Value);
        }

        /// <summary>
        /// 返回 lisp 数据
        /// </summary>
        /// <returns>lisp数据</returns>
        public virtual object GetValue()
        {
            return Value;
        }

        /// <summary>
        /// 设置 lisp 数据值
        /// </summary>
        /// <param name="value">数据</param>
        public virtual void SetValue(object value)
        {
            Value = new TypedValue(Value.TypeCode, value);
        }

        /// <summary>
        /// 设置 lisp 数据值
        /// </summary>
        /// <param name="code">组码</param>
        /// <param name="value">数据</param>
        public virtual void SetValue(int code, object value)
        {
            Value = new TypedValue(code, value);
        }
    }
    */

}