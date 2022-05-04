/*  封装jig
 *  20220503 cad22需要防止刷新过程中更改队列,08不会有.
 *  20220326 重绘图元的函数用错了,现在修正过来
 *  20211216 加入块表时候做一个差集,剔除临时图元
 *  20211209 补充正交变量设置和回收设置
 *  作者: 惊惊⎛⎝◕⏝⏝◕｡⎠⎞ ⎛⎝≥⏝⏝0⎠⎞ ⎛⎝⓿⏝⏝⓿｡⎠⎞ ⎛⎝≥⏝⏝≤⎠⎞
 *  博客: https://www.cnblogs.com/JJBox/p/15650770.html
 *
 *  例子1:
 *  var ptjig = new Jig();
 *  ptjig.SetOptions(Point3d.Origin);
 *  var pr = ptjig.Drag();
 *  if (pr.Status != PromptStatus.OK)
 *      return null;
 *
 *  例子2:
 *  var ppo1 = new PromptPointOptions(Environment.NewLine + "输入矩形角点1:<空格退出>")
 *  {
 *      AllowArbitraryInput = true,//任意输入
 *      AllowNone           = true //允许回车
 *  };
 *  var ppr1 = ed.GetPoint(ppo1);//用户点选
 *  if (ppr1.Status != PromptStatus.OK)
 *      return;
 *  var getPt = ppr1.Value;
 *
 *  var recEntityJig = new Jig((mousePoint, drawEntitys) => {
 *      #region 画柜子图形
 *      double length = Math.Abs(getPt.X - mousePoint.X);
 *      double high = Math.Abs(getPt.Y - mousePoint.Y);
 *      var ent = AddRecToEntity(Point3d.Origin, new Point3d(length, high, 0));
 *      drawEntitys.Enqueue(ent);
 *      #endregion
 *  });
 *  recEntityJig.SetOptions("指定矩形角点:", new Dictionary<string, string>() { { "Z", "中间(Z)" } );
 *
 *  bool flag = true;
 *  while (flag)
 *  {
 *      var pr = recEntityJig.Drag();
 *      if (string.IsNullOrEmpty(pr.StringResult))//在无输入的时候会等于空
 *          flag = false;
 *      else
 *      {
 *          switch (pr.StringResult.ToUpper()) //注意cad保留 https://www.cnblogs.com/JJBox/p/10224631.html
 *          {
 *              case "Z":
 *                  ed.WriteMessage("\n您触发了z关键字");
 *                  break;
 *              case " ":
 *                  flag = false;//空格结束
 *                  break;
 *          }
 *      }
 *  }
 *  //开启事务之后,图元加入数据库
 *  db.Action(tr=>{
 *     recEntityJig.AddEntityToMsPs(tr);
 *  });
 */

namespace IFoxCAD.Cad;

using Autodesk.AutoCAD.GraphicsInterface;
using Acap = Autodesk.AutoCAD.ApplicationServices.Application;

public delegate void WorldDrawEvent(WorldDraw draw);
public class Jig : DrawJig
{
    #region 成员
    /// <summary>
    /// 事件:默认是图元刷新,其余的:亮显/暗显等等工作自由补充
    /// </summary>
    public event WorldDrawEvent? WorldDrawEvent;
    /// <summary>
    /// 最后的鼠标点,用来确认长度
    /// </summary>
    public Point3d MousePointWcsLast;
    /// <summary>
    /// 最后的图元,用来生成
    /// </summary>
    public Entity[] Entitys => _drawEntitys.ToArray();

    Autodesk.AutoCAD.Geometry.Tolerance _tolerance;

    Queue<Entity> _drawEntitys;//重复生成的图元,放在这里刷新
    Action<Point3d, Queue<Entity>>? _action;
    JigPromptPointOptions? _options;
    const string _orthomode = "orthomode";
    bool _systemVariablesOrthomode = false; //正交修改
    bool _worldDrawFlag = false; // 防止刷新过程中更改队列
    #endregion

    #region 构造
    /// <summary>
    /// 在界面绘制图元
    /// </summary>
    /// <param name="action">
    /// 用来频繁执行的回调: <see langword="Point3d"/>鼠标点,<see langword="List"/>加入显示图元的容器
    /// </param>
    /// <param name="tolerance">鼠标移动的容差</param>
    public Jig(Action<Point3d, Queue<Entity>>? action = null, double tolerance = 1e-6)
    {
        _action = action;
        _tolerance = new(tolerance, tolerance);
        _drawEntitys = new();
    }
    #endregion

    #region 方法



