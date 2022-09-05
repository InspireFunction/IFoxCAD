namespace IFoxCAD.Cad;

/*  封装jig
 *  20220726 隐藏事件,利用函数进行数据库图元重绘
 *  20220710 修改SetOption()的空格结束,并添加例子到IFox
 *  20220503 cad22需要防止刷新过程中更改队列,是因为允许函数重入导致,08不会有.
 *  20220326 重绘图元的函数用错了,现在修正过来
 *  20211216 加入块表时候做一个差集,剔除临时图元
 *  20211209 补充正交变量设置和回收设置
 *  作者: 惊惊⎛⎝◕⏝⏝◕｡⎠⎞ ⎛⎝≥⏝⏝0⎠⎞ ⎛⎝⓿⏝⏝⓿｡⎠⎞ ⎛⎝≥⏝⏝≤⎠⎞
 *  博客: https://www.cnblogs.com/JJBox/p/15650770.html
 */

public delegate void WorldDrawEvent(WorldDraw draw);
public class JigEx : DrawJig
{
    #region 成员
    /// <summary>
    /// 事件:亮显/暗显会被刷新冲刷掉,所以这个事件用于补充非刷新的工作
    /// </summary>
    event WorldDrawEvent? WorldDrawEvent;
    /// <summary>
    /// 最后的鼠标点,用来确认长度
    /// </summary>
    public Point3d MousePointWcsLast;
    /// <summary>
    /// 最后的图元,用来生成
    /// </summary>
    public Entity[] Entitys => _drawEntitys.ToArray();


    readonly Action<Point3d, Queue<Entity>>? _mouseAction;
    readonly Tolerance _tolerance;// 容差

    readonly Queue<Entity> _drawEntitys;// 重复生成的图元,放在这里刷新
    JigPromptPointOptions? _options;// jig鼠标配置
    bool _worldDrawFlag = false; // 20220503

    bool _systemVariables_Orthomode = false;
    bool SystemVariables_Orthomode // 正交修改还原
    {
        get => _systemVariables_Orthomode;
        set
        {
            // 1正交,0非正交 // setvar: https://www.cnblogs.com/JJBox/p/10209541.html
            if (Env.OrthoMode != value)
                Env.OrthoMode = _systemVariables_Orthomode = value;
        }
    }
    #endregion

    #region 构造
    /// <summary>
    /// 在界面绘制图元
    /// </summary>
    JigEx()
    {
        _drawEntitys = new();
        DimensionEntitys = new();
    }

    /// <summary>
    /// 在界面绘制图元
    /// </summary>
    /// <param name="action">
    /// 用来频繁执行的回调:<br/>
    /// <see cref="Point3d"/>鼠标点;<br/>
    /// <see cref="Queue"/>加入新建的图元,鼠标采样期间会Dispose图元的;<br/>
    /// 所以已经在数据库图元利用事件加入,不要在此加入;<br/>
    /// </param>
    /// <param name="tolerance">鼠标移动的容差</param>
    public JigEx(Action<Point3d, Queue<Entity>>? action = null, double tolerance = 1e-6) : this()
    {
        _mouseAction = action;
        _tolerance = new(tolerance, tolerance);
    }
    #endregion

    #region 方法
    /// <summary>
    /// 鼠标配置:基点
    /// </summary>
    /// <param name="basePoint">基点</param>
    /// <param name="cursorType">光标绑定</param>
    /// <param name="msg">提示信息</param>
    /// <param name="orthomode">正交开关</param>
    public JigPromptPointOptions SetOptions(Point3d basePoint,
                                            CursorType cursorType = CursorType.RubberBand,
                                            string msg = "点选第二点",
                                            bool orthomode = false)
    {
        if (orthomode)
            SystemVariables_Orthomode = true;

        _options = JigPointOptions();
        _options.Message = Environment.NewLine + msg;
        _options.Cursor = cursorType;   // 光标绑定
        _options.UseBasePoint = true;   // 基点打开
        _options.BasePoint = basePoint; // 基点设定
        return _options;
    }

    /// <summary>
    /// 鼠标配置:提示信息,关键字
    /// </summary>
    /// <param name="msg">信息</param>
    /// <param name="keywords">关键字</param>
    /// <param name="orthomode">正交开关</param>
    /// <returns></returns>
    public JigPromptPointOptions SetOptions(string msg,
        Dictionary<string, string>? keywords = null,
        bool orthomode = false)
    {
        if (orthomode)
            SystemVariables_Orthomode = true;

        _options = JigPointOptions();
        _options.Message = Environment.NewLine + msg;

        // 加入关键字,加入时候将空格内容放到最后
        string spaceValue = string.Empty;
        const string spaceKey = " ";

        if (keywords != null)
            foreach (var item in keywords)
                if (item.Key == spaceKey)
                    spaceValue = item.Value;
                else
                    _options.Keywords.Add(item.Key, item.Key, item.Value);

        /// 因为默认配置函数<see cref="JigPointOptions">导致此处空格触发是无效的,
        /// 但是用户如果想触发,就需要在外部减去默认UserInputControls配置
        /// 要放最后,才能优先触发其他关键字
        if (spaceValue != string.Empty)
            _options.Keywords.Add(spaceKey, spaceKey, spaceValue);
        else
            _options.Keywords.Add(spaceKey, spaceKey, "<空格退出>");

        // 外部设置减去配置
        // _options.UserInputControls =
        //         _options.UserInputControls
        //         ^ UserInputControls.NullResponseAccepted     // 输入了鼠标右键,结束jig
        //         ^ UserInputControls.AnyBlankTerminatesInput; // 空格或回车,结束jig;
        return _options;
    }

