using System.Diagnostics;

namespace TestShared;

public class TestMarshal
{
    [CommandMethod(nameof(Test_DebuggerStepThrough))]
    public void Test_DebuggerStepThrough()
    {
        DebuggerStepThrough(() => {
            for (int i = 0; i < 10; i++)//断点可以进入此处
            {
            }
        });
    }

    [System.Diagnostics.DebuggerStepThrough]
    public void DebuggerStepThrough(Action action)
    {
        //throw new ArgumentNullException(nameof(action));//可以抛出
        int a = 0;//断点无法进入此处
        int b = 0;
        action?.Invoke();
        int c = 0;
        int d = 0;
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
            Debug.WriteLine("指针类型转换,获取x::" + a1->X);

            var a = (IntPtr)(&pt1);//explicit 显式转换 == new
            var pt2 = (Point3D)Marshal.PtrToStructure(a, typeof(Point3D));
            Debug.WriteLine("pt1转IntPtr地址::" + (int)&a);
            Debug.WriteLine("pt1地址::" + (int)&pt1);
            Debug.WriteLine("pt2地址::" + (int)&pt2);
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
            var result2 = (Point3D)Marshal.PtrToStructure(p, typeof(Point3D));
            result2.X = 220;
            Marshal.StructureToPtr(result2, p, true);
        }
        "封送法:".Print();
        pt.Print();

        // 拷贝到数组,还原指针到结构,最后将内存空间转换为目标结构体
        // 这不闹嘛~
        int typeSize = Marshal.SizeOf(pt);
        byte[] bytes = new byte[typeSize];
        IntPtr structPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pt));
        Marshal.StructureToPtr(pt, structPtr, true);
        Marshal.Copy(structPtr, bytes, 0, typeSize);
        var result = (Point3d)Marshal.PtrToStructure(structPtr, typeof(Point3d));
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