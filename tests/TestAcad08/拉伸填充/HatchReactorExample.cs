//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.EditorInput;
//using Autodesk.AutoCAD.Runtime;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace JoinBoxAcad
//{
//   public class HatchReactorExample
//   {
//       [CommandMethod(nameof(ShowHatchReactors))]
//       public void ShowHatchReactors()
//       {
//           Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

//           try
//           {
//               using (Transaction tr = ed.Document.Database.TransactionManager.StartTransaction())
//               {
//                   // 1. 遍历所有填充对象，查看关联关系
//                   BlockTable bt = (BlockTable)tr.GetObject(ed.Document.Database.BlockTableId, OpenMode.ForRead);
//                   BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

//                   ed.WriteMessage($"\n=== 遍历模型空间中的填充对象 ===\n");
//                   int hatchCount = 0;
//                   foreach (ObjectId objId in btr)
//                   {
//                       if (objId.ObjectClass.Name == "HATCH")
//                       {
//                           hatchCount++;
//                           Hatch hatch = (Hatch)tr.GetObject(objId, OpenMode.ForRead);

//                           ed.WriteMessage($"\n填充 {hatchCount} - ID: {hatch.ObjectId.Handle.Value.ToString()}");
//                           ed.WriteMessage($"\n  关联状态: {hatch.Associative}");
//                           ed.WriteMessage($"\n  循环数量: {hatch.NumberOfLoops}");

//                           // 获取关联的对象ID
//                           if (hatch.Associative)
//                           {
//                               using (ObjectIdCollection assocIds = hatch.GetAssociatedObjectIds())
//                               {
//                                   ed.WriteMessage($"\n  关联对象数量: {assocIds.Count}");
//                                   for (int i = 0; i < assocIds.Count; i++)
//                                   {
//                                       ObjectId assocId = assocIds[i];
//                                       if (assocId.IsOk())
//                                       {
//                                           DBObject assocObj = tr.GetObject(assocId, OpenMode.ForRead);
//                                           ed.WriteMessage($"\n    关联对象 {i+1}: {assocObj.GetType().Name} (Handle: {assocObj.ObjectId.Handle.Value.ToString()})");
//                                       }
//                                       else
//                                       {
//                                           ed.WriteMessage($"\n    关联对象 {i+1}: 无效ID");
//                                       }
//                                   }
//                               }
//                           }
//                       }
//                   }

//                   if (hatchCount == 0)
//                   {
//                       ed.WriteMessage("\n模型空间中没有填充对象。");
//                   }

//                   // 2. 遍历扩展字典，查找可能的反应器
//                   ed.WriteMessage($"\n\n=== 遍历数据库扩展字典 ===\n");

//                    DBDictionary extensionDict = (DBDictionary)tr.GetObject(ed.Document.Database.ExtensionDictionaryId, OpenMode.ForRead);
//                   List<string> reactorKeys = new List<string>();

//                   foreach (DBDictionaryEntry entry in extensionDict)
//                   {
//                       if (entry.Key.ToUpper().Contains("REACTOR") || entry.Key.ToUpper().Contains("ASSOC"))
//                       {
//                           reactorKeys.Add(entry.Key);
//                           ed.WriteMessage($"\n  字典项: {entry.Key} - 类型: {entry.Value.ObjectClass.Name}");
//                       }
//                   }

//                   if (reactorKeys.Count == 0)
//                   {
//                       ed.WriteMessage("\n扩展字典中没有找到反应器相关的项。");
//                   }

//                   // 3. 遍历填充的扩展字典
//                   ed.WriteMessage($"\n\n=== 遍历填充的扩展字典 ===\n");
//                   foreach (ObjectId objId in btr)
//                   {
//                       if (objId.ObjectClass.Name == "HATCH")
//                       {
//                           Hatch hatch = (Hatch)tr.GetObject(objId, OpenMode.ForRead);

//                           if (hatch.ExtensionDictionary != ObjectId.Null)
//                           {
//                               DBDictionary hatchExtDict = (DBDictionary)tr.GetObject(hatch.ExtensionDictionary, OpenMode.ForRead);
//                               ed.WriteMessage($"\n填充 (Handle: {hatch.ObjectId.Handle.Value.ToString()}) 的扩展字典:");

