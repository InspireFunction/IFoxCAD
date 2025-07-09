using ArgumentNullException = System.ArgumentNullException;

namespace IFoxCAD.Cad;

/// <summary>
/// 符号表类扩展函数
/// </summary>
public static class SymbolTableEx
{
    #region 图层表

    /// <summary>
    /// 添加图层
    /// </summary>
    /// <param name="table">图层符号表</param>
    /// <param name="name">图层名</param>
    /// <param name="color">图层颜色</param>
    /// <returns>图层id</returns>
    public static ObjectId Add(this SymbolTable<LayerTable, LayerTableRecord> table, string name, Color color)
    {
        return table.Add(name, lt => lt.Color = color);
    }

    /// <summary>
    /// 添加图层
    /// </summary>
    /// <param name="table">图层符号表</param>
    /// <param name="name">图层名</param>
    /// <param name="colorIndex">图层颜色索引值</param>
    /// <returns>图层id</returns>
    public static ObjectId Add(this SymbolTable<LayerTable, LayerTableRecord> table, string name,
        int colorIndex)
    {
        colorIndex %= 257; // 防止输入的颜色超出256
        colorIndex = Math.Abs(colorIndex); // 防止负数
        return table.Add(name, lt => lt.Color = Color.FromColorIndex(ColorMethod.ByColor, (short)colorIndex));
    }

    /// <summary>
    /// 更改图层名
    /// </summary>
    /// <param name="table">图层符号表</param>
    /// <param name="oldName">旧图层名</param>
    /// <param name="newName">新图层名</param>
    public static ObjectId Rename(this SymbolTable<LayerTable, LayerTableRecord> table, string oldName,
        string newName)
    {
        if (!table.Has(oldName))
            return ObjectId.Null;

        table.Change(oldName, layer => { layer.Name = newName; });
        return table[newName];
    }

    /// <summary>
    /// 删除图层
    /// </summary>
    /// <param name="table">层表</param>
    /// <param name="name">图层名</param>
    /// <returns>成功返回 <see langword="true"/>，失败返回 <see langword="false"/></returns>
    public static bool Delete(this SymbolTable<LayerTable, LayerTableRecord> table, string name)
    {
        if (SymbolUtilityServices.IsLayerZeroName(name) || SymbolUtilityServices.IsLayerDefpointsName(name) ||
            !table.Has(name) || table[name] == table.Database.Clayer)
            return false;

        table.CurrentSymbolTable.GenerateUsageData();
        var ltr = table.GetRecord(name);
        if (ltr is null)
            return false;

        if (ltr.IsUsed)
            return false;
        using (ltr.ForWrite())
            ltr.Erase();
        return true;
    }

    #endregion

    #region 块表

    /// <summary>
    /// 添加块定义
    /// </summary>
    /// <param name="table">块表</param>
    /// <param name="name">块名</param>
    /// <param name="action">对所添加块表的委托n</param>
    /// <param name="ents">添加图元的委托</param>
    /// <param name="attDef">添加属性定义的委托</param>
    /// <returns>块定义id</returns>
    /// TODO: 需要测试匿名块等特殊的块是否能定义
    public static ObjectId Add(this SymbolTable<BlockTable, BlockTableRecord> table, string name,
        Action<BlockTableRecord>? action = null, Func<IEnumerable<Entity>>? ents = null,
        Func<IEnumerable<AttributeDefinition>>? attDef = null)
    {
        return table.Add(name, btr =>
        {
            action?.Invoke(btr);

            var entsRes = ents?.Invoke();
            if (entsRes is not null)
                btr.AddEntity(entsRes);

            var addDefRes = attDef?.Invoke();
            if (addDefRes is not null)
                btr.AddEntity(addDefRes);
        });
    }

    /// <summary>
    /// 添加块定义
    /// </summary>
    /// <param name="table">块表</param>
    /// <param name="name">块名</param>
    /// <param name="ents">图元</param>
    /// <param name="attDef">属性定义</param>
    /// <returns></returns>
    public static ObjectId Add(this SymbolTable<BlockTable, BlockTableRecord> table, string name,
        IEnumerable<Entity>? ents = null, IEnumerable<AttributeDefinition>? attDef = null)
    {
        return table.Add(name, btr =>
        {
            if (ents is not null)
                btr.AddEntity(ents);
            if (attDef is not null)
                btr.AddEntity(attDef);
        });
    }

