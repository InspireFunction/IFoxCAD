namespace JoinBoxAcad;

using System.Security.Cryptography;

// DAG版本图
public class ActionDAG
{
    /// <summary>
    /// 根节点
    /// </summary>
    public VersionNode Root { get; }
    /// <summary>
    /// 当前版本的节点
    /// </summary>
    public VersionNode Current { get; set; }
    /// <summary>
    /// 分支
    /// </summary>
    public List<VersionNode> Branches { get; } = [];
    /// <summary>
    /// 历史版本
    /// </summary>
    public List<VersionNode> History => GetAllNodes();

    public ActionDAG()
    {
        Root = new VersionNode(new RootAction());
        Current = Root;
    }

    // 添加动作
    public VersionNode AddAction(IAction action, string commandContext = "无命令")
    {
        // 检查是否可以添加到当前节点的动作链
        if (CanAddToCurrentNode(action, commandContext))
        {
            // 添加到当前节点的动作链
            Current.Actions.Add(action);
            Current.CommandContext = commandContext;
            Current.CalculateHash();
            // 打印日志：加入到现有节点
            Env.Printl($"[DEBUG] 动作加入到现有节点: 节点ID={Current.Id.Substring(0, 8)}..., 命令上下文={Current.CommandContext}, 动作描述={action.Description}, 动作链长度={Current.Actions.Count}");
            return Current;
        }

        // 不能添加到当前节点，创建新节点
        var node = new VersionNode(action, Current);
        node.CommandContext = commandContext;
        Current.Children.Add(node);
        Current = node;
        // 打印日志：创建新节点
        Env.Printl($"[DEBUG] 动作创建新节点: 节点ID={node.Id.Substring(0, 8)}..., 命令上下文={node.CommandContext}, 动作描述={action.Description}");
        return node;
    }

    // 检查是否可以添加到当前节点
    private bool CanAddToCurrentNode(IAction action, string commandContext)
    {
        // 如果没有当前节点或当前节点是根节点，不能添加
        if (Current == null || Current == Root)
            return false;

        // 情况1：当前节点是<有命令>，直接加入
        if (commandContext != "无命令")
            return true;

        // 情况2：当前节点是<无命令>，检查动作链条末尾是否是相同图元id
        if (commandContext == "无命令" && Current.LastAction != null)
        {
            // 检查当前动作是否是实体动作
            if (action is EntityAction newEntityAction && Current.LastAction is EntityAction lastEntityAction)
            {
                // 如果动作链条末尾是相同图元id，就加入
                return newEntityAction.DBObjectId == lastEntityAction.DBObjectId;
            }
        }

        // 其他情况不能加入
        return false;
    }

    // 添加分支
    public VersionNode AddBranch(IAction action, VersionNode fromNode)
    {
        if (fromNode == null || !History.Contains(fromNode))
            throw new ArgumentException("无效的源节点");

        var node = new VersionNode(action, fromNode);
        fromNode.Children.Add(node);

        if (!Branches.Contains(fromNode))
            Branches.Add(fromNode);

        return node;
    }

    // 切换到指定节点
    public void SwitchTo(VersionNode targetNode)
    {
        if (targetNode == null || !History.Contains(targetNode))
            throw new ArgumentException("无效的目标节点");

        // 计算从当前节点到目标节点的路径
        var path = CalculatePath(Current, targetNode);

        // 执行路径中的动作
        foreach (var step in path)
        {
            if (step != null)
            {
                step.Execute();
                Env.Printl($"动作链执行了: ({step.Description})");
            }
        }

        Current = targetNode;
    }

    // 计算两个节点之间的路径
    private List<IAction> CalculatePath(VersionNode fromNode, VersionNode toNode)
    {
        var path = new List<IAction>();

        if (fromNode == toNode) return path;

        if (fromNode.IsAncestorOf(toNode))
        {
            // 向前：执行子节点动作
            var current = fromNode;
            while (current != toNode)
            {
                var child = current.Children.FirstOrDefault(c => c.IsAncestorOf(toNode));
                if (child == null) break;

                // 添加子节点的所有动作到路径中
                path.AddRange(child.Actions);
                current = child;
            }
        }
        else if (toNode.IsAncestorOf(fromNode))
        {
            // 向后：执行逆动作
            var current = fromNode;
            while (current != toNode)
            {
                // 为当前节点的每个动作添加逆动作，创建一个反转的副本
                var reversedActions = current.Actions.ToList();
                reversedActions.Reverse();
                foreach (var action in reversedActions)
                {
                    var inverseAction = action.GetInverseAction();
                    if (inverseAction is not null)
                    {
                        path.Add(inverseAction);
                    }
                }
                current = current.Parent;
            }
        }
        else
        {
            // 需要先回退到公共祖先
            var commonAncestor = FindCommonAncestor(fromNode, toNode);

            // 回退到公共祖先
            var backPath = CalculatePath(fromNode, commonAncestor);
            path.AddRange(backPath);

            // 前进到目标节点
            var forwardPath = CalculatePath(commonAncestor, toNode);
            path.AddRange(forwardPath);
        }

        return path;
    }

