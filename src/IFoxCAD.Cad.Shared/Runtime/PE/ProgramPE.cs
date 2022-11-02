namespace IFoxCAD.Cad;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;


/*  来源 https://blog.csdn.net/zgke/article/details/2955560 我在他基础上面增加了X64的处理
 *  调用例子
    static void Main(string[] args)
    {
        var path = @"C:\Program Files\Autodesk\AutoCAD 2021\acad.exe";
        // path = @"G:\AutoCAD 2008\acad.exe";

        var pe = new JoinBox.BasalCurrency.PeInfo(path);

        // 输出所有的函数名
        var sb = new StringBuilder();
        foreach (var item in pe.ExportDirectory.NameList)
        {
            sb.Append(Environment.NewLine);
            var str = System.Text.Encoding.Default.GetString(item as byte[]);
            sb.Append(str);
        }
        Debugx.Printl(sb.ToString());

        // 原作者的封装
        var ss = pe.GetPETable();
        foreach (var item in ss.Tables)
        {
        }
    }
*/

/// <summary>
/// 微软软件结构PE信息
/// </summary>
public class PeInfo
{
    #region 成员
    /// <summary>
    /// 获取是否正常打开文件
    /// </summary>
    public bool OpenFile { get; private set; } = false;
    public DosHeader? DosHeader { get; private set; }
    public DosStub? DosStub { get; private set; }
    public PEHeader? PEHeader { get; private set; }
    public OptionalHeader? OptionalHeader { get; private set; }
    public OptionalDirAttrib? OptionalDirAttrib { get; private set; }
    public SectionTable? SectionTable { get; private set; }
    /// <summary>
    /// 函数接口名单
    /// </summary>
    public ExportDirectory? ExportDirectory { get; private set; }
    public ImportDirectory? ImportDirectory { get; private set; }
    public ResourceDirectory? ResourceDirectory { get; private set; }
    /// <summary>
    /// PE文件完整路径
    /// </summary>
    public string? FullName;

    bool _IsX86 = true;

    /// <summary>
    /// 全部文件数据
    /// </summary>
    readonly byte[]? _PEFileByte;
    /// <summary>
    /// 文件读取的位置
    /// </summary>
    long _PEFileIndex = 0;
    #endregion

    #region 构造
    public PeInfo(string fullName)
    {
        if (fullName is null)
            throw new ArgumentException(nameof(fullName)); ;

        FullName = fullName;
        FileStream? file = null;
        OpenFile = false;
        try
        {
            // 文件流
            file = new FileStream(fullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);// FileShare才能进c盘
            _PEFileByte = new byte[file.Length];
            file.Read(_PEFileByte, 0, _PEFileByte.Length);
            LoadFile();
            OpenFile = true;
        }
        catch (Exception) { throw; }
        finally
        {
            file?.Close();
        }
    }
    #endregion

    #region 读表方法
    /// <summary>
    /// 开始读取
    /// </summary>
    private void LoadFile()
    {
        LoadDosHeader();         // 获取DOS头,为了兼容DOS所以首先处理这个,然后才到PE头
        LoadDosStub();           // 获取DOS的身体
        LoadPEHeader();          // PE头
        LoadOptionalHeader();    // PE头扩展
        LoadOptionalDirAttrib(); // 获取选项目录属性
        LoadSectionTable();      // 获取节表
        LoadExportDirectory();   // 获取输出表
        LoadImportDirectory();   // 获取输入表
        LoadResourceDirectory(); // 获取资源目录
    }

    /// <summary>
    /// 获得DOS头
    /// </summary>
    private void LoadDosHeader()
    {
        DosHeader = new DosHeader
        {
            FileStarIndex = _PEFileIndex
        };
        Loadbyte(ref DosHeader.e_magic);
        Loadbyte(ref DosHeader.e_cblp);
        Loadbyte(ref DosHeader.e_cp);
        Loadbyte(ref DosHeader.e_crlc);
        Loadbyte(ref DosHeader.e_cparhdr);
        Loadbyte(ref DosHeader.e_minalloc);
        Loadbyte(ref DosHeader.e_maxalloc);
        Loadbyte(ref DosHeader.e_ss);
        Loadbyte(ref DosHeader.e_sp);
        Loadbyte(ref DosHeader.e_csum);
        Loadbyte(ref DosHeader.e_ip);
        Loadbyte(ref DosHeader.e_cs);
        Loadbyte(ref DosHeader.e_rva);
        Loadbyte(ref DosHeader.e_fg);
        Loadbyte(ref DosHeader.e_bl1);
        Loadbyte(ref DosHeader.e_oemid);
        Loadbyte(ref DosHeader.e_oeminfo);
        Loadbyte(ref DosHeader.e_bl2);
        Loadbyte(ref DosHeader.e_PESTAR);

        DosHeader.FileEndIndex = _PEFileIndex;
    }

    /// <summary>
    /// 获得DOS SUB字段
    /// </summary>
    private void LoadDosStub()
    {
        if (DosHeader is null)
            return;

        long Size = GetLong(DosHeader.e_PESTAR) - _PEFileIndex;   // 获得SUB的大小
        DosStub = new DosStub(Size)
        {
            FileStarIndex = _PEFileIndex
        };
        Loadbyte(ref DosStub.DosStubData);
        DosStub.FileEndIndex = _PEFileIndex;
    }

    /// <summary>
    /// 获得PE的文件头
    /// </summary>
    /// <returns></returns>
    private void LoadPEHeader()
    {
        PEHeader = new PEHeader
        {
            FileStarIndex = _PEFileIndex
        };
        Loadbyte(ref PEHeader.Header);
        Loadbyte(ref PEHeader.Machine);// [76 1]==[0x4C 1]是x32,
        _IsX86 = PEHeader.Machine[0] == 0x4c && PEHeader.Machine[1] == 0x1;
        // (PEHeader.Machine[0] == 0x64 && PEHeader.Machine[1] == 0x86)// x64
        Loadbyte(ref PEHeader.NumberOfSections);
        Loadbyte(ref PEHeader.TimeDateStamp);
        Loadbyte(ref PEHeader.PointerToSymbolTable);
        Loadbyte(ref PEHeader.NumberOfSymbols);
        Loadbyte(ref PEHeader.SizeOfOptionalHeader);
        Loadbyte(ref PEHeader.Characteristics);
        PEHeader.FileEndIndex = _PEFileIndex;
    }

    /// <summary>
    /// 获得OPTIONAL PE扩展属性
    /// </summary>
    /// <returns></returns>
    private void LoadOptionalHeader()
    {
        // 这里必须通过PE文件来判断它是一个什么架构,从而使用x86-x64
        OptionalHeader = new OptionalHeader(_IsX86)
        {
            FileStarIndex = _PEFileIndex
        };
        Loadbyte(ref OptionalHeader.Magic);
        Loadbyte(ref OptionalHeader.MajorLinkerVersion);
        Loadbyte(ref OptionalHeader.MinorLinkerVersion);
        Loadbyte(ref OptionalHeader.SizeOfCode);
        Loadbyte(ref OptionalHeader.SizeOfInitializedData);
        Loadbyte(ref OptionalHeader.SizeOfUninitializedData);
        Loadbyte(ref OptionalHeader.AddressOfEntryPoint);
        Loadbyte(ref OptionalHeader.BaseOfCode);
        Loadbyte(ref OptionalHeader.ImageBase);
        Loadbyte(ref OptionalHeader.BaseOfData);
        Loadbyte(ref OptionalHeader.SectionAlignment);
        Loadbyte(ref OptionalHeader.FileAlignment);

        Loadbyte(ref OptionalHeader.MajorOperatingSystemVersion);
        Loadbyte(ref OptionalHeader.MinorOperatingSystemVersion);
        Loadbyte(ref OptionalHeader.MajorImageVersion);
        Loadbyte(ref OptionalHeader.MinorImageVersion);
        Loadbyte(ref OptionalHeader.MajorSubsystemVersion);
        Loadbyte(ref OptionalHeader.MinorSubsystemVersion);
        Loadbyte(ref OptionalHeader.Win32VersionValue);
        Loadbyte(ref OptionalHeader.SizeOfImage);
        Loadbyte(ref OptionalHeader.SizeOfHeards);
        Loadbyte(ref OptionalHeader.CheckSum);
        Loadbyte(ref OptionalHeader.Subsystem);
        Loadbyte(ref OptionalHeader.DLLCharacteristics);
        Loadbyte(ref OptionalHeader.SizeOfStackReserve);
        Loadbyte(ref OptionalHeader.SizeOfStackCommit);
        Loadbyte(ref OptionalHeader.SizeOfHeapReserve);
        Loadbyte(ref OptionalHeader.SizeOfHeapCommit);
        Loadbyte(ref OptionalHeader.LoaderFlags);
        Loadbyte(ref OptionalHeader.NumberOfRvaAndSizes);

        OptionalHeader.FileEndIndex = _PEFileIndex;
    }

