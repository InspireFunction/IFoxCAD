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
global using GrxCAD.ApplicationServices;
global using GrxCAD.EditorInput;
global using GrxCAD.Colors;
global using GrxCAD.DatabaseServices;
global using GrxCAD.Geometry;
global using GrxCAD.Runtime;
global using Acgi = GrxCAD.GraphicsInterface;

global using GrxCAD.DatabaseServices.Filters;
global using GrxCAD;

global using GrxCAD.Internal;

// jig命名空间会引起Viewport/Polyline等等重义,最好逐个引入 using GrxCAD.GraphicsInterface
global using GrxCAD.GraphicsInterface;
global using WorldDraw = GrxCAD.GraphicsInterface.WorldDraw;
global using Manager = GrxCAD.GraphicsSystem.Manager;
global using Viewport = GrxCAD.DatabaseServices.Viewport;
global using Cad_DwgFiler = GrxCAD.DatabaseServices.DwgFiler;
global using Cad_DxfFiler = GrxCAD.DatabaseServices.DxfFiler;
global using Cad_ErrorStatus = GrxCAD.Runtime.ErrorStatus;

// ifoxcad.basal 引用
global using IFoxCAD.Basal;




global using GrxCAD.Windows;
global using GrxCAD.GraphicsSystem;
global using LineWeight = GrxCAD.DatabaseServices.LineWeight;
global using Color = GrxCAD.Colors.Color;
global using Acap = GrxCAD.ApplicationServices.Application;
global using Acaop = GrxCAD.ApplicationServices.Application;

global using Polyline = GrxCAD.DatabaseServices.Polyline;
global using Group = GrxCAD.DatabaseServices.Group;
global using CursorType = GrxCAD.EditorInput.CursorType;
global using ColorDialog = GrxCAD.Windows.ColorDialog;
global using StatusBar = GrxCAD.Windows.StatusBar;
global using Utils = GrxCAD.Internal.Utils;
global using SystemVariableChangedEventArgs = GrxCAD.ApplicationServices.SystemVariableChangedEventArgs;
global using AcException = GrxCAD.Runtime.Exception;
global using Marshaler = GrxCAD.Runtime.Marshaler;

global using System.Threading;
global using System.Security;
global using Exception = System.Exception;
global using DrawingColor = System.Drawing.Color;
global using Registry = Microsoft.Win32.Registry;
global using RegistryKey = Microsoft.Win32.RegistryKey;
global using Region = GrxCAD.DatabaseServices.Region;
global using System.Linq.Expressions;
global using System.Collections.ObjectModel;
// 系统引用
global using System.Windows.Input;
global using System.Globalization;
global using System.Diagnostics;

// global using System.Windows.Data;
global using System.Net;
global using System.Diagnostics.CodeAnalysis;

global using IFoxFunKit;