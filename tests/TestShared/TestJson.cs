namespace TestShared;

public class TestJson
{
    /*
     * 需要引入:
     * <ItemGroup>
     *     <Reference Include="System.Web" />
     *     <Reference Include="System.Web.Extensions" />
     * </ItemGroup>
     */
    [CommandMethod(nameof(JavaScriptSerializer))]
    public void JavaScriptSerializer()
    {
        var RegisteredUsers = new List<int>();
        RegisteredUsers.Add(0);
        RegisteredUsers.Add(1);
        RegisteredUsers.Add(2);
        RegisteredUsers.Add(3);

        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        var serializedResult = serializer.Serialize(RegisteredUsers);
        var deserializedResult = serializer.Deserialize<List<int>>(serializedResult);
    }
}