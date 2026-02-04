#if NET35 || NET40
using System.Threading;

/// <summary>
/// 低版本补充 Volatile 类的功能
/// </summary>
public static class Volatile
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="value"></param>
    public static void Write(ref int location, int value)
    {
        Thread.VolatileWrite(ref location, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="value"></param>
    public static void Write(ref long location, long value)
    {
        Thread.VolatileWrite(ref location, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="value"></param>
    public static void Write(ref float location, float value)
    {
        Thread.VolatileWrite(ref location, value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="value"></param>
    public static void Write(ref double location, double value)
    {
        Thread.VolatileWrite(ref location, value);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="value"></param>
    public static void Write(ref bool location, bool value)
    {
        int temp = value ? 1 : 0;
        unsafe
        {
            fixed (bool* p = &location)
            {
                int* pInt = (int*)p;
                Thread.VolatileWrite(ref *pInt, temp);
            }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public static int Read(ref int location)
    {
        return Thread.VolatileRead(ref location);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public static long Read(ref long location)
    {
        return Thread.VolatileRead(ref location);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public static float Read(ref float location)
    {
        return Thread.VolatileRead(ref location);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public static double Read(ref double location)
    {
        return Thread.VolatileRead(ref location);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public static bool Read(ref bool location)
    {
        unsafe
        {
            fixed (bool* p = &location)
            {
                int* pInt = (int*)p;
                return Thread.VolatileRead(ref *pInt) != 0;
            }
        }
    }
}
#endif