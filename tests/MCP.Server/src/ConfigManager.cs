using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MCP.Server
{
    /// <summary>
    /// 配置管理器
    /// </summary>
    public class ConfigManager
    {
        private const string ConfigFileName = "config.json";
        private string _configPath;

        public Config Config { get; private set; }

        public ConfigManager()
        {
            _configPath = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
            Config = new Config();
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        public bool Load()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    Console.Error.WriteLine($"[INFO] Config file not found, creating default: {_configPath}");
                    CreateDefaultConfig();
                    return false;
                }

                var json = File.ReadAllText(_configPath);
                var doc = JsonNode.Parse(json);

                if (doc == null)
                {
                    Console.Error.WriteLine("[ERROR] Failed to parse config file");
                    return false;
                }

                // 手动解析配置
                Config = ParseConfig(doc);

                // 验证配置
                if (!ValidateConfig())
                {
                    return false;
                }

                // 监控配置文件变化
                WatchConfigFile();

                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] Load config error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 手动解析配置
        /// </summary>
        private Config ParseConfig(JsonNode doc)
        {
            var config = new Config();

            // 解析 CAD 配置
            var cadNode = doc["cad"];
            if (cadNode != null)
            {
                config.Cad.ExePath = cadNode["exe_path"]?.ToString() ?? "";
                config.Cad.StartupArgs = cadNode["startup_args"]?.ToString() ?? "nologo";
                config.Cad.PipeName = cadNode["pipe_name"]?.ToString() ?? "MCP_CAD_{pid}";
            }

            // 解析 Watchdog 配置
            var watchdogNode = doc["watchdog"];
            if (watchdogNode != null)
            {
                if (watchdogNode["timeout_stage1"] != null)
                    config.Watchdog.TimeoutStage1 = watchdogNode["timeout_stage1"]!.GetValue<int>();
                if (watchdogNode["timeout_stage2"] != null)
                    config.Watchdog.TimeoutStage2 = watchdogNode["timeout_stage2"]!.GetValue<int>();
                if (watchdogNode["esc_retry_count"] != null)
                    config.Watchdog.EscRetryCount = watchdogNode["esc_retry_count"]!.GetValue<int>();
            }

            // 解析 Logging 配置
            var loggingNode = doc["logging"];
            if (loggingNode != null)
            {
                config.Logging.Level = loggingNode["level"]?.ToString() ?? "info";
                config.Logging.SuccessCasePath = loggingNode["success_case_path"]?.ToString() ?? "success_cases.json";
            }

            return config;
        }

        /// <summary>
        /// 验证配置
        /// </summary>
        private bool ValidateConfig()
        {
            if (string.IsNullOrEmpty(Config.Cad.ExePath))
            {
                Console.Error.WriteLine("[ERROR] CAD executable path is not set");
                return false;
            }

            if (!File.Exists(Config.Cad.ExePath))
            {
                Console.Error.WriteLine($"[ERROR] CAD executable not found: {Config.Cad.ExePath}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 创建默认配置
        /// </summary>
        private void CreateDefaultConfig()
        {
            var defaultConfig = new JsonObject
            {
                ["cad"] = new JsonObject
                {
                    ["exe_path"] = @"C:\Program Files\AutoCAD 2008\acad.exe",
                    ["startup_args"] = "/nologo",
                    ["pipe_name"] = "MCP_CAD_{pid}"
                },
                ["watchdog"] = new JsonObject
                {
                    ["timeout_stage1"] = 5000,
                    ["timeout_stage2"] = 5000,
                    ["esc_retry_count"] = 3
                },
                ["logging"] = new JsonObject
                {
                    ["level"] = "info",
                    ["success_case_path"] = "success_cases.json"
                }
            };

            var json = defaultConfig.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_configPath, json);
            Console.Error.WriteLine($"[INFO] Default config created: {_configPath}");
            Console.Error.WriteLine("[INFO] Please edit the config file and set the correct CAD path");
        }

        /// <summary>
        /// 监控配置文件变化
        /// </summary>
        private void WatchConfigFile()
        {
            try
            {
                var watcher = new FileSystemWatcher
                {
                    Path = Path.GetDirectoryName(_configPath)!,
                    Filter = ConfigFileName,
                    NotifyFilter = NotifyFilters.LastWrite
                };

                watcher.Changed += (s, e) => {
                    // 延迟加载，避免文件锁定
                    Task.Delay(500).ContinueWith(_ => {
                        try
                        {
                            Console.Error.WriteLine("[INFO] Config file changed, reloading...");
                            Load();
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"[WARN] Failed to reload config: {ex.Message}");
                        }
                    });
                };

                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[WARN] Failed to watch config file: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 配置根类
    /// </summary>
    public class Config
    {
        public CadConfig Cad { get; set; } = new CadConfig();
        public WatchdogConfig Watchdog { get; set; } = new WatchdogConfig();
        public LoggingConfig Logging { get; set; } = new LoggingConfig();
    }

    /// <summary>
    /// CAD配置
    /// </summary>
    public class CadConfig
    {
        public string ExePath { get; set; } = "";
        public string StartupArgs { get; set; } = "nologo";
        public string PipeName { get; set; } = "MCP_CAD_{pid}";
    }

    /// <summary>
    /// 看门狗配置
    /// </summary>
    public class WatchdogConfig
    {
        public int TimeoutStage1 { get; set; } = 5000;
        public int TimeoutStage2 { get; set; } = 5000;
        public int EscRetryCount { get; set; } = 3;
    }

    /// <summary>
    /// 日志配置
    /// </summary>
    public class LoggingConfig
    {
        public string Level { get; set; } = "info";
        public string SuccessCasePath { get; set; } = "success_cases.json";
    }
}
