namespace Test;

public class TestLisp
{
    // 定义lisp函数
    [LispFunction(nameof(LispTest_RunLisp))]
    public static object LispTest_RunLisp(ResultBuffer rb)
    {
        CmdTest_RunLisp();
        return null!;
    }

    // 模态命令,只有当CAD发出命令提示或当前没有其他的命令或程序活动的时候才可以被触发
    [CommandMethod("CmdTest_RunLisp1")]
    // 透明命令,可以在一个命令提示输入的时候触发例如正交切换,zoom等
    [CommandMethod("CmdTest_RunLisp2", CommandFlags.Transparent)]
    // 选择图元之后执行命令将可以从 <see cref="Editor.GetSelection()"/> 获取图元
    [CommandMethod("CmdTest_RunLisp3", CommandFlags.UsePickSet)]
    // 命令执行前已选中部分实体.在命令执行过程中这些标记不会被清除
    [CommandMethod("CmdTest_RunLisp4", CommandFlags.Redraw)]
    // 命令不能在透视图中使用
    [CommandMethod("CmdTest_RunLisp5", CommandFlags.NoPerspective)]
    // 命令不能通过 MULTIPLE命令 重复触发
    [CommandMethod("CmdTest_RunLisp6", CommandFlags.NoMultiple)]
    // 不允许在模型空间使用命令
    [CommandMethod("CmdTest_RunLisp7", CommandFlags.NoTileMode)]
    // 不允许在布局空间使用命令
    [CommandMethod("CmdTest_RunLisp8", CommandFlags.NoPaperSpace)]
    // 命令不能在OEM产品中使用
    [CommandMethod("CmdTest_RunLisp9", CommandFlags.NoOem)]
    // 不能直接使用命令名调用,必须使用   组名.全局名  调用
    [CommandMethod("CmdTest_RunLisp10", CommandFlags.Undefined)]
    // 定义lisp方法.已废弃   请使用lispfunction
    [CommandMethod("CmdTest_RunLisp11", CommandFlags.Defun)]
    // 命令不会被存储在新的命令堆上
    [CommandMethod("CmdTest_RunLisp12", CommandFlags.NoNewStack)]
    // 命令不能被内部锁定(命令锁)
    [CommandMethod("CmdTest_RunLisp13", CommandFlags.NoInternalLock)]
    // 调用命令的文档将会被锁定为只读
    [CommandMethod("CmdTest_RunLisp14", CommandFlags.DocReadLock)]
    // 调用命令的文档将会被锁定,类似document.lockdocument
    [CommandMethod("CmdTest_RunLisp15", CommandFlags.DocExclusiveLock)]
    // 命令在CAD运行期间都能使用,而不只是在当前文档
    [CommandMethod("CmdTest_RunLisp16", CommandFlags.Session)]
    // 获取用户输入时,可以与属性面板之类的交互
    [CommandMethod("CmdTest_RunLisp17", CommandFlags.Interruptible)]
    // 命令不会被记录在命令历史记录
    [CommandMethod("CmdTest_RunLisp18", CommandFlags.NoHistory)]
#if (!zcad)
    // 命令不会被 UNDO取消
    [CommandMethod("CmdTest_RunLisp19", CommandFlags.NoUndoMarker)]
    // 不能在参照块中使用命令
    [CommandMethod("CmdTest_RunLisp20", CommandFlags.NoBlockEditor)]

    // acad09增,不会被动作录制器 捕捉到
    [CommandMethod("CmdTest_RunLisp21", CommandFlags.NoActionRecording)]
    // acad09增,会被动作录制器捕捉
    [CommandMethod("CmdTest_RunLisp22", CommandFlags.ActionMacro)]


    // 推断约束时不能使用命令
    [CommandMethod("CmdTest_RunLisp23", CommandFlags.NoInferConstraint)]
    // 命令允许在选择图元时临时显示动态尺寸
    [CommandMethod("CmdTest_RunLisp24", CommandFlags.TempShowDynDimension)]
#endif
    public static void CmdTest_RunLisp()
    {
        // 测试方法1: (command "CmdTest_RunLisp1")
        // 测试方式2: (LispTest_RunLisp)
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;

        var sb = new StringBuilder();
        foreach (var item in Enum.GetValues(typeof(EditorEx.RunLispFlag)))
        {
            sb.Append((byte)item);
            sb.Append(',');
        }
        sb.Remove(sb.Length - 1, 1);
        var option = new PromptIntegerOptions($"\n输入RunLispFlag枚举值:[{sb}]");
        var ppr = ed.GetInteger(option);

        if (ppr.Status != PromptStatus.OK)
            return;
        var flag = (EditorEx.RunLispFlag)ppr.Value;

        if (flag == EditorEx.RunLispFlag.AdsQueueexpr)
        {
            // 同步
            Env.Editor.RunLisp("(setq a 10)(princ)",
                EditorEx.RunLispFlag.AdsQueueexpr);
            Env.Editor.RunLisp("(princ a)",
                EditorEx.RunLispFlag.AdsQueueexpr);// 成功输出
        }
        else if (flag == EditorEx.RunLispFlag.AcedEvaluateLisp)
        {
            // 使用(command "CmdTest_RunLisp1")发送,然后 !b 查看变量,acad08是有值的,高版本是null
            var strlisp0 = "(setq b 20)";
            var res0 = Env.Editor.RunLisp(strlisp0,
                EditorEx.RunLispFlag.AcedEvaluateLisp); // 有lisp的返回值

            var strlisp1 = "(defun f1( / )(princ \"aa\"))";
            var res1 = Env.Editor.RunLisp(strlisp1,
                EditorEx.RunLispFlag.AcedEvaluateLisp); // 有lisp的返回值

            var strlisp2 = "(defun f2( / )(command \"line\"))";
            var res2 = Env.Editor.RunLisp(strlisp2,
                EditorEx.RunLispFlag.AcedEvaluateLisp); // 有lisp的返回值
        }
        else if (flag == EditorEx.RunLispFlag.SendStringToExecute)
        {
            // 测试异步
            // (command "CmdTest_RunLisp1")和(LispTest_RunLisp)4都是异步
            var str = "(setq c 40)(princ)";
            Env.Editor.RunLisp(str,
                EditorEx.RunLispFlag.SendStringToExecute); // 异步,后发送
            Env.Editor.RunLisp("(princ c)",
                EditorEx.RunLispFlag.AdsQueueexpr); // 同步,先发送了,输出是null
        }
    }
}