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
global using System.Runtime.InteropServices;
global using System.Collections.Specialized;

global using Exception = System.Exception;

global using Registry = Microsoft.Win32.Registry;
global using RegistryKey = Microsoft.Win32.RegistryKey;

/// cad 引用
global using ZwSoft.ZwCAD.ApplicationServices;
global using ZwSoft.ZwCAD.EditorInput;
global using ZwSoft.ZwCAD.Colors;
global using ZwSoft.ZwCAD.DatabaseServices;
global using ZwSoft.ZwCAD.Geometry;
global using ZwSoft.ZwCAD.Runtime;
global using Acap = ZwSoft.ZwCAD.ApplicationServices.Application;

global using ZwSoft.ZwCAD.DatabaseServices.Filters;
global using ZwSoft.ZwCAD;

//jig命名空间会引起Viewport/Polyline等等重义,最好逐个引入 using ZwSoft.ZwCAD.GraphicsInterface
global using ZwSoft.ZwCAD.GraphicsInterface;
global using WorldDraw = ZwSoft.ZwCAD.GraphicsInterface.WorldDraw;
global using Manager = ZwSoft.ZwCAD.GraphicsSystem.Manager;
global using Group = ZwSoft.ZwCAD.DatabaseServices.Group;
global using Viewport = ZwSoft.ZwCAD.DatabaseServices.Viewport;
global using Polyline = ZwSoft.ZwCAD.DatabaseServices.Polyline;
global using Cad_DwgFiler = ZwSoft.ZwCAD.DatabaseServices.DwgFiler;
global using Cad_DxfFiler = ZwSoft.ZwCAD.DatabaseServices.DxfFiler;
global using Cad_ErrorStatus = ZwSoft.ZwCAD.Runtime.ErrorStatus;

/// ifoxcad.basal 引用
global using IFoxCAD.Basal;