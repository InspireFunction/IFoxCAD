using IFoxFunKit;

namespace Test;

/// <summary>
/// IFoxFunKit 正确用法演示命令
/// 以下命令展示了 IFoxFunKit 中 Option&lt;T&gt; 和 Result&lt;T&gt; 的正确使用方式
/// 每个示例都对应 IFoxFunKit.TestNet35Analyzers/Program.cs 中的正确案例
/// </summary>
public class TestIFoxFunKitDemoCommands
{
    #region 辅助方法

    /// <summary>
    /// 安全除法 - 返回 Option&lt;int&gt;
    /// </summary>
    static Option<int> Divide(int a, int b)
    {
        if (b == 0)
            return Option<int>.None;
        return Option<int>.Some(a / b);
    }

    /// <summary>
    /// 安全除法 - 返回 Result&lt;int&gt;
    /// </summary>
    static Result<int> SafeDivide(int a, int b)
    {
        if (b == 0)
            return Result<int>.Err(new System.Exception("除数不能为零"));
        return Result<int>.Ok(a / b);
    }

    /// <summary>
    /// 尝试获取图层 - 返回 Option&lt;LayerTableRecord&gt;
    /// </summary>
    static Option<LayerTableRecord> TryGetLayer(DBTrans tr, string layerName)
    {
        var layerId = tr.LayerTable[layerName];
        if (layerId.IsNull)
            return Option<LayerTableRecord>.None;
        var layer = layerId.GetObject<LayerTableRecord>();
        if (layer == null)
            return Option<LayerTableRecord>.None;
        return Option<LayerTableRecord>.Some(layer);
    }

    #endregion

    #region Option<T> 正确用法示例

    /// <summary>
    /// 正确用法1: 检查 IsSome 并处理结果
    /// 对应 Program.cs 中的 Example1_Correct
    /// </summary>
    [CommandMethod(nameof(Test_IFoxFunKit_Option_IsSome))]
    public void Test_IFoxFunKit_Option_IsSome()
    {
        var result = Divide(10, 0);
        if (result.IsSome)
            Env.Printl($"用法1 - 检查结果: 结果 = {result.Value}");
        else
            Env.Printl("用法1 - 检查结果: 除数为零");
    }

    /// <summary>
    /// 正确用法2: 使用 Match 方法处理 Some 和 None 两种情况
    /// 对应 Program.cs 中的 Example2_Correct
    /// </summary>
    [CommandMethod(nameof(Test_IFoxFunKit_Option_Match))]
    public void Test_IFoxFunKit_Option_Match()
    {
        var result = Divide(10, 2);
        result.Match(
            some: v => Env.Printl($"用法2 - Match处理: 结果 = {v}"),
            none: () => Env.Printl("用法2 - Match处理: 除数为零")
        );
    }

    /// <summary>
    /// 正确用法3: 赋值后检查 IsNone 并处理
    /// 对应 Program.cs 中的 Example3_Correct
    /// </summary>
    [CommandMethod(nameof(Test_IFoxFunKit_Option_IsNone))]
    public void Test_IFoxFunKit_Option_IsNone()
    {
        Option<int> result = Divide(10, 2);
        if (result.IsNone)
            Env.Printl("用法3 - 检查IsNone: 除数为零");
        else
            Env.Printl($"用法3 - 检查IsNone: 结果 = {result.Value}");
    }

    /// <summary>
    /// 额外用法: 使用 UnwrapOr 提供默认值
    /// </summary>
    [CommandMethod(nameof(Test_IFoxFunKit_Option_UnwrapOr))]
    public void Test_IFoxFunKit_Option_UnwrapOr()
    {
        var result = Divide(10, 0).UnwrapOr(0);
        Env.Printl($"额外用法 - UnwrapOr: 结果 = {result} (除零时返回默认值0)");
    }

    /// <summary>
    /// 额外用法: 使用 Bind 进行链式操作
    /// </summary>
    [CommandMethod(nameof(Test_IFoxFunKit_Option_Bind))]
    public void Test_IFoxFunKit_Option_Bind()
    {
        var result = Divide(20, 4)
            .Bind(x => Divide(x, 2))
            .Match(
                some: v => $"额外用法 - Bind链式: 20/4/2 = {v}",
                none: () => "额外用法 - Bind链式: 计算失败"
            );
        Env.Printl(result);
    }

    /// <summary>
    /// CAD场景示例: 安全获取图层
    /// </summary>
    [CommandMethod(nameof(Test_IFoxFunKit_Option_CAD_Layer))]
    public void Test_IFoxFunKit_Option_CAD_Layer()
    {
        using var tr = DBTrans.Create();

        var layerOpt = TryGetLayer(tr, "0");
        layerOpt.Match(
            some: layer => Env.Printl($"找到图层: {layer.Name}, 颜色: {layer.Color}"),
            none: () => Env.Printl("未找到指定图层")
        );
    }

    #endregion

    #region Result<T> 正确用法示例

