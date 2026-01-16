namespace IFoxCAD.Cad;

/// <summary>
/// 子节点
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class QuadTreeNode<TEntity>
    : Rect
    where TEntity : QuadEntity
{
    #region 成员
    /// <summary>
    /// 子节点:第一象限:右上↗
    /// </summary>
    public QuadTreeNode<TEntity>? RightTopTree;
    /// <summary>
    /// 子节点:第二象限:左上↖
    /// </summary>
    public QuadTreeNode<TEntity>? LeftTopTree;
    /// <summary>
    /// 子节点:第三象限:左下↙
    /// </summary>
    public QuadTreeNode<TEntity>? LeftBottomTree;
    /// <summary>
    /// 子节点:第四象限:右下↘
    /// </summary>
    public QuadTreeNode<TEntity>? RightBottomTree;
    /// <summary>
    /// 所有子节点
    /// </summary>
    QuadTreeNode<TEntity>[] Nodes
    {
        get
        {
            return
            [
                RightTopTree!,
                 LeftTopTree!,
                 LeftBottomTree!,
                 RightBottomTree!
            ];
        }
    }
    /// <summary>
    /// 所有子节点是空的
    /// </summary>
    bool NodesIsEmpty => RightTopTree is null && LeftTopTree is null && LeftBottomTree is null && RightBottomTree is null;

    /// <summary>
    /// 父节点
    /// </summary>
    public QuadTreeNode<TEntity>? Parent;
    /// <summary>
    /// 节点的在四叉树的深度
    /// </summary>
    public int Depth;

    // 注意,内容没有限制:这不是 impement 四叉树的标准方法
    /// (节点图元是交叉线压着的,并不是矩形范围内全部,因为这是四叉树的特性决定)
    /// <summary>
    /// 本节点:内容
    /// </summary>
    public List<TEntity> Contents;

    /// <summary>
    /// 本节点和旗下所有子节点:内容群
    /// </summary>
    public void ContentsSubTree(List<TEntity> results)
    {
        if (Contents is null)
            return;
        results.AddRange(Contents);
        var nodes = Nodes;
        for (var i = 0; i < nodes.Length; i++)
            nodes[i]?.ContentsSubTree(results);
    }

    /// <summary>
    /// 本节点和旗下所有子节点:内容群数量
    /// </summary>
    public int CountSubTree
    {
        get
        {
            if (Contents is null)
                return 0;
            var count = Contents.Count;

            var nodes = Nodes;
            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node is null)
                    continue;
                count += node.CountSubTree;
            }
            return count;
        }
    }
    #endregion

    #region 构造
    /// <summary>
    /// 四叉树节点
    /// </summary>
    /// <param name="box">当前节点边界</param>
    /// <param name="parent">父节点</param>
    /// <param name="depth">节点深度</param>
    public QuadTreeNode(Rect box, QuadTreeNode<TEntity>? parent, int depth)
    {
        _X = box._X;
        _Y = box._Y;
        _Right = box._Right;
        _Top = box._Top;

        Parent = parent;
        Depth = depth;
        Contents = [];
    }
    #endregion

    #region 增
    /// <summary>
    /// 将原有节点插入用
    /// </summary>
    /// <param name="rect"></param>
    internal QuadTreeNode<TEntity>? Insert(Rect rect)
    {
        if (!Contains(rect))
            return null;

        // 四叉树分裂,将当前节点分为四个子节点
        if (NodesIsEmpty)
            CreateChildren();

        // 当前节点边界 包含 图元包围盒 就插入
        // 退出递归:4个节点都不完全包含
        // 4个节点的上层
        var nodes = Nodes;
        for (var i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (node is null)
                continue;

            if (node.Equals(rect))
            {
                rect = node;
                return node.Insert(rect);
            }
        }
        return this;
    }

    /// <summary>
    /// 将数据项递归插入四叉树
    /// </summary>
    /// <param name="ent"></param>
    public QuadTreeNode<TEntity>? Insert(TEntity ent)
    {
        if (!Contains(ent))
        {
            // Debugx.Printl("不在四叉树边界范围");
            // Trace.WriteLine("不在四叉树边界范围");
            return null;
        }

        // if (ent.IsPoint)
        // {
        //     // 找到最后一层包含它的节点,然后加入它
        //     // 因此是跳过分裂矩形的,以免造成无限递归
        //     var minNode = GetMinNode(ent);
        //     minNode.Contents.Add(ent);
        //     return minNode;
        // }

#if true2
        // 方案二:
        // 内容数超过才分裂,防止树深度过高,但是多选过滤时候慢一点
        if (Contents.Count > QuadTreeEvn.QuadTreeContentsCountSplit)
        {
            // 分裂出四个子节点
            if (_nodesIsEmpty)
            {
                CreateChildren();
                // 分裂之后将当前层的内容扔到四个子节点,
                // 如果被压着,那么就不会扔到下面
                for (int i = Contents.Count - 1; i >= 0; i--)
                {
                    var minNode = GetMinNode(Contents[i].Box);
                    minNode.Contents.Add(Contents[i]);
                    Contents.RemoveAt(i);
                }
            }
            else
            {
                // 没有分裂的话,就递归
                // 退出递归:4个节点都不完全包含,内容就是他们的父亲
                var nodes = _Nodes;
                for (int i = 0; i < nodes.Length; i++)
                {
                    var node = nodes[i];
                    if (node is null)
                        continue;

                    // 这里需要中断.(匿名方法ForEach无法中断,会造成父节点加入内容)
                    if (node.Contains(ent))
                        return node.Insert(ent);
                }
            }
        }
#else
        // 方案一:分裂到最细节点

        // 分裂出四个子节点
        if (NodesIsEmpty)
            CreateChildren();

        // 4个子节点开始递归
        // 退出递归:4个节点都不完全包含,内容就是他们的父亲
        var nodes = Nodes;
        for (var i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (node is null)
                continue;
            // 这里需要中断.(匿名方法ForEach无法中断,会造成父节点加入内容)
            if (node.Contains(ent))
                return node.Insert(ent);
        }
#endif

        // 为什么要用容器?
        // 相同包围盒或者四叉树分割线压着多个.
        Contents.Add(ent);
        return this;
    }

    /// <summary>
    /// 创建子节点
    /// </summary>
    void CreateChildren()
    {
        // 最小面积控制节点深度,但是这样可能导致树分成高,引起爆栈
        if (Depth > QuadTreeEvn.QuadTreeMaximumDepth)
            return;
        var recs = RectSplit(this);
        var de = Depth + 1;
        RightTopTree = new QuadTreeNode<TEntity>(recs[0], this, de);
        LeftTopTree = new QuadTreeNode<TEntity>(recs[1], this, de);
        LeftBottomTree = new QuadTreeNode<TEntity>(recs[2], this, de);
        RightBottomTree = new QuadTreeNode<TEntity>(recs[3], this, de);
    }

    /// <summary>
    /// 矩形分裂为四个
    /// </summary>
    /// <param name="box"></param>
    /// <returns></returns>
    static Rect[] RectSplit(Rect box)
    {
        var halfWidth = box.Width / 2.0;
        var halfHeight = box.Height / 2.0;

        var upperRight = new Rect(box._X + halfWidth, box._Y + halfHeight, box._Right, box._Top);
        var upperLeft = new Rect(box._X, box._Y + halfHeight, box._Right - halfWidth, box._Top);
        var lowerleft = new Rect(box._X, box._Y, box._Right - halfWidth, box._Top - halfHeight);// 基础
        var lowerRight = new Rect(box._X + halfWidth, box._Y, box._Right, box._Top - halfHeight);

        // 依照象限顺序输出
        return [upperRight, upperLeft, lowerleft, lowerRight];
    }
    #endregion

    #region 删
    /// <summary>
    /// 删除图元
    /// </summary>
    /// <param name="easeEnt">根据图元删除</param>
    public bool Remove(TEntity easeEnt)
    {
        // 通过图元id删除无疑是非常低效的,
        // 1.相当于在所有的容器查找它,但是移除只会移除一次,
        //  因此必须要求图元只会加入一次,才能中断检索剩余分支.
        // 2.这个代价还是太高,因此我们还是要默认条件,图元载入一次之后,不再改动.
        // 3.不再改动也不太合理,因为cad图元还是可以修改的

        // 1.处理内容
        if (Contents.Remove(easeEnt))
        {
            if (CountSubTree == 0)
                Clear(this);
            return true;
        }

        // 2.递归子节点移除
        var nodes = Nodes;
        for (var i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (node is null)
                continue;
            if (node.Remove(easeEnt))     // 递归进入子节点删除内容
                return true;              // 删除成功就中断其他节点的搜索
        }
        return false;
    }

    /// <summary>
    /// 递归进入最下层节点,然后开始清理
    /// </summary>
    /// <param name="node"></param>
    void Clear(QuadTreeNode<TEntity> node)
    {
        var nodes = Nodes;
        for (var i = 0; i < nodes.Length; i++)
            nodes[i]?.Clear(nodes[i]);

        node.Contents.Clear();
        // node.Contents = null;// 重复加入时候会出错
        node.RightTopTree = null;
        node.LeftTopTree = null;
        node.LeftBottomTree = null;
        node.RightBottomTree = null;
        node.Parent = null;
        // node.Box = zoreRect;
    }

    /// <summary>
    /// 删除子节点内容
    /// </summary>
    /// <param name="queryArea">根据范围删除</param>
    public void Remove(Rect queryArea)
    {
        // 本节点内容移除
        if (Contents is not null && Contents.Count > 0)// 从最上层的根节点开始进入
        {
            for (var i = Contents.Count - 1; i >= 0; i--)
            {
                var ent = Contents[i];
                // 移除之后,如果容器是0,那么这里不能直接 Contents=null,
                // 因为此节点下面可能还有节点,
                // 需要判断了其后数量0才可以清理.
                // 否则其后还有内容,那么此节点就是仍然可以用的.
                if (queryArea.Contains(ent))
                    Contents.Remove(ent);
            }
        }

        // 同插入一样
        // 跳到指定节点再搜索这个节点下面的图元
        var nodes = Nodes;
        for (var i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (node is null)
                continue;
            if (node.NodesIsEmpty)
                continue;

            // 此节点边界 完全包含 查询区域,则转到该节点,并跳过其余节点(打断此循环)
            if (node.Contains(queryArea))
            {
                node.Remove(queryArea);
                break;
            }
            // 查询区域 完全包含 此节点边界,提取此节点全部内容
            // 跳过分析碰撞,并继续循环搜索其他节点
            if (queryArea.Contains(node))
            {
                node.Clear(node);
                continue;
            }
            // 查询区域 与 此节点四边形边线碰撞 查询该四边形中,并继续循环搜索其他节点
            // 1,角点碰撞 2,边界碰撞
            if (node.IntersectsWith(queryArea))
                node.Remove(queryArea);
        }

        // 本节点内容移除之后,旗下还有内容的话,
        // 会跳过此处,再进入子节点进行递归,直到最后一个节点
        if (CountSubTree == 0)
            Clear(this);
    }
    #endregion

    #region 查
    /// <summary>
    /// 查询范围内的实体
    /// </summary>
    /// <param name="queryArea">查询矩形</param>
    /// <param name="results">查询结果</param>
    public void Query(Rect queryArea, List<TEntity> results)
    {
        GetCurrentContents(queryArea, results);

        // 遍历子节点
        var nodes = Nodes;
        for (var i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (node is null)
                continue;
            // 子节点的4个子节点都是空的,
            // 那么表示元素会在子节点这一层啊...
            if (node.NodesIsEmpty)
                continue;

            // 此节点边界 完全包含 查询区域,则转到该节点,并跳过其余节点(打断此循环)
            if (node.Contains(queryArea))
            {
                node.Query(queryArea, results);
                break;
            }
            // 查询区域 完全包含 此节点边界,提取此节点全部内容
            // 跳过分析碰撞,并继续循环搜索其他节点
            if (queryArea.Contains(node))
            {
                node.ContentsSubTree(results);
                continue;
            }
            // 查询区域 与 此节点四边形边线碰撞 查询该四边形中,并继续循环搜索其他节点
            // 1,角点碰撞 2,边界碰撞
            if (node.IntersectsWith(queryArea))
                node.Query(queryArea, results);
        }
    }

    /// <summary>
    /// 获取本节点内容
    /// </summary>
    /// <param name="queryArea"></param>
    /// <param name="results"></param>
    void GetCurrentContents(Rect queryArea, List<TEntity> results)
    {
        // 遍历当前节点内容,加入方式取决于碰撞模式
        if (QuadTreeEvn.SelectMode == QuadTreeSelectMode.IntersectsWith)
        {
            for (var i = 0; i < Contents.Count; i++)
            {
                var ent = Contents[i];
                if (queryArea.IntersectsWith(ent))
                    results.Add(ent);
            }
        }
        else
        {
            for (var i = 0; i < Contents.Count; i++)
            {
                var ent = Contents[i];
                if (queryArea.Contains(ent))
                    results.Add(ent);
            }
        }
    }

    /// <summary>
    /// 找临近图元
    /// </summary>
    /// <param name="queryArea">查找矩形</param>
    /// <returns></returns>
    public TEntity? FindNearEntity(Rect queryArea)
    {
        TEntity? resultEntity = default;
        // 1.找到 查找矩形 所在的节点,利用此节点的矩形.
        var queryNode = GetMinNode(queryArea);
        var queryAreaCenter = queryArea.CenterPoint;

        // 2.从根开始搜索
        //  如果搜索父亲的父亲的...内容群,它不是距离最近的,只是节点(亲属关系)最近
        //  储存找过的<图元,距离>
        var entDic = new Dictionary<TEntity, double>();

        var old = QuadTreeEvn.SelectMode;
        QuadTreeEvn.SelectMode = QuadTreeSelectMode.IntersectsWith;
        while (true)
        {
            // 循环找父节点大小
            var hw = queryNode.Width / 2.0;
            var hh = queryNode.Height / 2.0;
            // 3.利用选区中心扩展一个节点边界大小的矩形.从而选择图元
            //  再判断图元的与目标的距离,找到最小距离,即为最近
            var minPt = new Point2d(queryAreaCenter.X - hw, queryAreaCenter.Y - hh);
            var maxPt = new Point2d(queryAreaCenter.X + hw, queryAreaCenter.Y + hh);
            List<TEntity> ents = [];
            Query(new Rect(minPt, maxPt), ents);
            for (var i = 0; i < ents.Count; i++)
            {
                var ent = ents[i];
                if (entDic.ContainsKey(ent))
                    continue;
                var dis = ent.CenterPoint.GetDistanceTo(queryAreaCenter);
                if (dis > 1e-6)// 剔除本身
                    entDic.Add(ent, dis);
            }
            if (entDic.Count > 0)
            {
                resultEntity = entDic.OrderBy(a => a.Value).First().Key;
                break;
            }
            if (queryNode.Parent is null)// 最顶层就退出
                break;
            queryNode = queryNode.Parent;// 利用父节点矩形进行变大选区
        }
        QuadTreeEvn.SelectMode = old;
        return resultEntity;
    }

    /// <summary>
    /// 找临近节点的图元
    /// </summary>
    /// <param name="queryArea">查找矩形</param>
    /// <param name="findMode">查找什么方向</param>
    /// <returns></returns>
    [Obsolete("找附近节点的并不是最近的图元")]
    public TEntity? FindNeibor(Rect queryArea, QuadTreeFindMode findMode)
    {
        TEntity? resultEntity = default;
        // 1.找到 查找矩形 所在的节点,利用此节点的矩形.
        // 2.利用节点矩形是分裂的特点,边和边必然贴合.
        // 3.找到方向 findMode 拥有的节点,然后查找节点的内容
        var queryNode = GetMinNode(queryArea);

        var whileFlag = true;
        // 同一个节点可能包含邻居,因为四叉树的加入是图元压线,
        // 那么就在这里搜就得了,用中心点决定空间位置
        // 但是本空间的图元可能都比它矮,无法满足条件
        if (queryNode.CountSubTree > 1)
        {
            resultEntity = GetNearestNeighbor(queryNode, findMode, queryArea);
            if (resultEntity is null || resultEntity.CenterPoint == queryArea.CenterPoint)
                whileFlag = true;
            else
                whileFlag = false;
        }

        while (whileFlag)
        {
            // 同一个父节点是临近的,优先获取 兄弟节点 的内容.
            // 循环了第二次是北方兄弟的节点,
            // 但是这不是一个找到临近图元的方法,
            // 因为临近的可能是父亲的父亲的父亲...另一个函数 FindNearEntity 写
            // 本方案也仅仅作为找北方节点
            var parent = queryNode.Parent;
            if (parent is not null)
            {
                switch (findMode)
                {
                    case QuadTreeFindMode.Top:
                    {
                        // 下格才获取上格,否则导致做了无用功,上格就直接获取邻居了
                        if (parent.LeftBottomTree == queryNode || parent.RightBottomTree == queryNode)
                            resultEntity = GetNearestNeighbor(parent, findMode, queryArea);
                    }
                    break;
                    case QuadTreeFindMode.Bottom:
                    {
                        if (parent.LeftTopTree == queryNode || parent.RightTopTree == queryNode)
                            resultEntity = GetNearestNeighbor(parent, findMode, queryArea);
                    }
                    break;
                    case QuadTreeFindMode.Left:
                    {
                        if (parent.RightTopTree == queryNode || parent.RightBottomTree == queryNode)
                            resultEntity = GetNearestNeighbor(parent, findMode, queryArea);
                    }
                    break;
                    case QuadTreeFindMode.Right:
                    {
                        if (parent.LeftTopTree == queryNode || parent.LeftBottomTree == queryNode)
                            resultEntity = GetNearestNeighbor(parent, findMode, queryArea);
                    }
                    break;
                }
            }
            if (resultEntity is not null)
                break;

            // 通过 所在节点 找 邻居节点,
            // 拿到 邻居节点 下面的所有内容(图元)
            // 内容可能是空的,再从邻居那往北找...如果找到了四叉树最外层,仍然没有内容,退出循环
            var neiborNode = FindNeiborNode(queryNode, findMode);
            if (neiborNode is null)
                continue;
            if (neiborNode.CountSubTree > 0)
            {
                resultEntity = GetNearestNeighbor(neiborNode, findMode, queryArea);
                break;
            }
            if (neiborNode.Parent is null)// 如果找到了四叉树最外层,仍然没有内容,退出循环
                break;
            queryNode = neiborNode;
        }

        return resultEntity;
    }

    /// <summary>
    /// 查找节点的(本内容和子内容)与(查找面积)矩形中点对比,找到最近一个内容
    /// </summary>
    /// <param name="queryArea">查找面积</param>
    /// <param name="findMode">查找方向</param>
    /// <param name="queryNode">查找节点</param>
    /// <returns></returns>
    static TEntity? GetNearestNeighbor(QuadTreeNode<TEntity> queryNode,
                                       QuadTreeFindMode findMode,
                                       Rect queryArea)
    {
        TEntity? results = default;

        List<TEntity> lst = [];
        var qcent = queryArea.CenterPoint;

        switch (findMode)
        {
            case QuadTreeFindMode.Top:
            {
                // 取出Y比queryArea的还大的一个,是最近的那个
                var qy = qcent.Y;
                queryNode.ContentsSubTree(lst);
                lst.ForEach(ent => {
                    if (ent.CenterPoint.Y > qy)
                        lst.Add(ent);
                });
                lst = lst.OrderBy(ent => ent.CenterPoint.Y).ToList();
            }
            break;
            case QuadTreeFindMode.Bottom:
            {
                var qy = qcent.Y;
                queryNode.ContentsSubTree(lst);
                lst.ForEach(ent => {
                    if (ent.CenterPoint.Y < qy)
                        lst.Add(ent);
                });
                lst = lst.OrderByDescending(ent => ent.CenterPoint.Y).ToList();
            }
            break;
            case QuadTreeFindMode.Left:
            {
                var qx = qcent.Y;
                queryNode.ContentsSubTree(lst);
                lst.ForEach(ent => {
                    if (ent.CenterPoint.X > qx)
                        lst.Add(ent);
                });
                lst = lst.OrderBy(ent => ent.CenterPoint.X).ToList();
            }
            break;
            case QuadTreeFindMode.Right:
            {
                var qx = qcent.Y;
                queryNode.ContentsSubTree(lst);
                lst.ForEach(ent => {
                    if (ent.CenterPoint.X < qx)
                        lst.Add(ent);
                });
                lst = lst.OrderByDescending(ent => ent.CenterPoint.X).ToList();
            }
            break;
        }

        if (lst.Count > 0)
            return lst[0];// 可能就是本体重叠
        return results;
    }

    /// <summary>
    /// 找包含它的最小分支
    /// </summary>
    /// <param name="queryArea">查询的矩形</param>
    /// <returns>节点</returns>
    QuadTreeNode<TEntity> GetMinNode(Rect queryArea)
    {
        var nodes = Nodes;
        for (var i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (node is null)
                continue;

            // 边界包含查询面积,那么再递归查询,
            // 直到最后四个都不包含,那么上一个就是图元所在节点
            if (node.Contains(queryArea))
                return node.GetMinNode(queryArea);// 中断后面的范围,才可以返回正确的
        }
        return this;
    }

    /// <summary>
    /// 四叉树找邻居节点(相同或更大)
    /// </summary>
    /// <param name="tar">源节点</param>
    /// <param name="findMode">方向</param>
    /// <returns></returns>
    QuadTreeNode<TEntity>? FindNeiborNode(QuadTreeNode<TEntity> tar, QuadTreeFindMode findMode)
    {
        var parent = tar.Parent;
        if (parent is null)
            return null;
        switch (findMode)
        {
            case QuadTreeFindMode.Top:
            {
                // 判断当前节点在父节点的位置,如果是在 下格 就取对应的 上格
                if (tar == parent.LeftBottomTree)
                    return parent.LeftTopTree;
                if (tar == parent.RightBottomTree)
                    return parent.RightTopTree;
                // 否则就是上格
                // 找父节点的北方邻居..也就是在爷节点上面找
                // 递归,此时必然是 下格,就必然返回 上格,然后退出递归
                var parentNeibor = FindNeiborNode(parent, QuadTreeFindMode.Top);
                // 父节点的北方邻居 无 子节点
                if (parentNeibor is null || parentNeibor.RightTopTree is null)
                    return parentNeibor;// 返回父节点的北方邻居,比较大
                                        // 父节点的北方邻居 有 子节点,剩下条件就只有这两

                // 如果直接返回,那么是(相同或更大),
                // 而找邻近图元需要的是这个(相同或更大)下面的图元,在外面对这个格子内图元用坐标分析就好了
                if (tar == parent.LeftTopTree)
                    return parentNeibor.LeftBottomTree;
                return parentNeibor.RightBottomTree;
            }
            case QuadTreeFindMode.Bottom:
            {
                if (tar == parent.LeftTopTree)
                    return parent.LeftBottomTree;
                if (tar == parent.RightTopTree)
                    return parent.RightBottomTree;
                var parentNeibor = FindNeiborNode(parent, QuadTreeFindMode.Bottom);
                if (parentNeibor is null || parentNeibor.RightTopTree is null)
                    return parentNeibor;
                if (tar == parent.LeftBottomTree)
                    return parentNeibor.LeftTopTree;
                return parentNeibor.RightTopTree;
            }
            case QuadTreeFindMode.Right:
            {
                if (tar == parent.LeftTopTree)
                    return parent.RightTopTree;
                if (tar == parent.LeftBottomTree)
                    return parent.RightBottomTree;
                var parentNeibor = FindNeiborNode(parent, QuadTreeFindMode.Right);
                if (tar == parent.RightTopTree)
                    return parentNeibor?.LeftTopTree;
                return parentNeibor?.LeftBottomTree;
            }
            case QuadTreeFindMode.Left:
            {
                if (tar == parent.RightTopTree)
                    return parent.LeftTopTree;
                if (tar == parent.RightBottomTree)
                    return parent.LeftBottomTree;
                var parentNeibor = FindNeiborNode(parent, QuadTreeFindMode.Left);
                if (tar == parent.LeftTopTree)
                    return parentNeibor?.RightTopTree;
                return parentNeibor?.RightBottomTree;
            }
        }
        return null;
    }
    #endregion

    #region 改
    /*
    /// <summary>
    /// 所有的点归类到最小包围它的空间
    /// </summary>
    public void PointsToMinNode()
    {
       ForEach(node =>
       {
           for (int i = 0; i < node.Contents.Count; i++)
           {
               var ent = node.Contents[i];
               if (ent.IsPoint)
               {
                   // 如果最小包含!=当前,就是没有放在最适合的位置
                   var queryNode = GetMinNode(ent);
                   if (queryNode != node)
                   {
                       node.Remove(ent);
                       queryNode.Contents.Add(ent);
                   }
               }
           }
           return false;
       });
    }
    */
    #endregion

    #region 方法
    /// <summary>
    /// 递归全部节点(提供给根用的,所以是全部)
    /// </summary>
    /// <param name="action"></param>QTAction
    public bool ForEach(QuadTree<TEntity>.QTAction action)
    {
        // 执行本节点
        if (action(this))
            return true;

        // 递归执行本节点的子节点
        var nodes = Nodes;
        for (var i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (node is null)
                continue;
            if (node.ForEach(action))
                break;
        }
        return false;
    }
    #endregion
}