    /// <summary>
    /// 获取目录表
    /// </summary>
    /// <returns></returns>
    private void LoadOptionalDirAttrib()
    {
        if (OptionalHeader is null)
            return;

        OptionalDirAttrib = new OptionalDirAttrib
        {
            FileStarIndex = _PEFileIndex
        };

        long DirCount = GetLong(OptionalHeader.NumberOfRvaAndSizes);// 这里导致无法使用64位
        for (int i = 0; i != DirCount; i++)
        {
            OptionalDirAttrib.DirAttrib? directAttrib = new();
            Loadbyte(ref directAttrib.DirRva);
            Loadbyte(ref directAttrib.DirSize);
            OptionalDirAttrib.DirByte.Add(directAttrib);
        }
        OptionalDirAttrib.FileEndIndex = _PEFileIndex;
    }

    /// <summary>
    /// 获取节表
    /// </summary>
    private void LoadSectionTable()
    {
        if (PEHeader is null)
            return;

        SectionTable = new SectionTable();
        long Count = GetLong(PEHeader.NumberOfSections);
        SectionTable.FileStarIndex = _PEFileIndex;
        for (long i = 0; i != Count; i++)
        {
            var Section = new SectionTable.SectionData();

            Loadbyte(ref Section.SectName);
            Loadbyte(ref Section.VirtualAddress);
            Loadbyte(ref Section.SizeOfRawDataRVA);
            Loadbyte(ref Section.SizeOfRawDataSize);
            Loadbyte(ref Section.PointerToRawData);
            Loadbyte(ref Section.PointerToRelocations);
            Loadbyte(ref Section.PointerToLinenumbers);
            Loadbyte(ref Section.NumberOfRelocations);
            Loadbyte(ref Section.NumberOfLinenumbers);
            Loadbyte(ref Section.Characteristics);
            SectionTable.Section.Add(Section);
        }
        SectionTable.FileEndIndex = _PEFileIndex;
    }

    /// <summary>
    /// 读取输出表
    /// </summary>
    private void LoadExportDirectory()
    {
        if (OptionalDirAttrib is null)
            return;
        if (OptionalDirAttrib.DirByte.Count == 0)
            return;

        if (OptionalDirAttrib.DirByte[0] is not OptionalDirAttrib.DirAttrib exporRVA ||
            GetLong(exporRVA.DirRva) == 0)
            return;

        long exporAddress = GetLong(exporRVA.DirRva);  // 获取的位置
        ExportDirectory = new ExportDirectory();

        if (SectionTable is null)
            return;

        for (int i = 0; i != SectionTable.Section.Count; i++) // 循环节表
        {
            if (SectionTable.Section[i] is not SectionTable.SectionData sect)
                continue;

            long starRva = GetLong(sect.SizeOfRawDataRVA);
            long endRva = GetLong(sect.SizeOfRawDataSize);

            if (exporAddress >= starRva && exporAddress < starRva + endRva)
            {
                _PEFileIndex = exporAddress - GetLong(sect.SizeOfRawDataRVA) + GetLong(sect.PointerToRawData);

                ExportDirectory.FileStarIndex = _PEFileIndex;
                ExportDirectory.FileEndIndex = _PEFileIndex + GetLong(exporRVA.DirSize);

                Loadbyte(ref ExportDirectory.Characteristics);
                Loadbyte(ref ExportDirectory.TimeDateStamp);
                Loadbyte(ref ExportDirectory.MajorVersion);
                Loadbyte(ref ExportDirectory.MinorVersion);
                Loadbyte(ref ExportDirectory.Name);
                Loadbyte(ref ExportDirectory.Base);
                Loadbyte(ref ExportDirectory.NumberOfFunctions);
                Loadbyte(ref ExportDirectory.NumberOfNames);
                Loadbyte(ref ExportDirectory.AddressOfFunctions);
                Loadbyte(ref ExportDirectory.AddressOfNames);
                Loadbyte(ref ExportDirectory.AddressOfNameOrdinals);

                _PEFileIndex = GetLong(ExportDirectory.AddressOfFunctions) - GetLong(sect.SizeOfRawDataRVA) + GetLong(sect.PointerToRawData);
                long endIndex = GetLong(ExportDirectory.AddressOfNames) - GetLong(sect.SizeOfRawDataRVA) + GetLong(sect.PointerToRawData);
                long numb = (endIndex - _PEFileIndex) / 4;
                for (long z = 0; z != numb; z++)
                {
                    byte[] Data = new byte[4];
                    Loadbyte(ref Data);
                    ExportDirectory.AddressOfFunctionsList.Add(Data);
                }

                _PEFileIndex = endIndex;
                endIndex = GetLong(ExportDirectory.AddressOfNameOrdinals) - GetLong(sect.SizeOfRawDataRVA) + GetLong(sect.PointerToRawData);
                numb = (endIndex - _PEFileIndex) / 4;
                for (long z = 0; z != numb; z++)
                {
                    byte[] Data = new byte[4];
                    Loadbyte(ref Data);
                    ExportDirectory.AddressOfNamesList.Add(Data);
                }

                _PEFileIndex = endIndex;
                endIndex = GetLong(ExportDirectory.Name) - GetLong(sect.SizeOfRawDataRVA) + GetLong(sect.PointerToRawData);
                numb = (endIndex - _PEFileIndex) / 2;
                for (long z = 0; z != numb; z++)
                {
                    byte[] Data = new byte[2];
                    Loadbyte(ref Data);
                    ExportDirectory.AddressOfNameOrdinalsList.Add(Data);
                }

                _PEFileIndex = endIndex;

                if (_PEFileByte is not null)
                {
                    long ReadIndex = 0;
                    while (true)
                    {
                        if (_PEFileByte[_PEFileIndex + ReadIndex] == 0)
                        {
                            if (_PEFileByte[_PEFileIndex + ReadIndex + 1] == 0)
                                break;

                            var Date = new byte[ReadIndex];
                            Loadbyte(ref Date);
                            ExportDirectory.FunctionNamesByte.Add(Date);

                            _PEFileIndex++;
                            ReadIndex = 0;
                        }
                        ReadIndex++;
                    }
                }
                break;
            }
        }
    }

