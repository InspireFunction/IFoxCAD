using System.Diagnostics;

namespace IFoxCAD.Cad;

/// <summary>
/// Linq Distinct 消重比较两点在容差范围内就去除
/// </summary>
public class TolerancePoint2d : IEqualityComparer<Point2d>
{
    readonly double _tolerance;
    public TolerancePoint2d(double tolerance = 1e-6)
    {
        _tolerance = tolerance;
    }

    public bool Equals(Point2d a, Point2d b)//Point3d是struct不会为null
    {
        // 方形限定
        // 在 0~1e-6 范围实现 圆形限定 则计算部分在浮点数6位后,没有啥意义
        // 在 0~1e-6 范围实现 从时间和CPU消耗来说,圆形限定 都没有 方形限定 的好
        if (_tolerance <= 1e-6)
        {
            return Math.Abs(a.X - b.X) <= _tolerance && Math.Abs(a.Y - b.Y) <= _tolerance;
        }
        else
        {
            // 圆形限定
            // DistanceTo 分别对XYZ进行了一次乘法,也是总数3次乘法,然后求了一次平方根
            // (X86.CPU.FSQRT指令用的牛顿迭代法/软件层面可以使用快速平方根....我还以为CPU会采取快速平方根这样的取表操作)
            return a.IsEqualTo(b, new Tolerance(_tolerance, _tolerance));
        }
    }

    public int GetHashCode(Point2d obj)
    {
        //结构体直接返回 obj.GetHashCode(); Point3d ToleranceDistinct3d
        //因为结构体是用可值叠加来判断?或者因为结构体兼备了一些享元模式的状态?
        //而类是构造的指针,所以取哈希值要改成x+y+z..s给Equals判断用,+是会溢出,所以用^
        return (int)obj.X ^ (int)obj.Y;// ^ (int)obj.Z;
    }
}


[Serializable]
[StructLayout(LayoutKind.Sequential)]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
[DebuggerTypeProxy(typeof(Rect))]
public class Rect : IEquatable<Rect>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString("f4");

#pragma warning disable CA2211 // 非常量字段应当不可见
    public static TolerancePoint2d RectTolerance = new(1e-6);
    public static Tolerance CadTolerance = new(1e-6, 1e-6);
