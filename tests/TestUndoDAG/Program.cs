using System;
using System.Collections.Generic;
using System.Threading;
using TestUndoDAG;

namespace TestUndoDAG
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== UndoDAG 步骤序列测试 ===\n");

            var logger = SimpleActionLogger.Instance;

            Console.WriteLine("步骤1: 创建根节点");
            var dag = new ActionDAG();
            Console.WriteLine($"  根节点: {dag.Root.Id} (初始状态)");
            Console.WriteLine($"  当前节点: {dag.Current.Id} ('{dag.Current.Action.Description}')");
            Console.WriteLine($"  历史节点数: {dag.History.Count}");
            Console.WriteLine();

            Console.WriteLine("步骤2: 创建直线");
            var lineEntity = new LineEntity("LINE001", 0, 0, 10, 10);
            var createLineAction = new CreateEntityAction(lineEntity);
            Console.WriteLine($"  即将执行: {createLineAction}");
            logger.LogAction(createLineAction);
            Console.WriteLine($"  历史节点数: {logger.GetHistory().Count}");
            Console.WriteLine();

            Console.WriteLine("步骤3: 创建圆形");
            var circleEntity = new CircleEntity("CIRCLE001", 5, 5, 3);
            var createCircleAction = new CreateEntityAction(circleEntity);
            Console.WriteLine($"  即将执行: {createCircleAction}");
            logger.LogAction(createCircleAction);
            Console.WriteLine($"  历史节点数: {logger.GetHistory().Count}");
            Console.WriteLine();

            Console.WriteLine("步骤4: 创建另一个直线");
            var line2Entity = new LineEntity("LINE002", 20, 20, 30, 30);
            var createLine2Action = new CreateEntityAction(line2Entity);
            Console.WriteLine($"  即将执行: {createLine2Action}");
            logger.LogAction(createLine2Action);
            Console.WriteLine($"  历史节点数: {logger.GetHistory().Count}");
            Console.WriteLine();

            Console.WriteLine("步骤5: 第一次撤销 (撤销创建直线2)");
            Console.WriteLine($"  即将撤销: {createLine2Action}");
            logger.Undo();
            Console.WriteLine($"  历史节点数: {logger.GetHistory().Count}");
            Console.WriteLine();

            Console.WriteLine("步骤6: 第二次撤销 (撤销创建圆形)");
            Console.WriteLine($"  即将撤销: {createCircleAction}");
            logger.Undo();
            Console.WriteLine($"  历史节点数: {logger.GetHistory().Count}");
            Console.WriteLine();

            Console.WriteLine("步骤7: 第一次重做 (重做创建圆形)");
            Console.WriteLine($"  即将重做: {createCircleAction}");
            logger.Redo();
            Console.WriteLine($"  历史节点数: {logger.GetHistory().Count}");
            Console.WriteLine();

            Console.WriteLine("步骤8: 第二次重做 (重做创建直线2)");
            Console.WriteLine($"  即将重做: {createLine2Action}");
            logger.Redo();
            Console.WriteLine($"  历史节点数: {logger.GetHistory().Count}");
            Console.WriteLine();

            Console.WriteLine("=== 所有测试完成 ===");
        }
    }

    // 模拟动作类用于测试
    public class MockAction : BaseAction
    {
        public static int UndoExecutedCount = 0;

        public string ActionName { get; }

        // 静态映射表存储动作与其逆操作的对应关系
        private static Dictionary<string, IAction> _actionInverses = new Dictionary<string, IAction>();
        
        // 存储当前实例的逆操作
        private IAction _cachedInverseAction = null;

        public MockAction(string name, ActionType type)
        {
            ActionName = name;
            Type = type;
        }

        public override ActionType Type { get; }

        public override string Description => ActionName;

        public override void Execute()
        {
            Console.WriteLine($"    执行动作: {ToString()}");
        }

        public override string ToString()
        {
            return $"[{Id}] {ActionName} (类型: {Type})";
        }

        public override IAction GetInverseAction()
        {
            Interlocked.Increment(ref UndoExecutedCount);

            // 检查是否已有此动作的逆操作缓存
            if (_cachedInverseAction != null)
            {
                return _cachedInverseAction;
            }
            
            // 检查是否已有此动作的逆操作映射
            if (_actionInverses.ContainsKey(this.Id))
            {
                return _actionInverses[this.Id];
            }

            string inverseName;
            ActionType inverseType = Type;

            if (ActionName.StartsWith("撤销_"))
            {
                // 如果是逆操作，则返回原操作
                inverseName = ActionName.Substring(3); // 移除"撤销_"前缀
            }
            else
            {
                inverseName = $"撤销_{ActionName}";

                // 根据原动作类型设置逆操作类型
                switch (Type)
                {
                    case ActionType.CreateEntity:
                        inverseType = ActionType.DeleteEntity;  // 创建的逆操作是删除
                        break;
                    case ActionType.DeleteEntity:
                        inverseType = ActionType.CreateEntity;  // 删除的逆操作是创建
                        break;
                    case ActionType.ModifyEntity:
                        inverseType = ActionType.ModifyEntity;  // 修改的逆操作仍是修改
                        break;
                    default:
                        inverseType = Type;
                        break;
                }
            }

            // 创建逆操作实例
            var inverse = new MockAction(inverseName, inverseType);

            // 建立双向映射关系
            _actionInverses[this.Id] = inverse;
            _actionInverses[inverse.Id] = this;
            
            // 缓存逆操作
            _cachedInverseAction = inverse;

            return inverse;
        }

        public override IAction Clone()
        {
            return new MockAction(ActionName, Type);
        }

        public override bool CanMergeWith(IAction otherAction)
        {
            return false;
        }

        public override IAction MergeWith(IAction otherAction)
        {
            throw new InvalidOperationException("Mock action cannot merge");
        }
    }
}

