/*
 * 四叉树维基百科  http://en.wikipedia.org/wiki/Quadtree
 * 四叉树是一种分区空间的算法,更快找出内部或外部给定区域.
 * 通过一个正交矩形边界进行中心点分裂四个正交矩形,
 * 插入时候会一直分裂四个正交矩形,
 * 当分裂四个节点都无法单独拥有 图元包围盒 就停止分裂,并且你属于这四个节点的父亲.
 * (不包含就是面积少了,就这么一句话看代码看半天),
 * 还可以通过限制树的深度实现加速.
 *
 * 第一版: https://www.codeproject.com/script/Articles/ViewDownloads.aspx?aid=30535
 *
 * 第二版: 找邻居
 * https://blog.csdn.net/dive_shallow/article/details/112438050
 * https://geidav.wordpress.com/2017/12/02/advanced-octrees-4-finding-neighbor-nodes/
 *
 * 1.根节点:控制根节点从而控制所有节点
 * 2.子节点:包含自身根节点,插入矩形的时候进行递归分裂自身,和实现查找.
 * 3.接口:约束都要有正交矩形,否则无法调用"包含"方法
 * 4.选择模式:模仿cad的窗选和框选
 */
namespace IFoxCAD.Cad;

/// <summary>
/// 根节点控制器
/// </summary>
/// <typeparam name="TEntity">类型接口约束必须有正交矩形</typeparam>
public class QuadTree<TEntity> where TEntity : QuadEntity
{
    #region 成员
    /// <summary>
    /// 根节点
    /// </summary>
    QuadTreeNode<TEntity> _rootNode;

    /// <summary>
    /// 四叉树节点的数目
    /// </summary>
    public int Count { get => _rootNode.CountSubTree; }

    /// <summary>
    /// 点容器(红黑树)
    /// </summary>
    SortedSet<TEntity> _points;
    #endregion

    #region 构造
    /// <summary>
    /// 四叉树根节点控制器
    /// </summary>
    /// <param name="rect">四叉树矩形范围</param>
    public QuadTree(Rect rect)
    {
        _rootNode = new QuadTreeNode<TEntity>(rect, null, 0);// 初始化根节点
        _points = [];
    }
    #endregion

    #region 方法
    /// <summary>
    /// 通过根节点插入数据项
    /// </summary>
    /// <param name="ent"></param>
    public void Insert(TEntity ent)
    {
        /*
         * 图元点 是不分裂空间的,加入一个红黑树内部.
         */
        if (ent.IsPoint)
        {
            _points.Add(ent);
            return;
        }

        while (!_rootNode.Contains(ent))
        {
            /*
             * 四叉树插入时候,如果超出根边界,就需要扩展
             * 扩展时候有一个要求,当前边界要作为扩展边界的一个象限,也就是反向分裂
             *
             * 创建新根,计算原根在新根的位置,
             * 替换指针:获取新分裂的节点的父节点,判断它哪个儿子是它,
             * 替换之后可能仍然不包含图元边界,再循环计算.
             */
            var sq_Left = _rootNode._X;
            var sq_Botton = _rootNode._Y;
            var sq_Right = _rootNode._Right;
            var sq_Top = _rootNode._Top;
            if (ent._Y >= _rootNode._Y)// 上↑增殖
            {
                if (ent._X >= _rootNode._X)
                {
                    // 右上↗增殖
                    sq_Right += _rootNode.Width;
                    sq_Top += _rootNode.Height;
                }
                else
                {
                    // 左上↖增殖
                    sq_Left -= _rootNode.Width;
                    sq_Top += _rootNode.Height;
                }
            }
            else// 在下↓
            {
                if (ent._X >= _rootNode._X)
                {
                    // 右下↘增殖
                    sq_Right += _rootNode.Width;
                    sq_Botton -= _rootNode.Height;
                }
                else
                {
                    // 左下↙增殖
                    sq_Left -= _rootNode.Width;
                    sq_Botton -= _rootNode.Height;
                }
            }
            // 扩大2次方
            var rectSquare = new Rect(sq_Left, sq_Botton, sq_Right, sq_Top);

            // 四叉树的旧根要作为四分之一插入
            // 新根中计算原根
            // 把 旧根节点 连接到 新根节点 上面,然后新根成为根
            var newRoot = new QuadTreeNode<TEntity>(rectSquare, null, 0);
            var insert = newRoot.Insert(_rootNode);
            if (insert is null)
                throw new("四叉树:新根尺寸不对");
            if (!insert.Equals(_rootNode))
                throw new("四叉树:新旧节点大小不一致,无法连接");

            var insPar = insert.Parent;
            _rootNode.Parent = insPar;
            if (insPar is null)
                return;

            if (_rootNode.Equals(insPar.RightTopTree))
                insPar.RightTopTree = _rootNode;
            else if (_rootNode.Equals(insPar.RightBottomTree))
                insPar.RightBottomTree = _rootNode;
            else if (_rootNode.Equals(insPar.LeftBottomTree))
                insPar.LeftBottomTree = _rootNode;
            else if (_rootNode.Equals(insPar.LeftTopTree))
                insPar.LeftTopTree = _rootNode;
            else
                throw new("四叉树:新节点不对,无法连接");

            // 其后的子节点层数全部增加层数,
            // 要加多少层取决于当前根边界属于新根边界的所在层
            var depth = insert.Depth;
            if (depth == 0)
                throw new("四叉树:插入节点是0,造成错误");
            _rootNode.ForEach(node => {
                node.Depth += depth;
                return false;
            });

            // 交换根控制
            _rootNode = newRoot;
        }

        _rootNode.Insert(ent);
    }


