namespace BasalUnitTests;

public class ForEachExtensionTests
{
    [Fact]
    public void ForEach_SimpleAction_ExecutesForAllElements()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var sum = 0;

        list.ForEach(x => sum += x);

        Assert.Equal(15, sum);
    }

    [Fact]
    public void ForEach_WithIndex_ProvidesCorrectIndex()
    {
        var list = new List<string> { "a", "b", "c" };
        var indices = new List<int>();

        list.ForEach((index, item) => indices.Add(index));

        Assert.Equal(new List<int> { 0, 1, 2 }, indices);
    }

    [Fact]
    public void ForEach_WithCtrlState_Break_StopsIteration()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var result = new List<int>();

        list.ForEach((x, state) => {
            if (x == 3)
            {
                state.Break();
                return;
            }
            result.Add(x);
        });

        Assert.Equal(new List<int> { 1, 2 }, result);
    }

    [Fact]
    public void ForEach_WithCtrlStateAndIndex_Break_StopsIteration()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var result = new List<(int value, int index)>();

        list.ForEach((x, state, index) => {
            if (x == 3)
            {
                state.Break();
                return;
            }
            result.Add((x, index));
        });

        Assert.Equal(2, result.Count);
        Assert.Equal((1, 0), result[0]);
        Assert.Equal((2, 1), result[1]);
    }

    [Fact]
    public void ForEach_EmptyCollection_DoesNotExecute()
    {
        var list = new List<int>();
        var executed = false;

        list.ForEach(x => executed = true);

        Assert.False(executed);
    }

    [Fact]
    public void ForEach_CtrlState_InitialStateIsNone()
    {
        var state = new CtrlState();

        Assert.True(state.IsNone);
        Assert.False(state.IsRun);
        Assert.False(state.IsBreak);
    }

    [Fact]
    public void CtrlState_Start_SetsRunningState()
    {
        var state = new CtrlState();

        state.Start();

        Assert.True(state.IsRun);
        Assert.False(state.IsNone);
    }

    [Fact]
    public void CtrlState_Break_SetsBreakState()
    {
        var state = new CtrlState();

        state.Break();

        Assert.True(state.IsBreak);
    }

    [Fact]
    public void CtrlState_Clone_CreatesCopyWithSameState()
    {
        var original = new CtrlState();
        original.Start();

        var clone = original.Clone();

        Assert.True(clone.IsRun);
        Assert.Equal(original.State, clone.State);
    }

    [Fact]
    public void CtrlState_Exceptional_CanBeSetAndCleared()
    {
        var state = new CtrlState();

        state.Exceptional(true);
        Assert.True(state.IsExceptional);

        state.Exceptional(false);
        Assert.False(state.IsExceptional);
    }

    [Fact]
    public void CtrlState_Error_CanBeSetAndCleared()
    {
        var state = new CtrlState();

        state.Error(true);
        Assert.True(state.IsError);

        state.Error(false);
        Assert.False(state.IsError);
    }

    [Fact]
    public void ForEach_Array_WorksCorrectly()
    {
        var array = new int[] { 1, 2, 3, 4, 5 };
        var sum = 0;

        array.ForEach(x => sum += x);

        Assert.Equal(15, sum);
    }

    [Fact]
    public void ForEach_HashSet_WorksCorrectly()
    {
        var set = new HashSet<int> { 1, 2, 3 };
        var count = 0;

        set.ForEach(x => count++);

        Assert.Equal(3, count);
    }
}
