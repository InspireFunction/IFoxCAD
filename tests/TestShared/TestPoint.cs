namespace Test;
public class TestPoint
{
#if false
    [CommandMethod(nameof(Test_GetAngle))]
    public void Test_GetAngle()
    {
        var pt1 = new Point2d(0, 0);
        var pt2 = new Point2d(1, 1);
        var angle_pt1_pt2 = pt1.GetAngle(pt2);
        var angle_pt2_pt1 = pt2.GetAngle(pt1);
        Env.Printl($"pt1-pt2 angle is : {angle_pt1_pt2}, 角度是： {MathEx.ConvertRadToDeg(angle_pt1_pt2)}");
        Env.Printl($"pt2-pt1 angle is : {angle_pt2_pt1}, 角度是： {MathEx.ConvertRadToDeg(angle_pt2_pt1)}");

        var polar = pt1.Polar(Math.PI / 2, 10);
        Env.Printl($"pt1 90° 距离10的点是: {polar}");
        
    }
#endif
    [CommandMethod(nameof(Test_Endtoend))]
    public void Test_Endtoend()
    {
        var pts = new Point2dCollection
        {
            new(0, 0),
            new(0, 1),
            new(1, 1),
            new(1, 0)
        };


        foreach (Point2d pt in pts)
        {
            Env.Printl($"X={pt.X},Y={pt.Y}");
        }
        pts.End2End();
        Env.Printl("-------");
        foreach (Point2d pt in pts)
        {
            Env.Printl($"X={pt.X},Y={pt.Y}");
        }
        Env.Printl("--------");
        var ptss = new Point3dCollection
        {
            new(0, 0,0),
            new(0, 1,0),
            new(1, 1,0),
            new(1, 0,0)
        };
        
        foreach (Point3d pt in ptss)
        {
            Env.Printl($"X={pt.X},Y={pt.Y},Z={pt.Z}");
        }
        ptss.End2End();
        Env.Printl("-------");
        foreach (Point3d pt in ptss)
        {
            Env.Printl($"X={pt.X},Y={pt.Y},Z={pt.Z}");
        }
        
    }
    
    /// <summary>
    /// 红黑树排序点集
    /// </summary>
    [CommandMethod(nameof(Test_PtSortedSet))]
    public void Test_PtSortedSet()
    {
        var ss1 = new SortedSet<Point2d>
        {
            new Point2d(1, 1),
            new Point2d(4.6, 2),
            new Point2d(8, 3),
            new Point2d(4, 3),
            new Point2d(5, 40),
            new Point2d(6, 5),
            new Point2d(1, 6),
            new Point2d(7, 6),
            new Point2d(9, 6)
        };

        /*判断区间,超过就中断*/
        foreach (var item in ss1)
        {
            if (item.X > 3 && item.X < 7)
                DebugEx.Printl(item);
            else if (item.X >= 7)
                break;
        }
    }



    [CommandMethod(nameof(Test_PtGethash))]
    public void Test_PtGethash()
    {
        // test
        var pt = Env.Editor.GetPoint("pick pt").Value;
        // Tools.TestTimes2(1_000_000, "新语法", () => {
        //    pt.GetHashString2();
        // });
        Tools.TestTimes2(1_000_000, "旧语法", () => {
            pt.GetHashString();
        });
    }

