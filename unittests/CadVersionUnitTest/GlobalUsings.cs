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
global using System.Diagnostics;
global using System.Threading;

global using IFoxCAD.Cad;
global using Xunit;

global using Acap = Autodesk.AutoCAD.ApplicationServices.Application;

#if !NET8_0_OR_GREATER
global using ArgumentNullException = IFoxCAD.Basal.ArgumentNullEx;
#endif
