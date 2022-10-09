using Autodesk.AutoCAD.Runtime;

namespace TestShared;

public class TestMarshal
{
    [CommandMethod(nameof(Test_Marshal))]
    public void Test_Marshal()
    {
        // 0x01 申请内存,拷贝进去.那我要怎么直接获取这段内存然后直接修改呢?
        Point3d structObj = new(100, 50, 0);
        IntPtr structPtr = Marshal.AllocHGlobal(Marshal.SizeOf(structObj));
        Marshal.StructureToPtr(structObj, structPtr, true);
        "打印A:".Print();
        structObj.Print();

        // 0x02 拷贝到数组
        int typeSize = Marshal.SizeOf(structObj);
        byte[] bytes = new byte[typeSize];
        Marshal.Copy(structPtr, bytes, 0, typeSize);

        // 0x03 还原指针到结构
        // 将内存空间转换为目标结构体
        var result = (Point3d)Marshal.PtrToStructure(structPtr, typeof(Point3d));
        "打印B:".Print();
        result.Print();

        // 将Point3d内存转为Point3D,以此避开get保护,实现修改内部值
        unsafe
        {
            int* p = (int*)&structObj;
            var result2 = (Point3D)Marshal.PtrToStructure((IntPtr)p, typeof(Point3D));
            result2.SetX(220);
            Marshal.StructureToPtr(result2, (IntPtr)p, true);
        }
        "打印C:".Print();
        structObj.Print();

        // 避免在安全类型中转换,直接用结构指针处理
        unsafe
        {
            var structPt = (Point3D*)&structObj;
            structPt->SetY(1569);
        }
        "打印D:".Print();
        structObj.Print();


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