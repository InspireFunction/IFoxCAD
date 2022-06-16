namespace Test;

public class CadEntity : QuadEntity
{
    //这里加入其他字段
    public int MyProperty { get; set; }
    public List<QuadEntity>? Link;//碰撞链...这里外面自己封装去
    public System.Drawing.Color Color { get; set; }
    public CadEntity(ObjectId objectId, Rect box) : base(objectId, box)
    {

    }
}

public class TestQuadTree
{
    [CommandMethod("Test_QuadTree")]
    public void Test_QuadTree()
    {
        using var tr = new DBTrans();

        Rect dbExt;
        //使用数据库边界来进行
        if (!tr.Database.GetValidExtents3d(out Extents3d dbExtent))
        {
            //throw new ArgumentException("画一个矩形");

            //测试时候画个矩形,在矩形内画随机坐标的圆形
            dbExt = new Rect(0, 0, 32525, 32525);
        }
        else
        {
            var a = new Point2d(dbExtent.MinPoint.X, dbExtent.MinPoint.Y);
            var b = new Point2d(dbExtent.MaxPoint.X, dbExtent.MaxPoint.Y);
            dbExt = new Rect(a, b);
        }

        //创建四叉树
        var _quadTreeRoot = new QuadTree<CadEntity>(dbExt);

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

        int maximumItems = 1_0000; //生成多少个图元,30万图元±0.5秒,导致cad会令undo出错(八叉树深度过大 treemax)

        //随机图元生成
        List<CadEntity> ces = new();  //用于随机获取图元
        var allTime = Timer.RunTime(() => {
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
                var ce = new CadEntity(entId, entRect);
                ce.Color = Utility.RandomColor;
                ces.Add(ce);
            }
        });

        //测试只加入四叉树的时间
        var insertTime = Timer.RunTime(() => {
            _quadTreeRoot.Insert(ces);
        });

        tr.Editor.WriteMessage($"\n加入图元数量:{maximumItems}, 插入四叉树时间:{insertTime / 1000.0}秒, 画圆消耗时间:{allTime / 1000.0}秒");
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

}
