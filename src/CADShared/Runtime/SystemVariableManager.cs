// ReSharper disable InconsistentNaming

namespace IFoxCAD.Cad;

/// <summary>
/// 系统变量管理器
/// </summary>
public static class SystemVariableManager
{
    #region A

    /// <summary>
    /// 打开或关闭自动捕捉靶框的显示
    /// </summary>
    public static bool ApBox
    {
        get => Convert.ToBoolean(Acaop.GetSystemVariable(nameof(ApBox)));
        set => Acaop.SetSystemVariable(nameof(ApBox), Convert.ToInt32(value));
    }

    /// <summary>
    /// 对象捕捉靶框的大小，范围[1,50]
    /// </summary>
    public static int Aperture
    {
        get => Convert.ToInt32(Acaop.GetSystemVariable(nameof(Aperture)));
        set => Acaop.SetSystemVariable(nameof(Aperture), value);
    }

    /// <summary>
    /// 图形单位-角度-类型，范围[0-十进制度数,1-度/分/秒,2-百分度,3-弧度,4-勘测单位]
    /// </summary>
    public static int Aunits
    {
        get => Convert.ToInt32(Acaop.GetSystemVariable(nameof(Aunits)));
        set => Acaop.SetSystemVariable(nameof(Aunits), value);
    }

    /// <summary>
    /// 图形单位-角度-精度，范围<c>[0,8]</c>
    /// </summary>
    public static int Auprec
    {
        get => Convert.ToInt32(Acaop.GetSystemVariable(nameof(Auprec)));
        set => Acaop.SetSystemVariable(nameof(Auprec), value);
    }

    #endregion

    #region B

    /// <summary>
    /// 是否在块编辑器中
    /// </summary>
    public static bool BlockEditor => Acaop.GetSystemVariable(nameof(BlockEditor)) is 1;

    #endregion

    #region C

    /// <summary>
    /// 用于设置当前空间的当前注释比例的值
    /// </summary>
    public static string CanNoScale => Acaop.GetSystemVariable(nameof(CanNoScale)).ToString()!;

    /// <summary>
    /// 用于显示当前的注释性比例
    /// </summary>
    public static double CanNoScaleValue =>
        Convert.ToDouble(Acaop.GetSystemVariable(nameof(CanNoScale)));

    /// <summary>
    /// 储存以公元纪年为基准的日历数据和时间
    /// </summary>
    public static double CDate => Convert.ToDouble(Acaop.GetSystemVariable(nameof(CDate)));

    /// <summary>
    /// 设置新对象的颜色
    /// </summary>
    public static string CEColor
    {
        get => Acaop.GetSystemVariable(nameof(CEColor)).ToString()!;
        set => Acaop.SetSystemVariable(nameof(CEColor), value);
    }

    /// <summary>
    /// 设置新对象的线型比例因子
    /// </summary>
    public static double CELtScale
    {
        get => Convert.ToDouble(Acaop.GetSystemVariable(nameof(CELtScale)));
        set => Acaop.SetSystemVariable(nameof(CELtScale), value);
    }

    /// <summary>
    /// 设置新对象的线型
    /// </summary>
    public static string CELType
    {
        get => Acaop.GetSystemVariable(nameof(CELType)).ToString()!;
        set => Acaop.SetSystemVariable(nameof(CELType), value);
    }

    /// <summary>
    /// 设置新对象的线宽
    /// </summary>
    public static double CELWeight
    {
        get => Convert.ToDouble(Acaop.GetSystemVariable(nameof(CELWeight)));
        set => Acaop.SetSystemVariable(nameof(CELWeight), value);
    }

    /// <summary>
    /// 设置圆的默认半径
    /// </summary>
    public static double CircleRad
    {
        get => Convert.ToDouble(Acaop.GetSystemVariable(nameof(CircleRad)));
        set => Acaop.SetSystemVariable(nameof(CircleRad), value);
    }

    /// <summary>
    /// 设置当前图层
    /// </summary>
    public static string CLayer
    {
        get => Acaop.GetSystemVariable(nameof(CLayer)).ToString()!;
        set => Acaop.SetSystemVariable(nameof(CLayer), value);
    }

    /// <summary>
    /// 用于确定全屏显示是打开或关闭状态
    /// </summary>
    public static bool CleanScreenState =>
        Convert.ToBoolean(Acaop.GetSystemVariable(nameof(CleanScreenState)));

    /// <summary>
    /// 指示命令窗口是隐藏还是显示状态
    /// </summary>
    public static bool CliState => Convert.ToBoolean(Acaop.GetSystemVariable(nameof(CliState)));

    /// <summary>
    /// 存在活动命令
    /// </summary>
    public static bool CmdActive => Convert.ToBoolean(Acaop.GetSystemVariable(nameof(CmdActive)));

    /// <summary>
    /// 控制是否要打开对话框来显示命令
    /// </summary>
    public static bool CmdDia
    {
        get => Convert.ToBoolean(Acaop.GetSystemVariable(nameof(CmdDia)));
        set => Acaop.SetSystemVariable(nameof(CmdDia), Convert.ToInt32(value));
    }

    /// <summary>
    /// 在使用 LISP 的函数时，切换回应为打开或关闭
    /// </summary>
    public static bool CmdEcho
    {
        get => Convert.ToBoolean(Acaop.GetSystemVariable(nameof(CmdEcho)));
        set => Acaop.SetSystemVariable(nameof(CmdEcho), value ? 1 : 0);
    }

    /// <summary>
    /// 当前的命令
    /// </summary>
    public static string CmdNames => Acaop.GetSystemVariable(nameof(CmdNames)).ToString()!;

