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

global using Exception = System.Exception;
global using Registry = Microsoft.Win32.Registry;
global using RegistryKey = Microsoft.Win32.RegistryKey;

// cad 引用
global using GrxCAD.ApplicationServices;
global using GrxCAD.EditorInput;
global using GrxCAD.Colors;
global using GrxCAD.DatabaseServices;
global using GrxCAD.Geometry;
global using GrxCAD.Runtime;
global using Acap = GrxCAD.ApplicationServices.Application;
global using Acaop = GrxCAD.ApplicationServices.Application;
global using GrxCAD.DatabaseServices.Filters;

// jig命名空间会引起Viewport/Polyline等等重义,最好逐个引入 using Autodesk.AutoCAD.GraphicsInterface
global using GrxCAD.GraphicsInterface;
global using WorldDraw = GrxCAD.GraphicsInterface.WorldDraw;
global using Manager = GrxCAD.GraphicsSystem.Manager;
global using Group = GrxCAD.DatabaseServices.Group;
global using Viewport = GrxCAD.DatabaseServices.Viewport;
global using Polyline = GrxCAD.DatabaseServices.Polyline;

// ifoxcad.basal 引用
global using IFoxCAD.Basal;