    /// <summary>
    /// 查询四叉树,返回给定区域的数据项
    /// </summary>
    /// <param name="rect">矩形选区查询</param>
    /// <param name="selectMode">查询模式</param>
    /// <returns>查询结果列表</returns>
    public List<TEntity> Query(Rect rect, QuadTreeSelectMode selectMode = QuadTreeSelectMode.IntersectsWith)
    {
        QuadTreeEvn.SelectMode = selectMode;

        List<TEntity> results = [];
        // 选择图元
        _rootNode.Query(rect, results);
        // 选择点
        using var ptge = _points.GetEnumerator();
        switch (selectMode)
        {
            case QuadTreeSelectMode.IntersectsWith:
            case QuadTreeSelectMode.Contains:
            /* 由于红黑树的方法 _points.GetViewBetween()
             * 过滤只能过滤X区间,Y区间还是要过滤,
             * 那么我就只能用这样的方法加速了
             *
             * 而更好的方式是不用红黑树,去加入一个点云数据来进行,可谓是编程无极限....
             */
            while (ptge.MoveNext())
            {
                var ptEnt = ptge.Current;
                if (ptEnt != null && rect._X <= ptEnt._X && ptEnt._X <= rect._Right)
                {
                    if (rect._Y <= ptEnt._Y && ptEnt._Y <= rect.Top)
                        results.Add(ptEnt);
                }
                else if (ptEnt != null && ptEnt._X > rect._Right)
                    break;// 超过后面范围就break,因为红黑树已经排序
            }
            break;
            default:
            throw new("四叉树:" + nameof(selectMode));
        }
        return results;
    }

    /// <summary>
    /// 删除子节点
    /// </summary>
    /// <param name="rect">根据范围删除</param>
    public void Remove(Rect rect)
    {
        _rootNode.Remove(rect);
    }

    /// <summary>
    /// 删除子节点
    /// </summary>
    /// <param name="ent">根据图元删除</param>
    public void Remove(TEntity ent)
    {
        _rootNode.Remove(ent);
    }

    /// <summary>
    /// 找到附近节点图元
    /// </summary>
    [Obsolete("找附近节点的并不是最近的图元")]
    public TEntity? FindNeibor(Rect rect, QuadTreeFindMode findMode)
    {
        return _rootNode.FindNeibor(rect, findMode);
    }

    /// <summary>
    /// 找到附近图元
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public TEntity? FindNearEntity(Rect rect)
    {
        return _rootNode.FindNearEntity(rect);
    }

    /// <summary>
    /// 执行四叉树中特定的行为
    /// </summary>
    /// <param name="action"></param>
    public void ForEach(QTAction action)
    {
        _rootNode.ForEach(action);
    }

    /// <summary>
    /// 委托:四叉树节点上执行一个操作
    /// </summary>
    /// <param name="obj"></param>
    public delegate bool QTAction(QuadTreeNode<TEntity> obj);
    #endregion
}