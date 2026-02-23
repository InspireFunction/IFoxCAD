using System;
using System.Collections.Generic;

namespace TestAcad08.MCP
{
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
        private static readonly IFoxCAD.Cad.MyJsonSettings _jsonSettings = new()
        {
            Formatting = IFoxCAD.Cad.Formatting.None,
            PreserveReferencesHandling = IFoxCAD.Cad.PreserveReferencesHandling.None,
            ReferenceLoopHandling = IFoxCAD.Cad.ReferenceLoopHandling.Serialize
        };

        public static JsonRpcResponse? DeserializeResponse(string json)
            => IFoxCAD.Cad.MyJson.DeserializeObject<JsonRpcResponse>(json, _jsonSettings);

        public static string SerializeResponse(JsonRpcResponse response)
            => IFoxCAD.Cad.MyJson.SerializeObject(response, _jsonSettings);

        public static JsonRpcErrorResponse? DeserializeErrorResponse(string json)
            => IFoxCAD.Cad.MyJson.DeserializeObject<JsonRpcErrorResponse>(json, _jsonSettings);

        public static string SerializeErrorResponse(JsonRpcErrorResponse errorResponse)
            => IFoxCAD.Cad.MyJson.SerializeObject(errorResponse, _jsonSettings);

        public static JsonRpcResponse CreateSuccessResponse(object id, object result)
            => new JsonRpcResponse { id = id, result = result };

        public static JsonRpcErrorResponse CreateErrorResponse(object id, int code, string message)
            => new JsonRpcErrorResponse { id = id, error = new JsonRpcError { code = code, message = message } };
    }
}
