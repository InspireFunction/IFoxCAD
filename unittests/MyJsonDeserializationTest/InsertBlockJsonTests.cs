using IFoxCAD.Cad;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace JsonDeserializationTest
{
    public class InsertBlockJsonTests
    {
        private const string JsonFilePath = @"G:\K01.惊惊连盒\03.用户配置\09.命令插块.json";

        [Fact]
        public void Test_MyJson_Deserialize_ShouldFail_WithoutParameterlessConstructor()
        {
            if (!File.Exists(JsonFilePath))
            {
                return;
            }

            string json = File.ReadAllText(JsonFilePath);

            var exception = Record.Exception(() =>
            {
                var root = MyJson.DeserializeObject<Root>(json);
            });

            Assert.NotNull(exception);
            Assert.IsType<MissingMethodException>(exception);
            Assert.Contains("parameterless constructor", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Test_Newtonsoft_Deserialize_ShouldSuccess_WithoutParameterlessConstructor()
        {
            if (!File.Exists(JsonFilePath))
            {
                return;
            }

            string json = File.ReadAllText(JsonFilePath);

            var root = JsonConvert.DeserializeObject<Root>(json);

            Assert.NotNull(root);
            Assert.NotNull(root.DynamicJsonConfig);
            Assert.NotNull(root.DynamicJsonConfig.CadCmd);
            Assert.True(root.DynamicJsonConfig.CadCmd.Count > 0);
        }

        [Fact]
        public void Test_Newtonsoft_Deserialize_WithParameterlessConstructor()
        {
            if (!File.Exists(JsonFilePath))
            {
                return;
            }

            string json = File.ReadAllText(JsonFilePath);

            var root = JsonConvert.DeserializeObject<RootWithParameterlessConstructor>(json);

            Assert.NotNull(root);
            Assert.NotNull(root.DynamicJsonConfig);
            Assert.NotNull(root.DynamicJsonConfig.CadCmd);
            Assert.True(root.DynamicJsonConfig.CadCmd.Count > 0);

            var firstCmd = root.DynamicJsonConfig.CadCmd[0];
            Assert.Equal("sgt", firstCmd.命令);
            Assert.Equal("SD_施工图标准", firstCmd.块名称);
        }

        [Fact]
        public void Test_Compare_MyJson_vs_Newtonsoft()
        {
            string json = @"
{
  ""DynamicJsonConfig"": {
    ""DwgPath"": ""*03.用户配置\\00.制图规范.dwg"",
    ""CadCmd"": [
      {
        ""命令"": ""sgt"",
        ""块名称"": ""SD_施工图标准"",
        ""屏幕点击"": 1,
        ""颜色"": ""0"",
        ""图层"": ""0"",
        ""缩放"": false,
        ""镜像"": false
      }
    ]
  }
}";

            Console.WriteLine("========== Newtonsoft.Json 测试 ==========");
            try
            {
                var newtonsoftRoot = JsonConvert.DeserializeObject<Root>(json);
                Console.WriteLine($"Newtonsoft 反序列化成功: CadCmd 数量 = {newtonsoftRoot?.DynamicJsonConfig?.CadCmd?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Newtonsoft 反序列化失败: {ex.Message}");
            }

            Console.WriteLine("\n========== MyJson 测试 ==========");
            try
            {
                var myJsonRoot = MyJson.DeserializeObject<Root>(json);
                Console.WriteLine($"MyJson 反序列化成功: CadCmd 数量 = {myJsonRoot?.DynamicJsonConfig?.CadCmd?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MyJson 反序列化失败: {ex.GetType().Name} - {ex.Message}");
            }

            Console.WriteLine("\n========== 带无参构造函数的类测试 ==========");
            try
            {
                var myJsonRoot2 = MyJson.DeserializeObject<RootWithParameterlessConstructor>(json);
                Console.WriteLine($"MyJson (无参构造函数) 反序列化成功: CadCmd 数量 = {myJsonRoot2?.DynamicJsonConfig?.CadCmd?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MyJson (无参构造函数) 反序列化失败: {ex.GetType().Name} - {ex.Message}");
            }
        }
    }

    #region 原始类 - 无无参构造函数（复制自 DynamicAssemblyJson.cs）

    [Serializable]
    public class InsertBlockCommand
    {
        public string? 命令;
        public string 块名称;
        public string 图层;
        public int 屏幕点击;
        public short 颜色;
        public bool 缩放;
        public bool 镜像;

        public InsertBlockCommand(string 块名称, string 图层, string 屏幕点击, string 颜色, string 缩放, string 镜像)
        {
            this.块名称 = 块名称;
            this.图层 = 图层;
            this.屏幕点击 = int.Parse(屏幕点击);
            this.颜色 = short.Parse(颜色);
            this.缩放 = bool.Parse(缩放);
            this.镜像 = bool.Parse(镜像);
        }

        public override string ToString()
        {
            var strs = new string[] { 块名称, 图层, 屏幕点击.ToString(), 颜色.ToString(), 缩放.ToString(), 镜像.ToString() };
            return string.Join("\",\"", strs);
        }
    }

    public class DynamicJsonConfig
    {
        public string? DwgPath;
        public List<InsertBlockCommand>? CadCmd;
    }

    public class Root
    {
        public DynamicJsonConfig? DynamicJsonConfig;
    }

    #endregion

    #region 带无参构造函数的测试类

    [Serializable]
    public class InsertBlockCommandWithParameterlessConstructor
    {
        public string? 命令;
        public string 块名称 = string.Empty;
        public string 图层 = string.Empty;
        public int 屏幕点击;
        public short 颜色;
        public bool 缩放;
        public bool 镜像;

        public InsertBlockCommandWithParameterlessConstructor() { }

        public InsertBlockCommandWithParameterlessConstructor(
            string 块名称, string 图层, string 屏幕点击, string 颜色, string 缩放, string 镜像)
        {
            this.块名称 = 块名称;
            this.图层 = 图层;
            this.屏幕点击 = int.Parse(屏幕点击);
            this.颜色 = short.Parse(颜色);
            this.缩放 = bool.Parse(缩放);
            this.镜像 = bool.Parse(镜像);
        }
    }

    public class DynamicJsonConfigWithParameterlessConstructor
    {
        public string? DwgPath;
        public List<InsertBlockCommandWithParameterlessConstructor>? CadCmd;
    }

    public class RootWithParameterlessConstructor
    {
        public DynamicJsonConfigWithParameterlessConstructor? DynamicJsonConfig;
    }

    #endregion
}