    /// <summary>
    /// 添加块定义
    /// </summary>
    /// <param name="table">块表</param>
    /// <param name="name">块名</param>
    /// <param name="ents">图元(包括属性)</param>
    /// <returns></returns>
    public static ObjectId Add(this SymbolTable<BlockTable, BlockTableRecord> table, string name,
        params Entity[] ents)
    {
        return table.Add(name, null, () => ents);
    }

    /// <summary>
    /// 添加属性到块定义
    /// </summary>
    /// <param name="table">块表</param>
    /// <param name="id">块定义id</param>
    /// <param name="atts">属性列表</param>
    public static void AddAttsToBlocks(this SymbolTable<BlockTable, BlockTableRecord> table, ObjectId id,
        List<AttributeDefinition> atts)
    {
        List<string> attTags = [];
        table.Change(id, btr =>
        {
            btr.GetEntities<AttributeDefinition>().ForEach(def => attTags.Add(def.Tag.ToUpper()));

            foreach (var t in atts.Where(t => !attTags.Contains(t.Tag.ToUpper())))
                btr.AddEntity(t);
        });
    }

    /// <summary>
    /// 添加属性到块定义
    /// </summary>
    /// <param name="table">块表</param>
    /// <param name="name">块定义名字</param>
    /// <param name="atts">属性列表</param>
    public static void AddAttsToBlocks(this SymbolTable<BlockTable, BlockTableRecord> table, string name,
        List<AttributeDefinition> atts)
    {
        List<string> attTags = [];
        table.Change(name, btr =>
        {
            btr.GetEntities<AttributeDefinition>().ForEach(def => attTags.Add(def.Tag.ToUpper()));

            foreach (var t in atts.Where(t => !attTags.Contains(t.Tag.ToUpper())))
                btr.AddEntity(t);
        });
    }

    /// <summary>
    /// 从文件中获取块定义
    /// </summary>
    /// <param name="table">块表</param>
    /// <param name="fileName">文件名</param>
    /// <param name="over">是否覆盖</param>
    /// <returns>块定义Id</returns>
    public static ObjectId GetBlockFrom(this SymbolTable<BlockTable, BlockTableRecord> table, string fileName,
        bool over)
    {
        var blkDefName = SymbolUtilityServices.GetSymbolNameFromPathName(fileName, "dwg");
#if acad
        blkDefName = SymbolUtilityServices.RepairSymbolName(blkDefName, false);
#endif
        var id = table[blkDefName];
        var has = id != ObjectId.Null;

        /* 每次看这里都要反应一阵
           如果已经有这个id，并且要覆盖，或者没有这个id就执行下面的语句，不然就直接返回id
           其实就是如果有这个id，并且不覆盖，就直接返回，其他的情况都需要重新插入
           所以原代码可以修改
        if (has && over || !has)
        {
            using Database db = new(false, true);
            db.ReadDwgFile(fileName, FileShare.Read, true, null);
            db.CloseInput(true);
            id = table.Database.Insert(BlockTableRecord.ModelSpace, blkDefName, db, false);

            return id;
        } */

        if (has && over is false)
        {
            return id;
        }

        using Database db = new(false, true);
        db.ReadDwgFile(fileName, FileShare.Read, true, null);
        db.CloseInput(true);
        id = table.Database.Insert(BlockTableRecord.ModelSpace, blkDefName, db, false);
        return id;
    }


    /// <summary>
    /// 从文件中获取块定义
    /// </summary>
    /// <param name="table">块表</param>
    /// <param name="fileName">文件名</param>
    /// <param name="blockName">块定义名</param>
    /// <param name="over">是否覆盖</param>
    /// <returns>块定义Id</returns>
    public static ObjectId GetBlockFrom(this SymbolTable<BlockTable, BlockTableRecord> table, string fileName,
        string blockName, bool over)
    {
        return table.GetRecordFrom(t => t.BlockTable, fileName, blockName, over);
    }

