namespace IFoxCAD.Cad;

/// <summary>
/// 选择集过滤器抽象类
/// </summary>
public abstract class OpFilter
{
    /// <summary>
    /// 过滤器的名字
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// 只读属性，表示这个过滤器取反
    /// </summary>
    public OpFilter Not => new OpNot(this);

    /// <summary>
    /// 获取TypedValue类型的值的迭代器的抽象方法，子类必须重写
    /// </summary>
    /// <returns>TypedValue迭代器</returns>
    public abstract IEnumerable<TypedValue> GetValues();

    /// <summary>
    /// 非操作符，返回的是OpFilter类型变量的 <see cref="Not"/> 属性
    /// </summary>
    /// <param name="item">OpFilter类型变量</param>
    /// <returns>OpFilter对象</returns>
    public static OpFilter operator !(OpFilter item)
    {
        return item.Not;
    }

    /// <summary>
    /// 过滤器值转换为 TypedValue 类型数组
    /// </summary>
    /// <returns>TypedValue数组</returns>
    public TypedValue[] ToArray()
    {
        return GetValues().ToArray();
    }

    /// <summary>
    /// 隐式类型转换，将自定义的过滤器转换为 Autocad 认识的选择集过滤器
    /// </summary>
    /// <param name="item">过滤器对象</param>
    /// <returns>
    /// 选择集过滤器.
    /// </returns>
    public static implicit operator SelectionFilter(OpFilter item)
    {
        return new SelectionFilter(item.ToArray());
    }