    [CommandMethod(nameof(Test_Point3dHash))]
    public void Test_Point3dHash()
    {
        Env.Print($"4位小数的hash：{new Point3d(0.0_001, 0.0_002, 0.0).GetHashCode()}");
        Env.Print($"5位小数的hash：{new Point3d(0.00_001, 0.00_002, 0.0).GetHashCode()}");
        Env.Print($"6位小数的hash：{new Point3d(0.000_001, 0.000_002, 0.0).GetHashCode()}");
        Env.Print($"7位小数的hash：{new Point3d(0.000_0_001, 0.000_0_002, 0.0).GetHashCode()}");
        Env.Print($"8位小数的hash：{new Point3d(0.000_00_001, 0.000_00_002, 0.0).GetHashCode()}");
        Env.Print($"9位小数的hash：{new Point3d(0.000_000_001, 0.000_000_002, 0.0).GetHashCode()}");
        Env.Print($"10位小数的hash：{new Point3d(0.000_000_0001, 0.000_000_0002, 0.0).GetHashCode()}");
        Env.Print($"10位小数的hash：{new Point3d(0.000_000_0001, 0.000_000_0001, 0.0).GetHashCode()}");

        Env.Print($"11位小数的hash：{new Point3d(0.000_000_000_01, 0.000_000_000_02, 0.0).GetHashCode()}");
        Env.Print($"11位小数的hash：{new Point3d(0.000_000_000_01, 0.000_000_000_01, 0.0).GetHashCode()}");

        Env.Print($"12位小数的hash：{new Point3d(0.000_000_000_001, 0.000_000_000_002, 0.0).GetHashCode()}");
        Env.Print($"12位小数的hash：{new Point3d(0.000_000_000_001, 0.000_000_000_001, 0.0).GetHashCode()}");

        Env.Print($"13位小数的hash：{new Point3d(0.000_000_000_0001, 0.000_000_000_0002, 0.0).GetHashCode()}");
        Env.Print($"13位小数的hash：{new Point3d(0.000_000_000_0001, 0.000_000_000_0001, 0.0).GetHashCode()}");

        Env.Print($"14位小数的hash：{new Point3d(0.000_000_000_000_01, 0.000_000_000_000_02, 0.0).GetHashCode()}");
        Env.Print($"14位小数的hash：{new Point3d(0.000_000_000_000_01, 0.000_000_000_000_01, 0.0).GetHashCode()}");

        Env.Print($"15位小数的hash：{new Point3d(0.000_000_000_000_001, 0.000_000_000_000_002, 0.0).GetHashCode()}");
        Env.Print($"15位小数的hash：{new Point3d(0.000_000_000_000_001, 0.000_000_000_000_001, 0.0).GetHashCode()}");

        Env.Print($"16位小数的hash：{new Point3d(0.000_000_000_000_000_1, 0.000_000_000_000_000_2, 0.0).GetHashCode()}");
        Env.Print($"16位小数的hash：{new Point3d(0.000_000_000_000_000_1, 0.000_000_000_000_000_1, 0.0).GetHashCode()}");

        Env.Print($"17位小数的hash：{new Point3d(0.000_000_000_000_000_01, 0.000_000_000_000_000_02, 0.0).GetHashCode()}");
        Env.Print($"17位小数的hash：{new Point3d(0.000_000_000_000_000_01, 0.000_000_000_000_000_01, 0.0).GetHashCode()}");

        Env.Print($"18位小数的hash：{new Point3d(0.000_000_000_000_000_001, 0.000_000_000_000_000_002, 0.0).GetHashCode()}");
        Env.Print($"18位小数的hash：{new Point3d(0.000_000_000_000_000_001, 0.000_000_000_000_000_001, 0.0).GetHashCode()}");

        Env.Print($"19位小数的hash：{new Point3d(0.000_000_000_000_000_000_1, 0.000_000_000_000_000_000_2, 0.0).GetHashCode()}");
        Env.Print($"19位小数的hash：{new Point3d(0.000_000_000_000_000_000_1, 0.000_000_000_000_000_000_1, 0.0).GetHashCode()}");

        Env.Print($"20位小数的hash：{new Point3d(0.000_000_000_000_000_000_01, 0.000_000_000_000_000_000_02, 0.0).GetHashCode()}");
        Env.Print($"20位小数的hash：{new Point3d(0.000_000_000_000_000_000_01, 0.000_000_000_000_000_000_01, 0.0).GetHashCode()}");
    }

    [CommandMethod(nameof(Test_ListEqualspeed))]
    public void Test_ListEqualspeed()
    {
        var lst1 = new List<int> { 1, 2, 3, 4 };
        var lst2 = new List<int> { 1, 2, 3, 4 };
        lst1.SequenceEqual(null!);
        Tools.TestTimes2(1000000, "eqaulspeed:", () => {
            lst1.SequenceEqual(lst2);
        });
    }

    [CommandMethod(nameof(Test_Contains))]
    public void Test_Contains()
    {
        // test list and dict contains speed
        var lst = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
        var hashset = new HashSet<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
        var dict = new Dictionary<int, int>
        {
            { 1, 0 },
            { 2, 1 },
            { 3, 2 },
            { 4, 3 },
            { 5, 4 },
            { 6, 5 },
            { 7, 6 },
            { 8, 7 },
            { 9, 8 },
            { 10, 9 },
            { 11, 11 },
            { 12, 12 },
            { 13, 13 },
            { 14, 14 },
            { 15, 15 },
            { 16, 16 },
            { 17, 17 },
            { 18, 18 },
            { 19, 19 },
            { 20, 20 },
        };

        Tools.TestTimes2(100_0000, "list:", () => {
            lst.Contains(20);
        });

        Tools.TestTimes2(100_0000, "hashset:", () => {
            hashset.Contains(20);
        });

        Tools.TestTimes2(100_0000, "dict:", () => {
            dict.ContainsKey(20);
        });
    }
}