namespace TestShared;

#if true2
public class TestJson
{
    /*
       <ItemGroup>
         <Reference Include="System.Web.Extensions" />
       </ItemGroup>
     */
    protected void Page_Load(object sender, EventArgs e)
    {
        var RegisteredUsers = new List<int>();
        RegisteredUsers.Add(0);
        RegisteredUsers.Add(1);
        RegisteredUsers.Add(2);
        RegisteredUsers.Add(3);

        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        var serializedResult = serializer.Serialize(RegisteredUsers);
        var deserializedResult = serializer.Deserialize<List<InterceptCopyclip>>(serializedResult);
    }
}
#endif