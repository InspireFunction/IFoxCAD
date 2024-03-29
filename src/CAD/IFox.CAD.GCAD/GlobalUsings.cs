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
global using Gssoft.Gscad.ApplicationServices;
global using Gssoft.Gscad.EditorInput;
global using Gssoft.Gscad.Colors;
global using Gssoft.Gscad.DatabaseServices;
global using Gssoft.Gscad.Geometry;
global using Gssoft.Gscad.Runtime;
global using Acap = Gssoft.Gscad.ApplicationServices.Application;
global using Acaop = Gssoft.Gscad.ApplicationServices.Core.Application;
global using AcException = Gssoft.Gscad.Runtime.Exception;
global using Gssoft.Gscad.DatabaseServices.Filters;

// jig命名空间会引起Viewport/Polyline等等重义,最好逐个引入 using Autodesk.AutoCAD.GraphicsInterface
global using Gssoft.Gscad.GraphicsInterface;
global using WorldDraw = Gssoft.Gscad.GraphicsInterface.WorldDraw;
global using Manager = Gssoft.Gscad.GraphicsSystem.Manager;
global using Group = Gssoft.Gscad.DatabaseServices.Group;
global using Viewport = Gssoft.Gscad.DatabaseServices.Viewport;
global using Polyline = Gssoft.Gscad.DatabaseServices.Polyline;

// ifoxcad.basal 引用
global using IFoxCAD.Basal;


