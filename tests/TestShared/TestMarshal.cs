using System.Diagnostics;
using static IFoxCAD.Basal.WindowsAPI;

namespace TestShared;

public class TestMarshal
{
    [CommandMethod(nameof(TestToBytes))]
    public void TestToBytes()
    {
        var ptA = new Point3d(123, 456, 789);
        var bytes = StructToBytes(ptA);
        var ptB = BytesToStruct<Point3d>(bytes);
        Env.Printl(ptB);
    }

    [CommandMethod(nameof(Test_ChangeLinePoint))]
    public void Test_ChangeLinePoint()
    {
        var prs = Env.Editor.SSGet("\n 选择直线");
        if (prs.Status != PromptStatus.OK)
            return;

        using DBTrans tr = new();

        prs.Value.GetObjectIds().ForEach(id => {
            var line = id.GetObject<Line>();
            if (line == null)
                return;
            using (line.ForWrite())
                unsafe
                {
                    // 不允许直接 &这个表达式进行取址(因为它是属性,不是字段)
                    // 实际上就是默认拷贝一份副本出来
                    var p1 = line.StartPoint;
                    ((Point3D*)&p1)->X = 0;
                    ((Point3D*)&p1)->Y = 0;
                    ((Point3D*)&p1)->Z = 0;
                    line.StartPoint = p1;// 又放回去,节省了一个变量

                    var p2 = line.EndPoint;
                    ((Point3D*)&p2)->X = 100;
                    ((Point3D*)&p2)->Y = 100;
                    ((Point3D*)&p2)->Z = 0;
                    line.EndPoint = p2;
                }
        });
    }

    [CommandMethod(nameof(Test_ImplicitPoint3D))]
    public void Test_ImplicitPoint3D()
    {
        // 无法用指针转换类型,所以隐式转换是无法不new的,
        // 貌似是因为
        // 如果发生了获取对象的成员引用指针,没有new的话,会发生引用不计数...造成GC释放失效...
        // 而微软没有提供一种计数转移的方法...造成我无法实现此操作...
        unsafe
        {
            Point3d pt1 = new(1, 56, 89);
            var a1 = (Point3D*)&pt1;
            DebugEx.Printl("指针类型转换,获取x::" + a1->X);

            var pt2 = Point3D.Create(new IntPtr(&pt1));
            DebugEx.Printl("pt1地址::" + (int)&pt1);
            DebugEx.Printl("pt2地址::" + (int)&pt2);
            Debug.Assert(&pt1 == &pt2);//不相等,是申请了新内存
        }
    }

    [CommandMethod(nameof(Test_Marshal))]
    public void Test_Marshal()
    {
        var dm = Acap.DocumentManager;
        var doc = dm.MdiActiveDocument;
        var ed = doc.Editor;
        // 0x01 如何修改Point3d内容?
        Point3d pt = new(100, 50, 0);
        ed.WriteMessage("\n原始:" + pt.ToString());

        // 0x02 最佳方法:
        // 将Point3d内存转为Point3D,以此避开get保护,实现修改内部值
        // 为了避免在安全类型中转换,多了栈帧(无法内联),直接用指针处理
        unsafe
        {
            ((Point3D*)&pt)->X = 12345;//必须强转成这个指针类型,不然它为(Point3d*)
        }
        ed.WriteMessage("\n指针法:" + pt.ToString());

        // 0x03 此方法仍然需要不安全操作,而且多了几个函数调用...
        unsafe
        {
            var p = new IntPtr(&pt);
            var result2 = Point3D.Create(p);
            result2.X = 220;
            result2.ToPtr(p);
        }
        "封送法:".Print();
        pt.Print();

        // 拷贝到数组,还原指针到结构,最后将内存空间转换为目标结构体
        // 浪费内存,这不闹嘛~
        int typeSize = Marshal.SizeOf(pt);
        byte[] bytes = new byte[typeSize];
        IntPtr structPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pt));
        Marshal.StructureToPtr(pt, structPtr, true);
        Marshal.Copy(structPtr, bytes, 0, typeSize);
        var result = (Point3d)(Marshal.PtrToStructure(structPtr, typeof(Point3d)) ?? throw new InvalidOperationException());
        "内存拷贝:".Print();
        result.Print();

        //这个是不对的,会获取类型的指针,替换了就错误了
        //RuntimeTypeHandle handle = structObj.GetType().TypeHandle;
        //IntPtr ptr = handle.Value;
        //var result3 = (Point3D)Marshal.PtrToStructure(ptr, typeof(Point3D));
        //result3.SetX(330);
        //Marshal.StructureToPtr(result3, ptr, true);
        //"打印D:".Print();
        //structObj.Print();

        // 释放内存
        Marshal.FreeHGlobal(structPtr);
    }
}