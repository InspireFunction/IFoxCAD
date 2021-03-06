namespace Test
{
    public class TestPoint
    {
        [CommandMethod("TestptGethash")]
        public void TestptGethash()
        {
            // test
            var pt = Env.Editor.GetPoint("pick pt").Value;
            //Tools.TestTimes2(1_000_000, "新语法", () => {
            //    pt.GetHashString2();
            //});
            Tools.TestTimes2(1_000_000, "旧语法", () => {
                pt.GetHashString();
            });
        }

        [CommandMethod("Testpoint3d")]
        public void TestPoint3d()
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

        [CommandMethod("Testlistequalspeed")]
        public void Testlistequalspeed()
        {
            var lst1 = new List<int> { 1, 2, 3, 4 };
            var lst2 = new List<int> { 1, 2, 3, 4};
            lst1.EqualsAll(null);
            Tools.TestTimes2(1000000, "eqaulspeed:", () => {
                lst1.EqualsAll(lst2);
            });
            

        }

        [CommandMethod("Testcontains")]
        public void Testcontains()
        {
            // test list and dict contains speed
            var lst = new List<int> { 1, 2, 3, 4 , 5,6,7,8,9,10, 11,12,13,14,15,16,17,18,19,20};
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
    
    
}