    /// <summary>
    /// 读取输入表
    /// </summary>
    private void LoadImportDirectory()
    {
        if (OptionalDirAttrib is null)
            return;
        if (OptionalDirAttrib.DirByte.Count < 1)
            return;
        if (OptionalDirAttrib.DirByte[1] is not OptionalDirAttrib.DirAttrib ImporRVA)
            return;

        long ImporAddress = GetLong(ImporRVA.DirRva);  // 获取的位置
        if (ImporAddress == 0)
            return;
        long ImporSize = GetLong(ImporRVA.DirSize);  // 获取大小

        ImportDirectory = new ImportDirectory();

        long SizeRva = 0;
        long PointerRva = 0;

        long StarRva = 0;
        long EndRva = 0;

        #region 获取位置
        if (SectionTable is null)
            return;
        for (int i = 0; i != SectionTable.Section.Count; i++) // 循环节表
        {
            if (SectionTable.Section[i] is not SectionTable.SectionData Sect)
                continue;

            StarRva = GetLong(Sect.SizeOfRawDataRVA);
            EndRva = GetLong(Sect.SizeOfRawDataSize);

            if (ImporAddress >= StarRva && ImporAddress < StarRva + EndRva)
            {
                SizeRva = GetLong(Sect.SizeOfRawDataRVA);
                PointerRva = GetLong(Sect.PointerToRawData);
                _PEFileIndex = ImporAddress - SizeRva + PointerRva;

                ImportDirectory.FileStarIndex = _PEFileIndex;
                ImportDirectory.FileEndIndex = _PEFileIndex + ImporSize;
                break;
            }
        }

        if (SizeRva == 0 && PointerRva == 0)
            return;
        #endregion

        #region 输入表结构
        while (true)
        {
            var import = new ImportDirectory.ImportDate();
            Loadbyte(ref import.OriginalFirstThunk);
            Loadbyte(ref import.TimeDateStamp);
            Loadbyte(ref import.ForwarderChain);
            Loadbyte(ref import.Name);
            Loadbyte(ref import.FirstThunk);

            if (GetLong(import.Name) == 0)
                break;
            ImportDirectory.ImportList.Add(import); // 添加
        }
        #endregion


        #region 获取输入DLL名称
        for (int z = 0; z != ImportDirectory.ImportList.Count; z++)     // 获取引入DLL名字
        {
            if (ImportDirectory.ImportList[z] is not ImportDirectory.ImportDate Import)
                continue;

            long ImportDLLName = GetLong(Import.Name) - SizeRva + PointerRva;
            _PEFileIndex = ImportDLLName;
            long ReadCount = 0;
            while (_PEFileByte is not null) // 获取引入名
            {
                if (_PEFileByte[_PEFileIndex + ReadCount] == 0)
                {
                    Import.DLLName = new byte[ReadCount];
                    Loadbyte(ref Import.DLLName);
                    break;
                }
                ReadCount++;
            }
        }
        #endregion

        #region 获取引入方法 先获取地址 然后获取名字和头
        for (int z = 0; z != ImportDirectory.ImportList.Count; z++)     // 获取引入方法
        {
            if (ImportDirectory.ImportList[z] is not ImportDirectory.ImportDate import)
                continue;

            long importDLLName = GetLong(import.OriginalFirstThunk) - SizeRva + PointerRva;
            _PEFileIndex = importDLLName;
            while (true)
            {
                var function = new ImportDirectory.ImportDate.FunctionList();
                Loadbyte(ref function.OriginalFirst);

                long loadIndex = GetLong(function.OriginalFirst);
                if (loadIndex == 0)
                    break;
                long oldIndex = _PEFileIndex;

                _PEFileIndex = loadIndex - SizeRva + PointerRva;

                if (loadIndex >= StarRva && loadIndex < StarRva + EndRva)  // 发现有些数字超级大
                {
                    int ReadCount = 0;

                    while (_PEFileByte is not null)
                    {
                        if (ReadCount == 0)
                            Loadbyte(ref function.FunctionHead);
                        if (_PEFileByte[_PEFileIndex + ReadCount] == 0)
                        {
                            byte[] FunctionName = new byte[ReadCount];
                            Loadbyte(ref FunctionName);
                            function.FunctionName = FunctionName;

                            break;
                        }
                        ReadCount++;
                    }
                }
                else
                {
                    function.FunctionName = new byte[1];
                }

                _PEFileIndex = oldIndex;
                import.DLLFunctionList.Add(function);
            }
        }
        #endregion
    }

    /// <summary>
    /// 读取资源表
    /// </summary>
    private void LoadResourceDirectory()
    {
        #region 初始化
        if (OptionalDirAttrib is null || OptionalDirAttrib.DirByte.Count < 3)
            return;
        if (OptionalDirAttrib.DirByte[2] is not OptionalDirAttrib.DirAttrib ImporRVA)
            return;

        long ImporAddress = GetLong(ImporRVA.DirRva);  // 获取的位置
        if (ImporAddress == 0)
            return;
        long ImporSize = GetLong(ImporRVA.DirSize);  // 获取大小

        ResourceDirectory = new ResourceDirectory();

        long SizeRva = 0;
        long PointerRva = 0;
        long StarRva = 0;
        long PEIndex = 0;
        #endregion

        #region 获取位置
        if (SectionTable is null)
            return;

        for (int i = 0; i != SectionTable.Section.Count; i++) // 循环节表
        {
            if (SectionTable.Section[i] is not SectionTable.SectionData sect)
                continue;

            StarRva = GetLong(sect.SizeOfRawDataRVA);
            var EndRva = GetLong(sect.SizeOfRawDataSize);
            if (ImporAddress >= StarRva && ImporAddress < StarRva + EndRva)
            {
                SizeRva = GetLong(sect.SizeOfRawDataRVA);
                PointerRva = GetLong(sect.PointerToRawData);
                _PEFileIndex = ImporAddress - SizeRva + PointerRva;
                PEIndex = _PEFileIndex;
                ResourceDirectory.FileStarIndex = _PEFileIndex;
                ResourceDirectory.FileEndIndex = _PEFileIndex + ImporSize;
                break;
            }
        }

        if (SizeRva == 0 && PointerRva == 0)
            return;
        #endregion

        AddResourceNode(ResourceDirectory, PEIndex, 0, StarRva);
    }

    /// <summary>
    /// 添加资源节点
    /// </summary>
    /// <param name="node"></param>
    /// <param name="PEIndex"></param>
    /// <param name="RVA"></param>
    /// <param name="resourSectRva"></param>
    private void AddResourceNode(ResourceDirectory node, long PEIndex, long RVA, long resourSectRva)
    {
        _PEFileIndex = PEIndex + RVA;  // 设置位置
        Loadbyte(ref node.Characteristics);
        Loadbyte(ref node.TimeDateStamp);
        Loadbyte(ref node.MajorVersion);
        Loadbyte(ref node.MinorVersion);
        Loadbyte(ref node.NumberOfNamedEntries);
        Loadbyte(ref node.NumberOfIdEntries);

        long NameRVA = GetLong(node.NumberOfNamedEntries);
        for (int i = 0; i != NameRVA; i++)
        {
            var Entry = new ResourceDirectory.DirectoryEntry();
            Loadbyte(ref Entry.Name);
            Loadbyte(ref Entry.Id);
            byte[] temp = new byte[2];
            temp[0] = Entry.Name[0];
            temp[1] = Entry.Name[1];

            if (_PEFileByte is null)
                return;

            long NameIndex = GetLong(temp) + PEIndex;
            temp[0] = _PEFileByte[NameIndex + 0];
            temp[1] = _PEFileByte[NameIndex + 1];

            long NameCount = GetLong(temp);
            node.Name = new byte[NameCount * 2];

            for (int z = 0; z != node.Name.Length; z++)
                node.Name[z] = _PEFileByte[NameIndex + 2 + z];
            // System.Windows.Forms.MessageBox.Show(GetString(Entry.ID));

            temp[0] = Entry.Id[2];
            temp[1] = Entry.Id[3];

            long oldIndex = _PEFileIndex;

            if (GetLong(temp) == 0)
            {
                temp[0] = Entry.Id[0];
                temp[1] = Entry.Id[1];

                _PEFileIndex = GetLong(temp) + PEIndex;

                var dataRVA = new ResourceDirectory.DirectoryEntry.DataEntry();

                Loadbyte(ref dataRVA.ResourRVA);
                Loadbyte(ref dataRVA.ResourSize);
                Loadbyte(ref dataRVA.ResourTest);
                Loadbyte(ref dataRVA.ResourWen);

                _PEFileIndex = oldIndex;
                Entry.DataEntryList.Add(dataRVA);
                // System.Windows.Forms.MessageBox.Show(GetString(DataRVA.ResourRVA)+"*"+GetString(DataRVA.ResourSize));
            }
            else
            {
                temp[0] = Entry.Id[0];
                temp[1] = Entry.Id[1];

                var Resource = new ResourceDirectory();
                Entry.NodeDirectoryList.Add(Resource);
                AddResourceNode(Resource, PEIndex, GetLong(temp), resourSectRva);
            }
            _PEFileIndex = oldIndex;
            node.EntryList.Add(Entry);
        }

        long Count = GetLong(node.NumberOfIdEntries);
        for (int i = 0; i != Count; i++)
        {
            var entry = new ResourceDirectory.DirectoryEntry();
            Loadbyte(ref entry.Name);
            Loadbyte(ref entry.Id);
            // System.Windows.Forms.MessageBox.Show(GetString(Entry.Name)+"_"+GetString(Entry.Id));

            byte[] temp = new byte[2];
            temp[0] = entry.Id[2];
            temp[1] = entry.Id[3];

            long OldIndex = _PEFileIndex;

            if (GetLong(temp) == 0)
            {
                temp[0] = entry.Id[0];
                temp[1] = entry.Id[1];

                _PEFileIndex = GetLong(temp) + PEIndex;

                var dataRVA = new ResourceDirectory.DirectoryEntry.DataEntry();
                Loadbyte(ref dataRVA.ResourRVA);
                Loadbyte(ref dataRVA.ResourSize);
                Loadbyte(ref dataRVA.ResourTest);
                Loadbyte(ref dataRVA.ResourWen);

                long FileRva = GetLong(dataRVA.ResourRVA) - resourSectRva + PEIndex;

                dataRVA.FileStarIndex = FileRva;
                dataRVA.FileEndIndex = FileRva + GetLong(dataRVA.ResourSize);

                _PEFileIndex = OldIndex;
                entry.DataEntryList.Add(dataRVA);
                // System.Windows.Forms.MessageBox.Show(GetString(DataRVA.ResourRVA)+"*"+GetString(DataRVA.ResourSize));
            }
            else
            {
                temp[0] = entry.Id[0];
                temp[1] = entry.Id[1];
                var Resource = new ResourceDirectory();
                entry.NodeDirectoryList.Add(Resource);
                AddResourceNode(Resource, PEIndex, GetLong(temp), resourSectRva);
            }
            _PEFileIndex = OldIndex;
            node.EntryList.Add(entry);
        }
    }

