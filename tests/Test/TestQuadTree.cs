namespace Test;

/*
 * 这里属于用户调用例子,
 * 调用时候必须要继承它,再提供给四叉树
 * 主要是用户可以扩展属性
 */
public class CadEntity : QuadEntity
{
    //这里加入其他字段
    public List<QuadEntity>? Link;//碰撞链
    public System.Drawing.Color Color;
    public CadEntity(ObjectId objectId, Rect box) : base(objectId, box)
    {

    }
}

public class TestQuadTree
{
    QuadTree<CadEntity> _quadTreeRoot;


    [CommandMethod("Test_QuadTree")]
    public void Test_QuadTree()
    {
        using var tr = new DBTrans();

        Rect dbExt;
        //使用数据库边界来进行
        var dbExtent = tr.Database.GetValidExtents3d();
        if (dbExtent == null)
        {
            //throw new ArgumentException("画一个矩形");

            //测试时候画个矩形,在矩形内画随机坐标的圆形
            dbExt = new Rect(0, 0, 32525, 32525);
        }
        else
        {
            var a = new Point2d(dbExtent.Value.MinPoint.X, dbExtent.Value.MinPoint.Y);
            var b = new Point2d(dbExtent.Value.MaxPoint.X, dbExtent.Value.MaxPoint.Y);
            dbExt = new Rect(a, b);
        }

        //创建四叉树
        _quadTreeRoot = new QuadTree<CadEntity>(dbExt);

        //数据库边界
        var pl = dbExt.ToPoints();
        var databaseBoundary = new List<(Point3d, double, double, double)>
            {
                (new Point3d(pl[0].X,pl[0].Y,0),0,0,0),
                (new Point3d(pl[1].X,pl[1].Y,0),0,0,0),
                (new Point3d(pl[2].X,pl[2].Y,0),0,0,0),
                (new Point3d(pl[3].X,pl[3].Y,0),0,0,0),
            };
        tr.CurrentSpace.AddPline(databaseBoundary);

        int maximumItems = 30_0000; //生成多少个图元,导致cad会令undo出错(八叉树深度过大 treemax)

        //随机图元生成
        List<CadEntity> ces = new();  //用于随机获取图元
        Timer.RunTime(() => {
            //生成外边界和随机圆形 
            var grc = GenerateRandomCircle(maximumItems, dbExt);
            foreach (var ent in grc)
            {
                //初始化图元颜色
                ent.ColorIndex = 1; //Color.FromRgb(0, 0, 0);//黑色
                var edge = ent.GeometricExtents;
                //四叉树数据
                var entRect = new Rect(edge.MinPoint.X, edge.MinPoint.Y, edge.MaxPoint.X, edge.MaxPoint.Y);
                var entId = tr.CurrentSpace.AddEntity(ent);
                var ce = new CadEntity(entId, entRect)
                {
                    Color = Utility.RandomColor
                };
                ces.Add(ce);
            }
        }, Timer.TimeEnum.Millisecond, "画圆消耗时间:");//30万图元±3秒.cad2021

        //测试只加入四叉树的时间
        Timer.RunTime(() => {
            _quadTreeRoot.Insert(ces);
        }, Timer.TimeEnum.Millisecond, "插入四叉树时间:");//30万图元±0.7秒.cad2021

        tr.Editor.WriteMessage($"\n加入图元数量:{maximumItems}");
    }

    /// <summary>
    /// 创建随机圆形
    /// </summary>
    /// <param name="createNumber">创建数量</param>
    /// <param name="dbExt">数据库边界</param>
    IEnumerable<Entity> GenerateRandomCircle(int createNumber, Rect dbExt)
    {
        var x1 = (int)dbExt.X;
        var x2 = (int)(dbExt.X + dbExt.Width);
        var y1 = (int)dbExt.Y;
        var y2 = (int)(dbExt.Y + dbExt.Height);

        var rand = Utility.GetRandom();
        for (int i = 0; i < createNumber; i++)
        {
            var x = rand.Next(x1, x2) + rand.NextDouble();
            var y = rand.Next(y1, y2) + rand.NextDouble();
            yield return EntityEx.CreateCircle(new Point3d(x, y, 0), rand.Next(1, 100)); //起点，终点
        }
    }


    #region 四叉树查询节点
    //选择范围改图元颜色
    [CommandMethod("CmdTest_QuadTree3")]
    public void CmdTest_QuadTree3()
    {
        Ssget(QuadTreeSelectMode.IntersectsWith);
    }

    [CommandMethod("CmdTest_QuadTree4")]
    public void CmdTest_QuadTree4()
    {
        Ssget(QuadTreeSelectMode.Contains);
    }

    /// <summary>
    /// 改颜色
    /// </summary>
    /// <param name="mode"></param>
    void Ssget(QuadTreeSelectMode mode)
    {
        using var tr = new DBTrans();

        if (_quadTreeRoot is null)
            return;
        var rect = GetCorner(tr.Editor);
        if (rect is null)
            return;

        tr.Editor.WriteMessage("选择模式:" + mode);

        //仿选择集
        var ces = _quadTreeRoot.Query(rect, mode);
        ces.ForEach(item => {
            var ent = tr.GetObject<Entity>(item.ObjectId, OpenMode.ForWrite);
            ent.Color = Color.FromColor(item.Color);
            ent.DowngradeOpen();
            ent.Dispose();
        });
    }

    /// <summary>
    /// 交互获取
    /// </summary>
    /// <param name="ed"></param>
    /// <returns></returns>
    public Rect? GetCorner(Editor ed)
    {
        var optionsA = new PromptPointOptions($"{Environment.NewLine}起点位置:");
        var pprA = ed.GetPoint(optionsA);
        if (pprA.Status != PromptStatus.OK)
            return null;
        var optionsB = new PromptCornerOptions(Environment.NewLine + "输入矩形角点2:", pprA.Value)
        {
            UseDashedLine = true,//使用虚线
            AllowNone = true,//回车
        };
        var pprB = ed.GetCorner(optionsB);
        if (pprB.Status != PromptStatus.OK)
            return null;

        return new Rect(new Point2d(pprA.Value.X, pprA.Value.Y),
                        new Point2d(pprB.Value.X, pprB.Value.Y),
                        true);
    }
    #endregion
}
