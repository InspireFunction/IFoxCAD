namespace IFoxCAD.Cad;

/*
 *  添加的第一个边界必须是外边界,即用于定义图案填充最外面的边界。
 *  要添加外部边界,请使用添加环的类型为 HatchLoopTypes.Outermost 常量的 AppendLoop 方法,
 *  一旦外边界被定义,就可以继续添加另外的边界。
 *  添加内部边界请使用带 HatchLoopTypes.Default 常量的 AppendLoop 方法。
 *
 *  多个外边界的时候,添加的是(外边界,外边界,外边界,普通边界....)
 *  多个外边界的时候,添加的是(外边界,普通边界.....外边界,普通边界....)
 */

/// <summary>
/// 图案填充
/// </summary>
public class HatchInfo
{
    #region 成员
    /// <summary>
    /// 边界id(最外面放第一)
    /// </summary>
    readonly List<ObjectId> _boundaryIds;
    /// <summary>
    /// 填充图元
    /// </summary>
    readonly Hatch _hatch;
    /// <summary>
    /// 边界关联(此处不能直接=>给填充成员,因为它会加入反应器)
    /// </summary>
    readonly bool _boundaryAssociative;
    /// <summary>
    /// 填充的名称:用户定义(固定名称)/渐变/填充依据定义文件
    /// </summary>
    string? _hatchName;
    /// <summary>
    /// 填充模式类型(预定义/用户定义/自定义)
    /// </summary>
    HatchPatternType _patternTypeHatch;
    /// <summary>
    /// 渐变模式类型
    /// </summary>
    GradientPatternType _patternTypeGradient;
    /// <summary>
    /// 比例/间距
    /// </summary>
    double Scale => _hatch.PatternScale;
    /// <summary>
    /// 角度
    /// </summary>
    double Angle => _hatch.PatternAngle;
    #endregion

    #region 构造
    HatchInfo()
    {
        _hatch = new Hatch();
        _hatch.SetDatabaseDefaults();
        _boundaryIds = new();
    }

    /// <summary>
    /// 图案填充
    /// </summary>
    /// <param name="boundaryAssociative">关联边界</param>
    /// <param name="hatchOrigin">填充原点</param>
    /// <param name="hatchScale">比例</param>
    /// <param name="hatchAngle">角度</param>
    public HatchInfo(bool boundaryAssociative = true,
                     Point2d? hatchOrigin = null,
                     double hatchScale = 1,
                     double hatchAngle = 0) : this()
    {
        if (hatchScale <= 0)
            throw new ArgumentException("填充比例不允许小于等于0");

        _hatch.PatternScale = hatchScale;// 填充比例
        _hatch.PatternAngle = hatchAngle;// 填充角度
        _boundaryAssociative = boundaryAssociative;

        hatchOrigin ??= Point2d.Origin;
        _hatch.Origin = hatchOrigin.Value; // 填充原点
    }

    /// <summary>
    /// 图案填充
    /// </summary>
    /// <param name="boundaryIds">边界</param>
    /// <param name="boundaryAssociative">关联边界</param>
    /// <param name="hatchOrigin">填充原点</param>
    /// <param name="hatchScale">比例</param>
    /// <param name="hatchAngle">角度</param>
    public HatchInfo(IEnumerable<ObjectId> boundaryIds,
                     bool boundaryAssociative = true,
                     Point2d? hatchOrigin = null,
                     double hatchScale = 1,
                     double hatchAngle = 0)
        : this(boundaryAssociative, hatchOrigin, hatchScale, hatchAngle)
    {
        _boundaryIds.AddRange(boundaryIds);
    }

    #endregion

    #region 方法
    /// <summary>
    /// 模式1:预定义
    /// </summary>
    public HatchInfo Mode1PreDefined(string name)
    {
        _hatchName = name;
        _hatch.HatchObjectType = HatchObjectType.HatchObject; // 对象类型(填充/渐变)
        _patternTypeHatch = HatchPatternType.PreDefined;
        return this;
    }

