using System;
using Autodesk.AutoCAD.ApplicationServices;
using IFoxCAD.Cad;

namespace TestAcad08.MCP
{
    /// <summary>
    /// MCP插件主类
    /// 使用IFox的IFoxInitialize特性进行初始化
    /// </summary>
    public static class McpPlugin
    {
        private static NamedPipeServer? _pipeServer;
        private const string DefaultPipeName = "MCP_CAD_PIPE_{pid}";
        private static bool _isInitialized = false;

        /// <summary>
        /// 初始化MCP插件
        /// 在首次文档开启后执行
        /// </summary>
        [IFoxInitialize(Sequence.StartOnce)]
        public static void Initialize(Document doc)
        {
            try
            {
                if (_isInitialized)
                    return;

                _isInitialized = true;

                //try
                //{
                //    Console.InputEncoding = Encoding.UTF8;
                //    Console.OutputEncoding = Encoding.UTF8;
                //    Console.Out.NewLine = "\n";
                //}
                //catch (Exception ex)
                //{
                //    Env.Printl($"[MCP] Console encoding setup skipped: {ex.Message}");
                //}

                string pipeName = DefaultPipeName.Replace("{pid}", System.Diagnostics.Process.GetCurrentProcess().Id.ToString());
                Env.Printl($"[MCP] 准备创建管道: {pipeName}");

                _pipeServer = new NamedPipeServer(pipeName);
                Env.Printl("[MCP] NamedPipeServer已创建，准备启动...");

                _pipeServer.Start();
                Env.Printl($"[MCP] Plugin initialized, pipe: {pipeName}, PID: {System.Diagnostics.Process.GetCurrentProcess().Id}");
            }
            catch (Exception ex)
            {
                Env.Printl($"[MCP] Initialization error: {ex.GetType().Name}: {ex.Message}");
                Env.Printl($"[MCP] StackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 终止MCP插件
        /// </summary>
        [IFoxInitialize(Sequence.EndOnce)]
        public static void Terminate(Document doc)
        {
            try
            {
                _pipeServer?.Dispose();
                _pipeServer = null;
                _isInitialized = false;
                Env.Printl("[MCP] Plugin terminated");
            }
            catch { }
        }
    }
}