//                               if (hatchExtDict.Count == 0)
//                               {
//                                   ed.WriteMessage("  空");
//                               }
//                               else
//                               {
//                                   foreach (DBDictionaryEntry entry in hatchExtDict)
//                                   {
//                                       ed.WriteMessage($"\n    项: {entry.Key} - 类型: {entry.Value.ObjectClass.Name}");
//                                   }
//                               }
//                           }
//                           else
//                           {
//                               ed.WriteMessage($"\n填充 (Handle: {hatch.ObjectId.Handle.Value.ToString()}) 没有扩展字典");
//                           }
//                       }
//                   }

//                   tr.Commit();
//               }
//           }
//           catch (Exception ex)
//           {
//               ed.WriteMessage($"\n错误: {ex.Message}");
//               ed.WriteMessage($"\n堆栈跟踪: {ex.StackTrace}");
//           }
//       }

//       [CommandMethod(nameof(AnalyzeHatchResetIssue))]
//       public void AnalyzeHatchResetIssue()
//       {
//           Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

//           // 提示用户选择一个填充对象
//           PromptEntityOptions peo = new PromptEntityOptions("\n选择一个填充对象进行分析: ");
//           peo.SetRejectMessage("\n请选择填充对象！");
//           peo.AddAllowedClass(typeof(Hatch), true);
//           PromptEntityResult per = ed.GetEntity(peo);

//           if (per.Status != PromptStatus.OK)
//               return;

//           try
//           {
//               using (Transaction tr = ed.Document.Database.TransactionManager.StartTransaction())
//               {
//                   Hatch hatch = (Hatch)tr.GetObject(per.ObjectId, OpenMode.ForRead);

//                   ed.WriteMessage($"\n=== 填充对象分析报告 ===\n");
//                   ed.WriteMessage($"\n填充ID: {hatch.ObjectId.Handle.Value.ToString()}");
//                   ed.WriteMessage($"\n关联状态: {hatch.Associative}");
//                   ed.WriteMessage($"\n循环数量: {hatch.NumberOfLoops}");
//                   ed.WriteMessage($"\n填充类型: {hatch.PatternType}");
//                   ed.WriteMessage($"\n填充名称: {hatch.PatternName}");

//                   // 检查循环类型
//                   for (int i = 0; i < hatch.NumberOfLoops; i++)
//                   {
//                       HatchLoop loop = hatch.GetLoopAt(i);
//                       ed.WriteMessage($"\n  循环 {i}: 类型 = {loop.LoopType}, 是多段线 = {loop.IsPolyline}");

//                       if (loop.IsPolyline)
//                       {
//                           ed.WriteMessage($", 顶点数量 = {loop.Polyline.Count}");
//                       }
//                       else if (loop.Curves != null)
//                       {
//                           ed.WriteMessage($", 曲线数量 = {loop.Curves.Count}");
//                       }
//                   }

//                   // 测试重置边界操作
//                   ed.WriteMessage($"\n\n=== 测试边界重置 ===\n");

//                   // 先保存当前状态
//                   bool originalAssociative = hatch.Associative;
//                   int originalLoopCount = hatch.NumberOfLoops;

//                   // 尝试重置边界
//                   try
//                   {
//                       hatch.UpgradeOpen();

//                       // 1. 取消关联
//                       ed.WriteMessage($"\n1. 设置关联状态为false...");
//                       hatch.Associative = false;
//                       ed.WriteMessage(" 完成");

//                       // 2. 移除循环
//                       ed.WriteMessage($"\n2. 移除所有循环...");
//                       while (hatch.NumberOfLoops > 0)
//                       {
//                           hatch.RemoveLoopAt(0);
//                       }
//                       ed.WriteMessage($" 完成 (剩余循环: {hatch.NumberOfLoops})");

//                       // 3. 移除关联对象
//                       ed.WriteMessage($"\n3. 移除关联对象...");
//                       hatch.RemoveAssociatedObjectIds();
//                       ed.WriteMessage(" 完成");

//                       // 4. 计算填充
//                       ed.WriteMessage($"\n4. 计算填充...");
//                       hatch.EvaluateHatch(true);
//                       ed.WriteMessage(" 完成");

//                       ed.WriteMessage($"\n\n✅ 边界重置测试成功完成！");