    #endregion

    #region 工具方法
    /// <summary>
    /// 读数据 读byte[]的数量 会改边PEFileIndex的值
    /// </summary>
    /// <param name="data"></param>
    private void Loadbyte(ref byte[] data)
    {
        if (_PEFileByte is null)
            return;

        for (int i = 0; i != data.Length; i++)
        {
            data[i] = _PEFileByte[_PEFileIndex];
            _PEFileIndex++;
        }
    }
    /// <summary>
    /// 转换byte为字符串
    /// </summary>
    /// <param name="data">byte[]</param>
    /// <returns>AA BB CC DD</returns>
    private string GetString(byte[] data)
    {
        string Temp = "";
        for (int i = 0; i != data.Length - 1; i++)
            Temp += data[i].ToString("X02") + " ";

        Temp += data[data.Length - 1].ToString("X02");
        // Temp += data[^1].ToString("X02");
        return Temp;
    }
    /// <summary>
    /// 转换字符为显示数据
    /// </summary>
    /// <param name="data">byte[]</param>
    /// <param name="type">ASCII DEFAULT UNICODE BYTE</param>
    /// <returns></returns>
    private string GetString(byte[] data, string type)
    {
        if (type.Trim().ToUpper() == "ASCII")
            return System.Text.Encoding.ASCII.GetString(data);
        if (type.Trim().ToUpper() == "DEFAULT")
            return System.Text.Encoding.Default.GetString(data);
        if (type.Trim().ToUpper() == "UNICODE")
            return System.Text.Encoding.Unicode.GetString(data);
        if (type.Trim().ToUpper() == "BYTE")
        {
            string Temp = "";
            for (int i = data.Length - 1; i != 0; i--)
                Temp += data[i].ToString("X02") + " ";
            Temp += data[0].ToString("X02");
            return Temp;
        }
        return GetInt(data);
    }
    /// <summary>
    /// 转换BYTE为INT
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    static string GetInt(byte[] data)
    {
        string Temp = "";
        for (int i = 0; i != data.Length - 1; i++)
        {
            int ByteInt = (int)data[i];
            Temp += ByteInt.ToString() + " ";
        }
        int EndByteInt = (int)data[data.Length - 1];
        // int EndByteInt = (int)data[^1];
        Temp += EndByteInt.ToString();
        return Temp;
    }
    /// <summary>
    /// 转换数据为LONG
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private long GetLong(byte[] data)
    {
        if (data.Length > 4)
            return 0;

        string MC = "";
        // if (data.Length <= 4)
        for (int i = data.Length - 1; i != -1; i--)
            MC += data[i].ToString("X02");
        return Convert.ToInt64(MC, 16);
    }
    /// <summary>
    /// 添加一行信息
    /// </summary>
    /// <param name="refTable">表</param>
    /// <param name="data">数据</param>
    /// <param name="name">名称</param>
    /// <param name="describe">说明</param>
    private void AddTableRow(DataTable refTable, byte[]? data, string name, string describe)
    {
        if (data == null)
            throw new ArgumentException(nameof(data));

        refTable.Rows.Add(
            new string[]
            {
             name,
             data.Length.ToString(),
             GetString(data),
             GetLong(data).ToString(),
             GetString(data,"ASCII"),
             describe
             });
    }
    #endregion

    #region Table绘制
    /// <summary>
    /// 获取PE信息 DataSet方式
    /// </summary>
    /// <returns>多个表 最后资源表 绘制成树结构TABLE </returns>
    public DataSet? GetPETable()
    {
        if (OpenFile == false)
            return null;

        var ds = new DataSet("PEFile");
        var a1 = TableDosHeader();
        if (a1 is not null)
            ds.Tables.Add(a1);
        var a2 = TablePEHeader();
        if (a2 is not null)
            ds.Tables.Add(a2);
        var a3 = TableOptionalHeader();
        if (a3 is not null)
            ds.Tables.Add(a3);
        var a4 = TableOptionalDirAttrib();
        if (a4 is not null)
            ds.Tables.Add(a4);
        var a5 = TableSectionData();
        if (a5 is not null)
            ds.Tables.Add(a5);


        var a6 = TableExportDirectory();
        var a7 = TableExportFunction();
        if (a6 is not null)
            ds.Tables.Add(a6);
        if (a7 is not null)
            ds.Tables.Add(a7);

        var a8 = TableImportDirectory();
        var a9 = TableImportFunction();
        if (a8 is not null)
            ds.Tables.Add(a8);
        if (a9 is not null)
            ds.Tables.Add(a9);

        var a10 = TableResourceDirectory();
        if (a10 is not null)
            ds.Tables.Add(a10);

        return ds;
    }

    private DataTable? TableDosHeader()
    {
        if (DosHeader is null)
            return null;

        var returnTable = new DataTable("DosHeader FileStar{" + DosHeader.FileStarIndex.ToString() + "}FileEnd{" + DosHeader.FileEndIndex.ToString() + "}");
        returnTable.Columns.Add("Name");
        returnTable.Columns.Add("Size");
        returnTable.Columns.Add("Value16");
        returnTable.Columns.Add("Value10");
        returnTable.Columns.Add("ASCII");
        returnTable.Columns.Add("Describe");

        AddTableRow(returnTable, DosHeader.e_magic, "e_magic", "魔术数字");
        AddTableRow(returnTable, DosHeader.e_cblp, "e_cblp", "文件最后页的字节数");
        AddTableRow(returnTable, DosHeader.e_cp, "e_cp", "文件页数");
        AddTableRow(returnTable, DosHeader.e_crlc, "e_crlc", "重定义元素个数");
        AddTableRow(returnTable, DosHeader.e_cparhdr, "e_cparhdr", "头部尺寸,以段落为单位");
        AddTableRow(returnTable, DosHeader.e_minalloc, "e_minalloc", "所需的最小附加段");
        AddTableRow(returnTable, DosHeader.e_maxalloc, "e_maxalloc", "所需的最大附加段");
        AddTableRow(returnTable, DosHeader.e_ss, "e_ss", "初始的SS值(相对偏移量)");
        AddTableRow(returnTable, DosHeader.e_sp, "e_sp", "初始的SP值");
        AddTableRow(returnTable, DosHeader.e_csum, "e_csum", "校验和");
        AddTableRow(returnTable, DosHeader.e_ip, "e_ip", "初始的IP值");
        AddTableRow(returnTable, DosHeader.e_cs, "e_cs", "初始的CS值(相对偏移量)");
        AddTableRow(returnTable, DosHeader.e_rva, "e_rva", "");
        AddTableRow(returnTable, DosHeader.e_fg, "e_fg", "");
        AddTableRow(returnTable, DosHeader.e_bl1, "e_bl1", "");
        AddTableRow(returnTable, DosHeader.e_oemid, "e_oemid", "");
        AddTableRow(returnTable, DosHeader.e_oeminfo, "e_oeminfo", "");
        AddTableRow(returnTable, DosHeader.e_bl2, "e_bl2", "");
        AddTableRow(returnTable, DosHeader.e_PESTAR, "e_PESTAR", "PE开始 +本结构的位置");

        return returnTable;
    }

