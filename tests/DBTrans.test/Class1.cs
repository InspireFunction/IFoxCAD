using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Internal;

using IFoxCAD.Cad;
using Autodesk.AutoCAD.Colors;
using IFoxCAD.Cad.ExtensionMethod;

namespace test
{
    public class Class1
    {
        [CommandMethod("dbtest")]
        public void Dbtest()
        {
            using var tr = new DBTrans();
            tr.Editor.WriteMessage("\n测试 Editor 属性是否工作！");
            tr.Editor.WriteMessage("\n----------开始测试--------------");
            tr.Editor.WriteMessage("\n测试document属性是否工作");
            if (tr.Document == getdoc())
            {
                tr.Editor.WriteMessage("\ndocument 正常");
            }
            tr.Editor.WriteMessage("\n测试database属性是否工作");
            if (tr.Database == getdb())
            {
                tr.Editor.WriteMessage("\ndatabase 正常");
            }



        }

        [CommandMethod("layertest")]
        public void layertest()
        {
            using var tr = new DBTrans();
            tr.LayerTable.Add("1");
            tr.LayerTable.Add("2", lt =>
            {
                lt.Color = Color.FromColorIndex(ColorMethod.ByColor, 1);
                lt.LineWeight = LineWeight.LineWeight030;

            });
            tr.LayerTable.Remove("3");
            tr.LayerTable.Change("4", lt =>
            {
                lt.Color = Color.FromColorIndex(ColorMethod.ByColor, 2);
            });
        }
        [CommandMethod("layerAdd1")]
        public void layertest1()
        {
            using var tr = new DBTrans();
            tr.LayerTable.Add("test1", Color.FromColorIndex(ColorMethod.ByColor,1));
        }
        [CommandMethod("layerAdd2")]
        
        //Todo：小山山还没块表
        //[CommandMethod("linedemo1")]
        //public void addLine1()
        //{
        //    using var tr = new DBTrans();
        //    tr.BlockTable.Add(new BlockTableRecord(), line =>
        //    {
        //        line.
        //    });
        //}

        [CommandMethod("PrintLayerName")]
        public void PrintLayerName()
        {
            using var tr = new DBTrans();
            foreach (var layerRecord in tr.LayerTable.GetRecords())
            {
                tr.Editor.WriteMessage(layerRecord.Name);
            }

        }

        public Database getdb()
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            return db;
        }


        public Document getdoc()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            return doc;
        }
    }
}