    /// <summary>
    /// 转换为字符串
    /// </summary>
    /// <returns>字符串</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var value in GetValues())
            sb.Append(value);
        return sb.ToString();
    }

    /// <summary>
    /// 构建过滤器
    /// </summary>
    /// <example>
    /// 举两个利用构建函数创建选择集过滤的例子
    /// <code>
    /// <![CDATA[
    /// 例子1：
    /// var p = new Point3d(10, 10, 0);
    /// var f = OpFilter.Build(
    ///         e =>!(e.Dxf(0) == "line" & e.Dxf(8) == "0")
    ///         | e.Dxf(0) != "circle" & e.Dxf(8) == "2" & e.Dxf(10) >= p);
    ///
    /// 例子2：
    /// var f2 = OpFilter.Build(
    ///         e => e.Or(
    ///                 !e.And(e.Dxf(0) == "line", e.Dxf(8) == "0"),
    ///                 e.And(e.Dxf(0) != "circle", e.Dxf(8) == "2",
    ///                       e.Dxf(10) <= new Point3d(10, 10, 0))));
    /// ]]>
    /// </code></example>
    /// <param name="func">构建过滤器的函数委托</param>
    /// <returns>过滤器</returns>
    public static OpFilter Build(Func<Op, Op> func)
    {
        return func(new Op()).Filter!;
    }

    #region Operator

    /// <summary>
    /// 过滤器操作符类
    /// </summary>
    public class Op
    {
        /// <summary>
        /// 过滤器属性
        /// </summary>
        internal OpFilter? Filter { get; private set; }

        internal Op() { }

        private Op(OpFilter filter)
        {
            Filter = filter;
        }

        /// <summary>
        /// AND 操作符
        /// </summary>
        /// <param name="args">操作符类型的可变参数</param>
        /// <returns>Op对象</returns>
#pragma warning disable CA1822 // 将成员标记为 static
        public Op And(params Op[] args)
#pragma warning restore CA1822 // 将成员标记为 static
        {
            var filter = new OpAnd();
            foreach (var op in args)
                filter.Add(op.Filter!);
            return new Op(filter);
        }

        /// <summary>
        /// or 操作符
        /// </summary>
        /// <param name="args">操作符类型的可变参数</param>
        /// <returns>Op对象</returns>
#pragma warning disable CA1822 // 将成员标记为 static
        public Op Or(params Op[] args)
#pragma warning restore CA1822 // 将成员标记为 static
        {
            var filter = new OpOr();
            foreach (var op in args)
                filter.Add(op.Filter!);
            return new Op(filter);
        }

        /// <summary>
        /// dxf 操作符，此函数只能用于过滤器中，不是组码操作函数
        /// </summary>
        /// <param name="code">组码</param>
        /// <returns>Op对象</returns>
#pragma warning disable CA1822 // 将成员标记为 static
        public Op Dxf(int code)
#pragma warning restore CA1822 // 将成员标记为 static
        {
            return new Op(new OpEqual(code));
        }

        /// <summary>
        /// dxf 操作符，此函数只能用于过滤器中，不是组码操作函数
        /// </summary>
        /// <param name="code">组码</param>
        /// <param name="content">关系运算符的值，比如">,>,="</param>
        /// <returns>Op对象</returns>
#pragma warning disable CA1822 // 将成员标记为 static
        public Op Dxf(int code, string content)
#pragma warning restore CA1822 // 将成员标记为 static
        {
            return new Op(new OpComp(content, code));
        }

        /// <summary>
        /// 非操作符
        /// </summary>
        /// <param name="right">过滤器操作符对象</param>
        /// <returns>Op对象</returns>
        public static Op operator !(Op right)
        {
            right.Filter = !right.Filter!;
            return right;
        }

        /// <summary>
        /// 相等操作符
        /// </summary>
        /// <param name="left">过滤器操作符对象</param>
        /// <param name="right">数据</param>
        /// <returns>Op对象</returns>
        public static Op operator ==(Op left, object right)
        {
            var eq = (OpEqual)left.Filter!;
            eq.SetValue(right);
            return left;
        }

        /// <summary>
        /// 不等操作符
        /// </summary>
        /// <param name="left">过滤器操作符对象</param>
        /// <param name="right">数据</param>
        /// <returns>Op对象</returns>
        public static Op operator !=(Op left, object right)
        {
            var eq = (OpEqual)left.Filter!;
            eq.SetValue(right);
            left.Filter = eq.Not;
            return left;
        }

        private static Op GetCompOp(string content, Op left, object right)
        {
            var eq = (OpEqual)left.Filter!;
            var comp = new OpComp(content, eq.Value.TypeCode, right);
            return new Op(comp);
        }

        /// <summary>
        /// 大于操作符
        /// </summary>
        /// <param name="left">过滤器操作符对象</param>
        /// <param name="right">数据</param>
        /// <returns>Op对象</returns>
        public static Op operator >(Op left, object right)
        {
            return GetCompOp(">", left, right);
        }

        /// <summary>
        /// 小于操作符
        /// </summary>
        /// <param name="left">过滤器操作符对象</param>
        /// <param name="right">数据</param>
        /// <returns>Op对象</returns>
        public static Op operator <(Op left, object right)
        {
            return GetCompOp("<", left, right);
        }

        /// <summary>
        /// 大于等于操作符
        /// </summary>
        /// <param name="left">过滤器操作符对象</param>
        /// <param name="right">数据</param>
        /// <returns>Op对象</returns>
        public static Op operator >=(Op left, object right)
        {
            return GetCompOp(">=", left, right);
        }

        /// <summary>
        /// 小于等于操作符
        /// </summary>
        /// <param name="left">过滤器操作符对象</param>
        /// <param name="right">数据</param>
        /// <returns>Op对象</returns>
        public static Op operator <=(Op left, object right)
        {
            return GetCompOp("<=", left, right);
        }

        /// <summary>
        /// 大于等于操作符
        /// </summary>
        /// <param name="left">过滤器操作符对象</param>
        /// <param name="right">点</param>
        /// <returns>Op对象</returns>
        public static Op operator >=(Op left, Point3d right)
        {
            return GetCompOp(">,>,*", left, right);
        }

        /// <summary>
        /// 小于等于操作符
        /// </summary>
        /// <param name="left">过滤器操作符对象</param>
        /// <param name="right">点</param>
        /// <returns>Op对象</returns>
        public static Op operator <=(Op left, Point3d right)
        {
            return GetCompOp("<,<,*", left, right);
        }

        /// <summary>
        /// 并操作符
        /// </summary>
        /// <param name="left">过滤器操作符对象</param>
        /// <param name="right">过滤器操作符对象</param>
        /// <returns>Op对象</returns>
        public static Op operator &(Op left, Op right)
        {
            var filter = new OpAnd
            {
                left.Filter!,
                right.Filter!
            };
            return new Op(filter);
        }

        /// <summary>
        /// 或操作符
        /// </summary>
        /// <param name="left">过滤器操作符对象</param>
        /// <param name="right">过滤器操作符对象</param>
        /// <returns>Op对象</returns>
        public static Op operator |(Op left, Op right)
        {
            var filter = new OpOr
            {
                left.Filter!,
                right.Filter!
            };
            return new Op(filter);
        }

        /// <summary>
        /// 异或操作符
        /// </summary>
        /// <param name="left">过滤器操作符对象</param>
        /// <param name="right">过滤器操作符对象</param>
        /// <returns>Op对象</returns>
        public static Op operator ^(Op left, Op right)
        {
            var filter = new OpXor(left.Filter!, right.Filter!);
            return new Op(filter);
        }

        /// <summary>
        /// 比较函数
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>
        /// 是否相等
        /// </returns>
        public override bool Equals(object? obj) => base.Equals(obj);

        /// <summary>
        /// 获取HashCode
        /// </summary>
        /// <returns>HashCode</returns>
        public override int GetHashCode() => base.GetHashCode();
    }

    #endregion Operator
}
