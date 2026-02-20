// 系统引用
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
global using System.Runtime.CompilerServices;

// cad 引用
global using Autodesk.AutoCAD.ApplicationServices;
global using Autodesk.AutoCAD.EditorInput;
global using Autodesk.AutoCAD.Colors;
global using Autodesk.AutoCAD.DatabaseServices;
global using Autodesk.AutoCAD.Geometry;
global using Autodesk.AutoCAD.Runtime;
global using Acgi = Autodesk.AutoCAD.GraphicsInterface;

global using Autodesk.AutoCAD.DatabaseServices.Filters;
global using Autodesk.AutoCAD;

// jig命名空间会引起Viewport/Polyline等等重义,最好逐个引入 using Autodesk.AutoCAD.GraphicsInterface
global using Autodesk.AutoCAD.GraphicsInterface;
global using WorldDraw = Autodesk.AutoCAD.GraphicsInterface.WorldDraw;
global using Manager = Autodesk.AutoCAD.GraphicsSystem.Manager;
global using Viewport = Autodesk.AutoCAD.DatabaseServices.Viewport;
global using Cad_DwgFiler = Autodesk.AutoCAD.DatabaseServices.DwgFiler;
global using Cad_DxfFiler = Autodesk.AutoCAD.DatabaseServices.DxfFiler;
global using Cad_ErrorStatus = Autodesk.AutoCAD.Runtime.ErrorStatus;

// ifoxcad.basal 引用
global using IFoxCAD.Basal;





global using Autodesk.AutoCAD.Windows;
global using Autodesk.AutoCAD.GraphicsSystem;
global using LineWeight = Autodesk.AutoCAD.DatabaseServices.LineWeight;
global using Color = Autodesk.AutoCAD.Colors.Color;
global using Acap = Autodesk.AutoCAD.ApplicationServices.Application;
#if NET35
global using Acaop = Autodesk.AutoCAD.ApplicationServices.Application;
#else
global using Acaop = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#endif
global using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;
global using Group = Autodesk.AutoCAD.DatabaseServices.Group;
global using CursorType = Autodesk.AutoCAD.EditorInput.CursorType;
global using ColorDialog = Autodesk.AutoCAD.Windows.ColorDialog;
global using StatusBar = Autodesk.AutoCAD.Windows.StatusBar;
global using Utils = Autodesk.AutoCAD.Internal.Utils;
global using SystemVariableChangedEventArgs = Autodesk.AutoCAD.ApplicationServices.SystemVariableChangedEventArgs;
global using AcException = Autodesk.AutoCAD.Runtime.Exception;
global using Marshaler = Autodesk.AutoCAD.Runtime.Marshaler;

global using System.Threading;
global using System.Security;
global using Exception = System.Exception;
global using DrawingColor = System.Drawing.Color;
global using Registry = Microsoft.Win32.Registry;
global using RegistryKey = Microsoft.Win32.RegistryKey;
global using Region = Autodesk.AutoCAD.DatabaseServices.Region;
global using System.Linq.Expressions;
global using System.Collections.ObjectModel;
// 系统引用
global using System.Windows.Input;
global using System.Globalization;
global using System.Diagnostics;

// global using System.Windows.Data;
global using System.Net;
global using System.Diagnostics.CodeAnalysis;
