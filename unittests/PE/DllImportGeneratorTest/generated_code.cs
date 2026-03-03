namespace AutoCAD.Api
{

    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// _Global 类的Native方法 (850 个函数)
    /// </summary>
    public static class _Global
    {
        [DllImport("acad.exe", EntryPoint = "ACAD.exe", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ACAD_exe();

        [DllImport("acad.exe", EntryPoint = "?ADCHatchBhatch@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ADCHatchBhatch();

        [DllImport("acad.exe", EntryPoint = "?ADCHatchGetRegistryHPFileName@@YAHAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ADCHatchGetRegistryHPFileName(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?ADCHatchMultipleDropping@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ADCHatchMultipleDropping();

        [DllImport("acad.exe", EntryPoint = "?ADCHatchRtDDApplyMultipleHatch@@YAHABV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@00@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ADCHatchRtDDApplyMultipleHatch(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?ADCHatchRtDDCleanup@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ADCHatchRtDDCleanup();

        [DllImport("acad.exe", EntryPoint = "?ADCHatchRtDDRenderHatch@@YAHABV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ADCHatchRtDDRenderHatch(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?ADCHatchSetBhatchData@@YAXABV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ADCHatchSetBhatchData(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?ADCHatchSetOleDataObject@@YAXPAVCOleDataObject@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ADCHatchSetOleDataObject(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?ADCHatchSwichAcadViewBkColor@@YAHH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ADCHatchSwichAcadViewBkColor(int arg0);

        [DllImport("acad.exe", EntryPoint = "?ADCHatchSwichBackAcadViewBkColor@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ADCHatchSwichBackAcadViewBkColor();

        [DllImport("acad.exe", EntryPoint = "?ADCHatchWriteRegistryHPFileName@@YAHV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ADCHatchWriteRegistryHPFileName(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?ADCSetState@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ADCSetState(int arg0);

        [DllImport("acad.exe", EntryPoint = "?APSetState@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr APSetState(int arg0);

        [DllImport("acad.exe", EntryPoint = "?AcApGetDatabase@@YAPAVAcDbDatabase@@PAVCView@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AcApGetDatabase();

        [DllImport("acad.exe", EntryPoint = "?AcGetCurrentWorkspace@@YA?AW4ErrorStatus@Acad@@AAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AcGetCurrentWorkspace(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?AcPlGetHostAppServices@@YAPAVAcPlHostAppServices@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AcPlGetHostAppServices();

        [DllImport("acad.exe", EntryPoint = "?AcPlSetHostAppServices@@YA?AW4ErrorStatus@Acad@@PAVAcPlHostAppServices@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AcPlSetHostAppServices(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?AcSaveWorkspace@@YA?AW4ErrorStatus@Acad@@PB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AcSaveWorkspace(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?AcSetCurrentWorkspace@@YA?AW4ErrorStatus@Acad@@PB_W_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AcSetCurrentWorkspace(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?AcSetWorkspaceMask@@YA?AW4ErrorStatus@Acad@@K@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AcSetWorkspaceMask(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?AcWorkspaceMask@@YAKXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AcWorkspaceMask();

        [DllImport("acad.exe", EntryPoint = "?AcadGetContextMenuHandle@@YAPAUHMENU__@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AcadGetContextMenuHandle();

        [DllImport("acad.exe", EntryPoint = "?AcadGetCurrentDwgView@@YAPAUHWND__@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AcadGetCurrentDwgView();

        [DllImport("acad.exe", EntryPoint = "?AcadIsQuitting@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AcadIsQuitting();

        [DllImport("acad.exe", EntryPoint = "?AcadProcessSessionQueue@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AcadProcessSessionQueue();

        [DllImport("acad.exe", EntryPoint = "?AcadSetOpm@@YAXPAUIOpm@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AcadSetOpm(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?AddColorFromOPMToAllControls@@YAXABVAcCmColor@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr AddColorFromOPMToAllControls(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?CleanScreenSetState@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CleanScreenSetState(int arg0);

        [DllImport("acad.exe", EntryPoint = "?ClearOopsSSet@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ClearOopsSSet();

        [DllImport("acad.exe", EntryPoint = "?CopyHistCommandHook@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CopyHistCommandHook();

        [DllImport("acad.exe", EntryPoint = "?DRSetState@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DRSetState(int arg0);

        [DllImport("acad.exe", EntryPoint = "?DashboardSetState@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DashboardSetState(int arg0);

        [DllImport("acad.exe", EntryPoint = "?DimExportExpressToolHelper@@YAHPA_WPAVAcDbLinetypeTableRecord@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DimExportExpressToolHelper(string arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?DisplayFileOptionDialog@@YAHPAUHWND__@@ABV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@W4inout@@PAH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DisplayFileOptionDialog(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?DisplayLastSearchError@@YAXW4DisplayMode@AcSearchPath@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DisplayLastSearchError(char arg0, int arg1, sbyte arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, float arg9, int arg10, int arg11, int arg12);

        [DllImport("acad.exe", EntryPoint = "?DoAdlPan@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DoAdlPan();

        [DllImport("acad.exe", EntryPoint = "?DoAdlZoom@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DoAdlZoom();

        [DllImport("acad.exe", EntryPoint = "?DrawLTypePattern@@YAXPAVCDC@@VAcDbObjectId@@PAUtagRECT@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DrawLTypePattern(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?DrawLWLine@@YAXPAVCDC@@W4LineWeight@AcDb@@PAUtagRECT@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DrawLWLine(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?DrawLineTypeItemHelper@@YGXPAVCDC@@AAVAcDbObjectId@@VCRect@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort DrawLineTypeItemHelper();

        [DllImport("acad.exe", EntryPoint = "?ERSetState@@YAXF@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ERSetState(short arg0);

        [DllImport("acad.exe", EntryPoint = "?EnableMultiDocumentActivation@@YAHH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr EnableMultiDocumentActivation(int arg0);

        [DllImport("acad.exe", EntryPoint = "?EngageTool@@YAXPAVCTool@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr EngageTool(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?EstimateNumHatchLines@@YAHPBVAcDbHatch@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr EstimateNumHatchLines(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?ExpandPathVariables@@YA_NAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ExpandPathVariables(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?ExtractProductID@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ExtractProductID();

        [DllImport("acad.exe", EntryPoint = "?FileOptionsDialogHasTab@@YAHABV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@W4inout@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr FileOptionsDialogHasTab(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?GetAcadPaletteHandle@@YAPAUHPALETTE__@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetAcadPaletteHandle();

        [DllImport("acad.exe", EntryPoint = "?GetAcadStartInDir@@YA?AW4ErrorStatus@Acad@@HPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetAcadStartInDir(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?GetCommandInProgressInDoc@@YAFXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetCommandInProgressInDoc();

        [DllImport("acad.exe", EntryPoint = "?GetCommandRepeated@@YAFPAVAcApDocument@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetCommandRepeated(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?GetCurrentAttachmentSpace@@YAXAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetCurrentAttachmentSpace(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?GetIUnknownOfFullSubentPath@@YAPAUIUnknown@@AAVAcDbFullSubentPath@@AAU_GUID@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetIUnknownOfFullSubentPath();

        [DllImport("acad.exe", EntryPoint = "?GetIUnknownOfObject@@YAPAUIUnknown@@VAcDbObjectId@@AAU_GUID@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetIUnknownOfObject();

        [DllImport("acad.exe", EntryPoint = "?GetLineWeightFromAcad@@YA?AW4LineWeight@AcDb@@AAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetLineWeightFromAcad(IntPtr arg0, int arg1, int arg2, int arg3, int arg4, int arg5, char arg6, int arg7, int arg8, int arg9, int arg10, int arg11);

        [DllImport("acad.exe", EntryPoint = "?GetListOfPlotStyleTables@@YAHAAVCStringList@@_N1@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetListOfPlotStyleTables(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?GetListOfPlotStyles@@YAHAAVCStringList@@AAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@1_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetListOfPlotStyles(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?GetListOfPlotStylesFromDictionary@@YAHAAVCStringList@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetListOfPlotStylesFromDictionary(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?GetMRUInfo@@YAHW4MRUinfotype@@HPAVAcCmColor@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetMRUInfo(char arg0, int arg1, float arg2, IntPtr arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?GetOemCLSID@@YA?AU_GUID@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetOemCLSID(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?GetOemCLSID@@YA_NHPAU_GUID@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetOemCLSID_1(int arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?GetPlotStateForGS@@YAXPA_NPAH00@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetPlotStateForGS(IntPtr arg0, IntPtr arg1, int arg2, int arg3);

        [DllImport("acad.exe", EntryPoint = "?GetPlotStyleTableType@@YAHPB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetPlotStyleTableType(string arg0);

        [DllImport("acad.exe", EntryPoint = "?GetStartupFlag@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetStartupFlag();

        [DllImport("acad.exe", EntryPoint = "?GetTemplateFileLocation@@YAXPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetTemplateFileLocation(string arg0);

        [DllImport("acad.exe", EntryPoint = "?HandleToolbarComboMessage@@YAHIIIJ@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr HandleToolbarComboMessage(uint arg0, uint arg1, uint arg2, int arg3);

        [DllImport("acad.exe", EntryPoint = "?IPECenterPoint@@YA?AVAcGePoint3d@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr IPECenterPoint(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?IPEInvoke@@YA_NPAUIPEControl@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr IPEInvoke(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?IPEInvokeEditorOn@@YAHPAVAcDbEntity@@PBUIPESettings@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr IPEInvokeEditorOn(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?IPEIsEditor@@YAHH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr IPEIsEditor(int arg0);

        [DllImport("acad.exe", EntryPoint = "?InitializeProxyInet@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr InitializeProxyInet();

        [DllImport("acad.exe", EntryPoint = "?IsMultiDocumentActivationEnabled@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr IsMultiDocumentActivationEnabled();

        [DllImport("acad.exe", EntryPoint = "?IsUnmodifiedStartupDoc@@YAHPAVAcApDocument@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr IsUnmodifiedStartupDoc(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?LayerCommandLine@@YGXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort LayerCommandLine();

        [DllImport("acad.exe", EntryPoint = "?LightListSetState@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr LightListSetState(int arg0);

        [DllImport("acad.exe", EntryPoint = "?LineWeightToString@@YAXW4LineWeight@AcDb@@AAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr LineWeightToString(char arg0, int arg1, int arg2, int arg3, int arg4, int arg5, char arg6, int arg7, int arg8, int arg9, int arg10, int arg11);

        [DllImport("acad.exe", EntryPoint = "?LoadBitmapFileGdiPlus@@YAPAVBitmap@Gdiplus@@PB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr LoadBitmapFileGdiPlus(ushort arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6);

        [DllImport("acad.exe", EntryPoint = "?LocalizeReservedPlotStyleStrings@@YA_NAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr LocalizeReservedPlotStyleStrings(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?MSMSetState@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MSMSetState(int arg0);

        [DllImport("acad.exe", EntryPoint = "?MaterialEditorSetState@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr MaterialEditorSetState(int arg0);

        [DllImport("acad.exe", EntryPoint = "?OPMActiveState@@YAXF@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OPMActiveState(short arg0);

        [DllImport("acad.exe", EntryPoint = "?OPMCustomPatternValid@@YAHPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OPMCustomPatternValid(string arg0);

        [DllImport("acad.exe", EntryPoint = "?OPMFillPatternList@@YAHP6AXPA_W@Z@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OPMFillPatternList(IntPtr arg0, string arg1);

        [DllImport("acad.exe", EntryPoint = "?OPMGetPatName@@YAHHAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OPMGetPatName(int arg0, IntPtr arg1, char arg2, IntPtr arg3, char arg4, IntPtr arg5, char arg6);

        [DllImport("acad.exe", EntryPoint = "?OPMPlotStyleName@@YAHAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OPMPlotStyleName(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?OPMisohpvalid@@YAHPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OPMisohpvalid(string arg0);

        [DllImport("acad.exe", EntryPoint = "?OPMxemark@@YAXPAPAVSegment@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OPMxemark(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?OPMxmark@@YAXNNNPAPAVSegment@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OPMxmark(double arg0, double arg1, double arg2, IntPtr arg3);

        [DllImport("acad.exe", EntryPoint = "?OleScaleDialogPreference@@YAFAAF_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OleScaleDialogPreference(IntPtr arg0, bool arg1);

        [DllImport("acad.exe", EntryPoint = "?OrbitInProgress@@YA_NAAPAVCTool@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr OrbitInProgress(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?PreviewInWnd@@YAHPB_WHPAVCWnd@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PreviewInWnd(string arg0, int arg1, IntPtr arg2);

        [DllImport("acad.exe", EntryPoint = "?QCSetState@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr QCSetState(int arg0);

        [DllImport("acad.exe", EntryPoint = "?RemovingAutoLISP@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr RemovingAutoLISP();

        [DllImport("acad.exe", EntryPoint = "?RenderPrefsSetState@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr RenderPrefsSetState(int arg0);

        [DllImport("acad.exe", EntryPoint = "?RunPlotStyleOtherDialog@@YAHHHAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@PAVCWnd@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr RunPlotStyleOtherDialog(int arg0, int arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5, IntPtr arg6, char arg7);

        [DllImport("acad.exe", EntryPoint = "?SSMSetState@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SSMSetState(int arg0);

        [DllImport("acad.exe", EntryPoint = "?SendAppMessage@@YA_NPB_WW4AppMsgCode@AcRx@@PAX@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SendAppMessage(string arg0, char arg1, int arg2, IntPtr arg3, int arg4, float arg5, int arg6, int arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?SetArrayResultCallback@@YAXP6AXPAV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@@Z@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SetArrayResultCallback(IntPtr arg0, IntPtr arg1, IntPtr arg2);

        [DllImport("acad.exe", EntryPoint = "?SetAssociatedURLInDoc@@YAXPB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SetAssociatedURLInDoc(string arg0);

        [DllImport("acad.exe", EntryPoint = "?SetCommandInProgressInDoc@@YAXF@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SetCommandInProgressInDoc(short arg0);

        [DllImport("acad.exe", EntryPoint = "?SetCommandURLInDoc@@YAXPB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SetCommandURLInDoc(string arg0);

        [DllImport("acad.exe", EntryPoint = "?SetCustomGrips@@YAXAAV?$AcArray@VAcEdGripGlyph@@V?$AcArrayObjectCopyReallocator@VAcEdGripGlyph@@@@@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SetCustomGrips(IntPtr arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?SetDiastat@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SetDiastat(int arg0);

        [DllImport("acad.exe", EntryPoint = "?SetPartialOpenInDoc@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SetPartialOpenInDoc(int arg0);

        [DllImport("acad.exe", EntryPoint = "?SetSavedToAssociatedURLInDoc@@YAX_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SetSavedToAssociatedURLInDoc(bool arg0);

        [DllImport("acad.exe", EntryPoint = "?SetStartupFlag@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SetStartupFlag(int arg0);

        [DllImport("acad.exe", EntryPoint = "?ShowDwfSheet@@YAXPB_W0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ShowDwfSheet(string arg0, int arg1);

        [DllImport("acad.exe", EntryPoint = "?ShowDwfViewer@@YAXPB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ShowDwfViewer(string arg0);

        [DllImport("acad.exe", EntryPoint = "?SubstitutePathVariables@@YA_NAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SubstitutePathVariables(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?SunPropertiesSetState@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SunPropertiesSetState(int arg0);

        [DllImport("acad.exe", EntryPoint = "?TD_SetGotTokenCallback@@YAXP6AXXZ@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr TD_SetGotTokenCallback(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?TPSetState@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr TPSetState(int arg0);

        [DllImport("acad.exe", EntryPoint = "?ToolbarArxComm@@YAHHHPAX@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ToolbarArxComm(int arg0, int arg1, IntPtr arg2);

        [DllImport("acad.exe", EntryPoint = "?UnLoadPartialMenuCommand@@YAHPB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr UnLoadPartialMenuCommand(string arg0);

        [DllImport("acad.exe", EntryPoint = "?VSSetState@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr VSSetState(int arg0);

        [DllImport("acad.exe", EntryPoint = "?ViewComboSelect@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ViewComboSelect(int arg0);

        [DllImport("acad.exe", EntryPoint = "?WasLastCharEchoed@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr WasLastCharEchoed();

        [DllImport("acad.exe", EntryPoint = "?Xemark@@YAXPAPAVSegment@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Xemark(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?XmarkWithCircle@@YAXNNNPAPAVSegment@@HNM@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr XmarkWithCircle(double arg0, double arg1, double arg2, IntPtr arg3);

        [DllImport("acad.exe", EntryPoint = "?acAddPlotReactor@@YAXPAVAcApDocManager@@PAVAcApPlotReactor@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acAddPlotReactor(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acDocManagerPtr@@YAPAVAcApDocManager@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acDocManagerPtr();

        [DllImport("acad.exe", EntryPoint = "?acEditMTextAttributeInteractive@@YA_NPAVAcDbMText@@ABVAcDbObjectId@@_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acEditMTextAttributeInteractive(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acEditMTextInteractive@@YAHPAVAcDbMText@@_NPAUIPESettings@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acEditMTextInteractive(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acGetPlotReactorIterator@@YAPAVAcRxIterator@@PAVAcApDocManager@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acGetPlotReactorIterator();

        [DllImport("acad.exe", EntryPoint = "?acInternalGetCurVPAppliedView@@YA?AVAcDbObjectId@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acInternalGetCurVPAppliedView(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acInternalGetCurrentViewportContextColors@@YAPAVAcGiContextualColors@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acInternalGetCurrentViewportContextColors();

        [DllImport("acad.exe", EntryPoint = "?acInternalIsCurVPZoomUndoAvailable@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acInternalIsCurVPZoomUndoAvailable();

        [DllImport("acad.exe", EntryPoint = "?acInternalSetBackground@@YA_NPAVAcDbViewTableRecord@@VAcDbObjectId@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acInternalSetBackground(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acInternalSetBackground@@YA_NPAVAcDbViewport@@VAcDbObjectId@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acInternalSetBackground_1(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acInternalSetBackground@@YA_NPAVAcDbViewportTableRecord@@VAcDbObjectId@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acInternalSetBackground_2(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acInternalShowUIIntegrationBackground@@YA_N_N0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acInternalShowUIIntegrationBackground(bool arg0, int arg1);

        [DllImport("acad.exe", EntryPoint = "?acInternalZoomBack@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acInternalZoomBack();

        [DllImport("acad.exe", EntryPoint = "?acInternalZoomFactor@@YAXN@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acInternalZoomFactor(double arg0);

        [DllImport("acad.exe", EntryPoint = "?acInternalZoomPrevious@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acInternalZoomPrevious();

        [DllImport("acad.exe", EntryPoint = "?acProfileManagerPtr@@YAPAVAcApProfileManager@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acProfileManagerPtr();

        [DllImport("acad.exe", EntryPoint = "?acRemovePlotReactor@@YAXPAVAcApDocManager@@PAVAcApPlotReactor@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acRemovePlotReactor(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acStdSupportManager@@YAPAVCAcStdSupport@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acStdSupportManager();

        [DllImport("acad.exe", EntryPoint = "?acadUnhandledExceptionFilter@@YGJPAU_EXCEPTION_POINTERS@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acadUnhandledExceptionFilter(int arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "acapLayoutManager", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acapLayoutManager();

        [DllImport("acad.exe", EntryPoint = "?acapLongTransactionManagerPtr@@YAPAVAcApLongTransactionManager@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acapLongTransactionManagerPtr();

        [DllImport("acad.exe", EntryPoint = "?acbgAllowBGPlot@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acbgAllowBGPlot();

        [DllImport("acad.exe", EntryPoint = "?accipUtil@@YAPAVAcCipUtil@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr accipUtil();

        [DllImport("acad.exe", EntryPoint = "?acdbAngToSHighPrecision@@YAHNHPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acdbAngToSHighPrecision(double arg0, int arg1, string arg2);

        [DllImport("acad.exe", EntryPoint = "?acdbApplyCurDwgLayerTableChanges@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acdbApplyCurDwgLayerTableChanges();

        [DllImport("acad.exe", EntryPoint = "?acdbCanonicalToSystemRange@@YA_NHABVAcString@@AAV1@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acdbCanonicalToSystemRange(int arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acdbHatchHitTest@@YA_NPAVAcDbEntity@@ABVAcGePoint3d@@ABVAcGeVector3d@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acdbHatchHitTest(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acdbIsInHatchContourCalculation@@YA_NPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acdbIsInHatchContourCalculation(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acdbRToSHighPrecision@@YAHNHPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acdbRToSHighPrecision(double arg0, int arg1, string arg2);

        [DllImport("acad.exe", EntryPoint = "?acdbSystemRangeToCanonical@@YA_NHABVAcString@@AAV1@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acdbSystemRangeToCanonical(int arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acedAcadInQuiescentState@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedAcadInQuiescentState();

        [DllImport("acad.exe", EntryPoint = "?acedActiveViewportId@@YA?AVAcDbObjectId@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedActiveViewportId(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedAddDefaultContextMenu@@YAHPAVAcEdUIContext@@PBXPB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedAddDefaultContextMenu(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedAddDropTarget@@YAHPAVCOleDropTarget@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedAddDropTarget(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedAddMenuReactor@@YAHPAVAcApMenuReactor@@PB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedAddMenuReactor(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedAddObjectContextMenu@@YAHPBVAcRxClass@@PAVAcEdUIContext@@PBX@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedAddObjectContextMenu(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedAppendSearchPath@@YAXAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedAppendSearchPath(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?acedBeginUndoGroup@@YA_NPAVAcApDocument@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedBeginUndoGroup(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedCanDisplayDynInputContextMenu@@YA_NAAW4DiCoordModifier@AcDynInput@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCanDisplayDynInputContextMenu(IntPtr arg0, int arg1, sbyte arg2, int arg3, int arg4, int arg5, int arg6, int arg7, float arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14, int arg15);

        [DllImport("acad.exe", EntryPoint = "?acedCaptureLayoutThumbnails@@YA_NPAVAcDbDatabase@@_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCaptureLayoutThumbnails(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedCaptureModelView@@YA_NPAVAcDbDatabase@@PB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCaptureModelView(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedCaptureSheetAndSheetViews@@YA_NPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCaptureSheetAndSheetViews(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedCaptureThumbnails@@YA_NPAVAcDbDatabase@@_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCaptureThumbnails(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedCheckStringAgainstKwords@@YAHPB_W0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCheckStringAgainstKwords(string arg0, int arg1);

        [DllImport("acad.exe", EntryPoint = "?acedCleanupDeletedObjectsInAcad@@YA?AW4ErrorStatus@Acad@@_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCleanupDeletedObjectsInAcad(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedColorSettingsChanged@@YAHHHH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedColorSettingsChanged(int arg0, int arg1, int arg2);

        [DllImport("acad.exe", EntryPoint = "?acedCommandActive@@YAKXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCommandActive();

        [DllImport("acad.exe", EntryPoint = "?acedCommandCancelled@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCommandCancelled();

        [DllImport("acad.exe", EntryPoint = "?acedComputePickParams@@YA_NABVAcGePoint3d@@AAV1@AAVAcGeVector3d@@AAN@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedComputePickParams(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedConvertEntityToHatch@@YA?AW4ErrorStatus@Acad@@PAVAcDbHatch@@AAPAVAcDbEntity@@_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedConvertEntityToHatch(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedCoordFromPixelToWorld@@YAHHVCPoint@@QAN@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCoordFromPixelToWorld(int arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acedCoordFromPixelToWorld@@YAXABVCPoint@@QAN@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCoordFromPixelToWorld_1(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedCoordFromWorldToPixel@@YAHHQBNAAVCPoint@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCoordFromWorldToPixel(int arg0, IntPtr arg1, IntPtr arg2);

        [DllImport("acad.exe", EntryPoint = "?acedCreateDataSourceFromSelection@@YAPAVCOleDataSource@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCreateDataSourceFromSelection();

        [DllImport("acad.exe", EntryPoint = "?acedCreateEnhancedViewportOnDrop@@YA?AW4ErrorStatus@Acad@@PB_W0VAcGePoint2d@@NVAcDbObjectId@@AAV4@3@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCreateEnhancedViewportOnDrop(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedCreateEnhancedViewportOnDropDWG@@YA?AW4ErrorStatus@Acad@@PB_WPAVAcDbExtents@@VAcGePoint2d@@NVAcDbObjectId@@AAV5@4@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCreateEnhancedViewportOnDropDWG(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedCreateInternetShortcut@@YAHPB_W0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCreateInternetShortcut(string arg0, int arg1);

        [DllImport("acad.exe", EntryPoint = "?acedCreateShortcut@@YAHPAXPB_W11@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCreateShortcut(IntPtr arg0, string arg1, int arg2, int arg3);

        [DllImport("acad.exe", EntryPoint = "?acedCreateViewportByView@@YA?AW4ErrorStatus@Acad@@PAVAcDbDatabase@@ABVAcDbObjectId@@ABVAcGePoint2d@@NAAV4@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedCreateViewportByView(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedDieselEval@@YAPA_WPB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedDieselEval(string arg0);

        [DllImport("acad.exe", EntryPoint = "?acedDimEnableUpdate@@YA_N_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedDimEnableUpdate(bool arg0);

        [DllImport("acad.exe", EntryPoint = "?acedDimIsUpdateEnabled@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedDimIsUpdateEnabled();

        [DllImport("acad.exe", EntryPoint = "?acedDisableDefaultARXExceptionHandler@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedDisableDefaultARXExceptionHandler(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acedDisableSaveAsFormats@@YA?AW4ErrorStatus@Acad@@_N0000@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedDisableSaveAsFormats(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedDisableSpacePicking@@YA?AW4ErrorStatus@Acad@@PAVAcApDocument@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedDisableSpacePicking(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedDisableUsrbrk@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedDisableUsrbrk();

        [DllImport("acad.exe", EntryPoint = "?acedDoubleClickAction@@YA_NPAVAcDbEntity@@AAVAcGePoint3d@@_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedDoubleClickAction(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedDowngradeDocOpen@@YA?AW4ErrorStatus@Acad@@_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedDowngradeDocOpen(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedDrawOrderInherit@@YA?AW4ErrorStatus@Acad@@VAcDbObjectId@@AAV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@W4AcEdDrawOrderCmdType@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedDrawOrderInherit(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedDrawingStatusBarsVisible@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedDrawingStatusBarsVisible();

        [DllImport("acad.exe", EntryPoint = "?acedDrgInsrt@@YGHPAVAcDbObjectId@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acedDrgInsrt(int arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "acedDrgpreset", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedDrgpreset();

        [DllImport("acad.exe", EntryPoint = "?acedDynDimTextData@@YA_NPBVAcDbDimension@@VAcGePoint3d@@PB_W2_NAAUtagRECT@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedDynDimTextData(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedEditDimstyleInteractive@@YAHPAVAcDbDatabase@@PAVAcDbDimStyleTableRecord@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedEditDimstyleInteractive(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedEditMTextInteractive@@YAHPAVAcDbMText@@_N1@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedEditMTextInteractive(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedEditToleranceInteractive@@YAXPAVAcDbFcf@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedEditToleranceInteractive(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedEnableOleMessageFilter@@YAHH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedEnableOleMessageFilter(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acedEnableSpacePicking@@YA?AW4ErrorStatus@Acad@@PAVAcApDocument@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedEnableSpacePicking(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedEnableStatusBarItem@@YAXH_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedEnableStatusBarItem(int arg0, bool arg1);

        [DllImport("acad.exe", EntryPoint = "?acedEnableUsrbrk@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedEnableUsrbrk();

        [DllImport("acad.exe", EntryPoint = "?acedEndOverrideDropTarget@@YAHPAVCOleDropTarget@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedEndOverrideDropTarget(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedEndUndoGroup@@YA_NPAVAcApDocument@@_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedEndUndoGroup(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedEntityFromIData@@YAHPAXPAHAAVAcDbObjectId@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedEntityFromIData(IntPtr arg0, IntPtr arg1, IntPtr arg2);

        [DllImport("acad.exe", EntryPoint = "?acedEvaluateDiesel@@YAHPB_WPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedEvaluateDiesel(string arg0, string arg1);

        [DllImport("acad.exe", EntryPoint = "?acedEvaluateLisp@@YAHPB_WAAPAUresbuf@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedEvaluateLisp(string arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acedFindFileMulti@@YAHPBUresbuf@@PAPAU1@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedFindFileMulti(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedFreeScaleList@@YAXPAUAcScaleEntry@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedFreeScaleList(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedGeneratePreviewBitmaps@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGeneratePreviewBitmaps();

        [DllImport("acad.exe", EntryPoint = "?acedGenerateThumbnailBitmap@@YAPAUtagABITMAPINFO@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGenerateThumbnailBitmap();

        [DllImport("acad.exe", EntryPoint = "?acedGetAcadDoc@@YAPAVCDocument@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetAcadDoc();

        [DllImport("acad.exe", EntryPoint = "?acedGetAcadDockCmdLine@@YAPAVCWnd@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetAcadDockCmdLine();

        [DllImport("acad.exe", EntryPoint = "?acedGetAcadDwgView@@YAPAVCView@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetAcadDwgView();

        [DllImport("acad.exe", EntryPoint = "?acedGetAcadFrame@@YAPAVCMDIFrameWnd@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetAcadFrame();

        [DllImport("acad.exe", EntryPoint = "?acedGetAcadResourceInstance@@YAPAUHINSTANCE__@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetAcadResourceInstance();

        [DllImport("acad.exe", EntryPoint = "?acedGetAcadTextCmdLine@@YAPAVCWnd@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetAcadTextCmdLine();

        [DllImport("acad.exe", EntryPoint = "?acedGetAcadWinApp@@YAPAVCWinApp@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetAcadWinApp();

        [DllImport("acad.exe", EntryPoint = "?acedGetActiveIPFrame@@YAPAVCWnd@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetActiveIPFrame();

        [DllImport("acad.exe", EntryPoint = "?acedGetApplicationStatusBar@@YAPAVAcApStatusBar@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetApplicationStatusBar();

        [DllImport("acad.exe", EntryPoint = "?acedGetApplyFilterToToolbarFlag@@YG_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acedGetApplyFilterToToolbarFlag(bool arg0);

        [DllImport("acad.exe", EntryPoint = "?acedGetBlockEditMode@@YAIXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetBlockEditMode();

        [DllImport("acad.exe", EntryPoint = "?acedGetCMBaseAlias@@YAPB_WW4AcadContextMenuMode@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetCMBaseAlias(char arg0, int arg1, IntPtr arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, float arg11, int arg12, int arg13, int arg14, float arg15, int arg16, int arg17, int arg18);

        [DllImport("acad.exe", EntryPoint = "?acedGetChildFrameSettings@@YAHPAUtagChildFrmSettings@@PAVCMDIChildWnd@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetChildFrameSettings(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedGetCommandAtLevelForDocument@@YA?AW4ErrorStatus@Acad@@PAVAcApDocument@@AAPA_WH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetCommandAtLevelForDocument(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedGetCommandCapabilityBits@@YAKXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetCommandCapabilityBits();

        [DllImport("acad.exe", EntryPoint = "?acedGetCommandNestedDepth@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetCommandNestedDepth();

        [DllImport("acad.exe", EntryPoint = "?acedGetCurDocSaveFilename@@YAPB_WXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetCurDocSaveFilename();

        [DllImport("acad.exe", EntryPoint = "?acedGetCurDwgXrefGraph@@YA?AW4ErrorStatus@Acad@@AAVAcDbXrefGraph@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetCurDwgXrefGraph(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedGetCurDynInputManager@@YAPAVCAcDynInput@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetCurDynInputManager();

        [DllImport("acad.exe", EntryPoint = "?acedGetCurViewportObjectId@@YA?AVAcDbObjectId@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetCurViewportObjectId(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedGetCurrentCmdStr@@YAPB_WXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetCurrentCmdStr();

        [DllImport("acad.exe", EntryPoint = "?acedGetCurrentColors@@YAHPAUAcColorSettings@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetCurrentColors(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedGetCurrentColorsEx@@YAHPAUAcColorSettings@@PAUAcColorSettingsEx@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetCurrentColorsEx(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedGetCurrentSelectionSet@@YA?AW4ErrorStatus@Acad@@AAV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetCurrentSelectionSet(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedGetCurrentUCS@@YA?AW4ErrorStatus@Acad@@AAVAcGeMatrix3d@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetCurrentUCS(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedGetCurrentView@@YA?AW4ErrorStatus@Acad@@AAVAcDbViewTableRecord@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetCurrentView(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedGetDbModIgnoreMask@@YAFXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetDbModIgnoreMask();

        [DllImport("acad.exe", EntryPoint = "?acedGetEditorImp@@YAPAVAcEditorImp@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetEditorImp();

        [DllImport("acad.exe", EntryPoint = "?acedGetFileNavDialogEx@@YAHPB_W0000HAAHPAPAUresbuf@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetFileNavDialogEx(string arg0, int arg1, int arg2, int arg3, int arg4, int arg5, IntPtr arg6, IntPtr arg7);

        [DllImport("acad.exe", EntryPoint = "?acedGetFullInput@@YAHAAPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetFullInput(string arg0);

        [DllImport("acad.exe", EntryPoint = "?acedGetFullKword@@YAHPB_WAAPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetFullKword(string arg0, string arg1);

        [DllImport("acad.exe", EntryPoint = "?acedGetFullKword@@YAHPB_WAAPA_W_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetFullKword_1(string arg0, string arg1, bool arg2);

        [DllImport("acad.exe", EntryPoint = "?acedGetFullString@@YAHHPB_WAAPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetFullString(int arg0, string arg1, string arg2);

        [DllImport("acad.exe", EntryPoint = "?acedGetFullSubentPathsForCurrentSelection@@YA?AW4ErrorStatus@Acad@@ABVAcDbObjectId@@AAV?$AcArray@VAcDbFullSubentPath@@V?$AcArrayObjectCopyReallocator@VAcDbFullSubentPath@@@@@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetFullSubentPathsForCurrentSelection(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedGetHideWarningDialogs@@YA_NI@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetHideWarningDialogs(uint arg0);

        [DllImport("acad.exe", EntryPoint = "?acedGetHotGrips@@YA?AW4ErrorStatus@Acad@@PAVAcDbEntity@@AAV?$AcArray@HV?$AcArrayMemCopyReallocator@H@@@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetHotGrips(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedGetIUnknownForCurrentCommand@@YAHAAPAUIUnknown@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetIUnknownForCurrentCommand(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedGetInfoCenterControl@@YAPAVCWnd@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetInfoCenterControl();

        [DllImport("acad.exe", EntryPoint = "?acedGetInitialWorkspaceName@@YAHAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetInitialWorkspaceName(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "acedGetIntInRange", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetIntInRange();

        [DllImport("acad.exe", EntryPoint = "?acedGetInvertLayerFilterFlag@@YG_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acedGetInvertLayerFilterFlag(bool arg0);

        [DllImport("acad.exe", EntryPoint = "?acedGetIsStartupScript@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetIsStartupScript();

        [DllImport("acad.exe", EntryPoint = "?acedGetLeavesByDefaultMode@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetLeavesByDefaultMode();

        [DllImport("acad.exe", EntryPoint = "?acedGetLocalCommandName@@YAPB_WPB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetLocalCommandName(string arg0);

        [DllImport("acad.exe", EntryPoint = "?acedGetMenu@@YAPAUHMENU__@@PB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetMenu();

        [DllImport("acad.exe", EntryPoint = "?acedGetMenuItemIDFromTag@@YAIPB_W0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetMenuItemIDFromTag(string arg0, int arg1);

        [DllImport("acad.exe", EntryPoint = "?acedGetModuleFileName@@YA?AW4ErrorStatus@Acad@@PB_WPA_WK@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetModuleFileName(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedGetMoreHideWarningDialogs@@YA_NI@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetMoreHideWarningDialogs(uint arg0);

        [DllImport("acad.exe", EntryPoint = "?acedGetPanelSetData@@YAPAVAcPanelSetData@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetPanelSetData();

        [DllImport("acad.exe", EntryPoint = "?acedGetPanelSetResourceHandle@@YAPAUHINSTANCE__@@PB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetPanelSetResourceHandle();

        [DllImport("acad.exe", EntryPoint = "?acedGetRGB@@YAKH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetRGB(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acedGetRegistryCompany@@YAABV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetRegistryCompany(char arg0, IntPtr arg1, char arg2, IntPtr arg3, char arg4);

        [DllImport("acad.exe", EntryPoint = "?acedGetRunningPlotInfo@@YAPAVAcPlPlotInfo@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetRunningPlotInfo();

        [DllImport("acad.exe", EntryPoint = "?acedGetSelectionServiceVportId@@YA?AW4ErrorStatus@Acad@@AAVAcEdSelectionSetService@@HAAVAcDbObjectId@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetSelectionServiceVportId(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedGetSetvarVarname@@YAPB_WXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetSetvarVarname();

        [DllImport("acad.exe", EntryPoint = "?acedGetSupplementalCursorBitmap@@YAPAVCBitmap@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetSupplementalCursorBitmap();

        [DllImport("acad.exe", EntryPoint = "?acedGetSupportedSaveFormats@@YA?AW4ErrorStatus@Acad@@_N000AAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@1AAV?$AcArray@W4SaveFormat@AcApDocument@@V?$AcArrayMemCopyReallocator@W4SaveFormat@AcApDocument@@@@@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetSupportedSaveFormats(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedGetSystemColors@@YAHPAUAcColorSettings@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetSystemColors(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedGetSystemColorsEx@@YAHPAUAcColorSettings@@PAUAcColorSettingsEx@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetSystemColorsEx(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedGetSysvarList@@YAHPB_WAAVCStringArray@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetSysvarList(string arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acedGetToolPanelData@@YAPAVAcToolPanelData@@PB_W0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetToolPanelData();

        [DllImport("acad.exe", EntryPoint = "?acedGetTopmostFaceInSelectionSet@@YA_NAAY01$$CBJABVAcGePoint3d@@AAVAcDbObjectId@@AAVAcDbSubentId@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetTopmostFaceInSelectionSet(IntPtr arg0, int arg1, int arg2, int arg3, int arg4, int arg5, IntPtr arg6);

        [DllImport("acad.exe", EntryPoint = "?acedGetUnitsValueString@@YAPBVAcString@@W4UnitsValue@AcDb@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetUnitsValueString();

        [DllImport("acad.exe", EntryPoint = "?acedGetUseCurrentFilterFlag@@YG_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acedGetUseCurrentFilterFlag(bool arg0);

        [DllImport("acad.exe", EntryPoint = "?acedGetUserFavoritesDir@@YAHPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetUserFavoritesDir(string arg0);

        [DllImport("acad.exe", EntryPoint = "?acedGetVpOvrBackgColor@@YG?AVAcCmColor@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acedGetVpOvrBackgColor(IntPtr arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acedGetWhoHasInfo@@YA_NPB_WAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@11@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetWhoHasInfo(string arg0, IntPtr arg1, char arg2, IntPtr arg3, char arg4, IntPtr arg5, char arg6);

        [DllImport("acad.exe", EntryPoint = "?acedGetWhoHasInfo@@YG_NPB_WPAVAcString@@11@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acedGetWhoHasInfo_1(bool arg0, string arg1, IntPtr arg2);

        [DllImport("acad.exe", EntryPoint = "?acedGetWinNum@@YAHHH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetWinNum(int arg0, int arg1);

        [DllImport("acad.exe", EntryPoint = "?acedGetWorkspaceInfo@@YAHAAV?$AcArray@PAUAcNFWWSInfo@@V?$AcArrayMemCopyReallocator@PAUAcNFWWSInfo@@@@@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetWorkspaceInfo(IntPtr arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acedGetWorkspacePanelData@@YA_NAAV?$vector@VAcWsPanelData@@V?$allocator@VAcWsPanelData@@@std@@@std@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedGetWorkspacePanelData(IntPtr arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acedHasOverrideDropTarget@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedHasOverrideDropTarget();

        [DllImport("acad.exe", EntryPoint = "?acedHatchPalletteDialog@@YA_NPB_W_NAAPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedHatchPalletteDialog(string arg0, bool arg1, string arg2);

        [DllImport("acad.exe", EntryPoint = "?acedHideSegment@@YAXPAVSegment@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedHideSegment(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedHideViewport@@YAXJ@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedHideViewport(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acedHyperlinkBack@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedHyperlinkBack();

        [DllImport("acad.exe", EntryPoint = "?acedHyperlinkExecute@@YAXPB_W00@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedHyperlinkExecute(string arg0, int arg1, int arg2);

        [DllImport("acad.exe", EntryPoint = "?acedHyperlinkForward@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedHyperlinkForward();

        [DllImport("acad.exe", EntryPoint = "?acedHyperlinkStop@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedHyperlinkStop();

        [DllImport("acad.exe", EntryPoint = "?acedInitDialog@@YAHH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedInitDialog(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acedInsertWhipOwnerDraw@@YAPAVSegment@@JPAVOwnerDraw@Whip@@PAJ1_N2@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedInsertWhipOwnerDraw();

        [DllImport("acad.exe", EntryPoint = "?acedInsertXrefDrawingOnDrop@@YA?AW4ErrorStatus@Acad@@PAVAcDbDatabase@@PB_WAAVAcDbObjectId@@2@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedInsertXrefDrawingOnDrop(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedInvokeAnnoPropModifiedWRNMsg@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedInvokeAnnoPropModifiedWRNMsg();

        [DllImport("acad.exe", EntryPoint = "?acedInvokeAnnoScaleConfirmDlg@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedInvokeAnnoScaleConfirmDlg();

        [DllImport("acad.exe", EntryPoint = "?acedInvokeAnnoSelectScaleDlg@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedInvokeAnnoSelectScaleDlg();

        [DllImport("acad.exe", EntryPoint = "?acedInvokeMTextBackgroundMaskDialog@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedInvokeMTextBackgroundMaskDialog();

        [DllImport("acad.exe", EntryPoint = "?acedInvokeMTextColumnsDialog@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedInvokeMTextColumnsDialog();

        [DllImport("acad.exe", EntryPoint = "?acedInvokeTableStyleDialog@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedInvokeTableStyleDialog();

        [DllImport("acad.exe", EntryPoint = "?acedIs3DOrbitEnabled@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedIs3DOrbitEnabled();

        [DllImport("acad.exe", EntryPoint = "?acedIsDiesel@@YAHPB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedIsDiesel(string arg0);

        [DllImport("acad.exe", EntryPoint = "?acedIsDragging@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedIsDragging();

        [DllImport("acad.exe", EntryPoint = "?acedIsInBackgroundMode@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedIsInBackgroundMode();

        [DllImport("acad.exe", EntryPoint = "?acedIsInPlaceServer@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedIsInPlaceServer();

        [DllImport("acad.exe", EntryPoint = "?acedIsInputPending@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedIsInputPending();

        [DllImport("acad.exe", EntryPoint = "?acedIsLiveSectioning@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedIsLiveSectioning();

        [DllImport("acad.exe", EntryPoint = "acedIsMenuGroupLoaded", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedIsMenuGroupLoaded();

        [DllImport("acad.exe", EntryPoint = "?acedIsNamedDrawing@@YAHPAVAcApDocument@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedIsNamedDrawing(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedIsSaveFormatDisabled@@YA_NW4SaveFormat@AcApDocument@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedIsSaveFormatDisabled(char arg0, int arg1, IntPtr arg2, int arg3, short arg4, int arg5, int arg6, int arg7, int arg8, int arg9);

        [DllImport("acad.exe", EntryPoint = "?acedIsScrollingEnabled@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedIsScrollingEnabled();

        [DllImport("acad.exe", EntryPoint = "?acedIsTableStyleStandardName@@YA_NPB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedIsTableStyleStandardName(string arg0);

        [DllImport("acad.exe", EntryPoint = "?acedIsUpdateDisplayPaused@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedIsUpdateDisplayPaused();

        [DllImport("acad.exe", EntryPoint = "?acedIsUsrbrkDisabled@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedIsUsrbrkDisabled();

        [DllImport("acad.exe", EntryPoint = "?acedKillXrefBubbleMessage@@YA?AW4ErrorStatus@Acad@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedKillXrefBubbleMessage(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedLayerDisplayVisibility@@YAXVAcDbObjectId@@_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedLayerDisplayVisibility(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedLayerStateManagerDialog@@YGXPAUHWND__@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acedLayerStateManagerDialog();

        [DllImport("acad.exe", EntryPoint = "?acedLayerStateSaveDialog@@YGXPAUHWND__@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acedLayerStateSaveDialog();

        [DllImport("acad.exe", EntryPoint = "?acedLineWeightDialog@@YA_NW4LineWeight@AcDb@@_NAAW412@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedLineWeightDialog(char arg0, int arg1, int arg2, int arg3, int arg4, int arg5, char arg6, int arg7, int arg8, int arg9, int arg10, int arg11);

        [DllImport("acad.exe", EntryPoint = "?acedLinetypeDialog@@YA_NVAcDbObjectId@@_NAAPA_WAAV1@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedLinetypeDialog(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedLoadScaleList@@YAXAAPAUAcScaleEntry@@AAHPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedLoadScaleList(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedLoadScaleUnitStrings@@YAXQAPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedLoadScaleUnitStrings(string arg0);

        [DllImport("acad.exe", EntryPoint = "?acedLockGui@@YAX_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedLockGui(bool arg0);

        [DllImport("acad.exe", EntryPoint = "?acedLogicalViewSize@@YAXAAJ0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedLogicalViewSize(IntPtr arg0, int arg1);

        [DllImport("acad.exe", EntryPoint = "?acedManualInputProvided@@YAHPAVAcApDocument@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedManualInputProvided(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedMenuInputProvided@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedMenuInputProvided();

        [DllImport("acad.exe", EntryPoint = "?acedMspace@@YA?AW4ErrorStatus@Acad@@PAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedMspace(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedMspace@@YA?AW4ErrorStatus@Acad@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedMspace_1(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedNEntSelPEx@@YAHPB_WQAJQANHQAY03NPAPAUresbuf@@IPAH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedNEntSelPEx(string arg0, IntPtr arg1, IntPtr arg2, int arg3, IntPtr arg4, int arg5, int arg6, double arg7, IntPtr arg8);

        [DllImport("acad.exe", EntryPoint = "?acedNewSDIDocument@@YA?AW4ErrorStatus@Acad@@PB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedNewSDIDocument(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedNoDialogMode@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedNoDialogMode();

        [DllImport("acad.exe", EntryPoint = "?acedOleDoVerb@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedOleDoVerb(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acedOleSelected@@YA_NAAVAcDbObjectId@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedOleSelected(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedOpenDocWindowsMinimized@@YAX_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedOpenDocWindowsMinimized(bool arg0);

        [DllImport("acad.exe", EntryPoint = "?acedOsnapMarkerColor@@YAKXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedOsnapMarkerColor();

        [DllImport("acad.exe", EntryPoint = "?acedPasteItem@@YA_N_NPAUIDataObject@@PAUtagPOINT@@G0KK@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedPasteItem(bool arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acedPlotstyleDialog@@YA_NPB_W_NAAPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedPlotstyleDialog(string arg0, bool arg1, string arg2);

        [DllImport("acad.exe", EntryPoint = "?acedPostCommand@@YAHPB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedPostCommand(string arg0);

        [DllImport("acad.exe", EntryPoint = "?acedPromptForBlock@@YA?AW4ErrorStatus@Acad@@AAVAcDbObjectId@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedPromptForBlock(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedPspace@@YA?AW4ErrorStatus@Acad@@PAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedPspace(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedPspace@@YA?AW4ErrorStatus@Acad@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedPspace_1(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedPutPropChangeFlags@@YA?AW4ErrorStatus@Acad@@PBVAcRxClass@@JJ@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedPutPropChangeFlags(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedRefreshOsnapCursor@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRefreshOsnapCursor();

        [DllImport("acad.exe", EntryPoint = "?acedRegenLayers@@YGXABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acedRegenLayers();

        [DllImport("acad.exe", EntryPoint = "?acedRegenLayers@@YGXABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@HH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acedRegenLayers_1();

        [DllImport("acad.exe", EntryPoint = "?acedRegenLayersForOneVP@@YGXABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@ABVAcDbObjectId@@H_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acedRegenLayersForOneVP();

        [DllImport("acad.exe", EntryPoint = "?acedRegenPending@@YGHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acedRegenPending(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acedRegisterCustomDropTarget@@YAHPAUIDropTarget@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRegisterCustomDropTarget(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedRegisterExtendedTab@@YAHPB_W0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRegisterExtendedTab(string arg0, int arg1);

        [DllImport("acad.exe", EntryPoint = "?acedRegisterFilterWinMsg@@YAHQ6AHPAUtagMSG@@@Z@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRegisterFilterWinMsg(IntPtr arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acedRegisterForcedSubentityClass@@YAXPBVAcRxClass@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRegisterForcedSubentityClass(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedRegisterGripService@@YA?AW4ErrorStatus@Acad@@PAVAcApDocument@@PAVAcEdAppGripService@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRegisterGripService(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedRegisterOnIdleWinMsg@@YAHQ6AXXZ@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRegisterOnIdleWinMsg(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedRegisterStatusBarMenuItem@@YAHPAVAcStatusBarMenuItem@@W4AcStatusBarType@1@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRegisterStatusBarMenuItem(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedRegisterWatchWinMsg@@YAHQ6AXPBUtagMSG@@@Z@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRegisterWatchWinMsg(IntPtr arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acedReleaseThumbnailBitmap@@YAXPAUtagABITMAPINFO@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedReleaseThumbnailBitmap(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedRemoveDefaultContextMenu@@YAHPAVAcEdUIContext@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRemoveDefaultContextMenu(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedRemoveDropTarget@@YAHPAVCOleDropTarget@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRemoveDropTarget(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedRemoveFilterWinMsg@@YAHQ6AHPAUtagMSG@@@Z@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRemoveFilterWinMsg(IntPtr arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acedRemoveGripService@@YA?AW4ErrorStatus@Acad@@PAVAcApDocument@@PAVAcEdAppGripService@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRemoveGripService(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedRemoveMenuReactor@@YAHPAVAcApMenuReactor@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRemoveMenuReactor(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedRemoveObjectContextMenu@@YAHPBVAcRxClass@@PAVAcEdUIContext@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRemoveObjectContextMenu(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedRemoveOnIdleWinMsg@@YAHQ6AXXZ@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRemoveOnIdleWinMsg(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedRemoveSearchPath@@YAXAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRemoveSearchPath(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?acedRemoveSegment@@YAXPAVSegment@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRemoveSegment(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedRemoveWatchWinMsg@@YAHQ6AXPBUtagMSG@@@Z@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRemoveWatchWinMsg(IntPtr arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acedRenameBlockElementEntity@@YA?AW4ErrorStatus@Acad@@ABVAcDbObjectId@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRenameBlockElementEntity(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedReplaceCommandLineString@@YAHPB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedReplaceCommandLineString(string arg0);

        [DllImport("acad.exe", EntryPoint = "?acedResetScaleList@@YAX_N0PAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedResetScaleList(bool arg0, int arg1, IntPtr arg2);

        [DllImport("acad.exe", EntryPoint = "?acedResolveInternetShortcut@@YAHPB_WPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedResolveInternetShortcut(string arg0, string arg1);

        [DllImport("acad.exe", EntryPoint = "?acedResolveShortcut@@YAHPAXPB_WPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedResolveShortcut(IntPtr arg0, string arg1, string arg2);

        [DllImport("acad.exe", EntryPoint = "?acedRestoreCurrentView@@YA?AW4ErrorStatus@Acad@@ABVAcDbObjectId@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRestoreCurrentView(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedRestoreFromCleanScreen@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRestoreFromCleanScreen();

        [DllImport("acad.exe", EntryPoint = "?acedRestorePreviousUCS@@YA?AW4ErrorStatus@Acad@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRestorePreviousUCS(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedRestoreStatusBar@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRestoreStatusBar();

        [DllImport("acad.exe", EntryPoint = "?acedRestoreWindow@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRestoreWindow();

        [DllImport("acad.exe", EntryPoint = "?acedRevokeCustomDropTarget@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedRevokeCustomDropTarget();

        [DllImport("acad.exe", EntryPoint = "acedSSNameXEx", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSNameXEx();

        [DllImport("acad.exe", EntryPoint = "acedSSSetSubentTypes", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSSetSubentTypes();

        [DllImport("acad.exe", EntryPoint = "acedSSSubentAdd", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSSubentAdd();

        [DllImport("acad.exe", EntryPoint = "acedSSSubentDel", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSSubentDel();

        [DllImport("acad.exe", EntryPoint = "acedSSSubentLength", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSSubentLength();

        [DllImport("acad.exe", EntryPoint = "acedSSSubentMemb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSSubentMemb();

        [DllImport("acad.exe", EntryPoint = "acedSSSubentName", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSSubentName();

        [DllImport("acad.exe", EntryPoint = "acedSSSubentNameX", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSSubentNameX();

        [DllImport("acad.exe", EntryPoint = "?acedSaveCurrentDrawing@@YA?AW4ErrorStatus@Acad@@PB_WW4AcDbDwgVersion@AcDb@@W4MaintenanceReleaseVersion@4@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSaveCurrentDrawing(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedSaveScaleList@@YAXPAUAcScaleEntry@@HPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSaveScaleList(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSendCommandToExecute@@YA?AW4ErrorStatus@Acad@@PAVAcApDocument@@PB_W_N22@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSendCommandToExecute(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedSendDocumentBecomingNonCurrent@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSendDocumentBecomingNonCurrent();

        [DllImport("acad.exe", EntryPoint = "?acedSendMenuStringToExecute@@YA?AW4ErrorStatus@Acad@@PAVAcApDocument@@PB_W_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSendMenuStringToExecute(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedSendModelessOperationEnded@@YAXPB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSendModelessOperationEnded(string arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSendModelessOperationStart@@YAXPB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSendModelessOperationStart(string arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSendViewChanged@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSendViewChanged();

        [DllImport("acad.exe", EntryPoint = "?acedSetAlwaysPickOnSolved3dSolidBody@@YAX_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetAlwaysPickOnSolved3dSolidBody(bool arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetApplyFilterToToolbarFlag@@YGX_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acedSetApplyFilterToToolbarFlag();

        [DllImport("acad.exe", EntryPoint = "?acedSetBlockEditMode@@YAXI@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetBlockEditMode(uint arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetCMBaseAlias@@YA_NPB_WW4AcadContextMenuMode@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetCMBaseAlias(string arg0, char arg1, int arg2, IntPtr arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, float arg12, int arg13, int arg14, int arg15, float arg16, int arg17, int arg18, int arg19);

        [DllImport("acad.exe", EntryPoint = "?acedSetChildFrameSettings@@YAHPAUtagChildFrmSettings@@PAVCMDIChildWnd@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetChildFrameSettings(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "acedSetColorDialog", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSetColorDialog();

        [DllImport("acad.exe", EntryPoint = "acedSetColorDialogTrueColor", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSetColorDialogTrueColor();

        [DllImport("acad.exe", EntryPoint = "?acedSetColorDialogTrueColorWithCallback@@YAHAAVAcCmColor@@HABV1@W4DialogTabs@AcCm@@P6GXPAX1@Z3@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetColorDialogTrueColorWithCallback(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetColorPrompt@@YAHPA_WAAVAcCmColor@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetColorPrompt(string arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acedSetCurrentColors@@YAHPAUAcColorSettings@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetCurrentColors(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetCurrentColorsEx@@YAHPAUAcColorSettings@@PAUAcColorSettingsEx@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetCurrentColorsEx(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetCurrentUCS@@YA?AW4ErrorStatus@Acad@@ABVAcGeMatrix3d@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetCurrentUCS(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedSetCurrentVPort@@YA?AW4ErrorStatus@Acad@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetCurrentVPort(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedSetCurrentVPort@@YA?AW4ErrorStatus@Acad@@PBVAcDbViewport@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetCurrentVPort_1(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedSetCurrentView@@YA?AW4ErrorStatus@Acad@@PAVAcDbViewTableRecord@@PAVAcDbViewport@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetCurrentView(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedSetDbModIgnoreMask@@YAXF@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetDbModIgnoreMask(short arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetDrawingUnnamed@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetDrawingUnnamed();

        [DllImport("acad.exe", EntryPoint = "?acedSetDynInputCoordModifier@@YA_NW4DiCoordModifier@AcDynInput@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetDynInputCoordModifier(char arg0, int arg1, sbyte arg2, int arg3, int arg4, int arg5, int arg6, int arg7, float arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14, int arg15);

        [DllImport("acad.exe", EntryPoint = "?acedSetDynInputDisplayMessage@@YA_N_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetDynInputDisplayMessage(bool arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetDynInputOverrideType@@YA_NK@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetDynInputOverrideType(uint arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetHideWarningDialogs@@YAXI_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetHideWarningDialogs(uint arg0, bool arg1);

        [DllImport("acad.exe", EntryPoint = "?acedSetHotGrips@@YA?AW4ErrorStatus@Acad@@PAVAcDbEntity@@ABV?$AcArray@HV?$AcArrayMemCopyReallocator@H@@@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetHotGrips(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedSetIUnknownForCurrentCommand@@YAHQAUIUnknown@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetIUnknownForCurrentCommand(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetInvertLayerFilterFlag@@YGX_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acedSetInvertLayerFilterFlag();

        [DllImport("acad.exe", EntryPoint = "?acedSetIsViewportMaximized@@YAX_N0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetIsViewportMaximized(bool arg0, int arg1);

        [DllImport("acad.exe", EntryPoint = "?acedSetLeavesByDefaultMode@@YAHH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetLeavesByDefaultMode(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetLoadScaleUnitStringsCallback@@YAP6A_NQAPA_W@ZP6A_N0@Z@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetLoadScaleUnitStringsCallback(string arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetMoreHideWarningDialogs@@YAXI_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetMoreHideWarningDialogs(uint arg0, bool arg1);

        [DllImport("acad.exe", EntryPoint = "?acedSetNFWWorkspaceAndTemplate@@YAHABV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetNFWWorkspaceAndTemplate(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?acedSetPreDocCloseCallback@@YAP6A?AW4AcSaveDlgAnswer@@W41@@ZP6A?AW41@0@Z@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetPreDocCloseCallback(IntPtr arg0, int arg1, IntPtr arg2, IntPtr arg3, int arg4, sbyte arg5, int arg6, int arg7, IntPtr arg8, int arg9, int arg10, int arg11, int arg12);

        [DllImport("acad.exe", EntryPoint = "?acedSetR13FenceMode@@YAHH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetR13FenceMode(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetScrollingEnabled@@YAX_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetScrollingEnabled(bool arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetStatusBarMessage@@YAHPB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetStatusBarMessage(string arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetStatusBarPaneID@@YAHPAVAcPane@@W4AcStatusBarType@AcStatusBarMenuItem@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetStatusBarPaneID(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetStatusBarProgressMeter@@YAHPB_WHH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetStatusBarProgressMeter(string arg0, int arg1, int arg2);

        [DllImport("acad.exe", EntryPoint = "?acedSetStatusBarProgressMeterPos@@YAHH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetStatusBarProgressMeterPos(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetSupplementalCursorBitmap@@YA_NPAVCBitmap@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetSupplementalCursorBitmap(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetSupplementalCursorOffset@@YAXHH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetSupplementalCursorOffset(int arg0, int arg1);

        [DllImport("acad.exe", EntryPoint = "?acedSetUseCurrentFilterFlag@@YGX_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acedSetUseCurrentFilterFlag();

        [DllImport("acad.exe", EntryPoint = "?acedSetVarFromBag@@YAHPB_WPBUresbuf@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetVarFromBag(string arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acedSetVpMaximizeReactor@@YAXPAVAcApVpMaximizeReactor@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetVpMaximizeReactor(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetVpOvrBackgColor@@YGXABVAcCmColor@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acedSetVpOvrBackgColor();

        [DllImport("acad.exe", EntryPoint = "?acedSetWindowConfig@@YA_N_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetWindowConfig(bool arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSetXrefResolvedWithUpdateStatus@@YA?AW4ErrorStatus@Acad@@PAVAcDbBlockTableRecord@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSetXrefResolvedWithUpdateStatus(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedShowCommandLine@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedShowCommandLine(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acedShowDrawingStatusBars@@YAHH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedShowDrawingStatusBars(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acedShowNFWTopic@@YA_NPB_W00_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedShowNFWTopic(string arg0, int arg1, int arg2, bool arg3);

        [DllImport("acad.exe", EntryPoint = "?acedShowSegment@@YAXPAVSegment@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedShowSegment(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedShowViewport@@YAXJ@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedShowViewport(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSkipXrefNotification@@YA?AW4ErrorStatus@Acad@@PAVAcDbDatabase@@ABV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSkipXrefNotification(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedStartOverrideDropTarget@@YAHPAVCOleDropTarget@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedStartOverrideDropTarget(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSubAwareClientInfo@@YA?AV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSubAwareClientInfo(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?acedSubAwareLaunchModule@@YAXFPAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSubAwareLaunchModule(short arg0, IntPtr arg1, char arg2, IntPtr arg3, char arg4, IntPtr arg5, char arg6);

        [DllImport("acad.exe", EntryPoint = "?acedSubSelect@@YA_NABVAcDbObjectId@@VAcGePoint3d@@1@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSubSelect(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSuppressFileMRU@@YAX_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSuppressFileMRU(bool arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSuppressOPMUpdate@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSuppressOPMUpdate();

        [DllImport("acad.exe", EntryPoint = "?acedSupressFontSubstitutionDialog@@YA_N_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSupressFontSubstitutionDialog(bool arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSwapInDatabase@@YA_NPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSwapInDatabase(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedSwitchToCleanScreen@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedSwitchToCleanScreen();

        [DllImport("acad.exe", EntryPoint = "acedSyncFileOpen", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSyncFileOpen();

        [DllImport("acad.exe", EntryPoint = "?acedTableStyleStandardName@@YAPB_WXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedTableStyleStandardName();

        [DllImport("acad.exe", EntryPoint = "?acedTextFieldModified@@YA?AW4ErrorStatus@Acad@@PAVAcDbObject@@PAVAcDbField@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedTextFieldModified(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedToolTipSettingChanged@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedToolTipSettingChanged();

        [DllImport("acad.exe", EntryPoint = "?acedTrackPopupMenu@@YAIPAUHMENU__@@IHHPAUHWND__@@PBUtagRECT@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedTrackPopupMenu(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedTranformWorldToLogical@@YAXHAAVAcGePoint3d@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedTranformWorldToLogical(int arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acedTransPixelToLocalToWorld@@YAHHPBJPAN@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedTransPixelToLocalToWorld(int arg0, IntPtr arg1, IntPtr arg2);

        [DllImport("acad.exe", EntryPoint = "?acedTransPixelToWorld@@YAHHPBJPAN@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedTransPixelToWorld(int arg0, IntPtr arg1, IntPtr arg2);

        [DllImport("acad.exe", EntryPoint = "?acedTransScreenToWorld@@YAHHPBJPAN@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedTransScreenToWorld(int arg0, IntPtr arg1, IntPtr arg2);

        [DllImport("acad.exe", EntryPoint = "?acedTransWorldToScreen@@YAHHPBNPAJ@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedTransWorldToScreen(int arg0, IntPtr arg1, IntPtr arg2);

        [DllImport("acad.exe", EntryPoint = "acedUnZapDrginsert", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedUnZapDrginsert();

        [DllImport("acad.exe", EntryPoint = "?acedUndoOrRedoInProgress@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedUndoOrRedoInProgress();

        [DllImport("acad.exe", EntryPoint = "?acedUnregisterForcedSubentityClass@@YAXPBVAcRxClass@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedUnregisterForcedSubentityClass(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedUnregisterStatusBarMenuItem@@YAHPAVAcStatusBarMenuItem@@W4AcStatusBarType@1@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedUnregisterStatusBarMenuItem(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedUpdateDisplay@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedUpdateDisplay();

        [DllImport("acad.exe", EntryPoint = "?acedUpdateDisplayPause@@YAX_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedUpdateDisplayPause(bool arg0);

        [DllImport("acad.exe", EntryPoint = "?acedUpdateScaleList@@YAXPAUAcScaleEntry@@0HPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedUpdateScaleList(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedUpdateTextFromField@@YA?AW4ErrorStatus@Acad@@PAVAcDbObject@@PAVAcDbField@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedUpdateTextFromField(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedUpgradeDocOpen@@YA?AW4ErrorStatus@Acad@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedUpgradeDocOpen(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedUseDarkDynInputToolTipText@@YA_NABK@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedUseDarkDynInputToolTipText(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "acedUsrBrkWithMessagePump", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedUsrBrkWithMessagePump();

        [DllImport("acad.exe", EntryPoint = "?acedVPLayer@@YA?AW4ErrorStatus@Acad@@ABVAcDbObjectId@@ABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@W4VpFreezeOps@AcDb@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedVPLayer(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedViewportIdFromNumber@@YA?AVAcDbObjectId@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedViewportIdFromNumber(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acedVportPixelOrigin@@YAHHPAJ@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedVportPixelOrigin(int arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acedVportTableRecords2Vports@@YA?AW4ErrorStatus@Acad@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedVportTableRecords2Vports(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedVports2VportTableRecords@@YA?AW4ErrorStatus@Acad@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedVports2VportTableRecords(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedWblockFile@@YA?AW4ErrorStatus@Acad@@PB_WPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedWblockFile(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedWriteDwg@@YA?AW4ErrorStatus@Acad@@PB_W_NW4AcDbDwgVersion@AcDb@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedWriteDwg(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedXrefAttach@@YA?AW4ErrorStatus@Acad@@PB_W0PAVAcDbObjectId@@1PBVAcGePoint3d@@PBVAcGeScale3d@@PBN_NPAVAcDbDatabase@@0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedXrefAttach(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedXrefAttach@@YA?AW4ErrorStatus@Acad@@PB_WPA_W_NPAVAcDbObjectId@@3PBVAcGePoint3d@@PBVAcGeScale3d@@PBN_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedXrefAttach_1(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedXrefAttach@@YA?AW4ErrorStatus@Acad@@VAcDbObjectId@@PB_WPA_WPAV3@3PBVAcGePoint3d@@PBVAcGeScale3d@@PBN_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedXrefAttach_2(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedXrefBind@@YA?AW4ErrorStatus@Acad@@PB_W_N1IPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedXrefBind(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedXrefBind@@YA?AW4ErrorStatus@Acad@@PB_W_N1PAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedXrefBind_1(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedXrefCreateBlockname@@YA?AW4ErrorStatus@Acad@@PB_WAAPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedXrefCreateBlockname(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedXrefDetach@@YA?AW4ErrorStatus@Acad@@PB_W_NPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedXrefDetach(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "acedXrefDrgpreset", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedXrefDrgpreset();

        [DllImport("acad.exe", EntryPoint = "?acedXrefNotifyCheckFileChanged@@YA?AW4ErrorStatus@Acad@@VAcDbObjectId@@AA_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedXrefNotifyCheckFileChanged(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedXrefOverlay@@YA?AW4ErrorStatus@Acad@@PB_W0PAVAcDbObjectId@@1PBVAcGePoint3d@@PBVAcGeScale3d@@PBN_NPAVAcDbDatabase@@0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedXrefOverlay(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedXrefReload@@YA?AW4ErrorStatus@Acad@@ABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@_NPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedXrefReload(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedXrefReload@@YA?AW4ErrorStatus@Acad@@PB_W_NPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedXrefReload_1(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedXrefResolve@@YA?AW4ErrorStatus@Acad@@PAVAcDbDatabase@@_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedXrefResolve(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedXrefRetrieveFile@@YA?AW4ErrorStatus@Acad@@AAHVAcDbObjectId@@PA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedXrefRetrieveFile(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedXrefUnload@@YA?AW4ErrorStatus@Acad@@PB_W_NPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedXrefUnload(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedXrefUserAttach@@YA?AW4ErrorStatus@Acad@@PB_W0PAVAcDbObjectId@@1PBVAcGePoint3d@@PBVAcGeScale3d@@PBN_N5H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedXrefUserAttach(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedXrefUserOverlay@@YA?AW4ErrorStatus@Acad@@PB_W0PAVAcDbObjectId@@1PBVAcGePoint3d@@PBVAcGeScale3d@@PBN_N5H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedXrefUserOverlay(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedXrefXBind@@YA?AW4ErrorStatus@Acad@@V?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@_NPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedXrefXBind(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acedZoomToolHook@@YAX_N00@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acedZoomToolHook(bool arg0, int arg1, int arg2);

        [DllImport("acad.exe", EntryPoint = "?acgsCreate2DViewLimitManager@@YAPAVAcGs2DViewLimitManager@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsCreate2DViewLimitManager();

        [DllImport("acad.exe", EntryPoint = "?acgsDestroy2DViewLimitManager@@YAXPAVAcGs2DViewLimitManager@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsDestroy2DViewLimitManager(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acgsDisplayImage@@YAHHJJHHPBXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsDisplayImage(int arg0, int arg1, int arg2, int arg3, int arg4, IntPtr arg5, int arg6);

        [DllImport("acad.exe", EntryPoint = "?acgsGetDisplayInfo@@YAHAAH000@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsGetDisplayInfo(IntPtr arg0, int arg1, int arg2, int arg3);

        [DllImport("acad.exe", EntryPoint = "?acgsGetGsManager@@YAPAVAcGsManager@@PAVCView@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsGetGsManager();

        [DllImport("acad.exe", EntryPoint = "?acgsGetGsView@@YAPAVAcGsView@@H_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsGetGsView();

        [DllImport("acad.exe", EntryPoint = "?acgsGetHighlightColor@@YAGXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsGetHighlightColor();

        [DllImport("acad.exe", EntryPoint = "?acgsGetHighlightLinePattern@@YA?AW4LinePattern@AcGs@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsGetHighlightLinePattern(IntPtr arg0, int arg1, int arg2, int arg3, int arg4, int arg5, IntPtr arg6, int arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acgsGetHighlightLineWeight@@YAGXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsGetHighlightLineWeight();

        [DllImport("acad.exe", EntryPoint = "?acgsGetLensLength@@YAHHAAN@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsGetLensLength(int arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acgsGetOrthoViewParameters@@YAHHW4OrthographicView@AcDb@@PAVAcGeVector3d@@1@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsGetOrthoViewParameters(int arg0, char arg1, int arg2, decimal arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14, IntPtr arg15, IntPtr arg16, sbyte arg17, int arg18);

        [DllImport("acad.exe", EntryPoint = "?acgsGetScreenShot@@YAPAVAcGsScreenShot@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsGetScreenShot();

        [DllImport("acad.exe", EntryPoint = "?acgsGetViewParameters@@YAHHPAVAcGsView@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsGetViewParameters(int arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acgsGetViewportInfo@@YAHHAAH000@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsGetViewportInfo(int arg0, IntPtr arg1, int arg2, int arg3, int arg4);

        [DllImport("acad.exe", EntryPoint = "?acgsGetViewportRenderFlag@@YA_NH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsGetViewportRenderFlag(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acgsRedrawShortTermGraphics@@YAXFFFF@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsRedrawShortTermGraphics(short arg0, short arg1, short arg2, short arg3);

        [DllImport("acad.exe", EntryPoint = "?acgsRemoveAnonymousGraphics@@YAHH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsRemoveAnonymousGraphics(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acgsSetCustomUpdateMethod@@YAHP6AXPAXHHHH@Z0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsSetCustomUpdateMethod(IntPtr arg0, IntPtr arg1, int arg2, int arg3, int arg4, int arg5);

        [DllImport("acad.exe", EntryPoint = "?acgsSetHighlightColor@@YAXG@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsSetHighlightColor(ushort arg0);

        [DllImport("acad.exe", EntryPoint = "?acgsSetHighlightLinePattern@@YAXW4LinePattern@AcGs@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsSetHighlightLinePattern(char arg0, int arg1, int arg2, int arg3, int arg4, int arg5, IntPtr arg6, int arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acgsSetHighlightLineWeight@@YAXG@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsSetHighlightLineWeight(ushort arg0);

        [DllImport("acad.exe", EntryPoint = "?acgsSetLensLength@@YAHHABN@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsSetLensLength(int arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acgsSetView2D@@YAHH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsSetView2D(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acgsSetViewParameters@@YAHHPBVAcGsView@@_N11@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsSetViewParameters(int arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acgsSetViewportRenderFlag@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsSetViewportRenderFlag(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acgsSuspendDynamicTessellation@@YA_N_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsSuspendDynamicTessellation(bool arg0);

        [DllImport("acad.exe", EntryPoint = "?acgsWriteViewToUndoController@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsWriteViewToUndoController(int arg0);

        [DllImport("acad.exe", EntryPoint = "?acgsiMakeGrid@@YA?AW4ErrorStatus@Acad@@PAVAcGsView@@ABVAcGePoint3d@@ABVAcGeVector3d@@22ABVAcGePoint2d@@333NNJJ_N44ABVAcCmEntityColor@@55HHHAAPAVAcGiDrawable@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsiMakeGrid(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acgsiMakeIcon@@YA?AW4ErrorStatus@Acad@@PAVAcGsView@@ABVAcGePoint3d@@ABVAcGeVector3d@@22_NAAPAVAcGiDrawable@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsiMakeIcon(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acgsiSetDeviceLineweightInfo@@YA?AW4ErrorStatus@Acad@@PAVAcGsDevice@@_N1NH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsiSetDeviceLineweightInfo(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?acgsiUpdateGridColor@@YAX_NABVAcCmEntityColor@@11HHHAAPAVAcGiDrawable@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgsiUpdateGridColor(bool arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?acgzManager@@YAPAVAcGzManager@@PAVAcApDocument@@_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgzManager();

        [DllImport("acad.exe", EntryPoint = "?acgzService@@YAPAVAcGzService@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acgzService();

        [DllImport("acad.exe", EntryPoint = "?acplCreatePlotProgressDialog@@YGPAVAcPlPlotProgressDialog@@PAUHWND__@@_NHP6AXPAV1@@Z1@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acplCreatePlotProgressDialog(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acplPlotConfigManagerPtr@@YGPAVAcPlPlotConfigManager@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acplPlotConfigManagerPtr(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acplPlotReactorMgrPtr@@YGPAVAcPlPlotReactorMgr@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acplPlotReactorMgrPtr(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?acplProcessPlotState@@YG?AW4ProcessPlotState@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort acplProcessPlotState(IntPtr arg0, IntPtr arg1, int arg2, IntPtr arg3, int arg4, int arg5, int arg6, int arg7, IntPtr arg8, int arg9, IntPtr arg10, int arg11, int arg12);

        [DllImport("acad.exe", EntryPoint = "?acquireLayerFilter@@YAPAVAcDbFilter@@HVAcDbObjectId@@PAVAcDbObject@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acquireLayerFilter();

        [DllImport("acad.exe", EntryPoint = "?acreEntityToFaces@@YA?AW4ErrorStatus@Acad@@QAJQAPA_WH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acreEntityToFaces(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "acreRegisterCallout", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acreRegisterCallout();

        [DllImport("acad.exe", EntryPoint = "?acutAreFilesSame@@YA_NPB_W0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr acutAreFilesSame(string arg0, int arg1);

        [DllImport("acad.exe", EntryPoint = "?adcxattach@@YAXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr adcxattach(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "addAppToLispList", CallingConvention = CallingConvention.Cdecl)]
        public static extern void addAppToLispList();

        [DllImport("acad.exe", EntryPoint = "?addEntHighlightOverride@@YGXJH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort addEntHighlightOverride();

        [DllImport("acad.exe", EntryPoint = "?addSSgetFilterInputContextReactor@@YA?AW4ErrorStatus@Acad@@PAVAcApDocument@@PAVAcEdSSGetFilter2@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr addSSgetFilterInputContextReactor(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?addSSgetFilterInputContextReactor@@YA?AW4ErrorStatus@Acad@@PAVAcApDocument@@PAVAcEdSSGetFilter3@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr addSSgetFilterInputContextReactor_1(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?addSSgetFilterInputContextReactor@@YA?AW4ErrorStatus@Acad@@PAVAcApDocument@@PAVAcEdSSGetFilter@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr addSSgetFilterInputContextReactor_2(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?addSSgetFilterInputContextReactor@@YA?AW4ErrorStatus@Acad@@PAVAcApDocument@@PAVAcEdSSGetSubSelectFilter@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr addSSgetFilterInputContextReactor_3(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?addUniqueToPlotStyleNameDictionary@@YAXPB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr addUniqueToPlotStyleNameDictionary(string arg0);

        [DllImport("acad.exe", EntryPoint = "?adlmGetBorrowDaysLeft@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr adlmGetBorrowDaysLeft();

        [DllImport("acad.exe", EntryPoint = "?adlmGetBorrowExpirationDate@@YA_NPAU_SYSTEMTIME@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr adlmGetBorrowExpirationDate(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?adlmResumeHeartbeat@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr adlmResumeHeartbeat();

        [DllImport("acad.exe", EntryPoint = "?adlmReturnLicense@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr adlmReturnLicense();

        [DllImport("acad.exe", EntryPoint = "?adlmSuspendHeartbeat@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr adlmSuspendHeartbeat();

        [DllImport("acad.exe", EntryPoint = "ads_getSSetIter", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_getSSetIter();

        [DllImport("acad.exe", EntryPoint = "ads_getTempFilesLocationStr", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_getTempFilesLocationStr();

        [DllImport("acad.exe", EntryPoint = "ads_getbackdoor", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_getbackdoor();

        [DllImport("acad.exe", EntryPoint = "ads_more", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_more();

        [DllImport("acad.exe", EntryPoint = "ads_regen", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_regen();

        [DllImport("acad.exe", EntryPoint = "?ads_ssgetNested@@YAHPB_WPBV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@HPAPAV1@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ads_ssgetNested(string arg0, IntPtr arg1, IntPtr arg2);

        [DllImport("acad.exe", EntryPoint = "adsi_allregfun", CallingConvention = CallingConvention.Cdecl)]
        public static extern void adsi_allregfun();

        [DllImport("acad.exe", EntryPoint = "?adsi_forcehighlight@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr adsi_forcehighlight();

        [DllImport("acad.exe", EntryPoint = "adsi_getGApp", CallingConvention = CallingConvention.Cdecl)]
        public static extern void adsi_getGApp();

        [DllImport("acad.exe", EntryPoint = "?adsi_regfun@@YAHPBXP6AHHPBVAcEdCommand@@@Z@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr adsi_regfun(IntPtr arg0, IntPtr arg1, int arg2, IntPtr arg3);

        [DllImport("acad.exe", EntryPoint = "?appContextOpenDocument@@YA?AW4ErrorStatus@Acad@@PBUDocOpenParams@AcApDocManager@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr appContextOpenDocument(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?applyPendingUCS@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr applyPendingUCS();

        [DllImport("acad.exe", EntryPoint = "?break3dQueueForRegen@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr break3dQueueForRegen();

        [DllImport("acad.exe", EntryPoint = "?checkForXparent@@YA_NPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr checkForXparent(string arg0);

        [DllImport("acad.exe", EntryPoint = "?collectIdsForPurging@@YA?AW4ErrorStatus@Acad@@PAVAcDbDatabase@@ABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@AAV4@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr collectIdsForPurging(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?convertLWtoPixel@@YAHW4LineWeight@AcDb@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr convertLWtoPixel(char arg0, int arg1, int arg2, int arg3, int arg4, int arg5, char arg6, int arg7, int arg8, int arg9, int arg10, int arg11);

        [DllImport("acad.exe", EntryPoint = "?createGradientThumbnailEngine@@YAPAVCAcGradientThumbnailEngine@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr createGradientThumbnailEngine();

        [DllImport("acad.exe", EntryPoint = "?createPlotReactorInfo@@YAPAVAcApPlotReactorInfoImp@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr createPlotReactorInfo();

        [DllImport("acad.exe", EntryPoint = "?dashView@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dashView();

        [DllImport("acad.exe", EntryPoint = "?dbConnectActiveState@@YAXH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dbConnectActiveState(int arg0);

        [DllImport("acad.exe", EntryPoint = "?dgnSupported@@YAP6AXPB_W@Z_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dgnSupported(string arg0);

        [DllImport("acad.exe", EntryPoint = "?dlfnloop@@YAHPB_W00H0AAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@PAH2200@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr dlfnloop(string arg0, int arg1, int arg2, int arg3, int arg4, IntPtr arg5, char arg6, IntPtr arg7, char arg8, IntPtr arg9, char arg10);

        [DllImport("acad.exe", EntryPoint = "?drginsrt@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr drginsrt();

        [DllImport("acad.exe", EntryPoint = "?duplicateSelectionsAllowed@@YA_NPAVAcApDocument@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr duplicateSelectionsAllowed(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?enableOPTControls@@YA?AW4ErrorStatus@Acad@@_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr enableOPTControls(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?executeInSessionContext@@YAXP6AXPAX@Z0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr executeInSessionContext(IntPtr arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?findFileByAcadSupportPath@@YA?AW4ErrorStatus@Acad@@PB_W0HPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr findFileByAcadSupportPath(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?findvport@@YGPAVAcViewport@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort findvport(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?forceOpenLeader@@YA?AW4ErrorStatus@Acad@@AAPAVAcDbObject@@AAVAcDbObjectId@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr forceOpenLeader(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?get1dist@@YG_NPB_WPAN@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort get1dist(bool arg0, string arg1, IntPtr arg2);

        [DllImport("acad.exe", EntryPoint = "?getAcToolbarControl@@YAPAVCWnd@@PAV1@ABVCRect@@I@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getAcToolbarControl();

        [DllImport("acad.exe", EntryPoint = "?getAcViewTransitionServices@@YAPAVAcViewTransitionServices@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getAcViewTransitionServices();

        [DllImport("acad.exe", EntryPoint = "?getAcadEventSource@@YAPAVAcadEventSource@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getAcadEventSource();

        [DllImport("acad.exe", EntryPoint = "getAdeskFullName", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getAdeskFullName();

        [DllImport("acad.exe", EntryPoint = "getAdeskShortName", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getAdeskShortName();

        [DllImport("acad.exe", EntryPoint = "?getApplicationIcon@@YAPAUHICON__@@H_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getApplicationIcon();

        [DllImport("acad.exe", EntryPoint = "getAutocadName", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getAutocadName();

        [DllImport("acad.exe", EntryPoint = "getAutocadProgram", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getAutocadProgram();

        [DllImport("acad.exe", EntryPoint = "?getBasePoint@@YAHAAY02N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getBasePoint(IntPtr arg0, int arg1, int arg2, double arg3);

        [DllImport("acad.exe", EntryPoint = "?getBitmapInfo@@YA_NPBVImage@Atil@@AAPAVAcAtilDibImage@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getBitmapInfo(IntPtr arg0, IntPtr arg1, int arg2, int arg3);

        [DllImport("acad.exe", EntryPoint = "getCompanyName", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getCompanyName();

        [DllImport("acad.exe", EntryPoint = "?getCurDisplay@@YAPAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getCurDisplay();

        [DllImport("acad.exe", EntryPoint = "?getCurrentActivePlotStyleTable@@YA_NAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getCurrentActivePlotStyleTable(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?getCurrentDirPrefix@@YAPBVCAcUiPathname@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getCurrentDirPrefix();

        [DllImport("acad.exe", EntryPoint = "?getCurrentPlotStyleName@@YA?AW4ErrorStatus@Acad@@AAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getCurrentPlotStyleName(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getCurrentPlotterDevice@@YAPAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getCurrentPlotterDevice();

        [DllImport("acad.exe", EntryPoint = "?getDbWorkSpaceFilesLocation@@YAPB_WXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getDbWorkSpaceFilesLocation();

        [DllImport("acad.exe", EntryPoint = "?getLineOrArcFromVertex@@YAPAVAcDbEntity@@VAcDbObjectId@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getLineOrArcFromVertex();

        [DllImport("acad.exe", EntryPoint = "getMapName", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getMapName();

        [DllImport("acad.exe", EntryPoint = "?getMlinesCoreServices@@YAPAVMlinesCoreServices@@XZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getMlinesCoreServices();

        [DllImport("acad.exe", EntryPoint = "getOEMName", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getOEMName();

        [DllImport("acad.exe", EntryPoint = "?getPlotStylesFromStyleSheet@@YAHABV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@AAVCStringList@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getPlotStylesFromStyleSheet(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?getRSGBitsFromUDBits@@YAJJ@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getRSGBitsFromUDBits(int arg0);

        [DllImport("acad.exe", EntryPoint = "?getRawAngle@@YAHQBNPB_WPAN@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getRawAngle(IntPtr arg0, string arg1, IntPtr arg2);

        [DllImport("acad.exe", EntryPoint = "?getRefSearchPath@@YAHPAXQAPA_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getRefSearchPath(IntPtr arg0, string arg1);

        [DllImport("acad.exe", EntryPoint = "?getSheetSetCategories@@YAXAAV?$list@V?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@V?$allocator@V?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@std@@@std@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getSheetSetCategories(IntPtr arg0, IntPtr arg1, char arg2, IntPtr arg3, char arg4, IntPtr arg5, char arg6);

        [DllImport("acad.exe", EntryPoint = "?getShowHyperlinkCursor@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getShowHyperlinkCursor();

        [DllImport("acad.exe", EntryPoint = "?getUnitsConversion@@YA_NW4UnitsValue@AcDb@@AAN@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr getUnitsConversion(char arg0, int arg1, IntPtr arg2, IntPtr arg3, sbyte arg4, int arg5);

        [DllImport("acad.exe", EntryPoint = "?hatchedit@@YAXJH_NPBVAcDbObject@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr hatchedit(int arg0, int arg1, bool arg2, IntPtr arg3);

        [DllImport("acad.exe", EntryPoint = "?insrt@@YAHPAVAcDbBlockTableRecord@@PAVAcDragInsert@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr insrt(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?insrt@@YAHW4InsertCommandEnumType@@PB_WPAVAcDragInsert@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr insrt_1(char arg0, int arg1, uint arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, byte arg14, int arg15, int arg16, int arg17, int arg18, int arg19, int arg20, int arg21);

        [DllImport("acad.exe", EntryPoint = "?invokeTextStyleDialog@@YAXPAVAcDbDatabase@@PAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr invokeTextStyleDialog(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?isDynInputInEditMode@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr isDynInputInEditMode();

        [DllImport("acad.exe", EntryPoint = "?isPendingUCS@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr isPendingUCS();

        [DllImport("acad.exe", EntryPoint = "?isRegenHappening@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr isRegenHappening();

        [DllImport("acad.exe", EntryPoint = "?isTooManyArrayObjects@@YAHHAAJ0AAH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr isTooManyArrayObjects(int arg0, IntPtr arg1, int arg2, IntPtr arg3);

        [DllImport("acad.exe", EntryPoint = "?manageDimEditorOopsReactor@@YA?AW4ErrorStatus@Acad@@VAcDbObjectId@@HPAVAcDbPointRef@@HAAPAPAV4@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr manageDimEditorOopsReactor(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?manageDimEditorReactor@@YA?AW4ErrorStatus@Acad@@VAcDbObjectId@@_N0H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr manageDimEditorReactor(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?manageDimViewEditorReactor@@YA?AW4ErrorStatus@Acad@@H@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr manageDimViewEditorReactor(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?openReadOnlyDbTables@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr openReadOnlyDbTables();

        [DllImport("acad.exe", EntryPoint = "?opmXrefDlgHook@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr opmXrefDlgHook();

        [DllImport("acad.exe", EntryPoint = "?pan_to_point@@YA?AVCPoint@@ABVAcGePoint3d@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr pan_to_point(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?pattern_palette@@YAHAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@0@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr pattern_palette(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?plotStyleNameExistsInDictionary@@YAHPB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr plotStyleNameExistsInDictionary(string arg0);

        [DllImport("acad.exe", EntryPoint = "?plotStyleNameExistsInStyleTable@@YAHPB_WH@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr plotStyleNameExistsInStyleTable(string arg0, int arg1);

        [DllImport("acad.exe", EntryPoint = "?purgeDatabase@@YA?AW4ErrorStatus@Acad@@PAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr purgeDatabase(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?purgeObjectsFromDatabase@@YA?AW4ErrorStatus@Acad@@PAVAcDbDatabase@@ABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr purgeObjectsFromDatabase(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?readAndConvertOldVersions@@YA_NAAPAVAcDbDatabase@@PB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr readAndConvertOldVersions(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?redrawq@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr redrawq();

        [DllImport("acad.exe", EntryPoint = "removeAppFromLispList", CallingConvention = CallingConvention.Cdecl)]
        public static extern void removeAppFromLispList();

        [DllImport("acad.exe", EntryPoint = "?removeEntHighlightOverride@@YGXJ@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort removeEntHighlightOverride();

        [DllImport("acad.exe", EntryPoint = "?removeSSgetFilterInputContextReactor@@YA?AW4ErrorStatus@Acad@@PAVAcApDocument@@PAVAcEdSSGetFilter2@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr removeSSgetFilterInputContextReactor(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?removeSSgetFilterInputContextReactor@@YA?AW4ErrorStatus@Acad@@PAVAcApDocument@@PAVAcEdSSGetFilter3@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr removeSSgetFilterInputContextReactor_1(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?removeSSgetFilterInputContextReactor@@YA?AW4ErrorStatus@Acad@@PAVAcApDocument@@PAVAcEdSSGetFilter@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr removeSSgetFilterInputContextReactor_2(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?removeSSgetFilterInputContextReactor@@YA?AW4ErrorStatus@Acad@@PAVAcApDocument@@PAVAcEdSSGetSubSelectFilter@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr removeSSgetFilterInputContextReactor_3(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?renameSymbol@@YAXPAVAcDbDatabase@@W4symEnumType@SymUtil@@PBV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@2@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr renameSymbol(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?scriptbrk@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr scriptbrk();

        [DllImport("acad.exe", EntryPoint = "?setAllowDuplicateSelection@@YA?AW4ErrorStatus@Acad@@PAVAcApDocument@@E@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr setAllowDuplicateSelection(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?setBreak3dQueueForRegen@@YAX_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr setBreak3dQueueForRegen(bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setCurrentActivePlotStyleTable@@YA_NABV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr setCurrentActivePlotStyleTable(IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setCurrentPlotStyleName@@YA?AW4ErrorStatus@Acad@@PB_W@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr setCurrentPlotStyleName(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?setShowHyperlinkCursor@@YAX_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr setShowHyperlinkCursor(bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setUndoForLayoutTabs@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr setUndoForLayoutTabs();

        [DllImport("acad.exe", EntryPoint = "?setViewFromVtr@@YAXPAVAcDbViewTableRecord@@PAVAcGsView@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr setViewFromVtr(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?setXattachCBFunc@@YAXP6AXPAVAcDbDatabase@@PB_W@Z@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr setXattachCBFunc(IntPtr arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?storeSQLIndexInDwg@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr storeSQLIndexInDwg();

        [DllImport("acad.exe", EntryPoint = "thisProduct", CallingConvention = CallingConvention.Cdecl)]
        public static extern void thisProduct();

        [DllImport("acad.exe", EntryPoint = "thisProgram", CallingConvention = CallingConvention.Cdecl)]
        public static extern void thisProgram();

        [DllImport("acad.exe", EntryPoint = "?topwind@@YG_NPAVAcViewport@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort topwind(bool arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?trackEntHighlightOverride@@YGX_N@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort trackEntHighlightOverride();

        [DllImport("acad.exe", EntryPoint = "?updateViewportAnnotationScale@@YA?AW4ErrorStatus@Acad@@PAVAcDbViewport@@PAVAcDbAnnotationScale@@@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr updateViewportAnnotationScale(IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?visualstylesCmd@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr visualstylesCmd();

        [DllImport("acad.exe", EntryPoint = "?visualstylesCurrentCmd@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr visualstylesCurrentCmd();

        [DllImport("acad.exe", EntryPoint = "?wasACADStartedWithScript@@YA_NXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr wasACADStartedWithScript();

        [DllImport("acad.exe", EntryPoint = "?wcIsUsingWorkCenterFileOpenProc@@YAHXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr wcIsUsingWorkCenterFileOpenProc();

        [DllImport("acad.exe", EntryPoint = "?wcRestoreFileOpenProc@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr wcRestoreFileOpenProc();

        [DllImport("acad.exe", EntryPoint = "?wcSetFileOpenProc@@YAHP6GHPAUHWND__@@PAFHABV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@222AAV23@PAH4PB_W@Z@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr wcSetFileOpenProc(IntPtr arg0, int arg1, IntPtr arg2);

        [DllImport("acad.exe", EntryPoint = "?whichwind@@YGPAVAcViewport@@JJ@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort whichwind(IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?xfiles@@YAXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xfiles();

        [DllImport("acad.exe", EntryPoint = "?zoomax_objects_usepickfirst@@YAXHF@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr zoomax_objects_usepickfirst(int arg0, short arg1);

        [DllImport("acad.exe", EntryPoint = "AcadGetIDispatch", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AcadGetIDispatch();

        [DllImport("acad.exe", EntryPoint = "_AcadOnIdle", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AcadOnIdle();

        [DllImport("acad.exe", EntryPoint = "_AcadPasteToCommandLine", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AcadPasteToCommandLine();

        [DllImport("acad.exe", EntryPoint = "_AcadPreTranslateMessage", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AcadPreTranslateMessage();

        [DllImport("acad.exe", EntryPoint = "_AcadSecondaryLoop", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AcadSecondaryLoop();

        [DllImport("acad.exe", EntryPoint = "_AcadSetVbaSession", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AcadSetVbaSession();

        [DllImport("acad.exe", EntryPoint = "_EnableFloatingWindowsHook", CallingConvention = CallingConvention.Cdecl)]
        public static extern void EnableFloatingWindowsHook();

        [DllImport("acad.exe", EntryPoint = "GetViewportInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetViewportInfo();

        [DllImport("acad.exe", EntryPoint = "GetWaitingFileName", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetWaitingFileName();

        [DllImport("acad.exe", EntryPoint = "acadAdsVerNo", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acadAdsVerNo();

        [DllImport("acad.exe", EntryPoint = "_acadiGetCustomizationInterface@0", CallingConvention = CallingConvention.StdCall)]
        public static extern void acadiGetCustomizationInterface();

        [DllImport("acad.exe", EntryPoint = "_acadiSetCustomizationInterface@4", CallingConvention = CallingConvention.StdCall)]
        public static extern void acadiSetCustomizationInterface();

        [DllImport("acad.exe", EntryPoint = "acdbAngToF", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbAngToF();

        [DllImport("acad.exe", EntryPoint = "acdbAngToS", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbAngToS();

        [DllImport("acad.exe", EntryPoint = "acdbDictAdd", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbDictAdd();

        [DllImport("acad.exe", EntryPoint = "acdbDictNext", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbDictNext();

        [DllImport("acad.exe", EntryPoint = "acdbDictRemove", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbDictRemove();

        [DllImport("acad.exe", EntryPoint = "acdbDictRename", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbDictRename();

        [DllImport("acad.exe", EntryPoint = "acdbDictSearch", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbDictSearch();

        [DllImport("acad.exe", EntryPoint = "acdbDisToF", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbDisToF();

        [DllImport("acad.exe", EntryPoint = "acdbEntDel", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbEntDel();

        [DllImport("acad.exe", EntryPoint = "acdbEntGet", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbEntGet();

        [DllImport("acad.exe", EntryPoint = "acdbEntGetX", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbEntGetX();

        [DllImport("acad.exe", EntryPoint = "acdbEntLast", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbEntLast();

        [DllImport("acad.exe", EntryPoint = "acdbEntMake", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbEntMake();

        [DllImport("acad.exe", EntryPoint = "acdbEntMakeX", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbEntMakeX();

        [DllImport("acad.exe", EntryPoint = "acdbEntMod", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbEntMod();

        [DllImport("acad.exe", EntryPoint = "acdbEntNext", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbEntNext();

        [DllImport("acad.exe", EntryPoint = "acdbEntUpd", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbEntUpd();

        [DllImport("acad.exe", EntryPoint = "acdbInters", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbInters();

        [DllImport("acad.exe", EntryPoint = "acdbNamedObjDict", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbNamedObjDict();

        [DllImport("acad.exe", EntryPoint = "acdbRToS", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbRToS();

        [DllImport("acad.exe", EntryPoint = "acdbRawAngToF", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbRawAngToF();

        [DllImport("acad.exe", EntryPoint = "acdbRawAngToS", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbRawAngToS();

        [DllImport("acad.exe", EntryPoint = "acdbRegApp", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbRegApp();

        [DllImport("acad.exe", EntryPoint = "acdbSNValid", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbSNValid();

        [DllImport("acad.exe", EntryPoint = "acdbTblNext", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbTblNext();

        [DllImport("acad.exe", EntryPoint = "acdbTblObjName", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbTblObjName();

        [DllImport("acad.exe", EntryPoint = "acdbTblSearch", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acdbTblSearch();

        [DllImport("acad.exe", EntryPoint = "acedAlert", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedAlert();

        [DllImport("acad.exe", EntryPoint = "acedArxLoad", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedArxLoad();

        [DllImport("acad.exe", EntryPoint = "acedArxLoaded", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedArxLoaded();

        [DllImport("acad.exe", EntryPoint = "acedArxUnload", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedArxUnload();

        [DllImport("acad.exe", EntryPoint = "acedClearOLELock", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedClearOLELock();

        [DllImport("acad.exe", EntryPoint = "acedCmd", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedCmd();

        [DllImport("acad.exe", EntryPoint = "acedCmdLookup", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedCmdLookup();

        [DllImport("acad.exe", EntryPoint = "acedCmdUndefine", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedCmdUndefine();

        [DllImport("acad.exe", EntryPoint = "acedCommand", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedCommand();

        [DllImport("acad.exe", EntryPoint = "acedDefun", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedDefun();

        [DllImport("acad.exe", EntryPoint = "acedDefunEx", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedDefunEx();

        [DllImport("acad.exe", EntryPoint = "acedDragGen", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedDragGen();

        [DllImport("acad.exe", EntryPoint = "acedEatCommandThroat", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedEatCommandThroat();

        [DllImport("acad.exe", EntryPoint = "acedEntSel", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedEntSel();

        [DllImport("acad.exe", EntryPoint = "acedFNSplit", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedFNSplit();

        [DllImport("acad.exe", EntryPoint = "acedFindFile", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedFindFile();

        [DllImport("acad.exe", EntryPoint = "acedGetAngle", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetAngle();

        [DllImport("acad.exe", EntryPoint = "acedGetAppName", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetAppName();

        [DllImport("acad.exe", EntryPoint = "acedGetArgs", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetArgs();

        [DllImport("acad.exe", EntryPoint = "acedGetCName", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetCName();

        [DllImport("acad.exe", EntryPoint = "acedGetCfg", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetCfg();

        [DllImport("acad.exe", EntryPoint = "acedGetCommandForDocument", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetCommandForDocument();

        [DllImport("acad.exe", EntryPoint = "acedGetCorner", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetCorner();

        [DllImport("acad.exe", EntryPoint = "acedGetDist", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetDist();

        [DllImport("acad.exe", EntryPoint = "acedGetEnv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetEnv();

        [DllImport("acad.exe", EntryPoint = "acedGetFileD", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetFileD();

        [DllImport("acad.exe", EntryPoint = "acedGetFileNavDialog", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetFileNavDialog();

        [DllImport("acad.exe", EntryPoint = "acedGetFunCode", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetFunCode();

        [DllImport("acad.exe", EntryPoint = "acedGetInput", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetInput();

        [DllImport("acad.exe", EntryPoint = "acedGetInt", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetInt();

        [DllImport("acad.exe", EntryPoint = "acedGetKword", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetKword();

        [DllImport("acad.exe", EntryPoint = "acedGetOrient", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetOrient();

        [DllImport("acad.exe", EntryPoint = "acedGetPoint", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetPoint();

        [DllImport("acad.exe", EntryPoint = "acedGetReal", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetReal();

        [DllImport("acad.exe", EntryPoint = "acedGetString", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetString();

        [DllImport("acad.exe", EntryPoint = "acedGetStringB", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetStringB();

        [DllImport("acad.exe", EntryPoint = "acedGetSym", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetSym();

        [DllImport("acad.exe", EntryPoint = "acedGetVar", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGetVar();

        [DllImport("acad.exe", EntryPoint = "acedGrDraw", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGrDraw();

        [DllImport("acad.exe", EntryPoint = "acedGrRead", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGrRead();

        [DllImport("acad.exe", EntryPoint = "acedGrText", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGrText();

        [DllImport("acad.exe", EntryPoint = "acedGrVecs", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGrVecs();

        [DllImport("acad.exe", EntryPoint = "acedGraphScr", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedGraphScr();

        [DllImport("acad.exe", EntryPoint = "acedHelp", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedHelp();

        [DllImport("acad.exe", EntryPoint = "acedInitGet", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedInitGet();

        [DllImport("acad.exe", EntryPoint = "acedInvoke", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedInvoke();

        [DllImport("acad.exe", EntryPoint = "acedMenuCmd", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedMenuCmd();

        [DllImport("acad.exe", EntryPoint = "acedNEntSel", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedNEntSel();

        [DllImport("acad.exe", EntryPoint = "acedNEntSelP", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedNEntSelP();

        [DllImport("acad.exe", EntryPoint = "acedOsnap", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedOsnap();

        [DllImport("acad.exe", EntryPoint = "acedPopCommandDirectory", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedPopCommandDirectory();

        [DllImport("acad.exe", EntryPoint = "acedPostCommandPrompt", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedPostCommandPrompt();

        [DllImport("acad.exe", EntryPoint = "acedPrompt", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedPrompt();

        [DllImport("acad.exe", EntryPoint = "acedPutSym", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedPutSym();

        [DllImport("acad.exe", EntryPoint = "acedRedraw", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedRedraw();

        [DllImport("acad.exe", EntryPoint = "acedRegFunc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedRegFunc();

        [DllImport("acad.exe", EntryPoint = "acedReloadMenus", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedReloadMenus();

        [DllImport("acad.exe", EntryPoint = "acedRetInt", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedRetInt();

        [DllImport("acad.exe", EntryPoint = "acedRetList", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedRetList();

        [DllImport("acad.exe", EntryPoint = "acedRetName", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedRetName();

        [DllImport("acad.exe", EntryPoint = "acedRetNil", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedRetNil();

        [DllImport("acad.exe", EntryPoint = "acedRetPoint", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedRetPoint();

        [DllImport("acad.exe", EntryPoint = "acedRetReal", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedRetReal();

        [DllImport("acad.exe", EntryPoint = "acedRetStr", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedRetStr();

        [DllImport("acad.exe", EntryPoint = "acedRetT", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedRetT();

        [DllImport("acad.exe", EntryPoint = "acedRetVal", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedRetVal();

        [DllImport("acad.exe", EntryPoint = "acedRetVoid", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedRetVoid();

        [DllImport("acad.exe", EntryPoint = "acedSSAdd", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSAdd();

        [DllImport("acad.exe", EntryPoint = "acedSSDel", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSDel();

        [DllImport("acad.exe", EntryPoint = "acedSSFree", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSFree();

        [DllImport("acad.exe", EntryPoint = "acedSSGet", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSGet();

        [DllImport("acad.exe", EntryPoint = "acedSSGetFirst", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSGetFirst();

        [DllImport("acad.exe", EntryPoint = "acedSSGetKwordCallbackPtr", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSGetKwordCallbackPtr();

        [DllImport("acad.exe", EntryPoint = "acedSSGetOtherCallbackPtr", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSGetOtherCallbackPtr();

        [DllImport("acad.exe", EntryPoint = "acedSSLength", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSLength();

        [DllImport("acad.exe", EntryPoint = "acedSSMemb", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSMemb();

        [DllImport("acad.exe", EntryPoint = "acedSSName", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSName();

        [DllImport("acad.exe", EntryPoint = "acedSSNameX", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSNameX();

        [DllImport("acad.exe", EntryPoint = "acedSSSetFirst", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSSetFirst();

        [DllImport("acad.exe", EntryPoint = "acedSSSetKwordCallbackPtr", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSSetKwordCallbackPtr();

        [DllImport("acad.exe", EntryPoint = "acedSSSetOtherCallbackPtr", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSSSetOtherCallbackPtr();

        [DllImport("acad.exe", EntryPoint = "acedSetCfg", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSetCfg();

        [DllImport("acad.exe", EntryPoint = "acedSetCurrentWorkspace", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSetCurrentWorkspace();

        [DllImport("acad.exe", EntryPoint = "acedSetEnv", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSetEnv();

        [DllImport("acad.exe", EntryPoint = "acedSetFunHelp", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSetFunHelp();

        [DllImport("acad.exe", EntryPoint = "acedSetOLELock", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSetOLELock();

        [DllImport("acad.exe", EntryPoint = "acedSetVar", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSetVar();

        [DllImport("acad.exe", EntryPoint = "acedSetView", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedSetView();

        [DllImport("acad.exe", EntryPoint = "acedTablet", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedTablet();

        [DllImport("acad.exe", EntryPoint = "acedTextBox", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedTextBox();

        [DllImport("acad.exe", EntryPoint = "acedTextPage", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedTextPage();

        [DllImport("acad.exe", EntryPoint = "acedTextScr", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedTextScr();

        [DllImport("acad.exe", EntryPoint = "acedTrans", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedTrans();

        [DllImport("acad.exe", EntryPoint = "acedUndef", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedUndef();

        [DllImport("acad.exe", EntryPoint = "acedUpdate", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedUpdate();

        [DllImport("acad.exe", EntryPoint = "acedUsrBrk", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedUsrBrk();

        [DllImport("acad.exe", EntryPoint = "acedVports", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedVports();

        [DllImport("acad.exe", EntryPoint = "acedXformSS", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acedXformSS();

        [DllImport("acad.exe", EntryPoint = "acgiGetClipBoundary", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acgiGetClipBoundary();

        [DllImport("acad.exe", EntryPoint = "acgsReComputeVS", CallingConvention = CallingConvention.Cdecl)]
        public static extern void acgsReComputeVS();

        [DllImport("acad.exe", EntryPoint = "ads_action_tile", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_action_tile();

        [DllImport("acad.exe", EntryPoint = "ads_add_list", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_add_list();

        [DllImport("acad.exe", EntryPoint = "ads_client_data_tile", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_client_data_tile();

        [DllImport("acad.exe", EntryPoint = "ads_dimensions_tile", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_dimensions_tile();

        [DllImport("acad.exe", EntryPoint = "ads_done_dialog", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_done_dialog();

        [DllImport("acad.exe", EntryPoint = "ads_done_positioned_dialog", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_done_positioned_dialog();

        [DllImport("acad.exe", EntryPoint = "ads_end_image", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_end_image();

        [DllImport("acad.exe", EntryPoint = "ads_end_list", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_end_list();

        [DllImport("acad.exe", EntryPoint = "ads_filefilter", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_filefilter();

        [DllImport("acad.exe", EntryPoint = "ads_fill_image", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_fill_image();

        [DllImport("acad.exe", EntryPoint = "ads_get_attr", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_get_attr();

        [DllImport("acad.exe", EntryPoint = "ads_get_attr_string", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_get_attr_string();

        [DllImport("acad.exe", EntryPoint = "ads_get_filefilter", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_get_filefilter();

        [DllImport("acad.exe", EntryPoint = "ads_get_tile", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_get_tile();

        [DllImport("acad.exe", EntryPoint = "ads_getpointx", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_getpointx();

        [DllImport("acad.exe", EntryPoint = "ads_getpointxEx", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_getpointxEx();

        [DllImport("acad.exe", EntryPoint = "ads_load_dialog", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_load_dialog();

        [DllImport("acad.exe", EntryPoint = "ads_mode_tile", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_mode_tile();

        [DllImport("acad.exe", EntryPoint = "ads_new_dialog", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_new_dialog();

        [DllImport("acad.exe", EntryPoint = "ads_new_positioned_dialog", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_new_positioned_dialog();

        [DllImport("acad.exe", EntryPoint = "ads_queueexpr", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_queueexpr();

        [DllImport("acad.exe", EntryPoint = "ads_set_tile", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_set_tile();

        [DllImport("acad.exe", EntryPoint = "ads_slide_image", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_slide_image();

        [DllImport("acad.exe", EntryPoint = "ads_start_dialog", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_start_dialog();

        [DllImport("acad.exe", EntryPoint = "ads_start_image", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_start_image();

        [DllImport("acad.exe", EntryPoint = "ads_start_list", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_start_list();

        [DllImport("acad.exe", EntryPoint = "ads_term_dialog", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_term_dialog();

        [DllImport("acad.exe", EntryPoint = "ads_unload_dialog", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_unload_dialog();

        [DllImport("acad.exe", EntryPoint = "ads_vector_image", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ads_vector_image();

        [DllImport("acad.exe", EntryPoint = "adsi_GlobalMemoryStatus", CallingConvention = CallingConvention.Cdecl)]
        public static extern void adsi_GlobalMemoryStatus();

        [DllImport("acad.exe", EntryPoint = "adsi_ent2face", CallingConvention = CallingConvention.Cdecl)]
        public static extern void adsi_ent2face();

        [DllImport("acad.exe", EntryPoint = "adsi_invoke", CallingConvention = CallingConvention.Cdecl)]
        public static extern void adsi_invoke();

        [DllImport("acad.exe", EntryPoint = "adsi_kcabteg", CallingConvention = CallingConvention.Cdecl)]
        public static extern void adsi_kcabteg();

        [DllImport("acad.exe", EntryPoint = "adsi_kcabtes", CallingConvention = CallingConvention.Cdecl)]
        public static extern void adsi_kcabtes();

        [DllImport("acad.exe", EntryPoint = "adsi_newdict", CallingConvention = CallingConvention.Cdecl)]
        public static extern void adsi_newdict();

        [DllImport("acad.exe", EntryPoint = "adsw_acadDocWnd", CallingConvention = CallingConvention.Cdecl)]
        public static extern void adsw_acadDocWnd();

        [DllImport("acad.exe", EntryPoint = "adsw_acadMainWnd", CallingConvention = CallingConvention.Cdecl)]
        public static extern void adsw_acadMainWnd();

        [DllImport("acad.exe", EntryPoint = "callButtonEditor", CallingConvention = CallingConvention.Cdecl)]
        public static extern void callButtonEditor();

        [DllImport("acad.exe", EntryPoint = "_closeRefSearch", CallingConvention = CallingConvention.Cdecl)]
        public static extern void closeRefSearch();

        [DllImport("acad.exe", EntryPoint = "cuiAddBitmap", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cuiAddBitmap();

        [DllImport("acad.exe", EntryPoint = "cuiEndTransferBitmaps", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cuiEndTransferBitmaps();

        [DllImport("acad.exe", EntryPoint = "cuiIsUsingSmallIcon", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cuiIsUsingSmallIcon();

        [DllImport("acad.exe", EntryPoint = "cuiSaveMenuAndToolbarState", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cuiSaveMenuAndToolbarState();

        [DllImport("acad.exe", EntryPoint = "cuiStartTransferBitmaps", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cuiStartTransferBitmaps();

        [DllImport("acad.exe", EntryPoint = "enableCustomizableMode", CallingConvention = CallingConvention.Cdecl)]
        public static extern void enableCustomizableMode();

        [DllImport("acad.exe", EntryPoint = "isPointOverToolbar", CallingConvention = CallingConvention.Cdecl)]
        public static extern void isPointOverToolbar();

        [DllImport("acad.exe", EntryPoint = "_monitorAdlDragging", CallingConvention = CallingConvention.Cdecl)]
        public static extern void monitorAdlDragging();

        [DllImport("acad.exe", EntryPoint = "_openRefSearch", CallingConvention = CallingConvention.Cdecl)]
        public static extern void openRefSearch();

        [DllImport("acad.exe", EntryPoint = "respSdjhU3_x16", CallingConvention = CallingConvention.Cdecl)]
        public static extern void respSdjhU3_x16();

        [DllImport("acad.exe", EntryPoint = "startToolbarDragDrop", CallingConvention = CallingConvention.Cdecl)]
        public static extern void startToolbarDragDrop();

        [DllImport("acad.exe", EntryPoint = "transferSingleBitmap", CallingConvention = CallingConvention.Cdecl)]
        public static extern void transferSingleBitmap();

    }

    /// <summary>
    /// AcadPlotHostAppServices 类的Native方法 (9 个函数)
    /// </summary>
    public static class AcadPlotHostAppServices
    {
        [DllImport("acad.exe", EntryPoint = "?alertBackgroundPlotInProgress@AcadPlotHostAppServices@@QAEXPAUHWND__@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void alertBackgroundPlotInProgress(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?altPageSetUpsTemplate@AcadPlotHostAppServices@@QAEAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr altPageSetUpsTemplate(IntPtr thisPtr, char arg0, IntPtr arg1, char arg2, IntPtr arg3, char arg4);

        [DllImport("acad.exe", EntryPoint = "?generateLogFileName@AcadPlotHostAppServices@@QAEPA_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr generateLogFileName(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?isInChildProcessMode@AcadPlotHostAppServices@@QAE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool isInChildProcessMode(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?plotToFileLocation@AcadPlotHostAppServices@@QAEAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr plotToFileLocation(IntPtr thisPtr, char arg0, IntPtr arg1, char arg2, IntPtr arg3, char arg4);

        [DllImport("acad.exe", EntryPoint = "?postProcessLogFileName@AcadPlotHostAppServices@@QAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void postProcessLogFileName(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?sessionLogFileName@AcadPlotHostAppServices@@QAEAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr sessionLogFileName(IntPtr thisPtr, char arg0, IntPtr arg1, char arg2, IntPtr arg3, char arg4);

        [DllImport("acad.exe", EntryPoint = "?setPlotToFileLocation@AcadPlotHostAppServices@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setPlotToFileLocation(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?showBgPlotInProgressDlg@AcadPlotHostAppServices@@QAEXPAUHWND__@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void showBgPlotInProgressDlg(IntPtr thisPtr, IntPtr arg0);

    }

    /// <summary>
    /// AcadPlotInternalServices 类的Native方法 (39 个函数)
    /// </summary>
    public static class AcadPlotInternalServices
    {
        [DllImport("acad.exe", EntryPoint = "?areShadePlotPromptsNeeded@AcadPlotInternalServices@@QAEXAA_N0AAV?$HT_Smart_Pointer@VHT_Plotter_Definition@@@@HVAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void areShadePlotPromptsNeeded(IntPtr thisPtr, IntPtr arg0, int arg1, IntPtr arg2, IntPtr arg3);

        [DllImport("acad.exe", EntryPoint = "?calcEffectiveArea@AcadPlotInternalServices@@QAE_NPAVAcDbPlotSettings@@HPAN1@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool calcEffectiveArea(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?clearETXCommandDelimiterIfNecessary@AcadPlotInternalServices@@QAE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool clearETXCommandDelimiterIfNecessary(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?clearNamedPageSetupRefFromLayoutIfBogus@AcadPlotInternalServices@@QBE_NVAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool clearNamedPageSetupRefFromLayoutIfBogus(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?dialogValidator@AcadPlotInternalServices@@QAE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool dialogValidator(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?doValidate@AcadPlotInternalServices@@QAE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool doValidate(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?existExtents@AcadPlotInternalServices@@QAEHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int existExtents(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?get1Dist@AcadPlotInternalServices@@QAE_NPB_WPAN@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool get1Dist(IntPtr thisPtr, string arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?getClientType@AcadPlotInternalServices@@QAE?AW4PlotClientType@1@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getClientType(IntPtr thisPtr, IntPtr arg0, int arg1, IntPtr arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12);

        [DllImport("acad.exe", EntryPoint = "?getCurrentSheetPath@AcadPlotInternalServices@@QAE_NAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool getCurrentSheetPath(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?getHideWarningDialogs@AcadPlotInternalServices@@QAE_NW4ShowHide_Dialog_Type@1@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool getHideWarningDialogs(IntPtr thisPtr, char arg0, int arg1, IntPtr arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14, int arg15, int arg16, int arg17);

        [DllImport("acad.exe", EntryPoint = "?getOverlayViewportID@AcadPlotInternalServices@@QBEJXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int getOverlayViewportID(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?getPreviousLayoutPlotSettings@AcadPlotInternalServices@@QAEPBVAcDbPlotSettings@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getPreviousLayoutPlotSettings(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?getPreviousModelPlotSettings@AcadPlotInternalServices@@QAEPBVAcDbPlotSettings@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getPreviousModelPlotSettings(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?getPublishingDWFWithDSD@AcadPlotInternalServices@@QAE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool getPublishingDWFWithDSD(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?getShadePlotResolution@AcadPlotInternalServices@@QAE_NV?$HT_Smart_Pointer@VHT_Plot_Config@@@@AAH1AANPAVAcDbPlotSettings@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool getShadePlotResolution(IntPtr thisPtr, IntPtr arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?getSheetSetTemplatePath@AcadPlotInternalServices@@QAE_NAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool getSheetSetTemplatePath(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?getStartUpDirPath@AcadPlotInternalServices@@QAEXAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void getStartUpDirPath(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?getWindow@AcadPlotInternalServices@@QAEH_NAAN111@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int getWindow(IntPtr thisPtr, bool arg0, IntPtr arg1, int arg2, int arg3, int arg4);

        [DllImport("acad.exe", EntryPoint = "?isCancelled@AcadPlotInternalServices@@QAE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool isCancelled(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?isNoneMediaThere@AcadPlotInternalServices@@QAE_NV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool isNoneMediaThere(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?isSamePlotSettings@AcadPlotInternalServices@@QBE_NPBVAcDbPlotSettings@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool isSamePlotSettings(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?plotSetupIsUninitialized@AcadPlotInternalServices@@QAE_NPAVAcDbPlotSettings@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool plotSetupIsUninitialized(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?refreshValidatorLists@AcadPlotInternalServices@@QAEXPAVAcDbPlotSettings@@W4RefreshCode@AcPlPlotConfigManager@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void refreshValidatorLists(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?sameShadePlot@AcadPlotInternalServices@@QAE_NAAHAAVAcDbObjectId@@AA_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool sameShadePlot(IntPtr thisPtr, IntPtr arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?saveShadePlotSetting@AcadPlotInternalServices@@QAEXPAVAcDbLayout@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void saveShadePlotSetting(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?scanRVal@AcadPlotInternalServices@@QAEHPB_WPAN_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int scanRVal(IntPtr thisPtr, string arg0, IntPtr arg1, bool arg2);

        [DllImport("acad.exe", EntryPoint = "?setCommandCancelledState@AcadPlotInternalServices@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setCommandCancelledState(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setCurrentClient@AcadPlotInternalServices@@QAEXW4PlotClientType@1@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setCurrentClient(IntPtr thisPtr, char arg0, int arg1, IntPtr arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12);

        [DllImport("acad.exe", EntryPoint = "?setDialogValidator@AcadPlotInternalServices@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setDialogValidator(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setDoValidate@AcadPlotInternalServices@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setDoValidate(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setHideWarningDialogs@AcadPlotInternalServices@@QAEXW4ShowHide_Dialog_Type@1@_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setHideWarningDialogs(IntPtr thisPtr, char arg0, int arg1, IntPtr arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14, int arg15, int arg16, int arg17);

        [DllImport("acad.exe", EntryPoint = "?setNoneDeviceDefaults@AcadPlotInternalServices@@QAEXPAVAcDbPlotSettings@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setNoneDeviceDefaults(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?setNoneUserMediaInfo@AcadPlotInternalServices@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@0VHT_Float_XY@@VHT_Float_Rectangle@@W4Enum@HT_Media_Group@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setNoneUserMediaInfo(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setPublishingDWFWithDSD@AcadPlotInternalServices@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setPublishingDWFWithDSD(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?set_default_newdwg_style_sheet@AcadPlotInternalServices@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void set_default_newdwg_style_sheet(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?showSaveShadePlotPrompt@AcadPlotInternalServices@@QAE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool showSaveShadePlotPrompt(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?showVectorDeviceWarning@AcadPlotInternalServices@@QAE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool showVectorDeviceWarning(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?updateRequiresZoom@AcadPlotInternalServices@@QAE_NPBVAcDbPlotSettings@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool updateRequiresZoom(IntPtr thisPtr, IntPtr arg0);

    }

    /// <summary>
    /// AcadVectorIterator 类的Native方法 (1 个函数)
    /// </summary>
    public static class AcadVectorIterator
    {
        /// <summary>
        /// 数据符号 - AcadVectorIterator
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "??_FAcadVectorIterator@@QAEXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr default_ctor();

    }

    /// <summary>
    /// AcadVersionInfo 类的Native方法 (7 个函数)
    /// </summary>
    public static class AcadVersionInfo
    {
        [DllImport("acad.exe", EntryPoint = "?getDwgVersion@AcadVersionInfo@@YG?AW4MaintenanceReleaseVersion@AcDb@@PAW4AcDbDwgVersion@3@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getDwgVersion(IntPtr thisPtr, IntPtr arg0, int arg1, float arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, IntPtr arg13, int arg14, int arg15, int arg16, int arg17, IntPtr arg18, IntPtr arg19, sbyte arg20, int arg21);

        [DllImport("acad.exe", EntryPoint = "?releaseMajorMinorString@AcadVersionInfo@@YAPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr releaseMajorMinorString(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?releaseMajorVersion@AcadVersionInfo@@YAHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int releaseMajorVersion(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?releaseMarketVersion@AcadVersionInfo@@YAPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr releaseMarketVersion(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?releaseMinorVersion@AcadVersionInfo@@YAHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int releaseMinorVersion(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?versionNameString@AcadVersionInfo@@YAPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr versionNameString(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?versionString@AcadVersionInfo@@YAPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr versionString(IntPtr thisPtr);

    }

    /// <summary>
    /// AcApDocument 类的Native方法 (3 个函数)
    /// </summary>
    public static class AcApDocument
    {
        [DllImport("acad.exe", EntryPoint = "?desc@AcApDocument@@SAPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr desc(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?popDbmod@AcApDocument@@QAE?AW4ErrorStatus@Acad@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr popDbmod(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?pushDbmod@AcApDocument@@QAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void pushDbmod(IntPtr thisPtr);

    }

    /// <summary>
    /// AcDynamicDimManager 类的Native方法 (1 个函数)
    /// </summary>
    public static class AcDynamicDimManager
    {
        [DllImport("acad.exe", EntryPoint = "?calcDimScale@AcDynamicDimManager@@SANXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern double calcDimScale(IntPtr thisPtr);

    }

    /// <summary>
    /// AcEdJig 类的Native方法 (28 个函数)
    /// </summary>
    public static class AcEdJig
    {
        [DllImport("acad.exe", EntryPoint = "??0AcEdJig@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcEdJig@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?acquireAngle@AcEdJig@@QAE?AW4DragStatus@1@AAN@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr acquireAngle(IntPtr thisPtr, IntPtr arg0, int arg1, sbyte arg2, int arg3, int arg4, int arg5, IntPtr arg6, int arg7, int arg8, int arg9);

        [DllImport("acad.exe", EntryPoint = "?acquireAngle@AcEdJig@@QAE?AW4DragStatus@1@AANABVAcGePoint3d@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr acquireAngle_1(IntPtr thisPtr, IntPtr arg0, int arg1, sbyte arg2, int arg3, int arg4, int arg5, IntPtr arg6, int arg7, int arg8, int arg9);

        [DllImport("acad.exe", EntryPoint = "?acquireDist@AcEdJig@@QAE?AW4DragStatus@1@AAN@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr acquireDist(IntPtr thisPtr, IntPtr arg0, int arg1, sbyte arg2, int arg3, int arg4, int arg5, IntPtr arg6, int arg7, int arg8, int arg9);

        [DllImport("acad.exe", EntryPoint = "?acquireDist@AcEdJig@@QAE?AW4DragStatus@1@AANABVAcGePoint3d@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr acquireDist_1(IntPtr thisPtr, IntPtr arg0, int arg1, sbyte arg2, int arg3, int arg4, int arg5, IntPtr arg6, int arg7, int arg8, int arg9);

        [DllImport("acad.exe", EntryPoint = "?acquirePoint@AcEdJig@@QAE?AW4DragStatus@1@AAVAcGePoint3d@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr acquirePoint(IntPtr thisPtr, IntPtr arg0, int arg1, sbyte arg2, int arg3, int arg4, int arg5, IntPtr arg6, int arg7, int arg8, int arg9);

        [DllImport("acad.exe", EntryPoint = "?acquirePoint@AcEdJig@@QAE?AW4DragStatus@1@AAVAcGePoint3d@@ABV3@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr acquirePoint_1(IntPtr thisPtr, IntPtr arg0, int arg1, sbyte arg2, int arg3, int arg4, int arg5, IntPtr arg6, int arg7, int arg8, int arg9);

        [DllImport("acad.exe", EntryPoint = "?acquireString@AcEdJig@@QAE?AW4DragStatus@1@PA_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr acquireString(IntPtr thisPtr, IntPtr arg0, int arg1, sbyte arg2, int arg3, int arg4, int arg5, IntPtr arg6, int arg7, int arg8, int arg9);

        [DllImport("acad.exe", EntryPoint = "?append@AcEdJig@@QAE?AVAcDbObjectId@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr append(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?desc@AcEdJig@@SAPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr desc(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?dimData@AcEdJig@@UAEPAV?$AcArray@PAVAcDbDimData@@V?$AcArrayMemCopyReallocator@PAVAcDbDimData@@@@@@N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr dimData(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?dispPrompt@AcEdJig@@QAEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr dispPrompt(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?drag@AcEdJig@@QAE?AW4DragStatus@1@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr drag(IntPtr thisPtr, IntPtr arg0, int arg1, sbyte arg2, int arg3, int arg4, int arg5, IntPtr arg6, int arg7, int arg8, int arg9);

        [DllImport("acad.exe", EntryPoint = "?entity@AcEdJig@@UBEPAVAcDbEntity@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr entity(IntPtr thisPtr);

        /// <summary>
        /// 数据符号 - AcEdJig
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "?gpDesc@AcEdJig@@2PAVAcRxClass@@A", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gpDesc();

        [DllImport("acad.exe", EntryPoint = "?isA@AcEdJig@@UBEPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isA(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?keywordList@AcEdJig@@QAEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr keywordList(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?rxInit@AcEdJig@@SAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void rxInit(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?sampler@AcEdJig@@UAE?AW4DragStatus@1@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr sampler(IntPtr thisPtr, IntPtr arg0, int arg1, sbyte arg2, int arg3, int arg4, int arg5, IntPtr arg6, int arg7, int arg8, int arg9);

        [DllImport("acad.exe", EntryPoint = "?setDimValue@AcEdJig@@UAE?AW4ErrorStatus@Acad@@PBVAcDbDimData@@N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr setDimValue(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?setDispPrompt@AcEdJig@@QAAXPB_WZZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr setDispPrompt(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?setKeywordList@AcEdJig@@QAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setKeywordList(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?setSpecialCursorType@AcEdJig@@QAEXW4CursorType@1@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setSpecialCursorType(IntPtr thisPtr, char arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?setUserInputControls@AcEdJig@@QAEXW4UserInputControls@1@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setUserInputControls(IntPtr thisPtr, char arg0, int arg1, IntPtr arg2, int arg3);

        [DllImport("acad.exe", EntryPoint = "?specialCursorType@AcEdJig@@QAE?AW4CursorType@1@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr specialCursorType(IntPtr thisPtr, IntPtr arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?update@AcEdJig@@UAEHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int update(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?userInputControls@AcEdJig@@QAE?AW4UserInputControls@1@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr userInputControls(IntPtr thisPtr, IntPtr arg0, int arg1, IntPtr arg2, int arg3);

    }

    /// <summary>
    /// AcEdSolidSubentitySelector 类的Native方法 (3 个函数)
    /// </summary>
    public static class AcEdSolidSubentitySelector
    {
        [DllImport("acad.exe", EntryPoint = "??0AcEdSolidSubentitySelector@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcEdSolidSubentitySelector@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?selectFaces@AcEdSolidSubentitySelector@@QAE?AW4ErrorStatus@Acad@@AAVAcDbObjectId@@AAV?$AcArray@PAVAcDbSubentId@@V?$AcArrayMemCopyReallocator@PAVAcDbSubentId@@@@@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr selectFaces(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

    }

    /// <summary>
    /// AcEdSymbolUtilities 类的Native方法 (1 个函数)
    /// </summary>
    public static class AcEdSymbolUtilities
    {
        [DllImport("acad.exe", EntryPoint = "?servicesPtr100@AcEdSymbolUtilities@@YAPBVServices@1@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr servicesPtr100(IntPtr thisPtr, int arg0);

    }

    /// <summary>
    /// AcEdXrefData 类的Native方法 (13 个函数)
    /// </summary>
    public static class AcEdXrefData
    {
        [DllImport("acad.exe", EntryPoint = "??0AcEdXrefData@@QAE@ABVAcDbObjectId@@ABV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@1ABVAcDbDate@@2JJ@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?bubbleTextOverride@AcEdXrefData@@QBE?AV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr bubbleTextOverride(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?fileSize@AcEdXrefData@@QBEJXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int fileSize(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?fileTime@AcEdXrefData@@QBE?AVAcDbDate@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr fileTime(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?isModified@AcEdXrefData@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool isModified(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?resolutionTime@AcEdXrefData@@QBE?AVAcDbDate@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr resolutionTime(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?resolvedSize@AcEdXrefData@@QBEJXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int resolvedSize(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?setBubbleTextOverride@AcEdXrefData@@QAE?AW4ErrorStatus@Acad@@PB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr setBubbleTextOverride(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?setFileSize@AcEdXrefData@@QAEXJ@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setFileSize(IntPtr thisPtr, int arg0);

        [DllImport("acad.exe", EntryPoint = "?setFileTime@AcEdXrefData@@QAEXABVAcDbDate@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setFileTime(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?xrefBtrId@AcEdXrefData@@QBE?AVAcDbObjectId@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr xrefBtrId(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?xrefFullFilename@AcEdXrefData@@QBE?AV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr xrefFullFilename(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?xrefName@AcEdXrefData@@QBE?AV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr xrefName(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

    }

    /// <summary>
    /// AcEdXrefFileLock 类的Native方法 (11 个函数)
    /// </summary>
    public static class AcEdXrefFileLock
    {
        [DllImport("acad.exe", EntryPoint = "??0AcEdXrefFileLock@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcEdXrefFileLock@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?consistencyCheck@AcEdXrefFileLock@@QAE?AW4ErrorStatus@Acad@@AAHABVAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr consistencyCheck(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?consistencyCheck@AcEdXrefFileLock@@QAE?AW4ErrorStatus@Acad@@AAHABVAcDbObjectId@@AAV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr consistencyCheck_1(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?consistencyChecklocal@AcEdXrefFileLock@@QAE?AW4ErrorStatus@Acad@@AAHABVAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr consistencyChecklocal(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?lockFile@AcEdXrefFileLock@@QAE?AW4ErrorStatus@Acad@@AAHABVAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr lockFile(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?releaseFile@AcEdXrefFileLock@@QAE?AW4ErrorStatus@Acad@@AAH_N1@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr releaseFile(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?reloadFile@AcEdXrefFileLock@@QAE?AW4ErrorStatus@Acad@@AAHABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@H@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr reloadFile(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?reloadFile@AcEdXrefFileLock@@QAE?AW4ErrorStatus@Acad@@AAHH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr reloadFile_1(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?setInternalTNmgmt@AcEdXrefFileLock@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setInternalTNmgmt(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?xloadctlType@AcEdXrefFileLock@@QAE?AW4ErrorStatus@Acad@@AAHABVAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr xloadctlType(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

    }

    /// <summary>
    /// AcEdXrefTrayItemManager 类的Native方法 (6 个函数)
    /// </summary>
    public static class AcEdXrefTrayItemManager
    {
        [DllImport("acad.exe", EntryPoint = "??0AcEdXrefTrayItemManager@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcEdXrefTrayItemManager@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?addReactor@AcEdXrefTrayItemManager@@QAE?AW4ErrorStatus@Acad@@PAVAcDbDatabase@@PAVAcEdXrefTrayItemReactor@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr addReactor(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getTrayItemState@AcEdXrefTrayItemManager@@QAEPAVAcEdXrefTrayItemState@@PAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getTrayItemState(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?removeReactor@AcEdXrefTrayItemManager@@QAE?AW4ErrorStatus@Acad@@PAVAcDbDatabase@@PAVAcEdXrefTrayItemReactor@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr removeReactor(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?setTrayItemState@AcEdXrefTrayItemManager@@QAE?AW4ErrorStatus@Acad@@PAVAcDbDatabase@@PAVAcEdXrefTrayItemState@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr setTrayItemState(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

    }

    /// <summary>
    /// AcEdXrefTrayItemState 类的Native方法 (16 个函数)
    /// </summary>
    public static class AcEdXrefTrayItemState
    {
        [DllImport("acad.exe", EntryPoint = "??0AcEdXrefTrayItemState@@QAE@ABV0@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "??0AcEdXrefTrayItemState@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor_1(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcEdXrefTrayItemState@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?areNotificationsEnabled@AcEdXrefTrayItemState@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool areNotificationsEnabled(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?disableNotifications@AcEdXrefTrayItemState@@QAE?AW4ErrorStatus@Acad@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr disableNotifications(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?enableNotifications@AcEdXrefTrayItemState@@QAE?AW4ErrorStatus@Acad@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr enableNotifications(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getBubbleTextOverride@AcEdXrefTrayItemState@@QBE?AW4ErrorStatus@Acad@@AAPB_W0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getBubbleTextOverride(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?iconState@AcEdXrefTrayItemState@@QBE?AW4IconState@1@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr iconState(IntPtr thisPtr, IntPtr arg0, int arg1, uint arg2, int arg3, int arg4, int arg5, IntPtr arg6, int arg7, int arg8);

        [DllImport("acad.exe", EntryPoint = "?resetXrefManagerCommand@AcEdXrefTrayItemState@@QAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void resetXrefManagerCommand(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?resetXrefReloadAllCommand@AcEdXrefTrayItemState@@QAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void resetXrefReloadAllCommand(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?setBubbleTextOverride@AcEdXrefTrayItemState@@QAE?AW4ErrorStatus@Acad@@PB_W0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr setBubbleTextOverride(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?setIconStateOverride@AcEdXrefTrayItemState@@QAE?AW4ErrorStatus@Acad@@W4IconState@1@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr setIconStateOverride(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?setXrefCommand@AcEdXrefTrayItemState@@QAE?AW4ErrorStatus@Acad@@PB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr setXrefCommand(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?setXrefReloadCommand@AcEdXrefTrayItemState@@QAE?AW4ErrorStatus@Acad@@PB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr setXrefReloadCommand(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?xrefCommand@AcEdXrefTrayItemState@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr xrefCommand(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?xrefReloadAllCommand@AcEdXrefTrayItemState@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr xrefReloadAllCommand(IntPtr thisPtr);

    }

    /// <summary>
    /// AcGzGizmo 类的Native方法 (25 个函数)
    /// </summary>
    public static class AcGzGizmo
    {
        [DllImport("acad.exe", EntryPoint = "??0AcGzGizmo@@IAE@PAVAcGzGizmoImp@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "??0AcGzGizmo@@IAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor_1(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcGzGizmo@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?cast@AcGzGizmo@@SAPAV1@PBVAcRxObject@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr cast(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?desc@AcGzGizmo@@SAPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr desc(IntPtr thisPtr);

        /// <summary>
        /// 数据符号 - AcGzGizmo
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "?gpDesc@AcGzGizmo@@2PAVAcRxClass@@A", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gpDesc();

        [DllImport("acad.exe", EntryPoint = "?gsNode@AcGzGizmo@@UBEPAVAcGsNode@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr gsNode(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?highlightColor@AcGzGizmo@@UAE?AVAcCmEntityColor@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr highlightColor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?id@AcGzGizmo@@UBE?AVAcDbObjectId@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr id(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?inputPointFilter@AcGzGizmo@@UAE?AW4ErrorStatus@Acad@@AA_NAAVAcGePoint3d@@00AAPA_W0PAVAcGiViewportDraw@@PAVAcApDocument@@_NHABV4@6666W4OsnapMask@AcDb@@ABV?$AcArray@PAVAcDbCustomOsnapMode@@V?$AcArrayMemCopyReallocator@PAVAcDbCustomOsnapMode@@@@@@78ABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@ABV?$AcArray@V?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@V?$AcArrayObjectCopyReallocator@V?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@@@@@ABV?$AcArray@HV?$AcArrayMemCopyReallocator@H@@@@9ABV?$AcArray@V?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@V?$AcArrayObjectCopyReallocator@V?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@@@@@ABV?$AcArray@HV?$AcArrayMemCopyReallocator@H@@@@ABV?$AcArray@PAVAcGeCurve3d@@V?$AcArrayMemCopyReallocator@PAVAcGeCurve3d@@@@@@6PB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr inputPointFilter(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isA@AcGzGizmo@@UBEPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isA(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?isPersistent@AcGzGizmo@@UBEHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int isPersistent(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?model@AcGzGizmo@@UBEPAVAcGsModel@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr model(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?onDisplay@AcGzGizmo@@UAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void onDisplay(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?onErased@AcGzGizmo@@UAEXPAVAcGsModel@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void onErased(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?onHide@AcGzGizmo@@UAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void onHide(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?orientation@AcGzGizmo@@UBE?AVAcGeMatrix3d@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr orientation(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?position@AcGzGizmo@@UBE?AVAcGePoint3d@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr position(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?redraw@AcGzGizmo@@UAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void redraw(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?rxInit@AcGzGizmo@@SAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void rxInit(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?setAttributes@AcGzGizmo@@UAEKPAVAcGiDrawableTraits@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint setAttributes(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?setGsNode@AcGzGizmo@@UAEXPAVAcGsNode@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setGsNode(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?setModel@AcGzGizmo@@UAEXPAVAcGsModel@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setModel(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?setOrientation@AcGzGizmo@@UAEXVAcGeMatrix3d@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setOrientation(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?setPosition@AcGzGizmo@@UAEXVAcGePoint3d@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setPosition(IntPtr thisPtr, IntPtr arg0);

    }

    /// <summary>
    /// AcGzManager 类的Native方法 (6 个函数)
    /// </summary>
    public static class AcGzManager
    {
        [DllImport("acad.exe", EntryPoint = "?addReactor@AcGzManager@@QAEXPAVAcGzManagerReactor@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void addReactor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?current@AcGzManager@@UBEPAVAcGzGizmo@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr current(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?getGizmo@AcGzManager@@UAE?AW4ErrorStatus@Acad@@PB_WAAPAVAcGzGizmo@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getGizmo(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?hide@AcGzManager@@UAE?AW4ErrorStatus@Acad@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr hide(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?removeReactor@AcGzManager@@QAEXPAVAcGzManagerReactor@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void removeReactor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?show@AcGzManager@@UAE?AW4ErrorStatus@Acad@@PAVAcGzGizmo@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr show(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

    }

    /// <summary>
    /// AcGzManagerReactor 类的Native方法 (2 个函数)
    /// </summary>
    public static class AcGzManagerReactor
    {
        [DllImport("acad.exe", EntryPoint = "?gizmoToBeDisplayed@AcGzManagerReactor@@UAEXPAVAcGzGizmo@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void gizmoToBeDisplayed(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?gizmoToBeHidden@AcGzManagerReactor@@UAEXPAVAcGzGizmo@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void gizmoToBeHidden(IntPtr thisPtr, IntPtr arg0);

    }

    /// <summary>
    /// AcGzMove 类的Native方法 (16 个函数)
    /// </summary>
    public static class AcGzMove
    {
        [DllImport("acad.exe", EntryPoint = "?GizmoFactory@AcGzMove@@SAPAVAcGzClassFactory@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr GizmoFactory(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?GizmoName@AcGzMove@@SAPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr GizmoName(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?RolloverHit@AcGzMove@@UAEHKKH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int RolloverHit(IntPtr thisPtr, uint arg0, uint arg1, int arg2);

        [DllImport("acad.exe", EntryPoint = "?cast@AcGzMove@@SAPAV1@PBVAcRxObject@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr cast(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?desc@AcGzMove@@SAPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr desc(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?factory@AcGzMove@@UBEPAVAcGzClassFactory@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr factory(IntPtr thisPtr);

        /// <summary>
        /// 数据符号 - AcGzMove
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "?gpDesc@AcGzMove@@2PAVAcRxClass@@A", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gpDesc();

        [DllImport("acad.exe", EntryPoint = "?inputPointFilter@AcGzMove@@UAE?AW4ErrorStatus@Acad@@AA_NAAVAcGePoint3d@@00AAPA_W0PAVAcGiViewportDraw@@PAVAcApDocument@@_NHABV4@6666W4OsnapMask@AcDb@@ABV?$AcArray@PAVAcDbCustomOsnapMode@@V?$AcArrayMemCopyReallocator@PAVAcDbCustomOsnapMode@@@@@@78ABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@ABV?$AcArray@V?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@V?$AcArrayObjectCopyReallocator@V?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@@@@@ABV?$AcArray@HV?$AcArrayMemCopyReallocator@H@@@@9ABV?$AcArray@V?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@V?$AcArrayObjectCopyReallocator@V?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@@@@@ABV?$AcArray@HV?$AcArrayMemCopyReallocator@H@@@@ABV?$AcArray@PAVAcGeCurve3d@@V?$AcArrayMemCopyReallocator@PAVAcGeCurve3d@@@@@@6PB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr inputPointFilter(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isA@AcGzMove@@UBEPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isA(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?name@AcGzMove@@UBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr name(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?onDisplay@AcGzMove@@UAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void onDisplay(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?onErased@AcGzMove@@UAEXPAVAcGsModel@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void onErased(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?onHide@AcGzMove@@UAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void onHide(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?rxInit@AcGzMove@@SAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void rxInit(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?viewportDraw@AcGzMove@@UAEXPAVAcGiViewportDraw@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void viewportDraw(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?worldDraw@AcGzMove@@UAEHPAVAcGiWorldDraw@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int worldDraw(IntPtr thisPtr, IntPtr arg0);

    }

    /// <summary>
    /// AcGzRotate 类的Native方法 (16 个函数)
    /// </summary>
    public static class AcGzRotate
    {
        [DllImport("acad.exe", EntryPoint = "?GizmoFactory@AcGzRotate@@SAPAVAcGzClassFactory@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr GizmoFactory(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?GizmoName@AcGzRotate@@SAPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr GizmoName(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?RolloverHit@AcGzRotate@@UAEHKKH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int RolloverHit(IntPtr thisPtr, uint arg0, uint arg1, int arg2);

        [DllImport("acad.exe", EntryPoint = "?cast@AcGzRotate@@SAPAV1@PBVAcRxObject@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr cast(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?desc@AcGzRotate@@SAPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr desc(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?factory@AcGzRotate@@UBEPAVAcGzClassFactory@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr factory(IntPtr thisPtr);

        /// <summary>
        /// 数据符号 - AcGzRotate
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "?gpDesc@AcGzRotate@@2PAVAcRxClass@@A", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gpDesc();

        [DllImport("acad.exe", EntryPoint = "?inputPointFilter@AcGzRotate@@UAE?AW4ErrorStatus@Acad@@AA_NAAVAcGePoint3d@@00AAPA_W0PAVAcGiViewportDraw@@PAVAcApDocument@@_NHABV4@6666W4OsnapMask@AcDb@@ABV?$AcArray@PAVAcDbCustomOsnapMode@@V?$AcArrayMemCopyReallocator@PAVAcDbCustomOsnapMode@@@@@@78ABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@ABV?$AcArray@V?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@V?$AcArrayObjectCopyReallocator@V?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@@@@@ABV?$AcArray@HV?$AcArrayMemCopyReallocator@H@@@@9ABV?$AcArray@V?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@V?$AcArrayObjectCopyReallocator@V?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@@@@@ABV?$AcArray@HV?$AcArrayMemCopyReallocator@H@@@@ABV?$AcArray@PAVAcGeCurve3d@@V?$AcArrayMemCopyReallocator@PAVAcGeCurve3d@@@@@@6PB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr inputPointFilter(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isA@AcGzRotate@@UBEPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isA(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?name@AcGzRotate@@UBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr name(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?onDisplay@AcGzRotate@@UAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void onDisplay(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?onErased@AcGzRotate@@UAEXPAVAcGsModel@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void onErased(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?onHide@AcGzRotate@@UAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void onHide(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?rxInit@AcGzRotate@@SAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void rxInit(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?viewportDraw@AcGzRotate@@UAEXPAVAcGiViewportDraw@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void viewportDraw(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?worldDraw@AcGzRotate@@UAEHPAVAcGiWorldDraw@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int worldDraw(IntPtr thisPtr, IntPtr arg0);

    }

    /// <summary>
    /// AcGzService 类的Native方法 (5 个函数)
    /// </summary>
    public static class AcGzService
    {
        [DllImport("acad.exe", EntryPoint = "?createGizmo@AcGzService@@UBE?AW4ErrorStatus@Acad@@PB_WAAPAVAcGzGizmo@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr createGizmo(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getService@AcGzService@@SAPAV1@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getService(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?manager@AcGzService@@UAEPAVAcGzManager@@PAVAcApDocument@@_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr manager(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?registerGizmo@AcGzService@@UAE?AW4ErrorStatus@Acad@@PB_WPAVAcGzClassFactory@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr registerGizmo(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?unregisterGizmo@AcGzService@@UAE?AW4ErrorStatus@Acad@@PB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr unregisterGizmo(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

    }

    /// <summary>
    /// AcGzTransform 类的Native方法 (17 个函数)
    /// </summary>
    public static class AcGzTransform
    {
        [DllImport("acad.exe", EntryPoint = "??0AcGzTransform@@IAE@PAVAcGzGizmoImp@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "??0AcGzTransform@@IAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor_1(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcGzTransform@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?addReactor@AcGzTransform@@QAEXPAVAcGzTransformReactor@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void addReactor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?cast@AcGzTransform@@SAPAV1@PBVAcRxObject@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr cast(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?constraint@AcGzTransform@@UBE?AW4ConstraintType@1@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr constraint(IntPtr thisPtr, IntPtr arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14);

        [DllImport("acad.exe", EntryPoint = "?desc@AcGzTransform@@SAPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr desc(IntPtr thisPtr);

        /// <summary>
        /// 数据符号 - AcGzTransform
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "?gpDesc@AcGzTransform@@2PAVAcRxClass@@A", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gpDesc();

        [DllImport("acad.exe", EntryPoint = "?gripState@AcGzTransform@@UBE?AW4DrawType@AcDbGripOperations@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr gripState(IntPtr thisPtr, IntPtr arg0, int arg1, sbyte arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9);

        [DllImport("acad.exe", EntryPoint = "?isA@AcGzTransform@@UBEPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isA(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?onDisplay@AcGzTransform@@UAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void onDisplay(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?onHide@AcGzTransform@@UAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void onHide(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?removeReactor@AcGzTransform@@QAEXPAVAcGzTransformReactor@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void removeReactor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?reset@AcGzTransform@@UAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void reset(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?rxInit@AcGzTransform@@SAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void rxInit(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?setConstraint@AcGzTransform@@UAEXW4ConstraintType@1@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setConstraint(IntPtr thisPtr, char arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14);

        [DllImport("acad.exe", EntryPoint = "?setGripState@AcGzTransform@@UAEXW4DrawType@AcDbGripOperations@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setGripState(IntPtr thisPtr, char arg0, int arg1, sbyte arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9);

    }

    /// <summary>
    /// AcGzTransformReactor 类的Native方法 (1 个函数)
    /// </summary>
    public static class AcGzTransformReactor
    {
        [DllImport("acad.exe", EntryPoint = "?constraintModified@AcGzTransformReactor@@UAEXW4ConstraintType@AcGzTransform@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void constraintModified(IntPtr thisPtr, char arg0, int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14);

    }

    /// <summary>
    /// AcGzUtil 类的Native方法 (4 个函数)
    /// </summary>
    public static class AcGzUtil
    {
        [DllImport("acad.exe", EntryPoint = "?drawConstraint@AcGzUtil@@YA?AW4ErrorStatus@Acad@@PAVAcGiViewportDraw@@PBVAcGzTransform@@ABVAcCmEntityColor@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr drawConstraint(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?drawGrip@AcGzUtil@@YA?AW4ErrorStatus@Acad@@PAVAcGiViewportDraw@@ABVAcGePoint3d@@W4DrawType@AcDbGripOperations@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr drawGrip(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getGripColor@AcGzUtil@@YAHW4DrawType@AcDbGripOperations@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int getGripColor(IntPtr thisPtr, char arg0, int arg1, sbyte arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9);

        [DllImport("acad.exe", EntryPoint = "?willShowGizmos@AcGzUtil@@YA_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool willShowGizmos(IntPtr thisPtr);

    }

    /// <summary>
    /// AcPane 类的Native方法 (15 个函数)
    /// </summary>
    public static class AcPane
    {
        [DllImport("acad.exe", EntryPoint = "??0AcPane@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcPane@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        /// <summary>
        /// 虚函数表 - AcPane
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "??_7AcPane@@6B@", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr vftable();

        /// <summary>
        /// 数据符号 - AcPane
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "??_GAcPane@@UAEPAXI@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr scalar_dtor();

        [DllImport("acad.exe", EntryPoint = "?DisplayPopupPaneMenu@AcPane@@UAEIAAVCMenu@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint DisplayPopupPaneMenu(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?GetIcon@AcPane@@UBEPAUHICON__@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr GetIcon(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?GetMaxWidth@AcPane@@UAEHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int GetMaxWidth(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?GetMinWidth@AcPane@@UAEHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int GetMinWidth(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?GetStyle@AcPane@@UBEHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int GetStyle(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?GetText@AcPane@@UBEHAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int GetText(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?SetIcon@AcPane@@UAEHPAUHICON__@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int SetIcon(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?SetMaxWidth@AcPane@@UAEHH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int SetMaxWidth(IntPtr thisPtr, int arg0);

        [DllImport("acad.exe", EntryPoint = "?SetMinWidth@AcPane@@UAEHH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int SetMinWidth(IntPtr thisPtr, int arg0);

        [DllImport("acad.exe", EntryPoint = "?SetStyle@AcPane@@UAEHH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int SetStyle(IntPtr thisPtr, int arg0);

        [DllImport("acad.exe", EntryPoint = "?SetText@AcPane@@UAEHABV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int SetText(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

    }

    /// <summary>
    /// AcPlDSDData 类的Native方法 (55 个函数)
    /// </summary>
    public static class AcPlDSDData
    {
        [DllImport("acad.exe", EntryPoint = "??0AcPlDSDData@@QAE@ABV0@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "??0AcPlDSDData@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor_1(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcPlDSDData@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??4AcPlDSDData@@QAEAAV0@ABV0@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr operator_(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?DSDEntryAt@AcPlDSDData@@QAEAAVAcPlDSDEntry@@H@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr DSDEntryAt(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?categoryName@AcPlDSDData@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr categoryName(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?desc@AcPlDSDData@@SAPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr desc(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?destinationName@AcPlDSDData@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr destinationName(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?get3dDwfOptions@AcPlDSDData@@QBEABUAcPl3dDwfOptions@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr get3dDwfOptions(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?getDSDEntries@AcPlDSDData@@QBEXAAV?$AcArray@VAcPlDSDEntry@@V?$AcArrayObjectCopyReallocator@VAcPlDSDEntry@@@@@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void getDSDEntries(IntPtr thisPtr, IntPtr arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?getUnrecognizedData@AcPlDSDData@@QBEXAAV?$AcArray@PA_WV?$AcArrayMemCopyReallocator@PA_W@@@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void getUnrecognizedData(IntPtr thisPtr, IntPtr arg0, string arg1, IntPtr arg2, string arg3);

        /// <summary>
        /// 数据符号 - AcPlDSDData
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "?gpDesc@AcPlDSDData@@2PAVAcRxClass@@A", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gpDesc();

        [DllImport("acad.exe", EntryPoint = "?includeLayerInfo@AcPlDSDData@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool includeLayerInfo(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?isA@AcPlDSDData@@UBEPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isA(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?isHomogeneous@AcPlDSDData@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool isHomogeneous(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?isSheetSet@AcPlDSDData@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool isSheetSet(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?logFilePath@AcPlDSDData@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr logFilePath(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?majorVersion@AcPlDSDData@@QBEIXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint majorVersion(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?minorVersion@AcPlDSDData@@QBEIXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint minorVersion(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?noOfCopies@AcPlDSDData@@QBEIXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint noOfCopies(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?numberOfDSDEntries@AcPlDSDData@@QBEHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int numberOfDSDEntries(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?password@AcPlDSDData@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr password(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?plotStampOn@AcPlDSDData@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool plotStampOn(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?projectPath@AcPlDSDData@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr projectPath(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?promptForDwfName@AcPlDSDData@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool promptForDwfName(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?promptForPassword@AcPlDSDData@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool promptForPassword(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?pwdProtectPublishedDWF@AcPlDSDData@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool pwdProtectPublishedDWF(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?readDSD@AcPlDSDData@@QAE_NPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool readDSD(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?rxInit@AcPlDSDData@@SAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void rxInit(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?selectionSetName@AcPlDSDData@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr selectionSetName(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?set3dDwfOptions@AcPlDSDData@@QAEXABUAcPl3dDwfOptions@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void set3dDwfOptions(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?setCategoryName@AcPlDSDData@@QAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setCategoryName(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?setDSDEntries@AcPlDSDData@@QAEXABV?$AcArray@VAcPlDSDEntry@@V?$AcArrayObjectCopyReallocator@VAcPlDSDEntry@@@@@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setDSDEntries(IntPtr thisPtr, IntPtr arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?setDestinationName@AcPlDSDData@@QAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setDestinationName(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?setIncludeLayerInfo@AcPlDSDData@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setIncludeLayerInfo(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setIsHomogeneous@AcPlDSDData@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setIsHomogeneous(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setIsSheetSet@AcPlDSDData@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setIsSheetSet(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setLogFilePath@AcPlDSDData@@QAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setLogFilePath(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?setMajorVersion@AcPlDSDData@@QAEXI@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setMajorVersion(IntPtr thisPtr, uint arg0);

        [DllImport("acad.exe", EntryPoint = "?setMinorVersion@AcPlDSDData@@QAEXI@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setMinorVersion(IntPtr thisPtr, uint arg0);

        [DllImport("acad.exe", EntryPoint = "?setNoOfCopies@AcPlDSDData@@QAEXI@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setNoOfCopies(IntPtr thisPtr, uint arg0);

        [DllImport("acad.exe", EntryPoint = "?setPassword@AcPlDSDData@@QAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setPassword(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?setPlotStampOn@AcPlDSDData@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setPlotStampOn(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setProjectPath@AcPlDSDData@@QAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setProjectPath(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?setPromptForDwfName@AcPlDSDData@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setPromptForDwfName(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setPromptForPassword@AcPlDSDData@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setPromptForPassword(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setPwdProtectPublishedDWF@AcPlDSDData@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setPwdProtectPublishedDWF(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setSelectionSetName@AcPlDSDData@@QAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setSelectionSetName(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?setSheetSetName@AcPlDSDData@@QAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setSheetSetName(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?setSheetType@AcPlDSDData@@QAEXW4SheetType@AcPlDSDEntry@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setSheetType(IntPtr thisPtr, char arg0, int arg1, IntPtr arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8);

        [DllImport("acad.exe", EntryPoint = "?setUnrecognizedData@AcPlDSDData@@QAEXABV?$AcArray@PA_WV?$AcArrayMemCopyReallocator@PA_W@@@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setUnrecognizedData(IntPtr thisPtr, IntPtr arg0, string arg1, IntPtr arg2, string arg3);

        [DllImport("acad.exe", EntryPoint = "?setUnrecognizedData@AcPlDSDData@@QAEXPB_W0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setUnrecognizedData_1(IntPtr thisPtr, string arg0, int arg1);

        [DllImport("acad.exe", EntryPoint = "?sheetSetName@AcPlDSDData@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr sheetSetName(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?sheetType@AcPlDSDData@@QBE?AW4SheetType@AcPlDSDEntry@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr sheetType(IntPtr thisPtr, IntPtr arg0, int arg1, IntPtr arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8);

        [DllImport("acad.exe", EntryPoint = "?writeDSD@AcPlDSDData@@QAE_NPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool writeDSD(IntPtr thisPtr, string arg0);

    }

    /// <summary>
    /// AcPlDSDDataImp 类的Native方法 (6 个函数)
    /// </summary>
    public static class AcPlDSDDataImp
    {
        [DllImport("acad.exe", EntryPoint = "?acadProfile@AcPlDSDDataImp@@QBE?AV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr acadProfile(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?jobID@AcPlDSDDataImp@@QBEIXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint jobID(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?setAcadProfile@AcPlDSDDataImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setAcadProfile(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setJobID@AcPlDSDDataImp@@QAEXI@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setJobID(IntPtr thisPtr, uint arg0);

        [DllImport("acad.exe", EntryPoint = "?setTempDWFPC3Path@AcPlDSDDataImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setTempDWFPC3Path(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?tempDWFPC3Path@AcPlDSDDataImp@@QBE?AV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr tempDWFPC3Path(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

    }

    /// <summary>
    /// AcPlDSDEntry 类的Native方法 (20 个函数)
    /// </summary>
    public static class AcPlDSDEntry
    {
        [DllImport("acad.exe", EntryPoint = "??0AcPlDSDEntry@@QAE@ABV0@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "??0AcPlDSDEntry@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor_1(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcPlDSDEntry@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??4AcPlDSDEntry@@QAEAAV0@ABV0@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr operator_(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?NPS@AcPlDSDEntry@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr NPS(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?NPSSourceDWG@AcPlDSDEntry@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr NPSSourceDWG(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?desc@AcPlDSDEntry@@SAPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr desc(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?dwgName@AcPlDSDEntry@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr dwgName(IntPtr thisPtr);

        /// <summary>
        /// 数据符号 - AcPlDSDEntry
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "?gpDesc@AcPlDSDEntry@@2PAVAcRxClass@@A", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gpDesc();

        [DllImport("acad.exe", EntryPoint = "?has3dDwfSetup@AcPlDSDEntry@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool has3dDwfSetup(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?isA@AcPlDSDEntry@@UBEPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isA(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?layout@AcPlDSDEntry@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr layout(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?rxInit@AcPlDSDEntry@@SAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void rxInit(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?setDwgName@AcPlDSDEntry@@QAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setDwgName(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?setHas3dDwfSetup@AcPlDSDEntry@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setHas3dDwfSetup(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setLayout@AcPlDSDEntry@@QAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setLayout(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?setNPS@AcPlDSDEntry@@QAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setNPS(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?setNPSSourceDWG@AcPlDSDEntry@@QAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setNPSSourceDWG(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?setTitle@AcPlDSDEntry@@QAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setTitle(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?title@AcPlDSDEntry@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr title(IntPtr thisPtr);

    }

    /// <summary>
    /// AcPlDSDEntryImp 类的Native方法 (54 个函数)
    /// </summary>
    public static class AcPlDSDEntryImp
    {
        [DllImport("acad.exe", EntryPoint = "?docNumCopies@AcPlDSDEntryImp@@QBEIXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint docNumCopies(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?documentName@AcPlDSDEntryImp@@QBE?AV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr documentName(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?finalFilePath@AcPlDSDEntryImp@@QBE?AV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr finalFilePath(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?has3dDwfSetup@AcPlDSDEntryImp@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool has3dDwfSetup(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?hasPlotPort@AcPlDSDEntryImp@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool hasPlotPort(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?llx@AcPlDSDEntryImp@@QBE_NAAN@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool llx(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?llx@AcPlDSDEntryImp@@QBE_NAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool llx_1(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?lly@AcPlDSDEntryImp@@QBE_NAAN@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool lly(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?lly@AcPlDSDEntryImp@@QBE_NAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool lly_1(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?logNPS@AcPlDSDEntryImp@@QBE?AV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr logNPS(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?logNPSSourceDWG@AcPlDSDEntryImp@@QBE?AV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr logNPSSourceDWG(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?markupPropertiesFilePath@AcPlDSDEntryImp@@QBE_NAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool markupPropertiesFilePath(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?multipleCopies@AcPlDSDEntryImp@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool multipleCopies(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?orgSheetPath@AcPlDSDEntryImp@@QBE?AV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr orgSheetPath(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?orginalDwfName@AcPlDSDEntryImp@@QBE?AV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr orginalDwfName(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?parentDwfFilePath@AcPlDSDEntryImp@@QBE?AV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr parentDwfFilePath(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?plotStatus@AcPlDSDEntryImp@@QBE?AW4PlotStatus@1@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr plotStatus(IntPtr thisPtr, IntPtr arg0, int arg1, IntPtr arg2, int arg3, IntPtr arg4, int arg5, int arg6, int arg7);

        [DllImport("acad.exe", EntryPoint = "?plotToFileName@AcPlDSDEntryImp@@QBE?AV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr plotToFileName(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setDocNumCopies@AcPlDSDEntryImp@@QAEXI@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setDocNumCopies(IntPtr thisPtr, uint arg0);

        [DllImport("acad.exe", EntryPoint = "?setDocumentName@AcPlDSDEntryImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setDocumentName(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setFinalFilePath@AcPlDSDEntryImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setFinalFilePath(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setHas3dDwfSetup@AcPlDSDEntryImp@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setHas3dDwfSetup(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setHasPlotPort@AcPlDSDEntryImp@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setHasPlotPort(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setLlx@AcPlDSDEntryImp@@QAEXN@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setLlx(IntPtr thisPtr, double arg0);

        [DllImport("acad.exe", EntryPoint = "?setLlx@AcPlDSDEntryImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setLlx_1(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setLly@AcPlDSDEntryImp@@QAEXN@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setLly(IntPtr thisPtr, double arg0);

        [DllImport("acad.exe", EntryPoint = "?setLly@AcPlDSDEntryImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setLly_1(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setLogNPS@AcPlDSDEntryImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setLogNPS(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setLogNPSSourceDWG@AcPlDSDEntryImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setLogNPSSourceDWG(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setMarkupPropertiesFilePath@AcPlDSDEntryImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setMarkupPropertiesFilePath(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setMultipleCopies@AcPlDSDEntryImp@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setMultipleCopies(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setOrgSheetPath@AcPlDSDEntryImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setOrgSheetPath(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setOriginalDwfName@AcPlDSDEntryImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setOriginalDwfName(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setParentDwfFilePath@AcPlDSDEntryImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setParentDwfFilePath(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setPlotStatus@AcPlDSDEntryImp@@QAEXW4PlotStatus@1@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setPlotStatus(IntPtr thisPtr, char arg0, int arg1, IntPtr arg2, int arg3, IntPtr arg4, int arg5, int arg6, int arg7);

        [DllImport("acad.exe", EntryPoint = "?setPlotToFileName@AcPlDSDEntryImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setPlotToFileName(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setSetupType@AcPlDSDEntryImp@@QAEXW4SetupType@1@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setSetupType(IntPtr thisPtr, char arg0, int arg1, IntPtr arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8);

        [DllImport("acad.exe", EntryPoint = "?setSheetIdentifier@AcPlDSDEntryImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setSheetIdentifier(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setTempDwfPath@AcPlDSDEntryImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setTempDwfPath(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setTempFilePath@AcPlDSDEntryImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setTempFilePath(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setUrx@AcPlDSDEntryImp@@QAEXN@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setUrx(IntPtr thisPtr, double arg0);

        [DllImport("acad.exe", EntryPoint = "?setUrx@AcPlDSDEntryImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setUrx_1(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setUry@AcPlDSDEntryImp@@QAEXN@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setUry(IntPtr thisPtr, double arg0);

        [DllImport("acad.exe", EntryPoint = "?setUry@AcPlDSDEntryImp@@QAEXV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setUry_1(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?setUseLogNPS@AcPlDSDEntryImp@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setUseLogNPS(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setupType@AcPlDSDEntryImp@@QBE?AW4SetupType@1@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr setupType(IntPtr thisPtr, IntPtr arg0, int arg1, IntPtr arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8);

        [DllImport("acad.exe", EntryPoint = "?sheetIdentifier@AcPlDSDEntryImp@@QBE?AV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr sheetIdentifier(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?tempDwfPath@AcPlDSDEntryImp@@QBE?AV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr tempDwfPath(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?tempFilePath@AcPlDSDEntryImp@@QBE?AV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr tempFilePath(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?urx@AcPlDSDEntryImp@@QBE_NAAN@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool urx(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?urx@AcPlDSDEntryImp@@QBE_NAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool urx_1(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?ury@AcPlDSDEntryImp@@QBE_NAAN@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool ury(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?ury@AcPlDSDEntryImp@@QBE_NAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool ury_1(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?useLogNPS@AcPlDSDEntryImp@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool useLogNPS(IntPtr thisPtr);

    }

    /// <summary>
    /// AcPlDummy 类的Native方法 (5 个函数)
    /// </summary>
    public static class AcPlDummy
    {
        [DllImport("acad.exe", EntryPoint = "?cast@AcPlDummy@@SAPAV1@PBVAcRxObject@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr cast(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?desc@AcPlDummy@@SAPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr desc(IntPtr thisPtr);

        /// <summary>
        /// 数据符号 - AcPlDummy
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "?gpDesc@AcPlDummy@@2PAVAcRxClass@@A", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gpDesc();

        [DllImport("acad.exe", EntryPoint = "?isA@AcPlDummy@@UBEPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isA(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?rxInit@AcPlDummy@@SAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void rxInit(IntPtr thisPtr);

    }

    /// <summary>
    /// AcPlObject 类的Native方法 (1 个函数)
    /// </summary>
    public static class AcPlObject
    {
        [DllImport("acad.exe", EntryPoint = "??1AcPlObject@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

    }

    /// <summary>
    /// AcPlPlotConfig 类的Native方法 (24 个函数)
    /// </summary>
    public static class AcPlPlotConfig
    {
        [DllImport("acad.exe", EntryPoint = "??1AcPlPlotConfig@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?copyFrom@AcPlPlotConfig@@UAE?AW4ErrorStatus@Acad@@PBVAcRxObject@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr copyFrom(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?desc@AcPlPlotConfig@@SAPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr desc(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?deviceName@AcPlPlotConfig@@UBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr deviceName(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?deviceType@AcPlPlotConfig@@UBEKXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint deviceType(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?fullPath@AcPlPlotConfig@@UBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr fullPath(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?getCanonicalMediaNameList@AcPlPlotConfig@@UBEXAAV?$AcArray@PA_WV?$AcArrayMemCopyReallocator@PA_W@@@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void getCanonicalMediaNameList(IntPtr thisPtr, IntPtr arg0, string arg1, IntPtr arg2, string arg3);

        [DllImport("acad.exe", EntryPoint = "?getDefaultFileExtension@AcPlPlotConfig@@UBE?AW4ErrorStatus@Acad@@AAPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getDefaultFileExtension(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getDescriptionFields@AcPlPlotConfig@@UBEXAAPA_W00000@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void getDescriptionFields(IntPtr thisPtr, string arg0, int arg1, int arg2, int arg3, int arg4, int arg5);

        [DllImport("acad.exe", EntryPoint = "?getLocalMediaName@AcPlPlotConfig@@UBEXPB_WAAPA_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void getLocalMediaName(IntPtr thisPtr, string arg0, string arg1);

        [DllImport("acad.exe", EntryPoint = "?getMediaBounds@AcPlPlotConfig@@UBEXPB_WAAVAcGePoint2d@@AAVAcGeBoundBlock2d@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void getMediaBounds(IntPtr thisPtr, string arg0, IntPtr arg1);

        /// <summary>
        /// 数据符号 - AcPlPlotConfig
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "?gpDesc@AcPlPlotConfig@@2PAVAcRxClass@@A", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gpDesc();

        [DllImport("acad.exe", EntryPoint = "?isA@AcPlPlotConfig@@UBEPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isA(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?isCustomPossible@AcPlPlotConfig@@UBEKNN_N0HPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint isCustomPossible(IntPtr thisPtr, double arg0, double arg1, bool arg2, int arg3, int arg4, string arg5);

        [DllImport("acad.exe", EntryPoint = "?isPlotToFile@AcPlPlotConfig@@UBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool isPlotToFile(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?makeCustomMediaFromSizeDesc@AcPlPlotConfig@@UAE?AW4ErrorStatus@Acad@@PAVHT_Media_Size@@PAVHT_Media_Description@@_N2PB_W3AAPA_W444HAAN@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr makeCustomMediaFromSizeDesc(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?matchMediaSize@AcPlPlotConfig@@UAE_NNNNNW4PlotPaperUnits@AcDbPlotSettings@@_NHPB_WAAPA_W3AAN@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool matchMediaSize(IntPtr thisPtr, double arg0, double arg1, double arg2, double arg3, char arg4, int arg5, IntPtr arg6, int arg7, IntPtr arg8, int arg9, int arg10, IntPtr arg11, IntPtr arg12, sbyte arg13, int arg14, IntPtr arg15, int arg16, IntPtr arg17, int arg18, int arg19, int arg20, int arg21, int arg22);

        [DllImport("acad.exe", EntryPoint = "?maxDeviceDPI@AcPlPlotConfig@@UBEIXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint maxDeviceDPI(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?plotConfig@AcPlPlotConfig@@UBEPAVHT_Plot_Config@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr plotConfig(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?plotToFileCapability@AcPlPlotConfig@@UBE?AW4PlotToFileCapability@1@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr plotToFileCapability(IntPtr thisPtr, IntPtr arg0, int arg1, IntPtr arg2, int arg3, int arg4, int arg5, short arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, int arg14, int arg15, int arg16, int arg17, int arg18);

        [DllImport("acad.exe", EntryPoint = "?refreshMediaNameList@AcPlPlotConfig@@UAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void refreshMediaNameList(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?rxInit@AcPlPlotConfig@@SAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void rxInit(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?saveToPC3@AcPlPlotConfig@@UAE_NPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool saveToPC3(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?setPlotToFile@AcPlPlotConfig@@UAE?AW4ErrorStatus@Acad@@_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr setPlotToFile(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

    }

    /// <summary>
    /// AcPlPlotConfigInfo 类的Native方法 (17 个函数)
    /// </summary>
    public static class AcPlPlotConfigInfo
    {
        [DllImport("acad.exe", EntryPoint = "??0AcPlPlotConfigInfo@@QAE@ABV0@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "??0AcPlPlotConfigInfo@@QAE@PB_W0W4DeviceType@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor_1(IntPtr thisPtr, string arg0, int arg1, char arg2, int arg3, sbyte arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13);

        [DllImport("acad.exe", EntryPoint = "??0AcPlPlotConfigInfo@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor_2(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcPlPlotConfigInfo@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??4AcPlPlotConfigInfo@@QAEABV0@ABV0@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr operator_(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?cast@AcPlPlotConfigInfo@@SAPAV1@PBVAcRxObject@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr cast(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?copyFrom@AcPlPlotConfigInfo@@UAE?AW4ErrorStatus@Acad@@PBVAcRxObject@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr copyFrom(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?desc@AcPlPlotConfigInfo@@SAPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr desc(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?deviceName@AcPlPlotConfigInfo@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr deviceName(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?deviceType@AcPlPlotConfigInfo@@QBE?AW4DeviceType@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr deviceType(IntPtr thisPtr, IntPtr arg0, int arg1, sbyte arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11);

        [DllImport("acad.exe", EntryPoint = "?fullPath@AcPlPlotConfigInfo@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr fullPath(IntPtr thisPtr);

        /// <summary>
        /// 数据符号 - AcPlPlotConfigInfo
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "?gpDesc@AcPlPlotConfigInfo@@2PAVAcRxClass@@A", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gpDesc();

        [DllImport("acad.exe", EntryPoint = "?isA@AcPlPlotConfigInfo@@UBEPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isA(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?rxInit@AcPlPlotConfigInfo@@SAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void rxInit(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?setDeviceName@AcPlPlotConfigInfo@@QAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setDeviceName(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?setDeviceType@AcPlPlotConfigInfo@@QAEXW4DeviceType@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setDeviceType(IntPtr thisPtr, char arg0, int arg1, sbyte arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11);

        [DllImport("acad.exe", EntryPoint = "?setFullPath@AcPlPlotConfigInfo@@QAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setFullPath(IntPtr thisPtr, string arg0);

    }

    /// <summary>
    /// AcPlPlotErrorHandlerLock 类的Native方法 (11 个函数)
    /// </summary>
    public static class AcPlPlotErrorHandlerLock
    {
        [DllImport("acad.exe", EntryPoint = "??0AcPlPlotErrorHandlerLock@@QAE@PAVAcPlPlotErrorHandler@@PB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "??0AcPlPlotErrorHandlerLock@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor_1(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?appName@AcPlPlotErrorHandlerLock@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr appName(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?desc@AcPlPlotErrorHandlerLock@@SAPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr desc(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?getErrorHandler@AcPlPlotErrorHandlerLock@@QBEXAAPAVAcPlPlotErrorHandler@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void getErrorHandler(IntPtr thisPtr, IntPtr arg0);

        /// <summary>
        /// 数据符号 - AcPlPlotErrorHandlerLock
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "?gpDesc@AcPlPlotErrorHandlerLock@@2PAVAcRxClass@@A", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gpDesc();

        [DllImport("acad.exe", EntryPoint = "?isA@AcPlPlotErrorHandlerLock@@UBEPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isA(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?lock@AcPlPlotErrorHandlerLock@@QAE_NPAVAcPlPlotErrorHandler@@PB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool _lock(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?rxInit@AcPlPlotErrorHandlerLock@@SAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void rxInit(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?status@AcPlPlotErrorHandlerLock@@QBE?AW4LockStatus@1@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr status(IntPtr thisPtr, IntPtr arg0, int arg1, int arg2, int arg3, int arg4, int arg5, IntPtr arg6, int arg7, int arg8, int arg9);

        [DllImport("acad.exe", EntryPoint = "?unLock@AcPlPlotErrorHandlerLock@@QAE_NPAVAcPlPlotErrorHandler@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool unLock(IntPtr thisPtr, IntPtr arg0);

    }

    /// <summary>
    /// AcPlPlotFactory 类的Native方法 (2 个函数)
    /// </summary>
    public static class AcPlPlotFactory
    {
        [DllImport("acad.exe", EntryPoint = "?createPreviewEngine@AcPlPlotFactory@@SA?AW4ErrorStatus@Acad@@AAPAVAcPlPlotEngine@@J@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr createPreviewEngine(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?createPublishEngine@AcPlPlotFactory@@SA?AW4ErrorStatus@Acad@@AAPAVAcPlPlotEngine@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr createPublishEngine(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

    }

    /// <summary>
    /// AcPlPlotInfo 类的Native方法 (21 个函数)
    /// </summary>
    public static class AcPlPlotInfo
    {
        [DllImport("acad.exe", EntryPoint = "??0AcPlPlotInfo@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcPlPlotInfo@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?cast@AcPlPlotInfo@@SAPAV1@PBVAcRxObject@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr cast(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?copyFrom@AcPlPlotInfo@@UAE?AW4ErrorStatus@Acad@@PBVAcRxObject@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr copyFrom(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?desc@AcPlPlotInfo@@SAPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr desc(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?deviceOverride@AcPlPlotInfo@@QBEPBVAcPlPlotConfig@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr deviceOverride(IntPtr thisPtr);

        /// <summary>
        /// 数据符号 - AcPlPlotInfo
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "?gpDesc@AcPlPlotInfo@@2PAVAcRxClass@@A", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gpDesc();

        [DllImport("acad.exe", EntryPoint = "?isA@AcPlPlotInfo@@UBEPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isA(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?isCompatibleDocument@AcPlPlotInfo@@QBE_NPBV1@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool isCompatibleDocument(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?isValidated@AcPlPlotInfo@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool isValidated(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?layout@AcPlPlotInfo@@QBE?AVAcDbObjectId@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr layout(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?mergeStatus@AcPlPlotInfo@@QBEKXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint mergeStatus(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?overrideSettings@AcPlPlotInfo@@QBEPBVAcDbPlotSettings@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr overrideSettings(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?rxInit@AcPlPlotInfo@@SAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void rxInit(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?setDeviceOverride@AcPlPlotInfo@@QAEXPBVAcPlPlotConfig@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setDeviceOverride(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?setLayout@AcPlPlotInfo@@QAEXAAVAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setLayout(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?setOverrideSettings@AcPlPlotInfo@@QAEXPBVAcDbPlotSettings@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setOverrideSettings(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?setValidatedConfig@AcPlPlotInfo@@QAEXPBVAcPlPlotConfig@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setValidatedConfig(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?setValidatedSettings@AcPlPlotInfo@@QAE?AW4ErrorStatus@Acad@@PBVAcDbPlotSettings@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr setValidatedSettings(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?validatedConfig@AcPlPlotInfo@@QBEPBVAcPlPlotConfig@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr validatedConfig(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?validatedSettings@AcPlPlotInfo@@QBEPBVAcDbPlotSettings@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr validatedSettings(IntPtr thisPtr);

    }

    /// <summary>
    /// AcPlPlotInfoImp 类的Native方法 (5 个函数)
    /// </summary>
    public static class AcPlPlotInfoImp
    {
        [DllImport("acad.exe", EntryPoint = "?NPSPath@AcPlPlotInfoImp@@QBEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr NPSPath(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?copyPC3FileTo@AcPlPlotInfoImp@@QAE?AW4ErrorStatus@Acad@@PB_W0_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr copyPC3FileTo(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?longLivedPC3File@AcPlPlotInfoImp@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool longLivedPC3File(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?setLongLivedPC3File@AcPlPlotInfoImp@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setLongLivedPC3File(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setNPSPath@AcPlPlotInfoImp@@QAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setNPSPath(IntPtr thisPtr, string arg0);

    }

    /// <summary>
    /// AcPlPlotInfoValidator 类的Native方法 (24 个函数)
    /// </summary>
    public static class AcPlPlotInfoValidator
    {
        [DllImport("acad.exe", EntryPoint = "??0AcPlPlotInfoValidator@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcPlPlotInfoValidator@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?desc@AcPlPlotInfoValidator@@SAPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr desc(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?dimensionalWeight@AcPlPlotInfoValidator@@QBEIXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint dimensionalWeight(IntPtr thisPtr);

        /// <summary>
        /// 数据符号 - AcPlPlotInfoValidator
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "?gpDesc@AcPlPlotInfoValidator@@2PAVAcRxClass@@A", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gpDesc();

        [DllImport("acad.exe", EntryPoint = "?isA@AcPlPlotInfoValidator@@UBEPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isA(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?isCustomPossible@AcPlPlotInfoValidator@@UBEKAAVAcPlPlotInfo@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint isCustomPossible(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?matchingPolicy@AcPlPlotInfoValidator@@QBE?AW4MatchingPolicy@1@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr matchingPolicy(IntPtr thisPtr, IntPtr arg0, int arg1, float arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, IntPtr arg10, int arg11, int arg12, int arg13);

        [DllImport("acad.exe", EntryPoint = "?mediaBoundsWeight@AcPlPlotInfoValidator@@QBEIXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint mediaBoundsWeight(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?mediaGroupWeight@AcPlPlotInfoValidator@@QBEIXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint mediaGroupWeight(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?mediaMatchingThreshold@AcPlPlotInfoValidator@@QBEIXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint mediaMatchingThreshold(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?printableBoundsWeight@AcPlPlotInfoValidator@@QBEIXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint printableBoundsWeight(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?rxInit@AcPlPlotInfoValidator@@SAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void rxInit(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?setDimensionalWeight@AcPlPlotInfoValidator@@QAEXI@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setDimensionalWeight(IntPtr thisPtr, uint arg0);

        [DllImport("acad.exe", EntryPoint = "?setMediaBoundsWeight@AcPlPlotInfoValidator@@QAEXI@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setMediaBoundsWeight(IntPtr thisPtr, uint arg0);

        [DllImport("acad.exe", EntryPoint = "?setMediaGroupWeight@AcPlPlotInfoValidator@@QAEXI@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setMediaGroupWeight(IntPtr thisPtr, uint arg0);

        [DllImport("acad.exe", EntryPoint = "?setMediaMatchingPolicy@AcPlPlotInfoValidator@@QAEXW4MatchingPolicy@1@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setMediaMatchingPolicy(IntPtr thisPtr, char arg0, int arg1, float arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, IntPtr arg10, int arg11, int arg12, int arg13);

        [DllImport("acad.exe", EntryPoint = "?setMediaMatchingThreshold@AcPlPlotInfoValidator@@QAEXI@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setMediaMatchingThreshold(IntPtr thisPtr, uint arg0);

        [DllImport("acad.exe", EntryPoint = "?setPrintableBoundsWeight@AcPlPlotInfoValidator@@QAEXI@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setPrintableBoundsWeight(IntPtr thisPtr, uint arg0);

        [DllImport("acad.exe", EntryPoint = "?setSheetDimensionalWeight@AcPlPlotInfoValidator@@QAEXI@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setSheetDimensionalWeight(IntPtr thisPtr, uint arg0);

        [DllImport("acad.exe", EntryPoint = "?setSheetMediaGroupWeight@AcPlPlotInfoValidator@@QAEXI@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setSheetMediaGroupWeight(IntPtr thisPtr, uint arg0);

        [DllImport("acad.exe", EntryPoint = "?sheetDimensionalWeight@AcPlPlotInfoValidator@@QBEIXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint sheetDimensionalWeight(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?sheetMediaGroupWeight@AcPlPlotInfoValidator@@QBEIXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint sheetMediaGroupWeight(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?validate@AcPlPlotInfoValidator@@UAE?AW4ErrorStatus@Acad@@AAVAcPlPlotInfo@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr validate(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

    }

    /// <summary>
    /// AcPlPlotLoggingErrorHandler 类的Native方法 (16 个函数)
    /// </summary>
    public static class AcPlPlotLoggingErrorHandler
    {
        [DllImport("acad.exe", EntryPoint = "??0AcPlPlotLoggingErrorHandler@@QAE@PAVAcPlPlotLogger@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "??0AcPlPlotLoggingErrorHandler@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor_1(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcPlPlotLoggingErrorHandler@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?ariError@AcPlPlotLoggingErrorHandler@@UAE?AW4ErrorResult@AcPlPlotErrorHandler@@KIPB_W00@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr ariError(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?desc@AcPlPlotLoggingErrorHandler@@SAPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr desc(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?error@AcPlPlotLoggingErrorHandler@@UAE?AW4ErrorResult@AcPlPlotErrorHandler@@KIPB_W00@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr error(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        /// <summary>
        /// 数据符号 - AcPlPlotLoggingErrorHandler
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "?gpDesc@AcPlPlotLoggingErrorHandler@@2PAVAcRxClass@@A", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gpDesc();

        [DllImport("acad.exe", EntryPoint = "?info@AcPlPlotLoggingErrorHandler@@UAEXKIPB_W00@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void info(IntPtr thisPtr, uint arg0, uint arg1, string arg2, int arg3, int arg4);

        [DllImport("acad.exe", EntryPoint = "?infoMessage@AcPlPlotLoggingErrorHandler@@UAEXPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void infoMessage(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?isA@AcPlPlotLoggingErrorHandler@@UBEPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isA(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?logMessage@AcPlPlotLoggingErrorHandler@@UAEXPB_W0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void logMessage(IntPtr thisPtr, string arg0, int arg1);

        [DllImport("acad.exe", EntryPoint = "?messageBox@AcPlPlotLoggingErrorHandler@@UAEHPB_W0IH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int messageBox(IntPtr thisPtr, string arg0, int arg1, uint arg2, int arg3);

        [DllImport("acad.exe", EntryPoint = "?rxInit@AcPlPlotLoggingErrorHandler@@SAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void rxInit(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?severeError@AcPlPlotLoggingErrorHandler@@UAEXKIPB_W00@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void severeError(IntPtr thisPtr, uint arg0, uint arg1, string arg2, int arg3, int arg4);

        [DllImport("acad.exe", EntryPoint = "?terminalError@AcPlPlotLoggingErrorHandler@@UAEXKIPB_W00@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void terminalError(IntPtr thisPtr, uint arg0, uint arg1, string arg2, int arg3, int arg4);

        [DllImport("acad.exe", EntryPoint = "?warning@AcPlPlotLoggingErrorHandler@@UAE?AW4ErrorResult@AcPlPlotErrorHandler@@KIPB_W00@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr warning(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

    }

    /// <summary>
    /// AcPlPlotPageInfo 类的Native方法 (12 个函数)
    /// </summary>
    public static class AcPlPlotPageInfo
    {
        [DllImport("acad.exe", EntryPoint = "??0AcPlPlotPageInfo@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcPlPlotPageInfo@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?cast@AcPlPlotPageInfo@@SAPAV1@PBVAcRxObject@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr cast(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?desc@AcPlPlotPageInfo@@SAPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr desc(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?entityCount@AcPlPlotPageInfo@@QBEJXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int entityCount(IntPtr thisPtr);

        /// <summary>
        /// 数据符号 - AcPlPlotPageInfo
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "?gpDesc@AcPlPlotPageInfo@@2PAVAcRxClass@@A", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr gpDesc();

        [DllImport("acad.exe", EntryPoint = "?gradientCount@AcPlPlotPageInfo@@QBEJXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int gradientCount(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?isA@AcPlPlotPageInfo@@UBEPAVAcRxClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isA(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?oleObjectCount@AcPlPlotPageInfo@@QBEJXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int oleObjectCount(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?rasterCount@AcPlPlotPageInfo@@QBEJXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int rasterCount(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?rxInit@AcPlPlotPageInfo@@SAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void rxInit(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?shadedViewportType@AcPlPlotPageInfo@@QBEJXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int shadedViewportType(IntPtr thisPtr);

    }

    /// <summary>
    /// AcPlPlotReactor 类的Native方法 (10 个函数)
    /// </summary>
    public static class AcPlPlotReactor
    {
        [DllImport("acad.exe", EntryPoint = "??0AcPlPlotReactor@@IAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcPlPlotReactor@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?beginDocument@AcPlPlotReactor@@UAEXAAVAcPlPlotInfo@@PB_WJ_N1@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void beginDocument(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?beginPage@AcPlPlotReactor@@UAEXAAVAcPlPlotPageInfo@@AAVAcPlPlotInfo@@_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void beginPage(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?beginPlot@AcPlPlotReactor@@UAEXPAVAcPlPlotProgress@@W4PlotType@1@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void beginPlot(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?endDocument@AcPlPlotReactor@@UAEXW4PlotCancelStatus@AcPlPlotProgress@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void endDocument(IntPtr thisPtr, char arg0, int arg1, IntPtr arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, IntPtr arg9, int arg10, int arg11, int arg12);

        [DllImport("acad.exe", EntryPoint = "?endPage@AcPlPlotReactor@@UAEXW4SheetCancelStatus@AcPlPlotProgress@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void endPage(IntPtr thisPtr, char arg0, int arg1, IntPtr arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, IntPtr arg10, int arg11, int arg12, int arg13);

        [DllImport("acad.exe", EntryPoint = "?endPlot@AcPlPlotReactor@@UAEXW4PlotCancelStatus@AcPlPlotProgress@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void endPlot(IntPtr thisPtr, char arg0, int arg1, IntPtr arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, IntPtr arg9, int arg10, int arg11, int arg12);

        [DllImport("acad.exe", EntryPoint = "?pageCancelled@AcPlPlotReactor@@UAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void pageCancelled(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?plotCancelled@AcPlPlotReactor@@UAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void plotCancelled(IntPtr thisPtr);

    }

    /// <summary>
    /// AcPlPlotReactorMgr 类的Native方法 (2 个函数)
    /// </summary>
    public static class AcPlPlotReactorMgr
    {
        [DllImport("acad.exe", EntryPoint = "?addReactor@AcPlPlotReactorMgr@@QAEXPAVAcPlPlotReactor@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void addReactor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?removeReactor@AcPlPlotReactorMgr@@QAEXPAVAcPlPlotReactor@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void removeReactor(IntPtr thisPtr, IntPtr arg0);

    }

    /// <summary>
    /// AcPlSystemInternals 类的Native方法 (2 个函数)
    /// </summary>
    public static class AcPlSystemInternals
    {
        [DllImport("acad.exe", EntryPoint = "?getAcPlObjectImp@AcPlSystemInternals@@SAPAVAcPlObjectImp@@PAVAcPlObject@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getAcPlObjectImp(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?getConstAcPlObjectImp@AcPlSystemInternals@@SAPBVAcPlObjectImp@@PBVAcPlObject@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getConstAcPlObjectImp(IntPtr thisPtr);

    }

    /// <summary>
    /// AcqDDJig 类的Native方法 (1 个函数)
    /// </summary>
    public static class AcqDDJig
    {
        [DllImport("acad.exe", EntryPoint = "?getUcsPlane@AcqDDJig@@YA?AW4ErrorStatus@Acad@@AAVAcGeMatrix3d@@AAVAcGeVector3d@@11AAN@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getUcsPlane(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

    }

    /// <summary>
    /// AcStatusBarItem 类的Native方法 (29 个函数)
    /// </summary>
    public static class AcStatusBarItem
    {
        [DllImport("acad.exe", EntryPoint = "??0AcStatusBarItem@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcStatusBarItem@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        /// <summary>
        /// 虚函数表 - AcStatusBarItem
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "??_7AcStatusBarItem@@6B@", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr vftable();

        /// <summary>
        /// 数据符号 - AcStatusBarItem
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "??_GAcStatusBarItem@@UAEPAXI@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr scalar_dtor();

        [DllImport("acad.exe", EntryPoint = "?ClientToScreen@AcStatusBarItem@@UAEHPAUtagPOINT@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int ClientToScreen(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?ClientToScreen@AcStatusBarItem@@UAEHPAUtagRECT@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int ClientToScreen_1(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?ContentChanged@AcStatusBarItem@@UBEHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int ContentChanged(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?DisplayContextMenu@AcStatusBarItem@@UAEIAAVCMenu@@VCPoint@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern uint DisplayContextMenu(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?Enable@AcStatusBarItem@@UAEXH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void Enable(IntPtr thisPtr, int arg0);

        [DllImport("acad.exe", EntryPoint = "?GetIcon@AcStatusBarItem@@UBEPAUHICON__@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr GetIcon(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?GetInternalData@AcStatusBarItem@@UBEPAXH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr GetInternalData(IntPtr thisPtr, int arg0);

        [DllImport("acad.exe", EntryPoint = "?GetToolTipText@AcStatusBarItem@@UBEHAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int GetToolTipText(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?IsEnabled@AcStatusBarItem@@UAEHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int IsEnabled(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?IsVisible@AcStatusBarItem@@UAEHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int IsVisible(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?OnDelete@AcStatusBarItem@@UAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void OnDelete(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?OnLButtonDblClk@AcStatusBarItem@@UAEXIVCPoint@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void OnLButtonDblClk(IntPtr thisPtr, uint arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?OnLButtonDown@AcStatusBarItem@@UAEXIVCPoint@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void OnLButtonDown(IntPtr thisPtr, uint arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?OnRButtonDown@AcStatusBarItem@@UAEXIVCPoint@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void OnRButtonDown(IntPtr thisPtr, uint arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?QueryToolTipText@AcStatusBarItem@@UBEHAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int QueryToolTipText(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?ScreenToClient@AcStatusBarItem@@UAEHPAUtagPOINT@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int ScreenToClient(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?ScreenToClient@AcStatusBarItem@@UAEHPAUtagRECT@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int ScreenToClient_1(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?SetContentChanged@AcStatusBarItem@@UAEXH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void SetContentChanged(IntPtr thisPtr, int arg0);

        [DllImport("acad.exe", EntryPoint = "?SetIcon@AcStatusBarItem@@UAEHPAUHICON__@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int SetIcon(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?SetInternalData@AcStatusBarItem@@UAEXPAXH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void SetInternalData(IntPtr thisPtr, IntPtr arg0, int arg1);

        [DllImport("acad.exe", EntryPoint = "?SetToolTipText@AcStatusBarItem@@UAEHABV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int SetToolTipText(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?SetVisibilityChanged@AcStatusBarItem@@UAEXH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void SetVisibilityChanged(IntPtr thisPtr, int arg0);

        [DllImport("acad.exe", EntryPoint = "?SetVisible@AcStatusBarItem@@UAEXH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void SetVisible(IntPtr thisPtr, int arg0);

        [DllImport("acad.exe", EntryPoint = "?ShowTraySettingsDialog@AcStatusBarItem@@UAEHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int ShowTraySettingsDialog(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?VisibilityChanged@AcStatusBarItem@@UBEHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int VisibilityChanged(IntPtr thisPtr);

    }

    /// <summary>
    /// AcStatusBarMenuItem 类的Native方法 (6 个函数)
    /// </summary>
    public static class AcStatusBarMenuItem
    {
        [DllImport("acad.exe", EntryPoint = "??0AcStatusBarMenuItem@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcStatusBarMenuItem@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        /// <summary>
        /// 虚函数表 - AcStatusBarMenuItem
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "??_7AcStatusBarMenuItem@@6B@", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr vftable();

        /// <summary>
        /// 数据符号 - AcStatusBarMenuItem
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "??_GAcStatusBarMenuItem@@UAEPAXI@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr scalar_dtor();

        [DllImport("acad.exe", EntryPoint = "?CustomizeMenu@AcStatusBarMenuItem@@UAEHW4AcStatusBarType@1@PAVCMenu@@II@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int CustomizeMenu(IntPtr thisPtr, char arg0, int arg1, IntPtr arg2, IntPtr arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12);

        [DllImport("acad.exe", EntryPoint = "?InvokeMenuCommand@AcStatusBarMenuItem@@UAEHW4AcStatusBarType@1@I@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int InvokeMenuCommand(IntPtr thisPtr, char arg0, int arg1, IntPtr arg2, IntPtr arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12);

    }

    /// <summary>
    /// AcTrayItem 类的Native方法 (9 个函数)
    /// </summary>
    public static class AcTrayItem
    {
        [DllImport("acad.exe", EntryPoint = "??0AcTrayItem@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcTrayItem@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        /// <summary>
        /// 虚函数表 - AcTrayItem
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "??_7AcTrayItem@@6B@", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr vftable();

        /// <summary>
        /// 数据符号 - AcTrayItem
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "??_GAcTrayItem@@UAEPAXI@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr scalar_dtor();

        [DllImport("acad.exe", EntryPoint = "?CloseAllBubbleWindows@AcTrayItem@@UAEHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int CloseAllBubbleWindows(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?GetBubbleWindowControl@AcTrayItem@@UBEPAVAcTrayItemBubbleWindowControl@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr GetBubbleWindowControl(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?GetIcon@AcTrayItem@@UBEPAUHICON__@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr GetIcon(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?SetIcon@AcTrayItem@@UAEHPAUHICON__@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int SetIcon(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?ShowBubbleWindow@AcTrayItem@@UAEHPAVAcTrayItemBubbleWindowControl@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int ShowBubbleWindow(IntPtr thisPtr, IntPtr arg0);

    }

    /// <summary>
    /// AcTrayItemBubbleWindowControl 类的Native方法 (5 个函数)
    /// </summary>
    public static class AcTrayItemBubbleWindowControl
    {
        [DllImport("acad.exe", EntryPoint = "??0AcTrayItemBubbleWindowControl@@QAE@ABV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@0000@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "??0AcTrayItemBubbleWindowControl@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor_1(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcTrayItemBubbleWindowControl@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??4AcTrayItemBubbleWindowControl@@QAEAAV0@ABV0@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr operator_(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?InitDefault@AcTrayItemBubbleWindowControl@@AAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void InitDefault(IntPtr thisPtr);

    }

    /// <summary>
    /// AcViewMgrDlg 类的Native方法 (14 个函数)
    /// </summary>
    public static class AcViewMgrDlg
    {
        [DllImport("acad.exe", EntryPoint = "?acdbDefaultViewCategory@AcViewMgrDlg@@YA_NPBVAcDbDatabase@@AAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool acdbDefaultViewCategory(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?editViewBoundaries@AcViewMgrDlg@@YA_NPAVAcDbViewTableRecord@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool editViewBoundaries(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?getCurrentViewName@AcViewMgrDlg@@YA_NAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool getCurrentViewName(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?isSheetSetCmdAllowed@AcViewMgrDlg@@YA_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool isSheetSetCmdAllowed(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?restoreView@AcViewMgrDlg@@YA_NABVAcDbObjectId@@_N1@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool restoreView(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?restoreView@AcViewMgrDlg@@YA_NPB_WPAVAcDbDatabase@@_N2@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool restoreView_1(IntPtr thisPtr, string arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?saveViewLayerState@AcViewMgrDlg@@YA_NPAVAcDbDatabase@@PAVAcDbViewTableRecord@@K@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool saveViewLayerState(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?setCaptureOnLayoutSwitch@AcViewMgrDlg@@YAX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setCaptureOnLayoutSwitch(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?setIsoView@AcViewMgrDlg@@YA_NAAVAcGeVector3d@@_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool setIsoView(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?setOrthoView@AcViewMgrDlg@@YA_NW4OrthographicView@AcDb@@_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool setOrthoView(IntPtr thisPtr, char arg0, int arg1, decimal arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8, int arg9, int arg10, int arg11, int arg12, int arg13, IntPtr arg14, IntPtr arg15, sbyte arg16, int arg17);

        [DllImport("acad.exe", EntryPoint = "?updateFromViewport@AcViewMgrDlg@@YAXPAVAcDbViewTableRecord@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void updateFromViewport(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?viewComboClear@AcViewMgrDlg@@YAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void viewComboClear(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?viewComboSelect@AcViewMgrDlg@@YAXPA_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void viewComboSelect(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?viewComboSelect@AcViewMgrDlg@@YAXW4ViewComboPresetViewIndex@1@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void viewComboSelect_1(IntPtr thisPtr, char arg0, int arg1, IntPtr arg2, int arg3);

    }

    /// <summary>
    /// AcViewport 类的Native方法 (4 个函数)
    /// </summary>
    public static class AcViewport
    {
        [DllImport("acad.exe", EntryPoint = "?ConvertDwgvecToScreenCoords@AcViewport@@QBEXQAY02NQAUspoint@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ConvertDwgvecToScreenCoords(IntPtr thisPtr, IntPtr arg0, int arg1, int arg2, double arg3, IntPtr arg4);

        [DllImport("acad.exe", EntryPoint = "?context@AcViewport@@QBE?AW4ViewContext@1@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr context(IntPtr thisPtr, IntPtr arg0, int arg1, IntPtr arg2, int arg3);

        [DllImport("acad.exe", EntryPoint = "?cvdtsc@AcViewport@@QBEXNNPATsus@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void cvdtsc(IntPtr thisPtr, double arg0, double arg1, IntPtr arg2, int arg3, int arg4, int arg5);

        [DllImport("acad.exe", EntryPoint = "?cvsctd@AcViewport@@QBEXJJPAN0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void cvsctd(IntPtr thisPtr, int arg0, int arg1, IntPtr arg2, int arg3);

    }

    /// <summary>
    /// AcVisualStyleSet 类的Native方法 (5 个函数)
    /// </summary>
    public static class AcVisualStyleSet
    {
        [DllImport("acad.exe", EntryPoint = "??0AcVisualStyleSet@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1AcVisualStyleSet@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?Init@AcVisualStyleSet@@QAEXABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@PAVAcGsModel@@H@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void Init(IntPtr thisPtr, IntPtr arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?removeAll@AcVisualStyleSet@@QAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void removeAll(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?replace@AcVisualStyleSet@@QAEXABV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@PAVAcGsModel@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void replace(IntPtr thisPtr, IntPtr arg0, IntPtr arg1);

    }

    /// <summary>
    /// AcVSUtil 类的Native方法 (80 个函数)
    /// </summary>
    public static class AcVSUtil
    {
        [DllImport("acad.exe", EntryPoint = "?IsVisualStyleSysVar@AcVSUtil@@SA_NPB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool IsVisualStyleSysVar(IntPtr thisPtr, string arg0);

        [DllImport("acad.exe", EntryPoint = "?PurgeUnUsedAnonymousVS@AcVSUtil@@SA?AW4ErrorStatus@Acad@@PAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr PurgeUnUsedAnonymousVS(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?addNewVS@AcVSUtil@@SA?AW4ErrorStatus@Acad@@PA_WAAVAcDbObjectId@@PAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr addNewVS(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?addOneOffVS@AcVSUtil@@SA?AW4ErrorStatus@Acad@@AAVAcDbObjectId@@PA_WPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr addOneOffVS(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?addOneOffVS@AcVSUtil@@SA?AW4ErrorStatus@Acad@@AAVAcDbObjectId@@V4@PAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr addOneOffVS_1(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?copyVS@AcVSUtil@@SA?AW4ErrorStatus@Acad@@VAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr copyVS(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?deleteVS@AcVSUtil@@SA?AW4ErrorStatus@Acad@@PA_WPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr deleteVS(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?deleteVS@AcVSUtil@@SA?AW4ErrorStatus@Acad@@VAcDbObjectId@@PAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr deleteVS_1(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?duplicateVS@AcVSUtil@@SA?AW4ErrorStatus@Acad@@PA_WAAVAcDbObjectId@@V4@W4Type@AcGiVisualStyle@@PAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr duplicateVS(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getAnonymousVSName@AcVSUtil@@SA?AW4ErrorStatus@Acad@@AAVAcString@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getAnonymousVSName(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getCurVportTableRecordId@AcVSUtil@@SA?AVAcDbObjectId@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getCurVportTableRecordId(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?getCvpVSId@AcVSUtil@@SA?AW4ErrorStatus@Acad@@AAVAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getCvpVSId(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getCvpVSName@AcVSUtil@@SA?AW4ErrorStatus@Acad@@AAVAcString@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getCvpVSName(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getRenderModeByVSType@AcVSUtil@@SA?AW4ErrorStatus@Acad@@AAW4RenderMode@AcGsView@@W4Type@AcGiVisualStyle@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getRenderModeByVSType(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getVSDescription@AcVSUtil@@SA?AW4ErrorStatus@Acad@@ABVAcString@@AAV4@PAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getVSDescription(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getVSDescription@AcVSUtil@@SA?AW4ErrorStatus@Acad@@VAcDbObjectId@@AAVAcString@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getVSDescription_1(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getVSIdByName@AcVSUtil@@SA?AW4ErrorStatus@Acad@@AAVAcDbObjectId@@PB_WPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getVSIdByName(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getVSIdByType@AcVSUtil@@SA?AW4ErrorStatus@Acad@@AAVAcDbObjectId@@W4Type@AcGiVisualStyle@@PAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getVSIdByType(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getVSList@AcVSUtil@@SA?AW4ErrorStatus@Acad@@AAV?$AcArray@PB_WV?$AcArrayMemCopyReallocator@PB_W@@@@PAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getVSList(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getVSList@AcVSUtil@@SA?AW4ErrorStatus@Acad@@AAV?$AcArray@VAcDbObjectId@@V?$AcArrayMemCopyReallocator@VAcDbObjectId@@@@@@PAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getVSList_1(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getVSNameById@AcVSUtil@@SA?AW4ErrorStatus@Acad@@AAVAcString@@VAcDbObjectId@@_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getVSNameById(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getVSTypeById@AcVSUtil@@SA?AW4ErrorStatus@Acad@@AAW4Type@AcGiVisualStyle@@VAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getVSTypeById(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?getVSTypeByRenderMode@AcVSUtil@@SA?AW4ErrorStatus@Acad@@AAW4Type@AcGiVisualStyle@@W4RenderMode@AcGsView@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getVSTypeByRenderMode(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isAcadDefault@AcVSUtil@@SA?AW4ErrorStatus@Acad@@PB_WPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isAcadDefault(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isAcadDefault@AcVSUtil@@SA?AW4ErrorStatus@Acad@@VAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isAcadDefault_1(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isCvpVS2dType@AcVSUtil@@SA?AW4ErrorStatus@Acad@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isCvpVS2dType(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isCvpVS3dType@AcVSUtil@@SA?AW4ErrorStatus@Acad@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isCvpVS3dType(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isVS2dType@AcVSUtil@@SA?AW4ErrorStatus@Acad@@PA_WPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isVS2dType(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isVS2dType@AcVSUtil@@SA?AW4ErrorStatus@Acad@@VAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isVS2dType_1(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isVS3dType@AcVSUtil@@SA?AW4ErrorStatus@Acad@@PA_WPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isVS3dType(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isVS3dType@AcVSUtil@@SA?AW4ErrorStatus@Acad@@VAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isVS3dType_1(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isVSAnonymous@AcVSUtil@@SA?AW4ErrorStatus@Acad@@VAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isVSAnonymous(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isVSExisting@AcVSUtil@@SA?AW4ErrorStatus@Acad@@PB_WPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isVSExisting(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isVSInUse@AcVSUtil@@SA?AW4ErrorStatus@Acad@@PA_WPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isVSInUse(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isVSInUse@AcVSUtil@@SA?AW4ErrorStatus@Acad@@VAcDbObjectId@@PAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isVSInUse_1(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isVSInternalUseOnly@AcVSUtil@@SA?AW4ErrorStatus@Acad@@PB_WPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isVSInternalUseOnly(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isVSInternalUseOnly@AcVSUtil@@SA?AW4ErrorStatus@Acad@@VAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isVSInternalUseOnly_1(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isVSOneOff@AcVSUtil@@SA?AW4ErrorStatus@Acad@@VAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isVSOneOff(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isVSValidUserVisible@AcVSUtil@@SA?AW4ErrorStatus@Acad@@PB_WPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isVSValidUserVisible(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?isVSValidUserVisible@AcVSUtil@@SA?AW4ErrorStatus@Acad@@VAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr isVSValidUserVisible_1(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?openCvpVS@AcVSUtil@@SA?AW4ErrorStatus@Acad@@AAPAVAcDbVisualStyle@@W4OpenMode@AcDb@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr openCvpVS(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?renameVS@AcVSUtil@@SA?AW4ErrorStatus@Acad@@PA_WPB_WPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr renameVS(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?renameVS@AcVSUtil@@SA?AW4ErrorStatus@Acad@@PA_WVAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr renameVS_1(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?resetDefaultFactoryVS@AcVSUtil@@SA?AW4ErrorStatus@Acad@@PB_WPAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr resetDefaultFactoryVS(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?resetDefaultFactoryVS@AcVSUtil@@SA?AW4ErrorStatus@Acad@@VAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr resetDefaultFactoryVS_1(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?setActVpsVSId@AcVSUtil@@SA?AW4ErrorStatus@Acad@@VAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr setActVpsVSId(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?setCvpVS2d@AcVSUtil@@SA?AW4ErrorStatus@Acad@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr setCvpVS2d(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?setCvpVSId@AcVSUtil@@SA?AW4ErrorStatus@Acad@@VAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr setCvpVSId(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?setCvpVSName@AcVSUtil@@SA?AW4ErrorStatus@Acad@@PA_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr setCvpVSName(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?setVSDescription@AcVSUtil@@SA?AW4ErrorStatus@Acad@@PB_W0PAVAcDbDatabase@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr setVSDescription(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?setVSDescription@AcVSUtil@@SA?AW4ErrorStatus@Acad@@VAcDbObjectId@@PB_W@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr setVSDescription_1(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?shademode@AcVSUtil@@SA?AW4ErrorStatus@Acad@@HAAVAcDbObjectId@@V4@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr shademode(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?viewres@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAF@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr viewres(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsbackgrounds@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAFAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsbackgrounds(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsedgecolor@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAVAcCmColor@@AAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsedgecolor(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsedgejitter@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAFAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsedgejitter(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsedgeoverhang@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAFAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsedgeoverhang(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsedges@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAW4EdgeModel@AcGiEdgeStyle@@AAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsedges(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsedgesmooth@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAFAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsedgesmooth(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsedgewidth@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAFAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsedgewidth(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsfacecolormode@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAFAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsfacecolormode(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsfacehighlight@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPANAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsfacehighlight(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsfaceopacity@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPANAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsfaceopacity(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsfacestyle@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAW4LightingModel@AcGiFaceStyle@@AAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsfacestyle(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vshalogap@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAFAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vshalogap(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vshideprecision@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPA_NAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vshideprecision(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsintersectioncolor@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAVAcCmColor@@AAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsintersectioncolor(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsintersectionedges@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAFAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsintersectionedges(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsintersectionltype@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAFAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsintersectionltype(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsisoontop@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAFAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsisoontop(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vslightingquality@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAW4LightingQuality@AcGiFaceStyle@@AAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vslightingquality(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsmaterialmode@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAFAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsmaterialmode(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsmonocolor@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAVAcCmColor@@AAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsmonocolor(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsobscuredcolor@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAVAcCmColor@@AAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsobscuredcolor(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsobscurededges@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAFAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsobscurededges(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsobscuredltype@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAFAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsobscuredltype(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vsshadows@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAW4ShadowType@AcGiDisplayStyle@@AAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vsshadows(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vssilhcolor@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAVAcCmColor@@AAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vssilhcolor(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vssilhedges@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAFAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vssilhedges(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?vssilhwidth@AcVSUtil@@SA?AW4ErrorStatus@Acad@@_NPAFAAVAcDbObjectId@@0@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr vssilhwidth(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

    }

    /// <summary>
    /// CAcDynInput 类的Native方法 (1 个函数)
    /// </summary>
    public static class CAcDynInput
    {
        [DllImport("acad.exe", EntryPoint = "?contextSwitched@CAcDynInput@@QAE_NH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool contextSwitched(IntPtr thisPtr, int arg0);

    }

    /// <summary>
    /// CDraggerTool 类的Native方法 (11 个函数)
    /// </summary>
    public static class CDraggerTool
    {
        [DllImport("acad.exe", EntryPoint = "??0CDraggerTool@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1CDraggerTool@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?Char@CDraggerTool@@UAEXIIIPAVCView@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void _Char(IntPtr thisPtr, uint arg0, uint arg1, uint arg2, IntPtr arg3);

        [DllImport("acad.exe", EntryPoint = "?GetRuntimeClass@CDraggerTool@@UBEPAUCRuntimeClass@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr GetRuntimeClass(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?LButtonDown@CDraggerTool@@UAEXIVCPoint@@PAVCView@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void LButtonDown(IntPtr thisPtr, uint arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?LButtonUp@CDraggerTool@@UAEXIVCPoint@@PAVCView@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void LButtonUp(IntPtr thisPtr, uint arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?MButtonDown@CDraggerTool@@UAEXIVCPoint@@PAVCView@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void MButtonDown(IntPtr thisPtr, uint arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?MButtonUp@CDraggerTool@@UAEXIVCPoint@@PAVCView@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void MButtonUp(IntPtr thisPtr, uint arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?MouseMove@CDraggerTool@@UAEXIVCPoint@@PAVCView@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void MouseMove(IntPtr thisPtr, uint arg0, IntPtr arg1);

        [DllImport("acad.exe", EntryPoint = "?OnRawMouseInput@CDraggerTool@@UAEXPAUtagRAWINPUT@@PAVCView@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void OnRawMouseInput(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?TimedIntelliPan@CDraggerTool@@UAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void TimedIntelliPan(IntPtr thisPtr);

    }

    /// <summary>
    /// CLTypeControl 类的Native方法 (1 个函数)
    /// </summary>
    public static class CLTypeControl
    {
        [DllImport("acad.exe", EntryPoint = "?DrawLTypePattern@CLTypeControl@@SAXPAVCDC@@VAcDbObjectId@@PAUtagRECT@@H@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void DrawLTypePattern(IntPtr thisPtr, IntPtr arg0);

    }

    /// <summary>
    /// CMetaFile 类的Native方法 (3 个函数)
    /// </summary>
    public static class CMetaFile
    {
        [DllImport("acad.exe", EntryPoint = "??0CMetaFile@@QAE@AAUtagFORMATETC@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr, IntPtr arg0);

        /// <summary>
        /// 数据符号 - CMetaFile
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "??_GCMetaFile@@UAEPAXI@Z", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr scalar_dtor();

        [DllImport("acad.exe", EntryPoint = "?GetMetaLogFont@CMetaFile@@QAE_NAAUtagLOGFONTW@@V?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@J@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool GetMetaLogFont(IntPtr thisPtr, IntPtr arg0);

    }

    /// <summary>
    /// CMetaFileEnhanced 类的Native方法 (4 个函数)
    /// </summary>
    public static class CMetaFileEnhanced
    {
        [DllImport("acad.exe", EntryPoint = "??0CMetaFileEnhanced@@QAE@AAUtagFORMATETC@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?DecodeMetaFile@CMetaFileEnhanced@@UAE_NAAVCOleDataObject@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool DecodeMetaFile(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?enumerateMetaFileInit@CMetaFileEnhanced@@IAEXPAUHDC__@@PAX@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void enumerateMetaFileInit(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?enumerateMetaFileScale@CMetaFileEnhanced@@IAEXPAUHDC__@@PAX@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void enumerateMetaFileScale(IntPtr thisPtr, IntPtr arg0);

    }

    /// <summary>
    /// CMetaFileStandard 类的Native方法 (5 个函数)
    /// </summary>
    public static class CMetaFileStandard
    {
        [DllImport("acad.exe", EntryPoint = "??0CMetaFileStandard@@QAE@AAUtagFORMATETC@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?CopyToLogfont@CMetaFileStandard@@SAXPAUtagOLDLOGFONT@@AAUtagLOGFONTW@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void CopyToLogfont(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?DecodeMetaFile@CMetaFileStandard@@UAE_NAAVCOleDataObject@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool DecodeMetaFile(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?enumerateMetaFileInit@CMetaFileStandard@@IAEXPAUHDC__@@PAX@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void enumerateMetaFileInit(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?enumerateMetaFileScale@CMetaFileStandard@@IAEXPAUHDC__@@PAX@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void enumerateMetaFileScale(IntPtr thisPtr, IntPtr arg0);

    }

    /// <summary>
    /// COleUtil 类的Native方法 (3 个函数)
    /// </summary>
    public static class COleUtil
    {
        [DllImport("acad.exe", EntryPoint = "?convertPointsToAcadUnits@COleUtil@@YANH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern double convertPointsToAcadUnits(IntPtr thisPtr, int arg0);

        [DllImport("acad.exe", EntryPoint = "?getPixelsPerPaperUnit@COleUtil@@YAMM@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern float getPixelsPerPaperUnit(IntPtr thisPtr, float arg0);

        [DllImport("acad.exe", EntryPoint = "?getPlotPaperUnits@COleUtil@@YAHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int getPlotPaperUnits(IntPtr thisPtr);

    }

    /// <summary>
    /// CProxyInet 类的Native方法 (17 个函数)
    /// </summary>
    public static class CProxyInet
    {
        [DllImport("acad.exe", EntryPoint = "??0CProxyInet@@AAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1CProxyInet@@AAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?AddToFilenameCache@CProxyInet@@QAEHV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@0H@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int AddToFilenameCache(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?CProgInetDll@CProxyInet@@AAEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr CProgInetDll(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?CreateInternetTempFile@CProxyInet@@QAEHV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@AAV23@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int CreateInternetTempFile(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?DeleteProxyInet@CProxyInet@@SAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void DeleteProxyInet(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?GetFavoritesDirectory@CProxyInet@@QAEHAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int GetFavoritesDirectory(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?GetProxyInet@CProxyInet@@SAPAV1@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr GetProxyInet(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?IsBrowserControlInstalled@CProxyInet@@QAEHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int IsBrowserControlInstalled(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?IsRemoteFile@CProxyInet@@QAEHV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@AAV23@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int IsRemoteFile(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?LaunchBrowserDialog@CProxyInet@@QAEHAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@ABV23@111H@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int LaunchBrowserDialog(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?PreProcessURL@CProxyInet@@QAEHAAV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int PreProcessURL(IntPtr thisPtr, IntPtr arg0, char arg1, IntPtr arg2, char arg3, IntPtr arg4, char arg5);

        [DllImport("acad.exe", EntryPoint = "?SwitchAwayFromInternetDir@CProxyInet@@QAEABV?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@ABV23@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr SwitchAwayFromInternetDir(IntPtr thisPtr, char arg0, IntPtr arg1, char arg2, IntPtr arg3, char arg4);

        [DllImport("acad.exe", EntryPoint = "?TransferFile@CProxyInet@@QAE?AW4Status@AcadInet@@V?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@AAV45@KH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr TransferFile(IntPtr thisPtr, IntPtr arg0, int arg1, IntPtr arg2, int arg3, int arg4, int arg5);

        [DllImport("acad.exe", EntryPoint = "?WWWFormPost@CProxyInet@@QAE?AW4Status@AcadInet@@V?$CStringT@_WV?$StrTraitMFC_DLL@_WV?$ChTraitsCRT@_W@ATL@@@@@ATL@@000H0AAV45@HH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr WWWFormPost(IntPtr thisPtr, IntPtr arg0, int arg1, IntPtr arg2, int arg3, int arg4, int arg5);

        /// <summary>
        /// 数据符号 - CProxyInet
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "?m_tcUserShellFolders@CProxyInet@@0PB_WB", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr m_tcUserShellFolders();

        /// <summary>
        /// 数据符号 - CProxyInet
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "?m_theProxyInet@CProxyInet@@0PAV1@A", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr m_theProxyInet();

    }

    /// <summary>
    /// CTempFile 类的Native方法 (3 个函数)
    /// </summary>
    public static class CTempFile
    {
        [DllImport("acad.exe", EntryPoint = "??0CTempFile@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1CTempFile@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?CreateTempFile@CTempFile@@QAEPB_WXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr CreateTempFile(IntPtr thisPtr);

    }

    /// <summary>
    /// DisplayWireframeOverride 类的Native方法 (2 个函数)
    /// </summary>
    public static class DisplayWireframeOverride
    {
        [DllImport("acad.exe", EntryPoint = "??0DisplayWireframeOverride@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "??1DisplayWireframeOverride@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

    }

    /// <summary>
    /// HT_Media_Info 类的Native方法 (1 个函数)
    /// </summary>
    public static class HT_Media_Info
    {
        [DllImport("acad.exe", EntryPoint = "??1HT_Media_Info@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

    }

    /// <summary>
    /// HT_Media_Names 类的Native方法 (1 个函数)
    /// </summary>
    public static class HT_Media_Names
    {
        [DllImport("acad.exe", EntryPoint = "??1HT_Media_Names@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

    }

    /// <summary>
    /// HT_PC3_Data 类的Native方法 (1 个函数)
    /// </summary>
    public static class HT_PC3_Data
    {
        [DllImport("acad.exe", EntryPoint = "??1HT_PC3_Data@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

    }

    /// <summary>
    /// HT_Plot_Config 类的Native方法 (1 个函数)
    /// </summary>
    public static class HT_Plot_Config
    {
        [DllImport("acad.exe", EntryPoint = "??1HT_Plot_Config@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

    }

    /// <summary>
    /// HT_Plot_Config_Manager 类的Native方法 (6 个函数)
    /// </summary>
    public static class HT_Plot_Config_Manager
    {
        [DllImport("acad.exe", EntryPoint = "?get_driver_search_path@HT_Plot_Config_Manager@@QAE?AVHT_String@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr get_driver_search_path(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?get_style_sheet_search_path@HT_Plot_Config_Manager@@QAE?AVHT_String@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr get_style_sheet_search_path(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?get_udm_search_path@HT_Plot_Config_Manager@@QAE?AVHT_String@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr get_udm_search_path(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?set_driver_search_path@HT_Plot_Config_Manager@@QAEXVHT_String@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void set_driver_search_path(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?set_style_sheet_search_path@HT_Plot_Config_Manager@@QAEXVHT_String@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void set_style_sheet_search_path(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?set_udm_search_path@HT_Plot_Config_Manager@@QAEXVHT_String@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void set_udm_search_path(IntPtr thisPtr, IntPtr arg0);

    }

    /// <summary>
    /// HT_Plot_Style 类的Native方法 (1 个函数)
    /// </summary>
    public static class HT_Plot_Style
    {
        [DllImport("acad.exe", EntryPoint = "??1HT_Plot_Style@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

    }

    /// <summary>
    /// HT_Style_Sheet_Configuration 类的Native方法 (1 个函数)
    /// </summary>
    public static class HT_Style_Sheet_Configuration
    {
        [DllImport("acad.exe", EntryPoint = "??1HT_Style_Sheet_Configuration@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

    }

    /// <summary>
    /// HT_Thin_Plot_Config_Manager 类的Native方法 (1 个函数)
    /// </summary>
    public static class HT_Thin_Plot_Config_Manager
    {
        [DllImport("acad.exe", EntryPoint = "??1HT_Thin_Plot_Config_Manager@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

    }

    /// <summary>
    /// MetaEnhDebug 类的Native方法 (2 个函数)
    /// </summary>
    public static class MetaEnhDebug
    {
        [DllImport("acad.exe", EntryPoint = "??0MetaEnhDebug@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?GetTokenFromInt@MetaEnhDebug@@QAEPA_WH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr GetTokenFromInt(IntPtr thisPtr, int arg0);

    }

    /// <summary>
    /// MetaFont 类的Native方法 (1 个函数)
    /// </summary>
    public static class MetaFont
    {
        [DllImport("acad.exe", EntryPoint = "?CopyToLogfont@MetaFont@@QAEXPAUtagLOGFONTW@@AAU2@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void CopyToLogfont(IntPtr thisPtr, IntPtr arg0);

    }

    /// <summary>
    /// MetaWinDebug 类的Native方法 (2 个函数)
    /// </summary>
    public static class MetaWinDebug
    {
        [DllImport("acad.exe", EntryPoint = "??0MetaWinDebug@@QAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?GetTokenFromInt@MetaWinDebug@@QAEPA_WH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr GetTokenFromInt(IntPtr thisPtr, int arg0);

    }

    /// <summary>
    /// PCM_Class_Factory 类的Native方法 (1 个函数)
    /// </summary>
    public static class PCM_Class_Factory
    {
        [DllImport("acad.exe", EntryPoint = "??1PCM_Class_Factory@@UAE@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void dtor(IntPtr thisPtr);

    }

    /// <summary>
    /// ViewMgrDlgHook 类的Native方法 (7 个函数)
    /// </summary>
    public static class ViewMgrDlgHook
    {
        [DllImport("acad.exe", EntryPoint = "?DestroyLiveSectionList@ViewMgrDlgHook@@YAXAAV?$vector@PAULiveSectionEntry@ViewMgrDlgHook@@V?$allocator@PAULiveSectionEntry@ViewMgrDlgHook@@@std@@@std@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void DestroyLiveSectionList(IntPtr thisPtr, IntPtr arg0, IntPtr arg1, IntPtr arg2);

        [DllImport("acad.exe", EntryPoint = "?EnableLiveSection@ViewMgrDlgHook@@YA?AW4ErrorStatus@Acad@@VAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr EnableLiveSection(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?GetAvailableLiveSections@ViewMgrDlgHook@@YA?AW4ErrorStatus@Acad@@PBVAcDbDatabase@@AAV?$vector@PAULiveSectionEntry@ViewMgrDlgHook@@V?$allocator@PAULiveSectionEntry@ViewMgrDlgHook@@@std@@@std@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr GetAvailableLiveSections(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

        [DllImport("acad.exe", EntryPoint = "?IsLiveSectionHookEnabled@ViewMgrDlgHook@@YA_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool IsLiveSectionHookEnabled(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?LiveSectionHook@ViewMgrDlgHook@@YAPAULiveSectionCallback@1@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr LiveSectionHook(IntPtr thisPtr, int arg0);

        [DllImport("acad.exe", EntryPoint = "?RemoveLiveSectionHook@ViewMgrDlgHook@@YA_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool RemoveLiveSectionHook(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?SetLiveSectionHook@ViewMgrDlgHook@@YA_NPAULiveSectionCallback@1@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool SetLiveSectionHook(IntPtr thisPtr, IntPtr arg0, int arg1);

    }

    /// <summary>
    /// Whip 类的Native方法 (32 个函数)
    /// </summary>
    public static class Whip
    {
        [DllImport("acad.exe", EntryPoint = "??0Whip@@QAE@ABV0@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void ctor(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "??4Whip@@QAEAAV0@ABV0@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr operator_(IntPtr thisPtr, IntPtr arg0);

        /// <summary>
        /// 虚函数表 - Whip
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "??_7Whip@@6B@", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr vftable();

        /// <summary>
        /// 数据符号 - Whip
        /// 注意: 这是数据符号，不是函数。返回值是指向数据的指针。
        /// </summary>
        [DllImport("acad.exe", EntryPoint = "??_FWhip@@QAEXXZ", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr default_ctor();

        [DllImport("acad.exe", EntryPoint = "?AcadLogEnabled@Whip@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool AcadLogEnabled(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?GetAnnotationalLwtScaleFactor@Whip@@QBENXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern double GetAnnotationalLwtScaleFactor(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?GetDefaultLineweightIndex@Whip@@QBEHXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern int GetDefaultLineweightIndex(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?GetLinewtsAreAnnotational@Whip@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool GetLinewtsAreAnnotational(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?GetLwtDisplay@Whip@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool GetLwtDisplay(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?GetLwtToDeviceUnitsTransform@Whip@@QBENXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern double GetLwtToDeviceUnitsTransform(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?SetUseHeidiHatchPatterns@Whip@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void SetUseHeidiHatchPatterns(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?getLayerHint@Whip@@QAEPAVLayerOnOffHint@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr getLayerHint(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?get_base_lwt_transform@Whip@@QBENXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern double get_base_lwt_transform(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?get_current_entity@Whip@@QAEPAPAVCurrentEntity@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr get_current_entity(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?get_curwin@Whip@@QAEPAU_window@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr get_curwin(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?get_dlmtptr@Whip@@QAEPAVDisplayListMultithread@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr get_dlmtptr(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?get_driver@Whip@@QAEPAVDriver@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr get_driver(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?get_fontcache@Whip@@QAEPAVFontCacheHash@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr get_fontcache(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?get_force_indexed_color@Whip@@QAE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool get_force_indexed_color(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?get_hardcopy_flag@Whip@@QAE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool get_hardcopy_flag(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?get_heidi_heap@Whip@@QAEPAVHeidiHeap@@VDrawingMode@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr get_heidi_heap(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?get_highlight_image_frame_only@Whip@@QAE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool get_highlight_image_frame_only(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?get_indexed_color@Whip@@QAE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool get_indexed_color(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?get_interrupt_method@Whip@@QAEP6A_NPAX@ZXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr get_interrupt_method(IntPtr thisPtr, IntPtr arg0);

        [DllImport("acad.exe", EntryPoint = "?get_interrupt_parameter@Whip@@QAEPAXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr get_interrupt_parameter(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?get_palette_data@Whip@@QAEPADXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr get_palette_data(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?get_viewport_list@Whip@@QAEPAVViewportList@@XZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr get_viewport_list(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?pruneXORDuplicateSegments@Whip@@QAE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool pruneXORDuplicateSegments(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?setPruneXORDuplicateSegments@Whip@@QAEX_N@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern void setPruneXORDuplicateSegments(IntPtr thisPtr, bool arg0);

        [DllImport("acad.exe", EntryPoint = "?use_heidi_hatch_patterns@Whip@@QBE_NXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern bool use_heidi_hatch_patterns(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?whipdrawableEnterCriticalSection@Whip@@QAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void whipdrawableEnterCriticalSection(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?whipdrawableLeaveCriticalSection@Whip@@QAEXXZ", CallingConvention = CallingConvention.ThisCall)]
        public static extern void whipdrawableLeaveCriticalSection(IntPtr thisPtr);

    }

    /// <summary>
    /// XRefLockLocal 类的Native方法 (3 个函数)
    /// </summary>
    public static class XRefLockLocal
    {
        [DllImport("acad.exe", EntryPoint = "?Getfileinfo@XRefLockLocal@@SGHPB_WAAJAAH@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern int Getfileinfo(IntPtr thisPtr, string arg0, IntPtr arg1, IntPtr arg2);

        [DllImport("acad.exe", EntryPoint = "?filterThroughShareInfo@XRefLockLocal@@SGPAVAcDwgFileHandle@@PAV2@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr filterThroughShareInfo(IntPtr thisPtr);

        [DllImport("acad.exe", EntryPoint = "?lockDemandloadedXreffile@XRefLockLocal@@SG?AW4ErrorStatus@Acad@@VAcDbObjectId@@@Z", CallingConvention = CallingConvention.ThisCall)]
        public static extern IntPtr lockDemandloadedXreffile(IntPtr thisPtr, IntPtr arg0, int arg1, byte arg2, int arg3, int arg4, int arg5, int arg6, IntPtr arg7, int arg8, int arg9, int arg10);

    }

}
