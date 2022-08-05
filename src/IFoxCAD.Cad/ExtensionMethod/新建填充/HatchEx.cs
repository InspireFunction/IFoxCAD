namespace IFoxCAD.Cad;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Acap = Autodesk.AutoCAD.ApplicationServices.Application;
using System;
using System.Collections.Generic;

public static class HatchEx
{
    /// <summary>
    /// 遍历填充每条边
    /// </summary>
    /// <param name="hatch"></param>
    /// <param name="action"></param>
    public static void ForEach(this Hatch hatch, Action<HatchLoop> action)
    {
        for (int i = 0; i < hatch.NumberOfLoops; i++)
            action.Invoke(hatch.GetLoopAt(i));
    }
}