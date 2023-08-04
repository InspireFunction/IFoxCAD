﻿#if acad
global using AcException = Autodesk.AutoCAD.Runtime.Exception;
#elif gcad
global using AcException =Gstarsoft.GstarCAD.Runtime.Exception;
#elif zcad
global using AcException=ZwSoft.ZwCAD.Runtime.Exception;
#endif

namespace IFoxCAD.Cad;

/// <summary>
/// CAD错误大全
/// </summary>
public static class ErrorInfoEx
{
    /// <summary>
    /// 打印CAD错误信息到命令行
    /// <code>使用方法：
    /// try
    /// {
    ///     //你的代码
    /// }
    /// catch (AcException acex) { acex.AcErrorInfo(); }
    /// catch (Exception) { throw; }
    /// </code>
    /// </summary>
    /// <param name="acex">AcException</param>
    internal static void AcErrorInfo(this AcException acex)
    {
        string infostr = acex.Message switch
        {
            "eAlreadyInDb" => "已经在数据库中了",
            "eAmbiguousInput" => "模糊不清的输入",
            "eAmbiguousOutput" => "模糊不清的输出",
            "eAnonymousEntry" => "多重入口",
            "eBrokenHandle" => "损坏的句柄",
            "eBufferTooSmall" => "缓冲区太小",
            "eContainerNotEmpty" => "容器不为空",
            "eDeletedEntry" => "已经删除的函数入口",
            "eDuplicateDxfField" => "DXF字段重复",
            "eDuplicateIndex" => "重复的索引",
            "eDuplicateKey" => "重复的关键字",
            "eEndOfFile" => "文件结束",
            "eEndOfObject" => "对象结束",
            "eEntityInInactiveLayout" => "实体不在活动布局上",
            "eHandleExists" => "句柄已存在",
            "eHandleInUse" => "句柄被占用",
            "eIllegalEntityType" => "非法的实体类型",
            "eIllegalReplacement" => "非法的替代者",
            "eInvalidAdsName" => "无效的ADS名称",
            "eInvalidBlockName" => "不合理的块名称",
            "eInvalidDwgVersion" => "不合理的DWG版本",
            "eInvalidDxfCode" => "不合理的DXF编码",
            "eInvalidExtents" => "无效的空间范围",
            "eInvalidIndex" => "无效的索引",
            "eInvalidInput" => "无效的输入",
            "eInvalidKey" => "无效的关键字",
            "eInvalidOpenState" => "无效的打开状态",
            "eInvalidSymbolTableName" => "无效的符号名称",
            "eIsReading" => "正在读取",
            "eIsWriting" => "正在写入",
            "eKeyNotFound" => "关键字未找到",
            "eMissingDxfField" => "DXF字段缺失",
            "eNegativeValueNotAllowed" => "不允许输入负数",
            "eNotApplicable" => "不合适的",
            "eNotImplementedYet" => "尚未实现",
            "eNotOpenForRead" => "不是只读打开",
            "eNotOpenForWrite" => "不是可写打开",
            "eNotThatKindOfClass" => "类型不匹配",
            "eNullBlockName" => "块名称为空",
            "eNullEntityPointer" => "实体指针为空",
            "eNullHandle" => "空句柄",
            "eNullObjectId" => "对象ID为空",
            "eNullObjectPointer" => "对象指针为空",
            "eObjectToBeDeleted" => "对象即将被删除",
            "eOk" => "正确",
            "eOutOfDisk" => "硬盘容量不足",
            "eOutOfMemory" => "内存不足",
            "eUnknownHandle" => "未知句柄",
            "eWrongDatabase" => "错误的数据库",
            "eWrongObjectType" => "错误的类型",
            "eInvalidResBuf" => "不合理的ResBuf",
            "eBadDxfSequence" => "不正确的DXF顺序",
            "eFilerError" => "文件错误",
            "eVertexAfterFace" => "顶点在面后面",
            "eInvalidFaceVertexIndex" => "不合理的面顶点顺序",
            "eInvalidMeshVertexIndex" => "不合理的mesh顺序",
            "eOtherObjectsBusy" => "其它对象忙",
            "eMustFirstAddBlockToDb" => "必须先把块加入到数据库",
            "eCannotNestBlockDefs" => "不可以嵌套块定义",
            "eDwgRecoveredOK" => "修复DWG完成",
            "eDwgNotRecoverable" => "无法修复DWG",
            "eDxfPartiallyRead" => "DXF部分读取",
            "eDxfReadAborted" => "读取DXF终止",
            "eDxbPartiallyRead" => "DXB部分读取",
            "eDwgCRCDoesNotMatch" => "DWG文件的CRC不匹配",
            "eDwgSentinelDoesNotMatch" => "DWG文件的校验不匹配",
            "eDwgObjectImproperlyRead" => "DWG文件错误读取",
            "eNoInputFiler" => "没有找到输入过滤",
            "eDwgNeedsAFullSave" => "DWG需要完全保存",
            "eDxbReadAborted" => "DXB读取终止",
            "eFileLockedByACAD" => "文件被ACAD锁定",
            "eFileAccessErr" => "无法读取文件",
            "eFileSystemErr" => "文件系统错误",
            "eFileInternalErr" => "文件内部错误",
            "eFileTooManyOpen" => "文件被打开太多次",
            "eFileNotFound" => "未找到文件",
            "eDwkLockFileFound" => "找到DWG锁定文件",
            "eWasErased" => "对象被删除",
            "ePermanentlyErased" => "对象被永久删除",
            "eWasOpenForRead" => "对象只读打开",
            "eWasOpenForWrite" => "对象可写打开",
            "eWasOpenForUndo" => "对象撤销打开",
            "eWasNotifying" => "对象被通知",
            "eWasOpenForNotify" => "对象通知打开",
            "eOnLockedLayer" => "对象在锁定图层上",
            "eMustOpenThruOwner" => "必须经过所有者打开",
            "eSubentitiesStillOpen" => "子对象依然打开着",
            "eAtMaxReaders" => "超过最大打开次数",
            "eIsWriteProtected" => "对象被写保护",
            "eIsXRefObject" => "对象是XRef",
            "eNotAnEntity" => "对象不是实体",
            "eHadMultipleReaders" => "被多重打开",
            "eDuplicateRecordName" => "重复的记录名称",
            "eXRefDependent" => "依赖于XREF",
            "eSelfReference" => "引用自身",
            "eMissingSymbolTable" => "丢失符号化表",
            "eMissingSymbolTableRec" => "丢失符号化记录",
            "eWasNotOpenForWrite" => "不是可写打开",
            "eCloseWasNotifying" => "对象关闭,正在执行通知",
            "eCloseModifyAborted" => "对象关闭,修改被取消",
            "eClosePartialFailure" => "对象关闭,部分操作未成功",
            "eCloseFailObjectDamaged" => "对象被损坏,关闭失败",
            "eCannotBeErasedByCaller" => "对象不可以被当前呼叫者删除",
            "eCannotBeResurrected" => "不可以复活",
            "eWasNotErased" => "对象未删除",
            "eInsertAfter" => "在后面插入",
            "eFixedAllErrors" => "修复了所有错误",
            "eLeftErrorsUnfixed" => "剩下一些错误未修复",
            "eUnrecoverableErrors" => "不可恢复的错误",
            "eNoDatabase" => "没有数据库",
            "eXdataSizeExceeded" => "扩展数据长度太大",
            "eRegappIdNotFound" => "没有找到扩展数据注册ID",
            "eRepeatEntity" => "重复实体",
            "eRecordNotInTable" => "表中未找到记录",
            "eIteratorDone" => "迭代器完成",
            "eNullIterator" => "空的迭代器",
            "eNotInBlock" => "不在块中",
            "eOwnerNotInDatabase" => "所有者不在数据库中",
            "eOwnerNotOpenForRead" => "所有者不是只读打开",
            "eOwnerNotOpenForWrite" => "所有者不是可写打开",
            "eExplodeBeforeTransform" => "在变换之前就被炸开了",
            "eCannotScaleNonUniformly" => "不可以不同比例缩放",
            "eNotInDatabase" => "不在数据库中",
            "eNotCurrentDatabase" => "不是当前数据库",
            "eIsAnEntity" => "是一个实体",
            "eCannotChangeActiveViewport" => "不可以改变活动视口",
            "eNotInPaperspace" => "不在图纸空间中",
            "eCommandWasInProgress" => "正在执行命令",
            "eGeneralModelingFailure" => "创建模型失败",
            "eOutOfRange" => "超出范围",
            "eNonCoplanarGeometry" => "没有平面几何对象",
            "eDegenerateGeometry" => "退化的几何对象",
            "eInvalidAxis" => "无效的轴线",
            "ePointNotOnEntity" => "点不在实体上",
            "eSingularPoint" => "单一的点",
            "eInvalidOffset" => "无效的偏移",
            "eNonPlanarEntity" => "没有平面的实体",
            "eCannotExplodeEntity" => "不可分解的实体",
            "eStringTooLong" => "字符串太短",
            "eInvalidSymTableFlag" => "无效的符号化表标志",
            "eUndefinedLineType" => "没有定义的线型",
            "eInvalidTextStyle" => "无效的字体样式",
            "eTooFewLineTypeElements" => "太少的线型要素",
            "eTooManyLineTypeElements" => "太多的线型要素",
            "eExcessiveItemCount" => "过多的项目",
            "eIgnoredLinetypeRedef" => "忽略线型定义描述",
            "eBadUCS" => "不好的用户坐标系",
            "eBadPaperspaceView" => "不好的图纸空间视图",
            "eSomeInputDataLeftUnread" => "一些输入数据未被读取",
            "eNoInternalSpace" => "不是内部空间",
            "eInvalidDimStyle" => "无效的标注样式",
            "eInvalidLayer" => "无效的图层",
            "eUserBreak" => "用户打断",
            "eDwgNeedsRecovery" => "DWG文件需要修复",
            "eDeleteEntity" => "删除实体",
            "eInvalidFix" => "无效的方位",
            "eFSMError" => "FSM错误",
            "eBadLayerName" => "不好的图层名称",
            "eLayerGroupCodeMissing" => "图层分组编码丢失",
            "eBadColorIndex" => "不好的颜色索引号",
            "eBadLinetypeName" => "不好的线型名称",
            "eBadLinetypeScale" => "不好的线型缩放比例",
            "eBadVisibilityValue" => "不好的可见性值",
            "eProperClassSeparatorExpected" => "本身类未找到预期的分割符号(?)",
            "eBadLineWeightValue" => "不好的线宽值",
            "eBadColor" => "不好的颜色",
            "ePagerError" => "页面错误",
            "eOutOfPagerMemory" => "页面内存不足",
            "ePagerWriteError" => "页面不可写",
            "eWasNotForwarding" => "不是促进(?)",
            "eInvalidIdMap" => "无效的ID字典",
            "eInvalidOwnerObject" => "无效的所有者",
            "eOwnerNotSet" => "未设置所有者",
            "eWrongSubentityType" => "错误的子对象类型",
            "eTooManyVertices" => "太多节点",
            "eTooFewVertices" => "太少节点",
            "eNoActiveTransactions" => "不活动的事务",
            "eNotTopTransaction" => "不是最顶层的事务",
            "eTransactionOpenWhileCommandEnded" => "在命令结束的时候打开(/开始)事务",
            "eInProcessOfCommitting" => "在提交事务的过程中",
            "eNotNewlyCreated" => "不是新创建的",
            "eLongTransReferenceError" => "长事务引用错误",
            "eNoWorkSet" => "没有工作集",
            "eAlreadyInGroup" => "已经在组中了",
            "eNotInGroup" => "不在组中",
            "eInvalidREFIID" => "无效的REFIID",
            "eInvalidNormal" => "无效的标准",
            "eInvalidStyle" => "无效的样式",
            "eCannotRestoreFromAcisFile" => "不可以从Acis(?)文件中恢复",
            "eMakeMeProxy" => "自我代理",
            "eNLSFileNotAvailable" => "无效的NLS文件",
            "eNotAllowedForThisProxy" => "不允许这个代理",
            "eNotSupportedInDwgApi" => "在Dwg Api中不支持",
            "ePolyWidthLost" => "多段线宽度丢失",
            "eNullExtents" => "空的空间范围",
            "eExplodeAgain" => "再一次分解",
            "eBadDwgHeader" => "坏的DWG文件头",
            "eLockViolation" => "锁定妨碍当前操作",
            "eLockConflict" => "锁定冲突",
            "eDatabaseObjectsOpen" => "数据库对象打开",
            "eLockChangeInProgress" => "锁定改变中",
            "eVetoed" => "禁止",
            "eNoDocument" => "没有文档",
            "eNotFromThisDocument" => "不是从这个文档",
            "eLISPActive" => "LISP活动",
            "eTargetDocNotQuiescent" => "目标文档活动中",
            "eDocumentSwitchDisabled" => "禁止文档转换",
            "eInvalidContext" => "无效的上下文环境",
            "eCreateFailed" => "创建失败",
            "eCreateInvalidName" => "创建无效名称",
            "eSetFailed" => "设置失败",
            "eDelDoesNotExist" => "删除对象不存在",
            "eDelIsModelSpace" => "删除模型空间",
            "eDelLastLayout" => "删除最后一个布局",
            "eDelUnableToSetCurrent" => "删除后无法设置当前对象",
            "eDelUnableToFind" => "没有找到删除对象",
            "eRenameDoesNotExist" => "重命名对象不存在",
            "eRenameIsModelSpace" => "不可以重命令模型空间",
            "eRenameInvalidLayoutName" => "重命名无效的布局名称",
            "eRenameLayoutAlreadyExists" => "重命名布局名称已存在",
            "eRenameInvalidName" => "重命名无效名称",
            "eCopyDoesNotExist" => "拷贝不存在",
            "eCopyIsModelSpace" => "拷贝是模型空间",
            "eCopyFailed" => "拷贝失败",
            "eCopyInvalidName" => "拷贝无效名称",
            "eCopyNameExists" => "拷贝名称存在",
            "eProfileDoesNotExist" => "配置名称不存在",
            "eInvalidFileExtension" => "无效的文件后缀名成",
            "eInvalidProfileName" => "无效的配置文件名称",
            "eFileExists" => "文件存在",
            "eProfileIsInUse" => "配置文件存在",
            "eCantOpenFile" => "打开文件失败",
            "eNoFileName" => "没有文件名称",
            "eRegistryAccessError" => "读取注册表错误",
            "eRegistryCreateError" => "创建注册表项错误",
            "eBadDxfFile" => "坏的DXF文件",
            "eUnknownDxfFileFormat" => "未知的DXF文件格式",
            "eMissingDxfSection" => "丢失DXF分段",
            "eInvalidDxfSectionName" => "无效的DXF分段名称",
            "eNotDxfHeaderGroupCode" => "无效的DXF组码",
            "eUndefinedDxfGroupCode" => "没有定义DXF组码",
            "eNotInitializedYet" => "没有初始化",
            "eInvalidDxf2dPoint" => "无效的DXF二维点",
            "eInvalidDxf3dPoint" => "无效的DXD三维点",
            "eBadlyNestedAppData" => "坏的嵌套应用程序数据",
            "eIncompleteBlockDefinition" => "不完整的块定义",
            "eIncompleteComplexObject" => "不完整的合成(?复杂)对象",
            "eBlockDefInEntitySection" => "块定义在实体段中",
            "eNoBlockBegin" => "没有块开始",
            "eDuplicateLayerName" => "重复的图层名称",
            "eBadPlotStyleName" => "不好的打印样式名称",
            "eDuplicateBlockName" => "重复的块名称",
            "eBadPlotStyleType" => "不好的打印样式类型",
            "eBadPlotStyleNameHandle" => "不好的打印样式名称句柄",
            "eUndefineShapeName" => "没有定义形状名称",
            "eDuplicateBlockDefinition" => "重复的块定义",
            "eMissingBlockName" => "丢失了块名称",
            "eBinaryDataSizeExceeded" => "二进制数据长度太长",
            "eObjectIsReferenced" => "对象被引用",
            "eNoThumbnailBitmap" => "没有缩略图",
            "eGuidNoAddress" => "未找到GUID地址",
            "eMustBe0to2" => "必须是0到2",
            "eMustBe0to3" => "必须是0到3",
            "eMustBe0to4" => "必须是0到4",
            "eMustBe0to5" => "必须是0到5",
            "eMustBe0to8" => "必须是0到8",
            "eMustBe1to8" => "必须是1到8",
            "eMustBe1to15" => "必须是1到15",
            "eMustBePositive" => "必须为正数",
            "eMustBeNonNegative" => "必须为非负数",
            "eMustBeNonZero" => "不可以等于0",
            "eMustBe1to6" => "必须是1到6",
            "eNoPlotStyleTranslationTable" => "没有打印样式事务表(?)",
            "ePlotStyleInColorDependentMode" => "打印样式依赖颜色",
            "eMaxLayouts" => "最大布局数量",
            "eNoClassId" => "没有类ID",
            "eUndoOperationNotAvailable" => "撤销操作无效",
            "eUndoNoGroupBegin" => "撤销操作没有组开始",
            "eHatchTooDense" => "填充太密集",
            "eOpenFileCancelled" => "打开文件取消",
            "eNotHandled" => "没有处理",
            "eMakeMeProxyAndResurrect" => "将自己变成代理然后复活",
            "eFileMissingSections" => "文件丢失分段",
            "eRepeatedDwgRead" => "重复的读取DWG文件",
            "eWrongCellType" => "错误的单元格类型",
            "eCannotChangeColumnType" => "不可以改变列类型",
            "eRowsMustMatchColumns" => "行必须匹配列",
            "eFileSharingViolation" => "文件共享妨碍",
            "eUnsupportedFileFormat" => "不支持的文件格式",
            "eObsoleteFileFormat" => "废弃的文件格式",
            "eDwgShareDemandLoad" => "DWG共享要求加载(?)",
            "eDwgShareReadAccess" => "DWG共享读取",
            "eDwgShareWriteAccess" => "DWG共享写入",
            "eLoadFailed" => "加载失败",
            "eDeviceNotFound" => "驱动未找到",
            "eNoCurrentConfig" => "没有当前配置",
            "eNullPtr" => "空指针",
            "eNoLayout" => "没有布局",
            "eIncompatiblePlotSettings" => "不兼容的打印设置",
            "eNonePlotDevice" => "没有打印驱动",
            "eNoMatchingMedia" => "没有匹配的打印尺寸",
            "eInvalidView" => "无效的视图",
            "eInvalidWindowArea" => "无效的窗口范围",
            "eInvalidPlotArea" => "无效的打印范围",
            "eCustomSizeNotPossible" => "用户输入的打印尺寸不可能存在",
            "ePageCancelled" => "纸张取消",
            "ePlotCancelled" => "打印取消",
            "eInvalidEngineState" => "无效的引擎状态",
            "ePlotAlreadyStarted" => "已经开始在打印了",
            "eNoErrorHandler" => "没有错误处理",
            "eInvalidPlotInfo" => "无效的打印信息",
            "eNumberOfCopiesNotSupported" => "不支持打印份数",
            "eLayoutNotCurrent" => "不是当前布局",
            "eGraphicsNotGenerated" => "绘图对象创建失败(?)",
            "eCannotPlotToFile" => "不可以打印到文件",
            "eMustPlotToFile" => "必须打印到文件",
            "eNotMultiPageCapable" => "不支持多种纸张",
            "eBackgroundPlotInProgress" => "正在后台打印",
            "eSubSelectionSetEmpty" => "子选择集被设置为空",
            "eInvalidObjectId" => "无效的对象ID或者对象ID不在当前数据库",
            "eInvalidXrefObjectId" => "无效的XREF对象ID或者XREF对象ID不在当前数据库",
            "eNoViewAssociation" => "未找到对应的视图对象",
            "eNoLabelBlock" => "视口未找到关联的块",
            "eUnableToSetViewAssociation" => "设置视图关联视口失败",
            "eUnableToGetViewAssociation" => "无法找到关联的视图",
            "eUnableToSetLabelBlock" => "无法设置关联的块",
            "eUnableToGetLabelBlock" => "无法获取关联的块",
            "eUnableToRemoveAssociation" => "无法移除视口关联对象",
            "eUnableToSyncModelView" => "无法同步视口和模型空间视图",
            "eSecInitializationFailure" => "SEC(?)初始化错误",
            "eSecErrorReadingFile" => "SEC(?)读取文件错误",
            "eSecErrorWritingFile" => "SEC(?)写入文件错误",
            "eSecInvalidDigitalID" => "SEC(?)无效的数字ID",
            "eSecErrorGeneratingTimestamp" => "SEC(?)创建时间戳错误",
            "eSecErrorComputingSignature" => "SEC(?)电子签名错误",
            "eSecErrorWritingSignature" => "SEC(?)写入签名错误",
            "eSecErrorEncryptingData" => "SEC(?)加密数据错误",
            "eSecErrorCipherNotSupported" => "SEC(?)不支持的密码",
            "eSecErrorDecryptingData" => "SEC(?)解密数据错误",
            "eInetBase" => "网络错误",
            "eInetOk" => "网络正常",
            "eInetInCache" => "在缓冲区中",
            "eInetFileNotFound" => "网络文件不存在",
            "eInetBadPath" => "不好的网络路径",
            "eInetTooManyOpenFiles" => "打开太多网络文件",
            "eInetFileAccessDenied" => "打开网络文件被拒绝",
            "eInetInvalidFileHandle" => "无效的网络文件句柄",
            "eInetDirectoryFull" => "网络文件夹目录已满",
            "eInetHardwareError" => "网络硬件错误",
            "eInetSharingViolation" => "违反网络共享",
            "eInetDiskFull" => "网络硬盘满了",
            "eInetFileGenericError" => "网络文件创建错误",
            "eInetValidURL" => "无效的URL地址",
            "eInetNotAnURL" => "不是URL地址",
            "eInetNoWinInet" => "没有WinInet(?)",
            "eInetOldWinInet" => "旧的WinInet(?)",
            "eInetNoAcadInet" => "无法连接ACAD网站",
            "eInetNotImplemented" => "无法应用网络",
            "eInetProtocolNotSupported" => "网络协议不支持",
            "eInetCreateInternetSessionFailed" => "创建网络会话失败",
            "eInetInternetSessionConnectFailed" => "连接网络会话失败",
            "eInetInternetSessionOpenFailed" => "打开网络会话失败",
            "eInetInvalidAccessType" => "无效的网络接收类型",
            "eInetFileOpenFailed" => "打开网络文件失败",
            "eInetHttpOpenRequestFailed" => "打开HTTP协议失败",
            "eInetUserCancelledTransfer" => "用户取消了网络传输",
            "eInetHttpBadRequest" => "不合理的网络请求",
            "eInetHttpAccessDenied" => "HTTP协议拒绝",
            "eInetHttpPaymentRequired" => "HTTP协议要求付费",
            "eInetHttpRequestForbidden" => "禁止HTTP请求",
            "eInetHttpObjectNotFound" => "HTTP对象未找到",
            "eInetHttpBadMethod" => "不合理的HTTP请求方法",
            "eInetHttpNoAcceptableResponse" => "不接受的HTTP回复",
            "eInetHttpProxyAuthorizationRequired" => "要求HTTP代理授权",
            "eInetHttpTimedOut" => "HTTP超时",
            "eInetHttpConflict" => "HTTP冲突",
            "eInetHttpResourceGone" => "网络资源被用光",
            "eInetHttpLengthRequired" => "HTTP请求长度是必须的",
            "eInetHttpPreconditionFailure" => "HTTP预处理失败",
            "eInetHttpRequestTooLarge" => "HTTP请求太大",
            "eInetHttpUriTooLong" => "URL地址太长",
            "eInetHttpUnsupportedMedia" => "HTTP不支持的媒体",
            "eInetHttpServerError" => "HTTP服务器错误",
            "eInetHttpNotSupported" => "HTTP不支持",
            "eInetHttpBadGateway" => "HTTP网关错误",
            "eInetHttpServiceUnavailable" => "HTTP服务当前不可用",
            "eInetHttpGatewayTimeout" => "HTTP网关超时",
            "eInetHttpVersionNotSupported" => "HTTP版本不支持",
            "eInetInternetError" => "HTTP网络错误",
            "eInetGenericException" => "HTTP常规异常",
            "eInetUnknownError" => "HTTP未知错误",
            "eAlreadyActive" => "已经是活动的了",
            "eAlreadyInactive" => "已经是不活动的了",
            _ => acex.Message,
        };
        Acap.ShowAlertDialog($"{acex.Message}：{infostr}");
    }
}