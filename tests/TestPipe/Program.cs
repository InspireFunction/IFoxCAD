using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using IFoxCAD.Cad;

namespace TestPipe
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("测试管道通信...");

            // 模拟CAD端序列化
            var payload = new { message = "欢迎使用CAD" };
            var response = new
            {
                Id = "test-id",
                Type = "result",
                Payload = payload
            };

            var settings = new MyJsonSettings
            {
                Formatting = Formatting.None,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            };

            string json = MyJson.SerializeObject(response, settings);
            Console.WriteLine($"CAD端序列化结果: {json}");
            Console.WriteLine($"JSON长度: {json.Length}");

            // 测试StreamWriter.WriteLine的行为
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, Encoding.UTF8) { AutoFlush = true };
            writer.WriteLine(json);
            writer.Flush();

            ms.Position = 0;
            var bytes = ms.ToArray();
            Console.WriteLine($"StreamWriter.WriteLine写入的字节数: {bytes.Length}");
            Console.WriteLine($"字节内容: {BitConverter.ToString(bytes)}");

            // 检查换行符
            Console.WriteLine($"最后两个字节: {bytes[bytes.Length - 2]:X2} {bytes[bytes.Length - 1]:X2}");
            Console.WriteLine($"是否为\\r\\n: {bytes[bytes.Length - 2] == 0x0D && bytes[bytes.Length - 1] == 0x0A}");

            // 测试反序列化
            ms.Position = 0;
            using var reader = new StreamReader(ms, Encoding.UTF8);
            string received = reader.ReadLine();
            Console.WriteLine($"StreamReader.ReadLine读取结果: {received}");

            // 测试MyJson反序列化
            var deserialized = MyJson.DeserializeObject<Dictionary<string, object>>(received);
            Console.WriteLine($"反序列化成功: {deserialized != null}");

            if (deserialized != null)
            {
                var payloadObj = deserialized.TryGetValue("Payload", out var pl) ? pl : null;
                Console.WriteLine($"Payload类型: {payloadObj?.GetType().Name}");
                Console.WriteLine($"Payload值: {MyJson.SerializeObject(payloadObj, settings)}");

                var payloadDict = payloadObj as Dictionary<string, object>;
                Console.WriteLine($"转换为Dictionary<string, object>: {(payloadDict != null ? "成功" : "失败")}");

                if (payloadDict != null)
                {
                    var message = payloadDict.TryGetValue("message", out var msg) ? msg?.ToString() : null;
                    Console.WriteLine($"Message: {message}");
                }
            }
        }
    }
}
