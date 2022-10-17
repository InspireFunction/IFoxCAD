using System.Web.Script.Serialization;

namespace IFoxCAD.Cad;
/*
  Arx自定义实体类,加 读函数（assertReadEnabled）和写函数（assertWriteEnabled）

  所有属性位置都不要改动位置,因为涉及序列化
  [Serializable]               设置类   可以序列化
  [Newtonsoft.Json.JsonIgnore] 设置成员 不可序列化
*/


[Serializable]
public class DwgFiler : Cad_DwgFiler
{
#if NET35
    public int m_Position;
#else
    public long m_Position;
#endif
    public FilerType m_FilerType;
    public Cad_ErrorStatus m_FilerStatus;
    public List<IntPtr> AddressList;
    public int AddressListPt = 0;
    public List<byte[]> BinaryChunkList;
    public int BinaryChunkListPt = 0;
    public List<bool> BooleanList;
    public int BooleanListPt = 0;
    public List<byte> ByteList;
    public int ByteListPt = 0;
    public List<byte[]> BytesList;
    public int BytesListPt = 0;
    public List<double> DoubleList;
    public int DoubleListPt = 0;
    public List<Handle> HandleList;
    public int HandleListPt = 0;
    [NonSerialized]
    [ScriptIgnore]
    public List<ObjectId> HardOwnershipIdList;
    public int HardOwnershipIdListPt = 0;
    [NonSerialized]
    [ScriptIgnore]
    public List<ObjectId> HardPointerIdList;
    public int HardPointerIdListPt = 0;
    public List<short> Int16List;
    public int Int16ListPt = 0;
    public List<int> Int32List;
    public int Int32ListPt = 0;
#if !NET35
    public List<long> Int64List;
    public int Int64ListPt = 0;
#endif
    public List<Point2d> Point2dList;
    public int Point2dListPt = 0;
    public List<Point3d> Point3dList;
    public int Point3dListPt = 0;
    public List<Scale3d> Scale3dList;
    public int Scale3dListPt = 0;
    [NonSerialized]
    [ScriptIgnore]
    public List<ObjectId> SoftOwnershipIdList;
    public int SoftOwnershipIdListPt = 0;
    [NonSerialized]
    [ScriptIgnore]
    public List<ObjectId> SoftPointerIdList;
    public int SoftPointerIdListPt = 0;
    public List<string> StringList;
    public int StringListPt = 0;
    public List<ushort> Uint16List;
    public int uint16ListPt = 0;
    public List<uint> Uint32List;
    public int uint32ListPt = 0;
#if !NET35
    public List<ulong> Uint64List;
    public int uint64ListPt = 0;
#endif
    public List<Vector2d> Vector2dList;
    public int Vector2dListPt = 0;
    public List<Vector3d> Vector3dList;
    public int Vector3dListPt = 0;

    public DwgFiler()
    {
        m_Position = 0;
        m_FilerType = FilerType.CopyFiler;
        m_FilerStatus = Cad_ErrorStatus.OK;
        AddressList = new();
        BinaryChunkList = new();
        BooleanList = new();
        ByteList = new();
        BytesList = new();
        DoubleList = new();
        HandleList = new();
        HardOwnershipIdList = new();
        HardPointerIdList = new();
        Int16List = new();
        Int32List = new();
#if !NET35
        Int64List = new();
#endif
        Point2dList = new();
        Point3dList = new();
        Scale3dList = new();
        SoftOwnershipIdList = new();
        SoftPointerIdList = new();
        StringList = new();
        Uint16List = new();
        Uint32List = new();
#if !NET35
        Uint64List = new();
#endif
        Vector2dList = new();
        Vector3dList = new();
    }

#if NET35
    public override int Position => m_Position;
#else
    public override long Position => m_Position;
#endif
    public override FilerType FilerType => m_FilerType;

    public override Cad_ErrorStatus FilerStatus
    {
        get { return m_FilerStatus; }
        set { m_FilerStatus = value; }
    }

    public override IntPtr ReadAddress()
    {
        if (AddressList.Count == 0)
            return new();
        return AddressList[AddressListPt++];
    }

