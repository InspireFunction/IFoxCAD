#if acad
namespace IFoxCAD.Cad;

/* 此处暂未完成,无任何测试,尚且不知道怎么用 */
using System.Runtime.Remoting;

public class DxfFiler : Cad_DxfFiler
{
    public DxfFiler(IntPtr unmanagedPointer, [MarshalAs(UnmanagedType.U1)] bool autoDelete) : base(unmanagedPointer, autoDelete)
    {
    }

    public override bool IsModifyingExistingObject => base.IsModifyingExistingObject;

    public override double Thickness => base.Thickness;

    public override double Elevation => base.Elevation;

    public override bool AtEmbeddedObjectStart => base.AtEmbeddedObjectStart;

    public override bool AtEndOfObject => base.AtEndOfObject;

    public override bool AtExtendedData => base.AtExtendedData;

    public override bool AtEndOfFile => base.AtEndOfFile;

    public override int Precision { get => base.Precision; set => base.Precision = value; }

    public override string ErrorMessage => base.ErrorMessage;

    public override bool AtSubclassData(string value)
    {
        return base.AtSubclassData(value);
    }

    public override object Clone()
    {
        return base.Clone();
    }

    public override void CopyFrom(RXObject source)
    {
        base.CopyFrom(source);
    }

    public override ObjRef CreateObjRef(Type requestedType)
    {
        return base.CreateObjRef(requestedType);
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override void FilerStatus()
    {
        base.FilerStatus();
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override void HaltAtClassBoundaries(bool value)
    {
        base.HaltAtClassBoundaries(value);
    }

    public override object InitializeLifetimeService()
    {
        return base.InitializeLifetimeService();
    }

    public override void PushBackItem()
    {
        base.PushBackItem();
    }

    public override ResultBuffer ReadResultBuffer()
    {
        return base.ReadResultBuffer();
    }

    public override void ResetFilerStatus()
    {
        base.ResetFilerStatus();
    }

    public override int RewindFiler()
    {
        return base.RewindFiler();
    }

    public override void SetError(string format, params string[] values)
    {
        base.SetError(format, values);
    }

    public override void SetError(Cad_ErrorStatus value, string format, params string[] values)
    {
        base.SetError(value, format, values);
    }

    public override string ToString()
    {
        return base.ToString();
    }

    public override void WriteBool(DxfCode opCode, bool value)
    {
        base.WriteBool(opCode, value);
    }

    public override void WriteBoolean(DxfCode opCode, bool value)
    {
        base.WriteBoolean(opCode, value);
    }

    public override void WriteByte(DxfCode opCode, byte value)
    {
        base.WriteByte(opCode, value);
    }

    public override void WriteBytes(DxfCode opCode, byte[] chunk)
    {
        base.WriteBytes(opCode, chunk);
    }

    public override void WriteDouble(DxfCode opCode, double value, int precision)
    {
        base.WriteDouble(opCode, value, precision);
    }

    public override void WriteEmbeddedObjectStart()
    {
        base.WriteEmbeddedObjectStart();
    }

    public override void WriteHandle(DxfCode opCode, Handle value)
    {
        base.WriteHandle(opCode, value);
    }

    public override void WriteInt16(DxfCode opCode, short value)
    {
        base.WriteInt16(opCode, value);
    }

    public override void WriteInt32(DxfCode opCode, int value)
    {
        base.WriteInt32(opCode, value);
    }

    public override void WriteObjectId(DxfCode opCode, ObjectId value)
    {
        base.WriteObjectId(opCode, value);
    }

    public override void WritePoint2d(DxfCode opCode, Point2d value, int precision)
    {
        base.WritePoint2d(opCode, value, precision);
    }

    public override void WritePoint3d(DxfCode opCode, Point3d value, int precision)
    {
        base.WritePoint3d(opCode, value, precision);
    }

    public override void WriteResultBuffer(ResultBuffer buffer)
    {
        base.WriteResultBuffer(buffer);
    }

    public override void WriteScale3d(DxfCode opCode, Scale3d value, int precision)
    {
        base.WriteScale3d(opCode, value, precision);
    }

    public override void WriteString(DxfCode opCode, string value)
    {
        base.WriteString(opCode, value);
    }

    public override void WriteUInt16(DxfCode opCode, ushort value)
    {
        base.WriteUInt16(opCode, value);
    }

    public override void WriteUInt32(DxfCode opCode, uint value)
    {
        base.WriteUInt32(opCode, value);
    }

    public override void WriteVector2d(DxfCode opCode, Vector2d value, int precision)
    {
        base.WriteVector2d(opCode, value, precision);
    }

    public override void WriteVector3d(DxfCode opCode, Vector3d value, int precision)
    {
        base.WriteVector3d(opCode, value, precision);
    }

    public override void WriteXDataStart()
    {
        base.WriteXDataStart();
    }

    protected override void DeleteUnmanagedObject()
    {
        base.DeleteUnmanagedObject();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}
#endif