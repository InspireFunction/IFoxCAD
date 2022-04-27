namespace IFoxCAD.Cad;

public static class PointEx
{
    public static string GetHashString(this Point3d pt, int xyz = 3, int decimalRetain = 6)
    {
        string hash;
        string de = "f" + decimalRetain.ToString();
        switch (xyz)
        {
            case 1:
                hash = pt.X.ToString(de);
                break;
            case 2:
                hash = pt.X.ToString(de) + "," + pt.Y.ToString(de);
                break;
            default:
                //hash = $"{pt.X:f6},{pt.Y:f6},{pt.Z:f6}";
                hash = pt.X.ToString(de) + "," + pt.Y.ToString(de) + "," + pt.Z.ToString(de);
                break;
        }
        return "(" + hash + ")";
    }
}