    /// <summary>
    /// 鼠标配置:基点
    /// </summary>
    /// <param name="basePoint">基点</param>
    /// <param name="msg">提示信息</param>
    /// <param name="cursorType">光标绑定</param>
    /// <param name="orthomode">正交开关</param>
    public JigPromptPointOptions SetOptions(Point3d basePoint,
                                            CursorType cursorType = CursorType.RubberBand,
                                            string msg = "点选第二点",
                                            bool orthomode = false)
    {
        if (orthomode && CadSystem.Getvar(_orthomode) != "1")
        {
            CadSystem.Setvar(_orthomode, "1");//1正交,0非正交 //setvar: https://www.cnblogs.com/JJBox/p/10209541.html
            _systemVariablesOrthomode = true;
        }
        var tmp = new JigPromptPointOptions(Environment.NewLine + msg)
        {
            Cursor = cursorType,   //光标绑定
            UseBasePoint = true,   //基点打开
            BasePoint = basePoint, //基点设定

            //用户输入控件:  由UCS探测用 | 接受三维坐标
            UserInputControls =
                UserInputControls.GovernedByUCSDetect |
                UserInputControls.Accept3dCoordinates
        };
        _options = tmp;
        return _options;
    }

    /// <summary>
    /// 鼠标配置:提示信息,关键字
    /// </summary>
    /// <param name="msg">信息</param>
    /// <param name="keywords">关键字</param>
    /// <param name="orthomode">正交开关</param>
    /// <returns></returns>
    public JigPromptPointOptions SetOptions(string msg, Dictionary<string, string>? keywords = null, bool orthomode = false)
    {
        if (orthomode && CadSystem.Getvar(_orthomode) != "1")
        {
            CadSystem.Setvar(_orthomode, "1");//1正交,0非正交 //setvar: https://www.cnblogs.com/JJBox/p/10209541.html
            _systemVariablesOrthomode = true;
        }

        var tmp = new JigPromptPointOptions(Environment.NewLine + msg)
        {
            //用户输入控件:  由UCS探测用 | 接受三维坐标
            UserInputControls =
                  UserInputControls.GovernedByUCSDetect |
                  UserInputControls.Accept3dCoordinates
        };

        //加入关键字,加入时候将空格内容放到最后
        string spaceValue = string.Empty;
        const string spaceKey = " ";
        if (keywords != null)
        {
            var ge = keywords.GetEnumerator();
            while (ge.MoveNext())
            {
                if (ge.Current.Key == spaceKey)
                    spaceValue = ge.Current.Value;
                else
                    tmp.Keywords.Add(ge.Current.Key, ge.Current.Key, ge.Current.Value);
            }
        }

        //要放最后,才能优先触发其他关键字
        if (spaceValue != string.Empty)
            tmp.Keywords.Add(spaceKey, spaceKey, spaceValue);
        else
            tmp.Keywords.Add(spaceKey, spaceKey, "<空格退出>");

        _options = tmp;
        return _options;
    }

    /// <summary>
    /// 鼠标配置:自定义
    /// </summary>
    /// <param name="action"></param>
    /// <param name="orthomode">正交开关</param>
    public void SetOptions(Action<JigPromptPointOptions> action, bool orthomode = false)
    {
        var tmp = new JigPromptPointOptions();
        action.Invoke(tmp);
        _options = tmp;

        if (orthomode && CadSystem.Getvar(_orthomode) != "1")
        {
            CadSystem.Setvar(_orthomode, "1");//1正交,0非正交 //setvar: https://www.cnblogs.com/JJBox/p/10209541.html
            _systemVariablesOrthomode = true;
        }
    }

    /// <summary>
    /// 执行
    /// </summary>
    /// <returns></returns>
    public PromptResult Drag()
    {
        //jig功能必然是当前前台文档,所以封装内部更好调用
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        var dr = ed.Drag(this);
        if (_systemVariablesOrthomode)
            CadSystem.Setvar(_orthomode, "0");//1正交,0非正交 //setvar: https://www.cnblogs.com/JJBox/p/10209541.html
        return dr;
    }

    /// <summary>
    /// 最后一次的图元加入数据库
    /// </summary>
    /// <param name="tr">事务</param>
    /// <param name="removeEntity">不生成的图元用于排除,例如刷新时候的提示文字</param>
    /// <returns></returns>
    public IEnumerable<ObjectId>? AddEntityToMsPs(DBTrans tr,
        IEnumerable<Entity>? removeEntity = null)
    {
        var ents = Entitys;
        if (ents.Length == 0)
            return null;

        var ids = new List<ObjectId>();
        IEnumerable<Entity> es = ents;
        if (removeEntity != null)
            es = es.Except(removeEntity);

        var ge = es.GetEnumerator();
        while (ge.MoveNext())
            ids.Add(tr.CurrentSpace.AddEntity(ge.Current));

        return ids;
    }

