namespace IFoxCAD.Event;

public class EventMethodInfo
{
    public EventMethodInfo(MethodInfo method, EventParameterType parameterType, int level)
    {
        Method = method;
        ParameterType = parameterType;
        Level = level;
    }

    public MethodInfo Method { get; }
    public EventParameterType ParameterType { get; }
    public int Level { get; }
}
public enum EventParameterType
{
    None = 0,
    Object = 1,
    EventArgs = 2,
    Complete = 3,
}