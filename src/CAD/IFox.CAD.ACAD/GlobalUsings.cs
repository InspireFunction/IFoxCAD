﻿// 系统引用
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
global using Autodesk.AutoCAD.ApplicationServices;
global using Autodesk.AutoCAD.EditorInput;
global using Autodesk.AutoCAD.Colors;
global using Autodesk.AutoCAD.DatabaseServices;
global using Autodesk.AutoCAD.Geometry;
global using Autodesk.AutoCAD.Runtime;
global using Acap = Autodesk.AutoCAD.ApplicationServices.Application;
global using Acaop = Autodesk.AutoCAD.ApplicationServices.Core.Application;
global using AcException = Autodesk.AutoCAD.Runtime.Exception;

global using Autodesk.AutoCAD.DatabaseServices.Filters;

// jig命名空间会引起Viewport/Polyline等等重义,最好逐个引入 using Autodesk.AutoCAD.GraphicsInterface
global using Autodesk.AutoCAD.GraphicsInterface;
global using WorldDraw = Autodesk.AutoCAD.GraphicsInterface.WorldDraw;
global using Manager = Autodesk.AutoCAD.GraphicsSystem.Manager;
global using Group = Autodesk.AutoCAD.DatabaseServices.Group;
global using Viewport = Autodesk.AutoCAD.DatabaseServices.Viewport;
global using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

// ifoxcad.basal 引用
global using IFoxCAD.Basal;