    /// <summary>
    /// 鼠标配置:自定义
    /// </summary>
    /// <param name="action"></param>
    /// <param name="orthomode">正交开关</param>
    public void SetOptions(Action<JigPromptPointOptions> action, bool orthomode = false)
    {
        if (orthomode)
            SystemVariables_Orthomode = true;

        _options = new JigPromptPointOptions();
        action.Invoke(_options);
    }

    /// <summary>
    /// 执行
    /// </summary>
    /// <returns></returns>
    public PromptResult Drag()
    {
        // jig功能必然是当前前台文档,所以封装内部更好调用
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        var dr = ed.Drag(this);

        if (SystemVariables_Orthomode)
            SystemVariables_Orthomode = !SystemVariables_Orthomode;
        return dr;
    }

    /// <summary>
    /// 最后一次的图元加入数据库
    /// </summary>
    /// <param name="btrOfAddEntitySpace">加入此空间</param>
    /// <param name="removeEntity">不生成的图元用于排除,例如刷新时候的提示文字</param>
    /// <returns>加入数据库的id集合</returns>
    public IEnumerable<ObjectId>? AddEntityToMsPs(BlockTableRecord btrOfAddEntitySpace,
                                                  IEnumerable<Entity>? removeEntity = null)
    {
        // 内部用 _drawEntitys 外部用 Entitys,减少一层转换
        if (_drawEntitys.Count == 0)
            return null;

        IEnumerable<Entity> es = _drawEntitys;
        if (removeEntity != null)
            es = es.Except(removeEntity);// 差集

        return btrOfAddEntitySpace.AddEntity(es);
    }
    #endregion

    #region 重写
    /// <summary>
    /// 鼠标采样器
    /// </summary>
    /// <param name="prompts"></param>
    /// <returns>返回状态:令频繁刷新结束</returns>
    protected override SamplerStatus Sampler(JigPrompts prompts)
    {
        if (_worldDrawFlag)
            return SamplerStatus.NoChange;// OK的时候拖动鼠标与否都不出现图元

        if (_options is null)
            throw new NullReferenceException(nameof(_options));

        var pro = prompts.AcquirePoint(_options);
        if (pro.Status == PromptStatus.Keyword)
            return SamplerStatus.OK;
        else if (pro.Status != PromptStatus.OK)
            return SamplerStatus.Cancel;

        // 上次鼠标点不同(一定要这句,不然图元刷新太快会看到奇怪的边线)
        var mousePointWcs = pro.Value;

        // == 是比较类字段,但是最好转为哈希比较.
        // IsEqualTo 是方形判断(仅加法),但是cad是距离.
        // Distance  是圆形判断(会求平方根,使用了牛顿迭代),
        // 大量数据(十万以上/频繁刷新)面前会显得非常慢.
        if (mousePointWcs.IsEqualTo(MousePointWcsLast, _tolerance))
            return SamplerStatus.NoChange;

        // 上次循环的缓冲区图元清理,否则将会在vs输出遗忘 Dispose
        while (_drawEntitys.Count > 0)
            _drawEntitys.Dequeue().Dispose();

        // 委托把容器扔出去接收新创建的图元,然后给重绘更新
        _mouseAction?.Invoke(mousePointWcs, _drawEntitys);
        MousePointWcsLast = mousePointWcs;

        return SamplerStatus.OK;
    }

    /// <summary>
    /// 重绘已在数据库的图元 
    /// <para> 
    /// 0x01 此处不加入newEntity的,它们在构造函数的参数回调处加入,它们会进行频繁new和Dispose从而避免遗忘释放<br/>
    /// 0x02 此处用于重绘已经在数据的图元<br/>
    /// 0x03 此处用于图元亮显暗显,因为会被重绘冲刷掉所以独立出来不重绘,它们也往往已经存在数据库的
    /// </para>
    /// </summary>
    /// <remarks>
    /// newEntity只会存在一个图元队列中,而数据库图元可以分多个集合
    /// <para> 例如: 集合A亮显时 集合B暗显/集合B亮显时 集合A暗显,所以我没有设计多个"数据库图元集合"存放,而是由用户在构造函数外自行创建</para>
    /// </remarks>
    /// <param name="action"></param>
    public void DatabaseEntityDraw(WorldDrawEvent action)
    {
        WorldDrawEvent = action;
    }