    private DataTable? TablePEHeader()
    {
        if (PEHeader is null)
            return null;

        var returnTable = new DataTable("PeHeader FileStar{" + PEHeader.FileStarIndex.ToString() + "}FileEnd{" + PEHeader.FileEndIndex.ToString() + "}");
        returnTable.Columns.Add("Name");
        returnTable.Columns.Add("Size");
        returnTable.Columns.Add("Value16");
        returnTable.Columns.Add("Value10");
        returnTable.Columns.Add("ASCII");
        returnTable.Columns.Add("Describe");


        AddTableRow(returnTable, PEHeader.Header, "Header", "PE文件标记");
        AddTableRow(returnTable, PEHeader.Machine, "Machine", "该文件运行所要求的CPU.对于Intel平台,该值是IMAGE_FILE_MACHINE_I386 (14Ch).我们尝试了LUEVELSMEYER的pe.txt声明的14Dh和14Eh,但Windows不能正确执行. ");
        AddTableRow(returnTable, PEHeader.NumberOfSections, "NumberOfSections", "文件的节数目.如果我们要在文件中增加或删除一个节,就需要修改这个值.");
        AddTableRow(returnTable, PEHeader.TimeDateStamp, "TimeDateStamp", "文件创建日期和时间. ");
        AddTableRow(returnTable, PEHeader.PointerToSymbolTable, "PointerToSymbolTable", "用于调试. ");
        AddTableRow(returnTable, PEHeader.NumberOfSymbols, "NumberOfSymbols", "用于调试. ");
        AddTableRow(returnTable, PEHeader.SizeOfOptionalHeader, "SizeOfOptionalHeader", "指示紧随本结构之后的 OptionalHeader 结构大小,必须为有效值.");
        AddTableRow(returnTable, PEHeader.Characteristics, "Characteristics", "关于文件信息的标记,比如文件是exe还是dll.");

        return returnTable;
    }

    private DataTable? TableOptionalHeader()
    {
        if (OptionalHeader is null)
            return null;

        var returnTable = new DataTable("OptionalHeader FileStar{" + OptionalHeader.FileStarIndex.ToString() + "}FileEnd{" + OptionalHeader.FileEndIndex.ToString() + "}");
        returnTable.Columns.Add("Name");
        returnTable.Columns.Add("Size");
        returnTable.Columns.Add("Value16");
        returnTable.Columns.Add("Value10");
        returnTable.Columns.Add("ASCII");
        returnTable.Columns.Add("Describe");

        AddTableRow(returnTable, OptionalHeader.Magic, "Magic", "Magic 010B=普通可以执行,0107=ROM映像");
        AddTableRow(returnTable, OptionalHeader.MajorLinkerVersion, "MajorLinkerVersion", "主版本号");
        AddTableRow(returnTable, OptionalHeader.MinorLinkerVersion, "MinorLinkerVersion", "副版本号");
        AddTableRow(returnTable, OptionalHeader.SizeOfCode, "SizeOfCode", "代码段大小");
        AddTableRow(returnTable, OptionalHeader.SizeOfInitializedData, "SizeOfInitializedData", "已初始化数据大小");
        AddTableRow(returnTable, OptionalHeader.SizeOfUninitializedData, "SizeOfUninitializedData", "未初始化数据大小");
        AddTableRow(returnTable, OptionalHeader.AddressOfEntryPoint, "AddressOfEntryPoint", "执行将从这里开始(RVA)");
        AddTableRow(returnTable, OptionalHeader.BaseOfCode, "BaseOfCode", "代码基址(RVA)");
        AddTableRow(returnTable, OptionalHeader.ImageBase, "ImageBase", "数据基址(RVA)");
        if (_IsX86)
        {
            AddTableRow(returnTable, OptionalHeader.BaseOfData, "ImageFileCode", "映象文件基址");
        }
        AddTableRow(returnTable, OptionalHeader.SectionAlignment, "SectionAlign", "区段列队");
        AddTableRow(returnTable, OptionalHeader.MajorOperatingSystemVersion, "MajorOSV", "文件列队");
        AddTableRow(returnTable, OptionalHeader.MinorOperatingSystemVersion, "MinorOSV", "操作系统主版本号");
        AddTableRow(returnTable, OptionalHeader.MajorImageVersion, "MajorImageVer", "映象文件主版本号");
        AddTableRow(returnTable, OptionalHeader.MinorImageVersion, "MinorImageVer", "映象文件副版本号");
        AddTableRow(returnTable, OptionalHeader.MajorSubsystemVersion, "MajorSV", "子操作系统主版本号");
        AddTableRow(returnTable, OptionalHeader.MinorSubsystemVersion, "MinorSV", "子操作系统副版本号");
        AddTableRow(returnTable, OptionalHeader.Win32VersionValue, "UNKNOW", "Win32版本值");
        AddTableRow(returnTable, OptionalHeader.SizeOfImage, "SizeOfImage", "映象文件大小");
        AddTableRow(returnTable, OptionalHeader.SizeOfHeards, "SizeOfHeards", "标志头大小");
        AddTableRow(returnTable, OptionalHeader.CheckSum, "CheckSum", "文件效验");
        AddTableRow(returnTable, OptionalHeader.Subsystem, "Subsystem", "子系统(映象文件)1本地 2WINDOWS-GUI 3WINDOWS-CUI 4 POSIX-CUI");
        AddTableRow(returnTable, OptionalHeader.DLLCharacteristics, "DLL_Characteristics", "DLL标记");
        AddTableRow(returnTable, OptionalHeader.SizeOfStackReserve, "Bsize", "保留栈的大小");
        AddTableRow(returnTable, OptionalHeader.SizeOfStackCommit, "TimeBsize", "初始时指定栈大小");
        AddTableRow(returnTable, OptionalHeader.SizeOfHeapReserve, "AucBsize", "保留堆的大小");
        AddTableRow(returnTable, OptionalHeader.SizeOfHeapCommit, "SizeOfBsize", "初始时指定堆大小");
        AddTableRow(returnTable, OptionalHeader.LoaderFlags, "FuckBsize", "加载器标志");
        AddTableRow(returnTable, OptionalHeader.NumberOfRvaAndSizes, "DirectCount", "数据目录数");

        return returnTable;
    }

    private DataTable? TableOptionalDirAttrib()
    {
        if (OptionalDirAttrib is null)
            return null;

        var returnTable = new DataTable(
            "OptionalDirAttrib  FileStar{"
            + OptionalDirAttrib.FileStarIndex.ToString()
            + "}FileEnd{"
            + OptionalDirAttrib.FileEndIndex.ToString()
            + "}");
        returnTable.Columns.Add("Name");
        returnTable.Columns.Add("Size");
        returnTable.Columns.Add("Value16");
        returnTable.Columns.Add("Value10");
        returnTable.Columns.Add("ASCII");
        returnTable.Columns.Add("Describe");

        var tableName = new Hashtable
        {
            { 0, "输出表" },
            { 1, "输入表" },
            { 2, "资源表" },
            { 3, "异常表" },
            { 4, "安全表" },
            { 5, "基部重定位表" },
            { 6, "调试数据" },
            { 7, "版权数据" },
            { 8, "全局PTR" },
            { 9, "TLS表" },
            { 10, "装入配置表" },
            { 11, "其他表1" },
            { 12, "其他表2" },
            { 13, "其他表3" },
            { 14, "其他表4" },
            { 15, "其他表5" }
        };

        for (int i = 0; i != OptionalDirAttrib.DirByte.Count; i++)
        {
            if (OptionalDirAttrib.DirByte[i] is not OptionalDirAttrib.DirAttrib MyDirByte)
                continue;

            string? Name;
            var tn = tableName[i];
            if (tn is not null)
                Name = tn.ToString();
            else
                Name = $"未知表{i}";
            AddTableRow(returnTable, MyDirByte.DirRva, Name!, "地址");
            AddTableRow(returnTable, MyDirByte.DirSize, "", "大小");
        }
        return returnTable;
    }

