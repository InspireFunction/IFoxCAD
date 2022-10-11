namespace TestShared;

public class TestMarshal
{
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
            var p = (IntPtr)(IntPtr*)&pt;
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