namespace IFoxCAD.Cad;
/// <summary>
/// Ϊdwg�ļ���ӱ��
/// </summary>
public static class DwgMark
{

    private const int FREESPACE = 0x15;
    private const int FREESPACEDEFAULT = 0x00;
    /// <summary>
    /// Ϊdwg�ļ���ӱ�ʶ
    /// </summary>
    /// <param name="file">DWG�ļ�</param>
    /// <param name="bite">ASCII��ʶ�ֽ�0X00~0X7F</param>
    /// <exception cref="ArgumentException">��dwg�ļ��ᱨ������bite������Ҳ����</exception>
    public static void AddMark(FileInfo file, int bite)
    {
        if (file.Extension.ToLower() != ".dwg")
        {
            throw new ArgumentException("������dwg�ļ���");
        }
        if (bite > 0x7F || bite < 0x00)
        {
            throw new ArgumentException("�ַ�������ASCII��Χ��");
        }
        using BinaryWriter bw = new BinaryWriter(File.Open(file.FullName, FileMode.Open));
        bw.BaseStream.Position = FREESPACE;//�ļ�ͷ��21���ֽ�
        bw.Write(bite); //д�����ݣ���һ���ֽ�
    }
    /// <summary>
    /// ��dwg�ļ���ǻָ�ΪĬ��ֵ
    /// </summary>
    /// <param name="file">�ļ�</param>
    /// <exception cref="ArgumentException">��dwg�ļ��ᱨ��</exception>
    public static void RemoveMark(FileInfo file)
    {
        if (file.Extension.ToLower() != ".dwg")
        {
            throw new ArgumentException("������dwg�ļ���");
        }
        using BinaryWriter bw = new BinaryWriter(File.Open(file.FullName, FileMode.Open));
        bw.BaseStream.Position = FREESPACE;//�ļ�ͷ��21���ֽ�
        bw.Write(FREESPACEDEFAULT); //д�����ݣ���һ���ֽ�
    }
    /// <summary>
    /// ��ȡ���õ�dwg�ļ����
    /// </summary>
    /// <param name="file">�ļ�</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">��dwg�ļ��ᱨ��</exception>
    public static int GetMark(FileInfo file)
    {
        if (file.Extension.ToLower() != ".dwg")
        {
            throw new ArgumentException("������dwg�ļ���");
        }
        using FileStream fs = File.OpenRead(file.FullName);
        fs.Seek(FREESPACE, SeekOrigin.Begin);
        byte[] mark = new byte[1];
        fs.Read(mark, 0, mark.Length);
        return mark[0];
    }
}
