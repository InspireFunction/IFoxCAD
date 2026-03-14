---
name: MCP调试技能
description: 针对AutoCAD MCP (Model Context Protocol) 调试的实战经验总结,包含常见的通信问题和解决方案.
---

# MCP调试技能

## 技能描述
MCP调试技能是针对AutoCAD MCP系统调试的经验总结,涵盖MCP Server与CAD插件之间的通信问题诊断和修复.

## 背景
MCP系统由两部分组成:
- **MCP.Server**: 外部服务,启动CAD并通过命名管道与CAD通信
- **McpPlugin**: CAD插件,运行在CAD内部,接收命令并执行

## 常见问题及解决方案

### 问题1: MCP响应收不到,服务端一直转圈

**现象**: 
- 日志显示已连接CAD,但客户端一直等待响应
- 服务端显示"等待MCP请求..."

**原因**: 
MCP协议要求响应写到**标准输出(stdout)**,但代码错误地写到了**错误流(stderr)**

**解决方案**:
```csharp
// 错误写法
Console.Error.WriteLine(response);

// 正确写法
Console.Out.WriteLine(response);
```

### 问题2: 发送请求后CAD没有响应

**现象**:
- 请求发送到CAD后,CAD端没有收到消息
- 服务端显示请求超时

**原因**:
CAD端使用 `WriteLine` 发送响应(带换行符),以换行符判断消息结束。但发送端没有添加换行符。

**解决方案**:
```csharp
// 错误写法
var requestBytes = Encoding.UTF8.GetBytes(request);

// 正确写法
var requestWithNewline = request + "\n";
var requestBytes = Encoding.UTF8.GetBytes(requestWithNewline);
```

### 问题3: 连接确认消息读取不完整

**现象**:
- 日志显示 `Received confirmation: �` (乱码)
- 连接确认消息没有正确解析

**原因**:
读取时只调用一次 `ReadAsync`,可能没等到完整消息(包括换行符)

**解决方案**:
```csharp
// 错误写法
int bytesRead = await _pipeClient.ReadAsync(buffer, 0, buffer.Length);
var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

// 正确写法 - 循环读取直到遇到换行符
var responseBuilder = new StringBuilder();
while (true)
{
    int bytesRead = await _pipeClient.ReadAsync(buffer, 0, buffer.Length);
    if (bytesRead <= 0) break;

    var chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
    responseBuilder.Append(chunk);

    if (chunk.Contains('\n'))
        break;
}
var response = responseBuilder.ToString().Trim();
```

## 调试技巧

### 添加连接确认机制
在CAD插件端,连接成功后主动发送确认消息:

```csharp
// CAD端 NamedPipeServer.cs
private void SendConnectionConfirmation(StreamWriter writer)
{
    var confirmation = new PipeResponse
    {
        Id = "",
        Type = "connected",
        Payload = new { message = "CAD MCP Plugin connected successfully" }
    };
    writer.WriteLine(MyJson.SerializeObject(confirmation));
}
```

### 日志分级
- **Console.Error**: 服务端内部日志(启动信息、调试信息)
- **Console.Out**: MCP协议响应(必须是JSON格式)

## 适用场景
- MCP服务连接成功后客户端无响应
- CAD命令发送后没有执行结果
- 命名管道通信异常
- MCP协议交互失败

## 注意事项
1. MCP协议严格区分stdout和stderr
2. 命名管道通信必须以换行符作为消息分隔符
3. 连接确认不是MCP协议标准,是自定义的调试辅助功能
4. 调试时可以添加更多Console.Error.WriteLine输出中间状态
