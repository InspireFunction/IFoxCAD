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


//<!--json要求的程序集,用这个不产生dll,但是可能因为环境丢失了这个东西,导致程序运行失败,尤其是net40-->
//<!--<Reference Include="System.Web.Extensions"/>-->
//#if !NET472
//global using System.Web.Script.Serialization;/*序列化的类 程序集:System.Web.Extensions.dll*/ 
//#else
//global using System.Text.Json;
//#endif

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

/// ifoxcad
global using IFoxCAD.Cad;
global using IFoxCAD.Basal;