    /// <summary>
    /// 返回图形中的当前选项卡（模型或布局）的名称
    /// </summary>
    public static string CTab
    {
        get => Acaop.GetSystemVariable(nameof(CTab)).ToString()!;
        set => Acaop.SetSystemVariable(nameof(CTab), value);
    }

    /// <summary>
    /// 指定十字光标的显示大小。输入的数字代表十字光标相对于屏幕的比例。<br/>
    /// 范围[1, 100]
    /// </summary>
    public static int CursorSize
    {
        get => Convert.ToInt32(Acaop.GetSystemVariable(nameof(CursorSize)));
        set => Acaop.SetSystemVariable(nameof(CursorSize), value);
    }

    /// <summary>
    /// 当前viewport的编号
    /// </summary>
    public static short CVPort
    {
        get => Convert.ToInt16(Acaop.GetSystemVariable(nameof(CVPort)));
        set => Acaop.SetSystemVariable(nameof(CVPort), value);
    }

    #endregion

    #region D

    /// <summary>
    /// 是否开启双击
    /// </summary>
    public static bool DblClick
    {
        get => Convert.ToBoolean(Acaop.GetSystemVariable(nameof(DblClick)));
        set => Acaop.SetSystemVariable(nameof(DblClick), Convert.ToInt32(value));
    }

    /// <summary>
    /// 指示图形的修改状态
    /// </summary>
    public static DBmod DBMod => (DBmod)Acaop.GetSystemVariable(nameof(DBMod));

    /// <summary>
    /// 动态输入
    /// </summary>
    public static int DynMode
    {
        get => Convert.ToInt32(Acaop.GetSystemVariable(nameof(DynMode)));
        set => Acaop.SetSystemVariable(nameof(DynMode), value);
    }

    /// <summary>
    /// 动态提示
    /// </summary>
    public static bool DynPrompt
    {
        get => Convert.ToBoolean(Acaop.GetSystemVariable(nameof(DynPrompt)));
        set => Acaop.SetSystemVariable(nameof(DynPrompt), Convert.ToInt32(value));
    }

    #endregion

    #region G

    /// <summary>
    /// 显示图形栅格
    /// </summary>
    public static bool GridMode
    {
        get => Acaop.GetSystemVariable(nameof(GridMode)) is 1;
        set => Acaop.SetSystemVariable(nameof(GridMode), Convert.ToInt32(value));
    }

    #endregion

    #region H

    /// <summary>
    /// 当前填充的名称
    /// </summary>
    public static string HPName
    {
        get => Acaop.GetSystemVariable(nameof(HPName)).ToString()!;
        set => Acaop.SetSystemVariable(nameof(HPName), value);
    }

    /// <summary>
    /// 填充比例
    /// </summary>
    public static double HPScale
    {
        get => Convert.ToDouble(Acaop.GetSystemVariable(nameof(HPScale)));
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(HPScale), "HPScale必须大于0");
            Acaop.SetSystemVariable(nameof(HPScale), value);
        }
    }

    #endregion

    #region I

    /// <summary>
    /// 图形单位-插入时的缩放单位
    /// </summary>
    public static UnitsValue Insunits
    {
        get => (UnitsValue)Acaop.GetSystemVariable(nameof(Insunits));
        set => Acaop.SetSystemVariable(nameof(Insunits), (int)value);
    }

    #endregion

    #region L

    /// <summary>
    /// 储存所输入相对于当前用户坐标系统(UCS)的最后点的值
    /// </summary>
    public static Point3d LastPoint
    {
        get => (Point3d)Acaop.GetSystemVariable(nameof(LastPoint));
        set => Acaop.SetSystemVariable(nameof(LastPoint), value);
    }

    /// <summary>
    /// 图形单位-长度-类型，范围[1-科学,2-小数,3-工程,4-建筑,5-分数]
    /// </summary>
    public static int Lunits
    {
        get => Convert.ToInt32(Acaop.GetSystemVariable(nameof(Lunits)));
        set => Acaop.SetSystemVariable(nameof(Lunits), value);
    }

    /// <summary>
    /// 图形单位-长度-精度，范围<c>[0,8]</c>
    /// </summary>
    public static int Luprec
    {
        get => Convert.ToInt32(Acaop.GetSystemVariable(nameof(Luprec)));
        set => Acaop.SetSystemVariable(nameof(Luprec), value);
    }

    #endregion

    #region M

    /// <summary>
    /// 图形单位
    /// </summary>
    public static MeasurementValue Measurement
    {
        get => (MeasurementValue)Acaop.GetSystemVariable(nameof(Measurement));
        set => Acaop.SetSystemVariable(nameof(Measurement), Convert.ToInt32(value));
    }

    #endregion

    #region O

    /// <summary>
    /// 正交
    /// </summary>
    public static bool OrthoMode
    {
        get => Convert.ToBoolean(Acaop.GetSystemVariable(nameof(OrthoMode)));
        set => Acaop.SetSystemVariable(nameof(OrthoMode), Convert.ToInt32(value));
    }

    #endregion

    #region P

    /// <summary>
    /// 允许先选择后执行
    /// </summary>
    public static bool PickFirst
    {
        get => Convert.ToBoolean(Acaop.GetSystemVariable(nameof(PickFirst)));
        set => Acaop.SetSystemVariable(nameof(PickFirst), Convert.ToInt32(value));
    }

    #endregion

    #region T

    /// <summary>
    /// 视图点
    /// </summary>
    public static Point3d Target => (Point3d)Acaop.GetSystemVariable(nameof(Target));

    #endregion
}