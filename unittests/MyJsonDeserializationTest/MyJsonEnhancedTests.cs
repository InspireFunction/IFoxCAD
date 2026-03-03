using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace JsonDeserializationTest
{
    /// <summary>
    /// 测试增强版 MyJsonEnhanced
    /// </summary>
    public class MyJsonEnhancedTests
    {
        private const string JsonFilePath = @"G:\K01.惊惊连盒\03.用户配置\09.命令插块.json";

        [Fact]
        public void Test_MyJsonEnhanced_Deserialize_WithoutParameterlessConstructor()
        {
            if (!File.Exists(JsonFilePath))
            {
                return;
            }

            string json = File.ReadAllText(JsonFilePath);

            // 使用增强版 MyJsonEnhanced
            var root = MyJsonEnhanced.DeserializeObject<Root>(json);

            Assert.NotNull(root);
            Assert.NotNull(root.DynamicJsonConfig);
            Assert.NotNull(root.DynamicJsonConfig.CadCmd);
            Assert.True(root.DynamicJsonConfig.CadCmd.Count > 0);

            var firstCmd = root.DynamicJsonConfig.CadCmd[0];
            Assert.Equal("sgt", firstCmd.命令);
            Assert.Equal("SD_施工图标准", firstCmd.块名称);
        }

        [Fact]
        public void Test_MyJsonEnhanced_TypeConversion()
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

            var root = MyJsonEnhanced.DeserializeObject<Root>(json);

            Assert.NotNull(root);
            Assert.NotNull(root.DynamicJsonConfig);
            Assert.NotNull(root.DynamicJsonConfig.CadCmd);
            Assert.Single(root.DynamicJsonConfig.CadCmd);

            var cmd = root.DynamicJsonConfig.CadCmd[0];
            Assert.Equal("sgt", cmd.命令);
            Assert.Equal("SD_施工图标准", cmd.块名称);
            Assert.Equal(1, cmd.屏幕点击);
            Assert.Equal((short)0, cmd.颜色);
            Assert.Equal("0", cmd.图层);
            Assert.False(cmd.缩放);
            Assert.False(cmd.镜像);
        }

        [Fact]
        public void Test_MyJsonEnhanced_CompareWithNewtonsoft()
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
      },
      {
        ""命令"": ""sgt2"",
        ""块名称"": ""SD_施工图标准2"",
        ""屏幕点击"": 2,
        ""颜色"": ""1"",
        ""图层"": ""1"",
        ""缩放"": true,
        ""镜像"": true
      }
    ]
  }
}";

            // 使用 Newtonsoft.Json
            var newtonsoftRoot = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(json);

            // 使用增强版 MyJsonEnhanced
            var myJsonRoot = MyJsonEnhanced.DeserializeObject<Root>(json);

            Assert.NotNull(newtonsoftRoot);
            Assert.NotNull(myJsonRoot);
            Assert.Equal(newtonsoftRoot.DynamicJsonConfig.CadCmd.Count, myJsonRoot.DynamicJsonConfig.CadCmd.Count);

            for (int i = 0; i < newtonsoftRoot.DynamicJsonConfig.CadCmd.Count; i++)
            {
                var nsCmd = newtonsoftRoot.DynamicJsonConfig.CadCmd[i];
                var myCmd = myJsonRoot.DynamicJsonConfig.CadCmd[i];

                Assert.Equal(nsCmd.命令, myCmd.命令);
                Assert.Equal(nsCmd.块名称, myCmd.块名称);
                Assert.Equal(nsCmd.图层, myCmd.图层);
                Assert.Equal(nsCmd.屏幕点击, myCmd.屏幕点击);
                Assert.Equal(nsCmd.颜色, myCmd.颜色);
                Assert.Equal(nsCmd.缩放, myCmd.缩放);
                Assert.Equal(nsCmd.镜像, myCmd.镜像);
            }
        }

        [Fact]
        public void Test_MyJsonEnhanced_Serialize()
        {
            var root = new Root
            {
                DynamicJsonConfig = new DynamicJsonConfig
                {
                    DwgPath = "*03.用户配置\\00.制图规范.dwg",
                    CadCmd = new List<InsertBlockCommand>
                    {
                        new InsertBlockCommand(
                            "SD_施工图标准",
                            "0",
                            "1",
                            "0",
                            "false",
                            "false")
                        {
                            命令 = "sgt"
                        }
                    }
                }
            };

            string json = MyJsonEnhanced.SerializeObject(root);

            Assert.NotNull(json);
            Assert.Contains("DynamicJsonConfig", json);
            Assert.Contains("SD_施工图标准", json);
            Assert.Contains("sgt", json);

            // 验证可以反序列化回来
            var deserialized = MyJsonEnhanced.DeserializeObject<Root>(json);
            Assert.NotNull(deserialized);
            Assert.Equal("sgt", deserialized.DynamicJsonConfig.CadCmd[0].命令);
        }

        [Fact]
        public void Test_MyJsonEnhanced_EmptyObject()
        {
            string json = "{}";

            var root = MyJsonEnhanced.DeserializeObject<Root>(json);

            Assert.NotNull(root);
            Assert.Null(root.DynamicJsonConfig);
        }

        [Fact]
        public void Test_MyJsonEnhanced_NullValues()
        {
            string json = @"
{
  ""DynamicJsonConfig"": {
    ""DwgPath"": null,
    ""CadCmd"": []
  }
}";

            var root = MyJsonEnhanced.DeserializeObject<Root>(json);

            Assert.NotNull(root);
            Assert.NotNull(root.DynamicJsonConfig);
            Assert.Null(root.DynamicJsonConfig.DwgPath);
            Assert.NotNull(root.DynamicJsonConfig.CadCmd);
            Assert.Empty(root.DynamicJsonConfig.CadCmd);
        }
    }
}