    #endregion


    #region 线型表

    /// <summary>
    /// 添加线型
    /// </summary>
    /// <param name="table">线型表</param>
    /// <param name="name">线型名</param>
    /// <param name="description">线型说明</param>
    /// <param name="length">线型长度</param>
    /// <param name="dash">笔画长度数组</param>
    /// <returns>线型id</returns>
    public static ObjectId Add(this SymbolTable<LinetypeTable, LinetypeTableRecord> table, string name,
        string description, double length, double[] dash)
    {
        return table.Add(name, ltt =>
        {
            ltt.AsciiDescription = description;
            ltt.PatternLength = length; // 线型的总长度
            ltt.NumDashes = dash.Length; // 组成线型的笔画数目
            for (var i = 0; i < dash.Length; i++)
            {
                ltt.SetDashLengthAt(i, dash[i]);
            }
            // ltt.SetDashLengthAt(0, 0.5); // 0.5个单位的划线
            // ltt.SetDashLengthAt(1, -0.25); // 0.25个单位的空格
            // ltt.SetDashLengthAt(2, 0); // 一个点
            // ltt.SetDashLengthAt(3, -0.25); // 0.25个单位的空格
        });
    }

    #endregion

    #region 文字样式表

    /// <summary>
    /// 添加文字样式记录
    /// </summary>
    /// <param name="table">文字样式表</param>
    /// <param name="textStyleName">文字样式名</param>
    /// <param name="font">字体名</param>
    /// <param name="xScale">宽度比例</param>
    /// <returns>文字样式Id</returns>
    public static ObjectId Add(this SymbolTable<TextStyleTable, TextStyleTableRecord> table,
        string textStyleName, string font, double xScale = 1.0)
    {
        return table.Add(textStyleName, tstr =>
        {
            tstr.Name = textStyleName;
            tstr.FileName = font;
            tstr.XScale = xScale;
        });
    }

    /// <summary>
    /// 添加文字样式记录
    /// </summary>
    /// <param name="table">文字样式表</param>
    /// <param name="textStyleName">文字样式名</param>
    /// <param name="fontTtf">字体名枚举</param>
    /// <param name="xScale">宽度比例</param>
    /// <returns>文字样式Id</returns>
    public static ObjectId Add(this SymbolTable<TextStyleTable, TextStyleTableRecord> table,
        string textStyleName, FontTTF fontTtf, double xScale = 1.0)
    {
        return table.Add(textStyleName, fontTtf.GetDescription(), xScale);
    }

    /// <summary>
    /// <p>添加文字样式记录,如果存在就默认强制替换</p>
    /// <para>此函数为了 <see langword="二惊"/> 和 <see langword="edata"/> 而设</para>
    /// </summary>
    /// <param name="table">文字样式表</param>
    /// <param name="textStyleName">文字样式名</param>
    /// <param name="smallFont">字体名</param>
    /// <param name="bigFont">大字体名</param>
    /// <param name="xScale">宽度比例</param>
    /// <param name="height">高度</param>
    /// <param name="forceChange">是否强制替换</param>
    /// <returns>文字样式Id</returns>
    public static ObjectId AddWithChange(this SymbolTable<TextStyleTable, TextStyleTableRecord> table,
        string textStyleName, string smallFont, string bigFont = "", double xScale = 1, double height = 0,
        bool forceChange = true)
    {
        if (forceChange && table.Has(textStyleName))
        {
            table.Change(textStyleName, ttr =>
            {
                ttr.FileName = smallFont;
                ttr.XScale = xScale;
                ttr.TextSize = height;
                if (bigFont != string.Empty)
                    ttr.BigFontFileName = bigFont;
            });
            return table[textStyleName];
        }

        return table.Add(textStyleName, ttr =>
        {
            ttr.FileName = smallFont;
            ttr.XScale = xScale;
            ttr.TextSize = height;
        });
    }

    #endregion
}