// CAD图元基类
public abstract class CadEntity
{
    public string Id { get; set; }
    public string EntityType { get; set; }
    
    protected CadEntity(string id, string entityType)
    {
        Id = id;
        EntityType = entityType;
    }
    
    public abstract CadEntity Clone();
}

// 线条图元
public class LineEntity : CadEntity
{
    public double StartX { get; set; }
    public double StartY { get; set; }
    public double EndX { get; set; }
    public double EndY { get; set; }
    
    public LineEntity(string id, double startX, double startY, double endX, double endY) 
        : base(id, "Line")
    {
        StartX = startX;
        StartY = startY;
        EndX = endX;
        EndY = endY;
    }
    
    public override CadEntity Clone()
    {
        return new LineEntity(Id, StartX, StartY, EndX, EndY);
    }
    
    public override string ToString()
    {
        return $"{EntityType}({Id}): ({StartX},{StartY})-({EndX},{EndY})";
    }
}

// 圆形图元
public class CircleEntity : CadEntity
{
    public double CenterX { get; set; }
    public double CenterY { get; set; }
    public double Radius { get; set; }
    
    public CircleEntity(string id, double centerX, double centerY, double radius) 
        : base(id, "Circle")
    {
        CenterX = centerX;
        CenterY = centerY;
        Radius = radius;
    }
    
    public override CadEntity Clone()
    {
        return new CircleEntity(Id, CenterX, CenterY, Radius);
    }
    
    public override string ToString()
    {
        return $"{EntityType}({Id}): center({CenterX},{CenterY}), radius={Radius}";
    }
}

// CAD图元数据库
public class CadDatabase
{
    private Dictionary<string, CadEntity> _entities = new Dictionary<string, CadEntity>();
    
    public void AddEntity(CadEntity entity)
    {
        _entities[entity.Id] = entity;
    }
    
    public void RemoveEntity(string id)
    {
        _entities.Remove(id);
    }
    
    public CadEntity GetEntity(string id)
    {
        _entities.TryGetValue(id, out CadEntity entity);
        return entity;
    }
    
    public Dictionary<string, CadEntity> GetAllEntities()
    {
        return new Dictionary<string, CadEntity>(_entities);
    }
    
    public void Clear()
    {
        _entities.Clear();
    }
}

// 修改操作记录类 - 记录新旧值
public class ModificationRecord<T>
{
    public T OldValue { get; set; }
    public T NewValue { get; set; }
    
    public ModificationRecord(T oldValue, T newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}

// CAD操作接口
public interface ICadAction : IAction
{
    void Apply(CadDatabase db);
}

// 创建图元操作
public class CreateEntityAction : BaseAction, ICadAction
{
    public CadEntity Entity { get; }
    
