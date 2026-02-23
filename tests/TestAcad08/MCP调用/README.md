# MCP CAD 调用项目

让Trae能够通过MCP协议自动测试CAD、调用命令、交互并获取历史输入输出。

## 项目结构

```
MCP调用/
├── MCP.CADPlugin/          # .NET 3.5 CAD插件 (AutoCAD 2008)
│   ├── NamedPipeServer.cs  # 命名管道服务器
│   ├── CommandExecutor.cs  # 命令执行器
│   ├── PgpParser.cs        # PGP命令解析器
│   ├── HistoryManager.cs   # 历史记录管理
│   ├── DocumentManager.cs  # 文档管理
│   ├── Initializer.cs      # 插件初始化器
│   └── MCP.CADPlugin.csproj
│
├── MCP.Server/             # .NET 8 AOT 控制台程序
│   ├── Program.cs          # 入口程序
│   ├── McpServer.cs        # MCP协议处理
│   ├── NamedPipeClient.cs  # 命名管道客户端
│   ├── Watchdog.cs         # 看门狗
│   ├── ConfigManager.cs    # 配置管理
│   ├── SuccessCaseLogger.cs # 成功案例记录
│   └── MCP.Server.csproj
│
├── config.json             # 配置文件
└── README.md               # 本文件
```

## 功能特性

### MCP接口

| 接口名 | 说明 |
|--------|------|
| `get_cad_info` | 获取CAD进程信息(PID、版本、年份) |
| `send_command` | 发送命令到CAD执行 |
| `get_history` | 获取命令执行历史 |
| `switch_document` | 切换活动文档 |
| `update_pgp` | 更新PGP命令缓存 |
| `get_command_status` | 获取当前命令状态 |

### 核心特性

1. **PGP命令检测**: 自动检测命令是否有`-`前缀版本，提示LLM使用非交互式版本
2. **看门狗机制**: 
   - 第一阶段(5秒): 发送ESC取消命令
   - 第二阶段(5秒): 通知用户CAD可能卡死
3. **成功案例记录**: 自动记录成功的命令执行案例
4. **多文档支持**: 支持切换文档和按文档记录历史

## 使用方法

### 1. 编译CAD插件

```bash
cd MCP.CADPlugin
dotnet build -c Release
```

### 2. 编译MCP Server

```bash
cd MCP.Server
dotnet publish -c Release -r win-x64 --self-contained true
```

### 3. 配置

编辑 `config.json`:

```json
{
    "cad": {
        "exe_path": "C:\\Program Files\\AutoCAD 2008\\acad.exe",
        "startup_args": "nologo",
        "pipe_name": "MCP_CAD_{pid}"
    }
}
```

### 4. 运行

```bash
MCP.Server.exe
```

## 注意事项

1. **必须使用`-`前缀命令**: 为避免交互式提示，请使用`-circle`而非`circle`
2. **线程安全**: CAD API不是线程安全的，所有命令在主线程执行
3. **进程安全**: 看门狗不会自动关闭CAD进程（可能有未保存图纸）

## 技术栈

- **CAD插件**: .NET 3.5 (AutoCAD 2008)
- **MCP Server**: .NET 8.0 AOT编译
- **通讯**: Named Pipe
- **协议**: MCP (Model Context Protocol)
