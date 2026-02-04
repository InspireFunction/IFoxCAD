using System;
using System.Collections.Generic;
using System.Text;

namespace IFoxCAD.Cad;

/// <summary>
/// 
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsNullOrWhiteSpace(this string? value)
    {
        if (value == null)
            return true;
        for (int i = 0; i < value.Length; i++)
        {
            if (!char.IsWhiteSpace(value[i]))
                return false;
        }
        return true;
    }
}