    #endregion

    #region 重写
    /// <summary>
    /// 鼠标频繁采点
    /// </summary>
    /// <param name="prompts"></param>
    /// <returns>返回状态:令频繁刷新结束</returns>
    protected override SamplerStatus Sampler(JigPrompts prompts)
    {
        if (_worldDrawFlag)
            return SamplerStatus.NoChange;//OK的时候拖动鼠标与否都不出现图元

        if (_options is null)
            throw new ArgumentNullException(nameof(_options));

        var pro = prompts.AcquirePoint(_options);
        if (pro.Status == PromptStatus.Keyword)
            return SamplerStatus.OK;
        else if (pro.Status != PromptStatus.OK)
            return SamplerStatus.Cancel;

        //上次鼠标点不同(一定要这句,不然图元刷新太快会看到奇怪的边线)
        var mousePointWcs = pro.Value;

        //== 是比较类字段,但是最好转为哈希比较.
        //IsEqualTo 是方形判断(仅加法),但是cad是距离.
        //Distance  是圆形判断(会求平方根,使用了牛顿迭代),
        //大量数据(十万以上/频繁刷新)面前会显得非常慢.
        if (mousePointWcs.IsEqualTo(MousePointWcsLast, _tolerance))
            return SamplerStatus.NoChange;

        //上次循环的缓冲区图元清理,否则将会在vs输出遗忘 Dispose
        while (_drawEntitys.Count > 0)
            _drawEntitys.Dequeue().Dispose();

        //委托把容器扔出去接收新创建的图元,然后给重绘更新
        _action?.Invoke(mousePointWcs, _drawEntitys);
        MousePointWcsLast = mousePointWcs;

        return SamplerStatus.OK;
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
     * 再将 EntityList 传给 WorldDrawEvent 事件,事件内实现亮显和淡显.
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
}




class CadSystem
{
/* 此类函数和env类重复
 * todo:计划删除
 */
    /// <summary>
    /// 获取系统变量值
    /// </summary>
    /// <param name="name">变量名</param>
    /// <returns>成功获取值,失败null</returns>
    public static string? Getvar(string name)
    {
        return Acap.GetSystemVariable(name).ToString();
    }
    /// <summary>
    /// 设置系统或环境变量
    /// </summary>
    /// <param name="name">变量名</param>
    /// <param name="parameter">变量值</param>
    /// <returns>成功设置返回值,失败null</returns>
    public static void Setvar(string? name, string? parameter)
    {
        if (name is null)
            throw new ArgumentNullException(nameof(name));
        if (parameter is null)
            throw new ArgumentNullException(nameof(parameter));

        try
        {
            //改系统变量
            var value = Acap.GetSystemVariable(name);
            if (value is null) return;
            var valueTypeName = value.GetType().Name;
            //如果出现了clayer无法设置,是没有锁文档导致的
            switch (valueTypeName)
            {
                case "String":
                    Acap.SetSystemVariable(name, parameter.Replace("\"", ""));//去掉引号
                    break;
                case "Double":
                    Acap.SetSystemVariable(name, double.Parse(parameter));
                    break;
                case "Int16":
                    Acap.SetSystemVariable(name, short.Parse(parameter));
                    break;
                case "Int32":
                    Acap.SetSystemVariable(name, int.Parse(parameter));
                    break;
            }
        }
        catch
        { }
    }
}

#if false
| UserInputControls.NullResponseAccepted           //接受空响应
| UserInputControls.DoNotEchoCancelForCtrlC        //不要取消CtrlC的回音
| UserInputControls.DoNotUpdateLastPoint           //不要更新最后一点
| UserInputControls.NoDwgLimitsChecking            //没有Dwg限制检查
| UserInputControls.NoZeroResponseAccepted         //接受非零响应
| UserInputControls.NoNegativeResponseAccepted     //不否定回复已被接受
| UserInputControls.Accept3dCoordinates            //返回点的三维坐标,是转换坐标系了?
| UserInputControls.AcceptMouseUpAsPoint           //接受释放按键时的点而不是按下时
| UserInputControls.AnyBlankTerminatesInput        //任何空白终止输入
| UserInputControls.InitialBlankTerminatesInput    //初始空白终止输入
| UserInputControls.AcceptOtherInputString         //接受其他输入字符串
| UserInputControls.NoZDirectionOrtho              //无方向正射,直接输入数字时以基点到当前点作为方向
| UserInputControls.UseBasePointElevation          //使用基点高程,基点的Z高度探测
#endif