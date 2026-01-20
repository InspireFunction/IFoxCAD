using Autodesk.AutoCAD.GraphicsSystem;
using System.Drawing;

#if true2
namespace JoinBoxAcad;

public partial class HatchChange
{
    [CommandMethod(nameof(aaa))]
    public void aaa()
    {
        // 你这个代码不是acad2008的
        // 获取命名对象词典-主词典 Dictionaries
        using var tr = new DBTrans();
        var nod = tr.NamedObjectsDict;

        var a = GetDictCount(nod);
        Env.Printl("字典数量: " + a);
    }

    HashSet<ObjectId> _numSet = [];

    // 递归统计数量
    int GetDictCount(DBDictionary dict)
    {
        var tr = DBTrans.GetTop(dict.Database);

        int num = 0;
        foreach (var en in dict)
        {
            var id = en.Value;
            if (!id.IsOk()) continue;

            // 防止菱形引用,数据字典a内的数据字典b记录着a.
            if (!_numSet.Add(en.Value))
                continue;

            // 为什么这里直接崩溃呢??
            using var obj = tr.GetObject(id, OpenMode.ForRead);

            if (obj is null) continue;

            if (obj is DBDictionary dict2) // 字典套字典,继续递归,会菱形引用.
            {
                num = dict2.Count + GetDictCount(dict2);
            }
            else if (obj is Xrecord xr) // x记录
            {
                var data = xr.Data;
                if (data is null) continue;

                var tvArray = data.AsArray();
                num += tvArray.Length;
                foreach (var tv in tvArray)
                {
                    //item2.Value
                    //item2.TypeCode
                    if (tv.Value is DBDictionary d)
                    {
                        num += GetDictCount(d);
                    }
                    else if (tv.Value is DBObject dbobj)
                    {
                        Env.Printl($"这是DBObject 1 " + tv.Value.GetType().Name);
                    }
                    else if (tv.Value is ObjectId mid)
                    {
                        Env.Printl("这是ObjectId " + mid);
                    }
                    else
                    {
                        Env.Printl("这是什么类型? " + tv.Value.GetType().Name);
                    }
                }
            }
            else if (obj is DBObject dbobj) // 关联标注的反应器 
            {
                //dbobj.XData
                //dbobj.ExtensionDictionary
                Env.Printl("这是DBObject");
            }

        }
        return num;
    }

}

#endif

public partial class HatchChange
{
    [CommandMethod(nameof(aaa))]
    public void aaa()
    {
        using var tr = new DBTrans();
        var nod = tr.NamedObjectsDict;

        _visitedObjects.Clear();
        PrintDictTree(nod, 0, "命名对象词典");
    }

    HashSet<ObjectId> _visitedObjects = new();

    void PrintDictTree(DBDictionary dict, int level, string name)
    {
        var indent = new string(' ', level * 4);
        var prefix = level == 0 ? "" : "├── ";

        Env.Print($"{indent}{prefix}{name} [{dict.Count} 项]");

        var tr = DBTrans.GetTop(dict.Database);
        int count = 0;

        foreach (var entry in dict)
        {
            count++;
            var id = entry.Value;
            if (!id.IsOk()) continue;

            var childIndent = new string(' ', (level + 1) * 4);
            var childPrefix = count == dict.Count ? "└── " : "├── ";

            if (!_visitedObjects.Add(id))
            {
                continue;
            }

            try
            {
                using var obj = tr.GetObject(id, OpenMode.ForRead);
                if (obj is null) continue;

                if (obj is DBDictionary subDict)
                {
                    PrintDictTree(subDict, level + 1, $"{entry.Key} [字典]");
                }
                else
                {
                    Env.Print($"{childIndent}{childPrefix}{entry.Key} [{obj.GetType().Name}]");
                }
            }
            catch
            {
                Env.Print($"{childIndent}{childPrefix}{entry.Key} [错误]");
            }
        }
    }
}