namespace JoinBoxAcad;

/// <summary>
/// 实体序列化工具类 - 使用DwgFilerEx进行正确的DWG序列化
/// </summary>
public static class EntitySerializer
{
    /// <summary>
    /// 使用DwgFilerEx序列化实体对象为字符串快照
    /// </summary>
    /// <param name="entity">要序列化的实体</param>
    /// <returns>序列化后的字符串快照</returns>
    public static string SerializeEntityToSnapshot(DBObject entity)
    {
        if (entity == null)
            return string.Empty;

        try
        {
            // 使用DwgFilerEx进行序列化
            var dwgFilerEx = new DwgFilerEx(entity);
            return dwgFilerEx.SerializeObject();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DwgFilerEx序列化失败: {ex.Message}, 使用回退方案");
            return SerializeEntityBasic(entity);
        }
    }

    /// <summary>
    /// 基本的实体序列化回退方案
    /// </summary>
    /// <param name="entity">要序列化的实体</param>
    /// <returns>序列化后的字符串快照</returns>
    private static string SerializeEntityBasic(DBObject entity)
    {
        if (entity == null)
            return string.Empty;

        try
        {
            // 简单返回类型名作为回退快照
            return entity.GetType().FullName;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"基本序列化失败: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// 从字符串快照反序列化实体
    /// </summary>
    /// <param name="snapshot">字符串快照</param>
    /// <param name="entityType">实体类型</param>
    /// <returns>反序列化后的实体</returns>
    public static DBObject? DeserializeEntityFromSnapshot(string snapshot, Type entityType)
    {
        if (string.IsNullOrEmpty(snapshot) || entityType == null)
            return null;

        try
        {
            // 使用DwgFilerEx反序列化
            var dwgFilerEx = DwgFilerEx.DeserializeObject(snapshot);
            if (dwgFilerEx != null)
            {
                var entity = (DBObject)Activator.CreateInstance(entityType);
                entity.DwgIn(dwgFilerEx.DwgFiler);
                return entity;
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"实体反序列化失败: {ex.Message}");
            return null;
        }
    }
}