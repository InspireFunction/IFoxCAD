namespace Test;

/*
 * 这里属于用户调用例子,
 * 调用时候必须要继承它,再提供给四叉树
 * 主要是用户可以扩展属性
 */
public class CadEntity : QuadEntity
{
    public ObjectId ObjectId;
    //这里加入其他字段
    public List<QuadEntity>? Link;//碰撞链
    public System.Drawing.Color Color;
    public double Angle;
    public CadEntity(ObjectId objectId, Rect box) : base(box)
    {
        ObjectId = objectId;
    }
    public int CompareTo(CadEntity? other)
    {
        if (other == null)
            return -1;
        return GetHashCode() ^ other.GetHashCode();
    }
    public override int GetHashCode()
    {
        return (base.GetHashCode(), ObjectId.GetHashCode()).GetHashCode();
    }
}

public partial class TestQuadTree
{
    QuadTree<CadEntity> _quadTreeRoot;

    #region 四叉树创建并加入
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

        //生成多少个图元,导致cad会令undo出错(八叉树深度过大 treemax)
        //int maximumItems = 30_0000;
        int maximumItems = 1000;

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
                /*加入随机点*/
                var p = edge.MinPoint + new Vector3d(10, 10, 0);
                entRect = new Rect(p.Point2d(), p.Point2d());
                entId = tr.CurrentSpace.AddEntity(new DBPoint(p));
                var dbPointCe = new CadEntity(entId, entRect);
                ces.Add(dbPointCe);
            }
        }, Timer.TimeEnum.Millisecond, "画圆消耗时间:");//30万图元±3秒.cad2021

        //测试只加入四叉树的时间
        Timer.RunTime(() => {
            for (int i = 0; i < ces.Count; i++)
            {
                _quadTreeRoot.Insert(ces[i]);
            }
        }, Timer.TimeEnum.Millisecond, "插入四叉树时间:");//30万图元±0.7秒.cad2021

        tr.Editor.WriteMessage($"\n加入图元数量:{maximumItems}");
    }

    /// <summary>
    /// 创建随机圆形
    /// </summary>
    /// <param name="createNumber">创建数量</param>
    /// <param name="dbExt">数据库边界</param>
    static IEnumerable<Entity> GenerateRandomCircle(int createNumber, Rect dbExt)
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

    /*TODO 啊惊: 有点懒不想改了*/
#if true2 

    //选择加入到四叉树
    [CommandMethod("CmdTest_QuadTree21")]
    public void CmdTest_QuadTree21()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;
        ed.WriteMessage("\n选择单个图元加入已有的四叉树");

        var ss = ed.Ssget();
        if (ss.Count == 0)
            return;

        AddQuadTreeRoot(db, ed, ss);
    }

    //自动加入全图到四叉树
    [CommandMethod("CmdTest_QuadTree20")]
    public void CmdTest_QuadTree20()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;
        ed.WriteMessage("\n自动加入全图到四叉树");

        var ss = new List<ObjectId>();
        int entnum = 0;
        var time1 = Timer.RunTime(() => {
            db.Action(tr => {
                db.TraverseBlockTable(tr, btRec => {
                    if (!btRec.IsLayout)//布局跳过
                        return false;

                    foreach (var item in btRec)
                    {
                        //var ent = item.ToEntity(tr);
                        ss.Add(item);
                        ++entnum;//图元数量:100000, 遍历全图时间:0.216秒 CmdTest_QuadTree2
                    }
                    return false;
                });
            });
        });
        ed.WriteMessage($"\n图元数量:{entnum}, 遍历全图时间:{time1 / 1000.0}秒");

        //清空原有的
        _quadTreeRoot = null;
        AddQuadTreeRoot(db, ed, ss);
    } 

    void AddQuadTreeRoot(Database db, Editor ed, List<ObjectId> ss)
    {
        if (_quadTreeRoot is null)
        {
            ed.WriteMessage("\n四叉树是空的,重新初始化");

            Rect dbExt;
            //使用数据库边界来进行
            var dbExtent = db.GetValidExtents3d();
            if (dbExtent == null)
            {
                //throw new ArgumentException("画一个矩形");

                //测试时候画个矩形,在矩形内画随机坐标的圆形
                dbExt = new Rect(0, 0, 32525, 32525);
            }
            else
            {
                dbExt = new Rect(dbExtent.Value.MinPoint.Point2d(), dbExtent.Value.MaxPoint.Point2d());
            }
            _quadTreeRoot = new(dbExt);
        }

        /* 测试:
         * 为了测试删除内容释放了分支,再重复加入是否报错
         * 先创建 CmdTest_QuadTree1
         * 再减去 CmdTest_QuadTree0
         * 然后原有黑色边界,再生成边界 CmdTest_Create00,对比删除效果.
         * 然后加入 CmdTest_QuadTree2
         * 然后原有黑色边界,再生成边界 CmdTest_Create00,对比删除效果.
         */

        List<CadEntity> ces = new();
        db.Action(tr => {
            ss.ForEach(entId => {
                var ent = entId.ToEntity(tr);
                if (ent is null)
                    return;
                var edge = new EdgeEntity(ent);
                //四叉树数据
                var ce = new CadEntity(entId, edge.Edge)
                {
                    Color = Utility.RandomColor
                };
                ces.Add(ce);

                edge.Dispose();
            });
        });

        var time2 = Timer.RunTime(() => {
            _quadTreeRoot.Insert(ces);
        });
        ed.WriteMessage($"\n图元数量:{ces.Count}, 加入四叉树时间:{time2 / 1000.0}秒");
    }
