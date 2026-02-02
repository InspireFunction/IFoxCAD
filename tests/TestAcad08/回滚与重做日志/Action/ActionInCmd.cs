namespace JoinBoxAcad;

/// <summary>
/// 通用命令动作类
/// </summary>
public class CommandAction : ActionBase
{
    public ObjectId[]? ObjectIds { get; }

    public override ActionType Type { get; }
    public override string Description => GetDescription(Type);

    public CommandAction(ActionType cmdActionType, ObjectId[]? objectIds = null)
    {
        Type = cmdActionType;
        ObjectIds = objectIds;
    }


    // 这里全部都是正向执行,也就是Do和Redo,逆向执行靠转换类型
    public override void Execute()
    {
        var doc = Acap.DocumentManager.MdiActiveDocument;
        if (doc == null) return;
        var logger = EnhancedUndoRedoManager.GetLogger(doc);

        switch (Type)
        {
            case ActionType.RefSetAdd:
            // 在位编辑添加
            doc.Editor?.SetImpliedSelection(ObjectIds);
            logger?.AsyncCmdsPush("REFSET");
            Env.Editor.RunLisp("(command \"_.refset\" \"a\")");
            Env.Printl($"执行在位编辑添加操作，对象数: {ObjectIds?.Length}");
            break;
            case ActionType.RefSetRemove:
            // 在位编辑移除
            doc.Editor?.SetImpliedSelection(ObjectIds);
            logger?.AsyncCmdsPush("REFSET");
            Env.Editor.RunLisp("(command \"_.refset\" \"r\")");
            Env.Printl($"执行在位编辑移除操作，对象数: {ObjectIds?.Length}");
            break;
            case ActionType.RefEdit:
            // 在位编辑开始
            doc.Editor?.SetImpliedSelection(ObjectIds);
            logger?.AsyncCmdsPush("REFEDIT");
            //Env.Editor.RunLisp("(command \"_.refedit\" \"y\")"); // 未知命令Y,怎么重做时候可以一步到位啊??
            Env.Editor.RunLisp("(command \"_.refedit\")");
            break;
            case ActionType.RefEditUndo:
            logger?.AsyncCmdsPush("REFCLOSE");
            Env.Editor.RunLisp("(command \"_.refclose\" \"d\")");
            break;
            case ActionType.RefClose:
            //在位编辑保存
            logger?.AsyncCmdsPush("REFCLOSE");
            Env.Editor.RunLisp("(command \"_.refclose\" \"s\")");
            break;
            case ActionType.RefCloseUndo:
            doc.Editor?.SetImpliedSelection(ObjectIds);
            logger?.AsyncCmdsPush("REFEDIT");
            Env.Editor.RunLisp("(command \"_.refedit\")");
            break;

            case ActionType.BlockEdit:
            //块编辑开始
            doc.Editor?.SetImpliedSelection(ObjectIds);
            logger?.AsyncCmdsPush("BEDIT");
            Env.Editor.RunLisp("(command \"_.bedit\")");
            break;
            case ActionType.BlockEditUndo:
            logger?.AsyncCmdsPush("BCLOSE");
            Env.Editor.RunLisp("(command \"_.bclose\" \"_s\")");
            break;
            case ActionType.BlockEditClose:
            //块编辑保存
            doc.Editor?.SetImpliedSelection(ObjectIds);
            logger?.AsyncCmdsPush("BCLOSE");
            Env.Editor.RunLisp("(command \"_.bclose\" \"_s\")");
            break;
            case ActionType.BlockEditCloseUndo:
            logger?.AsyncCmdsPush("BEDIT");
            Env.Editor.RunLisp("(command \"_.bedit\")");
            break;
        }
    }


    public static HashSet<string> RevCmdMap = ["BEDIT", "BCLOSE", "REFEDIT", "REFCLOSE"];


    public override IAction GetInverseAction()
    {
        var inverseActionType = GetInverseActionType(Type);
        return new CommandAction(inverseActionType, ObjectIds);
    }

    public override IAction Clone()
    {
        return new CommandAction(Type, ObjectIds);
    }

    public override bool CanMergeWith(IAction otherAction) => false;

    public override IAction MergeWith(IAction otherAction) =>
        throw new InvalidOperationException($"{Description} 不能合并");


    private string GetDescription(ActionType cmdActionType)
    {
        return cmdActionType switch
        {
            ActionType.RefSetAdd => $"在位编辑(添加): {ObjectIds?.Length} 个对象",
            ActionType.RefSetRemove => $"在位编辑块(移除): {ObjectIds?.Length} 个对象",
            ActionType.RefEdit => "在位编辑开始",
            ActionType.RefEditUndo => "在位编辑开始的撤销",
            ActionType.RefClose => "在位编辑保存",
            ActionType.RefCloseUndo => "在位编辑保存的撤销",
            ActionType.BlockEdit => "块编辑开始",
            ActionType.BlockEditUndo => "块编辑开始的撤销",
            ActionType.BlockEditClose => "块编辑保存",
            ActionType.BlockEditCloseUndo => "块编辑保存的撤销",
            _ => "未知命令动作"
        };
    }

    private static ActionType GetInverseActionType(ActionType cmdActionType)
    {
        return cmdActionType switch
        {
            ActionType.RefSetAdd => ActionType.RefSetRemove,
            ActionType.RefSetRemove => ActionType.RefSetAdd,
            ActionType.RefEdit => ActionType.RefEditUndo,
            ActionType.RefEditUndo => ActionType.RefEdit,
            ActionType.RefClose => ActionType.RefCloseUndo,
            ActionType.RefCloseUndo => ActionType.RefClose,
            ActionType.BlockEdit => ActionType.BlockEditUndo,
            ActionType.BlockEditUndo => ActionType.BlockEdit,
            ActionType.BlockEditClose => ActionType.BlockEditCloseUndo,
            ActionType.BlockEditCloseUndo => ActionType.BlockEditClose,
            _ => cmdActionType
        };
    }
}