    private DataTable? TableSectionData()
    {
        if (SectionTable is null)
            return null;

        var returnTable = new DataTable(
            "SectionData FileStar{"
            + SectionTable.FileStarIndex.ToString()
            + "}FileEnd{"
            + SectionTable.FileEndIndex.ToString()
            + "}");
        returnTable.Columns.Add("Name");
        returnTable.Columns.Add("Size");
        returnTable.Columns.Add("Value16");
        returnTable.Columns.Add("Value10");
        returnTable.Columns.Add("ASCII");
        returnTable.Columns.Add("Describe");

        for (int i = 0; i != SectionTable.Section.Count; i++)
        {
            if (SectionTable.Section[i] is not SectionTable.SectionData SectionDate)
                continue;

            AddTableRow(returnTable, SectionDate.SectName, "SectName", "名字");
            AddTableRow(returnTable, SectionDate.VirtualAddress, "VirtualAddress", "虚拟内存地址");
            AddTableRow(returnTable, SectionDate.SizeOfRawDataRVA, "SizeOfRawDataRVA", "RVA偏移");
            AddTableRow(returnTable, SectionDate.SizeOfRawDataSize, "SizeOfRawDataSize", "RVA大小");
            AddTableRow(returnTable, SectionDate.PointerToRawData, "PointerToRawData", "指向RAW数据");
            AddTableRow(returnTable, SectionDate.PointerToRelocations, "PointerToRelocations", "指向定位号");
            AddTableRow(returnTable, SectionDate.PointerToLinenumbers, "PointerToLinenumbers", "指向行数");
            AddTableRow(returnTable, SectionDate.NumberOfRelocations, "NumberOfRelocations", "定位号");
            AddTableRow(returnTable, SectionDate.NumberOfLinenumbers, "NumberOfLinenumbers", "行数号");
            AddTableRow(returnTable, SectionDate.Characteristics, "Characteristics", "区段标记");
        }
        return returnTable;
    }

    private DataTable? TableExportDirectory()
    {
        if (ExportDirectory is null)
            return null;

        var returnTable = new DataTable(
            "ExportDirectory FileStar{"
            + ExportDirectory.FileStarIndex.ToString()
            + "}FileEnd{"
            + ExportDirectory.FileEndIndex.ToString()
            + "}");
        returnTable.Columns.Add("Name");
        returnTable.Columns.Add("Size");
        returnTable.Columns.Add("Value16");
        returnTable.Columns.Add("Value10");
        returnTable.Columns.Add("ASCII");
        returnTable.Columns.Add("Describe");

        AddTableRow(returnTable, ExportDirectory.Characteristics, "Characteristics", "一个保留字段,目前为止值为0.");
        AddTableRow(returnTable, ExportDirectory.TimeDateStamp, "TimeDateStamp", "产生的时间.");
        AddTableRow(returnTable, ExportDirectory.MajorVersion, "MajorVersion", "主版本号");
        AddTableRow(returnTable, ExportDirectory.MinorVersion, "MinorVersion", "副版本号");
        AddTableRow(returnTable, ExportDirectory.Name, "Name", "一个RVA,指向一个dll的名称的ascii字符串.");
        AddTableRow(returnTable, ExportDirectory.Base, "Base", "输出函数的起始序号.一般为1.");
        AddTableRow(returnTable, ExportDirectory.NumberOfFunctions, "NumberOfFunctions", "输出函数入口地址的数组 中的元素个数.");
        AddTableRow(returnTable, ExportDirectory.NumberOfNames, "NumberOfNames", "输出函数名的指针的数组 中的元素个数,也是输出函数名对应的序号的数组 中的元素个数.");
        AddTableRow(returnTable, ExportDirectory.AddressOfFunctions, "AddressOfFunctions", "一个RVA,指向输出函数入口地址的数组.");
        AddTableRow(returnTable, ExportDirectory.AddressOfNames, "AddressOfNames", "一个RVA,指向输出函数名的指针的数组.");
        AddTableRow(returnTable, ExportDirectory.AddressOfNameOrdinals, "AddressOfNameOrdinals", "一个RVA,指向输出函数名对应的序号的数组.");

        return returnTable;
    }
    private DataTable? TableExportFunction()
    {
        if (ExportDirectory is null)
            return null;

        var returnTable = new DataTable("ExportFunctionList");
        returnTable.Columns.Add("Name");
        returnTable.Columns.Add("Size");
        returnTable.Columns.Add("Value16");
        returnTable.Columns.Add("Value10");
        returnTable.Columns.Add("ASCII");
        returnTable.Columns.Add("Describe");

        for (int i = 0; i != ExportDirectory.FunctionNamesByte.Count; i++)
        {
            AddTableRow(returnTable,
                ExportDirectory.FunctionNamesByte[i],
                "Name",
                "_ExportDirectory.Name-Sect.SizeOfRawDataRVA+Sect.PointerToRawData");
        }

        for (int i = 0; i != ExportDirectory.AddressOfNamesList.Count; i++)
        {
            if (ExportDirectory.AddressOfNamesList[i] is not byte[] a)
                continue;
            AddTableRow(returnTable, a, "NamesList", "");
        }

        for (int i = 0; i != ExportDirectory.AddressOfFunctionsList.Count; i++)
        {
            if (ExportDirectory.AddressOfFunctionsList[i] is not byte[] a)
                continue;
            AddTableRow(returnTable, a, "Functions", "");
        }

        for (int i = 0; i != ExportDirectory.AddressOfNameOrdinalsList.Count; i++)
        {
            if (ExportDirectory.AddressOfNameOrdinalsList[i] is not byte[] a)
                continue;
            AddTableRow(returnTable, a, "NameOrdinals", "");
        }
        return returnTable;
    }
    private DataTable? TableImportDirectory()
    {
        if (ImportDirectory is null)
            return null;

        var returnTable = new DataTable("ImportDirectory FileStar{" + ImportDirectory.FileStarIndex.ToString() + "}FileEnd{" + ImportDirectory.FileEndIndex.ToString() + "}");
        returnTable.Columns.Add("Name");
        returnTable.Columns.Add("Size");
        returnTable.Columns.Add("Value16");
        returnTable.Columns.Add("Value10");
        returnTable.Columns.Add("ASCII");
        returnTable.Columns.Add("Describe");

        for (int i = 0; i != ImportDirectory.ImportList.Count; i++)
        {
            if (ImportDirectory.ImportList[i] is not ImportDirectory.ImportDate ImportByte)
                continue;

            AddTableRow(returnTable, ImportByte.DLLName, "输入DLL名称", "**********");
            AddTableRow(returnTable, ImportByte.OriginalFirstThunk, "OriginalFirstThunk", "这里实际上保存着一个RVA,这个RVA指向一个DWORD数组,这个数组可以叫做输入查询表.每个数组元素,或者叫一个表项,保存着一个指向函数名的RVA或者保存着一个函数的序号.");
            AddTableRow(returnTable, ImportByte.TimeDateStamp, "TimeDateStamp", "当这个值为0的时候,表明还没有bind.不为0的话,表示已经bind过了.有关bind的内容后面介绍.");
            AddTableRow(returnTable, ImportByte.ForwarderChain, "ForwarderChain", "");
            AddTableRow(returnTable, ImportByte.Name, "Name", "一个RVA,这个RVA指向一个ascii以空字符结束的字符串,这个字符串就是本结构对应的dll文件的名字.");
            AddTableRow(returnTable, ImportByte.FirstThunk, "FirstThunk", "一个RVA,这个RVA指向一个DWORD数组,这个数组可以叫输入地址表.如果bind了的话,这个数组的每个元素,就是一个输入函数的入口地址.");
        }

        return returnTable;
    }
    private DataTable? TableImportFunction()
    {
        if (ImportDirectory is null)
            return null;

        var returnTable = new DataTable("ImportFunctionList");
        returnTable.Columns.Add("Name");
        returnTable.Columns.Add("Size");
        returnTable.Columns.Add("Value16");
        returnTable.Columns.Add("Value10");
        returnTable.Columns.Add("ASCII");
        returnTable.Columns.Add("Describe");

        for (int i = 0; i != ImportDirectory.ImportList.Count; i++)
        {
            if (ImportDirectory.ImportList[i] is not ImportDirectory.ImportDate ImportByte)
                continue;

            AddTableRow(returnTable, ImportByte.DLLName, "DLL-Name", "**********");

            for (int z = 0; z != ImportByte.DLLFunctionList.Count; z++)
            {
                if (ImportByte.DLLFunctionList[z] is not ImportDirectory.ImportDate.FunctionList Function)
                    continue;

                AddTableRow(returnTable, Function.FunctionName, "FunctionName", "");
                AddTableRow(returnTable, Function.FunctionHead, "FunctionHead", "");
                AddTableRow(returnTable, Function.OriginalFirst, "OriginalFirstThunk", "");
            }
        }
        return returnTable;
    }
    private DataTable? TableResourceDirectory()
    {
        if (ResourceDirectory is null)
            return null;
        var returnTable = new DataTable("ResourceDirectory FileStar{" + ResourceDirectory.FileStarIndex.ToString() + "}FileEnd{" + ResourceDirectory.FileEndIndex.ToString() + "}");
        returnTable.Columns.Add("GUID");
        returnTable.Columns.Add("Text");
        returnTable.Columns.Add("ParentID");
        AddResourceDirectoryRow(returnTable, ResourceDirectory, "");
        return returnTable;
    }
    private void AddResourceDirectoryRow(DataTable myTable, ResourceDirectory Node, string parentID)
    {
        string Name = "";
        if (Node.Name is not null)
            Name = GetString(Node.Name, "UNICODE");

        for (int i = 0; i != Node.EntryList.Count; i++)
        {
            if (Node.EntryList[i] is not ResourceDirectory.DirectoryEntry Entry)
                continue;

            long ID = GetLong(Entry.Name);

            string GUID = Guid.NewGuid().ToString();

            string IDNAME = "ID{" + ID + "}";
            if (Name.Length != 0)
                IDNAME += "Name{" + Name + "}";

            if (parentID.Length == 0)
            {
                IDNAME += ID switch
                {
                    1 => "Type{Cursor}",
                    2 => "Type{Bitmap}",
                    3 => "Type{Icon}",
                    4 => "Type{Cursor}",
                    5 => "Type{Menu}",
                    6 => "Type{Dialog}",
                    7 => "Type{String Table}",
                    8 => "Type{Font Directory}",
                    9 => "Type{Font}",
                    10 => "Type{Accelerators}",
                    11 => "Type{Unformatted}",
                    12 => "Type{Message Table}",
                    13 => "Type{Group Cursor}",
                    14 => "Type{Group Icon}",
                    15 => "Type{Information}",
                    16 => "Type{Version}",
                    _ => "Type{未定义}",
                };
            }

            myTable.Rows.Add(new string[] { GUID, IDNAME, parentID });

            for (int z = 0; z != Entry.DataEntryList.Count; z++)
            {
                if (Entry.DataEntryList[z] is not ResourceDirectory.DirectoryEntry.DataEntry Data)
                    continue;

                string Text = "Address{" + GetString(Data.ResourRVA) + "} Size{" + GetString(Data.ResourSize) + "} FileBegin{" + Data.FileStarIndex.ToString() + "-" + Data.FileEndIndex.ToString() + "}";

                myTable.Rows.Add(new string[] { Guid.NewGuid().ToString(), Text, GUID });
            }

            for (int z = 0; z != Entry.NodeDirectoryList.Count; z++)
            {
                if (Entry.NodeDirectoryList[z] is not ResourceDirectory a)
                    continue;
                AddResourceDirectoryRow(myTable, a, GUID);
            }
        }
    }
    #endregion
}

