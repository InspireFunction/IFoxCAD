using System.Reflection;
using Autodesk.AutoCAD.DatabaseServices;

namespace JoinBoxAcad;

/// <summary>
/// 成员（字段和属性）操作辅助类
/// </summary>
public static class MemberHelper
{
    /// <summary>
    /// 尝试设置成员值（字段或属性）
    /// </summary>
    public static bool TrySetMember(DBObject entity, string memberName, object value, bool isField)
    {
        var type = entity.GetType();
        var member = FindMemberInHierarchy(type, memberName, isField);
        if (member != null)
        {
            try
            {
                if (isField)
                {
                    var field = (FieldInfo)member;
                    field.SetValue(entity, value);
                    Env.Printl($"[DEBUG] 直接设置字段成功: {memberName} = {value}");
                }
                else
                {
                    var property = (PropertyInfo)member;
                    // 跳过已知会引发异常的属性 
                    if (_jump.Contains(property.Name))
                        return false;

                    if (property.CanWrite)
                    {
                        // TODO 点为什么不能设置?
                        if (value is Point3d)
                            return false;
                        property.SetValue(entity, value, null);
                        Env.Printl($"[DEBUG] 设置属性成功: {memberName} = {value}");
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                Env.Printl($"[ERROR] 设置{(isField ? "字段" : "属性")}失败: {memberName}, 错误: {e.Message}");
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// 尝试回滚成员值（字段或属性）
    /// </summary>
    public static bool TryRollbackMember(DBObject entity, string memberName, object oldValue, bool isField)
    {
        var type = entity.GetType();
        var member = FindMemberInHierarchy(type, memberName, isField);
        if (member != null)
        {
            try
            {
                if (isField)
                {
                    var field = (FieldInfo)member;
                    field.SetValue(entity, oldValue);
                    Env.Printl($"[DEBUG] 直接回滚字段: {memberName} = {oldValue}");
                }
                else
                {
                    var property = (PropertyInfo)member;
                    // 跳过已知会引发异常的属性
                    if (_jump.Contains(property.Name))
                        return false;

                    if (property.CanWrite)
                    {
                        property.SetValue(entity, oldValue, null);
                        Env.Printl($"[DEBUG] 回滚属性: {memberName} = {oldValue}");
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Env.Printl($"[ERROR] 回滚{(isField ? "字段" : "属性")}失败: {memberName}, 错误: {ex.Message}");
                return false;
            }
        }
        return false;
    }

    /// <summary>
    /// 递归查找成员（字段或属性），包括父类
    /// </summary>
    public static MemberInfo FindMemberInHierarchy(Type type, string memberName, bool isField)
    {
        var currentType = type;
        while (currentType != null && currentType != typeof(object))
        {
            if (isField)
            {
                // 查找当前类的字段，包括公共和私有字段
                var field = currentType.GetField(memberName, _ff);
                if (field != null)
                    return field;
            }
            else
            {
                // 查找当前类的属性，包括公共属性
                var property = currentType.GetProperty(memberName, _ff);
                if (property != null)
                    return property;
            }
            // 移动到父类
            currentType = currentType.BaseType;
        }
        return null;
    }

    // 这些接口反射会爆异常
    static HashSet<string> _jump = ["IncludingErased", "PreviewIcon", "PlotStyleName"];

    static BindingFlags _ff = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    /// <summary>
    /// 递归获取实体的所有成员（字段和属性），并保存到快照中
    /// </summary>
    public static void GetMemberInHierarchy(DBObject entity, EntitySnapshot snapshot, Type type)
    {
        // 终止条件：基类为 null
        if (type == null || type == typeof(object))
            return;

        // 获取所有字段
        var fields = type.GetFields(_ff);

        foreach (var field in fields)
        {
            try
            {
                if (_jump.Contains(field.Name))
                    continue;

                // 这里要传入 entity 实例，不是 type
                var value = field.GetValue(entity);
                snapshot.Fields[field.Name] = value;
            }
            catch
            {
                _jump.Add(field.Name);
            }
        }

        // 获取所有属性（作为字段的补充）
        var properties = type.GetProperties(_ff);

        foreach (var property in properties)
        {
            try
            {
                // 跳过已经记录过的字段/属性
                if (_jump.Contains(property.Name) || snapshot.Fields.ContainsKey(property.Name))
                    continue;

                // 跳过一些已知会引发异常的属性
                if (_jump.Contains(property.Name))
                    continue;

                // 只处理可读属性，且跳过索引器属性
                if (property.CanRead)
                {
                    // 检查是否是索引器属性（具有参数）
                    if (property.GetIndexParameters().Length > 0)
                    {
                        // 跳过索引器属性，因为它们需要参数才能访问
                        continue;
                    }

                    try
                    {
                        // 尝试直接获取属性值
                        var value = property.GetValue(entity, null);
                        snapshot.Fields[property.Name] = value;
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("Must be transaction resident"))
                    {
                        // 忽略需要事务的属性，这些属性在事务上下文中才能访问
                        continue;
                    }
                }
            }
            catch
            {
                _jump.Add(property.Name);
            }
        }

        // 递归处理基类
        GetMemberInHierarchy(entity, snapshot, type.BaseType);
    }
}