//                   }
//                   catch (Exception ex)
//                   {
//                       ed.WriteMessage($"\n❌ 边界重置测试失败: {ex.Message}");
//                   }

//                   tr.Commit();
//               }
//           }
//           catch (Exception ex)
//           {
//               ed.WriteMessage($"\n错误: {ex.Message}");
//               ed.WriteMessage($"\n堆栈跟踪: {ex.StackTrace}");
//           }
//       }

//       [CommandMethod(nameof(SafeHatchReset))]
//       public void SafeHatchReset()
//       {
//           Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

//           // 提示用户选择一个填充对象
//           PromptEntityOptions peo = new PromptEntityOptions("\n选择一个填充对象进行安全重置: ");
//           peo.SetRejectMessage("\n请选择填充对象！");
//           peo.AddAllowedClass(typeof(Hatch), true);
//           PromptEntityResult per = ed.GetEntity(peo);

//           if (per.Status != PromptStatus.OK)
//               return;

//           try
//           {
//               using (Transaction tr = ed.Document.Database.TransactionManager.StartTransaction())
//               {
//                   Hatch hatch = (Hatch)tr.GetObject(per.ObjectId, OpenMode.ForWrite);

//                   ed.WriteMessage($"\n=== 安全重置填充边界 ===\n");

//                   // 安全重置边界的步骤

//                   // 1. 保存原始属性
//                   bool originalAssociative = hatch.Associative;

//                   // 2. 检查是否有有效的关联对象
//                   List<ObjectId> validAssocIds = new List<ObjectId>();
//                   if (hatch.Associative)
//                   {
//                       using (ObjectIdCollection assocIds = hatch.GetAssociatedObjectIds())
//                       {
//                           foreach (ObjectId id in assocIds)
//                           {
//                               if (id.IsOk() && !id.IsErased)
//                               {
//                                   validAssocIds.Add(id);
//                               }
//                           }
//                       }
//                   }

//                   ed.WriteMessage($"\n原始关联对象数量: {validAssocIds.Count}");

//                   // 3. 安全重置边界
//                   bool success = SafeResetBoundary(hatch, validAssocIds, tr);

//                   if (success)
//                   {
//                       ed.WriteMessage($"\n✅ 填充边界安全重置成功！");
//                   }
//                   else
//                   {
//                       ed.WriteMessage($"\n❌ 填充边界重置失败！");
//                   }

//                   tr.Commit();
//               }
//           }
//           catch (Exception ex)
//           {
//               ed.WriteMessage($"\n错误: {ex.Message}");
//               ed.WriteMessage($"\n堆栈跟踪: {ex.StackTrace}");
//           }
//       }

//       /// <summary>
//       /// 安全重置填充边界的方法
//       /// </summary>
//       /// <param name="hatch">填充对象</param>
//       /// <param name="boundaryIds">边界对象ID列表</param>
//       /// <param name="tr">当前事务</param>
//       /// <returns>是否成功</returns>
//       private bool SafeResetBoundary(Hatch hatch, List<ObjectId> boundaryIds, Transaction tr)
//       {
//           try
//           {
//               Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

//               // 步骤1: 取消关联性
//               hatch.Associative = false;

//               // 步骤2: 移除所有循环
//               while (hatch.NumberOfLoops > 0)
//               {
//                   hatch.RemoveLoopAt(0);
//               }

//               // 步骤3: 移除所有关联对象
//               hatch.RemoveAssociatedObjectIds();

//               // 步骤4: 重新添加边界（如果提供了有效边界）
//               if (boundaryIds.Count > 0)
//               {
//                   using (ObjectIdCollection obIds = new ObjectIdCollection())
//                   {
//                       foreach (ObjectId id in boundaryIds)
//                       {
//                           if (id.IsOk() && !id.IsErased)
//                           {
//                               obIds.Clear();
//                               obIds.Add(id);
//                               hatch.AppendLoop(HatchLoopTypes.Default, obIds);
//                           }
//                       }
//                   }

//                   // 重新设置关联性
//                   hatch.Associative = true;
//               }

//               // 步骤5: 计算填充
//               hatch.EvaluateHatch(true);

//               return true;
//           }
//           catch (Exception ex)
//           {
//               Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
//               ed.WriteMessage($"\n重置边界时出错: {ex.Message}");
//               return false;
//           }
//       }
//   }
//}