    // 查找公共祖先
    private VersionNode FindCommonAncestor(VersionNode node1, VersionNode node2)
    {
        var ancestors1 = new HashSet<VersionNode>();
        var current = node1;
        while (current != null)
        {
            ancestors1.Add(current);
            current = current.Parent;
        }

        current = node2;
        while (current != null)
        {
            if (ancestors1.Contains(current))
                return current;
            current = current.Parent;
        }

        return Root;
    }

    // 获取所有节点
    private List<VersionNode> GetAllNodes()
    {
        var nodes = new List<VersionNode>();
        CollectNodes(Root, nodes);
        return nodes;
    }

    private void CollectNodes(VersionNode node, List<VersionNode> nodes)
    {
        nodes.Add(node);
        foreach (var child in node.Children)
        {
            CollectNodes(child, nodes);
        }
    }

    // 序列化
    public string Serialize()
    {
        var data = new
        {
            CurrentNodeId = Current.Id,
            Nodes = SerializeNodes()
        };

        try
        {
            var serializeSettings = new MyJsonSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented,
                Converters = [new ObjectIdConverter()]
            };

            var result = MyJson.SerializeObject(data, serializeSettings);
            Env.Printl($"DAG Serialize Success, Result Length: {result.Length}");
            return result;
        }
        catch (Exception ex)
        {
            Env.Printl($"DAG Serialize Error, Message: {ex.Message}");
            Env.Printl($"DAG Serialize Error Details: {ex}");
            throw;
        }
    }

    private Dictionary<string, object> SerializeNodes()
    {
        var nodes = new Dictionary<string, object>();

        foreach (var node in History)
        {
            try
            {
                // 序列化所有动作
                var actionsSerialized = new List<string>();
                foreach (var action in node.Actions)
                {
                    if (action is BaseAction baseAction)
                    {
                        actionsSerialized.Add(baseAction.Serialize());
                    }
                }
                
                var actionType = node.Actions.Count > 0 ? node.Actions[0].GetType().Name : "None";
                Env.Printl($"Serializing node: {node.Id}, Action Type: {actionType}, Actions Count: {node.Actions.Count}");
                Env.Printl($"Node {node.Id} Actions Serialized Length: {actionsSerialized.Sum(s => s.Length)}");

                nodes[node.Id] = new
                {
                    Actions = actionsSerialized,
                    CommandContext = node.CommandContext,
                    ParentId = node.Parent?.Id,
                    Depth = node.Depth,
                    Hash = node.Hash
                };
            }
            catch (Exception ex)
            {
                Env.Printl($"Error serializing node {node.Id}: {ex.Message}");
                Env.Printl($"Error serializing node details: {ex}");
                throw;
            }
        }

        Env.Printl($"Total nodes serialized: {nodes.Count}");
        return nodes;
    }
}

// 根动作（虚拟）
public class RootAction : BaseAction
{
    public override ActionType Type => ActionType.OtherOperation;
    public override string Description => "系统初始状态 (根节点)";
    public override void Execute() { }
    public override IAction GetInverseAction() => this;
    public override IAction Clone() => this;
    public override bool CanMergeWith(IAction otherAction) => false;
    public override IAction MergeWith(IAction otherAction) => this;
}

// 版本节点
public class VersionNode
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public List<IAction> Actions { get; set; } = new(); // 动作链
    public VersionNode Parent { get; set; }
    public List<VersionNode> Children { get; } = new();
    public int Depth { get; set; }
    public string Hash { get; private set; }
    public string CommandContext { get; set; } = "无命令"; // 节点的命令上下文

    // 快捷属性：获取第一个动作的描述
    public IAction FirstAction => Actions.FirstOrDefault();
    // 快捷属性：获取最后一个动作
    public IAction LastAction => Actions.LastOrDefault();

    public VersionNode(IAction action, VersionNode parent = null)
    {
        Actions.Add(action);
        Parent = parent;
        Depth = parent?.Depth + 1 ?? 0;
        CalculateHash();
    }

    public VersionNode(List<IAction> actions, VersionNode parent = null)
    {
        Actions = actions;
        Parent = parent;
        Depth = parent?.Depth + 1 ?? 0;
        CalculateHash();
    }

    public void RecalculateHash()
    {
        CalculateHash();
    }

    public void CalculateHash()
    {
        // 基于所有动作生成哈希
        var actionData = Actions.Select(a => $"{a.GuId}-{a.Type}-{a.Timestamp.Ticks}").ToArray();
        var data = string.Join("-", actionData) + $"-{Depth}";
        using (var sha256 = SHA256.Create())
        {
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            Hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    public bool IsAncestorOf(VersionNode node)
    {
        var current = node;
        while (current != null)
        {
            if (current == this) return true;
            current = current.Parent;
        }
        return false;
    }

    public bool IsDescendantOf(VersionNode node) => node.IsAncestorOf(this);
}