    public override byte[]? ReadBinaryChunk()
    {
        if (BinaryChunkList.Count == 0)
            return null;
        return BinaryChunkList[BinaryChunkListPt++];
    }

    public override bool ReadBoolean()
    {
        if (BooleanList.Count == 0)
            return false;
        return BooleanList[BooleanListPt++];
    }

    public override byte ReadByte()
    {
        if (ByteList.Count == 0)
            return 0;
        return ByteList[ByteListPt++];
    }

    public override void ReadBytes(byte[] value)
    {
        if (ByteList.Count == 0)
            return;
        value = new byte[BytesList[BytesListPt].Length];
        BytesList[BytesListPt++].CopyTo(value, 0);
    }

    public override double ReadDouble()
    {
        if (DoubleList.Count == 0)
            return 0;
        return DoubleList[DoubleListPt++];
    }

    public override Handle ReadHandle()
    {
        if (HandleList.Count == 0)
            return new();
        return HandleList[HandleListPt++];
    }

    public override ObjectId ReadHardOwnershipId()
    {
        if (HardOwnershipIdList.Count == 0)
            return new();
        return HardOwnershipIdList[HardOwnershipIdListPt++];
    }

    public override ObjectId ReadHardPointerId()
    {
        if (HardPointerIdList.Count == 0)
            return new();
        return HardPointerIdList[HardPointerIdListPt++];
    }

    public override short ReadInt16()
    {
        if (Int16List.Count == 0)
            return 0;
        return Int16List[Int16ListPt++];
    }

    public override int ReadInt32()
    {
        if (Int32List.Count == 0)
            return 0;
        return Int32List[Int32ListPt++];
    }

#if !NET35
    public override long ReadInt64()
    {
        if (Int64List.Count == 0)
            return 0;
        return Int64List[Int64ListPt++];
    }
#endif

    public override Point2d ReadPoint2d()
    {
        if (Point2dList.Count == 0)
            return new();
        return Point2dList[Point2dListPt++];
    }

    public override Point3d ReadPoint3d()
    {
        if (Point3dList.Count == 0)
            return new();
        return Point3dList[Point3dListPt++];
    }

    public override Scale3d ReadScale3d()
    {
        if (Scale3dList.Count == 0)
            return new();
        return Scale3dList[Scale3dListPt++];
    }

    public override ObjectId ReadSoftOwnershipId()
    {
        if (SoftOwnershipIdList.Count == 0)
            return new();
        return SoftOwnershipIdList[SoftOwnershipIdListPt++];
    }

    public override ObjectId ReadSoftPointerId()
    {
        if (SoftPointerIdList.Count == 0)
            return new();
        return SoftPointerIdList[SoftPointerIdListPt++];
    }

    public override string? ReadString()
    {
        if (StringList.Count == 0)
            return null;
        return StringList[StringListPt++];
    }

    public override ushort ReadUInt16()
    {
        if (Uint16List.Count == 0)
            return 0;
        return Uint16List[uint16ListPt++];
    }

    public override uint ReadUInt32()
    {
        if (Uint32List.Count == 0)
            return 0;
        return Uint32List[uint32ListPt++];
    }

#if !NET35
    public override ulong ReadUInt64()
    {
        if (Uint64List.Count == 0)
            return 0;
        return Uint64List[uint64ListPt++];
    }
#endif

    public override Vector2d ReadVector2d()
    {
        if (Vector2dList.Count == 0)
            return new();
        return Vector2dList[Vector2dListPt++];
    }

    public override Vector3d ReadVector3d()
    {
        if (Vector3dList.Count == 0)
            return new();
        return Vector3dList[Vector3dListPt++];
    }