#region 类
/// <summary>
/// DOS文件都MS开始
/// </summary>
public class DosHeader // IMAGE_DOS_HEADER
{
    public byte[] e_magic = new byte[2];       // 魔术数字
    public byte[] e_cblp = new byte[2];       // 文件最后页的字节数
    public byte[] e_cp = new byte[2];       // 文件页数
    public byte[] e_crlc = new byte[2];       // 重定义元素个数
    public byte[] e_cparhdr = new byte[2];       // 头部尺寸,以段落为单位
    public byte[] e_minalloc = new byte[2];       // 所需的最小附加段
    public byte[] e_maxalloc = new byte[2];       // 所需的最大附加段
    public byte[] e_ss = new byte[2];       // 初始的SS值(相对偏移量)
    public byte[] e_sp = new byte[2];       // 初始的SP值
    public byte[] e_csum = new byte[2];       // 校验和
    public byte[] e_ip = new byte[2];       // 初始的IP值
    public byte[] e_cs = new byte[2];       // 初始的CS值(相对偏移量)
    public byte[] e_rva = new byte[2];
    public byte[] e_fg = new byte[2];
    public byte[] e_bl1 = new byte[8];
    public byte[] e_oemid = new byte[2];
    public byte[] e_oeminfo = new byte[2];
    public byte[] e_bl2 = new byte[20];
    public byte[] e_PESTAR = new byte[2];   // PE开始 +自己的位置........重点

    public long FileStarIndex = 0;
    public long FileEndIndex = 0;
}

/// <summary>
/// DOS程序 提示
/// </summary>
public class DosStub
{
    public byte[] DosStubData;
    public DosStub(long Size)
    {
        DosStubData = new byte[Size];
    }
    public long FileStarIndex = 0;
    public long FileEndIndex = 0;
}

/// <summary>
/// PE文件头
/// </summary>
public class PEHeader // IMAGE_FILE_HEADER
{
    public byte[] Header = new byte[4];// PE文件标记
    public byte[] Machine = new byte[2];// 该文件运行所要求的CPU.对于Intel平台,该值是IMAGE_FILE_MACHINE_I386 (14Ch).我们尝试了LUEVELSMEYER的pe.txt声明的14Dh和14Eh,但Windows不能正确执行.看起来,除了禁止程序执行之外,本域对我们来说用处不大.
    public byte[] NumberOfSections = new byte[2];// 文件的节数目.如果我们要在文件中增加或删除一个节,就需要修改这个值.
    public byte[] TimeDateStamp = new byte[4];// 文件创建日期和时间.我们不感兴趣.
    public byte[] PointerToSymbolTable = new byte[4];// 用于调试.
    public byte[] NumberOfSymbols = new byte[4];// 用于调试.
    public byte[] SizeOfOptionalHeader = new byte[2];// 指示紧随本结构之后的 OptionalHeader 结构大小,必须为有效值. IMAGE_OPTIONAL_HEADER32 结构大小
    public byte[] Characteristics = new byte[2];// 关于文件信息的标记,比如文件是exe还是dll.

    public long FileStarIndex = 0;
    public long FileEndIndex = 0;
}

/// <summary>
/// PE头扩展
/// </summary>
// https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-image_optional_header32 // IMAGE_OPTIONAL_HEADER32
// https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-image_optional_header64 // IMAGE_OPTIONAL_HEADER64
public class OptionalHeader
{
    public byte[] Magic = new byte[2];                   // Magic 010B=普通可以执行,0107=ROM映像
    public byte[] MajorLinkerVersion = new byte[1];      // 主版本号
    public byte[] MinorLinkerVersion = new byte[1];      // 副版本号
    public byte[] SizeOfCode = new byte[4];              // 代码段大小
    public byte[] SizeOfInitializedData = new byte[4];   // 已初始化数据大小
    public byte[] SizeOfUninitializedData = new byte[4]; // 未初始化数据大小
    public byte[] AddressOfEntryPoint = new byte[4];     // 执行将从这里开始(RVA).........入口点指向,dll填充0
    public byte[] BaseOfCode = new byte[4];              // 代码基址(RVA)

    public byte[] BaseOfData = new byte[4];                   // 映象文件基址

    public byte[] ImageBase = new byte[4];                    // 数据基址(RVA)