    /// <summary>
    /// 正确用法4: 使用 Match 方法处理 Ok 和 Err 两种情况
    /// 对应 Program.cs 中的 Example4_Correct
    /// </summary>
    [CommandMethod(nameof(Test_IFoxFunKit_Result_Match))]
    public void Test_IFoxFunKit_Result_Match()
    {
        var result = SafeDivide(10, 0);
        result.Match(
            ok: v => Env.Printl($"用法4 - Result Match: 结果 = {v}"),
            err: e => Env.Printl($"用法4 - Result Match: 错误 = {e.Message}")
        );
    }

    /// <summary>
    /// 正确用法5: 检查 IsOk 并处理结果
    /// 对应 Program.cs 中的 Example5_Correct
    /// </summary>
    [CommandMethod(nameof(Test_IFoxFunKit_Result_IsOk))]
    public void Test_IFoxFunKit_Result_IsOk()
    {
        var result = SafeDivide(10, 2);
        if (result.IsOk)
            Env.Printl($"用法5 - 检查IsOk: 结果 = {result.OkValue}");
        else
            Env.Printl($"用法5 - 检查IsOk: 错误 = {result.ErrValue.Message}");
    }

    /// <summary>
    /// 额外用法: 使用 Map 转换成功值
    /// </summary>
    [CommandMethod(nameof(Test_IFoxFunKit_Result_Map))]
    public void Test_IFoxFunKit_Result_Map()
    {
        var result = SafeDivide(100, 5)
            .Map(x => x * 2)
            .Match(
                ok: v => $"额外用法 - Map转换: 100/5*2 = {v}",
                err: e => $"额外用法 - Map转换: 错误 = {e.Message}"
            );
        Env.Printl(result);
    }

    /// <summary>
    /// 额外用法: 使用 UnwrapOrElse 处理错误
    /// </summary>
    [CommandMethod(nameof(Test_IFoxFunKit_Result_UnwrapOrElse))]
    public void Test_IFoxFunKit_Result_UnwrapOrElse()
    {
        var result = SafeDivide(10, 0)
            .UnwrapOrElse(e => -1);
        Env.Printl($"额外用法 - UnwrapOrElse: 结果 = {result} (错误时返回-1)");
    }

    /// <summary>
    /// CAD场景示例: 安全创建圆
    /// </summary>
    [CommandMethod(nameof(Test_IFoxFunKit_Result_CAD_Circle))]
    public void Test_IFoxFunKit_Result_CAD_Circle()
    {
        using var tr = DBTrans.Create();

        // 尝试获取半径（模拟可能失败的计算）
        Result<double> radiusResult = GetRadiusFromUser();

        radiusResult.Match(
            ok: radius => {
                var circle = new Circle(Point3d.Origin, Vector3d.ZAxis, radius);
                tr.CurrentSpace.AddEntity(circle);
                Env.Printl($"成功创建圆，半径: {radius}");
            },
            err: e => {
                Env.Printl($"创建圆失败: {e.Message}");
            }
        );
    }

    /// <summary>
    /// 模拟从用户获取半径（可能失败）
    /// </summary>
    static Result<double> GetRadiusFromUser()
    {
        // 模拟：随机成功或失败
        var random = new Random();
        if (random.Next(2) == 0)
            return Result<double>.Err(new System.Exception("用户取消了输入"));
        return Result<double>.Ok(random.Next(10, 100));
    }

    #endregion

    #region 综合演示

    /// <summary>
    /// IFoxFunKit 综合演示命令
    /// </summary>
    [CommandMethod(nameof(Test_IFoxFunKit_Demo))]
    public void Test_IFoxFunKit_Demo()
    {
        Env.Printl("=== IFoxFunKit 正确用法演示 ===");
        Env.Printl("");

        // Option<T> 演示
        Env.Printl("【Option<T> 演示】");
        Env.Printl("-------------------");

        var opt1 = Divide(10, 0);
        opt1.Match(
            some: v => Env.Printl($"10/0 = {v}"),
            none: () => Env.Printl("10/0 = None (除数为零)")
        );

        var opt2 = Divide(10, 2);
        opt2.Match(
            some: v => Env.Printl($"10/2 = {v}"),
            none: () => Env.Printl("10/2 = None")
        );

        var opt3 = Divide(20, 4).UnwrapOr(0);
        Env.Printl($"20/4 使用 UnwrapOr = {opt3}");

        Env.Printl("");

        // Result<T> 演示
        Env.Printl("【Result<T> 演示】");
        Env.Printl("-------------------");

        var res1 = SafeDivide(10, 0);
        res1.Match(
            ok: v => Env.Printl($"10/0 = {v}"),
            err: e => Env.Printl($"10/0 = Err: {e.Message}")
        );

        var res2 = SafeDivide(10, 2);
        res2.Match(
            ok: v => Env.Printl($"10/2 = {v}"),
            err: e => Env.Printl($"10/2 = Err: {e.Message}")
        );

        var res3 = SafeDivide(100, 5).Map(x => x * 2);
        res3.Match(
            ok: v => Env.Printl($"100/5*2 = {v}"),
            err: e => Env.Printl($"100/5*2 = Err: {e.Message}")
        );

        Env.Printl("");
        Env.Printl("=== 演示完成 ===");
    }

    #endregion
}
