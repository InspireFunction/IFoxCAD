namespace IFoxCAD.Cad;


using System.Data;
using PointV = Point2d;

/// <summary>
/// 填充边界转换器
/// </summary>
public class HatchConverter
{
    #region 辅助类
    /// <summary>
    /// 生成圆形数据
    /// </summary>
    class CircleData
    {
        public PointV Center;
        public double Radius;

        /// <summary>
        /// 生成圆形数据
        /// </summary>
        /// <param name="symmetryAxisPoint1">对称点1</param>
        /// <param name="symmetryAxisPoint2">对称点2</param>
        public CircleData(PointV symmetryAxisPoint1, PointV symmetryAxisPoint2)
        {
            Center = symmetryAxisPoint1.GetCenter(symmetryAxisPoint2);
            Radius = symmetryAxisPoint1.GetDistanceTo(symmetryAxisPoint2) * 0.5;
        }
    }

    /// <summary>
    /// 填充转换器的数据
    /// </summary>
    class HatchConverterData
    {
        public List<BulgeVertexWidth> PolyLineData;
        public List<CircleData> CircleData;
        public List<NurbCurve2d> SplineData;

        /// <summary>
        /// 填充转换器的数据
        /// </summary>
        public HatchConverterData()
        {
            PolyLineData = new();
            CircleData = new();
            SplineData = new();
        }
    }
    #endregion

    #region 成员
    /// <summary>
    /// 外部只能调用id,否则跨事务造成错误
    /// </summary>
    public ObjectId OldHatchId
    {
        get
        {
            if (_oldHatch is null)
                return ObjectId.Null;
            return _oldHatch.ObjectId;
        }
    }
    readonly Hatch? _oldHatch;

    readonly List<HatchConverterData> _hcDatas;
    /// <summary>
    /// 填充边界id(生成的/已存在反应器的直接提取)
    /// </summary>
    public List<ObjectId> BoundaryIds;
    #endregion

    #region 构造
    /// <summary>
    /// 填充边界转换器
    /// </summary>
    HatchConverter()
    {
        _hcDatas = new();
        BoundaryIds = new();
    }

    /// <summary>
    /// 填充边界转换器
    /// </summary>
    /// <param name="hatch">需要转化的Hatch对象</param>
    public HatchConverter(Hatch hatch) : this()
    {
        _oldHatch = hatch;

        if (hatch.Associative)
        {
            // 填充边界反应器
            var assIds = hatch.GetAssociatedObjectIds();
            if (assIds != null)
            {
                foreach (ObjectId id in assIds)
                    if (id.IsOk())
                        BoundaryIds.Add(id);

                if (BoundaryIds.Count == 0)
                {
                    throw new ArgumentException("关联的填充边界被删除后没有清理反应器,请调用:" +
                        "\n hatch.RemoveAssociatedObjectIds()" +
                        "\n hatch.Associative = false");
                }
            }
        }
    }

    /// <summary>
    /// 提取边界信息
    /// </summary>
    public void GetBoundarysData()
    {
        _oldHatch?.ForEach(loop => {
            HatchConverterData hcData = new();

            bool isCurve2d = true;
            if (loop.IsPolyline)
            {
                // 边界是多段线
                HatchLoopIsPolyline(loop, hcData);
                isCurve2d = false;
            }
            else
            {
                if (loop.Curves.Count == 2)// 1是不可能的,大于2的是曲线
                {
                    // 边界是曲线,过滤可能是圆形的情况
                    var cir = TwoArcFormOneCircle(loop);
                    if (cir is not null)
                    {
                        hcData.CircleData.Add(cir);
                        isCurve2d = false;
                    }
                }
            }

            // 边界是曲线
            if (isCurve2d)
                HatchLoopIsCurve2d(loop, hcData);

            _hcDatas.Add(hcData);
        });
    }
    #endregion

    #region 方法
    /// <summary>
    /// 多段线处理
    /// </summary>
    /// <param name="loop">填充边界</param>
    /// <param name="hcData">收集图元信息</param>
    static void HatchLoopIsPolyline(HatchLoop loop, HatchConverterData hcData)
    {
        //if (loop is null)
        //    throw new ArgumentNullException(nameof(loop));

        //if (hcData is null)
        //    throw new ArgumentNullException(nameof(hcData));

        loop.NotNull(nameof(loop)); 
        hcData.NotNull(nameof(hcData));


        // 判断为圆形:
        // 上下两个圆弧,然后填充,就会生成此种填充
        // 顶点数是3,凸度是半圆,两个半圆就是一个圆形
        if (loop.Polyline.Count == 3 && loop.Polyline[0].Bulge == 1 && loop.Polyline[1].Bulge == 1 ||
            loop.Polyline.Count == 3 && loop.Polyline[0].Bulge == -1 && loop.Polyline[1].Bulge == -1)
        {
            hcData.CircleData.Add(new CircleData(loop.Polyline[0].Vertex, loop.Polyline[1].Vertex));
        }
        else
        {
            // 遍历多段线信息
            var bvc = loop.Polyline;
            for (int i = 0; i < bvc.Count; i++)
                hcData.PolyLineData.Add(new BulgeVertexWidth(bvc[i]));
        }
    }

