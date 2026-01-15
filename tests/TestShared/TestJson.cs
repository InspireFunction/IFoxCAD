namespace TestShared;

public class TestJson
{
    // ���л�����
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

        Env.Printl("���л�:");
        var serializedResult = MyJson.SerializeObject(RegisteredUsers);
        Env.Printl(serializedResult);

        Env.Printl("�����л�:");
        var deserializedResult = MyJson.DeserializeObject<List<int>>(serializedResult);
    }
}