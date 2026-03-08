#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace IFoxCAD.Cad;

/// <summary>
/// Linq Distinct 消重比较两点在容差范围内就去除
/// </summary>
public class TolerancePoint2d : IEqualityComparer<Point2d>
{
    readonly double _tolerance;
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="tolerance">容差</param>
    public TolerancePoint2d(double tolerance = 1e-6)
    {
        _tolerance = tolerance;
    }
    /// <summary>
    /// 比较
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool Equals(Point2d a, Point2d b)// Point3d是struct不会为null
    {
        /*默认规则是==是0容差,Eq是有容差*/
        // 方形限定
        // 在 0~1e-6 范围实现 圆形限定 则计算部分在浮点数6位后,没有啥意义
        // 在 0~1e-6 范围实现 从时间和CPU消耗来说,圆形限定 都没有 方形限定 的好
        if (_tolerance <= 1e-6)
            return Math.Abs(a.X - b.X) <= _tolerance && Math.Abs(a.Y - b.Y) <= _tolerance;

        // 圆形限定
        // DistanceTo 分别对XYZ进行了一次乘法,也是总数3次乘法,然后求了一次平方根
        // (X86.CPU.FSQRT指令用的牛顿迭代法/软件层面可以使用快速平方根....我还以为CPU会采取快速平方根这样的取表操作)
        return a.IsEqualTo(b, new Tolerance(_tolerance, _tolerance));
    }
    /// <summary>
    /// 哈希
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int GetHashCode(Point2d obj)
    {
        // 结构体直接返回 obj.GetHashCode(); Point3d ToleranceDistinct3d
        // 因为结构体是用可值叠加来判断?或者因为结构体兼备了一些享元模式的状态?
        // 而类是构造的指针,所以取哈希值要改成x+y+z..s给Equals判断用,+是会溢出,所以用^
        return (int)obj.X ^ (int)obj.Y;// ^ (int)obj.Z;
    }
}

/// <summary>
/// 矩形范围类
/// </summary>
[Serializable]
[StructLayout(LayoutKind.Sequential)]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(Rect))]
public class Rect : IEquatable<Rect>, IComparable<Rect>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString("f4");

#pragma warning disable CA2211 // 非常量字段应当不可见
    /// <summary>
    /// 矩形容差
    /// </summary>
    public static TolerancePoint2d RectTolerance = new(1e-6);
    /// <summary>
    /// cad容差
    /// </summary>
    public static Tolerance CadTolerance = new(1e-6, 1e-6);
