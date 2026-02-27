namespace BasalUnitTests;

public class EnumTests
{
    [Flags]
    public enum PlugIn
    {
        [Description("惊惊")]
        JoinBox = 1,
        [Description("源泉")]
        YuanQuan = 2,
        [Description("迷你")]
        IMinCad = 4,

        Lisp = JoinBox | YuanQuan | IMinCad,

        DOCBAR = 8,
        DUOTAB = 16,

        All = Lisp | DOCBAR | DUOTAB
    }

    [Flags]
    public enum PlugIn2
    {
        [Description("惊惊")]
        JoinBox = 1,
        [Description("源泉")]
        YuanQuan = 2,
        [Description("迷你")]
        IMinCad = 4,

        [Description("*Lisp*")]
        Lisp = JoinBox | YuanQuan | IMinCad,

        DOCBAR = 8,
        DUOTAB = 16,

        All = Lisp | DOCBAR | DUOTAB
    }

    public enum Season : byte
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }

    [Fact]
    public void PlugIn_SingleValue_HasCorrectDescription()
    {
        var result = PlugIn.JoinBox.PrintNote();
        Assert.Equal("惊惊", result);
    }

    [Fact]
    public void PlugIn_CombinedValueWithoutDescription_ReturnsCombinedDescriptions()
    {
        var result = PlugIn.Lisp.PrintNote();

        Assert.Contains("惊惊", result);
        Assert.Contains("源泉", result);
        Assert.Contains("迷你", result);
    }

    [Fact]
    public void PlugIn_All_ReturnsAllDescriptions()
    {
        var result = PlugIn.All.PrintNote();

        Assert.Contains("惊惊", result);
        Assert.Contains("源泉", result);
        Assert.Contains("迷你", result);
    }

    [Fact]
    public void PlugIn2_LispHasDescription_ReturnsItsDescription()
    {
        var result = PlugIn2.Lisp.PrintNote();
        Assert.Equal("*Lisp*", result);
    }

    [Fact]
    public void PlugIn_HasFlag_ReturnsTrueForContainedFlag()
    {
        var lisp = PlugIn.Lisp;

        Assert.True(lisp.HasFlag(PlugIn.JoinBox));
        Assert.True(lisp.HasFlag(PlugIn.YuanQuan));
        Assert.True(lisp.HasFlag(PlugIn.IMinCad));
    }

    [Fact]
    public void PlugIn_HasFlag_ReturnsFalseForNotContainedFlag()
    {
        var lisp = PlugIn.Lisp;

        Assert.False(lisp.HasFlag(PlugIn.DOCBAR));
        Assert.False(lisp.HasFlag(PlugIn.DUOTAB));
    }

    [Fact]
    public void Season_EnumValues_HaveCorrectByteValues()
    {
        Assert.Equal((byte)0, (byte)Season.Spring);
        Assert.Equal((byte)1, (byte)Season.Summer);
        Assert.Equal((byte)2, (byte)Season.Autumn);
        Assert.Equal((byte)3, (byte)Season.Winter);
    }

    [Fact]
    public void Season_GetValues_ReturnsAllValues()
    {
        var values = Enum.GetValues(typeof(Season));

        Assert.Equal(4, values.Length);
        var seasonArray = new Season[values.Length];
        values.CopyTo(seasonArray, 0);
        Assert.Contains(Season.Spring, seasonArray);
        Assert.Contains(Season.Summer, seasonArray);
        Assert.Contains(Season.Autumn, seasonArray);
        Assert.Contains(Season.Winter, seasonArray);
    }

    [Fact]
    public void Season_IterateWithEnumerator_WorksCorrectly()
    {
        var sb = new StringBuilder();
        var enums = Enum.GetValues(typeof(Season)).GetEnumerator();

        while (enums.MoveNext())
        {
            sb.Append(((byte)enums.Current).ToString());
            sb.Append(",");
        }

        Assert.Equal("0,1,2,3,", sb.ToString());
    }

    [Fact]
    public void Enum_GetDescription_ReturnsDescriptionAttribute()
    {
        var desc = PlugIn.JoinBox.GetDescription();
        Assert.Equal("惊惊", desc);
    }

    [Fact]
    public void Enum_GetDescription_NoDescription_ReturnsName()
    {
        var desc = PlugIn.DOCBAR.GetDescription();
        Assert.Equal("DOCBAR", desc);
    }
}
