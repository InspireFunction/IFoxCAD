namespace JoinBoxAcad;

public static class HatchHelper
{
    /// <summary>
    /// 遍历填充每条边
    /// </summary>
    /// <param name="hatch"></param>
    /// <param name="action"></param>
    public static void ForEach(this Hatch hatch, Action<HatchLoop> action)
    {
        for (int i = 0; i < hatch.NumberOfLoops; i++)
            action.Invoke(hatch.GetLoopAt(i));
    }


#if false
    /// <summary>
    /// 分离填充边界
    /// 将外边界和包含的边界成为一个集
    /// </summary>
    /// <param name="bianjie">填充的边界(只有多段线和圆)</param>
    /// <returns>多个id集,id中第一个是外边界</returns>
    public static IEnumerable<ObjectId>[] SeparationBorder(this IEnumerable<ObjectId> bianjie)
    {
        IEnumerable<ObjectId>[] objectIds = null;
        if (bianjie.Length < 1) return null;

        Database db = bianjie[0].Database;

        // 首先获取一个图元,这个图元默认成边界,看它是否包含第二个,如果是,加入a集
        // 如果不是,它是否把自己包含了,
        // 如果是,把它加入a集,并下次使用它来成为外边界
        // 如果不是,则为新边界,把它加入b集
        List<ObjectId> alist = new List<ObjectId>();                          // 边界包含
        List<ObjectId> blist = new List<ObjectId>();                          // 有另外的边界
        List<IEnumerable<ObjectId>> clist = new List<IEnumerable<ObjectId>>();// 边界集

        using (Transaction tr = db.TransactionManager.StartTransaction())
        {
            while (true)
            {
                for (int i = 0; i < bianjie.Length; i++)
                {
                    Entity ent = bianjie[i].ObjectIdToEntity(false);
                    ent.UpgradeOpen();
                    for (int j = i + 1; j < bianjie.Length; j++)
                    {
                        if (ent is Polyline polyline)// 多段线
                        {
                        }
                        else if (ent is Circle circle) // 圆
                        {
                        }
                    }
                    ent.DowngradeOpen();
                }

                if (blist.Count == 0)// 没有其他边界就结束循环
                {
                    break;
                }
                // 把blist的第一个用来作为新的外边界
            }
        }
        return objectIds;
    }

    /// <summary>
    /// 判断边界是否包含图元 布尔算法...
    /// </summary>
    /// <param name="border">边界</param>
    /// <param name="include">包含的图元</param>
    /// <returns>是true,否false</returns>
    public static bool BorderIntoCollect(this ObjectId border, ObjectId include)
    {
        bool flag = false;
        Database db = border.Database;
        using (Transaction tr = db.TransactionManager.StartTransaction())
        {
            Entity ent = border.ObjectIdToEntity(false);
            Entity ent2 = include.ObjectIdToEntity(false);

            if (ent is Polyline polyline)// 多段线边界
            {
                if (ent2 is Polyline polyline2)// 多段线
                {
                }
                else if (ent2 is Circle circle2) // 圆
                {
                    // 判断圆心在多段线内
                    if (circle2.Center.RayCasting(polyline.GetPolylinePoints()) != 3)
                    {
                        if (true)// 半径若大于多段线最长那段,表示包含不到,是圆包含了多段线(含有弧度就错了)
                        {
                            flag = true;
                        }
                    }
                    else  // 圆心不在边界内,判断边界是否有交点
                    {
                    }
                }
            }
            else if (ent is Circle circle) // 圆边界
            {
                if (ent2 is Polyline polyline2)// 多段线
                {
                }
                else if (ent2 is Circle circle2) // 圆
                {
                    // 填充边界不存在交集
                    // 两个圆心的距离>两个圆的半径和=两圆分离
                    double length = circle.Center.GetDistanceBetweenTwoPoint(circle2.Center);
                    if (length < circle.Radius + circle2.Radius)
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }
    }
#endif
}