    /* WorldDraw 封装外的操作说明:
     * 0x01
     * 我有一个业务是一次性生成四个方向的箭头,因为cad08缺少瞬时图元,
     * 那么可以先提交一次事务,再开一个事务,把Entity传给jig,最后选择删除部分.
     * 虽然这个是可行的方案,但是Entity穿越事务本身来说是非必要不使用的.
     * 0x02
     * 四个箭头最近鼠标的亮显,其余淡显,
     * 在jig使用淡显ent.Unhighlight和亮显ent.Highlight()
     * 需要绕过重绘,否则重绘将导致图元频闪,令这两个操作失效,
     * 此时需要自定义一个集合 EntityList (不使用本函数的_drawEntitys)
     * 再将 EntityList 传给 WorldDrawEvent 事件,事件内实现亮显和淡显(事件已经利用 DatabaseEntityDraw函数进行提供).
     * 0x03
     * draw.Geometry.Draw(_drawEntitys[i]);
     * 此函数有问题,acad08克隆一份数组也可以用来刷新,
     * 而arx上面的jig只能一次改一个,所以可以用此函数.
     * 起因是此函数属于异步刷新,
     * 同步上下文的刷新是 RawGeometry
     * 0x04
     * cad22测试出现,08不会,
     * draw.RawGeometry.Draw(ent);会跳到 Sampler(),所以设置 _worldDrawFlag
     * 但是禁止重绘重入的话(令图元不频繁重绘),那么鼠标停着的时候就看不见图元,
     * 所以只能重绘结束的时候才允许鼠标采集,采集过程的时候不会触发重绘,
     * 这样才可以保证容器在重绘中不被更改.
     */
    /// <summary>
    /// 重绘图形
    /// </summary>
    protected override bool WorldDraw(WorldDraw draw)
    {
        _worldDrawFlag = true;
        WorldDrawEvent?.Invoke(draw);
        _drawEntitys.ForEach(ent => {
            draw.RawGeometry.Draw(ent);
        });
        _worldDrawFlag = false;
        return true;
    }
    #endregion


    /// <summary>
    /// 用户输入控制默认配置
    /// <para>令jig.Drag().Status == <see cref="PromptStatus.None"/></para>
    /// </summary>
    /// <returns></returns>
    static JigPromptPointOptions JigPointOptions()
    {
        return new JigPromptPointOptions()
        {
            UserInputControls =
                  UserInputControls.GovernedByUCSDetect     // 由UCS探测用
                | UserInputControls.Accept3dCoordinates     // 接受三维坐标
                | UserInputControls.NullResponseAccepted    // 输入了鼠标右键,结束jig
                | UserInputControls.AnyBlankTerminatesInput // 空格或回车,结束jig;
        };
    }

    /// <summary>
    /// 空格默认是<see cref="PromptStatus.None"/>,
    /// <para>将它设置为<see cref="PromptStatus.Keyword"/></para>
    /// </summary>
    public void SetSpaceIsKeyword()
    {
        var opt = _options;
        if (opt == null)
            throw new ArgumentNullException(nameof(_options));

        if ((opt.UserInputControls & UserInputControls.NullResponseAccepted) == UserInputControls.NullResponseAccepted)
            opt.UserInputControls ^= UserInputControls.NullResponseAccepted; // 输入了鼠标右键,结束jig
        if ((opt.UserInputControls & UserInputControls.AnyBlankTerminatesInput) == UserInputControls.AnyBlankTerminatesInput)
            opt.UserInputControls ^= UserInputControls.AnyBlankTerminatesInput; // 空格或回车,结束jig
    }


    #region 注释数据
    /// <summary>
    /// 注释数据,可以在缩放的时候不受影响
    /// </summary>
    public DynamicDimensionDataCollection DimensionEntitys;

    /// <summary>
    /// 重写注释数据
    /// </summary>
    /// <param name="dimScale"></param>
    /// <returns></returns>
    protected override DynamicDimensionDataCollection GetDynamicDimensionData(double dimScale)
    {
        base.GetDynamicDimensionData(dimScale);
        return DimensionEntitys;
    }
    #endregion

    protected override void ViewportDraw(ViewportDraw draw)
    {
        base.ViewportDraw(draw);
    }
}

#if false
| UserInputControls.DoNotEchoCancelForCtrlC        // 不要取消CtrlC的回音
| UserInputControls.DoNotUpdateLastPoint           // 不要更新最后一点
| UserInputControls.NoDwgLimitsChecking            // 没有Dwg限制检查
| UserInputControls.NoZeroResponseAccepted         // 接受非零响应
| UserInputControls.NoNegativeResponseAccepted     // 不否定回复已被接受
| UserInputControls.Accept3dCoordinates            // 返回点的三维坐标,是转换坐标系了?
| UserInputControls.AcceptMouseUpAsPoint           // 接受释放按键时的点而不是按下时

| UserInputControls.InitialBlankTerminatesInput    // 初始 空格或回车,结束jig
| UserInputControls.AcceptOtherInputString         // 接受其他输入字符串
| UserInputControls.NoZDirectionOrtho              // 无方向正射,直接输入数字时以基点到当前点作为方向
| UserInputControls.UseBasePointElevation          // 使用基点高程,基点的Z高度探测
#endif