# MCP调用 项目计划

## 项目概述

构建一个MCP(Model Context Protocol)服务，让Trae能够自动测试CAD、调用命令、交互并获取历史输入输出。

## 技术栈

* **CAD插件**: .NET 3.5 (AutoCAD 2008)

* **控制台程序**: .NET 8.0 AOT编译

* **通讯方式**: 命名管道(Named Pipe)

***

## 架构设计

```
┌─────────────────────────────────────────────────────────────────┐
│                        Trae (MCP Client)                         │
│                     发送命令 / 接收结果                           │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ MCP Protocol (JSON-RPC)
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    MCP Server (.NET 8 AOT)                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │  MCP Handler │  │ Watchdog   │  │ Named Pipe Client       │  │
│  │  - 命令解析  │  │ - 超时检测  │  │ - 连接CAD进程           │  │
│  │  - 响应封装  │  │ - ESC发送   │  │ - 异步通讯              │  │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ Named Pipe
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│              CAD Plugin (.NET 3.5 / AutoCAD 2008)                │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │ Command     │  │ PGP Parser  │  │ Named Pipe Server       │  │
│  │ Executor    │  │ - 命令映射  │  │ - 接收命令              │  │
│  │ - 异步执行  │  │ - 版本检测  │  │ - 返回结果              │  │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘  │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │ History     │  │ Document    │  │ Status Monitor          │  │
│  │ Manager     │  │ Switcher    │  │ - CMDACTIVE检测         │  │
│  │ - 输入历史  │  │ - 文档切换  │  │ - 空闲状态              │  │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

***

## 第一期功能规划

### 1. MCP接口清单

| 接口名                  | 方法   | 参数                             | 返回值                      | 说明        |
| -------------------- | ---- | ------------------------------ | ------------------------ | --------- |
| `get_cad_info`       | call | -                              | pid, year, version       | 获取CAD进程信息 |
| `send_command`       | call | command: string, timeout?: int | success, message, output | 发送命令到CAD  |
| `get_history`        | call | doc\_name?: string             | history\[]               | 获取命令历史    |
| `switch_document`    | call | doc\_name: string              | success                  | 切换活动文档    |
| `update_pgp`         | call | -                              | success                  | 更新PGP命令缓存 |
| `get_command_status` | call | -                              | status, cmd\_active      | 获取当前命令状态  |

### 2. 命令执行流程

```
1. LLM发送命令 (如: "circle 0,0,0 500")
   ↓
2. MCP Server接收并解析
   ↓
3. 检查命令是否有 "-" 前缀版本
   - 如果有: 返回提示要求使用 "-" 前缀版本
   - 如果没有: 继续执行
   ↓
4. 通过Named Pipe发送给CAD插件
   ↓
5. CAD插件执行命令
   - 使用 Editor.CommandAsync
   - 监听 OnCompleted 回调
   ↓
6. 检测执行结果
   - 成功: 返回输出结果
   - 超时: 触发看门狗
   - 卡交互: 发送ESC
   ↓
7. 记录成功案例
```

### 3. PGP命令检测

```csharp
// 伪代码
Map<string, CommandInfo> pgpMap; // 命令名 -> 命令信息

bool HasDashVersion(string command)
{
    string upperCmd = command.ToUpper();
    if (upperCmd.StartsWith("-")) return false; // 已经是-前缀
    
    string dashVersion = "-" + upperCmd;
    return pgpMap.ContainsKey(dashVersion);
}

// 遍历支持路径的所有PGP文件
void LoadPgpCommands()
{
    foreach (var pgpPath in SupportPaths)
    {
        ParsePgpFile(pgpPath);
    }
}
```

### 4. 看门狗策略

```
命令执行超时检测:
├── 第一阶段 (5秒超时)
│   └── 发送 ESC 到CAD
│
├── 第二阶段 (再5秒超时)
│   └── 通知LLM: "CAD可能卡死，是否关闭进程?"
│   └── 等待用户决定
│
└── 状态检测
    ├── (getvar "CMDACTIVE") == 0 表示空闲
    └── Editor.GetLastInputString() 检测输入状态
