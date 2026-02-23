namespace TestAcad08.MCP
{
    /// <summary>
    /// PGP文件解析器
    /// </summary>
    public class PgpParser
    {
        private readonly Dictionary<string, CommandInfo> _commands;
        private readonly object _lockObj = new object();

        public PgpParser()
        {
            _commands = new Dictionary<string, CommandInfo>(StringComparer.OrdinalIgnoreCase);
            Reload();
        }

        public int CommandCount
        {
            get
            {
                lock (_lockObj)
                {
                    return _commands.Count;
                }
            }
        }

        public void Reload()
        {
            lock (_lockObj)
            {
                _commands.Clear();

                try
                {
                    var supportPaths = GetSupportPaths();

                    foreach (var path in supportPaths)
                    {
                        if (!Directory.Exists(path)) continue;

                        var pgpFiles = Directory.GetFiles(path, "*.pgp");
                        foreach (var pgpFile in pgpFiles)
                        {
                            ParsePgpFile(pgpFile);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PgpParser.Reload error: {ex.Message}");
                }
            }
        }

        public bool HasDashVersion(string command)
        {
            lock (_lockObj)
            {
                string cmdName = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                        .FirstOrDefault() ?? "";

                if (string.IsNullOrEmpty(cmdName)) return false;
                if (cmdName.StartsWith("-")) return false;

                string dashVersion = "-" + cmdName;
                return _commands.ContainsKey(dashVersion);
            }
        }

        public CommandInfo? GetCommandInfo(string commandName)
        {
            lock (_lockObj)
            {
                _commands.TryGetValue(commandName, out var info);
                return info;
            }
        }

        public List<string> GetAllCommandNames()
        {
            lock (_lockObj)
            {
                return _commands.Keys.ToList();
            }
        }

        private void ParsePgpFile(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();

                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";"))
                        continue;

                    var parts = trimmedLine.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                          .Select(p => p.Trim())
                                          .ToArray();

                    if (parts.Length >= 2)
                    {
                        string alias = parts[0];
                        string cmdDef = parts[1];

                        if (cmdDef.StartsWith("*"))
                        {
                            cmdDef = cmdDef.Substring(1);
                        }

                        var info = new CommandInfo
                        {
                            Alias = alias,
                            CommandName = cmdDef,
                            Description = parts.Length > 2 ? parts[2] : "",
                            SourceFile = filePath
                        };

                        if (!_commands.ContainsKey(alias))
                        {
                            _commands[alias] = info;
                        }

                        if (!_commands.ContainsKey(cmdDef))
                        {
                            _commands[cmdDef] = info;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ParsePgpFile error ({filePath}): {ex.Message}");
            }
        }

        private List<string> GetSupportPaths()
        {
            var paths = new List<string>();

            try
            {
                var supportPath = Acap.GetSystemVariable("ACAD")?.ToString();
                if (!string.IsNullOrEmpty(supportPath))
                {
                    var pathArray = supportPath?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    paths.AddRange(pathArray.Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)));
                }

                var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var cadDir = Path.GetDirectoryName(exePath);
                if (!string.IsNullOrEmpty(cadDir) && !paths.Contains(cadDir))
                {
                    paths.Add(cadDir);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetSupportPaths error: {ex.Message}");
            }

            return paths;
        }
    }

    public class CommandInfo
    {
        public string Alias { get; set; } = "";
        public string CommandName { get; set; } = "";
        public string Description { get; set; } = "";
        public string SourceFile { get; set; } = "";
    }
}