    public CreateEntityAction(CadEntity entity)
    {
        Entity = entity;
    }
    
    public override ActionType Type => ActionType.CreateEntity;
    
    public override string Description => $"创建{Entity.EntityType}({Entity.Id})";

    public void Apply(CadDatabase db)
    {
        db.AddEntity(Entity);
        Console.WriteLine($"    应用操作: {Description}");
    }

    public override void Execute()
    {
        Apply(SimpleActionLogger.Database);
    }

    public override IAction GetInverseAction()
    {
        return new DeleteEntityAction(Entity.Id, Entity.Clone());
    }

    public override IAction Clone()
    {
        return new CreateEntityAction(Entity.Clone());
    }

    public override bool CanMergeWith(IAction otherAction)
    {
        return false;
    }

    public override IAction MergeWith(IAction otherAction)
    {
        throw new InvalidOperationException("Cannot merge CreateEntityAction");
    }
    
    public override string ToString()
    {
        return $"[{Id}] {Description} (类型: {Type})";
    }
}

// 删除图元操作
public class DeleteEntityAction : BaseAction, ICadAction
{
    public string EntityId { get; }
    public CadEntity SavedEntity { get; }  // 保存被删除的实体，用于恢复
    
    public DeleteEntityAction(string entityId, CadEntity savedEntity)
    {
        EntityId = entityId;
        SavedEntity = savedEntity;
    }
    
    public override ActionType Type => ActionType.DeleteEntity;
    
    public override string Description => $"删除{SavedEntity?.EntityType}({EntityId})";

    public void Apply(CadDatabase db)
    {
        db.RemoveEntity(EntityId);
        Console.WriteLine($"    应用操作: {Description}");
    }

    public override void Execute()
    {
        Apply(SimpleActionLogger.Database);
    }

    public override IAction GetInverseAction()
    {
        return new CreateEntityAction(SavedEntity);
    }

    public override IAction Clone()
    {
        return new DeleteEntityAction(EntityId, SavedEntity.Clone());
    }

    public override bool CanMergeWith(IAction otherAction)
    {
        return false;
    }

    public override IAction MergeWith(IAction otherAction)
    {
        throw new InvalidOperationException("Cannot merge DeleteEntityAction");
    }
    
    public override string ToString()
    {
        return $"[{Id}] {Description} (类型: {Type})";
    }
}

// 修改图元操作
public class ModifyEntityAction : BaseAction, ICadAction
{
    public string EntityId { get; }
    public string PropertyName { get; }
    public object OldValue { get; }
    public object NewValue { get; }
    
    public ModifyEntityAction(string entityId, string propertyName, object oldValue, object newValue)
    {
        EntityId = entityId;
        PropertyName = propertyName;
        OldValue = oldValue;
        NewValue = newValue;
    }
    
    public override ActionType Type => ActionType.ModifyEntity;
    
    public override string Description => $"修改{EntityId}.{PropertyName}: {OldValue} -> {NewValue}";

    public void Apply(CadDatabase db)
    {
        // 实际应用修改到数据库中的实体
        Console.WriteLine($"    应用操作: {Description}");
    }

    public override void Execute()
    {
        Apply(SimpleActionLogger.Database);
    }

    public override IAction GetInverseAction()
    {
        // 修改的逆操作是将值改回旧值
        return new ModifyEntityAction(EntityId, PropertyName, NewValue, OldValue);
    }

    public override IAction Clone()
    {
        return new ModifyEntityAction(EntityId, PropertyName, OldValue, NewValue);
    }

    public override bool CanMergeWith(IAction otherAction)
    {
        // 只有同实体同属性的修改操作才能合并
        if (otherAction is ModifyEntityAction otherModify &&
            otherModify.EntityId == EntityId &&
            otherModify.PropertyName == PropertyName)
        {
            return true;
        }
        return false;
    }

    public override IAction MergeWith(IAction otherAction)
    {
        if (CanMergeWith(otherAction))
        {
            var otherModify = (ModifyEntityAction)otherAction;
            // 合并操作：保留最初的旧值和最终的新值
            return new ModifyEntityAction(EntityId, PropertyName, OldValue, otherModify.NewValue);
        }
        throw new InvalidOperationException("Cannot merge with incompatible action");
    }
    
    public override string ToString()
    {
        return $"[{Id}] {Description} (类型: {Type})";
    }
}