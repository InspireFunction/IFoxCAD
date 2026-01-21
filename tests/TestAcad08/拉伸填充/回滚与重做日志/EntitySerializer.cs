namespace JoinBoxAcad;

// 工具类：用于使用DwgFilerEx序列化和反序列化DBObject
public static class EntitySerializer
{
    /// <summary>
    /// 序列化DBObject到字节数组
    /// </summary>
    public static byte[] SerializeDBObject(DBObject entity)
    {
        try
        {
            var dwgFiler = new DwgFilerEx();
            dwgFiler.DwgOut(entity);

            // 将DwgFiler序列化为JSON字符串，再转为字节数组
            var json = dwgFiler.SerializeObject();
            Console.WriteLine($"SerializeDBObject Success: {entity.GetType().Name}, JSON Length: {json.Length}");
            var result = Encoding.UTF8.GetBytes(json);
            Console.WriteLine($"SerializeDBObject Result Bytes Length: {result.Length}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SerializeDBObject Error: {entity.GetType().Name}, Message: {ex.Message}");
            Console.WriteLine($"SerializeDBObject Error Details: {ex}");
            throw;
        }
    }

    /// <summary>
    /// 从字节数组反序列化DBObject
    /// </summary>
    public static DBObject? DeserializeDBObject(byte[] data, Database database, Type entityType)
    {
        var json = Encoding.UTF8.GetString(data);

        try
        {
            Console.WriteLine($"DeserializeDBObject: JSON Length: {json.Length}, Content Preview: {(json.Length > 100 ? json.Substring(0, 100) : json)}");

            // 首先创建一个DBObject实例
            if (entityType == null) return null;

            var newEntity = Activator.CreateInstance(entityType) as DBObject;
            if (newEntity == null) return null;


            // 解决方案：先修复JSON格式，然后再反序列化
            var fixedJson = FixJsonFormat(json);

            // 现在反序列化DwgFilerEx
            var dwgFilerEx = DwgFilerEx.DeserializeObject(fixedJson);
            if (dwgFilerEx == null) return null;


            // newEntity.DwgIn(dwgFilerEx.DwgFiler);

            // TODO 为什么不可以把这个加入当前呢??
            // 如果不可以也合理,毕竟不能存在一模一样的图元,id要根据cad分配器分配..
            // 那么我反序列化的对象要如何加入回去呢??


            // 使用事务将数据读入新实体
            using var trans = new DBTrans(database);
            using (newEntity.ForWrite())
            {
                newEntity.DwgIn(dwgFilerEx.DwgFiler);
            }
            trans.AddNewlyCreatedDBObject(newEntity, true);

            Console.WriteLine($"Successfully deserialized DBObject: {entityType.Name}");
            return newEntity;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DeserializeDBObject Error: {ex.Message}");
            Console.WriteLine($"DeserializeDBObject Error Details: {ex}");
            Console.WriteLine($"Problematic JSON: {(json.Length > 200 ? json.Substring(0, 200) + "..." : json)}");
            throw;
        }
    }

    /// <summary>
    /// 修复JSON格式，将非标准格式转换为标准格式
    /// </summary>
    private static string FixJsonFormat(string json)
    {
        // 将形如 (x,y,z) 的坐标格式转换为 [x,y,z] 数组格式
        var regex = new System.Text.RegularExpressions.Regex(@"\((-?\d+(?:\.\d+)?(?:,\s*-?\d+(?:\.\d+)?)*)\)");
        var result = regex.Replace(json, match => {
            var content = match.Groups[1].Value;
            var values = content.Split(',');
            var trimmedValues = values.Select(v => v.Trim()).ToArray();
            var jsonArray = "[" + string.Join(",", trimmedValues) + "]";
            Console.WriteLine($"Fixed coordinate format: {match.Value} -> {jsonArray}");
            return jsonArray;
        });

        return result;
    }

    /// <summary>
    /// 克隆DBObject，使用DwgFilerEx确保即使原对象所属的事务结束后仍能使用
    /// </summary>
    public static DBObject? CloneEntityWithDwgFiler(DBObject entity)
    {
        if (entity == null) return null;

        // 序列化实体
        var serializedData = SerializeDBObject(entity);

        // 反序列化到新实体
        return DeserializeDBObject(serializedData, entity.Database, entity.GetType());
    }
}