#pragma warning restore CA2211 // 非常量字段应当不可见

    #region 字段
    #endregion

    #region 成员
    /// <summary>
    /// X
    /// </summary>
    public double X { get; set; }
    /// <summary>
    /// Y
    /// </summary>
    public double Y { get; set; }
    /// <summary>
    /// 左
    /// </summary>
    public double Left
    {
        get { return X; }
        set { X = value; }
    }
    /// <summary>
    /// 下
    /// </summary>
    public double Bottom
    {
        get { return Y; }
        set { Y = value; }
    }
    /// <summary>
    /// 右
    /// </summary>
    public double Right { get; set; }
    /// <summary>
    /// 上
    /// </summary>
    public double Top { get; set; }
    /// <summary>
    /// 宽
    /// </summary>
    public double Width => Right - X;
    /// <summary>
    /// 高
    /// </summary>
    public double Height => Top - Y;
    /// <summary>
    /// 面积
    /// </summary>
    public double Area
    {
        get
        {
            var ar = (Right - X) * (Top - Y);
            return ar < 1e-10 ? 0 : ar;
        }
    }
    /// <summary>
    /// 左下Min
    /// </summary>
    public Point2d MinPoint => LeftLower;
    /// <summary>
    /// 右上Max
    /// </summary>
    public Point2d MaxPoint => RightUpper;
    /// <summary>
    /// 中间
    /// </summary>
    public Point2d CenterPoint => Midst;

    /// <summary>
    /// 左下Min
    /// </summary>
    public Point2d LeftLower => new(X, Y);

    /// <summary>
    /// 左中
    /// </summary>
    public Point2d LeftMidst => new(X, Midst.Y);

    /// <summary>
    /// 左上
    /// </summary>
    public Point2d LeftUpper => new(X, Top);

    /// <summary>
    /// 右上Max
    /// </summary>
    public Point2d RightUpper => new(Right, Top);

    /// <summary>
    /// 右中
    /// </summary>
    public Point2d RightMidst => new(Right, Midst.Y);

    /// <summary>
    /// 右下
    /// </summary>
    public Point2d RightBottom => new(Right, Y);

    /// <summary>
    /// 中间
    /// </summary>
    public Point2d Midst => new(((Right - X) * 0.5) + X, ((Top - Y) * 0.5) + Y);

    /// <summary>
    /// 中上
    /// </summary>
    public Point2d MidstUpper => new(Midst.X, Top);

    /// <summary>
    /// 中下
    /// </summary>
    public Point2d MidstBottom => new(Midst.X, Y);

    /// <summary>
    /// 是一个点
    /// 水平或垂直直线包围盒是面积是0,所以面积是0不一定是点
    /// </summary>
    public bool IsPoint => Math.Abs(X - Right) < 1e-10 && Math.Abs(Y - Top) < 1e-10;
    #endregion

    #region 构造
    /// <summary>
    /// 矩形类
    /// </summary>
    public Rect()
    {
    }

    /// <summary>
    /// 矩形类
    /// </summary>
    /// <param name="left">左</param>
    /// <param name="bottom">下</param>
    /// <param name="right">右</param>
    /// <param name="top">上</param>
    public Rect(double left, double bottom, double right, double top)
    {
        X = left;
        Y = bottom;
        Right = right;
        Top = top;
    }

    /// <summary>
    /// 构造矩形类
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p3"></param>
    /// <param name="check">是否检查大小</param>
    public Rect(Point2d p1, Point2d p3, bool check = false)
    {
        if (check)
        {
            X = Math.Min(p1.X, p3.X);
            Y = Math.Min(p1.Y, p3.Y);
            Right = Math.Max(p1.X, p3.X);
            Top = Math.Max(p1.Y, p3.Y);
        }
        else
        {
            X = p1.X;
            Y = p1.Y;
            Right = p3.X;
            Top = p3.Y;
        }
    }
    #endregion

    #region 重载运算符_比较
    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Rect);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public bool Equals(Rect? b)
    {
        return Equals(b, 1e-6);/*默认规则是==是0容差,Eq是有容差*/
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool operator !=(Rect? a, Rect? b)
    {
        return !(a == b);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool operator ==(Rect? a, Rect? b)
    {
        // 此处地方不允许使用==null,因为此处是定义
        if (b is null)
            return a is null;
        else if (a is null)
            return false;
        if (ReferenceEquals(a, b))// 同一对象
            return true;

        return a.Equals(b, 0);
    }

    /// <summary>
    /// 比较核心
    /// </summary>
    public bool Equals(Rect? b, double tolerance = 1e-6)
    {
        if (b is null)
            return false;
        if (ReferenceEquals(this, b)) // 同一对象
            return true;

        return Math.Abs(X - b.X) < tolerance &&
                Math.Abs(Right - b.Right) < tolerance &&
                Math.Abs(Top - b.Top) < tolerance &&
                Math.Abs(Y - b.Y) < tolerance;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return (((int)X ^ (int)Y).GetHashCode() ^ (int)Right).GetHashCode() ^ (int)Top;
    }
    #endregion

    #region 包含
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Point2d"></param>
    /// <returns></returns>
    public bool Contains(Point2d Point2d)
    {
        return Contains(Point2d.X, Point2d.Y);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool Contains(double x, double y)
    {
        return X <= x && x <= Right &&
               Y <= y && y <= Top;
    }

    /// <summary>
    /// 四个点都在内部就是包含
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public bool Contains(Rect rect)
    {
        return X <= rect.X && rect.Right <= Right &&
               Y <= rect.Y && rect.Top <= Top;
    }

    /// <summary>
    /// 一个点在内部就是碰撞
    /// </summary>
    /// <param name="rect"></param>
    /// <returns>true内部</returns>
    [MethodImpl]
    public bool IntersectsWith(Rect rect)
    {
        return rect.X <= Right && X <= rect.Right &&
                rect.Top >= Y && rect.Y <= Top;
    }
    #endregion

    #region 方法
    /// <summary>
    /// 获取共点
    /// </summary>
    /// <returns></returns>
    public Point2d[] GetCommonPoint(Rect other)
    {
        return ToPoints().Intersect(other.ToPoints(), RectTolerance).ToArray();
    }
    /// <summary>
    /// 转换为point2d数组
    /// </summary>
    /// <returns></returns>
    public Point2d[] ToPoints()
    {
        var a = MinPoint;// min
        Point2d b = new(Right, Y);
        var c = MaxPoint;// max
        Point2d d = new(X, Top);
        return [a, b, c, d];
    }
    /// <summary>
    /// 转换为point2d元组
    /// </summary>
    /// <returns></returns>
    public (Point2d boxMin, Point2d boxRigthDown, Point2d boxMax, Point2d boxLeftUp) ToPoints4()
    {
        var a = MinPoint;// min
        Point2d b = new(Right, Y);
        var c = MaxPoint;// max
        Point2d d = new(X, Top);
        return (a, b, c, d);
    }

    /// <summary>
    /// 四周膨胀
    /// </summary>
    /// <returns></returns>
    public Rect Expand(double d)
    {
        return new Rect(X - d, Y - d, Right + d, Top + d);
    }

    /// <summary>
    /// 是否矩形(带角度)
    /// </summary>
    /// <param name="ptList"></param>
    /// <param name="tolerance"></param>
    /// <returns></returns>
    public static bool IsRectAngle(IEnumerable<Point2d> ptList, double tolerance = 1e-8)
    {
        //if (ptList == null)
        //    throw new ArgumentNullException(nameof(ptList));
        ArgumentNullException.ThrowIfNull(ptList);
        var pts = ptList.ToList();
        /*
         *  消重,不这里设置,否则这不是一个正确的单元测试
         *  // var ptList = pts.Distinct().ToList();
         *  var ptList = pts.DistinctExBy((a, b) => a.DistanceTo(b) < 1e-6).ToList();
         */
        if (pts.Count == 5)
        {
            // 首尾点相同移除最后
            if (pts[0].IsEqualTo(pts[^1], CadTolerance))
                pts.RemoveAt(pts.Count - 1);
        }
        if (pts.Count != 4)
            return false;

        // 最快的方案
        // 点乘求值法:(为了处理 正梯形/平行四边形 需要三次)
        // 这里的容差要在1e-8内,因为点乘的三次浮点数乘法会令精度变低
        var dot = DotProductValue(pts[0], pts[1], pts[3]);
        if (Math.Abs(dot) < tolerance)
        {
            dot = DotProductValue(pts[1], pts[2], pts[0]);
            if (Math.Abs(dot) < tolerance)
            {
                dot = DotProductValue(pts[2], pts[3], pts[1]);
                return Math.Abs(dot) < tolerance;
            }
        }
        return false;
    }

    /// <summary>
    /// 点积,求值
    /// <a href="https://zhuanlan.zhihu.com/p/359975221"> 1.是两个向量的长度与它们夹角余弦的积 </a>
    /// <a href="https://www.cnblogs.com/JJBox/p/14062009.html#_label1"> 2.求四个点是否矩形使用 </a>
    /// </summary>
    /// <param name="o">原点</param>
    /// <param name="a">点</param>
    /// <param name="b">点</param>
    /// <returns><![CDATA[>0方向相同,夹角0~90度;=0相互垂直;<0方向相反,夹角90~180度]]></returns>
    static double DotProductValue(Point2d o, Point2d a, Point2d b)
    {
        var oa = o.GetVectorTo(a);
        var ob = o.GetVectorTo(b);
        return (oa.X * ob.X) + (oa.Y * ob.Y);
    }

    /// <summary>
    /// 是否轴向矩形(无角度)
    /// </summary>
    public static bool IsRect(IEnumerable<Point2d> ptList, double tolerance = 1e-10)
    {
        //if (ptList == null)
        //    throw new ArgumentNullException(nameof(ptList));
        ArgumentNullException.ThrowIfNull(ptList);
        var pts = ptList.ToList();
        if (pts.Count == 5)
        {
            // 首尾点相同移除最后
            if (pts[0].IsEqualTo(pts[^1], CadTolerance))
                pts.RemoveAt(pts.Count - 1);
        }
        if (pts.Count != 4)
            return false;

        return Math.Abs(pts[0].X - pts[3].X) < tolerance &&
                Math.Abs(pts[0].Y - pts[1].Y) < tolerance &&
                Math.Abs(pts[1].X - pts[2].X) < tolerance &&
                Math.Abs(pts[2].Y - pts[3].Y) < tolerance;
    }

    /// <summary>
    /// 获取点集的包围盒的最小点和最大点(无角度)
    /// </summary>
    /// <param name="pts"></param>
    public static (Point2d boxMin, Point2d boxMax) GetMinMax(IEnumerable<Point2d> pts)
    {
        var xMin = double.MaxValue;
        var xMax = double.MinValue;
        var yMin = double.MaxValue;
        var yMax = double.MinValue;
        // var zMin = double.MaxValue;
        // var zMax = double.MinValue;

        foreach (var p in pts)
        {
            xMin = Math.Min(p.X, xMin);
            xMax = Math.Max(p.X, xMax);
            yMin = Math.Min(p.Y, yMin);
            yMax = Math.Max(p.Y, yMax);
            // zMin = Math.Min(p.Z, zMin);
            // zMax = Math.Max(p.Z, zMax);
        }
        return (new Point2d(xMin, yMin), new Point2d(xMax, yMax));
    }

    /// <summary>
    /// 矩形点序逆时针排列,将min点[0],max点是[3](带角度)
    /// </summary>
    /// <param name="pts1"></param>
    /// <returns></returns>
    public static bool RectAnglePointOrder(IEnumerable<Point2d> pts1)
    {
        //if (pts == null)
        //    throw new ArgumentNullException(nameof(pts));
        ArgumentNullException.ThrowIfNull(pts1);
        if (!IsRectAngle(pts1))
            return false;

        // 获取min和max点(非包围盒)
        var pts = pts1.OrderBy(a => a.X).ThenBy(a => a.Y).ToList();
        var minPt = pts.First();
        var maxPt = pts.Last();
        var link = new LoopList<Point2d>();
        link.AddRange(pts);

        pts.Clear();
        // 排序这四个点,左下/右下/右上/左上
        var node = link.Find(minPt);
        for (var i = 0; i < 4; i++)
        {
            pts.Add(node!.Value);
            node = node.Next;
        }
        // 保证是逆时针
        var isAcw = CrossAclockwise(pts[0], pts[1], pts[2]);
        if (!isAcw)
            (pts[3], pts[1]) = (pts[1], pts[3]);
        return true;
    }

    /// <summary>
    /// 叉积,二维叉乘计算
    /// </summary>
    /// <param name="a">传参是向量,表示原点是0,0</param>
    /// <param name="b">传参是向量,表示原点是0,0</param>
    /// <returns>其模为a与b构成的平行四边形面积</returns>
    static double Cross(Vector2d a, Vector2d b)
    {
        return a.X * b.Y - a.Y * b.X;
    }

    /// <summary>
    /// 叉积,二维叉乘计算
    /// </summary>
    /// <param name="o">原点</param>
    /// <param name="a">oa向量</param>
    /// <param name="b">ob向量,此为判断点</param>
    /// <returns>返回值有正负,表示绕原点四象限的位置变换,也就是有向面积</returns>
    static double Cross(Point2d o, Point2d a, Point2d b)
    {
        return Cross(o.GetVectorTo(a), o.GetVectorTo(b));
    }

    /// <summary>
    /// 叉积,逆时针方向为真
    /// </summary>
    /// <param name="o">直线点1</param>
    /// <param name="a">直线点2</param>
    /// <param name="b">判断点</param>
    /// <returns>b点在oa的逆时针时为 <see langword="true"/></returns>
    static bool CrossAclockwise(Point2d o, Point2d a, Point2d b)
    {
        return Cross(o, a, b) > -1e-6;// 浮点数容差考虑
    }

#if !WinForm
    /// <summary>
    /// 创建矩形范围多段线
    /// </summary>
    /// <returns>多段线对象</returns>
    public Entity ToPolyLine()
    {
        List<BulgeVertex> bv = [];
        Polyline pl = new();
        pl.SetDatabaseDefaults();
        int index = 0;
        foreach (var vertex in ToPoints())
        {
            pl.AddVertexAt(index, vertex, 0, 0, 0);
        }
        return pl;
    }
#endif

    /// <summary>
    /// 列扫碰撞检测(碰撞算法)
    /// 比四叉树还快哦~
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="box">继承Rect的集合</param>
    /// <param name="firstProcessing">先处理集合每一个成员;返回true就跳过后续委托</param>
    /// <param name="collisionProcessing">碰撞,返回两个碰撞的成员;返回true就跳过后续委托</param>
    /// <param name="lastProcessing">后处理集合每一个成员</param>
    public static void XCollision<T>(List<T> box,
        Func<T, bool> firstProcessing,
        Func<T, T, bool> collisionProcessing,
        Action<T> lastProcessing) where T : Rect
    {
        // 先排序X:不需要Y排序,因为Y的上下浮动不共X .ThenBy(a => a.Box.Y)
        // 因为先排序就可以有序遍历x区间,超过就break,达到更快
        box = box.OrderBy(a => a.X).ToList();

        // 遍历所有图元
        for (var i = 0; i < box.Count; i++)
        {
            var oneRect = box[i];
            if (firstProcessing(oneRect))
                continue;

            var actionlast = true;

            // 搜索范围要在 one 的头尾中间的部分
            for (var j = i + 1; j < box.Count; j++)
            {
                var twoRect = box[j];
                // x碰撞:矩形2的Left 在 矩形1[Left-Right]闭区间;穿过的话,也必然有自己的Left因此不需要处理
                if (oneRect.X <= twoRect.X && twoRect.X <= oneRect.Right)
                {
                    // y碰撞,那就是真的碰撞了
                    if ((oneRect.Top >= twoRect.Top && twoRect.Top >= oneRect.Y) /*包容上边*/
                     || (oneRect.Top >= twoRect.Y && twoRect.Y >= oneRect.Y)     /*包容下边*/
                     || (twoRect.Top >= oneRect.Top && oneRect.Y >= twoRect.Y))  /*穿过*/
                    {
                        if (collisionProcessing(oneRect, twoRect))
                            actionlast = false;
                    }
                    // 这里想中断y高过它的无意义比较,
                    // 但是必须排序Y,而排序Y必须同X,而这里不是同X(而是同X区间),所以不能中断
                    // 而做到X区间排序,就必须创造一个集合,再排序这个集合,
                    // 会导致每个图元都拥有一次X区间集合,开销更巨大(因此放弃).
                }
                else
                    break;// 因为已经排序了,后续的必然超过 x碰撞区间
            }

            if (actionlast)
                lastProcessing(oneRect);
        }
    }

    #endregion

    #region 转换类型
#if !WinForm
    // 隐式转换(相当于是重载赋值运算符)
    // public static implicit operator Rect(System.Windows.Rect rect)
    // {
    //    return new Rect(rect.Left, rect.Bottom, rect.Right, rect.Top);
    // }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="rect"></param>
    public static implicit operator Rect(System.Drawing.RectangleF rect)
    {
        return new Rect(rect.Left, rect.Bottom, rect.Right, rect.Top);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="rect"></param>
    public static implicit operator Rect(System.Drawing.Rectangle rect)
    {
        return new Rect(rect.Left, rect.Bottom, rect.Right, rect.Top);
    }
#endif

    #region ToString
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public sealed override string ToString()
    {
        return ToString(null, null);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    public string ToString(IFormatProvider? provider)
    {
        return ToString(null, provider);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="format"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    public string ToString(string? format = null, IFormatProvider? formatProvider = null)
    {
        return $"({X.ToString(format, formatProvider)},{Y.ToString(format, formatProvider)})," +
               $"({Right.ToString(format, formatProvider)},{Top.ToString(format, formatProvider)})";

        // return $"X={X.ToString(format, formatProvider)}," +
        //        $"Y={Y.ToString(format, formatProvider)}," +
        //        $"Right={Right.ToString(format, formatProvider)}," +
        //        $"Top={Top.ToString(format, formatProvider)}";
    }

    /*为了红黑树,加入这个*/
    /// <summary>
    /// 
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public int CompareTo(Rect? rect)
    {
        if (rect == null)
            return -1;
        if (X < rect.X)
            return -1;
        else if (X > rect.X)
            return 1;
        else if (Y < rect.Y)/*x是一样的*/
            return -1;
        else if (Y > rect.Y)
            return 1;
        return 0;/*全部一样*/
    }
    #endregion

    #endregion
}

#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