    /// <summary>
    /// 两个圆弧组成圆形
    /// </summary>
    /// <param name="loop"></param>
    /// <returns></returns>
    static CircleData? TwoArcFormOneCircle(HatchLoop loop)
    {
        //if (loop is null)
        //    throw new ArgumentNullException(nameof(loop));

        loop.NotNull(nameof(loop));

        if (loop.Curves.Count != 2)
            throw new ArgumentException(
                "边界非多段线,而且点数!=2,点数为:" + nameof(loop.Curves.Count) + ";两个矩形交集的时候会出现此情况.");

        CircleData? circular = null;

        // 判断为圆形:
        // 用一条(不是两条)多段线画出两条圆弧为正圆,就会生成此种填充
        // 边界为曲线,数量为2,可能是两个半圆曲线,如果是,就加入圆形数据中

        // 第一段
        var getCurves1Pts = loop.Curves[0].GetSamplePoints(3);   // 曲线取样点分两份(3点)
        var mid1Pt = getCurves1Pts[1];                           // 腰点
        double bulge1 = loop.Curves[0].StartPoint.GetArcBulge(mid1Pt, loop.Curves[0].EndPoint);

        // 第二段
        var getCurves2Pts = loop.Curves[1].GetSamplePoints(3);
        var mid2Pt = getCurves2Pts[1];
        double bulge2 = loop.Curves[1].StartPoint.GetArcBulge(mid2Pt, loop.Curves[1].EndPoint);

        // 第一段上弧&&第二段反弧 || 第一段反弧&&第二段上弧
        if (bulge1 == -1 && bulge2 == -1 || bulge1 == 1 && bulge2 == 1)
            circular = new CircleData(loop.Curves[0].StartPoint, loop.Curves[1].StartPoint); // 两个起点就是对称点

        return circular;
    }

    /// <summary>
    /// 处理边界曲线
    /// </summary>
    /// <param name="loop">填充边界</param>
    /// <param name="hcData">收集图元信息</param>
    static void HatchLoopIsCurve2d(HatchLoop loop, HatchConverterData hcData)
    {
        // 取每一段曲线,曲线可能是直线来的,但是圆弧会按照顶点来分段
        int curveIsClosed = 0;

        // 遍历边界的多个子段
        foreach (Curve2d curve in loop.Curves)
        {
            // 计数用于实现闭合
            curveIsClosed++;
            if (curve is NurbCurve2d spl)
            {
                // 判断为样条曲线:
                hcData.SplineData.Add(spl);
                continue;
            }

            var pts = curve.GetSamplePoints(3);
            var midPt = pts[1];
            if (curve.StartPoint.IsEqualTo(curve.EndPoint, new Tolerance(1e-6, 1e-6)))// 首尾相同,就是圆形
            {
                // 判断为圆形:
                // 获取起点,然后采样三点,中间就是对称点(直径点)
                hcData.CircleData.Add(new CircleData(curve.StartPoint, midPt));
                continue;
            }

            // 判断为多段线,圆弧:
            double bulge = curve.StartPoint.GetArcBulge(midPt, curve.EndPoint);
            hcData.PolyLineData.Add(new BulgeVertexWidth(curve.StartPoint, bulge));

            // 末尾点,不闭合的情况下就要获取这个
            if (curveIsClosed == loop.Curves.Count)
                hcData.PolyLineData.Add(new BulgeVertexWidth(curve.EndPoint, 0));
        }
    }

