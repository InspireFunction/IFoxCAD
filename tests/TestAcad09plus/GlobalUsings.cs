/// 系统引用
global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Text;
global using System.Reflection;
global using System.Text.RegularExpressions;
global using Microsoft.Win32;
global using System.ComponentModel;
global using System.Runtime.CompilerServices;

/// autocad 引用
global using Autodesk.AutoCAD.ApplicationServices;
global using Autodesk.AutoCAD.EditorInput;
global using Autodesk.AutoCAD.Colors;
global using Autodesk.AutoCAD.DatabaseServices;
global using Autodesk.AutoCAD.Geometry;
global using Autodesk.AutoCAD.Runtime;
global using Acgi = Autodesk.AutoCAD.GraphicsInterface;
global using Acap = Autodesk.AutoCAD.ApplicationServices.Application;

global using Registry = Microsoft.Win32.Registry;
global using RegistryKey = Microsoft.Win32.RegistryKey;

global using Acad_DwgFiler = Autodesk.AutoCAD.DatabaseServices.DwgFiler;
global using Acad_DxfFiler = Autodesk.AutoCAD.DatabaseServices.DxfFiler;

/// ifoxcad
global using IFoxCAD.Cad;
global using IFoxCAD.Basal;
#if !ac2008
global using IFoxCAD.WPF;
#endif