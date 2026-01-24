using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using IFoxCAD.CADShared;

namespace TestConsole;

/// <summary>
/// 测试文档特定撤销系统
/// </summary>
public class TestDocumentSpecificUndoSystem
{
    public void TestBasicFunctionality()
    {
        Console.WriteLine("=== 测试文档特定撤销系统基本功能 ===");
        
        // 获取活动文档
        Document doc = Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
        {
            Console.WriteLine("没有活动文档，跳过测试");
            return;
        }
        
        // 获取系统实例
        var undoSystem = DocumentSpecificUndoSystem.Instance;
        
        // 测试1: 获取文档对应的DAG
        var dag = undoSystem.GetOrCreateDag(doc);
        Console.WriteLine($"获取文档DAG: {dag != null}");
        Console.WriteLine($"当前DAG深度: {dag.Current.Depth}");
        
        // 测试2: 创建记录本
        var recordBook = undoSystem.CreateNewRecordBook(doc);
        Console.WriteLine($"创建记录本: {recordBook != null}");
        Console.WriteLine($"记录本初始计数: {recordBook.Count}");
        
        // 测试3: 添加一些记录到记录本
        // 注意：这里我们使用虚拟的ObjectId进行演示
        // 在实际使用中，这些应该是真实的ObjectId
        var fakeId1 = ObjectId.Null; // 实际应用中会是真实的ObjectId
        var fakeId2 = ObjectId.Null; // 实际应用中会是真实的ObjectId
        
        // 模拟记录变更
        undoSystem.LogChange(doc, ModificationType.Appended, fakeId1);
        undoSystem.LogChange(doc, ModificationType.Modified, fakeId2);
        
        var updatedRecordBook = undoSystem.GetRecordBook(doc);
        Console.WriteLine($"记录变更后记录本计数: {updatedRecordBook?.Count ?? 0}");
        
        // 测试4: 开始新命令
        undoSystem.StartNewCommand(doc, "TEST_COMMAND");
        Console.WriteLine($"开始新命令后记录本计数: {undoSystem.GetRecordBook(doc)?.Count ?? 0}");
        
        // 测试5: 完成命令
        undoSystem.CompleteCommandAndSaveToDag(doc, "TEST_COMMAND");
        Console.WriteLine($"完成命令后DAG节点数: {undoSystem.GetDag(doc)?.History.Count ?? 0}");
        
        Console.WriteLine("=== 基本功能测试完成 ===\n");
    }
    
    public void TestMultipleDocuments()
    {
        Console.WriteLine("=== 测试多文档支持 ===");
        
        // 获取所有文档
        var docManager = Application.DocumentManager;
        var documents = new List<Document>();
        
        // 收集所有文档
        foreach (Document doc in docManager)
        {
            documents.Add(doc);
        }
        
        var undoSystem = DocumentSpecificUndoSystem.Instance;
        
        // 为每个文档创建DAG并验证是独立的
        foreach (var doc in documents)
        {
            var dag = undoSystem.GetOrCreateDag(doc);
            Console.WriteLine($"文档 '{doc.Name}' DAG 创建: {dag != null}");
            Console.WriteLine($"  DAG节点数: {dag.History.Count}");
        }
        
        Console.WriteLine($"被跟踪的文档总数: {undoSystem.GetTrackedDocuments().GetEnumerator().MoveNext()}");
        
        Console.WriteLine("=== 多文档支持测试完成 ===\n");
    }
    
    public void TestUndoRedo()
    {
        Console.WriteLine("=== 测试撤销重做功能 ===");
        
        Document doc = Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
        {
            Console.WriteLine("没有活动文档，跳过测试");
            return;
        }
        
        var undoSystem = DocumentSpecificUndoSystem.Instance;
        
        // 检查是否可以撤销
        bool canUndo = undoSystem.CanUndo(doc);
        Console.WriteLine($"是否可以撤销: {canUndo}");
        
        // 检查是否可以重做
        bool canRedo = undoSystem.CanRedo(doc);
        Console.WriteLine($"是否可以重做: {canRedo}");
        
        // 测试撤销操作
        if (canUndo)
        {
            bool undoResult = undoSystem.Undo(doc);
            Console.WriteLine($"撤销操作结果: {undoResult}");
        }
        else
        {
            Console.WriteLine("当前状态下无法撤销");
        }
        
        // 测试重做操作
        if (canRedo)
        {
            bool redoResult = undoSystem.Redo(doc);
            Console.WriteLine($"重做操作结果: {redoResult}");
        }
        else
        {
            Console.WriteLine("当前状态下无法重做");
        }
        
        Console.WriteLine("=== 撤销重做功能测试完成 ===\n");
    }
    
    [CommandMethod("RunDocumentSpecificUndoTests")]
    public void RunAllTests()
    {
        Console.WriteLine("开始运行文档特定撤销系统测试...\n");
        
        TestBasicFunctionality();
        TestMultipleDocuments();
        TestUndoRedo();
        
        Console.WriteLine("所有测试完成！");
    }
}