```

### 5. 通讯协议设计

**Named Pipe消息格式 (JSON):**

```json
// 请求
{
    "id": "uuid",
    "type": "command",
    "payload": {
        "command": "circle 0,0,0 500",
        "timeout": 30
    }
}

// 响应
{
    "id": "uuid",
    "type": "result",
    "payload": {
        "success": true,
        "message": "命令执行成功",
        "output": [...],
        "execution_time": 1200
    }
}

// 错误
{
    "id": "uuid",
    "type": "error",
    "payload": {
        "code": "TIMEOUT",
        "message": "命令执行超时"
    }
}
```

***

## 项目结构

```
tests/TestAcad08/MCP调用/
├── MCP.Server/                          # .NET 8 AOT 控制台
│   ├── Program.cs                       # 入口
│   ├── McpServer.cs                     # MCP协议处理
│   ├── Watchdog.cs                      # 看门狗
│   ├── NamedPipeClient.cs               # 管道客户端
│   ├── ConfigManager.cs                 # JSON配置管理
│   └── MCP.Server.csproj
│
├── MCP.CADPlugin/                       # .NET 3.5 CAD插件
│   ├── Commands.cs                      # 命令定义
│   ├── NamedPipeServer.cs               # 管道服务端
│   ├── CommandExecutor.cs               # 命令执行器
│   ├── PgpParser.cs                     # PGP解析器
│   ├── HistoryManager.cs                # 历史记录管理
│   ├── DocumentManager.cs               # 文档管理
│   └── MCP.CADPlugin.csproj
│
└── config.json                          # 配置文件模板
```

***

## 实现细节

### 1. Editor.CommandAsync 兼容性

根据参考文章，CommandAsync 在 .NET 3.5 中可用：

```csharp
// .NET 3.5 兼容代码
var cr = ed.CommandAsync("circle", "0,0,0", "500");
cr.OnCompleted(() =>
{
    // 命令完成后的处理
    var psr = ed.SelectLast();
    // ...
});
```

### 2. 多线程处理

* **接收消息**: 使用新线程持续监听Named Pipe

* **执行命令**: 必须通过CAD的Idle机制在主线程执行

* **状态检测**: 使用定时器检测CMDACTIVE

```csharp
// 在CAD中执行命令的正确方式
Application.Idle += (s, e) =>
{
    if (commandQueue.TryDequeue(out var cmd))
    {
        ExecuteCommand(cmd);
    }
};
```

### 3. 配置管理

```json
{
    "cad": {
        "exe_path": "C:/Program Files/AutoCAD 2008/acad.exe",
        "startup_args": "nologo",
        "pipe_name": "MCP_CAD_{pid}"
    },
    "watchdog": {
        "timeout_stage1": 5000,
        "timeout_stage2": 5000,
        "esc_retry_count": 3
    },
    "logging": {
        "level": "info",
        "success_case_path": "./success_cases.json"
    }
}
```

***

## 风险提示

1. **版本差异**: 不同CAD版本的命令参数可能不同，必须使用"-"前缀版本
2. **线程安全**: CAD API不是线程安全的，所有命令执行必须在主线程
3. **内存泄漏**: 注意COM对象的释放，使用using语句
4. **进程卡死**: 看门狗只能检测，不能强制关闭（可能有未保存图纸）

***

## 后续扩展

### 第二期

* 图形选择接口 (SelectAll, SelectWindow等)

* 实体属性读取/修改

* 图层/块表操作

### 第三期

* 多CAD版本支持 (2008-2024)

* 图形预览功能

* 批量脚本执行

***

## 验收标准

* [ ] MCP Server能启动并监听请求

* [ ] 能正确启动CAD进程并建立管道连接

* [ ] 能发送命令并接收执行结果

* [ ] 能正确检测"-"前缀命令并提示

* [ ] 看门狗能在超时后发送ESC

* [ ] 能记录成功案例到文件

* [ ] 通讯失败时返回友好错误信息

