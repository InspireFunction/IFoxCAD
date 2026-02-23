namespace TestAcad08.MCP;

public class PipeRequest
{
    public object? Id { get; set; }
    public string Type { get; set; } = "";
    public Dictionary<string, object>? Payload { get; set; }
}

public class PipeResponse
{
    public object? Id { get; set; }
    public string Type { get; set; } = "";
    public object? Payload { get; set; }
}

public class JsonRpcResponse
{
    public string jsonrpc { get; } = "2.0";
    public object? id { get; set; }
    public object? result { get; set; }
}

public class JsonRpcError
{
    public int code { get; set; }
    public string message { get; set; } = "";
}

public class JsonRpcErrorResponse
{
    public string jsonrpc { get; } = "2.0";
    public object? id { get; set; }
    public JsonRpcError? error { get; set; }
}

public static class JsonRpcProtocol
{
    public static IFoxCAD.Cad.MyJsonSettings JsonSettings { get; } = new()
    {
        Formatting = IFoxCAD.Cad.Formatting.None,
        PreserveReferencesHandling = IFoxCAD.Cad.PreserveReferencesHandling.None,
        ReferenceLoopHandling = IFoxCAD.Cad.ReferenceLoopHandling.Serialize
    };
}