    /// <summary>
    /// 模式2:用户定义
    /// </summary>
    /// <param name="patternDouble">是否双向</param>
    public HatchInfo Mode2UserDefined(bool patternDouble = true)
    {
        _hatchName = "_USER";
        _hatch.HatchObjectType = HatchObjectType.HatchObject; // 对象类型(填充/渐变)
        _patternTypeHatch = HatchPatternType.UserDefined;

        _hatch.PatternDouble = patternDouble; // 是否双向（必须写在 SetHatchPattern 之前）
        _hatch.PatternSpace = Scale;         // 间距（必须写在 SetHatchPattern 之前）
        return this;
    }

    /// <summary>
    /// 模式3:自定义
    /// </summary>
    /// <param name="name"></param>
    public HatchInfo Mode3UserDefined(string name)
    {
        _hatchName = name;
        _hatch.HatchObjectType = HatchObjectType.HatchObject; // 对象类型(填充/渐变)
        _patternTypeHatch = HatchPatternType.CustomDefined;
        return this;
    }

    /// <summary>
    /// 模式4:渐变填充
    /// </summary>
    /// <param name="name">渐变填充名称</param>
    /// <param name="colorStart">渐变色起始颜色</param>
    /// <param name="colorEnd">渐变色结束颜色</param>
    /// <param name="gradientShift">渐变移动</param>
    /// <param name="shadeTintValue">色调值</param>
    /// <param name="gradientOneColorMode">单色<see langword="true"/>双色<see langword="false"/></param>
    public HatchInfo Mode4Gradient(GradientName name, Color colorStart, Color colorEnd,
        float gradientShift = 0,
        float shadeTintValue = 0,
        bool gradientOneColorMode = false)
    {
        // entget渐变的名字必然是"SOLID",但是这里作为"渐变"名,而不是"填充"名
        _hatchName = name.ToString();
        _hatch.HatchObjectType = HatchObjectType.GradientObject;      // 对象类型(填充/渐变)
        _patternTypeGradient = GradientPatternType.PreDefinedGradient;// 模式4:渐变
        // _patternTypeGradient = GradientPatternType.UserDefinedGradient;// 模式5:渐变..这种模式干啥用呢

        // 设置渐变色填充的起始和结束颜色
        var gColor1 = new GradientColor(colorStart, 0);
        var gColor2 = new GradientColor(colorEnd, 1);
        _hatch.SetGradientColors(new GradientColor[] { gColor1, gColor2 });

        _hatch.GradientShift = gradientShift;              // 梯度位移
        _hatch.ShadeTintValue = shadeTintValue;            // 阴影色值
        _hatch.GradientOneColorMode = gradientOneColorMode;// 渐变单色/双色
        _hatch.GradientAngle = Angle;                      // 渐变角度

        return this;
    }

    /// <summary>
    /// 构建
    /// </summary>
    /// <param name="btrOfAddEntitySpace">将填充加入此空间</param>
    public ObjectId Build(BlockTableRecord btrOfAddEntitySpace)
    {
        // 加入数据库
        var hatchId = btrOfAddEntitySpace.AddEntity(_hatch);

        // 设置模式:渐变/填充
        if (_hatch.HatchObjectType == HatchObjectType.GradientObject)
            _hatch.SetGradient(_patternTypeGradient, _hatchName);
        else
            _hatch.SetHatchPattern(_patternTypeHatch, _hatchName);

        // 关联边界,如果不先添加数据库空间内就会出错
        // 为 true 会加入反应器,因此比较慢(二维码将会十几秒才生成好),视需求而定.
        _hatch.Associative = _boundaryAssociative;

        // 利用 AppendLoop 重载加入,这里就不处理
        if (_boundaryIds.Count > 0)
            AppendLoop(_boundaryIds, HatchLoopTypes.Default);

        // 计算填充并显示(若边界出错,这句会异常)
        _hatch.EvaluateHatch(true);

        return hatchId;
    }

    /// <summary>
    /// 执行图元的属性修改
    /// </summary>
    /// <param name="action">扔出填充实体</param>
    public HatchInfo Action(Action<Hatch> action)
    {
        action(_hatch);
        return this;
    }

    /// <summary>
    /// 清空边界集合
    /// </summary>
    public HatchInfo ClearBoundary()
    {
        _boundaryIds.Clear();
        return this;
    }

