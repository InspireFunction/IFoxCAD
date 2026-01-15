namespace TestShared;

public class TestJson
{
    // 序列化测试
    [CommandMethod(nameof(JavaScriptSerializer))]
    public void JavaScriptSerializer()
    {
        var RegisteredUsers = new List<int>
        {
            0,
            1,
            2,
            3
        };

        Env.Printl("序列化:");
        var serializedResult = MyJson.SerializeObject(RegisteredUsers);
        Env.Printl(serializedResult);

        Env.Printl("反序列化:");
        var deserializedResult = MyJson.DeserializeObject<List<int>>(serializedResult);
    }
}