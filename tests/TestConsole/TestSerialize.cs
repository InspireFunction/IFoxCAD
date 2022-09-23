using System.Runtime.Serialization;

namespace TestConsole;

[Serializable]
public struct StructDemo
{
    public char[] Chars1;
    public char[] Chars2;
}

[Serializable]
public struct StructDemo2 : ISerializable
{
    public char[] Chars1;
    public char[] Chars2;

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("i", Chars1);
        info.AddValue("j", Chars2);
    }
}

// 二进制序列化,存在元数据
//{
//    StructDemo structDemo = new();
//    structDemo.Chars1 = "aaa".ToCharArray();
//    structDemo.Chars2 = "bbb".ToCharArray();

//    using MemoryStream stream = new();
//    BinaryFormatter formatter = new();
//    formatter.Serialize(stream, structDemo);
//    var str = Encoding.ASCII.GetString(stream.ToArray());
//    Console.WriteLine(str);
//}

//{
//    StructDemo2 structDemo = new();
//    structDemo.Chars1 = "aaa".ToCharArray();
//    structDemo.Chars2 = "bbb".ToCharArray();

//    using MemoryStream stream = new();
//    BinaryFormatter formatter = new();
//    formatter.Serialize(stream, structDemo);
//    var str = Encoding.ASCII.GetString(stream.ToArray());
//    Console.WriteLine(str);
//}