    /// <summary>
    /// 删除边界图元
    /// </summary>
    public HatchInfo EraseBoundary()
    {
        for (int i = 0; i < _boundaryIds.Count; i++)
            _boundaryIds[i].Erase();
        return this;
    }

    /// <summary>
    /// 加入边界
    /// </summary>
    /// <param name="boundaryIds">边界id</param>
    /// <param name="hatchLoopTypes">加入方式</param>
    void AppendLoop(IEnumerable<ObjectId> boundaryIds,
                    HatchLoopTypes hatchLoopTypes = HatchLoopTypes.Default)
    {
        var obIds = new ObjectIdCollection();
        // 边界是闭合的,而且已经加入数据库
        // 填充闭合环类型.最外面
        foreach (var border in boundaryIds)
        {
            obIds.Clear();
            obIds.Add(border);
            _hatch.AppendLoop(hatchLoopTypes, obIds);
        }
        obIds.Dispose();
    }

    /// <summary>
    /// 加入边界(仿高版本的填充函数)
    /// </summary>
    /// <param name="pts">点集</param>
    /// <param name="bluges">凸度集</param>
    /// <param name="btrOfAddEntitySpace">加入此空间</param>
    /// <param name="hatchLoopTypes">加入方式</param>
    /// <returns></returns>
    public HatchInfo AppendLoop(Point2dCollection pts,
                                DoubleCollection bluges,
                                BlockTableRecord btrOfAddEntitySpace,
                                HatchLoopTypes hatchLoopTypes = HatchLoopTypes.Default)
    {
        if (pts == null)
            throw new ArgumentNullException(nameof(pts));

        var ptsEnd2End = pts.End2End();
#if NET35
        _boundaryIds.Add(CreateAddBoundary(ptsEnd2End, bluges, btrOfAddEntitySpace));
#else
        // 2011新增API,可以不生成图元的情况下加入边界,
        // 通过这里进入的话,边界 _boundaryIds 是空的,那么 Build() 时候就需要过滤空的
        _hatch.AppendLoop(hatchLoopTypes, ptsEnd2End, bluges);
#endif
        return this;
    }

#if NET35
    /// <summary>
    /// 通过点集和凸度生成边界的多段线
    /// </summary>
    /// <param name="pts">点集</param>
    /// <param name="bluges">凸度集</param>
    /// <param name="btrOfAddEntitySpace">加入此空间</param>
    /// <returns>多段线id</returns>
    static ObjectId CreateAddBoundary(Point2dCollection? pts,
        DoubleCollection? bluges,
        BlockTableRecord btrOfAddEntitySpace)
    {
        if (pts is null)
            throw new ArgumentException(null, nameof(pts));
        if (bluges is null)
            throw new ArgumentException(null, nameof(bluges));

        var bvws = new List<BulgeVertexWidth>();

        var itor1 = pts.GetEnumerator();
        var itor2 = bluges.GetEnumerator();
        while (itor1.MoveNext() && itor2.MoveNext())
            bvws.Add(new BulgeVertexWidth(itor1.Current, itor2.Current));

        return btrOfAddEntitySpace.AddPline(bvws);
    }
#endif
    #endregion

    #region 枚举
    /// <summary>
    /// 渐变色填充的图案名称
    /// </summary>
    public enum GradientName
    {
        /// <summary>
        /// 线状渐变
        /// </summary>
        Linear,
        /// <summary>
        /// 圆柱状渐变
        /// </summary>
        Cylinder,
        /// <summary>
        /// 反圆柱状渐变
        /// </summary>
        Invcylinder,
        /// <summary>
        /// 球状渐变
        /// </summary>
        Spherical,
        /// <summary>
        /// 反球状渐变
        /// </summary>
        Invspherical,
        /// <summary>
        /// 半球状渐变
        /// </summary>
        Hemisperical,
        /// <summary>
        /// 反半球状渐变
        /// </summary>
        InvHemisperical,
        /// <summary>
        /// 抛物面状渐变
        /// </summary>
        Curved,
        /// <summary>
        /// 反抛物面状渐变
        /// </summary>
        Incurved
    }
    #endregion
}