    public byte[] SectionAlignment = new byte[4];             // 区段列队.....
    public byte[] FileAlignment = new byte[4];                // 文件列队

    public byte[] MajorOperatingSystemVersion = new byte[2];  // 操作系统主版本号
    public byte[] MinorOperatingSystemVersion = new byte[2];  // 操作系统副版本号
    public byte[] MajorImageVersion = new byte[2];            // 映象文件主版本号
    public byte[] MinorImageVersion = new byte[2];            // 映象文件副版本号
    public byte[] MajorSubsystemVersion = new byte[2];        // 子操作系统主版本号
    public byte[] MinorSubsystemVersion = new byte[2];        // 子操作系统副版本号
    public byte[] Win32VersionValue = new byte[4];            // Win32版本值
    public byte[] SizeOfImage = new byte[4];                  // 映象文件大小
    public byte[] SizeOfHeards = new byte[4];                 // 标志头大小
    public byte[] CheckSum = new byte[4];                     // 文件效验
    public byte[] Subsystem = new byte[2];                    // 子系统(映象文件)1本地 2WINDOWS-GUI 3WINDOWS-CUI 4 POSIX-CUI
    public byte[] DLLCharacteristics = new byte[2];           // DLL标记

    public byte[] SizeOfStackReserve = new byte[4];           // 保留栈的大小
    public byte[] SizeOfStackCommit = new byte[4];            // 初始时指定栈大小
    public byte[] SizeOfHeapReserve = new byte[4];            // 保留堆的大小
    public byte[] SizeOfHeapCommit = new byte[4];             // 初始时指定堆大小
    public byte[] LoaderFlags = new byte[4];                  // 加载器标志
    public byte[] NumberOfRvaAndSizes = new byte[4];          // 数据目录数

    public long FileStarIndex = 0;
    public long FileEndIndex = 0;

    public OptionalHeader(bool is32)
    {
        if (!is32)
        {
            // X64没有了,但是为了代码保留修改幅度不大,所以置0
            BaseOfData = new byte[0];// x64必须置于0
            // x64长度增加的
            int ulonglong = 8;
            ImageBase = new byte[ulonglong];          // 数据基址(RVA)
            SizeOfStackReserve = new byte[ulonglong]; // 保留栈的大小
            SizeOfStackCommit = new byte[ulonglong];  // 初始时指定栈大小
            SizeOfHeapReserve = new byte[ulonglong];  // 保留堆的大小
            SizeOfHeapCommit = new byte[ulonglong];   // 初始时指定堆大小
        }
    }
}

/// <summary>
/// 目录结构
/// </summary>
public class OptionalDirAttrib
{
    public ArrayList DirByte = new();
    public class DirAttrib
    {
        public byte[] DirRva = new byte[4];   // 地址
        public byte[] DirSize = new byte[4];   // 大小
    }
    public long FileStarIndex = 0;
    public long FileEndIndex = 0;
}

/// <summary>
/// 节表
/// </summary>
public class SectionTable
{
    public ArrayList Section = new();
    public class SectionData
    {
        public byte[] SectName = new byte[8];             // 名字
        public byte[] VirtualAddress = new byte[4];             // 虚拟内存地址
        public byte[] SizeOfRawDataRVA = new byte[4];             // RVA偏移
        public byte[] SizeOfRawDataSize = new byte[4];             // RVA大小
        public byte[] PointerToRawData = new byte[4];             // 指向RAW数据
        public byte[] PointerToRelocations = new byte[4];             // 指向定位号
        public byte[] PointerToLinenumbers = new byte[4];             // 指向行数
        public byte[] NumberOfRelocations = new byte[2];             // 定位号
        public byte[] NumberOfLinenumbers = new byte[2];             // 行数号
        public byte[] Characteristics = new byte[4];             // 区段标记
    }

    public long FileStarIndex = 0;
    public long FileEndIndex = 0;
}

/// <summary>
/// 输出表
/// </summary>
public class ExportDirectory
{
    public byte[] Characteristics = new byte[4];       // 一个保留字段,目前为止值为0.
    public byte[] TimeDateStamp = new byte[4];         // 产生的时间
    public byte[] MajorVersion = new byte[2];          // 主版本号
    public byte[] MinorVersion = new byte[2];          // 副版本号
    public byte[] Name = new byte[4];                  // 一个RVA,指向一个dll的名称的ascii字符串
    public byte[] Base = new byte[4];                  // 输出函数的起始序号.一般为1
    public byte[] NumberOfFunctions = new byte[4];     // 输出函数入口地址的数组中的元素个数
    public byte[] NumberOfNames = new byte[4];         // 输出函数名的指针的数组中的元素个数,也是输出函数名对应的序号的数组中的元素个数
    public byte[] AddressOfFunctions = new byte[4];    // 一个RVA,指向输出函数入口地址的数组
    public byte[] AddressOfNames = new byte[4];        // 一个RVA,指向输出函数名的指针的数组
    public byte[] AddressOfNameOrdinals = new byte[4]; // 一个RVA,指向输出函数名对应的序号的数组

    public ArrayList AddressOfFunctionsList = new();
    public ArrayList AddressOfNamesList = new();
    public ArrayList AddressOfNameOrdinalsList = new();
    /// <summary>
    /// 函数指针名称集合
    /// </summary>
    public List<byte[]> FunctionNamesByte = new();
    public long FileStarIndex = 0;
    public long FileEndIndex = 0;

    /// <summary>
    /// 获取函数名
    /// </summary>
    public HashSet<string> FunctionNames()
    {
        HashSet<string> names = new();
        for (int i = 0; i < FunctionNamesByte.Count; i++)
            names.Add(Encoding.Default.GetString(FunctionNamesByte[i]));
        return names;
    }
}


/// <summary>
/// 输入表
/// </summary>
public class ImportDirectory
{
    public ArrayList ImportList = new();

    public class ImportDate
    {
        public byte[] OriginalFirstThunk = new byte[4]; // 这里实际上保存着一个RVA,这个RVA指向一个DWORD数组,这个数组可以叫做输入查询表.每个数组元素,或者叫一个表项,保存着一个指向函数名的RVA或者保存着一个函数的序号.
        public byte[] TimeDateStamp = new byte[4];      // 当这个值为0的时候,表明还没有bind.不为0的话,表示已经bind过了.有关bind的内容后面介绍.
        public byte[] ForwarderChain = new byte[4];
        public byte[] Name = new byte[4];       // 一个RVA,这个RVA指向一个ascii以空字符结束的字符串,这个字符串就是本结构对应的dll文件的名字.
        public byte[] FirstThunk = new byte[4]; // 一个RVA,这个RVA指向一个DWORD数组,这个数组可以叫输入地址表.如果bind了的话,这个数组的每个元素,就是一个输入函数的入口地址.

        public byte[]? DLLName;  // DLL名称
        public ArrayList DLLFunctionList = new();
        public class FunctionList
        {
            public byte[] OriginalFirst = new byte[4];
            public byte[]? FunctionName;
            public byte[] FunctionHead = new byte[2];
        }
    }
    public long FileStarIndex = 0;
    public long FileEndIndex = 0;
}

/// <summary>
/// 资源表
/// </summary>
public class ResourceDirectory
{
    public byte[] Characteristics = new byte[4];
    public byte[] TimeDateStamp = new byte[4];
    public byte[] MajorVersion = new byte[2];
    public byte[] MinorVersion = new byte[2];
    public byte[] NumberOfNamedEntries = new byte[2];
    public byte[] NumberOfIdEntries = new byte[2];
    public byte[]? Name;
    public ArrayList EntryList = new();

    public class DirectoryEntry
    {
        public byte[] Name = new byte[4];
        public byte[] Id = new byte[4];
        public ArrayList DataEntryList = new();
        public ArrayList NodeDirectoryList = new();

        public class DataEntry
        {
            public byte[] ResourRVA = new byte[4];
            public byte[] ResourSize = new byte[4];
            public byte[] ResourTest = new byte[4];
            public byte[] ResourWen = new byte[4];

            public long FileStarIndex = 0;
            public long FileEndIndex = 0;
        }
    }

    public long FileStarIndex = 0;
    public long FileEndIndex = 0;
}
#endregion