#pragma warning restore CA2211 // 非常量字段应当不可见

    #region 字段
    //这里的成员不要用{get}封装成属性,否则会导致跳转了一次函数,
    //10w图元将会从187毫秒变成400毫秒
    //不用 protected 否则子类传入Rect对象进来无法用
    internal double _X;
    internal double _Y;
    internal double _Right;
    internal double _Top;
    #endregion

    #region 成员
    public double X => _X;
    public double Y => _Y;
    public double Left => _X;
    public double Bottom => _Y;
    public double Right => _Right;
    public double Top => _Top;

    public double Width => _Right - _X;
    public double Height => _Top - _Y;
    public double Area
    {
        get
        {
            var ar = (_Right - _X) * (_Top - _Y);
            return ar < 1e-10 ? 0 : ar;
        }
    }

    public Point2d MinPoint => LeftLower;
    public Point2d MaxPoint => RightUpper;
    public Point2d CenterPoint => Midst;

    /// <summary>
    /// 左下Min
    /// </summary>
    public Point2d LeftLower => new(_X, _Y);

    /// <summary>
    /// 左中
    /// </summary>
    public Point2d LeftMidst => new(_X, Midst.Y);

    /// <summary>
    /// 左上
    /// </summary>
    public Point2d LeftUpper => new(_X, _Top);

    /// <summary>
    /// 右上Max
    /// </summary>
    public Point2d RightUpper => new(_Right, _Top);

    /// <summary>
    /// 右中
    /// </summary>
    public Point2d RightMidst => new(_Right, Midst.Y);

    /// <summary>
    /// 右下
    /// </summary>
    public Point2d RightBottom => new(_Right, _Y);

    /// <summary>
    /// 中间
    /// </summary>
    public Point2d Midst => new(((_Right - _X) * 0.5) + _X, ((_Top - _Y) * 0.5) + _Y);

    /// <summary>
    /// 中上
    /// </summary>
    public Point2d MidstUpper => new(Midst.X, _Top);

    /// <summary>
    /// 中下
    /// </summary>
    public Point2d MidstBottom => new(Midst.X, _Y);
    #endregion

    #region 构造
    public Rect() { }

    /// <summary>
    /// 矩形类
    /// </summary>
    /// <param name="left">左</param>
    /// <param name="bottom">下</param>
    /// <param name="right">右</param>
    /// <param name="top">上</param>
    public Rect(double left, double bottom, double right, double top)
    {
        _X = left;
        _Y = bottom;
        _Right = right;
        _Top = top;
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
            _X = Math.Min(p1.X, p3.X);
            _Y = Math.Min(p1.Y, p3.Y);
            _Right = Math.Max(p1.X, p3.X);
            _Top = Math.Max(p1.Y, p3.Y);
        }
        else
        {
            _X = p1.X;
            _Y = p1.Y;
            _Right = p3.X;
            _Top = p3.Y;
        }
    }
    #endregion

    #region 重载运算符_比较
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Rect);
    }
    public bool Equals(Rect? b)
    {
        return this.Equals(b, 1e-6);
    }
    public static bool operator !=(Rect? a, Rect? b)
    {
        return !(a == b);
    }
    public static bool operator ==(Rect? a, Rect? b)
    {
        //此处地方不允许使用==null,因为此处是定义
        if (b is null)
            return a is null;
        else if (a is null)
            return false;
        if (ReferenceEquals(a, b))//同一对象
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
        if (ReferenceEquals(this, b)) //同一对象
            return true;

        return Math.Abs(_X - b._X) < tolerance &&
                Math.Abs(_Right - b._Right) < tolerance &&
                Math.Abs(_Top - b._Top) < tolerance &&
                Math.Abs(_Y - b._Y) < tolerance;
    }

    public override int GetHashCode()
    {
        return (((int)_X ^ (int)_Y).GetHashCode() ^ (int)_Right).GetHashCode() ^ (int)_Top;
    }
    #endregion

    #region 包含
    public bool Contains(Point2d Point2d)
    {
        return Contains(Point2d.X, Point2d.Y);
    }
    public bool Contains(double x, double y)
    {
        return _X <= x && x <= _Right &&
               _Y <= y && y <= _Top;
    }

    /// <summary>
    /// 四个点都在内部就是包含
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public bool Contains(Rect rect)
    {
        return _X <= rect._X && rect._Right <= _Right &&
                _Y <= rect._Y && rect._Top <= _Top;
    }

    /// <summary>
    /// 一个点在内部就是碰撞
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public bool IntersectsWith(Rect rect)
    {
        return rect._X <= _Right && _X <= rect._Right &&
                rect._Top >= _Y && rect._Y <= _Top;
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

    public Point2d[] ToPoints()
    {
        Point2d a = MinPoint;//min
        Point2d b = new(_Right, _Y);
        Point2d c = MaxPoint;//max
        Point2d d = new(_X, _Top);
        return new Point2d[] { a, b, c, d };
    }

    public (Point2d boxMin, Point2d boxRigthDown, Point2d boxMax, Point2d boxLeftUp) ToPoints4()
    {
        Point2d a = MinPoint;//min
        Point2d b = new(_Right, _Y);
        Point2d c = MaxPoint;//max
        Point2d d = new(_X, _Top);
        return (a, b, c, d);
    }

    /// <summary>
    /// 四周膨胀
    /// </summary>
    /// <returns></returns>
    public Rect Expand(double d)
    {
        return new Rect(_X - d, _Y - d, _Right + d, _Top + d);
    }

    /// <summary>
    /// 是否矩形(带角度)
    /// </summary>
    /// <param name="ptList"></param>
    /// <returns></returns>
    public static bool IsRectAngle(List<Point2d>? ptList, double tolerance = 1e-8)
    {
        if (ptList == null)
            throw new ArgumentNullException(nameof(ptList));

        var pts = ptList.ToList();
        /*  消重,不这里设置,否则这不是一个正确的单元测试
            *  //var ptList = pts.Distinct().ToList();
            *  var ptList = pts.DistinctExBy((a, b) => a.DistanceTo(b) < 1e-6).ToList();
            */
        if (ptList.Count == 5)
        {
            //首尾点相同移除最后
            if (pts[0].IsEqualTo(pts[^1], CadTolerance))
                pts.RemoveAt(pts.Count - 1);
        }
        if (pts.Count != 4)
            return false;

        //最快的方案
        //点乘求值法:(为了处理 正梯形/平行四边形 需要三次)
        //这里的容差要在1e-8内,因为点乘的三次浮点数乘法会令精度变低
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
    public static bool IsRect(List<Point2d>? ptList, double tolerance = 1e-10)
    {
        if (ptList == null)
            throw new ArgumentNullException(nameof(ptList));

        var pts = ptList.ToList();
        if (ptList.Count == 5)
        {
            //首尾点相同移除最后
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
        //var zMin = double.MaxValue;
        //var zMax = double.MinValue;

        pts.ForEach(p => {
            xMin = Math.Min(p.X, xMin);
            xMax = Math.Max(p.X, xMax);
            yMin = Math.Min(p.Y, yMin);
            yMax = Math.Max(p.Y, yMax);
            //zMin = Math.Min(p.Z, zMin);
            //zMax = Math.Max(p.Z, zMax);
        });
        return (new Point2d(xMin, yMin), new Point2d(xMax, yMax));
    }

    /// <summary>
    /// 矩形点序逆时针排列,将min点[0],max点是[3](带角度)
    /// </summary>
    /// <param name="pts"></param>
    /// <returns></returns>
    public static bool RectAnglePointOrder(List<Point2d>? pts)
    {
        if (pts == null)
            throw new ArgumentNullException(nameof(pts));

        if (!Rect.IsRectAngle(pts))
            return false;

        //获取min和max点(非包围盒)
        pts = pts.OrderBy(a => a.X).ThenBy(a => a.Y).ToList();
        var minPt = pts.First();
        var maxPt = pts.Last();
        var link = new LoopList<Point2d>();
        link.AddRange(pts);

        pts.Clear();
        //排序这四个点,左下/右下/右上/左上
        var node = link.Find(minPt);
        for (int i = 0; i < 4; i++)
        {
            pts.Add(node!.Value);
            node = node.Next;
        }
        //保证是逆时针
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
    /// <returns>b点在oa的逆时针<see cref="true"/></returns>
    static bool CrossAclockwise(Point2d o, Point2d a, Point2d b)
    {
        return Cross(o, a, b) > -1e-6;//浮点数容差考虑
    }

#if !WinForm
    public Autodesk.AutoCAD.DatabaseServices.Entity ToPolyLine()
    {
        var bv = new List<BulgeVertex>();
        var pts = ToPoints();
        Polyline pl = new();
        pl.SetDatabaseDefaults();
        pts.ForEach((i, vertex) => {
            pl.AddVertexAt(i, vertex, 0, 0, 0);
        });
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
        //先排序X:不需要Y排序,因为Y的上下浮动不共X .ThenBy(a => a.Box.Y)
        //因为先排序就可以有序遍历x区间,超过就break,达到更快
        box = box.OrderBy(a => a._X).ToList();

        //遍历所有图元
        for (int i = 0; i < box.Count; i++)
        {
            var oneRect = box[i];
            if (firstProcessing(oneRect))
                continue;

            bool actionlast = true;

            //搜索范围要在 one 的头尾中间的部分
            for (int j = i + 1; j < box.Count; j++)
            {
                var twoRect = box[j];
                //x碰撞:矩形2的Left 在 矩形1[Left-Right]闭区间;穿过的话,也必然有自己的Left因此不需要处理
                if (oneRect._X <= twoRect._X && twoRect._X <= oneRect._Right)
                {
                    //y碰撞,那就是真的碰撞了
                    if ((oneRect._Top >= twoRect._Top && twoRect._Top >= oneRect._Y) /*包容上边*/
                     || (oneRect._Top >= twoRect._Y && twoRect._Y >= oneRect._Y)     /*包容下边*/
                     || (twoRect._Top >= oneRect._Top && oneRect._Y >= twoRect._Y))  /*穿过*/
                    {
                        if (collisionProcessing(oneRect, twoRect))
                            actionlast = false;
                    }
                    //这里想中断y高过它的无意义比较,
                    //但是必须排序Y,而排序Y必须同X,而这里不是同X(而是同X区间),所以不能中断
                    //而做到X区间排序,就必须创造一个集合,再排序这个集合,
                    //会导致每个图元都拥有一次X区间集合,开销更巨大(因此放弃).
                }
                else
                    break;//因为已经排序了,后续的必然超过 x碰撞区间
            }

            if (actionlast)
                lastProcessing(oneRect);
        }
    }

    #endregion

    #region 转换类型
#if !WinForm
    // 隐式转换(相当于是重载赋值运算符)
    public static implicit operator Rect(System.Windows.Rect rect)
    {
        return new Rect(rect.Left, rect.Bottom, rect.Right, rect.Top);
    }
    public static implicit operator Rect(System.Drawing.RectangleF rect)
    {
        return new Rect(rect.Left, rect.Bottom, rect.Right, rect.Top);
    }
    public static implicit operator Rect(System.Drawing.Rectangle rect)
    {
        return new Rect(rect.Left, rect.Bottom, rect.Right, rect.Top);
    }
#endif

    #region ToString
    public sealed override string ToString()
    {
        return ToString(null, null);
    }
    public string ToString(IFormatProvider? provider)
    {
        return ToString(null, provider);
    }
    public string ToString(string? format = null, IFormatProvider? formatProvider = null)
    {
        return $"({_X.ToString(format, formatProvider)},{_Y.ToString(format, formatProvider)})," +
               $"({_Right.ToString(format, formatProvider)},{_Top.ToString(format, formatProvider)})";

        // return $"X={_X.ToString(format, formatProvider)}," +
        //        $"Y={_Y.ToString(format, formatProvider)}," +
        //        $"Right={_Right.ToString(format, formatProvider)}," +
        //        $"Top={_Top.ToString(format, formatProvider)}";
    }
    #endregion

    #endregion


}