    /// <summary>
    /// 创建边界图元
    /// </summary>
    /// <param name="outEnts">返回图元</param>
    public void CreateBoundary(List<Entity> outEnts)
    {
        for (int i = 0; i < _hcDatas.Count; i++)
        {
            var data = _hcDatas[i];

            // 生成边界:多段线
            if (data.PolyLineData.Count > 0)
            {
                Polyline pl = new();
                pl.SetDatabaseDefaults();
                for (int j = 0; j < data.PolyLineData.Count; j++)
                {
                    pl.AddVertexAt(j,
                        data.PolyLineData[j].Vertex,
                        data.PolyLineData[j].Bulge,
                        data.PolyLineData[j].StartWidth,
                        data.PolyLineData[j].EndWidth);
                }
                outEnts.Add(pl);
            }

            // 生成边界:圆
            data.CircleData.ForEach(item => {
                outEnts.Add(new Circle(item.Center.Point3d(), Vector3d.ZAxis, item.Radius));
            });

            // 生成边界:样条曲线
            data.SplineData.ForEach(item => {
                outEnts.Add(item.ToCurve());
            });
        }

        if (_oldHatch is not null)
        {
            outEnts.ForEach(ent => {
                ent.Color = _oldHatch.Color;
                ent.Layer = _oldHatch.Layer;
            });
        }
    }


    /// <summary>
    /// 创建边界图元和新填充到当前空间
    /// </summary>
    /// <param name="btrOfAddEntitySpace"></param>
    /// <param name="boundaryAssociative">边界关联</param>
    /// <param name="createHatchFlag">是否创建填充,false则只创建边界</param>
    /// <returns>新填充id,边界在<see cref="BoundaryIds"/>获取</returns>
    public ObjectId CreateBoundarysAndHatchToMsPs(BlockTableRecord btrOfAddEntitySpace,
        bool boundaryAssociative = true,
        bool createHatchFlag = true)
    {
        List<Entity> boEnts = new();
        CreateBoundary(boEnts);
        boEnts.ForEach(ent => {
            BoundaryIds.Add(btrOfAddEntitySpace.AddEntity(ent));
        });

        if (!createHatchFlag)
            return ObjectId.Null;
        /*
         * 此处为什么要克隆填充,而不是新建填充?
         * 因为填充如果是新建的,那么将会丢失基点,概念如下:
         * 两个一样的填充,平移其中一个,那么再提取他们的基点会是一样的!
         * 所以生成时候就不等同于画面相同.
         * 也因为我不知道什么新建方式可以新建一模一样的填充,因此使用了克隆
         * 那么它的平移后的基点在哪里呢?
         */

        using ObjectIdCollection idc = new(new ObjectId[] { OldHatchId });
        using IdMapping map = new();
        btrOfAddEntitySpace.DeepCloneEx(idc, map);
        var newHatchId = map.GetValues()[0];
        var tr = DBTrans.GetTopTransaction(btrOfAddEntitySpace.Database);

        bool openErased = false;
        bool openLockedLayer = false;
        var hatchEnt = tr.GetObject(newHatchId, OpenMode.ForWrite,
                                       openErased, openLockedLayer) as Hatch;
        if (hatchEnt != null)
        {
            ResetBoundary(hatchEnt, boundaryAssociative);
            hatchEnt.DowngradeOpen();
        }
        return newHatchId;
    }

    /// <summary>
    /// 重设边界
    /// </summary>
    /// <param name="hatch"></param>
    /// <param name="boundaryAssociative">边界关联</param>
    void ResetBoundary(Hatch hatch, bool boundaryAssociative = true)
    {
        if (BoundaryIds.Count == 0)
            return;

        // todo ------ acad08分离填充报错: Microsoft Visual Studio C 运行库在 acad.exe 中检测到一个错误
        // 0x01 测试命令 CmdTest_CreateHatch 创建是可以分离的,
        // 那么可能是 克隆后 修改导致的,
        // 我是克隆了之后移除原有边界,为了一些xdata之类的
        // 0x02 测试了 hatch.SetDatabaseDefaults(); 并不是因为这个
        // 0x03 测试了 v1110 不移除原有边界,而是加入了之后再移除旧的边界,也是一样
        // 要处理这个问题,我想:自己实现一个分离填充,不用cad自带的,然后单独填充每个.
        // 填充边界的算法是扫描线算法.这样就可以绕过去了...发现过于麻烦,放弃...

        // v1110 删除原有边界
        while (hatch.NumberOfLoops != 0)
            hatch.RemoveLoopAt(0);

        hatch.Associative = boundaryAssociative;

        using ObjectIdCollection obIds = new();
        for (int i = 0; i < BoundaryIds.Count; i++)
        {
            obIds.Clear();
            obIds.Add(BoundaryIds[i]);
            // 要先添加最外面的边界
            if (i == 0)
                hatch.AppendLoop(HatchLoopTypes.Outermost, obIds);
            else
                hatch.AppendLoop(HatchLoopTypes.Default, obIds);
        }
        // 计算填充并显示
        hatch.EvaluateHatch(true);
    }
    #endregion
}