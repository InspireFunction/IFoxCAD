namespace Test
{
    public class TestPoint
    {
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
    }
}