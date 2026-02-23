using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MCP.Server;

/// <summary>
/// 成功案例记录器
/// </summary>
public class SuccessCaseLogger : IDisposable
{
    private readonly string _filePath;
    private readonly List<SuccessCase> _cases;
    private readonly object _lockObj = new object();
    private const int MaxCases = 1000;

    public SuccessCaseLogger(string filePath)
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, filePath);
        _cases = LoadCases();
    }

    /// <summary>
    /// 记录成功案例
    /// </summary>
    public void Log(SuccessCase successCase)
    {
        lock (_lockObj)
        {
            _cases.Add(successCase);

            // 限制数量
            if (_cases.Count > MaxCases)
            {
                _cases.RemoveAt(0);
            }

            // 保存到文件
            SaveCases();
        }
    }

    /// <summary>
    /// 获取所有案例
    /// </summary>
    public List<SuccessCase> GetAllCases()
    {
        lock (_lockObj)
        {
            return _cases.ToList();
        }
    }

    /// <summary>
    /// 搜索案例
    /// </summary>
    public List<SuccessCase> SearchCases(string keyword)
    {
        lock (_lockObj)
        {
            return _cases
                .Where(c => c.Command.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    /// <summary>
    /// 加载案例
    /// </summary>
    private List<SuccessCase> LoadCases()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return new List<SuccessCase>();
            }

            var json = File.ReadAllText(_filePath);
            var doc = JsonNode.Parse(json);

            if (doc == null || doc is not JsonArray array)
            {
                return new List<SuccessCase>();
            }

            var cases = new List<SuccessCase>();
            foreach (var item in array)
            {
                if (item == null) continue;

                cases.Add(new SuccessCase
                {
                    Timestamp = item["timestamp"]?.GetValue<DateTime>() ?? DateTime.MinValue,
                    Command = item["command"]?.ToString() ?? "",
                    ExecutionTime = item["execution_time"]?.GetValue<long>() ?? 0,
                    Notes = item["notes"]?.ToString() ?? ""
                });
            }

            return cases;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[WARN] Failed to load success cases: {ex.Message}");
            return new List<SuccessCase>();
        }
    }

    /// <summary>
    /// 保存案例
    /// </summary>
    private void SaveCases()
    {
        try
        {
            var array = new JsonArray();
            foreach (var c in _cases)
            {
                array.Add(new JsonObject
                {
                    ["timestamp"] = c.Timestamp,
                    ["command"] = c.Command,
                    ["execution_time"] = c.ExecutionTime,
                    ["notes"] = c.Notes
                });
            }

            var json = array.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[WARN] Failed to save success cases: {ex.Message}");
        }
    }

    public void Dispose()
    {
        SaveCases();
    }
}

/// <summary>
/// 成功案例
/// </summary>
public class SuccessCase
{
    public DateTime Timestamp { get; set; }
    public string Command { get; set; } = "";
    public long ExecutionTime { get; set; }
    public string Notes { get; set; } = "";
}
