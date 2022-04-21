namespace IFoxCAD.Cad;

public static class PointEx
{
    public static string GetHashString(this Point3d pt)
    {
        return $"{pt.X:n6}{pt.Y:n6}{pt.Z:n6}";
    }
}