namespace TestAcad08.MCP;

public static class HistoryManager
{
    private static readonly Dictionary<string, List<HistoryEntry>> _documentHistories;
    private static readonly object _lockObj = new object();
    private static readonly int MaxHistoryPerDoc = 1000;

    static HistoryManager()
    {
        _documentHistories = new Dictionary<string, List<HistoryEntry>>(StringComparer.OrdinalIgnoreCase);
    }

    public static void AddEntry(HistoryEntry entry)
    {
        lock (_lockObj)
        {
            string docKey = GetCurrentDocumentKey();

            if (!_documentHistories.ContainsKey(docKey))
            {
                _documentHistories[docKey] = [];
            }

            var list = _documentHistories[docKey];
            list.Add(entry);

            if (list.Count > MaxHistoryPerDoc)
            {
                list.RemoveAt(0);
            }
        }
    }

    public static List<HistoryEntry> GetHistory(string? documentName = null)
    {
        lock (_lockObj)
        {
            string docKey = documentName ?? GetCurrentDocumentKey();

            if (_documentHistories.TryGetValue(docKey, out var history))
            {
                return history.ToList();
            }

            return new List<HistoryEntry>();
        }
    }

    public static List<HistoryEntry> GetLastEntries(int count, string? documentName = null)
    {
        lock (_lockObj)
        {
            var history = GetHistory(documentName);
            int startIndex = Math.Max(0, history.Count - count);
            return history.Skip(startIndex).Take(count).ToList();
        }
    }

    public static void ClearHistory(string? documentName = null)
    {
        lock (_lockObj)
        {
            if (documentName is null || string.IsNullOrEmpty(documentName))
            {
                _documentHistories.Clear();
            }
            else
            {
                _documentHistories.Remove(documentName);
            }
        }
    }

    public static void ExportToFile(string filePath, string? documentName = null)
    {
        lock (_lockObj)
        {
            var history = GetHistory(documentName);
            var sb = new StringBuilder();

            sb.AppendLine("Timestamp,Command,Success,ExecutionTime(ms),Message");

            foreach (var entry in history)
            {
                sb.AppendLine($"{entry.Timestamp:yyyy-MM-dd HH:mm:ss},{EscapeCsv(entry.Command)},{entry.Success},{entry.ExecutionTime},{EscapeCsv(entry.Message)}");
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }

    private static string GetCurrentDocumentKey()
    {
        try
        {
            var doc = Acap.DocumentManager.MdiActiveDocument;
            return doc?.Name ?? "Default";
        }
        catch
        {
            return "Default";
        }
    }

    private static string EscapeCsv(string? field)
    {
        if (field is null || string.IsNullOrEmpty(field)) return "";

        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }

        return field;
    }
}

public class HistoryEntry
{
    public DateTime Timestamp { get; set; }
    public string Command { get; set; } = "";
    public bool Success { get; set; }
    public long ExecutionTime { get; set; }
    public string Message { get; set; } = "";
}
