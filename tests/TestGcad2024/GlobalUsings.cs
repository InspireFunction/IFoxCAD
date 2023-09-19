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

/// autocad 引用
global using Gssoft.Gscad.ApplicationServices;
global using Gssoft.Gscad.EditorInput;
global using Gssoft.Gscad.Colors;
global using Gssoft.Gscad.DatabaseServices;
global using Gssoft.Gscad.Geometry;
global using Gssoft.Gscad.Runtime;
global using Acap = Gssoft.Gscad.ApplicationServices.Application;
global using Acaop = Gssoft.Gscad.ApplicationServices.Core.Application;
global using Acgi = Gssoft.Gscad.GraphicsInterface;

global using Gssoft.Gscad.DatabaseServices.Filters;
global using Gssoft.Gscad;

// jig命名空间会引起Viewport/Polyline等等重义,最好逐个引入 using Gssoft.Gscad.GraphicsInterface
global using WorldDraw = Gssoft.Gscad.GraphicsInterface.WorldDraw;
global using Manager = Gssoft.Gscad.GraphicsSystem.Manager;
global using Group = Gssoft.Gscad.DatabaseServices.Group;
global using Viewport = Gssoft.Gscad.DatabaseServices.Viewport;
global using Gssoft.Gscad.GraphicsInterface;
global using Polyline = Gssoft.Gscad.DatabaseServices.Polyline;
global using Cad_DwgFiler = Gssoft.Gscad.DatabaseServices.DwgFiler;
global using Cad_DxfFiler = Gssoft.Gscad.DatabaseServices.DxfFiler;
global using Cad_ErrorStatus = Gssoft.Gscad.Runtime.ErrorStatus;


/// ifoxcad
global using IFoxCAD.Cad;
global using IFoxCAD.Basal;

