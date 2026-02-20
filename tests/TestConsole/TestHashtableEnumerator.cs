using System;
using System.Collections;
using System.Collections.Generic;
using IFoxCAD.Cad;

namespace TestConsole
{
    public class TestHashtableEnumerator
    {
        public static void RunTests()
        {
            Console.WriteLine("=== HashtableEnumerator 测试 ===");
            
            try
            {
                // 创建一个Hashtable并填充一些数据
                var hashtable = new Hashtable();
                hashtable["key1"] = "value1";
                hashtable["key2"] = "value2";
                
                // 获取枚举器 - 这就是导致问题的对象类型
                var enumerator = hashtable.GetEnumerator();
                
                Console.WriteLine("1. 尝试序列化 HashtableEnumerator 对象:");
                Console.WriteLine($"   对象类型: {enumerator.GetType()}");
                
                // 先手动调用MoveNext()，这样会使枚举器进入有效状态，可能触发异常
                Console.WriteLine("   调用MoveNext()使枚举器进入有效状态...");
                var hasElements = enumerator.MoveNext();
                Console.WriteLine($"   MoveNext()返回: {hasElements}");
                                
                if(hasElements)
                {
                    Console.WriteLine($"   当前项 Key: {enumerator.Key}, Value: {enumerator.Value}");
                }
                                
                // 尝试序列化这个枚举器，这应该会触发异常
                try
                {
                    var json = MyJson.SerializeObject(enumerator);
                    Console.WriteLine($"   序列化成功: {json}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   序列化失败: {ex.Message}");
                    Console.WriteLine($"   异常类型: {ex.GetType().Name}");
                    Console.WriteLine($"   堆栈跟踪: {ex.StackTrace}");
                }
                
                Console.WriteLine("\n2. 尝试序列化包含枚举器的容器:");
                var container = new Dictionary<string, object>
                {
                    {"normal_data", "test"},
                    {"enumerator", enumerator}
                };
                
                try
                {
                    var json = MyJson.SerializeObject(container);
                    Console.WriteLine($"   容器序列化成功: {json}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   容器序列化失败: {ex.Message}");
                    Console.WriteLine($"   异常类型: {ex.GetType().Name}");
                }
                
                Console.WriteLine("\n3. 测试其他枚举器类型:");
                var list = new List<int> { 1, 2, 3 };
                var listEnumerator = list.GetEnumerator();
                
                try
                {
                    var json = MyJson.SerializeObject(listEnumerator);
                    Console.WriteLine($"   List Enumerator 序列化成功: {json}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   List Enumerator 序列化失败: {ex.Message}");
                    Console.WriteLine($"   异常类型: {ex.GetType().Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"测试过程中发生异常: {ex.Message}");
            }
            
            Console.WriteLine("HashtableEnumerator 测试完成！");
        }
    }
}