    public override void ResetFilerStatus()
    {
        AddressList.Clear();
        AddressListPt = 0;
        BinaryChunkList.Clear();
        BinaryChunkListPt = 0;
        BooleanList.Clear();
        BooleanListPt = 0;
        ByteList.Clear();
        ByteListPt = 0;
        BytesList.Clear();
        BytesListPt = 0;
        DoubleList.Clear();
        DoubleListPt = 0;
        HandleList.Clear();
        HandleListPt = 0;
        HardOwnershipIdList.Clear();
        HardOwnershipIdListPt = 0;
        HardPointerIdList.Clear();
        HardPointerIdListPt = 0;
        Int16List.Clear();
        Int16ListPt = 0;
        Int32List.Clear();
        Int32ListPt = 0;
#if !NET35
        Int64List.Clear();
        Int64ListPt = 0;
#endif
        Point2dList.Clear();
        Point2dListPt = 0;
        Point3dList.Clear();
        Point3dListPt = 0;
        Scale3dList.Clear();
        Scale3dListPt = 0;
        SoftOwnershipIdList.Clear();
        SoftOwnershipIdListPt = 0;
        SoftPointerIdList.Clear();
        SoftPointerIdListPt = 0;
        StringList.Clear();
        StringListPt = 0;
        Uint16List.Clear();
        uint16ListPt = 0;
        Uint32List.Clear();
        uint32ListPt = 0;
#if !NET35
        Uint64List.Clear();
        uint64ListPt = 0;
#endif
        Vector2dList.Clear();
        Vector2dListPt = 0;
        Vector3dList.Clear();
        Vector3dListPt = 0;

        m_FilerType = FilerType.CopyFiler;
    }

#if zcad // 中望官方的问题
    public override void Seek(int offset, int method)
    {
        var ed = Acap.DocumentManager.MdiActiveDocument.Editor;
        ed.WriteMessage(MethodInfo.GetCurrentMethod().Name + " = " + " \n ");
    }
    public override void Seek(long offset, int method)
    {
        Seek((int)offset, method);
    }
#endif

#if acad || gcad
    public override void Seek(
#if NET35
    int
#else
    long
#endif
    offset, int method)
    {
        var ed = Acap.DocumentManager.MdiActiveDocument.Editor;
        ed.WriteMessage(MethodInfo.GetCurrentMethod().Name + " = " + " \n ");
    }
#endif

    public override void WriteAddress(IntPtr value)
    {
        AddressList.Add(value);
    }

    public override void WriteBinaryChunk(byte[] chunk)
    {
        BinaryChunkList.Add(chunk);
    }

    public override void WriteBoolean(bool value)
    {
        BooleanList.Add(value);
    }

    public override void WriteByte(byte value)
    {
        ByteList.Add(value);
    }

    public override void WriteBytes(byte[] value)
    {
        BytesList.Add(value);
    }

    public override void WriteDouble(double value)
    {
        DoubleList.Add(value);
    }

    public override void WriteHandle(Handle handle)
    {
        HandleList.Add(handle);
    }

    public override void WriteHardOwnershipId(ObjectId value)
    {
        HardOwnershipIdList.Add(value);
    }

    public override void WriteHardPointerId(ObjectId value)
    {
        HardPointerIdList.Add(value);
    }

    public override void WriteInt16(short value)
    {
        Int16List.Add(value);
    }

    public override void WriteInt32(int value)
    {
        Int32List.Add(value);
    }

#if !NET35
    public override void WriteInt64(long value)
    {
        Int64List.Add(value);
    }
#endif
    public override void WritePoint2d(Point2d value)
    {
        Point2dList.Add(value);
    }

    public override void WritePoint3d(Point3d value)
    {
        Point3dList.Add(value);
    }

    public override void WriteScale3d(Scale3d value)
    {
        Scale3dList.Add(value);
    }

    public override void WriteSoftOwnershipId(ObjectId value)
    {
        SoftOwnershipIdList.Add(value);
    }

    public override void WriteSoftPointerId(ObjectId value)
    {
        SoftPointerIdList.Add(value);
    }

    public override void WriteString(string value)
    {
        StringList.Add(value);
    }

    public override void WriteUInt16(ushort value)
    {
        Uint16List.Add(value);
    }

    public override void WriteUInt32(uint value)
    {
        Uint32List.Add(value);
    }

#if !NET35
    public override void WriteUInt64(ulong value)
    {
        Uint64List.Add(value);
    }
#endif

    public override void WriteVector2d(Vector2d value)
    {
        Vector2dList.Add(value);
    }

    public override void WriteVector3d(Vector3d value)
    {
        Vector3dList.Add(value);
    }

    public override string ToString()
    {
        return new JavaScriptSerializer().Serialize(this);
    }
}