#endif

    #endregion

    /*TODO 啊惊: 有点懒不想改了*/
#if true2

    #region 节点边界显示
    //四叉树减去节点
    [CommandMethod("CmdTest_QuadTree0")]
    public void CmdTest_QuadTree0()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        //var db = doc.Database;
        var ed = doc.Editor;
        ed.WriteMessage("\n四叉树减区");

        if (_quadTreeRoot is null)
        {
            ed.WriteMessage("\n四叉树是空的");
            return;
        }
        var rect = GetCorner(ed);
        if (rect is null)
            return;
        _quadTreeRoot.Remove(rect);
    }

    //创建节点边界
    [CommandMethod("CmdTest_QuadTree00")]
    public void CmdTest_CreateNodesRect()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var db = doc.Database;
        var ed = doc.Editor;
        ed.WriteMessage("\n创建边界");

        if (_quadTreeRoot is null)
        {
            ed.WriteMessage("\n四叉树是空的");
            return;
        }

        //此处发现了一个事务处理的bug,提交数量过多的时候,会导致 ctrl+z 无法回滚,
        //需要把事务放在循环体内部
        //报错: 0x6B00500A (msvcr80.dll)处(位于 acad.exe 中)引发的异常: 0xC0000005: 写入位置 0xFFE00000 时发生访问冲突。
        //画出所有的四叉树节点边界,因为事务放在外面引起
        var nodeRects = new List<Rect>();
        _quadTreeRoot.ForEach(node => {
            nodeRects.Add(node);
            return false;
        });
        var rectIds = new List<ObjectId>();
        foreach (var item in nodeRects)//Count = 97341 当数量接近这个量级
        {
            db.Action(tr => {
                var pts = item.ToPoints();
                var rec = EntityAdd.AddPolyLineToEntity(pts.ToPoint2d());
                rec.ColorIndex = 250;
                rectIds.Add(tr.AddEntityToMsPs(db, rec));
            });
        }
        db.Action(tr => {
            db.CoverGroup(tr, rectIds);
        });

        //获取四叉树深度
        int dep = 0;
        _quadTreeRoot.ForEach(node => {
            dep = dep > node.Depth ? dep : node.Depth;
            return false;
        });
        ed.WriteMessage($"\n四叉树深度是: {dep}");
    }
    #endregion

#endif

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
    public static Rect? GetCorner(Editor ed)
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

//public partial class TestQuadTree
//{
//    public void Cmd_tt6()
//    {
//        using var tr = new DBTrans();
//        var ed = tr.Editor;
//        //创建四叉树,默认参数无所谓
//        var TreeRoot = new QuadTree<CadEntity>(new Rect(0, 0, 32525, 32525));

//        var fil = OpFilter.Bulid(e => e.Dxf(0) == "LINE");
//        var psr = ed.SSGet("\n 选择需要连接的直线", fil);
//        if (psr.Status != PromptStatus.OK) return;
//        var LineEnts = new List<Line>(psr.Value.GetEntities<Line>(OpenMode.ForWrite)!);
//        //将实体插入到四岔树
//        foreach (var line in LineEnts)
//        {
//            var edge = line.GeometricExtents;
//            var entRect = new Rect(edge.MinPoint.X, edge.MinPoint.Y, edge.MaxPoint.X, edge.MaxPoint.Y);
//            var ce = new CadEntity(line.Id, entRect)
//            {
//                //四叉树数据
//                Angle = line.Angle
//            };
//            TreeRoot.Insert(ce);
//        }

//        var ppo = new PromptPointOptions(Environment.NewLine + "\n指定标注点:<空格退出>")
//        {
//            AllowArbitraryInput = true,//任意输入
//            AllowNone = true //允许回车
//        };
//        var ppr = ed.GetPoint(ppo);//用户点选
//        if (ppr.Status != PromptStatus.OK)
//            return;
//        var rect = new Rect(ppr.Value.Point2d(), 100, 100);
//        tr.CurrentSpace.AddEntity(rect.ToPolyLine());//显示选择靶标范围

//        var nent = TreeRoot.FindNearEntity(rect);//查询最近实体，按逆时针
//        var ent = tr.GetObject<Entity>(nent.ObjectId, OpenMode.ForWrite);//打开实体
//        ent.ColorIndex = Utility.GetRandom().Next(1, 256);//1~256随机色
//        ent.DowngradeOpen();//实体降级
//        ent.Dispose();

//        var res = TreeRoot.Query(rect, QuadTreeSelectMode.IntersectsWith);//查询选择靶标范围相碰的ID
//        res.ForEach(item => {
//            if (item.Angle == 0 || item.Angle == Math.PI) //过滤直线角度为0或180的直线
//            {
//                var ent = tr.GetObject<Entity>(item.ObjectId, OpenMode.ForWrite);
//                ent.ColorIndex = Utility.GetRandom().Next(1, 7);
//                ent.DowngradeOpen();
//                ent.Dispose();
//            }
//        });
//    }
//}